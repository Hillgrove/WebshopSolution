using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Webshop.Data;
using Webshop.Data.Models;
using Webshop.Shared.DTOs;

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

        public async Task<User> RegisterUserAsync(UserAuthDto userAuthDto)
        {
            userAuthDto.Email = userAuthDto.Email.Trim().ToLower();

            if (!_validationService.IsEmailValid(userAuthDto.Email))
            {
                throw new ArgumentException(nameof(userAuthDto.Email));
            }

            if (!_passwordService.IsPasswordValidLength(userAuthDto.Password))
            {
                throw new ArgumentException(nameof(_passwordService.IsPasswordValidLength));
            }

            if (!_passwordService.IsPasswordStrong(userAuthDto.Password))
            {
                throw new ArgumentException(nameof(_passwordService.IsPasswordStrong));
            }

            if (await _passwordService.IsPasswordPwned(userAuthDto.Password))
            {
                throw new ArgumentException(nameof(_passwordService.IsPasswordPwned));
            }

            var passwordHash = _hashingService.GenerateHash(userAuthDto.Password);
            var createdUser = new User
            {
                Email = userAuthDto.Email,
                PasswordHash = passwordHash
            };

            var addedUser = await _userRepository.AddAsync(createdUser);
            return addedUser;
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

        public async Task LoginAsync(HttpContext httpContext, UserAuthDto userAuthDto)
        {
            string ratelimitKey = RateLimitingService.GenerateRateLimitKey(httpContext, userAuthDto.VisitorId);

            if (_rateLimitingService.IsRateLimited(ratelimitKey, _loginkey))
            {
                throw new HttpRequestException(null, null, System.Net.HttpStatusCode.TooManyRequests);
            }

            bool isValidUser = await VerifyUserCredentialsAsync(userAuthDto.Email, userAuthDto.Password);
            if (!isValidUser)
            {
                _rateLimitingService.RegisterAttempt(ratelimitKey, _loginkey);
                throw new UnauthorizedAccessException();
            }

            _rateLimitingService.ResetAttempts(ratelimitKey, _loginkey);
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
