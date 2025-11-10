using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Services.Otp
{
    public interface IOtpService
    {
        Task<string> GenerateAndReturnCodeAsync(string email, string purpose);
        bool Verify(string email, string purpose, string code);
        bool CanResend(string email, string purpose);
    }
}
