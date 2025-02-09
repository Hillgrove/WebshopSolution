//using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Cryptography;
//using System.Security.Policy;
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
        private readonly PwnedPasswordService _pwnedPasswordService;

        public UserService(
            EmailService emailService,
            HashingService hashingService, 
            IUserRepository userRepository, 
            ValidationService validationService,
            RateLimitingService rateLimitingService,
            PwnedPasswordService pwnedPasswordService
            )
        {
            _emailService = emailService;
            _hashingService = hashingService;
            _userRepository = userRepository;
            _validationService = validationService;
            _rateLimitingService = rateLimitingService;
            _pwnedPasswordService = pwnedPasswordService;            
        }

        public async Task<User> RegiserUserAsync(UserCredentialsDto userCredentialsDto)
        {
            userCredentialsDto.Email = userCredentialsDto.Email.Trim().ToLower();

            if (!_validationService.IsEmailValid(userCredentialsDto.Email))
            {
                throw new InvalidOperationException("Invalid email format.");
            }

            if (!_validationService.IsPasswordValidLength(userCredentialsDto.Password))
            {
                throw new InvalidOperationException("Password not strong enough.");
            }

            if (!_validationService.IsPasswordStrong(userCredentialsDto.Password))
            {
                throw new InvalidOperationException("Password not strong enough");
            }

            if (await _pwnedPasswordService.IsPasswordPwned(userCredentialsDto.Password))
            {
                throw new InvalidOperationException("This password has been found in data breaches. Please choose another.");
            }

            var createdUser = CreateUser(userCredentialsDto.Email, userCredentialsDto.Password);
            var addedUser = await _userRepository.AddAsync(createdUser);
            return addedUser;
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var hashedToken = _hashingService.ComputeSha256Hash(resetPasswordDto.Token);
            var user = await _userRepository.GetUserByPasswordResetTokenAsync(hashedToken);

            if (user == null || user.PasswordResetTokenExpiration < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Invalid or expired token.");
            }

            if (!_validationService.IsPasswordValidLength(resetPasswordDto.NewPassword) ||
                !_validationService.IsPasswordStrong(resetPasswordDto.NewPassword))
            {
                throw new InvalidOperationException("Password does not meet the required criteria.");
            }

            user.PasswordHash = _hashingService.GenerateHash(resetPasswordDto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            await _userRepository.UpdateAsync(user);

            if (user.Email != null)
            {
                await _emailService.SendPasswordChangedNotification(user.Email);
            }
        }

        public async Task ForgotPasswordAsync(UserEmailDto userEmailDto, string ipAddress, string deviceFingerPrint, string resetLink)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmailDto.Email);
            if (user != null)
            {
                // TODO: Need explanation of what this is doing
                await _userRepository.SavePasswordResetTokenAsync(user.Id, resetLink, DateTime.Now.AddMinutes(30));
                await _emailService.SendPasswordResetEmail(user.Email, resetLink);                
            }
        }

        public async Task LoginAsync(UserCredentialsDto userCredentialsDto, string ipAddress)
        {
            string deviceFingerprint = userCredentialsDto.VisitorId ?? "unknown";
            string rateLimitKey = $"{ipAddress}:{deviceFingerprint}";

            if (_rateLimitingService.IsRateLimited(rateLimitKey, "Login"))
            {
                throw new InvalidOperationException("Too many login attempts. Please try again later.");
            }

            bool isValidUser = await VerifyUserCredentialsAsync(userCredentialsDto.Email, userCredentialsDto.Password);
            if (!isValidUser)
            {
                _rateLimitingService.RegisterAttempt(rateLimitKey, "Login");
                throw new UnauthorizedAccessException();
            }

            _rateLimitingService.ResetAttempts(rateLimitKey, "Login");
        }

        public User CreateUser(string email, string password)
        {
            var passwordHash = _hashingService.GenerateHash(password);

            return new User
            {
                Email = email,
                PasswordHash = passwordHash
            };
        }

        public async Task<bool> VerifyUserCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || user.PasswordHash == null)
            {
                return false;
            }

            return _hashingService.VerifyHash(password, user.PasswordHash);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hashedToken = _hashingService.ComputeSha256Hash(token);

            await _userRepository.SavePasswordResetTokenAsync(user.Id, hashedToken, DateTime.UtcNow.AddMinutes(30));

            return token;
        }
    }
}
