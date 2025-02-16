using Court_Management.Models.DTOs;
using Court_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Court_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MembershipsController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MembershipDTO>>> GetAllMemberships()
        {
            var memberships = await _membershipService.GetAllAsync();
            return Ok(memberships);
        }

        [HttpGet("my-membership")]
        public async Task<ActionResult<MembershipDTO>> GetMyMembership()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var membership = await _membershipService.GetUserMembershipAsync(userId);
            
            if (membership == null)
            {
                return NotFound(new { message = "No active membership found" });
            }

            return Ok(membership);
        }

        [HttpGet("types")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MembershipTypeDTO>>> GetMembershipTypes()
        {
            var types = await _membershipService.GetMembershipTypesAsync();
            return Ok(types);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipDTO>> GetMembership(int id)
        {
            var membership = await _membershipService.GetByIdAsync(id);
            if (membership == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && membership.UserId != userId)
            {
                return Forbid();
            }

            return Ok(membership);
        }

        [HttpPost]
        public async Task<ActionResult<MembershipDTO>> CreateMembership(CreateMembershipDTO createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Only admins can create memberships for other users
            if (!User.IsInRole("Admin") && createDto.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                var membership = await _membershipService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetMembership), new { id = membership.Id }, membership);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MembershipDTO>> UpdateMembership(int id, UpdateMembershipDTO updateDto)
        {
            try
            {
                var membership = await _membershipService.UpdateAsync(id, updateDto);
                if (membership == null)
                {
                    return NotFound();
                }
                return Ok(membership);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMembership(int id)
        {
            var result = await _membershipService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
