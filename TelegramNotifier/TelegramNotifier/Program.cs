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
TelegramManager tm = new(client);
await tm.DoLogin(tgSettingsConfSection.AccountPhone);
var operationKind = args[0];
if (operationKind == "send-message-to-user")
{
    var targetUserName = args[1];
    var message = args[2];
    await tm.SendMessageToUser(targetUserName, message);
}
if (operationKind == "create-chat")
{
    var groupName = args[1];
    var invitedUserName = args[2];
    await tm.CreateChat(groupName, invitedUserName);
}
if (operationKind == "delete-chat")
{
    var chat_id = long.Parse(args[1]);
    await tm.DeleteChat(chat_id);
}
if (operationKind == "invite-user")
{
    var chat_id = long.Parse(args[1]);
    var invitedUserName = args[2];
    await tm.AddUserToChat(chat_id, invitedUserName);
}
if (operationKind == "delete-user-from-chat")
{
    var chat_id = long.Parse(args[1]);
    var usernameToDelete = args[2];
    await tm.DeleteUserFromChat(chat_id, usernameToDelete);
}
if (operationKind == "get-messages")
{
    var chat_id = long.Parse(args[1]);
   var messages =  await tm.GetMessagesFromChat(chat_id);
    messages.ToList().ForEach((x) => Console.WriteLine(x));
}
if (operationKind == "send-message-to-chat")
{
    var chat_id = long.Parse(args[1]);
    var message = args[2];
    await tm.SendMessageToChat(chat_id, message);

}






