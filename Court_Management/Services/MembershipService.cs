using Court_Management.Data;
using Court_Management.Models;
using Court_Management.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Court_Management.Services
{
    public interface IMembershipService : IBaseService<MembershipDTO, CreateMembershipDTO, UpdateMembershipDTO>
    {
        Task<MembershipDTO> GetUserMembershipAsync(string userId);
        Task<IEnumerable<MembershipTypeDTO>> GetMembershipTypesAsync();
        Task<bool> IsUserMembershipActiveAsync(string userId);
    }

    public class MembershipService : IMembershipService
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<MembershipType, decimal> _membershipPrices;

        public MembershipService(ApplicationDbContext context)
        {
            _context = context;
            _membershipPrices = new Dictionary<MembershipType, decimal>
            {
                { MembershipType.Basic, 50.00m },
                { MembershipType.Premium, 80.00m },
                { MembershipType.Gold, 120.00m }
            };
        }

        public async Task<IEnumerable<MembershipDTO>> GetAllAsync()
        {
            return await _context.Memberships
                .Include(m => m.User)
                .Select(m => new MembershipDTO
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = $"{m.User.FirstName} {m.User.LastName}",
                    Type = m.Type.ToString(),
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    IsActive = m.IsActive,
                    Price = m.Price
                })
                .ToListAsync();
        }

        public async Task<MembershipDTO> GetByIdAsync(int id)
        {
            var membership = await _context.Memberships
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (membership == null) return null;

            return new MembershipDTO
            {
                Id = membership.Id,
                UserId = membership.UserId,
                UserName = $"{membership.User.FirstName} {membership.User.LastName}",
                Type = membership.Type.ToString(),
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                IsActive = membership.IsActive,
                Price = membership.Price
            };
        }

        public async Task<MembershipDTO> CreateAsync(CreateMembershipDTO createDto)
        {
            var membershipType = Enum.Parse<MembershipType>(createDto.Type);
            var monthlyPrice = _membershipPrices[membershipType];
            var totalPrice = monthlyPrice * createDto.DurationInMonths;

            var membership = new Membership
            {
                UserId = createDto.UserId,
                Type = membershipType,
                StartDate = createDto.StartDate,
                EndDate = createDto.StartDate.AddMonths(createDto.DurationInMonths),
                IsActive = true,
                Price = totalPrice
            };

            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(membership.Id);
        }

        public async Task<MembershipDTO> UpdateAsync(int id, UpdateMembershipDTO updateDto)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null) return null;

            membership.Type = Enum.Parse<MembershipType>(updateDto.Type);
            membership.EndDate = updateDto.EndDate;
            membership.IsActive = updateDto.IsActive;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(membership.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null) return false;

            _context.Memberships.Remove(membership);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MembershipDTO> GetUserMembershipAsync(string userId)
        {
            var membership = await _context.Memberships
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.IsActive);

            if (membership == null) return null;

            return new MembershipDTO
            {
                Id = membership.Id,
                UserId = membership.UserId,
                UserName = $"{membership.User.FirstName} {membership.User.LastName}",
                Type = membership.Type.ToString(),
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                IsActive = membership.IsActive,
                Price = membership.Price
            };
        }

        public async Task<IEnumerable<MembershipTypeDTO>> GetMembershipTypesAsync()
        {
            return new List<MembershipTypeDTO>
            {
                new MembershipTypeDTO
                {
                    Type = MembershipType.Basic.ToString(),
                    MonthlyPrice = _membershipPrices[MembershipType.Basic],
                    Description = "Basic membership with standard court access",
                    Benefits = new List<string>
                    {
                        "Court booking up to 3 days in advance",
                        "Access to basic facilities",
                        "Online booking system access"
                    }
                },
                new MembershipTypeDTO
                {
                    Type = MembershipType.Premium.ToString(),
                    MonthlyPrice = _membershipPrices[MembershipType.Premium],
                    Description = "Premium membership with enhanced benefits",
                    Benefits = new List<string>
                    {
                        "Court booking up to 7 days in advance",
                        "Access to premium facilities",
                        "Discounted coaching sessions",
                        "Free equipment rental"
                    }
                },
                new MembershipTypeDTO
                {
                    Type = MembershipType.Gold.ToString(),
                    MonthlyPrice = _membershipPrices[MembershipType.Gold],
                    Description = "Gold membership with exclusive benefits",
                    Benefits = new List<string>
                    {
                        "Court booking up to 14 days in advance",
                        "Access to all facilities",
                        "Priority court booking",
                        "Free coaching sessions monthly",
                        "VIP lounge access",
                        "Guest passes included"
                    }
                }
            };
        }

        public async Task<bool> IsUserMembershipActiveAsync(string userId)
        {
            return await _context.Memberships
                .AnyAsync(m => m.UserId == userId && 
                              m.IsActive && 
                              m.EndDate > DateTime.UtcNow);
        }
    }
}
