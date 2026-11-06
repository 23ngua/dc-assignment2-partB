namespace BankWebApp.Models.ViewModels.Customer
{
    public class CustomerTransactionVM
    {
        public string Type { get; set; } = "";

        public decimal Amount { get; set; }

        public string Description { get; set; } = "";

        public DateTime TimeStamp { get; set; }
    }
}
