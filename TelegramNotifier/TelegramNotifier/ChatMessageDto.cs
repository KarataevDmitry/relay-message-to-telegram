namespace TelegramNotifier;

/// <summary>
/// DTO for JSON output of get-messages (MCP-friendly).
/// </summary>
public sealed class ChatMessageDto
{
    public int Id { get; set; }
    public int Date { get; set; }
    public long? FromUserId { get; set; }
    public string? FromUserName { get; set; }
    public string Text { get; set; } = "";
    public int? ReplyToMsgId { get; set; }
    public int? TopicId { get; set; }
}
