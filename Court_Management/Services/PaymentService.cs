using Court_Management.Data;
using Court_Management.Models;
using Court_Management.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Court_Management.Services
{
    public interface IPaymentService : IBaseService<PaymentDTO, CreatePaymentDTO, UpdatePaymentDTO>
    {
        Task<IEnumerable<PaymentDTO>> GetUserPaymentsAsync(string userId);
        Task<PaymentSummaryDTO> GetUserPaymentSummaryAsync(string userId);
        Task<PaymentDTO> ProcessPaymentAsync(CreatePaymentDTO createDto);
    }

    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentDTO>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Booking)
                .ThenInclude(b => b.Court)
                .Select(p => new PaymentDTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = $"{p.User.FirstName} {p.User.LastName}",
                    BookingId = p.BookingId,
                    BookingDetails = p.Booking != null 
                        ? $"{p.Booking.Court.Name} - {p.Booking.StartTime:g} to {p.Booking.EndTime:g}"
                        : null,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status.ToString(),
                    Type = p.Type.ToString(),
                    TransactionId = p.TransactionId
                })
                .ToListAsync();
        }

        public async Task<PaymentDTO> GetByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Booking)
                .ThenInclude(b => b.Court)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return null;

            return new PaymentDTO
            {
                Id = payment.Id,
                UserId = payment.UserId,
                UserName = $"{payment.User.FirstName} {payment.User.LastName}",
                BookingId = payment.BookingId,
                BookingDetails = payment.Booking != null 
                    ? $"{payment.Booking.Court.Name} - {payment.Booking.StartTime:g} to {payment.Booking.EndTime:g}"
                    : null,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.Status.ToString(),
                Type = payment.Type.ToString(),
                TransactionId = payment.TransactionId
            };
        }

        public async Task<PaymentDTO> CreateAsync(CreatePaymentDTO createDto)
        {
            var payment = new Payment
            {
                UserId = createDto.UserId,
                BookingId = createDto.BookingId,
                Amount = createDto.Amount,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending,
                Type = Enum.Parse<PaymentType>(createDto.Type),
                TransactionId = Guid.NewGuid().ToString("N")
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(payment.Id);
        }

        public async Task<PaymentDTO> UpdateAsync(int id, UpdatePaymentDTO updateDto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return null;

            payment.Status = Enum.Parse<PaymentStatus>(updateDto.Status);
            payment.TransactionId = updateDto.TransactionId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(payment.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PaymentDTO>> GetUserPaymentsAsync(string userId)
        {
            return await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Booking)
                .ThenInclude(b => b.Court)
                .Where(p => p.UserId == userId)
                .Select(p => new PaymentDTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = $"{p.User.FirstName} {p.User.LastName}",
                    BookingId = p.BookingId,
                    BookingDetails = p.Booking != null 
                        ? $"{p.Booking.Court.Name} - {p.Booking.StartTime:g} to {p.Booking.EndTime:g}"
                        : null,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    Status = p.Status.ToString(),
                    Type = p.Type.ToString(),
                    TransactionId = p.TransactionId
                })
                .ToListAsync();
        }

        public async Task<PaymentSummaryDTO> GetUserPaymentSummaryAsync(string userId)
        {
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var summary = new PaymentSummaryDTO
            {
                TotalAmount = payments.Sum(p => p.Amount),
                TotalPayments = payments.Count,
                PaymentsByType = payments
                    .GroupBy(p => p.Type)
                    .ToDictionary(g => g.Key.ToString(), g => g.Sum(p => p.Amount)),
                RecentPayments = await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Booking)
                    .ThenInclude(b => b.Court)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .Select(p => new PaymentDTO
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        UserName = $"{p.User.FirstName} {p.User.LastName}",
                        BookingId = p.BookingId,
                        BookingDetails = p.Booking != null 
                            ? $"{p.Booking.Court.Name} - {p.Booking.StartTime:g} to {p.Booking.EndTime:g}"
                            : null,
                        Amount = p.Amount,
                        PaymentDate = p.PaymentDate,
                        Status = p.Status.ToString(),
                        Type = p.Type.ToString(),
                        TransactionId = p.TransactionId
                    })
                    .ToListAsync()
            };

            return summary;
        }

        public async Task<PaymentDTO> ProcessPaymentAsync(CreatePaymentDTO createDto)
        {
            // In a real application, this would integrate with a payment gateway
            // For now, we'll simulate a successful payment
            var payment = new Payment
            {
                UserId = createDto.UserId,
                BookingId = createDto.BookingId,
                Amount = createDto.Amount,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Completed,
                Type = Enum.Parse<PaymentType>(createDto.Type),
                TransactionId = Guid.NewGuid().ToString("N")
            };

            _context.Payments.Add(payment);

            // If this is a booking payment, update the booking status
            if (payment.BookingId.HasValue)
            {
                var booking = await _context.Bookings.FindAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(payment.Id);
        }
    }
}
