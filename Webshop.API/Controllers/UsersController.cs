using Microsoft.AspNetCore.Mvc;
using Webshop.API.Attributes;
using Webshop.Data;
using Webshop.Services;
using Webshop.Shared.DTOs;
using Webshop.Shared.Enums;

namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwordService;
        private readonly ValidationService _validationService;

        public UsersController(
            UserService userService,
            EmailService emailService,
            IUserRepository repository,
            PasswordService passwordService,
            ValidationService validationService)
        {
            _userService = userService;
            _emailService = emailService;
            _userRepository = repository;
            _passwordService = passwordService;
            _validationService = validationService;
        }


        // GET: api/<UsersController>/all
        [HttpGet("all")]
        [SessionAuthorize(Roles = new[] { "Admin" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();

            return Ok(users.Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                CreatedAt = u.CreatedAt.ToString("o") // ISO 8601 format
            }));
        }

        // GET: api/<UsersController>/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> Get(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserDto userDto = new UserDto { Id = user.Id, Email = user.Email };
            return Ok(userDto);
        }

        // GET api/<UsersController>/me
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Ok(new { message = "User not logged in", role = "Guest" });
            }

            var role = HttpContext.Session.GetString("UserRole") ?? "Guest";
            var user = await _userRepository.GetByIdAsync(userId.Value)
                ?? throw new InvalidOperationException("User should never be null here.");

            if (user == null)
            {
                return Ok(new { message = "User not found", role = "Guest" });
            }

            return Ok(new { email = user.Email, role });
        }

        // POST api/<UsersController>/register
        [HttpPost("register")]
        [SessionAuthorize(Roles = new[] { "Guest" })]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> Register([FromBody] UserAuthDto userAuthDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterUserAsync(userAuthDto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var userDto = new UserDto { Id = result.User!.Id, Email = result.User.Email };
            return CreatedAtAction(nameof(Get), new { id = userDto.Id }, userDto);
        }

        // POST api/<UsersController>/login
        [HttpPost("login")]
        [SessionAuthorize(Roles = new[] { "Guest" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult> Login([FromBody] UserAuthDto userAuthDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ResultDto result = await _userService.LoginAsync(HttpContext, userAuthDto);

            if (!result.Success && result.Error == ErrorCode.RateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, result.Message);
            }

            if (!result.Success && result.Error == ErrorCode.WrongCredentials)
            {
                return Unauthorized(result.Message);
            }

            // Extract the cart before clearing the session
            var cartJson = HttpContext.Session.GetString("ShoppingCart");

            // ASVS: 3.2.1 - Clear session to prevent session fixation
            HttpContext.Session.Clear();
            Response.Cookies.Delete("__Host-WebshopSession");

            // Store user session
            var user = await _userRepository.GetUserByEmailAsync(userAuthDto.Email)
                ?? throw new InvalidOperationException("User should never be null here.");
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);

            // Restore the cart after login
            if (!string.IsNullOrEmpty(cartJson))
            {
                HttpContext.Session.SetString("ShoppingCart", cartJson);
            }

            return Ok(new { message = result.Message, role = user.Role });
        }

        // POST api/<UsersController>/logout
        [HttpPost("logout")]
        [SessionAuthorize(Roles = new[] { "Customer", "Admin" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }
            
            HttpContext.Session.Clear(); // Use if you want to remove all session data, including cart info
            //HttpContext.Session.Remove("UserId"); // use if you only want to remove your auth token

            // Invalidate session cookie
            Response.Cookies.Append("__Host-WebshopSession", "", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1), // Expire immediately
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok(new { message = "Logged out" });
        }

        // POST api/<UsersController>/forgot-password
        [HttpPost("forgot-password")]
        [SessionAuthorize(Roles = new[] { "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ForgotPasswordAsync(HttpContext, forgotPasswordDto);

            if (!result.Success && result.Error == ErrorCode.RateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, result.Message);
            }

            // Returns ok to not give user knowledge if the email exists
            if (!result.Success && result.Error == ErrorCode.NotFound)
            {
                return Ok(result.Message);
            }

            return Ok(result.Message);
        }

        // POST api/<UsersController>/reset-password
        [HttpPost("reset-password")]
        [SessionAuthorize(Roles = new[] { "Guest" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ResetPasswordAsync(HttpContext, resetPasswordDto);

            if (!result.Success && result.Error == ErrorCode.NotFound)
            {
                return Unauthorized(result.Message);
            }

            if (!result.Success && result.Error == ErrorCode.WeakPassword)
            {
                return BadRequest(result.Message);
            }

            return Ok("Password has been reset successfully.");
        }

        // POST api/<UsersController>/change-password
        [HttpPost("change-password")]
        [SessionAuthorize(Roles = new[] { "Customer" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }

            var result = await _userService.ChangePasswordAsync(userId.Value, changePasswordDto);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        [SessionAuthorize(Roles = new[] { "Admin" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == "Admin" || user.Role == "Guest")
            {
                return BadRequest("Cannot delete Admin or Guest accounts.");
            }

            await _userRepository.DeleteAsync(id);
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
