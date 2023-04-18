// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var tgSettingsConfSection = configuration.GetRequiredSection("TelegramBotSettings").Get<TelegramBotSettings>();
var tgGroupId = new ChatId(tgSettingsConfSection.GroupId);
ITelegramBotClient tgBot = new TelegramBotClient(tgSettingsConfSection.BotKey);
await tgBot.SendTextMessageAsync(tgGroupId, args[0]);