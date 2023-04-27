// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;

using System.Text;

using TL;

using WTelegram;
if (args.Length == 0)
{
    Console.WriteLine(File.ReadAllText("APP_USAGE.txt"));
    return;
}
IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var tgSettingsConfSection = configuration.GetRequiredSection(nameof(TelegramSettings)).Get<TelegramSettings>();
using var client = new Client(tgSettingsConfSection.API_ID, tgSettingsConfSection.API_HASH, tgSettingsConfSection.SessionPathName);
StreamWriter WTelegramLogs = new StreamWriter(tgSettingsConfSection.LogFileName, true, Encoding.UTF8) { AutoFlush = true };
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