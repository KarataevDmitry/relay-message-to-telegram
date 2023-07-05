// See https://aka.ms/new-console-template for more information

using System.Text;



using TelegramNotifier;

using TL;

using WTelegram;
var currDir = Directory.GetCurrentDirectory();
if (args.Length == 0)
{
    var pathToAppUsageFile = Path.Combine(currDir, "APP_USAGE.txt");
    Console.WriteLine(File.ReadAllText(pathToAppUsageFile));
    return;
}
ConfigurationAccessor ca = new();
var tgSettingsConfSection = ca.Settings;

using var client = new Client(tgSettingsConfSection.API_ID, tgSettingsConfSection.API_HASH, tgSettingsConfSection.SessionPathName);
var pathToLogFile = Path.Combine(currDir, tgSettingsConfSection.LogFileName);
StreamWriter WTelegramLogs = new(pathToLogFile, true, Encoding.UTF8) { AutoFlush = true };
WTelegram.Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
await DoLogin(tgSettingsConfSection.AccountPhone);
var operationKind = args[0];
if (operationKind == "send-message-to-user")
{
    var targetUserName = args[1];
    var message = args[2];
    var result = await client.Contacts_ResolveUsername(targetUserName);
    await client.SendMessageAsync(result.User, message);
}
if (operationKind == "create-chat")
{
    var groupName = args[1];
    var invitedUserName = args[2];
    var result = await client.Contacts_ResolveUsername(invitedUserName);
    var data = await client.Messages_CreateChat(new InputUser[] { result.User }, groupName);
    Console.WriteLine($"Chat created with id={data.Chats.Keys.ElementAt(0)}");
}
if (operationKind == "delete-chat")
{
    var chat_id = long.Parse(args[1]);
    await client.DeleteChat(new InputPeerChat(chat_id));
    Console.WriteLine($"chat with id = {chat_id} is deleted");
}
if (operationKind == "invite-user")
{
    var chat_id = long.Parse(args[1]);
    var invitedUserName = args[2];
    var result = await client.Contacts_ResolveUsername(invitedUserName);
    await client.AddChatUser(new InputPeerChat(chat_id), result.User);
}
if (operationKind == "delete-user-from-chat")
{
    var chat_id = long.Parse(args[1]);
    var deletedUserName = args[2];
    var result = await client.Contacts_ResolveUsername(deletedUserName);
    await client.DeleteChatUser(new InputPeerChat(chat_id), result.User);
}
if (operationKind == "get-messages")
{
    var chat_id = long.Parse(args[1]);
    var history = await client.Messages_GetHistory(new InputPeerChat(chat_id));
    foreach (var m in history.Messages.OfType<Message>())
    {
        Console.WriteLine(m.message);
    }
}
if (operationKind == "send-message-to-chat")
{
    var chat_id = long.Parse(args[1]);
    var message = args[2];
    await client.SendMessageAsync(new InputPeerChat(chat_id), message);

}



async Task DoLogin(string loginInfo) // (add this method to your code)
{
    while (client.User == null)
    {
        switch (await client.Login(loginInfo)) // returns which config is needed to continue login
        {
            case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
            case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
            case "password": loginInfo = Console.ReadLine(); break;// if user has enabled 2FA
            default: loginInfo = null; break;
        }
    }

    Console.WriteLine($"We are logged-in as {client.User} (id {client.User.id})");
}