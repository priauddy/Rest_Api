namespace Court_Management.Models.DTOs
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CourtId { get; set; }
        public string CourtName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
    }

    public class CreateBookingDTO
    {
        public int CourtId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class UpdateBookingDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
    }

    public class BookingAvailabilityDTO
    {
        public int CourtId { get; set; }
        public DateTime Date { get; set; }
        public List<TimeSlot> AvailableSlots { get; set; }
    }

    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
