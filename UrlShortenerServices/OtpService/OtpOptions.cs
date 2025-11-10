namespace UrlShortener.Services.Otp;

public class OtpOptions
{
    public int CodeLength { get; set; } = 6;         // 6 số
    public int TtlMinutes { get; set; } = 10;        // Hết hạn 10 phút
    public int MaxAttempts { get; set; } = 5;        // Thử tối đa 5 lần
    public int ResendCooldownSeconds { get; set; } = 60; // Chờ 60s mới cho resend
}