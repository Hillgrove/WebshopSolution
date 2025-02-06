using System.Collections.Concurrent;

namespace Webshop.Services
{
    public class RateLimitingService
    {
        private readonly ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)> _loginAttempts = new();
        private readonly int _maxAttempts;
        private readonly TimeSpan _lockoutDuration;

        public RateLimitingService(int maxAttempts, TimeSpan lockoutDuration)
        {
            _maxAttempts = maxAttempts;
            _lockoutDuration = lockoutDuration;
        }

        public bool IsRateLimited(string key)
        {
            if (_loginAttempts.TryGetValue(key, out var attemptInfo))
            {
                if (attemptInfo.Attempts >= _maxAttempts && DateTime.UtcNow - attemptInfo.LastAttempt < _lockoutDuration)
                {
                    return true;
                }
            }

            return false;
        }

        public void RegisterAttempt(string key)
        {
            _loginAttempts.AddOrUpdate(
                key,
                addValueFactory: _ => (1, DateTime.UtcNow),
                updateValueFactory: (_, attemptInfo) => (attemptInfo.Attempts + 1, DateTime.UtcNow));
        }

        public void ResetAttempts(string key)
        {
            _loginAttempts.TryRemove(key, out _);
        }
    }
}
