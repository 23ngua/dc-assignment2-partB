namespace DataWebService.Models.Entities
{
    public class Audit
    {
        public int AuditID { get; set; }

        // Admin who performed the action
        public string AdminUsername { get; set; } = "";

        // Example: Create User, Edit User, Deactivate User
        public string Action { get; set; } = "";

        // User/account affected by the action
        public string Target { get; set; } = "";

        // Extra info about what happened
        public string Details { get; set; } = "";

        // When the action occurred
        public DateTime Timestamp { get; set; }
    }
}
