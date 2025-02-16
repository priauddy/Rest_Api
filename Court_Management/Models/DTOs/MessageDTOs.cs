namespace Court_Management.Models.DTOs
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
    }

    public class CreateMessageDTO
    {
        public string ReceiverId { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }

    public class UpdateMessageDTO
    {
        public bool IsRead { get; set; }
    }

    public class MessageSummaryDTO
    {
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }
        public List<MessageDTO> RecentMessages { get; set; }
    }
}
