using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using TL;
using TelegramNotifier;

namespace TelegramNotifier.Commands;

// --- send-message-to-user ---
internal sealed class SendMessageToUserCommand : AsyncCommand<SendMessageToUserCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<USERNAME>")]
        public string Username { get; set; } = "";

        [CommandArgument(1, "<MESSAGE>")]
        public string Message { get; set; } = "";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.SendMessageToUser(settings.Username, settings.Message).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Message sent.[/]");
        return 0;
    }
}

// --- create-chat ---
internal sealed class CreateChatCommand : AsyncCommand<CreateChatCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_NAME>")]
        public string ChatName { get; set; } = "";

        [CommandArgument(1, "<USERNAME>")]
        public string Username { get; set; } = "";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.CreateChat(settings.ChatName, settings.Username).ConfigureAwait(false);
        return 0;
    }
}

// --- delete-chat ---
internal sealed class DeleteChatCommand : AsyncCommand<DeleteChatCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.DeleteChat(settings.ChatId).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Chat deleted.[/]");
        return 0;
    }
}

// --- invite-user ---
internal sealed class InviteUserCommand : AsyncCommand<InviteUserCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "<USERNAME>")]
        public string Username { get; set; } = "";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.AddUserToChat(settings.ChatId, settings.Username).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]User invited.[/]");
        return 0;
    }
}

// --- delete-user-from-chat ---
internal sealed class DeleteUserFromChatCommand : AsyncCommand<DeleteUserFromChatCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "<USERNAME>")]
        public string Username { get; set; } = "";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.DeleteUserFromChat(settings.ChatId, settings.Username).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]User removed from chat.[/]");
        return 0;
    }
}

// --- list-chats (JSON to stdout for MCP) ---
internal sealed class ListChatsCommand : AsyncCommand<ListChatsCommand.Settings>
{
    public sealed class Settings : CommandSettings { }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var chats = await RunContext.TelegramManager.ListChatsAsync().ConfigureAwait(false);
        var arr = chats.Select(c => new { chat_id = c.ChatId, title = c.Title }).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(arr));
        return 0;
    }
}

// --- get-messages (JSON to stdout for MCP) ---
internal sealed class GetMessagesCommand : AsyncCommand<GetMessagesCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "[TOPIC_ID]")]
        public int? TopicId { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var dtos = await RunContext.TelegramManager.GetMessagesFromChatAsDtos(settings.ChatId, settings.TopicId).ConfigureAwait(false);
        var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = false });
        Console.Out.WriteLine(json);
        return 0;
    }
}

// --- get-media ---
internal sealed class GetMediaCommand : AsyncCommand<GetMediaCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var messages = await RunContext.TelegramManager.GetMessagesFromChat(settings.ChatId).ConfigureAwait(false);
        var withMedia = messages.Where(m => m.flags.HasFlag(Message.Flags.has_media)).ToList();
        var path = RunContext.Config.ApplicationSettings.SavedMediaLocationPath;
        foreach (var msg in withMedia)
            await RunContext.TelegramManager.DownloadMediaFromMessage(msg, path).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Downloaded [bold]{0}[/] media file(s).[/]", withMedia.Count);
        return 0;
    }
}

// --- create-topic ---
internal sealed class CreateTopicCommand : AsyncCommand<CreateTopicCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "<TITLE>")]
        public string Title { get; set; } = "";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var topicId = await RunContext.TelegramManager.CreateForumTopic(settings.ChatId, settings.Title).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Topic created.[/] Topic id (top_msg_id): [bold]{0}[/]", topicId);
        return 0;
    }
}

// --- send-message-to-chat ---
internal sealed class SendMessageToChatCommand : AsyncCommand<SendMessageToChatCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "<MESSAGE>")]
        public string Message { get; set; } = "";

        [CommandArgument(2, "[TOPIC_ID]")]
        public int? TopicId { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.SendMessageToChat(settings.ChatId, settings.Message, settings.TopicId).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Message sent.[/]");
        return 0;
    }
}

// --- edit-message ---
internal sealed class EditMessageCommand : AsyncCommand<EditMessageCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "<MESSAGE_ID>")]
        public int MessageId { get; set; }

        [CommandArgument(2, "<NEW_TEXT>")]
        public string NewText { get; set; } = "";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.EditMessage(settings.ChatId, settings.MessageId, settings.NewText).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Message edited.[/]");
        return 0;
    }
}

// --- delete-message ---
internal sealed class DeleteMessageCommand : AsyncCommand<DeleteMessageCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "<MESSAGE_ID>")]
        public int MessageId { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await RunContext.TelegramManager.DeleteMessage(settings.ChatId, settings.MessageId).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Message deleted.[/]");
        return 0;
    }
}

// --- update-old-messages ---
internal sealed class UpdateOldMessagesCommand : AsyncCommand<UpdateOldMessagesCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<CHAT_ID>")]
        public long ChatId { get; set; }

        [CommandArgument(1, "[TOPIC_ID]")]
        public int? TopicId { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        int edited = await RunContext.TelegramManager.UpdateOldMessagesInChat(settings.ChatId, settings.TopicId).ConfigureAwait(false);
        AnsiConsole.MarkupLine("[green]Updated {0} message(s).[/]", edited);
        return 0;
    }
}
