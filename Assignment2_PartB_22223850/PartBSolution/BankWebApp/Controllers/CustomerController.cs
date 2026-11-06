using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankWebApp.Services;
using BankWebApp.Models.ViewModels.Customer;
using BankWebApp.Models.DTOs;
using BankWebApp.Helpers;

namespace BankWebApp.Controllers
{
    [Authorize(Roles = "Customer, Administrator")]
    public class CustomerController : Controller
    {
        private readonly PartAService _partAService;
        private readonly IWebHostEnvironment _environment;
        
        public CustomerController(PartAService partAService, IWebHostEnvironment environment)
        {
            _partAService = partAService;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var userResult = await _partAService.GetUserAsync(username);

            var user = userResult.User;

            if (user == null)
            {
                ViewData["ProfileError"] = 
                    string.IsNullOrWhiteSpace(userResult.Message)
                        ? "Unable to retrieve user details."
                        : userResult.Message;

                return View(new CustomerProfileVM
                {
                    Username = username
                });
            }

            var model = new CustomerProfileVM
            {
                Username = user.Username,
                FirstName = user.FName,
                LastName = user.LName,
                Email = user.Email,
                Address = user.Address,
                Phone = user.PhoneNum,
                Picture = user.Picture ?? ""
            };

            return View(model);
        }

        public async Task<IActionResult> AccountHistory(
            int? accNum = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string sortOrder = "date_desc",
            int page = 1) 
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
                return RedirectToAction("Index", "Login");

            var userResult = await _partAService.GetUserAsync(username);

            var user = userResult.User;

            if (user == null)
            {
                TempData["AccountHistoryError"] =
                    string.IsNullOrWhiteSpace(userResult.Message)
                        ? "Unable to retrieve user details."
                        : userResult.Message;

                return View(new CustomerAccountHistoryVM());
            }

            var accountsResult = await _partAService.GetAccountsForUserAsync(user.UserID);

            var accounts = accountsResult.Accounts;

            if (accounts == null)
            {
                TempData["AccountHistoryError"] =
                    string.IsNullOrWhiteSpace(accountsResult.Message)
                        ? "Unable to retrieve accounts."
                        : accountsResult.Message;

                return View(new CustomerAccountHistoryVM());
            }

            if (accounts.Count == 0)
            {
                TempData["AccountHistoryError"] = "No accounts were found for the user.";
                return View(new CustomerAccountHistoryVM());
            }

            // Use selected account, otherwise default to the customer's first account
            var selectedAccount = accNum.HasValue ? accounts.FirstOrDefault(a => a.AccNum == accNum.Value) : accounts.First();

            // Prevent customer from requesting another customer's account
            if (selectedAccount == null)
            {
                TempData["AccountHistoryError"] = "The selected account was not found.";
                return View(new CustomerAccountHistoryVM());
            }

            var transactionsResult = await _partAService.GetTransactionsForAccountAsync(selectedAccount.AccNum);

            var transactions = transactionsResult.Transactions;

            if (transactions == null)
            {
                TempData["AccountHistoryError"] =
                    string.IsNullOrWhiteSpace(transactionsResult.Message)
                        ? "Unable to retrieve transactions."
                        : transactionsResult.Message;

                return View(new CustomerAccountHistoryVM());
            }

            var filteredTransactions = transactions.AsEnumerable();

