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
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetAllBookings()
        {
            var bookings = await _bookingService.GetAllAsync();
            return Ok(bookings);
        }

        [HttpGet("my-bookings")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetMyBookings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBooking(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && booking.UserId != userId)
            {
                return Forbid();
            }

            return Ok(booking);
        }

        [HttpGet("court/{courtId}/availability")]
        public async Task<ActionResult<BookingAvailabilityDTO>> GetCourtAvailability(int courtId, [FromQuery] DateTime date)
        {
            var availability = await _bookingService.GetCourtAvailabilityAsync(courtId, date);
            return Ok(availability);
        }

        [HttpPost]
        public async Task<ActionResult<BookingDTO>> CreateBooking(CreateBookingDTO createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var booking = await _bookingService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BookingDTO>> UpdateBooking(int id, UpdateBookingDTO updateDto)
        {
            var existingBooking = await _bookingService.GetByIdAsync(id);
            if (existingBooking == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && existingBooking.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                var booking = await _bookingService.UpdateAsync(id, updateDto);
                return Ok(booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && booking.UserId != userId)
            {
                return Forbid();
            }

            var result = await _bookingService.DeleteAsync(id);
            return NoContent();
        }
    }
}
