namespace Court_Management.Models.DTOs
{
    public class CourtDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsIndoor { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateCourtDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsIndoor { get; set; }
        public decimal HourlyRate { get; set; }
    }

    public class UpdateCourtDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsAvailable { get; set; }
    }
}
