namespace SatisfactoryLogger;

public class AppSettings
{
    public FileOptions FileOptions { get; set; } = null!;
    public DiscordOptions DiscordOptions { get; set; } = null!;
}

public class FileOptions
{
    public string SatisfactoryLogDirectory { get; set; } = null!;
}

public class DiscordOptions
{
    public string WebhookURL { get; set; } = null!;
}
