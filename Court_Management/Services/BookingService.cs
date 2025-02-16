using Court_Management.Data;
using Court_Management.Models;
using Court_Management.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Court_Management.Services
{
    public interface IBookingService : IBaseService<BookingDTO, CreateBookingDTO, UpdateBookingDTO>
    {
        Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(string userId);
        Task<BookingAvailabilityDTO> GetCourtAvailabilityAsync(int courtId, DateTime date);
        Task<bool> IsTimeSlotAvailableAsync(int courtId, DateTime startTime, DateTime endTime);
    }

    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookingDTO>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)
                .Select(b => new BookingDTO
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    CourtId = b.CourtId,
                    CourtName = b.Court.Name,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt,
                    UserName = $"{b.User.FirstName} {b.User.LastName}"
                })
                .ToListAsync();
        }

        public async Task<BookingDTO> GetByIdAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return null;

            return new BookingDTO
            {
                Id = booking.Id,
                UserId = booking.UserId,
                CourtId = booking.CourtId,
                CourtName = booking.Court.Name,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt,
                UserName = $"{booking.User.FirstName} {booking.User.LastName}"
            };
        }

        public async Task<BookingDTO> CreateAsync(CreateBookingDTO createDto)
        {
            // Validate time slot availability
            if (!await IsTimeSlotAvailableAsync(createDto.CourtId, createDto.StartTime, createDto.EndTime))
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }

            var court = await _context.Courts.FindAsync(createDto.CourtId);
            if (court == null)
            {
                throw new InvalidOperationException("Court not found.");
            }

            // Calculate total price
            var duration = createDto.EndTime - createDto.StartTime;
            var totalPrice = court.HourlyRate * (decimal)duration.TotalHours;

            var booking = new Booking
            {
                CourtId = createDto.CourtId,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(booking.Id);
        }

        public async Task<BookingDTO> UpdateAsync(int id, UpdateBookingDTO updateDto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return null;

            // Validate time slot availability if times are being updated
            if (booking.StartTime != updateDto.StartTime || booking.EndTime != updateDto.EndTime)
            {
                if (!await IsTimeSlotAvailableAsync(booking.CourtId, updateDto.StartTime, updateDto.EndTime))
                {
                    throw new InvalidOperationException("The selected time slot is not available.");
                }
            }

            booking.StartTime = updateDto.StartTime;
            booking.EndTime = updateDto.EndTime;
            booking.Status = Enum.Parse<BookingStatus>(updateDto.Status);

            // Recalculate total price if times changed
            if (booking.StartTime != updateDto.StartTime || booking.EndTime != updateDto.EndTime)
            {
                var court = await _context.Courts.FindAsync(booking.CourtId);
                var duration = updateDto.EndTime - updateDto.StartTime;
                booking.TotalPrice = court.HourlyRate * (decimal)duration.TotalHours;
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(booking.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .Select(b => new BookingDTO
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    CourtId = b.CourtId,
                    CourtName = b.Court.Name,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt,
                    UserName = $"{b.User.FirstName} {b.User.LastName}"
                })
                .ToListAsync();
        }

        public async Task<BookingAvailabilityDTO> GetCourtAvailabilityAsync(int courtId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var existingBookings = await _context.Bookings
                .Where(b => b.CourtId == courtId &&
                           b.StartTime < endOfDay &&
                           b.EndTime > startOfDay &&
                           b.Status != BookingStatus.Cancelled)
                .ToListAsync();

            var timeSlots = new List<TimeSlot>();
            var currentTime = startOfDay.AddHours(8); // Start at 8 AM
            var endTime = startOfDay.AddHours(22);    // End at 10 PM

            while (currentTime < endTime)
            {
                var slotEnd = currentTime.AddHours(1);
                var isAvailable = !existingBookings.Any(b =>
                    b.StartTime < slotEnd && b.EndTime > currentTime);

                timeSlots.Add(new TimeSlot
                {
                    StartTime = currentTime,
                    EndTime = slotEnd,
                    IsAvailable = isAvailable
                });

                currentTime = slotEnd;
            }

            return new BookingAvailabilityDTO
            {
                CourtId = courtId,
                Date = date,
                AvailableSlots = timeSlots
            };
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int courtId, DateTime startTime, DateTime endTime)
        {
            return !await _context.Bookings
                .AnyAsync(b => b.CourtId == courtId &&
                             b.Status != BookingStatus.Cancelled &&
                             b.StartTime < endTime &&
                             b.EndTime > startTime);
        }
    }
}
