// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;

using System.Text;

using TL;

using WTelegram;
var currDir = Directory.GetCurrentDirectory();
if (args.Length == 0)
{
    var pathToAppUsageFile = Path.Combine(currDir, "APP_USAGE.txt");
    Console.WriteLine(File.ReadAllText(pathToAppUsageFile));
    return;
}
var pathToConfigurationFile = Path.Combine(currDir, "appsettings.json");
IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(pathToConfigurationFile).Build();
var tgSettingsConfSection = configuration.GetRequiredSection(nameof(TelegramSettings)).Get<TelegramSettings>();
using var client = new Client(tgSettingsConfSection.API_ID, tgSettingsConfSection.API_HASH, tgSettingsConfSection.SessionPathName);
var pathToLogFile = Path.Combine(currDir, tgSettingsConfSection.LogFileName);
StreamWriter WTelegramLogs = new StreamWriter(pathToLogFile, true, Encoding.UTF8) { AutoFlush = true };
WTelegram.Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
await DoLogin(tgSettingsConfSection.AccountPhone);
if (args.Length == 2)
{
    var targetUserName = args[1];
    var targetMessage = args[0];
    var result = await client.Contacts_ResolveUsername(targetUserName);
    await client.SendMessageAsync(result.User, targetMessage);
}
async Task DoLogin(string loginInfo) // (add this method to your code)
{
    while (client.User == null)
        switch (await client.Login(loginInfo)) // returns which config is needed to continue login
        {
            case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
            case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
            case "password": loginInfo = Console.ReadLine(); break;// if user has enabled 2FA
            default: loginInfo = null; break;
        }
    Console.WriteLine($"We are logged-in as {client.User} (id {client.User.id})");
}