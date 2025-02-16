using Court_Management.Data;
using Court_Management.Models;
using Court_Management.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Court_Management.Services
{
    public interface ICourtService : IBaseService<CourtDTO, CreateCourtDTO, UpdateCourtDTO>
    {
        Task<IEnumerable<CourtDTO>> GetAvailableCourtsAsync();
    }

    public class CourtService : ICourtService
    {
        private readonly ApplicationDbContext _context;

        public CourtService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourtDTO>> GetAllAsync()
        {
            return await _context.Courts
                .Select(c => new CourtDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsIndoor = c.IsIndoor,
                    HourlyRate = c.HourlyRate,
                    IsAvailable = c.IsAvailable
                })
                .ToListAsync();
        }

        public async Task<CourtDTO> GetByIdAsync(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null) return null;

            return new CourtDTO
            {
                Id = court.Id,
                Name = court.Name,
                Description = court.Description,
                IsIndoor = court.IsIndoor,
                HourlyRate = court.HourlyRate,
                IsAvailable = court.IsAvailable
            };
        }

        public async Task<CourtDTO> CreateAsync(CreateCourtDTO createDto)
        {
            var court = new Court
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IsIndoor = createDto.IsIndoor,
                HourlyRate = createDto.HourlyRate,
                IsAvailable = true
            };

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            return new CourtDTO
            {
                Id = court.Id,
                Name = court.Name,
                Description = court.Description,
                IsIndoor = court.IsIndoor,
                HourlyRate = court.HourlyRate,
                IsAvailable = court.IsAvailable
            };
        }

        public async Task<CourtDTO> UpdateAsync(int id, UpdateCourtDTO updateDto)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null) return null;

            court.Name = updateDto.Name;
            court.Description = updateDto.Description;
            court.HourlyRate = updateDto.HourlyRate;
            court.IsAvailable = updateDto.IsAvailable;

            await _context.SaveChangesAsync();

            return new CourtDTO
            {
                Id = court.Id,
                Name = court.Name,
                Description = court.Description,
                IsIndoor = court.IsIndoor,
                HourlyRate = court.HourlyRate,
                IsAvailable = court.IsAvailable
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null) return false;

            _context.Courts.Remove(court);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CourtDTO>> GetAvailableCourtsAsync()
        {
            return await _context.Courts
                .Where(c => c.IsAvailable)
                .Select(c => new CourtDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsIndoor = c.IsIndoor,
                    HourlyRate = c.HourlyRate,
                    IsAvailable = c.IsAvailable
                })
                .ToListAsync();
        }
    }
}
