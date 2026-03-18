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
        private const long SUPERGROUP_CHAT_ID_THRESHOLD = -1000000000000L;

        readonly Client _telegramClient;
        private Dictionary<long, InputPeer>? _peerCache;
        private Dictionary<long, InputChannel>? _channelCache;

        public TelegramManager(Client client)
        {
            _telegramClient = client;
        }

        /// <summary>
        /// Resolves InputPeer for the given chat_id. Basic groups use InputPeerChat;
        /// supergroups (forum-capable) use InputPeerChannel from dialogs.
        /// </summary>
        private async Task<InputPeer> GetInputPeer(long chatId)
        {
            // Basic group: small positive id (e.g. 840666093). Supergroups from list-chats: 1e9..1e12
            if (chatId > 0 && chatId < 1000000000L)
                return new InputPeerChat(chatId);

            if (_peerCache != null && _peerCache.TryGetValue(chatId, out var cached))
                return cached;

            var dialogs = await _telegramClient.Messages_GetAllDialogs();
            // Supergroup: from list-chats we use positive 1000000000000 - ch.id; standard is negative -1000000000000 - ch.id
            long channelId = chatId > 0 ? 1000000000000L - chatId : -SUPERGROUP_CHAT_ID_THRESHOLD - chatId;
            if (dialogs is Messages_DialogsSlice slice)
            {
                foreach (var (id, chat) in slice.chats)
                {
                    if (chat is Channel ch && ch.id == channelId)
                    {
                        var peer = new InputPeerChannel(channelId, ch.access_hash);
                        _peerCache ??= new Dictionary<long, InputPeer>();
                        _peerCache[chatId] = peer;
                        return peer;
                    }
                }
            }
            throw new InvalidOperationException($"Channel for chat_id {chatId} not found in dialogs. Is the account in the group?");
        }

        /// <summary>
        /// Resolves InputChannel for a supergroup (required for forum topic API). Throws for basic groups.
        /// </summary>
        private async Task<InputChannel> GetInputChannel(long chatId)
        {
            if (chatId > 0 && chatId < 1000000000L)
                throw new InvalidOperationException("create-topic works only for supergroups (forum). Use a forum chat_id from list-chats.");
            if (_channelCache != null && _channelCache.TryGetValue(chatId, out var cached))
                return cached;
            var dialogs = await _telegramClient.Messages_GetAllDialogs();
            long channelId = chatId > 0 ? 1000000000000L - chatId : -SUPERGROUP_CHAT_ID_THRESHOLD - chatId;
            if (dialogs is Messages_DialogsSlice slice)
            {
                foreach (var (_, chat) in slice.chats)
                {
                    if (chat is Channel ch && ch.id == channelId)
                    {
                        var inputChannel = new InputChannel(channelId, ch.access_hash);
                        _channelCache ??= new Dictionary<long, InputChannel>();
                        _channelCache[chatId] = inputChannel;
                        return inputChannel;
                    }
                }
            }
            throw new InvalidOperationException($"Channel for chat_id {chatId} not found in dialogs. Is the account in the group?");
        }

        /// <summary>
        /// List all chats (groups and supergroups/channels) the user is in. Returns (chat_id, title) for each.
        /// </summary>
        public async Task<IReadOnlyList<(long ChatId, string Title)>> ListChatsAsync()
        {
            var dialogs = await _telegramClient.Messages_GetAllDialogs();
            var list = new List<(long, string)>();
            IEnumerable<KeyValuePair<long, ChatBase>>? chats = null;
            if (dialogs is Messages_DialogsSlice slice)
                chats = slice.chats;
            else if (dialogs is Messages_Dialogs full)
                chats = full.chats;
            if (chats == null)
                return list;
            foreach (var (_, chat) in chats)
            {
                if (chat is Channel ch)
                {
                    // This codebase uses positive chat_id for supergroups: 1000000000000 - ch.id
                    long chatId = -SUPERGROUP_CHAT_ID_THRESHOLD - ch.id;
                    list.Add((chatId, ch.Title ?? ""));
                }
                else if (chat is Chat c)
                {
                    list.Add((c.id, c.Title ?? ""));
                }
            }
            return list;
        }

        /// <summary>
        /// Create a forum topic in a supergroup (requires forum enabled and manage_topics rights).
        /// Returns the created topic's message id (top_msg_id).
        /// </summary>
        public async Task<int> CreateForumTopic(long chatId, string title)
        {
            var channel = await GetInputChannel(chatId);
            var randomId = (long)(DateTime.UtcNow.Ticks % long.MaxValue);
            var updates = await _telegramClient.Channels_CreateForumTopic(channel, title, randomId);
            if (updates is Updates updatesObj && updatesObj.updates?.Length > 0)
            {
                foreach (var u in updatesObj.updates)
                {
                    if (u is UpdateMessageID mid)
                        return mid.id;
                }
            }
            return 0;
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
        public async Task SendMessageToChat(long chat_id, string message, int? topicId = null)
        {
            var peer = await GetInputPeer(chat_id);
            string html = TelegramFormatHelper.MdToTelegramHtml(message);
            MessageEntity[]? entities = _telegramClient.HtmlToEntities(ref html);
            if (topicId is int topId)
                await _telegramClient.SendMessageAsync(peer, html, reply_to_msg_id: topId, entities: entities);
            else
                await _telegramClient.SendMessageAsync(peer, html, entities: entities);
        }

        /// <summary>
        /// Edit an existing message in a chat. Only messages sent by this account can be edited.
        /// </summary>
        public async Task EditMessage(long chatId, int messageId, string newText)
        {
            var peer = await GetInputPeer(chatId);
            string html = TelegramFormatHelper.MdToTelegramHtml(newText);
            MessageEntity[]? entities = _telegramClient.HtmlToEntities(ref html);
            const int flagMessage = 1 << 11;
            const int flagEntities = 1 << 15; // optional entities
            var req = new TL.Methods.Messages_EditMessage
            {
                flags = (TL.Methods.Messages_EditMessage.Flags)(flagMessage | flagEntities),
                peer = peer,
                id = messageId,
                message = html,
                entities = entities
            };
            await _telegramClient.Invoke(req);
        }

        /// <summary>
        /// Delete a message in a chat. Only messages sent by this account (or with admin rights in channels).
        /// </summary>
        public async Task DeleteMessage(long chatId, int messageId)
        {
            var peer = await GetInputPeer(chatId);
            await _telegramClient.DeleteMessages(peer, [messageId]);
        }

       public async Task SendMessageToUser(string targetUserName, string message)
        {
            var result = await _telegramClient.Contacts_ResolveUsername(targetUserName);
            string html = TelegramFormatHelper.MdToTelegramHtml(message);
            MessageEntity[]? entities = _telegramClient.HtmlToEntities(ref html);
            try
            {
                await _telegramClient.SendMessageAsync(result.User, html, entities: entities);
            }
            catch (RpcException e)
            {
                Console.WriteLine($"I have a trouble  {e.Message} when send message to @{targetUserName}");
            }
        }
        /// <summary>
        /// Re-sends each of our messages in the chat through MD→HTML so they get formatting (bold, code, etc.).
        /// Only messages sent by the current account are edited. Uses the last page of history.
        /// </summary>
        public async Task<int> UpdateOldMessagesInChat(long chatId, int? topicId = null)
        {
            long? ourUserId = _telegramClient.User?.id;
            if (ourUserId == null)
                return 0;
            var messages = await GetMessagesFromChat(chatId, topicId);
            var ours = messages.Where(m => m.from_id is PeerUser pu && pu.user_id == ourUserId && !string.IsNullOrEmpty(m.message)).ToList();
            int edited = 0;
            foreach (var msg in ours)
            {
                try
                {
                    await EditMessage(chatId, msg.id, msg.message ?? "").ConfigureAwait(false);
                    edited++;
                }
                catch (RpcException)
                {
                    // skip failed (e.g. message too old, no change, etc.)
                }
            }
            return edited;
        }

        public async Task<IEnumerable<Message>> GetMessagesFromChat(long chat_id, int? topicId = null)
        {
            var peer = await GetInputPeer(chat_id);
            var history = await _telegramClient.Messages_GetHistory(peer);
            var messages = history.Messages.OfType<Message>();
            if (topicId is int topId)
                messages = messages.Where(m => m.reply_to?.reply_to_top_id == topId || m.reply_to?.reply_to_msg_id == topId || (m.reply_to == null && topId == 0));
            return messages;
        }

        /// <summary>
        /// Get messages as DTOs for JSON output (MCP / scripting).
        /// </summary>
        public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesFromChatAsDtos(long chatId, int? topicId = null)
        {
            var messages = await GetMessagesFromChat(chatId, topicId);
            var dtos = new List<ChatMessageDto>();
            foreach (var msg in messages)
            {
                long? fromId = msg.from_id is PeerUser pu ? pu.user_id : null;
                int? replyTo = msg.reply_to?.reply_to_msg_id;
                int? topId = msg.reply_to?.reply_to_top_id ?? replyTo;
                var dateUnix = (int)((DateTimeOffset)DateTime.SpecifyKind(msg.date, DateTimeKind.Utc)).ToUnixTimeSeconds();
                dtos.Add(new ChatMessageDto
                {
                    Id = msg.id,
                    Date = dateUnix,
                    FromUserId = fromId,
                    Text = msg.message ?? "",
                    ReplyToMsgId = replyTo,
                    TopicId = topId
                });
            }
            return dtos;
        }
        public async Task DownloadMediaFromMessage(Message message, string mediaFolder)
        {
            var d = (MessageMediaDocument) message.media;

            string directory = Path.Combine(Directory.GetCurrentDirectory(), mediaFolder);
           if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var outStr = new FileStream(Path.Combine(directory, (d.document as Document).Filename), FileMode.OpenOrCreate);
           _ = await _telegramClient.DownloadFileAsync(d.document as Document, outStr);
        }
     
    }
}
