namespace Court_Management.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public MembershipType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public decimal Price { get; set; }
        
        public virtual ApplicationUser User { get; set; }
    }

    public enum MembershipType
    {
        Basic,
        Premium,
        Gold
    }
}
