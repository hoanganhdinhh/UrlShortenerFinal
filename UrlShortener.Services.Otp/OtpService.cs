using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using UrlShortener.Services.Otp;

namespace UrlShortener.Services.Otp;

internal record OtpState(string Hash, DateTime ExpiresAt, int Attempts, DateTime LastSentUtc);

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly OtpOptions _opt;

    public OtpService(IMemoryCache cache, IOptions<OtpOptions> options)
    {
        _cache = cache;
        _opt = options.Value;
    }

    public async Task<string> GenerateAndReturnCodeAsync(string email, string purpose)
    {
        if (!CanResend(email, purpose))
            throw new InvalidOperationException("Please wait before requesting another code.");

        string otp = GenerateCode(_opt.CodeLength);
        string key = CacheKey(email, purpose);
        string hash = Hash($"{email}|{purpose}|{otp}");
        DateTime now = DateTime.UtcNow;
        var state = new OtpState(hash, now.AddMinutes(_opt.TtlMinutes), 0, now);

        _cache.Set(key, state, state.ExpiresAt);
        await Task.CompletedTask; // giữ async
        return otp; // trả code cho controller để gửi email
    }

    public bool Verify(string email, string purpose, string code)
    {
        string key = CacheKey(email, purpose);
        if (!_cache.TryGetValue(key, out OtpState? state) || state == null)
            return false;

        if (DateTime.UtcNow > state.ExpiresAt)
        {
            _cache.Remove(key);
            return false;
        }

        if (state.Attempts >= _opt.MaxAttempts)
        {
            _cache.Remove(key);
            return false;
        }

        bool ok = state.Hash == Hash($"{email}|{purpose}|{code}");
        var next = state with { Attempts = state.Attempts + 1 };

        if (ok)
            _cache.Remove(key);
        else
            _cache.Set(key, next, next.ExpiresAt);

        return ok;
    }

    public bool CanResend(string email, string purpose)
    {
        string key = CacheKey(email, purpose);
        if (!_cache.TryGetValue(key, out OtpState? state) || state == null)
            return true;

        return (DateTime.UtcNow - state.LastSentUtc).TotalSeconds >= _opt.ResendCooldownSeconds;
    }

    // ==== Helpers ====
    private static string CacheKey(string email, string purpose)
        => $"otp:{purpose}:{email.ToLowerInvariant()}";

    private static string GenerateCode(int length)
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        uint value = BitConverter.ToUInt32(bytes) % (uint)Math.Pow(10, length);
        return value.ToString(new string('0', length));
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }
}