            // DATE RANGE FILTERING
            if (startDate.HasValue)
            {
                filteredTransactions = filteredTransactions
                    .Where(t => t.Timestamp >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                // Includes whole selected end date
                var dayAfterEndDate = endDate.Value.Date.AddDays(1);

                filteredTransactions = filteredTransactions
                    .Where(t => t.Timestamp < dayAfterEndDate);
            }

            // SORTING
            filteredTransactions = sortOrder switch
            {
                "date_asc" => filteredTransactions.OrderBy(t => t.Timestamp),

                "amount_asc" => filteredTransactions.OrderBy(t => t.Amount),

                "amount_desc" => filteredTransactions.OrderByDescending(t => t.Amount),

                _ => filteredTransactions.OrderByDescending(t => t.Timestamp)
            };

            // PAGING
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var totalTransactions = filteredTransactions.Count();

            var totalPages = Math.Max(1, (int)Math.Ceiling(totalTransactions / (double)pageSize));

            if (page > totalPages)
            {
                page = totalPages;
            }

            var pagedTransactions = filteredTransactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // CREATE VIEW MODEL

            var accountSummary = new CustomerAccountSummaryVM
            {
                AccountName = selectedAccount.AccName,
                AccountNumber = selectedAccount.AccNum.ToString(),

                // AccountDTO stores balance in cents
                Balance = selectedAccount.Balance / 100m,

                Transactions = pagedTransactions
                    .Select(t => new CustomerTransactionVM
                    {
                        Type = TransactionTypeName.ToDisplayName(t.Type),
                        Amount = t.Amount / 100m,
                        TimeStamp = t.Timestamp,
                        Description = t.Description ?? ""
                    })
                    .ToList()
            };

            var model = new CustomerAccountHistoryVM
            {
                AccountNumber = selectedAccount.AccNum.ToString(),
                AvailableAccounts = accounts,

                StartDate = startDate,
                EndDate = endDate,

                SortOrder = sortOrder,

                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,

                Accounts = new List<CustomerAccountSummaryVM>
                {
                    accountSummary
                }
            };

            return View(model);
        }

        // Display the transfer money form
        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var userResult = await _partAService.GetUserAsync(username);

            var user = userResult.User;

            if (user == null)
            {
                TempData["TransferError"] =
                    string.IsNullOrWhiteSpace(userResult.Message)
                        ? "Unable to retrieve user details."
                        : userResult.Message;

                return View(new CustomerTransferVM());
            }

            var accountsResult = await _partAService.GetAccountsForUserAsync(user.UserID);

            var accounts = accountsResult.Accounts;

            if (accounts == null)
            {
                TempData["TransferError"] =
                    string.IsNullOrWhiteSpace(accountsResult.Message)
                        ? "Unable to retrieve accounts."
                        : accountsResult.Message;

                return View(new CustomerTransferVM());
            }

            if (accounts.Count == 0)
            {
                TempData["TransferError"] = "No accounts were found for this user.";
                return View(new CustomerTransferVM());
            }

            var model = new CustomerTransferVM
            {
                FromAccount = accounts.First().AccNum.ToString(),
                SourceAccounts = accounts
            };

            return View(model);
        }

        // Handle transfer form submission
        [HttpPost]
        public async Task<IActionResult> Transfer(CustomerTransferVM model)
        {
            // Reload logged-in customer's accounts so dropdown reamins populated if transfer form has error
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var userResult = await _partAService.GetUserAsync(username);

            var user = userResult.User;

            if (user == null)
            {
                TempData["TransferError"] =
                    string.IsNullOrWhiteSpace(userResult.Message)
                        ? "Unable to retrieve user details."
                        : userResult.Message;

                return View(model);
            }

            var accountsResult = await _partAService.GetAccountsForUserAsync(user.UserID);

            var accounts = accountsResult.Accounts;

            if (accounts == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(accountsResult.Message)
                        ? "Unable to retrieve accounts."
                        : accountsResult.Message);

                return View(model);
            }

            model.SourceAccounts = accounts;
            
            // Check the validation attributes in TransferViewModel
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Make sure selected source account belongs to the logged-in customer
            if (!accounts.Any(a => a.AccNum.ToString() == model.FromAccount))
            {
                ModelState.AddModelError(nameof(model.FromAccount), "You can only transfer money from one of your own accounts.");

                return View(model);
            }

            // Prevent transferring money to the same account
            if (model.FromAccount == model.ToAccount)
            {
                ModelState.AddModelError(nameof(model.ToAccount), "You cannot transfer money to the same account.");

                return View(model);
            }

            if (!int.TryParse(model.FromAccount, out int fromAccount))
            {
                ModelState.AddModelError(nameof(model.FromAccount), "Invalid source amount.");

                return View(model);
            }

