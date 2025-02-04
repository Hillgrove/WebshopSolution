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
        private readonly UserRepositoryList _userRepository;
        private readonly UserService _userService;

        public UsersController(UserRepositoryList repository, UserService userService)
        {
            _userRepository = repository;
            _userService = userService;
        }

        // GET: api/<UsersController>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<User>> Get()
        {
            IEnumerable<User> users = _userRepository.GetAll();
            return Ok(users);
        }

        // POST api/<UsersController>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserResponseDto> Register([FromBody] UserRegistrationDto userRegistrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdUser = _userService.CreateUser(userRegistrationDto.Email, userRegistrationDto.Password);
            var addedUser = _userRepository.Add(createdUser);
            var userResponse = new UserResponseDto
            {
                Email = addedUser.Email
            };

            return CreatedAtAction(nameof(Get), new { id = addedUser.Id }, userResponse);
        }
    }
}
