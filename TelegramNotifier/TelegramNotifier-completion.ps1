# Tab-completion for TelegramNotifier.exe (PowerShell).
# Load once:  . .\TelegramNotifier-completion.ps1
# Or add to $PROFILE for permanent completion.

$script:TelegramNotifierCommands = @(
    'send-message-to-user',
    'create-chat',
    'delete-chat',
    'invite-user',
    'delete-user-from-chat',
    'list-chats',
    'get-messages',
    'get-media',
    'create-topic',
    'send-message-to-chat'
)

Register-ArgumentCompleter -Native -CommandName 'TelegramNotifier.exe' -ScriptBlock {
    param($wordToComplete, $commandAst, $cursorPosition)
    $script:TelegramNotifierCommands | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object { $_ }
}