            if (!int.TryParse(model.ToAccount, out int toAccount))
            {
                ModelState.AddModelError(nameof(model.ToAccount), "Invalid destination amount.");

                return View(model);
            }

            var result = await _partAService.TransferMoneyAsync(
                fromAccount,
                toAccount,
                model.Amount,
                model.Description);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["TransferMessage"] = $"Transfer of {model.Amount:C} to account {model.ToAccount} was successful.";

            return RedirectToAction(nameof(TransferSuccess));
        }

        [HttpGet]
        public IActionResult TransferSuccess()
        {
            return View();
        }

        // Display edit profile form
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var userResult = await _partAService.GetUserAsync(username);

            var user = userResult.User;

            if (user == null)
            {
                TempData["ProfileError"] =
                    string.IsNullOrWhiteSpace(userResult.Message)
                        ? "Unable to retrieve your profile from the service."
                        : userResult.Message;

                return RedirectToAction(nameof(Profile));
            }

            var model = new CustomerProfileVM
            {
                Username = user.Username,
                FirstName = user.FName,
                LastName = user.LName,
                Email = user.Email,
                Address = user.Address,
                Phone = user.PhoneNum,
                Picture = user.Picture ?? ""
            };

            return View(model);
        }

        // Handle edit profile form submission
        [HttpPost] 
        public async Task<IActionResult> EditProfile(CustomerProfileVM model)
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var existingUserResult = await _partAService.GetUserAsync(username);

            var existingUser = existingUserResult.User;

            if (existingUser == null)
            {
                ViewData["ProfileError"] =
                    string.IsNullOrWhiteSpace(existingUserResult.Message)
                        ? "Unable to retrieve your current profile."
                        : existingUserResult.Message;

                return View(model);
            }

            // Check normal form validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Keep existing pfp unless new one is uploaded
            var picturePath = existingUser.Picture;

            // Process new pfp if supplied
            if (model.PictureFile != null && model.PictureFile.Length > 0)
            {
                // Only allow image files
                if (!model.PictureFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError(nameof(model.PictureFile), "Please select a valid image file.");

                    return View(model);
                }

                // Create pfp dir if does not alr exist
                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");

                Directory.CreateDirectory(uploadFolder);

                // Generate unique filename so uploads do not overwrite each other
                var extension = Path.GetExtension(model.PictureFile.FileName);

                var fileName = $"{Guid.NewGuid()}{extension}";

                var fullPath = Path.Combine(uploadFolder, fileName);

                // Save uploaded image
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.PictureFile.CopyToAsync(stream);
                }

                // Store relative webpath in database
                picturePath = $"/uploads/profile-pictures/{fileName}";
            }

            var updatedUser = new UserDTO
            {
                UserID = existingUser.UserID,
                Username = username,
                FName = model.FirstName,
                LName = model.LastName,
                Email = model.Email,
                Address = model.Address,
                PhoneNum = model.Phone,
                Picture = picturePath,
                Role = existingUser.Role,
                Active = existingUser.Active,

                // Password handled separately using password API
                Password = ""
            };

            var updateResult = await _partAService.UpdateUserAsync(username, updatedUser);

            if (!updateResult.Success)
            {
                ModelState.AddModelError(
                    string.Empty, 
                    string.IsNullOrWhiteSpace(updateResult.Message)
                        ? "Unable to update your profile. Please try again."
                        : updateResult.Message);

                return View(model);
            }

            // Change password separately if the customer entered a new one
            bool passwordChanged = false;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var passwordResult = await _partAService.ChangePasswordAsync(username, model.NewPassword);

                if (!passwordResult.Success)
                {
                    ModelState.AddModelError(nameof(model.NewPassword), passwordResult.Message);

                    return View(model);
                }

                passwordChanged = true;
            }

            // Success message
            if (passwordChanged)
            {
                TempData["ProfileMessage"] = "Profile details and password updated successfully.";
            }
            else
            {
                TempData["ProfileMessage"] = "Profile details updated successfully.";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}