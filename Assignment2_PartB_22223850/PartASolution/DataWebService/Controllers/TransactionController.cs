using DataWebService.Controllers;
using DataWebService.Database.Repositories;
using DataWebService.Models.Entities;
using DataWebService.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DataWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        // POST api/transaction - Create transaction 
        [HttpPost("transfer")]
        public IActionResult Register([FromBody] Transaction transaction)
        {
            if (transaction.Amount <= 0 || transaction.Amount > 99999999)
            {
                return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Transaction amount needs to be positive"));
            }
            if (transaction.Amount > TransactionRepository.getBalanceOfAcc(transaction.FromAcc ?? 0))
            {
                return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Account dosnt have enough money for request"));
            }

            // Specific logic for each Transaction type
            switch (transaction.Type)
            {
                case TransactionType.Deposit:   // MONEY GOING TOACC, NO FROMACC (1)
                    if(!AccountRepository.accountExists(transaction.ToAcc))
                    {
                        return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Invalid toAcc for Deposit"));
                    }

                    long newToBal = transaction.Amount + TransactionRepository.getBalanceOfAcc((int)transaction.ToAcc);
                    AccountRepository.UpdateBalance(transaction.ToAcc ?? 0, newToBal);  // Updates Balance of ToAcc
                    
                    break;

                case TransactionType.Withdrawal:    // MONEY GOING FROMACC, NO TOACC (2)
                    if (!AccountRepository.accountExists(transaction.FromAcc))
                    {
                        return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Invalid toAcc or fromAcc for Withdrawl"));
                    }

                    long newFromBal = TransactionRepository.getBalanceOfAcc((int)transaction.FromAcc) - transaction.Amount;
                    AccountRepository.UpdateBalance(transaction.FromAcc ?? 0, newFromBal);  // Updates Balance of ToAcc

                    break;

                case TransactionType.Transfer:  // MONEY GOING FromAcc -> ToAcc (0)
                    if (!AccountRepository.accountExists(transaction.ToAcc) && !AccountRepository.accountExists(transaction.FromAcc))
                    {
                        return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Invalid toAcc or fromAcc for Transfer"));
                    }

                    long newBalTo = transaction.Amount + TransactionRepository.getBalanceOfAcc((int)transaction.ToAcc);
                    long newBalFrom = TransactionRepository.getBalanceOfAcc((int)transaction.FromAcc) - transaction.Amount;
                    AccountRepository.UpdateBalance(transaction.ToAcc ?? 0, newBalTo);  // Updates Balance of ToAcc
                    AccountRepository.UpdateBalance(transaction.FromAcc ?? 0, newBalFrom);  // Updates Balance of FromAcc

                    break;

                default:
                    return StatusCode(500, APIErrors.ServerError());

            }

            TransactionRepository.InsertTransaction(transaction);   // Inserts New transaction

            return Created($"{transaction.TransactionID}", transaction);    // Successful transaction
        }

        // GET api/transaction/1 - Retrieve transaction from Transaction ID
        [HttpGet("getFromTransactionID/{transactionID}")]
        public IActionResult GetFromTransactionID(int transactionID)
        {
            Transaction? transaction = TransactionRepository.GetTransactionFromTransactionID(transactionID);
            if (transaction == null)
            {
                return NotFound(APIErrors.NotFound("TRANSACTION_NOT_FOUND", $"No transaction with transaction ID '{transactionID}'."));
            }
            return Ok(transaction);
        }

        // GET api/transaction/1 - Retrieve transaction from Transaction ID
        [HttpGet("getFromAccID/{accID}")]
        public IActionResult GetFromAccID(int accID)
        {
            List<Transaction?> transaction = TransactionRepository.GetAllTransactionsFromAccID(accID);
            if (transaction == null)
            {
                return NotFound(APIErrors.NotFound("TRANSACTIONS_NOT_FOUND", $"No transaction with account ID '{accID}'."));
            }
            return Ok(transaction);
        }


        // GET api/transaction/userID - Retrieve list of all transactionf from user ID
        [HttpGet("getFromUserID/{userID}")]
        public IActionResult GetAllFromUserID(int userID)
        {
            List<Transaction?> transaction = TransactionRepository.GetAllTransactionsFromUserID(userID);
            if (transaction == null)
            {
                return NotFound(APIErrors.NotFound("TRANSACTIONS_NOT_FOUND", $"No transactions with user ID '{userID}'."));
            }
            return Ok(transaction);
        }
    }
}