using System.Security.Cryptography;
using System.Text;

namespace Webshop.Services
{
    public class PwnedPasswordService
    {
        private readonly HttpClient _httpClient;
        private const string HIBP_API = "https://api.pwnedpasswords.com/range/";

        public PwnedPasswordService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> IsPasswordPwned(string password)
        {
            string sha1Hash = ComputeSha1Hash(password);
            string prefix = sha1Hash[..5];
            string suffix = sha1Hash[5..];

            var response = await _httpClient.GetStringAsync(HIBP_API + prefix);
            return response.Split("\n").Any(line => line.StartsWith(suffix));
        }

        private static string ComputeSha1Hash(string password)
        {
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }
    }
}
