using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Webshop.Data;
using Webshop.Data.Models;
using Webshop.Shared.DTOs;
using Webshop.Shared.Enums;

namespace Webshop.Services
{
    public class UserService
    {
        private readonly EmailService _emailService;
        private readonly HashingService _hashingService;
        private readonly IUserRepository _userRepository;
        private readonly ValidationService _validationService;      
        private readonly RateLimitingService _rateLimitingService;
        private readonly PasswordService _passwordService;

        private readonly string _passwordResetkey = "PasswordReset";
        private readonly string _loginkey = "Login";

        public UserService(
            EmailService emailService,
            HashingService hashingService, 
            IUserRepository userRepository, 
            PasswordService passwordService,
            ValidationService validationService,
            RateLimitingService rateLimitingService
            )
        {
            _emailService = emailService;
            _hashingService = hashingService;
            _userRepository = userRepository;
            _passwordService = passwordService;            
            _validationService = validationService;
            _rateLimitingService = rateLimitingService;
        }

        public async Task<RegistrationResultDto> RegisterUserAsync(UserAuthDto userAuthDto)
        {
            userAuthDto.Email = userAuthDto.Email.Trim().ToLower();

            if (await _userRepository.GetUserByEmailAsync(userAuthDto.Email) != null)
            {
                return new RegistrationResultDto { Success = false, Message = "Email is already registered" };
            }

            if (!_validationService.IsEmailValid(userAuthDto.Email))
            {
                return new RegistrationResultDto { Success = false, Message = "Invalid email format." };
            }

            if (!_passwordService.IsPasswordValidLength(userAuthDto.Password))
            {
                return new RegistrationResultDto { Success = false, Message = "Password needs to be between 8 and 64 characters long" };
            }

            if (!_passwordService.IsPasswordStrong(userAuthDto.Password))
            {
                return new RegistrationResultDto { Success = false, Message = "Password is not strong enough" };
            }

            if (await _passwordService.IsPasswordPwned(userAuthDto.Password))
            {
                return new RegistrationResultDto { Success = false, Message = "This password has been found in data breaches. Please choose another." };
            }

            var passwordHash = _hashingService.GenerateHash(userAuthDto.Password);
            var createdUser = new User
            {
                Email = userAuthDto.Email,
                PasswordHash = passwordHash
            };

            var addedUser = await _userRepository.AddAsync(createdUser);
            return new RegistrationResultDto { Success = true, Message = "User created.", User = addedUser };
        }

        public async Task ResetPasswordAsync(HttpContext httpContext, ResetPasswordDto resetPasswordDto)
        {
            string rateLimitKey = RateLimitingService.GenerateRateLimitKey(httpContext, resetPasswordDto.VisitorId);

            var hashedToken = _hashingService.ComputeSha256Hash(resetPasswordDto.Token);
            var user = await _userRepository.GetUserByPasswordResetTokenAsync(hashedToken);

            if (user == null || user.PasswordResetTokenExpiration < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException();
            }

            if (!_passwordService.IsPasswordValidLength(resetPasswordDto.NewPassword) ||
                !_passwordService.IsPasswordStrong(resetPasswordDto.NewPassword))
            {
                throw new ArgumentException(nameof(resetPasswordDto.NewPassword));
            }

            user.PasswordHash = _hashingService.GenerateHash(resetPasswordDto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            await _userRepository.UpdateAsync(user);

            _rateLimitingService.ResetAttempts(rateLimitKey, _passwordResetkey);

            if (user.Email != null)
            {
                await _emailService.SendPasswordChangedNotification(user.Email);
            }
        }

        public async Task<LoginResultDto> LoginAsync(HttpContext httpContext, UserAuthDto userAuthDto)
        {
            string ratelimitKey = RateLimitingService.GenerateRateLimitKey(httpContext, userAuthDto.VisitorId);

            if (_rateLimitingService.IsRateLimited(ratelimitKey, _loginkey))
            {
                return new LoginResultDto { Success = false, Error = LoginErrorCode.RateLimited, Message = "Too many login attempts. Please try again later." };
            }

            bool isValidUser = await VerifyUserCredentialsAsync(userAuthDto.Email, userAuthDto.Password);
            if (!isValidUser)
            {
                _rateLimitingService.RegisterAttempt(ratelimitKey, _loginkey);
                return new LoginResultDto { Success = false, Error = LoginErrorCode.WrongCredentials, Message = "You have entered an invalid username or password" };
            }

            _rateLimitingService.ResetAttempts(ratelimitKey, _loginkey);
            return new LoginResultDto { Success = true, Message = "Login successful" };
        }

        public async Task ForgotPasswordAsync(HttpContext httpContext, ForgotPasswordDto forgotPasswordDto, string resetLink)
        {
            string rateLimitKey = RateLimitingService.GenerateRateLimitKey(httpContext, forgotPasswordDto.VisitorId);
            if (_rateLimitingService.IsRateLimited(rateLimitKey, _passwordResetkey))
            {
                throw new HttpRequestException(null, null, System.Net.HttpStatusCode.TooManyRequests);
            }

            var user = await _userRepository.GetUserByEmailAsync(forgotPasswordDto.Email);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var token = await GenerateAndSavePasswordResetTokenAsync(user);
                await _emailService.SendPasswordResetEmail(user.Email, $"{resetLink}?token={token}");
                _rateLimitingService.RegisterAttempt(rateLimitKey, _passwordResetkey);
            }
        }

        private async Task<bool> VerifyUserCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || user.PasswordHash == null)
            {
                return false;
            }

            return _hashingService.VerifyHash(password, user.PasswordHash);
        }

        private async Task<string> GenerateAndSavePasswordResetTokenAsync(User user)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedToken = _hashingService.ComputeSha256Hash(token);

            await _userRepository.SavePasswordResetTokenAsync(user.Id, hashedToken, DateTime.UtcNow.AddMinutes(30));

            return token;
        }
    }
}
