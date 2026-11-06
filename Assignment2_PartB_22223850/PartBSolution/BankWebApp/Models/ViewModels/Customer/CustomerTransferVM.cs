using BankWebApp.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Models.ViewModels.Customer
{
    public class CustomerTransferVM
    {
        // Account that the customer is transferring money from
        [Required]
        public string FromAccount { get; set; } = "";

        // Accounts owned by the currently logged-in customer
        public List<AccountDTO> SourceAccounts { get; set; } = new();

        // Destination account number
        [Required]
        [Display(Name = "To Account")]
        public string ToAccount { get; set; } = "";

        // Amount of money to transfer
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than $0.")]
        public decimal Amount { get; set; }

        // The optional description/reference for the transfer
        [Display(Name = "Description")]
        public string Description { get; set; } = "";
    }
}
