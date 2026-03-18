# relay-message-to-telegram
CLI Telegram Message Sender
# Usage

```./TelegramNotifier.exe <command> <parameters>```

where ```<command>``` is one of:

	 send-message-to-chat 
		description: send a message to chat (optional topicId for forum groups)
		parameters: chatId, message [, topicId]
		example: .\TelegramNotifier.exe send-message-to-chat 840666093 "Hello"
	  create-chat
		description: create a group with name <chatName> and invite <username> to this group
		parameters: chatName, username
		output: id of created chat
		example of use: .\TelegramNotifier.exe create-chat testChat krawler
	 delete-chat:
		description: delete chat with <chatId>
		parameters: chatId
		example of use: .\TelegramNotifier.exe delete-chat 840666093
	get-messages
		description: get messages from chat (output: JSON). Optional topicId for forum groups.
		parameters: chatId [, topicId]
		example: .\TelegramNotifier.exe get-messages 840666093
		example with topic: .\TelegramNotifier.exe get-messages -1001234567890 42
	create-topic
		description: create a forum topic in a supergroup (forum enabled; manage_topics required).
		parameters: chatId, title
		example: .\TelegramNotifier.exe create-topic -1001234567890 "New topic"
	delete-user-from-chat
		description: delete a user with <username> from chat with <chatId>
		parameters: chatId username
		example of use: .\TelegramNotifier.exe delete-user-from-chat 979115881 krawler
	invite-user
		description: invites a user with <username> to chat with <chatId>
		parameters: chatId username
		example of use: .\TelegramNotifier.exe invite-user 979115881 answeroom
	send-message-to-user
		description: send a <message> to user with <username>
		parameters: username, message
		example of use: \TelegramNotifier.exe send-message-to-user krawler "This is programmatically sended message"

if user privacy settings ban invites or messages you'll be noticed on console 

username provided without @ character

first launch of app request user login, next - uses .session file
if u want to use another user you should remove *.session

**Topics (forum groups):** For supergroups with topics, use optional `topicId` (the topic's top message id) in `get-messages` and `send-message-to-chat`. `get-messages` outputs a JSON array (Id, Date, FromUserId, Text, ReplyToMsgId, TopicId) for MCP/scripting.

**Tab completion (PowerShell):** In the folder with `TelegramNotifier.exe`, run once per session: `. .\TelegramNotifier-completion.ps1` (script is in repo `TelegramNotifier\TelegramNotifier-completion.ps1`). Then `.\TelegramNotifier.exe <TAB>` will list commands. To enable permanently, add that line to your PowerShell profile (`$PROFILE`).

## Config

Конфиг: **appsettings.toml** (скопируй `appsettings.toml.example` → `appsettings.toml` и заполни API_ID, API_HASH, AccountPhone) или по‑старому **appsettings.json**. Оба файла в `.gitignore`.

## MCP server (TelegramRelay.Mcp)

Model Context Protocol server that exposes CLI as tools for AI agents:

- **telegram_get_messages** — `chat_id` (required), `topic_id` (optional). Returns JSON array of messages.
- **telegram_send_message** — `chat_id`, `message` (required), `topic_id` (optional).

Build: from repo root run `dotnet build TelegramNotifier\TelegramNotifier.sln`. Publish MCP: `dotnet publish TelegramRelay.Mcp\TelegramRelay.Mcp.csproj -c Release`. The server expects **TelegramNotifier.exe** either next to the MCP executable or its path in **TELEGRAM_RELAY_EXE**. When publishing, copy `TelegramNotifier.exe` (and its config/session) next to `TelegramRelay.Mcp.exe`, or set `TELEGRAM_RELAY_EXE` in the environment.
