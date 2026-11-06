using DataWebService.Database.Repositories;
using DataWebService.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DataWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        // POST api/audit - create new audit record
        [HttpPost]
        public IActionResult CreateAudit([FromBody] Audit audit)
        {
            if (string.IsNullOrWhiteSpace(audit.AdminUsername) ||
                string.IsNullOrWhiteSpace(audit.Action) ||
                string.IsNullOrWhiteSpace(audit.Target))
            {
                return BadRequest("Admin username, action and target are required.");
            }

            audit.Timestamp = DateTime.Now;

            bool success = AuditRepository.InsertAudit(audit);

            if (!success)
            {
                return StatusCode(500, "Unable to create audit record.");
            }

            return Ok();
        }

        // GET api/audit - retrieve all audit records
        [HttpGet]
        public IActionResult GetAllAudits()
        {
            var audits = AuditRepository.GetAllAudits();

            return Ok(audits);
        }
    }
}
