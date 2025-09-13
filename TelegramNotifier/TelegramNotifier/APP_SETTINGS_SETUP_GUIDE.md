# Detailed Guide for Configuring appsettings.json

## Overview

Your application uses **WTelegramClient** to work with Telegram API, which means it operates as a regular Telegram application, not as a bot. This requires obtaining API keys from Telegram for developers.

## appsettings.json Structure

Your `appsettings.json` file should contain the following settings:

```json
{
  "TelegramSettings": {
    "API_ID": "your_api_id",
    "API_HASH": "your_api_hash",
    "AccountPhone": "+7XXXXXXXXXX",
    "SessionPathName": "session.session",
    "LogFileName": "telegram.log"
  }
}
```

## Step-by-Step Guide to Get API Keys

### Step 1: Register on my.telegram.org

1. Open your browser and go to [https://my.telegram.org](https://my.telegram.org)
2. Log in to your Telegram account using your phone number and verification code
3. If you don't have a Telegram account, first register in the Telegram app

### Step 2: Create an Application

1. After logging into my.telegram.org, click on **"API development tools"**
2. Click **"Create new application"**
3. Fill out the form:
   - **App title**: Your application name (e.g., "TelegramNotifier")
   - **Short name**: Short name (e.g., "tnotifier")
   - **Platform**: Select "Desktop"
   - **Description**: Application description (e.g., "Telegram notifications")
   - **URL**: You can leave empty or specify your project URL
4. Click **"Create application"**

### Step 3: Get API Keys

After creating the application, you will receive:
- **API ID** - numeric identifier (e.g., 12345678)
- **API Hash** - 32-character string (e.g., "abcdef1234567890abcdef1234567890")

### Step 4: Configure appsettings.json

Replace the values in your `appsettings.json` file:

```json
{
  "TelegramSettings": {
    "API_ID": "12345678",
    "API_HASH": "abcdef1234567890abcdef1234567890",
    "AccountPhone": "+79123456789",
    "SessionPathName": "session.session",
    "LogFileName": "telegram.log"
  }
}
```

## Parameter Description

| Parameter | Description | Example |
|-----------|-------------|---------|
| `API_ID` | Your application ID from my.telegram.org | `12345678` |
| `API_HASH` | Your application hash from my.telegram.org | `abcdef1234567890abcdef1234567890` |
| `AccountPhone` | Telegram account phone number in international format | `+79123456789` |
| `SessionPathName` | Session file name for saving (created automatically) | `session.session` |
| `LogFileName` | Log file name | `telegram.log` |

## Important Notes

### Security
- **DO NOT SHARE** API_ID and API_HASH with others
- **DO NOT PUBLISH** this data publicly
- Add `appsettings.json` to `.gitignore` if you plan to use Git

### First Run
On the first application run:
1. You will receive a verification code in Telegram
2. Enter this code in the console
3. If two-factor authentication is enabled, enter the password
4. Session will be saved to `session.session` file

### Session File
- The `session.session` file contains authorization data
- **DO NOT DELETE** this file, otherwise you'll need to re-authorize
- This file is tied to a specific account and device

## Example of Complete appsettings.json

```json
{
  "TelegramSettings": {
    "API_ID": "12345678",
    "API_HASH": "abcdef1234567890abcdef1234567890",
    "AccountPhone": "+79123456789",
    "SessionPathName": "session.session",
    "LogFileName": "telegram.log"
  }
}
```

## Configuration Verification

After configuration, run the application without parameters to verify:

```bash
./TelegramNotifier.exe
```

If the settings are correct, you will see the application usage instructions.

## Troubleshooting

### Error "PHONE_NUMBER_INVALID"
- Check phone number format (should include country code, e.g., +7)
- Ensure the number is registered in Telegram

### Error "API_ID_INVALID" or "API_HASH_INVALID"
- Check the correctness of copied API_ID and API_HASH
- Ensure the application was created on my.telegram.org

### Error "SESSION_PASSWORD_NEEDED"
- Two-factor authentication is enabled
- Enter your Telegram account password

## Additional Information

- API keys are tied to your Telegram account
- One account can create multiple applications
- If you delete the application on my.telegram.org, API keys will become invalid
- For production, it's recommended to use a separate Telegram account

## Application Usage

After successful configuration, you can use the following commands:

```bash
# Send message to chat
./TelegramNotifier.exe send-message-to-chat 840666093 "This is a programmatically sent message"

# Create chat
./TelegramNotifier.exe create-chat testChat username

# Delete chat
./TelegramNotifier.exe delete-chat 840666093

# Get messages from chat
./TelegramNotifier.exe get-messages 840666093

# Delete user from chat
./TelegramNotifier.exe delete-user-from-chat 979115881 username

# Invite user to chat
./TelegramNotifier.exe invite-user 979115881 username

# Send message to user
./TelegramNotifier.exe send-message-to-user username "This is a programmatically sent message"
```

## Privacy Settings

If user privacy settings ban invites or messages, you'll be notified in the console with appropriate error messages.
