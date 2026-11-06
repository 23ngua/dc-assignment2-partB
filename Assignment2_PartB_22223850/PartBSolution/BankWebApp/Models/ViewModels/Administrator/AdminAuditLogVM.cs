using BankWebApp.Models.DTOs;

namespace BankWebApp.Models.ViewModels.Administrator
{
    public class AdminAuditLogVM
    {
        public List<AuditDTO> Audits { get; set; } = new();
    }
}
