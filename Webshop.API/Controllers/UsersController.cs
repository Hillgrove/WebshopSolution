﻿using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<UserDto>> Get(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserDto userDto = new UserDto { Id = user.Id, Email = user.Email };
            return Ok(userDto);
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

            var result = await _userService.LoginAsync(HttpContext, userAuthDto);

            if (!result.Success && result.Error == ErrorCode.RateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, result.Message);
            }

            if (!result.Success && result.Error == ErrorCode.WrongCredentials)
            {
                return Unauthorized(result.Message);
            }
            
            return Ok(result.Message);
        }

        // POST api/<UsersController>/logout
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Logout()
        {
            // TODO: implement Logout endpoint
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

        // TODO: implement ChangePassword endpoint
        // POST api/<UsersController>/change-password
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.ChangePasswordAsync(changePasswordDto);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }
}
