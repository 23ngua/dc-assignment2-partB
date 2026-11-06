using BankWebApp.Models.DTOs;
using BankWebApp.Models.Enums;
using BankWebApp.Models.ViewModels.Auth;
using RestSharp;

// Simple test method - to prove BankWebApp can reach DataWebService

namespace BankWebApp.Services
{
    public class PartAService
    {
        private readonly RestClient _client;

        public PartAService(IConfiguration configuration)
        {
            var baseUrl = configuration["PartAService:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("Part A service base URL is not configured.");
            }

            _client = new RestClient(baseUrl);
        }

        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                var request = new RestRequest("swagger/index.html", Method.Get);
                var response = await _client.ExecuteAsync(request);

                return response.IsSuccessful;
            }
            catch
            {
                return false;
            }
        }

        // BankWebApp -> GET api/user/{username} -> Part A -> return PartAUserDto
        public async Task<(UserDTO? User, string Message)> GetUserAsync(string username)
        {
            try
            {
                var request = new RestRequest($"api/user/{username}", Method.Get);
                var response = await _client.ExecuteAsync<UserDTO>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    return (response.Data, "");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (null, "No matching user was found.");
                }

                return (null, "Unable to retrieve the user. The banking service returned an error.");
            }
            catch
            {
                return (null, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // Get user by user ID
        public async Task<(UserDTO? User, string Message)> GetUserByIdAsync(int userId)
        {
            try
            {
                var request = new RestRequest($"api/User/id/{userId}", Method.Get);

                var response = await _client.ExecuteAsync<UserDTO>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    return (response.Data, "");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (null, "No matching user was found.");
                }

                return (null, "Unable to retrieve the user. The banking service returned an error.");
            }
            catch
            {
                return (null, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        public async Task<(UserDTO? User, string Message)> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                var loginRequest = new UserDTO
                {
                    Username = usernameOrEmail,
                    Password = password
                };

                var request = new RestRequest("api/User/login", Method.Post);
                request.AddJsonBody(loginRequest);

                var response = await _client.ExecuteAsync<UserDTO>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    return (response.Data, "");
                }

                // PartA returned an error such as invalid credentials
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var message = response.Content.Trim().Trim('"');

                    return (null, message);
                }

                return (null, "Unable to log in. The banking service returned an error.");
            }
            catch
            {
                return (null, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // BankWebApp -> POST api/User -> Part A -> create new customer
        public async Task<(bool Success, string Message)> RegisterUserAsync(RegisterVM model)
        {
            try
            {
                var newUser = new
                {
                    Username = model.Username ?? "",
                    FName = model.FirstName ?? "",
                    LName = model.LastName ?? "",
                    Email = model.Email ?? "",
                    Address = model.Address ?? "",
                    PhoneNum = model.Phone ?? "",
                    Password = model.Password ?? "",

                    // Public registration only creates CUSTOMERS
                    Role = (int)UserRole.Customer,

                    // New users start active
                    Active = true,

                    Picture = ""
                };

                var request = new RestRequest("api/User", Method.Post);
                request.AddJsonBody(newUser);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "Registration successful.");
                }

                // Use any useful error returned by Part A
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var message = response.Content.Trim().Trim('"');
                    return (false, message);
                }

                if ((int)response.StatusCode == 0)
                {
                    return (false, "Unable to communicate with the banking service. Please try again later.");
                }

                return (false, "Unable to register the user. The banking service returned an error.");
            }
            catch
            {
                return (false, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // BankWebApp -> PUT api/User/{username} -> Part A -> update user details
        public async Task<(bool Success, string Message)> UpdateUserAsync(string username, UserDTO user)
        {
            try
            {
                var request = new RestRequest($"api/User/{Uri.EscapeDataString(username)}", Method.Put);

                request.AddJsonBody(user);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "User updated successfully.");
                }

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var message = response.Content.Trim().Trim('"');
                    return (false, message);
                }

                if ((int)response.StatusCode == 0)
                {
                    return (false, "Unable to communicate with the banking service. Please try again later.");
                }

                return (false, $"Unable to update user. The banking service returned an error.");
            }
            catch
            {
                return (false, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(UserDTO user)
        {
            try
            {
                var request = new RestRequest("api/User", Method.Post);

                request.AddJsonBody(user);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "User created successfully.");
                }

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var message = response.Content.Trim().Trim('"');

                    return (false, message);
                }

                return (false, $"Unable to communicate with the banking service. Please try again later.");
            }
            catch
            {
                return (false, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // Create an audit record
        public async Task<(bool Success, string Message)> CreateAuditAsync(AuditDTO audit)
        {
            try
            {
                var request = new RestRequest("api/Audit", Method.Post);

                request.AddJsonBody(audit);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "Audit record created successfully.");
                }

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var message = response.Content.Trim().Trim('"');

                    return (false, message);
                }

                return (false, "Unable to create the audit record. The banking service returned an error.");
            }
            catch
            {
                return (false, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // Get all audit records
        public async Task<(List<AuditDTO>? Audits, string Message)> GetAuditAsync()
        {
            try
            {
                var request = new RestRequest("api/Audit", Method.Get);

                var response = await _client.ExecuteAsync<List<AuditDTO>>(request);

                if (response.IsSuccessful)
                {
                    return (response.Data ?? new List<AuditDTO>(), "");
                }

                return (null, "Unable to retrieve the audit log. The banking service returned an error.");
            }
            catch
            {
                return (null, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // Change a user's password through PartA password endpoint
        public async Task<(bool Success, string Message)> ChangePasswordAsync(string username, string newPassword)
        {
            try
            {
                var request = new RestRequest($"api/User/{Uri.EscapeDataString(username)}/password", Method.Put);

                var passwordRequest = new UserDTO
                {
                    Username = username,
                    FName = "",
                    LName = "",
                    Email = "",
                    Address = "",
                    PhoneNum = "",
                    Picture = "",
                    Password = newPassword,
                    Role = (int)UserRole.Customer,
                    Active = true
                };

                request.AddJsonBody(passwordRequest);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "Password updated successfully.");
                }

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var message = response.Content.Trim().Trim('"');
                    return (false, message);
                }

                if ((int)response.StatusCode == 0)
                {
                    return (false, "Unable to communicate with the banking service. Please try again later.");
                }

                return (false, "Unable to update the password. The banking service returned an error.");
            }
            catch
            {
                return (false, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // BankWebApp -> PATCH api/User/{username}/deactivate
        public async Task<(bool Success, string Message)> DeactivateUserAsync(string username)
        {
            try
            {
                var request = new RestRequest($"api/User/deactivate/{username}", Method.Patch);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "User deactivated successfully.");
                }

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    var message = response.Content.Trim().Trim('"');

                    return (false, message);
                }

                return (false, "Unable to deactivate the user. The banking service returned an error.");
            }
            catch
            {
                return (false, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        public async Task<(List<AccountDTO>? Accounts, string Message)> GetAccountsForUserAsync(int ownerID)
        {
            try
            {
                var request = new RestRequest($"api/Account/getAccounts/{ownerID}", Method.Get);

                var response = await _client.ExecuteAsync<List<AccountDTO>>(request);

                if (response.IsSuccessful)
                {
                    return (response.Data ?? new List<AccountDTO>(), "");
                }

                return (null, "Unable to retrieve the accounts. The banking service returned an error.");
            }
            catch
            {
                return (null, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        // Get one account by account num
        public async Task<(AccountDTO? Account, string Message)> GetAccountAsync(int accNum)
        {
            try
            {
                var request = new RestRequest($"api/Account/{accNum}", Method.Get);

                var response = await _client.ExecuteAsync<AccountDTO>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    return (response.Data, "");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (null, "Account not found.");
                }

                return (null, "Unable to retrieve the account. The banking service returned an error.");
            }
            catch
            {
                return (null, "Unable to communicate with the banking service. Please try again later.");
            }
        }

        public async Task<(bool Success, string Message)> TransferMoneyAsync(
            int fromAccount,
            int toAccount,
            decimal amount,
            string description)
        {
            try
            {
                var transferRequest = new
                {
                    FromAcc = fromAccount,
                    ToAcc = toAccount,
                    Amount = Convert.ToInt64(amount * 100m),
                    Description = description
                };

                var request = new RestRequest("api/Transaction/transfer", Method.Post);
                request.AddJsonBody(transferRequest);

                var response = await _client.ExecuteAsync(request);

                // Transfer succeeded
                if (response.IsSuccessful)
                {
                    return (true, "Transfer completed successfully.");
                }

                // PartA returned an error message
                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    string message = response.Content.Trim().Trim('"');
                    return (false, message);
                }

                return (false, "Unable to complete the transfer.");
            }
            catch
            {
                return (false, "Unable to communicate with the banking service.");
            }
        }

        public async Task<(List<TransactionDTO>? Transactions, string Message)> GetTransactionsForAccountAsync(int accNum)
        {
            try
            {
                var request = new RestRequest($"api/Transaction/getFromAccID/{accNum}", Method.Get);

                var response = await _client.ExecuteAsync<List<TransactionDTO>>(request);

                if (response.IsSuccessful)
                {
                    return (
                        response.Data ?? new List<TransactionDTO>(), "");
                }

                return (null, "Unable to retrieve transactions. The banking service returned an error.");
            }
            catch
            {
                return (null, "Unable to communicate with the banking service. Please try again later.");
            }
        }
    }
}
