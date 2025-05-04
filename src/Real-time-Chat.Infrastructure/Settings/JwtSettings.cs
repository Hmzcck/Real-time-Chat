﻿namespace Real_time_Chat.Infrastructure.Settings;

public sealed class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpirationInMinutes { get; set; }
}