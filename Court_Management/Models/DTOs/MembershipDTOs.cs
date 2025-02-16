namespace Court_Management.Models.DTOs
{
    public class MembershipDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateMembershipDTO
    {
        public string UserId { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public int DurationInMonths { get; set; }
    }

    public class UpdateMembershipDTO
    {
        public string Type { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class MembershipTypeDTO
    {
        public string Type { get; set; }
        public decimal MonthlyPrice { get; set; }
        public string Description { get; set; }
        public List<string> Benefits { get; set; }
    }
}
