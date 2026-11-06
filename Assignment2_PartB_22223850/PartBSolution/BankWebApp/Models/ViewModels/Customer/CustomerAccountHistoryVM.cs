using BankWebApp.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Models.ViewModels.Customer
{
    public class CustomerAccountHistoryVM
    {
        public string AccountNumber { get; set; } = "";

        public List<CustomerAccountSummaryVM> Accounts { get; set; } = new();
        public List<AccountDTO> AvailableAccounts { get; set; } = new();

        // Date range filtering
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Sorting
        public string SortOrder { get; set; } = "date_desc";

        // Paging
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; } = 1;
    }
}