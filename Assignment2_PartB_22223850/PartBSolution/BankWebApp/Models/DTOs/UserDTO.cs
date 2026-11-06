namespace BankWebApp.Models.DTOs
{
    // Data Transfer Object - user data returned by Part A Web API
    public class UserDTO
    {
        public int UserID { get; set; }
        public string Username { get; set; } = "";
        public string FName { get; set; } = "";
        public string LName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string PhoneNum { get; set; } = "";
        public string? Picture { get; set; }
        public string Password { get; set; } = "";
        public int Role { get; set; }
        public bool Active { get; set; }
    }
}
