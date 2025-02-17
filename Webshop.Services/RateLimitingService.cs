using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Webshop.Services
{
    public class RateLimitingService
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)>> _attempts = new();
        private readonly Dictionary<string, (int MaxAttempts, TimeSpan LockoutDuration)> _rateLimitConfigs;

        public RateLimitingService()
        {
            _rateLimitConfigs = new Dictionary<string, (int MaxAttempts, TimeSpan LockoutDuration)>
            {
                { "Login", (3, TimeSpan.FromMinutes(10)) },
                { "PasswordReset", (3, TimeSpan.FromMinutes(60)) }
            };
        }

        public static string GenerateRateLimitKey(HttpContext httpContext, string? deviceFingerprint)
        {
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string fingerprint = deviceFingerprint ?? "unknown";
            return $"{ipAddress}:{fingerprint}";
        }

        public bool IsRateLimited(string ratelimitKey, string actionKey)
        {
            if (!_rateLimitConfigs.TryGetValue(actionKey, out var config))
            {
                throw new ArgumentException($"Rate limiting configuration for action '{actionKey}' not found.");
            }

            if (_attempts.TryGetValue(actionKey, out var actionAttempts) &&
                    actionAttempts.TryGetValue(ratelimitKey, out var attemptInfo))
            {
                if (attemptInfo.Attempts >= config.MaxAttempts && DateTime.UtcNow - attemptInfo.LastAttempt < config.LockoutDuration)
                {
                    return true;
                }
            }

            return false;
        }

        public void RegisterAttempt(string ratelimitKey, string actionKey)
        {
            var actionAttempts = _attempts.GetOrAdd(actionKey, _ => new ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)>());
            actionAttempts.AddOrUpdate(
                ratelimitKey,
                addValueFactory: _ => (1, DateTime.UtcNow),
                updateValueFactory: (_, attemptInfo) => (attemptInfo.Attempts + 1, DateTime.UtcNow));
        }

        public void ResetAttempts(string ratelimitKey, string actionKey)
        {
            if (_attempts.TryGetValue(actionKey, out var actionAttempts))
            {
                actionAttempts.TryRemove(ratelimitKey, out _);
            }
        }
    }
}
 