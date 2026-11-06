using DataWebService.Models.Enums;

namespace DataWebService.Models.Entities
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public TransactionType Type { get; set; }
        public int? FromAcc { get; set; } // this is null for deposits
        public int? ToAcc { get; set; } // and toAcc null for withdrawals
        public string? Description { get; set; }
        public long Amount { get; set; } // make sure amount is > 0, added check in sql
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}