using System.Text.Json;
using ModelContextProtocol.Protocol;
using Tool = ModelContextProtocol.Protocol.Tool;

namespace TelegramRelay.Mcp;

internal static class ToolCatalog
{
    private static JsonElement Schema(object schema) => JsonSerializer.SerializeToElement(schema);

    internal static List<Tool> Build() =>
    [
        new Tool
        {
            Name = "telegram_get_messages",
            Description = "Get messages from a Telegram chat. Returns JSON array (Id, Date, FromUserId, Text, ReplyToMsgId, TopicId). For forum groups use topic_id to filter by topic.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Chat ID (e.g. -1001234567890 for supergroup)." },
                    topic_id = new { type = "integer", description = "Optional: topic (thread) ID to filter messages in a forum group." }
                },
                required = new[] { "chat_id" }
            })
        },
        new Tool
        {
            Name = "telegram_send_message",
            Description = "Send a message to a Telegram chat. Optional topic_id for forum groups.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Chat ID." },
                    message = new { type = "string", description = "Message text to send." },
                    topic_id = new { type = "integer", description = "Optional: topic ID for forum groups." }
                },
                required = new[] { "chat_id", "message" }
            })
        },
        new Tool
        {
            Name = "telegram_create_topic",
            Description = "Create a forum topic in a supergroup (forum must be enabled; requires manage_topics). Returns the new topic ID.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Supergroup chat ID (e.g. -1001234567890)." },
                    title = new { type = "string", description = "Topic title." }
                },
                required = new[] { "chat_id", "title" }
            })
        },
        new Tool
        {
            Name = "telegram_invite_user",
            Description = "Invite a user by username to a chat (group/supergroup).",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Chat ID." },
                    username = new { type = "string", description = "Telegram username (without @)." }
                },
                required = new[] { "chat_id", "username" }
            })
        },
        new Tool
        {
            Name = "telegram_remove_user",
            Description = "Remove a user by username from a chat (group/supergroup).",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Chat ID." },
                    username = new { type = "string", description = "Telegram username (without @)." }
                },
                required = new[] { "chat_id", "username" }
            })
        },
        new Tool
        {
            Name = "telegram_send_message_to_user",
            Description = "Send a direct message to a user by their Telegram username.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    username = new { type = "string", description = "Telegram username (without @)." },
                    message = new { type = "string", description = "Message text." }
                },
                required = new[] { "username", "message" }
            })
        },
        new Tool
        {
            Name = "telegram_list_chats",
            Description = "List all chats (groups, supergroups) the account is in. Returns JSON array of { chat_id, title }. Use to find chat_id by group name (e.g. AI Guiders).",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new { },
                required = Array.Empty<string>()
            })
        },
        new Tool
        {
            Name = "telegram_edit_message",
            Description = "Edit an existing message in a Telegram chat. Only messages sent by this account can be edited. message_id is the Id from get_messages.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Chat ID." },
                    message_id = new { type = "integer", description = "ID of the message to edit (from telegram_get_messages)." },
                    message = new { type = "string", description = "New message text." }
                },
                required = new[] { "chat_id", "message_id", "message" }
            })
        },
        new Tool
        {
            Name = "telegram_delete_message",
            Description = "Delete a message in a Telegram chat. Only messages sent by this account (or with admin rights in channels). message_id from get_messages (Id field).",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Chat ID." },
                    message_id = new { type = "integer", description = "ID of the message to delete (from telegram_get_messages)." }
                },
                required = new[] { "chat_id", "message_id" }
            })
        },
        new Tool
        {
            Name = "telegram_update_old_messages",
            Description = "Re-format old messages in a chat: fetches the last page of history, edits each message sent by this account with the same text so it gets MD→HTML formatting (bold, code, etc.). Optional topic_id for forum groups.",
            InputSchema = Schema(new
            {
                type = "object",
                properties = new
                {
                    chat_id = new { type = "integer", description = "Chat ID." },
                    topic_id = new { type = "integer", description = "Optional: topic ID to limit to one thread in a forum group." }
                },
                required = new[] { "chat_id" }
            })
        }
    ];
}
