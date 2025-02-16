using Court_Management.Models.DTOs;
using Court_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Court_Management.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourtsController : ControllerBase
    {
        private readonly ICourtService _courtService;

        public CourtsController(ICourtService courtService)
        {
            _courtService = courtService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourtDTO>>> GetCourts()
        {
            var courts = await _courtService.GetAllAsync();
            return Ok(courts);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<CourtDTO>>> GetAvailableCourts()
        {
            var courts = await _courtService.GetAvailableCourtsAsync();
            return Ok(courts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourtDTO>> GetCourt(int id)
        {
            var court = await _courtService.GetByIdAsync(id);
            if (court == null)
            {
                return NotFound();
            }
            return Ok(court);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourtDTO>> CreateCourt(CreateCourtDTO createDto)
        {
            var court = await _courtService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetCourt), new { id = court.Id }, court);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourtDTO>> UpdateCourt(int id, UpdateCourtDTO updateDto)
        {
            var court = await _courtService.UpdateAsync(id, updateDto);
            if (court == null)
            {
                return NotFound();
            }
            return Ok(court);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourt(int id)
        {
            var result = await _courtService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
