using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace TelegramRelay.Mcp;

internal static class Handlers
{
    public static async Task<string> HandleAsync(string name, IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (name == "telegram_get_messages")
            return await GetMessagesAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_send_message")
            return await SendMessageAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_create_topic")
            return await CreateTopicAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_invite_user")
            return await InviteUserAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_remove_user")
            return await RemoveUserAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_send_message_to_user")
            return await SendMessageToUserAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_list_chats")
            return await ListChatsAsync(cancellationToken).ConfigureAwait(false);
        if (name == "telegram_edit_message")
            return await EditMessageAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_delete_message")
            return await DeleteMessageAsync(args, cancellationToken).ConfigureAwait(false);
        if (name == "telegram_update_old_messages")
            return await UpdateOldMessagesAsync(args, cancellationToken).ConfigureAwait(false);
        throw new ArgumentException($"Unknown tool: {name}");
    }

    private static async Task<string> GetMessagesAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        var chatId = chatIdEl.GetInt64();
        var list = new List<string> { "get-messages", chatId.ToString() };
        if (args.TryGetValue("topic_id", out var topicIdEl))
            list.Add(topicIdEl.GetInt32().ToString());

        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            return $"Relay exited with code {exitCode}. Stderr: {stderr}";

        var jsonLine = RelayRunner.ExtractJsonLine(stdout);
        if (string.IsNullOrEmpty(jsonLine))
            return "[]";
        return jsonLine;
    }

    private static async Task<string> SendMessageAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        if (!args.TryGetValue("message", out var messageEl))
            throw new ArgumentException("message is required");
        var chatId = chatIdEl.GetInt64();
        var message = messageEl.GetString() ?? "";
        var list = new List<string> { "send-message-to-chat", chatId.ToString(), message };
        if (args.TryGetValue("topic_id", out var topicIdEl))
            list.Add(topicIdEl.GetInt32().ToString());

        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            throw new InvalidOperationException($"Relay exited with code {exitCode}. Stderr: {stderr}");
        return "OK";
    }

    private static async Task<string> EditMessageAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        if (!args.TryGetValue("message_id", out var messageIdEl))
            throw new ArgumentException("message_id is required");
        if (!args.TryGetValue("message", out var messageEl))
            throw new ArgumentException("message is required");
        var list = new List<string>
        {
            "edit-message",
            chatIdEl.GetInt64().ToString(),
            messageIdEl.GetInt32().ToString(),
            messageEl.GetString() ?? ""
        };
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            throw new InvalidOperationException($"Relay exited with code {exitCode}. Stderr: {stderr}");
        return "OK";
    }

    private static async Task<string> DeleteMessageAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        if (!args.TryGetValue("message_id", out var messageIdEl))
            throw new ArgumentException("message_id is required");
        var list = new List<string>
        {
            "delete-message",
            chatIdEl.GetInt64().ToString(),
            messageIdEl.GetInt32().ToString()
        };
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            throw new InvalidOperationException($"Relay exited with code {exitCode}. Stderr: {stderr}");
        return "OK";
    }

    private static async Task<string> CreateTopicAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        if (!args.TryGetValue("title", out var titleEl))
            throw new ArgumentException("title is required");
        var list = new List<string> { "create-topic", chatIdEl.GetInt64().ToString(), titleEl.GetString() ?? "" };
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            return $"Relay exited with code {exitCode}. Stderr: {stderr}";
        return stdout.Trim();
    }

    private static async Task<string> InviteUserAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        if (!args.TryGetValue("username", out var usernameEl))
            throw new ArgumentException("username is required");
        var list = new List<string> { "invite-user", chatIdEl.GetInt64().ToString(), usernameEl.GetString() ?? "" };
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            throw new InvalidOperationException($"Relay exited with code {exitCode}. Stderr: {stderr}");
        return "OK";
    }

    private static async Task<string> RemoveUserAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        if (!args.TryGetValue("username", out var usernameEl))
            throw new ArgumentException("username is required");
        var list = new List<string> { "delete-user-from-chat", chatIdEl.GetInt64().ToString(), usernameEl.GetString() ?? "" };
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            throw new InvalidOperationException($"Relay exited with code {exitCode}. Stderr: {stderr}");
        return "OK";
    }

    private static async Task<string> SendMessageToUserAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("username", out var usernameEl))
            throw new ArgumentException("username is required");
        if (!args.TryGetValue("message", out var messageEl))
            throw new ArgumentException("message is required");
        var list = new List<string> { "send-message-to-user", usernameEl.GetString() ?? "", messageEl.GetString() ?? "" };
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            throw new InvalidOperationException($"Relay exited with code {exitCode}. Stderr: {stderr}");
        return "OK";
    }

    private static async Task<string> ListChatsAsync(CancellationToken cancellationToken)
    {
        var list = new List<string> { "list-chats" };
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            return $"Relay exited with code {exitCode}. Stderr: {stderr}";
        var line = RelayRunner.ExtractJsonLine(stdout);
        return string.IsNullOrEmpty(line) ? "[]" : line;
    }

    private static async Task<string> UpdateOldMessagesAsync(IReadOnlyDictionary<string, JsonElement> args, CancellationToken cancellationToken)
    {
        if (!args.TryGetValue("chat_id", out var chatIdEl))
            throw new ArgumentException("chat_id is required");
        var list = new List<string> { "update-old-messages", chatIdEl.GetInt64().ToString() };
        if (args.TryGetValue("topic_id", out var topicIdEl))
            list.Add(topicIdEl.GetInt32().ToString());
        var (stdout, stderr, exitCode) = await RelayRunner.RunAsync(list, cancellationToken).ConfigureAwait(false);
        if (exitCode != 0)
            return $"Relay exited with code {exitCode}. Stderr: {stderr}";
        return stdout.Trim();
    }
}
