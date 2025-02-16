namespace Court_Management.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? BookingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentType Type { get; set; }
        public string TransactionId { get; set; }
        
        public virtual ApplicationUser User { get; set; }
        public virtual Booking Booking { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum PaymentType
    {
        Booking,
        Membership,
        Other
    }
}
