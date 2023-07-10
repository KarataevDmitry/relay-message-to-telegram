# relay-message-to-telegram
CLI Telegram Message Sender
# Usage

```./TelegramNotifier.exe <command> <data>```

where ```<command>``` is one of:

	 send-message-to-chat 
		description: send a message to chat with id
		parameters: chatId, message
		example of use: .\TelegramNotifier.exe send-message-to-chat 840666093 "This is programmatically sended message"
	  create-chat
		description: create a group with name <chatName> and invite <username> to this group
		parameters: chatName, username
		output: id of created chat
		example of use: .\TelegramNotifier.exe create-chat testChat krawler
	 delete-chat:
		description: delete chat with <chatId>
		parameters: chatId
		example of use: .\TelegramNotifier.exe delete-chat 840666093
	get-messages-from-chat
		description: get messages from chat with <chatId> (without service messages)
		parameters: chatId
		example of use: .\TelegramNotifier.exe get-messages 840666093
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
