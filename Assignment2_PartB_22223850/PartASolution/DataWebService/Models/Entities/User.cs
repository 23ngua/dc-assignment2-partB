using DataWebService.Models.Enums;

namespace DataWebService.Models.Entities
{
    public class User
    {
        public int UserID { get; set; } // primary key
        public string Username { get; set; } = ""; // unique
        public string FName { get; set; } = ""; 
        public string LName { get; set; } = "";
        public string Email { get; set; } = ""; // unique
        public string Address { get; set; } = "";
        public string PhoneNum { get; set; } = "";
        public string? Picture { get; set; }
        public string Password { get; set; } = ""; // stores the password (hash)
        public UserRole Role { get; set; }
        public bool Active { get; set; } = true;
    }
}