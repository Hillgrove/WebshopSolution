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

        public UsersController(IUserRepository repository, UserService userService, ValidationService validationService, PwnedPasswordService pwnedPasswordService)
        {
            _userRepository = repository;
            _userService = userService;
            _validationService = validationService;
            _pwnedPasswordService = pwnedPasswordService;
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
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserCredentialsDto userRegistrationDto)
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

            if (await _pwnedPasswordService.IsPasswordPwned(userRegistrationDto.Password))
            {
                return BadRequest("This password has been found in data breaches. Please choose another.");
            }

            var createdUser = _userService.CreateUser(userRegistrationDto.Email, userRegistrationDto.Password);
            var addedUser = _userRepository.Add(createdUser);
            var userResponse = new UserResponseDto
            {
                Email = addedUser.Email
            };

            return CreatedAtAction(nameof(Get), new { id = addedUser.Id }, userResponse);
        }

        // POST api/<UsersController>/login
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<UserResponseDto> Login([FromBody] UserCredentialsDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool isValidUser = _userService.VerifyUserCredentials(userLoginDto.Email, userLoginDto.Password);
            if (!isValidUser)
            {
                return Unauthorized();
            }

            var user = _userRepository.GetAll().FirstOrDefault(u => u.Email == userLoginDto.Email);
            var userResponse = new UserResponseDto
            {
                Email = userLoginDto.Email
            };

            return Ok(userResponse);
        }
    }
}
