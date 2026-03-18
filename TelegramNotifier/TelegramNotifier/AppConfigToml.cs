namespace TelegramNotifier;

/// <summary>Root model for appsettings.toml (sections [TelegramSettings], [ApplicationSettings]).</summary>
public class AppConfigToml
{
    public TelegramSettings? TelegramSettings { get; set; }
    public ApplicationSettings? ApplicationSettings { get; set; }
}
