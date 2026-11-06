using DataWebService.Database.Repositories;
using DataWebService.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DataWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly Dictionary<string, string> problems = new Dictionary<string, string>();

        // POST api/account - Create account 
        [HttpPost]
        public IActionResult Register([FromBody] Account acc)
        {
            if (string.IsNullOrWhiteSpace(acc.AccName)) { problems["accNum"] = "Account # is required."; }
            if (int.IsNegative(acc.OwnerID)) { problems["ownerID"] = "Account owner ID is required."; }

            if (problems.Count > 0)
            {
                return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "One or more fields are missing or invalid.", problems));
            }

            // overwrite fields the client must not control
            acc.Active = true;

            if (!AccountRepository.InsertAccount(acc))
            {
                return StatusCode(500, APIErrors.ServerError());
            }
            return Created($"/api/account/{acc.AccNum}", acc);
        }

        // GET api/account/1 - Retrieve account based off account number
        [HttpGet("account/{accNum}")]
        public IActionResult Get(int accNum)
        {
            Account? acc = AccountRepository.GetAccountFromAccNum(accNum);
            if (acc == null)
            {
                return NotFound(APIErrors.NotFound("ACCOUNT_NOT_FOUND", $"No acount with account# '{accNum}'."));
            }
            return Ok(acc);
        }

        [HttpGet("getAccounts/{ownerID}")]
        public IActionResult GetAccounts(int ownerID)
        {
            List<Account?> account = AccountRepository.GetAllAccountsFromOwnerID(ownerID);
            if (account == null)
            {
                return NotFound(APIErrors.NotFound("ACCOUNTS_NOT_FOUND", $"No accounts with owner ID '{ownerID}'."));
            }
            return Ok(account);

        }


        // PUT api/put/1 - Update account details
        [HttpPut("updateAccount/{accNum:int}")]
        public IActionResult UpdateAccName(int accNum, [FromBody] Account acc)
        {
            if (string.IsNullOrWhiteSpace(acc.AccName))
            {
                return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Account # is either missing or invalid"));
            }
            if (!AccountRepository.UpdateAccountName(accNum, acc.AccName))
            {
                return NotFound(APIErrors.NotFound("ACCOUNT_NOT_FOUND", $"No account with account # '{accNum}'."));
            }
            return NoContent(); // 204
        }

        // PATCH api/account/5/deactivate - Deactivate Account
        [HttpPatch("deactivate/{accNum:int}")]
        public IActionResult Deactivate(int accNum)
        {
            if (!AccountRepository.Deactivate(accNum))
            {
                return NotFound(APIErrors.NotFound("ACCOUNT_NOT_FOUND", $"No account with account # '{accNum}'."));
            }
            return NoContent(); // 204
        }
    }
}