using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace CSharpURLShortenerBot
{
    public static class ShortenerBot
    {
        #region Fields

        /// <summary>
        /// Start command response
        /// </summary>
        private const string StartCommandResponse = "Welcome\n" +
                                                    "Send a url like the following url to shortening:\n" +
                                                    " https://www.google.com/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png";

        /// <summary>
        /// Invalid URL response
        /// </summary>
        private const string InvalidUrlResponse = "Invalid url!";

        /// <summary>
        /// Url shortener site
        /// </summary>
        private const string ShortenedUrl = "http://yon.ir/{0}";

        /// <summary>
        /// Url shortener site API address
        /// </summary>
        private const string UrlShortenerApi = "http://api.yon.ir/?url={0}";

        /// <summary>
        /// To communicate with Telegram Bot API
        /// </summary>
        private static ITelegramBotClient _botClient;

        #endregion

        #region Events

        /// <summary>
        /// Occur when user send a message to user
        /// </summary>
        public static event EventHandler<ChatEventArgs> MessageReceived;
        private static void OnMessageReceived(ChatEventArgs e)
        {
            MessageReceived?.Invoke(null, e);
        }

        /// <summary>
        /// Occur when bot send a message to bot
        /// </summary>
        public static event EventHandler<ChatEventArgs> MessageSent;
        private static void OnMessageSent(ChatEventArgs e)
        {
            MessageSent?.Invoke(null, e);
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Launch the robot with given token
        /// </summary>
        /// <param name="botToken">توکن ربات</param>
        public static void InitializeBot(string botToken)
        {
            _botClient = new TelegramBotClient(botToken);
            _botClient.OnMessage += OnMessageRecevied;
        }

        /// <summary>
        /// Start bot
        /// </summary>
        public static void Start()
        {
            _botClient.StartReceiving();
        }

        /// <summary>
        /// Stop bot
        /// </summary>
        public static void Stop()
        {
            _botClient.StopReceiving();
        }

        #endregion

        #region Private Members

        /// <summary>
        /// To validate URL using regular expression regular expression
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool IsValidUrl(string url)
        {
            if (url == null)
                return false;

            try
            {
                const string pattern = @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$";
                var regex = new Regex(pattern);
                var match = regex.Match(url);
                return match.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// To shortening URL using yon.ir API
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string> GetShortUrlAsync(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var siteResponse = await client.DownloadDataTaskAsync(string.Format(UrlShortenerApi, url));
                    var json = JObject.Parse(Encoding.UTF8.GetString(siteResponse));
                    var shortedUrl = json["output"].ToString();
                    return string.Format(ShortenedUrl, shortedUrl);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Occur when user message received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void OnMessageRecevied(object sender, MessageEventArgs e)
        {
            var message = e.Message.Text;

            if (string.IsNullOrEmpty(message))
                return;

            OnMessageReceived(new ChatEventArgs { Message = message });

            if (message.Equals("/start"))
            {
                // ارسال پیام خوش آمد گویی به کاربر
                await _botClient.SendTextMessageAsync(e.Message.Chat,
                    StartCommandResponse,
                    ParseMode.Default,
                    true);

                OnMessageSent(new ChatEventArgs { Message = StartCommandResponse });
                return;
            }

            if (!IsValidUrl(message))
            {
                // ارسال پیام خطا به کاربر
                await _botClient.SendTextMessageAsync(e.Message.Chat,
                    InvalidUrlResponse,
                    ParseMode.Default,
                    true, false,
                    e.Message.MessageId);

                OnMessageSent(new ChatEventArgs { Message = InvalidUrlResponse });
                return;
            }

            await SendShortenedUrlAsync(e, message);
        }

        /// <summary>
        /// Used to send shortened URL to the user
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task SendShortenedUrlAsync(MessageEventArgs e, string message)
        {
            var shoretedUrl = await GetShortUrlAsync(message);

            await _botClient.SendTextMessageAsync(e.Message.Chat,
                shoretedUrl,  
                ParseMode.Default, 
                true, 
                false, 
                e.Message.MessageId); 

            OnMessageSent(new ChatEventArgs { Message = shoretedUrl });
        }

        #endregion
    }
}
