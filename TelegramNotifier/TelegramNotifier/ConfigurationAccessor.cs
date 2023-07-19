using Microsoft.Extensions.Configuration;

using System.Configuration;

namespace TelegramNotifier
{
    public class ConfigurationAccessor
    {
        public TelegramSettings Settings { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public ConfigurationAccessor()
        {
            var currDir = Directory.GetCurrentDirectory();
            var pathToConfigurationFile = Path.Combine(currDir, "appsettings.json");
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(pathToConfigurationFile).Build();
            var tgSettingsConfSection = configuration?.GetRequiredSection(nameof(TelegramSettings))?.Get<TelegramSettings>();
            var appSettings = configuration?.GetRequiredSection(nameof(ApplicationSettings))?.Get<ApplicationSettings>();
            Settings = tgSettingsConfSection;
            ApplicationSettings = appSettings;
        }
    }
}
