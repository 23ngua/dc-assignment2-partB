namespace BankWebApp.Models.DTOs
{
    public class AuditDTO
    {
        public int AuditID { get; set; }

        public string AdminUsername { get; set; } = "";

        public string Action { get; set; } = "";

        public string Target { get; set; } = "";

        public string Details { get; set; } = "";

        public DateTime Timestamp { get; set; }
    }
}