﻿namespace TaskManagerAPI.Infrastructure.Options;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
}