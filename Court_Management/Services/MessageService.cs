using Court_Management.Data;
using Court_Management.Models;
using Court_Management.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Court_Management.Services
{
    public interface IMessageService : IBaseService<MessageDTO, CreateMessageDTO, UpdateMessageDTO>
    {
        Task<IEnumerable<MessageDTO>> GetUserInboxAsync(string userId);
        Task<IEnumerable<MessageDTO>> GetUserSentMessagesAsync(string userId);
        Task<MessageSummaryDTO> GetUserMessageSummaryAsync(string userId);
        Task<bool> MarkAsReadAsync(int messageId);
    }

    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MessageDTO>> GetAllAsync()
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                    ReceiverId = m.ReceiverId,
                    ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
                    Subject = m.Subject,
                    Content = m.Content,
                    SentDate = m.SentDate,
                    IsRead = m.IsRead
                })
                .ToListAsync();
        }

        public async Task<MessageDTO> GetByIdAsync(int id)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null) return null;

            return new MessageDTO
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
                ReceiverId = message.ReceiverId,
                ReceiverName = $"{message.Receiver.FirstName} {message.Receiver.LastName}",
                Subject = message.Subject,
                Content = message.Content,
                SentDate = message.SentDate,
                IsRead = message.IsRead
            };
        }

        public async Task<MessageDTO> CreateAsync(CreateMessageDTO createDto)
        {
            var message = new Message
            {
                SenderId = createDto.ReceiverId, // This will be set by the controller
                ReceiverId = createDto.ReceiverId,
                Subject = createDto.Subject,
                Content = createDto.Content,
                SentDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(message.Id);
        }

        public async Task<MessageDTO> UpdateAsync(int id, UpdateMessageDTO updateDto)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return null;

            message.IsRead = updateDto.IsRead;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(message.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MessageDTO>> GetUserInboxAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentDate)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                    ReceiverId = m.ReceiverId,
                    ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
                    Subject = m.Subject,
                    Content = m.Content,
                    SentDate = m.SentDate,
                    IsRead = m.IsRead
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MessageDTO>> GetUserSentMessagesAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SentDate)
                .Select(m => new MessageDTO
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                    ReceiverId = m.ReceiverId,
                    ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
                    Subject = m.Subject,
                    Content = m.Content,
                    SentDate = m.SentDate,
                    IsRead = m.IsRead
                })
                .ToListAsync();
        }

        public async Task<MessageSummaryDTO> GetUserMessageSummaryAsync(string userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .ToListAsync();

            return new MessageSummaryDTO
            {
                TotalMessages = messages.Count,
                UnreadMessages = messages.Count(m => !m.IsRead),
                RecentMessages = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Where(m => m.ReceiverId == userId)
                    .OrderByDescending(m => m.SentDate)
                    .Take(5)
                    .Select(m => new MessageDTO
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                        ReceiverId = m.ReceiverId,
                        ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
                        Subject = m.Subject,
                        Content = m.Content,
                        SentDate = m.SentDate,
                        IsRead = m.IsRead
                    })
                    .ToListAsync()
            };
        }

        public async Task<bool> MarkAsReadAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return false;

            message.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
