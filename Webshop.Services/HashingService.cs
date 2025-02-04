using Isopoh.Cryptography.Argon2;

namespace Webshop.Services
{
    public class HashingService
    {
        public string GenerateHash(string password)
        {
            return Argon2.Hash(password);
        }

        public bool VerifyHash(string password, string hash)
        {
            return Argon2.Verify(hash, password);
        }
    }
}
