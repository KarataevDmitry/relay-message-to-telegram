using Microsoft.Extensions.Configuration;
using Tomlyn;

namespace TelegramNotifier;

public class ConfigurationAccessor
{
    public TelegramSettings Settings { get; set; }
    public ApplicationSettings ApplicationSettings { get; set; }

    public ConfigurationAccessor()
    {
        var currDir = Directory.GetCurrentDirectory();
        var pathToml = Path.Combine(currDir, "appsettings.toml");
        var pathJson = Path.Combine(currDir, "appsettings.json");

        if (File.Exists(pathToml))
        {
            var tomlText = File.ReadAllText(pathToml);
            var model = TomlSerializer.Deserialize<AppConfigToml>(tomlText) ?? new AppConfigToml();
            Settings = model.TelegramSettings ?? new TelegramSettings();
            ApplicationSettings = model.ApplicationSettings ?? new ApplicationSettings();
        }
        else if (File.Exists(pathJson))
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(pathJson, optional: true)
                .Build();
            Settings = configuration.GetSection(nameof(TelegramSettings)).Get<TelegramSettings>() ?? new TelegramSettings();
            ApplicationSettings = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>() ?? new ApplicationSettings();
        }
        else
        {
            Settings = new TelegramSettings();
            ApplicationSettings = new ApplicationSettings();
        }

        ApplyDefaults();
    }

    void ApplyDefaults()
    {
        if (string.IsNullOrWhiteSpace(Settings.SessionPathName) || Settings.SessionPathName.Contains('<'))
            Settings.SessionPathName = "WTelegram.session";
        if (string.IsNullOrWhiteSpace(Settings.LogFileName) || Settings.LogFileName.Contains('<'))
            Settings.LogFileName = "wtelegram.log";
        if (string.IsNullOrWhiteSpace(ApplicationSettings.SavedMediaLocationPath) || ApplicationSettings.SavedMediaLocationPath.Contains('<'))
            ApplicationSettings.SavedMediaLocationPath = "SavedMedia";
    }
}
