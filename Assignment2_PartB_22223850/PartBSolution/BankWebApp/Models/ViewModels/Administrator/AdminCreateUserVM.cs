using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Models.ViewModels.Administrator
{
    public class AdminCreateUserVM
    {
        [Required]
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
        public string Phone { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "Customer";
    }
}