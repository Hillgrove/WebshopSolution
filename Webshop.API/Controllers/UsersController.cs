using Microsoft.AspNetCore.Mvc;
using Webshop.Data;
using Webshop.Data.Models;
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
        private readonly RateLimitingService _rateLimitingService;

        public UsersController(
            UserService userService,
            EmailService emailService,
            IUserRepository repository,
            PasswordService passwordService,
            ValidationService validationService,
            RateLimitingService rateLimitingService)
        {
            _userService = userService;
            _emailService = emailService;
            _userRepository = repository;
            _passwordService = passwordService;
            _validationService = validationService;
            _rateLimitingService = rateLimitingService;
        }

        // TODO: remove when done testing as it exposes all hashed passwords
        // GET: api/<UsersController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            IEnumerable<User> users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> Get(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetCurrentUser()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not logged in.");
            }

            return Ok(new { email = userEmail });
        }

        // POST api/<UsersController>/register
        [HttpPost("register")]
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

            // Store user session
            HttpContext.Session.Clear(); // ASVS: 3.2.1 - Clear session to prevent session fixation
            HttpContext.Session.SetString("UserEmail", userAuthDto.Email);

            return Ok(new { message = result.Message });
        }

        // POST api/<UsersController>/logout
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not logged in.");
            }
            
            HttpContext.Session.Clear(); // Use if you want to remove all session data, including cart info
                                         //HttpContext.Session.Remove("UserEmail"); // use if you only want to remove your auth token

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not logged in.");
            }

            var result = await _userService.ChangePasswordAsync(userEmail, changePasswordDto);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }
}
