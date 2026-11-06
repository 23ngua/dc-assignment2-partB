using BankWebApp.Models;

namespace BankWebApp.Models.ViewModels.Customer
{
    public class CustomerAccountSummaryVM
    {
        public string AccountName { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public decimal Balance { get; set; }
        public List<CustomerTransactionVM> Transactions { get; set; } = new();
    }
}