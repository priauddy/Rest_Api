namespace Court_Management.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CourtId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public virtual ApplicationUser User { get; set; }
        public virtual Court Court { get; set; }
        public virtual Payment Payment { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }
}
