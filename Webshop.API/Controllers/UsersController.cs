using Microsoft.AspNetCore.Mvc;
using Webshop.Data;
using Webshop.Data.Models;
using Webshop.Services;
using Webshop.Shared.DTOs;

namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserService _userService;
        private readonly ValidationService _validationService;
        private readonly RateLimitingService _rateLimitingService;

        public UsersController(
            IUserRepository repository,
            UserService userService,
            ValidationService validationService,
            RateLimitingService rateLimitingService)
        {
            _userRepository = repository;
            _userService = userService;
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

        // POST api/<UsersController>/register
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserEmailDto>> Register([FromBody] UserCredentialsDto userCredentialsDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userResponse = await _userService.RegiserUserAsync(userCredentialsDto);
                return CreatedAtAction(nameof(Get), new { id = userResponse.Id }, userResponse);
            }

            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/<UsersController>/login
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult> Login([FromBody] UserCredentialsDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                await _userService.LoginAsync(userLoginDto, ipAddress);
                return Ok();
            }

            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, ex.Message);
            }

            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        // POST api/<UsersController>/logout
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult logout()
        {
            // TODO: Complete method
            return Ok(new { message = "Logged out" });
        }


        // POST api/<UsersController>/forgot-password
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword([FromBody] UserEmailDto userEmailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string deviceFingerprint = userEmailDto.VisitorId ?? "unknown";
            string rateLimitKey = $"{ipAddress}:{deviceFingerprint}";

            if (_rateLimitingService.IsRateLimited(rateLimitKey, "PasswordReset"))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, "Too many requests. Please try again later.");
            }

            try
            {
                var user = await _userRepository.GetUserByEmailAsync(userEmailDto.Email);
                if (user != null)
                {
                    var token = await _userService.GeneratePasswordResetTokenAsync(user);
                    var resetLink = Url.Action("ResetPassword", "Users", new { token }, Request.Scheme);
                    await _userService.ForgotPasswordAsync(userEmailDto, ipAddress, deviceFingerprint, resetLink);
                }

                _rateLimitingService.RegisterAttempt(rateLimitKey, "PasswordReset");

                return Ok("If this email exists in our system, you will receive a password reset email.");
            }

            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, ex.Message);
            }
        }

        // POST api/<UsersController>/reset-password
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.ResetPasswordAsync(resetPasswordDto);
                return Ok("Password has been reset successfully.");
            }

            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
