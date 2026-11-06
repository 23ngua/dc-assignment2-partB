namespace BankWebApp.Models.DTOs
{
    public class TransactionDTO
    {
        public int TransactionID { get; set; }
        public int Type { get; set; }
        public int? FromAcc { get; set; }
        public int? ToAcc { get; set; }
        public string? Description { get; set; }
        public long Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
