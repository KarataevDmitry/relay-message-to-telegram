using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;
using TelegramNotifier;
using TelegramNotifier.Commands;
using WTelegram;

var currDir = Directory.GetCurrentDirectory();

if (args.Length == 0)
{
    var usagePath = Path.Combine(currDir, "APP_USAGE.txt");
    if (File.Exists(usagePath))
    {
        var usage = File.ReadAllText(usagePath);
        Console.WriteLine(usage);
    }
    else
    {
        var app = new CommandApp();
        ConfigureCommands(app);
        return await app.RunAsync(["--help"]).ConfigureAwait(false);
    }
    return 0;
}

var ca = new ConfigurationAccessor();
var tgSettings = ca.Settings;
using var client = new Client(tgSettings.API_ID, tgSettings.API_HASH, tgSettings.SessionPathName);
var logPath = Path.Combine(currDir, tgSettings.LogFileName);
await using (var logStream = new StreamWriter(logPath, true, Encoding.UTF8) { AutoFlush = true })
{
    WTelegram.Helpers.Log = (lvl, str) => logStream.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
    var tm = new TelegramManager(client);
    await tm.DoLogin(tgSettings.AccountPhone).ConfigureAwait(false);
    RunContext.Config = ca;
    RunContext.TelegramManager = tm;

    var commandApp = new CommandApp();
    ConfigureCommands(commandApp);
    var result = await commandApp.RunAsync(args).ConfigureAwait(false);
    WTelegram.Helpers.Log = (_, _) => { }; // client.Dispose() may log; stream is about to close
    return result;
}

static void ConfigureCommands(ICommandApp app)
{
    app.Configure(config =>
    {
        config.AddCommand<SendMessageToUserCommand>("send-message-to-user");
        config.AddCommand<CreateChatCommand>("create-chat");
        config.AddCommand<DeleteChatCommand>("delete-chat");
        config.AddCommand<InviteUserCommand>("invite-user");
        config.AddCommand<DeleteUserFromChatCommand>("delete-user-from-chat");
        config.AddCommand<ListChatsCommand>("list-chats");
        config.AddCommand<GetMessagesCommand>("get-messages");
        config.AddCommand<GetMediaCommand>("get-media");
        config.AddCommand<CreateTopicCommand>("create-topic");
        config.AddCommand<SendMessageToChatCommand>("send-message-to-chat");
        config.AddCommand<EditMessageCommand>("edit-message");
        config.AddCommand<DeleteMessageCommand>("delete-message");
        config.AddCommand<UpdateOldMessagesCommand>("update-old-messages");
    });
}
