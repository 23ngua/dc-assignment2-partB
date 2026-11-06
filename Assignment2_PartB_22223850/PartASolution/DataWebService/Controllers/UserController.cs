using DataWebService.Database.Repositories;
using DataWebService.Models.Entities;
using DataWebService.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DataWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IPasswordHasher<User> _hasher; // uses aspnetcore built in password hasher
        private readonly Dictionary<string, string> problems = new Dictionary<string, string>();

        public UserController(IPasswordHasher<User> hasher)
        {
            _hasher = hasher;
        }

        // GET api/user/freddyb - Retrieve a user by username
        [HttpGet("{username}")]
        public IActionResult Get(string username)
        {
            User? user = UserRepository.GetUserFromUsername(username);
            if (user == null)
            {
                return NotFound(APIErrors.NotFound("USER_NOT_FOUND", $"No user with username '{username}'."));
            }
            return Ok(HidePassword(user));
        }

        // GET api/user/email/freddyb@something.com - Retrieve a user by email
        [HttpGet("email/{email}")]
        public IActionResult GetByEmail(string email)
        {
            User? user = UserRepository.GetUserFromEmail(email);
            if (user == null)
            {
                return NotFound(APIErrors.NotFound("USER_NOT_FOUND", $"No user with email '{email}'."));
            }
            return Ok(HidePassword(user));
        }

        // POST api/user - Register a new user, password gets hashed here
        [HttpPost]
        public IActionResult Register([FromBody] User newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.Username)) { problems["username"] = "Username is required."; }
            if (string.IsNullOrWhiteSpace(newUser.FName)) { problems["fName"] = "First name is required."; }
            if (string.IsNullOrWhiteSpace(newUser.LName)) { problems["lName"] = "Last name is required."; }
            if (string.IsNullOrWhiteSpace(newUser.Email)) { problems["email"] = "Email is required."; }
            if (string.IsNullOrWhiteSpace(newUser.Address)) { problems["address"] = "Address is required."; }
            if (string.IsNullOrWhiteSpace(newUser.PhoneNum)) { problems["phoneNum"] = "Phone number is required."; }
            if (string.IsNullOrWhiteSpace(newUser.Password) || newUser.Password.Length < 8) { problems["password"] = "Password must be at least 8 characters."; }

            if (problems.Count > 0)
            {
                return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "One or more fields are missing or invalid.", problems));
            }

            // 409: duplicates
            if (UserRepository.GetUserFromUsername(newUser.Username) != null)
            {
                return Conflict(APIErrors.Conflict("USERNAME_TAKEN", "Username already taken."));
            }
            if (UserRepository.GetUserFromEmail(newUser.Email) != null)
            {
                return Conflict(APIErrors.Conflict("EMAIL_TAKEN", "Email already taken."));
            }

            // overwrite fields the client must not control
            newUser.Role = UserRole.Customer;
            newUser.Active = true;
            newUser.Password = _hasher.HashPassword(newUser, newUser.Password);

            if (!UserRepository.InsertUser(newUser))
            {
                return StatusCode(500, APIErrors.ServerError());
            }

            return Created($"/api/user/{newUser.Username}", HidePassword(newUser));
        }

        // PUT api/user/freddyb - Update profile details (name, email, address, phone, picture only)
        [HttpPut("{username}")]
        public IActionResult Update(string username, [FromBody] User userChanges)
        {
            // find user
            User? existing = UserRepository.GetUserFromUsername(username);
            if (existing == null)
            {
                return NotFound(APIErrors.NotFound("USER_NOT_FOUND", $"No user with username '{username}'."));
            }

            // check body fields
            if (string.IsNullOrWhiteSpace(userChanges.FName)) { problems["fName"] = "First name is required."; }
            if (string.IsNullOrWhiteSpace(userChanges.LName)) { problems["lName"] = "Last name is required."; }
            if (string.IsNullOrWhiteSpace(userChanges.Email)) { problems["email"] = "Email is required."; }
            if (string.IsNullOrWhiteSpace(userChanges.Address)) { problems["address"] = "Address is required."; }
            if (string.IsNullOrWhiteSpace(userChanges.PhoneNum)) { problems["phoneNum"] = "Phone number is required."; }

            if (problems.Count > 0)
            {
                return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Some fields either missing or required", problems));
            }

            // check email uniqueness
            User? emailOwner = UserRepository.GetUserFromEmail(userChanges.Email);
            if (emailOwner != null && emailOwner.UserID != existing.UserID)
            {
                return Conflict(APIErrors.Conflict("EMAIL_TAKEN", "Email already taken."));
            }

            // copy only the allowed fields (not password, role and active)
            existing.FName = userChanges.FName;
            existing.LName = userChanges.LName;
            existing.Email = userChanges.Email;
            existing.Address = userChanges.Address;
            existing.PhoneNum = userChanges.PhoneNum;
            existing.Picture = userChanges.Picture;

            if (!UserRepository.UpdateUser(existing))
            {
                return StatusCode(500, APIErrors.ServerError());
            }
            return NoContent(); // 204
        }

        // PUT api/user/fredb/password - Change password (body: { "password": "newpassword" })
        [HttpPut("{username}/password")]
        public IActionResult ChangePassword(string username, [FromBody] User body)
        {
            if (string.IsNullOrWhiteSpace(body.Password) || body.Password.Length < 8)
            {
                problems["password"] = "Password must be at least 8 characters.";
                return BadRequest(APIErrors.BadRequest("INVALID_FIELDS_INPUTTED", "Some fields either missing or required", problems));
            }

            User? existing = UserRepository.GetUserFromUsername(username);
            if (existing == null)
            {
                return NotFound(APIErrors.NotFound("USER_NOT_FOUND", $"No user with username '{username}'."));
            }

            string hash = _hasher.HashPassword(existing, body.Password);
            if (!UserRepository.UpdatePassword(username, hash))
            {
                return StatusCode(500, APIErrors.ServerError());
            }
            return NoContent(); // 204
        }

        // PATCH api/user/fredb/deactivate - Deactivate a user
        [HttpPatch("{username}/deactivate")]
        public IActionResult Deactivate(string username)
        {
            if (!UserRepository.Deactivate(username))
            {
                return NotFound(APIErrors.NotFound("USER_NOT_FOUND", $"No user with username '{username}'."));
            }
            return NoContent(); // 204
        }

        // POST api/user/login - Verify username/email and password
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginDetails)
        {
            if (string.IsNullOrWhiteSpace(loginDetails.Username) ||
                string.IsNullOrWhiteSpace(loginDetails.Password)) 
            {
                return BadRequest("username and password are required");
            }

            User? user = UserRepository.GetUserFromUsername(loginDetails.Username);

            if (user == null)
            {
                user = UserRepository.GetUserFromEmail(loginDetails.Username);
            }

            if (user == null)
            {
                return Unauthorized("Invalid username/email or password");
            }

            if (!user.Active)
            {
                return Unauthorized("User account is inactive");
            }

            var result = _hasher.VerifyHashedPassword(user, user.Password, loginDetails.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid username/email or password");
            }

            return Ok(HidePassword(user));
        }

        // GET api/user/id/5 - Retrieve a user by user ID
        [HttpGet("id/{userId:int}")]
        public IActionResult GetById(int userId)
        {
            User? user = UserRepository.GetUserFromUserID(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(HidePassword(user));
        }
        
        // blanks passwords, passwords leaving server shouldnt be revealed
        private static User HidePassword(User user)
        {
            user.Password = "";
            return user;
        }
    }
}