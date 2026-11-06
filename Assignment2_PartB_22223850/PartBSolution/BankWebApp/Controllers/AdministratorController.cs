using BankWebApp.Helpers;
using BankWebApp.Models.DTOs;
using BankWebApp.Models.Enums;
using BankWebApp.Models.ViewModels.Administrator;
using BankWebApp.Models.ViewModels.Customer;
using BankWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankWebApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : Controller
    {
        private readonly PartAService _partAService;

        public AdministratorController(PartAService partAService)
        {
            _partAService = partAService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Display administrator create user form
        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new AdminCreateUserVM();

            return View(model);
        }

        // Handle administrator create user submission
        [HttpPost]
        public async Task<IActionResult> CreateUser(AdminCreateUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var newUser = new UserDTO
            {
                Username = model.Username,
                FName = model.FirstName,
                LName = model.LastName,
                Email = model.Email,
                Address = model.Address,
                PhoneNum = model.Phone,
                Password = model.Password,

                Picture = "",

                Role = model.Role == nameof(UserRole.Administrator) ? (int)UserRole.Administrator : (int)UserRole.Customer,
                Active = true
            };

            var result = await _partAService.CreateUserAsync(newUser);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty, 
                    string.IsNullOrWhiteSpace(result.Message)
                        ? "Unable to create the user. Please try again."
                        : result.Message);

                return View(model);
            }

            // Record admin action in audit log
            var audit = new AuditDTO
            {
                AdminUsername = User.Identity?.Name ?? "Unknown",
                Action = "Create User",
                Target = model.Username,
                Details = $"Created user {model.Username} with role {model.Role}."
            };

            var auditResult = await _partAService.CreateAuditAsync(audit);

            if (!auditResult.Success)
            {
                TempData["UserUpdateError"] =
                    string.IsNullOrWhiteSpace(auditResult.Message)
                        ? $"User {model.Username} was created, but the audit record could not be created."
                        : $"User {model.Username} was created, but the audit record could not be created. {auditResult.Message}";

                return RedirectToAction(nameof(UserSearch));
            }

            TempData["UserUpdateMessage"] = $"User {model.Username} was created successfully.";

            return RedirectToAction(nameof(UserSearch));
        }

        // Display the user search page
        [HttpGet]
        public IActionResult UserSearch()
        {
            var model = new AdminUserSearchVM();

            return View(model);
        }

        // Display administrator transaction search page
        [HttpGet]
        public IActionResult TransactionSearch()
        {
            var model = new AdminTransactionSearchVM();

            return View(model);
        }

        // Display admin audit log
        [HttpGet]
        public async Task<IActionResult> AuditLog()
        {
            var auditResult = await _partAService.GetAuditAsync();

            var model = new AdminAuditLogVM
            {
                Audits = auditResult.Audits ?? new List<AuditDTO>()
            };

            if (auditResult.Audits == null)
            {
                ViewData["AuditError"] =
                    string.IsNullOrWhiteSpace(auditResult.Message)
                        ? "Unable to retrieve the audit log."
                        : auditResult.Message;
            }

            return View(model);
        }

        // Handle administrator transaction search
        [HttpPost]
        public async Task<IActionResult> TransactionSearch(AdminTransactionSearchVM model)
        {
            model.SearchPerformed = true;

            // Require an account number for now
            if (!model.AccountNumber.HasValue)
            {
                ModelState.AddModelError(nameof(model.AccountNumber), "Please enter an account number.");

                return View(model);
            }

            var accountResult = await _partAService.GetAccountAsync(model.AccountNumber.Value);

            var account = accountResult.Account;

            if (account == null)
            {
                ModelState.AddModelError(
                    nameof(model.AccountNumber),
                    string.IsNullOrWhiteSpace(accountResult.Message)
                        ? "Unable to retrieve the account."
                        : accountResult.Message);

                return View(model);
            }

            var transactionsResult = await _partAService.GetTransactionsForAccountAsync(model.AccountNumber.Value);

            var transactions = transactionsResult.Transactions;

            if (transactions == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(transactionsResult.Message)
                        ? "Unable to retrieve transactions."
                        : transactionsResult.Message);

                return View(model);
            }

            var filteredTransactions = transactions.AsEnumerable();

            // DATE RANGE FILTERING
            if (model.StartDate.HasValue)
            {
                filteredTransactions = filteredTransactions
                    .Where(t => t.Timestamp >= model.StartDate.Value.Date);
            }

            if (model.EndDate.HasValue)
            {
                var dayAfterEndDate = model.EndDate.Value.Date.AddDays(1);

                filteredTransactions = filteredTransactions
                    .Where(t => t.Timestamp < dayAfterEndDate);
            }

            // SORTING
            filteredTransactions = model.SortOrder switch
            {
                "date_asc" => filteredTransactions.OrderBy(t => t.Timestamp),

                "amount_asc" => filteredTransactions.OrderBy(t => t.Amount),

                "amount_desc" => filteredTransactions.OrderByDescending(t => t.Amount),

                _ => filteredTransactions.OrderByDescending(t => t.Timestamp)
            };

            // PAGING
            const int pageSize = 10;

            if (model.CurrentPage < 1)
            {
                model.CurrentPage = 1;
            }

            var totalTransactions = filteredTransactions.Count();

            model.TotalPages = Math.Max(1, (int)Math.Ceiling(totalTransactions / (double)pageSize));

            if (model.CurrentPage > model.TotalPages)
            {
                model.CurrentPage = model.TotalPages;
            }

            var pagedTransactions = filteredTransactions
                .Skip((model.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            model.PageSize = pageSize;

            model.Transactions = pagedTransactions
                .Select(t => new CustomerTransactionVM
                {
                    Type = TransactionTypeName.ToDisplayName(t.Type),
                    Amount = t.Amount / 100m,
                    TimeStamp = t.Timestamp,
                    Description = t.Description ?? ""
                })
                .ToList();

            return View(model);
        }

        // Handle administrator user search
        [HttpPost]
        public async Task<IActionResult> UserSearch(AdminUserSearchVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.SearchPerformed = true;

            UserDTO? user = null;

            // If search term is a number, treat it as an account number
            if (int.TryParse(model.SearchTerm, out int accountNumber))
            {
                var accountResult =
                    await _partAService.GetAccountAsync(accountNumber);

                var account = accountResult.Account;

                if (account == null)
                {
                    model.UserFound = false;

                    ModelState.AddModelError(
                        string.Empty,
                        string.IsNullOrWhiteSpace(accountResult.Message)
                            ? "Unable to retrieve the account."
                            : accountResult.Message);

                    return View(model);
                }

                // Find the owner of the account
                var userResult =
                    await _partAService.GetUserByIdAsync(account.OwnerID);

                user = userResult.User;

                if (user == null)
                {
                    model.UserFound = false;

                    ModelState.AddModelError(
                        string.Empty,
                        string.IsNullOrWhiteSpace(userResult.Message)
                            ? "Unable to retrieve the user."
                            : userResult.Message);

                    return View(model);
                }
            }
            else
            {
                // Otherwise search by username
                var userResult =
                    await _partAService.GetUserAsync(model.SearchTerm);

                user = userResult.User;

                if (user == null)
                {
                    model.UserFound = false;

                    ModelState.AddModelError(
                        string.Empty,
                        string.IsNullOrWhiteSpace(userResult.Message)
                            ? "Unable to retrieve the user."
                            : userResult.Message);

                    return View(model);
                }
            }

            model.UserFound = true;

            model.Username = user.Username;
            model.FirstName = user.FName;
            model.LastName = user.LName;
            model.Email = user.Email;
            model.Address = user.Address;
            model.Phone = user.PhoneNum;

            return View(model);
        }

        [HttpGet]  
        public async Task<IActionResult> EditUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction(nameof(UserSearch));
            }

            var userResult = await _partAService.GetUserAsync(username);

            var user = userResult.User;

            if (user == null)
            {
                TempData["UserUpdateError"] =
                    string.IsNullOrWhiteSpace(userResult.Message)
                        ? "Unable to retrieve user details."
                        : userResult.Message;

                return RedirectToAction(nameof(UserSearch));
            }

            var model = new AdminEditUserVM
            {
                Username = user.Username,
                FirstName = user.FName,
                LastName = user.LName,
                Email = user.Email,
                Address = user.Address,
                Phone = user.PhoneNum
            };

            return View(model);
        }

        // Handles administrator user edit submission
        [HttpPost]
        public async Task<IActionResult> EditUser(AdminEditUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUserResult = await _partAService.GetUserAsync(model.Username);

            var existingUser = existingUserResult.User;

            if (existingUser == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(existingUserResult.Message)
                        ? "Unable to retrieve the user."
                        : existingUserResult.Message);

                return View(model);
            }

            var updatedUser = new UserDTO
            {
                Username = model.Username,
                FName = model.FirstName,
                LName = model.LastName,
                Email = model.Email,
                Address = model.Address,
                PhoneNum = model.Phone,

                // Preserve values that the admin is not editing
                Picture = existingUser.Picture,
                Password = existingUser.Password,
                Role = existingUser.Role,
                Active = existingUser.Active
            };

            var result = await _partAService.UpdateUserAsync(model.Username, updatedUser);

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty, 
                    string.IsNullOrWhiteSpace(result.Message) 
                        ? "Unable to update the user. Please try again."
                        : result.Message);

                return View(model);
            }

            // Update password separately if admin entered a new password
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var passwordResult = await _partAService.ChangePasswordAsync(model.Username, model.NewPassword);

                if (!passwordResult.Success)
                {
                    ModelState.AddModelError(
                        nameof(model.NewPassword),
                        string.IsNullOrWhiteSpace(passwordResult.Message)
                            ? "Unable to update the password."
                            : passwordResult.Message);

                    return View(model);
                }
            }

            // Record admin action in audit log
            var audit = new AuditDTO
            {
                AdminUsername = User.Identity?.Name ?? "Unknown",
                Action = "Edit User",
                Target = model.Username,
                Details = $"Updated details for user {model.Username}."
            };

            var auditResult = await _partAService.CreateAuditAsync(audit);

            if (!auditResult.Success)
            {
                TempData["UserUpdateError"] =
                    string.IsNullOrWhiteSpace(auditResult.Message)
                        ? $"User {model.Username} was updated, but the audit record could not be created."
                        : $"User {model.Username} was updated, but the audit record could not be created. {auditResult.Message}";

                return RedirectToAction(nameof(UserSearch));
            }

            TempData["UserUpdateMessage"] = $"User {model.Username} was updated successfully.";

            return RedirectToAction(nameof(UserSearch));
        }

        // Handles administrator user deactivation
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["UserUpdateMessage"] = "Unable to deactivate user.";
                return RedirectToAction(nameof(UserSearch));
            }

            var deactivateResult = await _partAService.DeactivateUserAsync(username);

            if (!deactivateResult.Success)
            {
                TempData["UserUpdateError"] =
                    string.IsNullOrWhiteSpace(deactivateResult.Message)
                        ? $"Unable to deactivate user {username}."
                        : deactivateResult.Message;

                return RedirectToAction(nameof(UserSearch));
            }

            // Record admin action in audit log
            var audit = new AuditDTO
            {
                AdminUsername = User.Identity?.Name ?? "Unknown",
                Action = "Deactivate User",
                Target = username,
                Details = $"Deactivated user {username}."
            };

            var auditResult = await _partAService.CreateAuditAsync(audit);

            if (!auditResult.Success)
            {
                TempData["UserUpdateError"] =
                    string.IsNullOrWhiteSpace(auditResult.Message)
                        ? "The user was deactivated, but the audit record could not be created."
                        : $"The user was deactivated, but the audit record could not be created. {auditResult.Message}";

                return RedirectToAction(nameof(UserSearch));
            }

            TempData["UserUpdateMessage"] = $"User {username} was deactivated successfully.";

            return RedirectToAction(nameof(UserSearch));
        }
    }
}