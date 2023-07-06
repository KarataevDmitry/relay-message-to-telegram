using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TL;

using WTelegram;

namespace TelegramNotifier
{
    public class TelegramManager
    {
        private const string USER_PRIVACY_RESTRICTED_MESSAGE = "USER_PRIVACY_RESTRICTED";
        private const string USER_NOT_MUTUAL_CONTACT = "USER_NOT_MUTUAL_CONTACT";
        readonly Client _telegramClient;
   
        public TelegramManager(Client client)
        {
                _telegramClient = client;
        }
        public async Task DoLogin(string loginInfo) // (add this method to your code)
        {
            while (_telegramClient.User == null)
            {
                switch (await _telegramClient.Login(loginInfo)) // returns which config is needed to continue login
                {
                    case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
                    case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
                    case "password": loginInfo = Console.ReadLine(); break;// if user has enabled 2FA
                    default: loginInfo = null; break;
                }
            }

            Console.WriteLine($"We are logged-in as {_telegramClient.User} (id {_telegramClient.User.id})");
        }
        public async Task CreateChat(string groupName, string invitedUserName)
        {
            var result = await _telegramClient.Contacts_ResolveUsername(invitedUserName);
            var data = await _telegramClient.Messages_CreateChat(new InputUser[] { result.User }, groupName);
            Console.WriteLine($"Chat created with id={data.Chats.Keys.ElementAt(0)}");
        }
        public async Task DeleteChat(long chat_id)
        {
            await _telegramClient.DeleteChat(new InputPeerChat(chat_id));
            Console.WriteLine($"chat with id = {chat_id} is deleted");
        }
        public async Task AddUserToChat(long chat_id, string invitedUserName)
        {
            var result = await _telegramClient.Contacts_ResolveUsername(invitedUserName);

            try
            {
                await _telegramClient.AddChatUser(new InputPeerChat(chat_id), result.User);
            }
            catch (RpcException e)
            {
                if (e.Message== USER_PRIVACY_RESTRICTED_MESSAGE)
                {
                    Console.WriteLine($"Addition for user @{invitedUserName} to chat {chat_id} is restricted by privacy settings of invited user");
                }
                if (e.Message==USER_NOT_MUTUAL_CONTACT)
                {
                    Console.WriteLine($"User @{invitedUserName} already left chat {chat_id} and I can't add it");
                }
            }
        }
        public async Task DeleteUserFromChat(long chat_id, string deletedUserName)
        {
            var result = await _telegramClient.Contacts_ResolveUsername(deletedUserName);
            await _telegramClient.DeleteChatUser(new InputPeerChat(chat_id), result.User);
        }
        public async Task SendMessageToChat(long chat_id, string message)
        {
            await _telegramClient.SendMessageAsync(new InputPeerChat(chat_id), message);
        }
       public async Task SendMessageToUser(string targetUserName, string message)
        {
            var result = await _telegramClient.Contacts_ResolveUsername(targetUserName);

            try
            {
                await _telegramClient.SendMessageAsync(result.User, message);
            }
            catch (RpcException e)
            {
                Console.WriteLine($"I have a trouble  {e.Message} when send message to @{targetUserName}");
                throw;
            }
        }
        public async Task<IEnumerable<Message>> GetMessagesFromChat(long chat_id)
        {
            var history = await _telegramClient.Messages_GetHistory(new InputPeerChat(chat_id));
            return history.Messages.OfType<Message>();
        }
    }
}
