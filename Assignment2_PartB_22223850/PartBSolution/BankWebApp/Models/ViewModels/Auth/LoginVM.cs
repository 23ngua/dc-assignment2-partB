using System.ComponentModel.DataAnnotations;

/*
 * LoginViewModel.cs 
 * 
 * - Gives MVC login fields.
 * - Uses built-in validation attributes.
 * 
 */

namespace BankWebApp.Models.ViewModels.Auth
{
    public class LoginVM
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
