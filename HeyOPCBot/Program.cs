using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HeyOPCBot
{
    public class Program
    {
        static void Main(string[] args) => CreateBot().GetAwaiter().GetResult();
        public static DiscordSocketClient Client;
        public static CommandService Commands;
        public static Random Random;
        public static string Prefix;
        public static async Task CreateBot()
        {
            if(!File.Exists(AppContext.BaseDirectory+"/configuration.json"))
            {
                var Config = new Config();
                Config.Save();
                Console.WriteLine("Created a new config file, add the shit and try again son");
                await Task.Delay(-1);
            }
            else
            {
                Client = new DiscordSocketClient(new DiscordSocketConfig()
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 10000,
                    AlwaysDownloadUsers = true,
                    DefaultRetryMode = RetryMode.RetryTimeouts
                });
                Console.WriteLine("Created Client");
                Commands = new CommandService(new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Verbose
                });
                Console.WriteLine("Created CommandService");
                Client.Log += Client_Log;
                Client.MessageReceived += Client_MessageReceived;
                Random = new Random();
                var Config = HeyOPCBot.Config.Load();
                Prefix = Config.Prefix;
                await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
                Console.WriteLine("HeyOPCBot has " + Commands.Commands.Count() + " commands from " + Commands.Modules.Count() + " modules");
                await StartBot(Config.Token);
            }            
        }

        private static async Task Client_MessageReceived(SocketMessage arg)
        {
            if(!arg.Author.IsBot)
            {
                var message = arg as SocketUserMessage;
                if (message == null) return;
                int argPos = 0;
                var chan = message.Channel as SocketGuildChannel;
                var context = new SocketCommandContext(Client, message);
                if (message.HasStringPrefix(Prefix, ref argPos))
                {
                    var result = await Commands.ExecuteAsync(context, argPos);
                    if (result != null)
                    {
                        if (!result.IsSuccess)
                        {
                            if (result.Error != CommandError.UnknownCommand)
                            {
                                if (result.Error == CommandError.UnmetPrecondition || result.Error == CommandError.BadArgCount)
                                    await MessageHandler.SendChannel(context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command famalam" }, Description = Convert.ToString(result.ErrorReason), Color = new Color(255, 0, 0) });
                            }
                        }
                    }
                }
                else { }
            }
        }

        private static Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public static async Task StartBot(string Token)
        {
            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }
    }
    public class Commands : ModuleBase
    {
        [Command("quote")]
        public async Task ReturnQuote()
        {
            var Quotes = JsonConvert.DeserializeObject<List<Quotes>>(File.ReadAllText(AppContext.BaseDirectory + @"\quotes.json"));
            var Quote = Quotes.ElementAtOrDefault(Program.Random.Next(0, Quotes.Count));
            await MessageHandler.SendChannel(Context.Channel,"", new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = Program.Client.CurrentUser.GetAvatarUrl(),
                    Name = Quote.Author,
                    Url = Quote.URL
                },
                Description = Quote.Quote
            });
        }
        [Command("vquote")]
        public async Task VoiceQuote()
        {

        }
        [Command("chat")]
        public async Task ChatThing([Remainder]string ChatMessage)
        {
            Console.WriteLine(Tools.CalculateSimilarity(ChatMessage, "youre gay"));
            if(Tools.CalculateSimilarity(ChatMessage,"youre gay")>0.3)
                await MessageHandler.SendChannel(Context.Channel, "i'm **NOT** gay");
            else if (Tools.CalculateSimilarity(ChatMessage, "youre a buttface") > 0.3)
                await MessageHandler.SendChannel(Context.Channel, "i'm **NOT** a buttface, you are");
            else if (Tools.CalculateSimilarity(ChatMessage, "you have a buttface") > 0.3)
                await MessageHandler.SendChannel(Context.Channel, "i **DO NOT** have a buttface, you do");
            else if (Tools.CalculateSimilarity(ChatMessage, "buttface") > 0.3)
                await MessageHandler.SendChannel(Context.Channel, "i **DO NOT** have a buttface, you do");
            else
                await MessageHandler.SendChannel(Context.Channel, "no");
        }
    }
    /*https://stackoverflow.com/questions/6944056/c-sharp-compare-string-similarity*/
    static class Tools
    {
        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
    }
    
}
