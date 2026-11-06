using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Models.ViewModels.Administrator
{
    public class AdminUserSearchVM
    {
        // Search text entered by the administrator
        [Required]
        [Display(Name = "Enter username: ")]
        public string SearchTerm { get; set; } = "";

        // User details returned from the search
        public string Username { get; set; } = "";

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";

        // Indicates whether a matching user was found
        public bool UserFound { get; set; }

        // Indicates whether a serch has actually been performed
        public bool SearchPerformed { get; set; }
    }
}