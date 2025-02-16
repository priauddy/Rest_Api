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
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetAllPayments()
        {
            var payments = await _paymentService.GetAllAsync();
            return Ok(payments);
        }

        [HttpGet("my-payments")]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetMyPayments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(payments);
        }

        [HttpGet("my-summary")]
        public async Task<ActionResult<PaymentSummaryDTO>> GetMyPaymentSummary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var summary = await _paymentService.GetUserPaymentSummaryAsync(userId);
            return Ok(summary);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDTO>> GetPayment(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && payment.UserId != userId)
            {
                return Forbid();
            }

            return Ok(payment);
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentDTO>> ProcessPayment(CreatePaymentDTO createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Only admins can process payments for other users
            if (!User.IsInRole("Admin") && createDto.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(createDto);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentDTO>> UpdatePayment(int id, UpdatePaymentDTO updateDto)
        {
            try
            {
                var payment = await _paymentService.UpdateAsync(id, updateDto);
                if (payment == null)
                {
                    return NotFound();
                }
                return Ok(payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var result = await _paymentService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
