using BankWebApp.Models.ViewModels.Customer;
using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Models.ViewModels.Administrator
{
    public class AdminTransactionSearchVM
    {
        public int? AccountNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Sorting
        public string SortOrder { get; set; } = "date_desc";

        // Results
        public List<CustomerTransactionVM> Transactions { get; set; } = new();

        // Paging
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; } = 1;

        public bool SearchPerformed { get; set; }
    }
}
