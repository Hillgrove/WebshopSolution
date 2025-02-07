using Microsoft.AspNetCore.Mvc;
using Webshop.API.DTOs;
using Webshop.Data;
using Webshop.Data.Models;
using Webshop.Services;

namespace Webshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserService _userService;
        private readonly ValidationService _validationService;
        private readonly PwnedPasswordService _pwnedPasswordService;
        private readonly RateLimitingService _rateLimitingService;

        public UsersController(
            IUserRepository repository,
            UserService userService,
            ValidationService validationService,
            PwnedPasswordService pwnedPasswordService,
            RateLimitingService rateLimitingService)
        {
            _userRepository = repository;
            _userService = userService;
            _validationService = validationService;
            _pwnedPasswordService = pwnedPasswordService;
            _rateLimitingService = rateLimitingService;
        }

        // TODO: remove when done testing as it exposes all hashed passwords
        // GET: api/<UsersController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<User>> Get()
        {
            IEnumerable<User> users = _userRepository.GetAll();
            return Ok(users);
        }

        // POST api/<UsersController>/register
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserEmailDto>> Register([FromBody] UserCredentialsDto userRegistrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            userRegistrationDto.Email = userRegistrationDto.Email.Trim().ToLower();

            if (!_validationService.IsEmailValid(userRegistrationDto.Email))
            {
                return BadRequest("Invalid email format.");
            }

            if (!_validationService.IsPasswordValidLength(userRegistrationDto.Password))
            {
                return BadRequest("Password must be between 8 and 64 characters long");
            }

            if (!_validationService.IsPasswordStrong(userRegistrationDto.Password))
            {
                return BadRequest("Password not strong enough");
            }

            if (await _pwnedPasswordService.IsPasswordPwned(userRegistrationDto.Password))
            {
                return BadRequest("This password has been found in data breaches. Please choose another.");
            }

            try
            {
                var createdUser = _userService.CreateUser(userRegistrationDto.Email, userRegistrationDto.Password);
                var addedUser = _userRepository.Add(createdUser);
                var userResponse = new UserEmailDto { Email = addedUser.Email };

                return CreatedAtAction(nameof(Get), new { id = addedUser.Id }, userResponse);
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
        public ActionResult<UserEmailDto> Login([FromBody] UserCredentialsDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string deviceFingerprint = userLoginDto.VisitorId ?? "unknown";

            string rateLimitKey = $"{ipAddress}:{deviceFingerprint}";

            if (_rateLimitingService.IsRateLimited(rateLimitKey))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, "Too many login attempts. Please try again later.");
            }

            bool isValidUser = _userService.VerifyUserCredentials(userLoginDto.Email, userLoginDto.Password);
            if (!isValidUser)
            {
                _rateLimitingService.RegisterAttempt(rateLimitKey);
                return Unauthorized();
            }

            HttpContext.Session.SetString("UserEmail", userLoginDto.Email);

            _rateLimitingService.ResetAttempts(rateLimitKey);

            var user = _userRepository.GetUserByEmail(userLoginDto.Email);
            var userResponse = new UserEmailDto
            {
                Email = userLoginDto.Email
            };

            return Ok(userResponse);
        }

        // POST api/<UsersController>/logout
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult logout()
        {
            return Ok(new { message = "Logged out" });
        }


        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult ResetPassword([FromBody] UserEmailDto userEmailDto)
        {
            // TODO: complete method
            return Ok();
        }
    }
}
