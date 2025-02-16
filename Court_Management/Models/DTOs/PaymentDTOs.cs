namespace Court_Management.Models.DTOs
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int? BookingId { get; set; }
        public string BookingDetails { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string TransactionId { get; set; }
    }

    public class CreatePaymentDTO
    {
        public string UserId { get; set; }
        public int? BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class UpdatePaymentDTO
    {
        public string Status { get; set; }
        public string TransactionId { get; set; }
    }

    public class PaymentSummaryDTO
    {
        public decimal TotalAmount { get; set; }
        public int TotalPayments { get; set; }
        public Dictionary<string, decimal> PaymentsByType { get; set; }
        public List<PaymentDTO> RecentPayments { get; set; }
    }
}
