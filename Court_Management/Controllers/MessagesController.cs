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
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetAllMessages()
        {
            var messages = await _messageService.GetAllAsync();
            return Ok(messages);
        }

        [HttpGet("inbox")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMyInbox()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = await _messageService.GetUserInboxAsync(userId);
            return Ok(messages);
        }

        [HttpGet("sent")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMySentMessages()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var messages = await _messageService.GetUserSentMessagesAsync(userId);
            return Ok(messages);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<MessageSummaryDTO>> GetMyMessageSummary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var summary = await _messageService.GetUserMessageSummaryAsync(userId);
            return Ok(summary);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDTO>> GetMessage(int id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && message.ReceiverId != userId && message.SenderId != userId)
            {
                return Forbid();
            }

            return Ok(message);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createDto)
        {
            var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            try
            {
                var message = await _messageService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (message.ReceiverId != userId)
            {
                return Forbid();
            }

            var result = await _messageService.MarkAsReadAsync(id);
            if (!result)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _messageService.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && message.ReceiverId != userId && message.SenderId != userId)
            {
                return Forbid();
            }

            var result = await _messageService.DeleteAsync(id);
            return NoContent();
        }
    }
}
