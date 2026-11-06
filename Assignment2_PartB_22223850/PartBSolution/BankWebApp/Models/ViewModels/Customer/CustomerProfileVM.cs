using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Models.ViewModels.Customer
{
    public class CustomerProfileVM
    {
        public string Username { get; set; } = "";
        
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Address { get; set; } = "";

        [Required]
        [Phone]
        public string Phone { get; set; } = "";

        // Existing profile picture path/name
        public string? Picture { get; set; } = "";

        // New picture selected by customer
        public IFormFile? PictureFile { get; set; }

        // Optional new password
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string? NewPassword { get; set; }
    }
}