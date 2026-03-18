namespace TelegramNotifier;

/// <summary>
/// Holder for config and TelegramManager, set once after login before running any command.
/// </summary>
internal static class RunContext
{
    public static ConfigurationAccessor Config { get; set; } = null!;
    public static TelegramManager TelegramManager { get; set; } = null!;
}
