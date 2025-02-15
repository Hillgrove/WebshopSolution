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

            try
            {
                var user = await _userService.RegisterUserAsync(userAuthDto);
                var userDto = new UserDto { Id = user.Id, Email = user.Email };
                return CreatedAtAction(nameof(Get), new { id = userDto.Id }, userDto);
            }

            catch (ArgumentException ex) when (ex.ParamName == nameof(userAuthDto.Email))
            {
                return BadRequest("Invalid email format.");
            }
            catch (ArgumentException ex) when (ex.ParamName == nameof(_passwordService.IsPasswordValidLength))
            {
                return BadRequest("Password is too short.");
            }
            catch (ArgumentException ex) when (ex.ParamName == nameof(_passwordService.IsPasswordStrong))
            {
                return BadRequest("Password is not strong enough.");
            }
            catch (ArgumentException ex) when (ex.ParamName == nameof(_passwordService.IsPasswordPwned))
            {
                return BadRequest("This password has been found in data breaches. Please choose another.");
            }
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

            try
            {
                await _userService.LoginAsync(HttpContext, userAuthDto);
                return Ok();
            }
            
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, "Too many login attempts. Please try again later.");
            }

            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You have entered an invalid username or password");
            }
        }

        // POST api/<UsersController>/logout
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // TODO: Complete method
            return Ok(new { message = "Logged out" });
        }


        // POST api/<UsersController>/forgot-password
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string? resetLink = "https://127.0.0.1:5500/#/reset-password";
                if (resetLink == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
                }

                await _userService.ForgotPasswordAsync(HttpContext, forgotPasswordDto, resetLink);
                return Ok("If this email exists in our system, you will receive a password reset email.");
            }

            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, "Too many login attempts. Please try again later.");
            }
        }

        // POST api/<UsersController>/reset-password
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.ResetPasswordAsync(HttpContext, resetPasswordDto);
                return Ok("Password has been reset successfully.");
            }

            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid or expired token.");
            }

            catch (ArgumentException ex) when (ex.ParamName == nameof(resetPasswordDto.NewPassword))
            {
                return BadRequest("Password does not meet the required criteria.");
            }



            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
