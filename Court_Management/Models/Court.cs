namespace Court_Management.Models
{
    public class Court
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsIndoor { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsAvailable { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
