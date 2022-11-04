//Console application Discord bot with periodic webscraping functions
//Developed in .NET 6 with Discord.NET framework
//Martin Qvarnström SUT22 Campus Varberg, mqvarnstrom80@gmail.com, github: qvarnstr0m

//Program.cs class with Main method to handle runtime

using Discord.Commands;
using Discord.WebSocket;
using Discord;
using DiscordBotSUT22.Modules;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using System.Web;

namespace DiscordBotSUT22
{
    internal class Program
    {
        //List of strings to hold parsed Html
        private List<string> classesList = new List<string>();

        //Final List of InsiderTransaction objects
        private List<OnSiteLecture> finishedList = new List<OnSiteLecture>();

        //String to hold the raw Html
        private string response = "";

        //String to hold the full url to scrape
        private string url = "http://student.varberg.se/kiosk/schema/";

        //String to hold the value of the class
        private string campusClass = "SUT22";

        //Main method to run client
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        //Fields to handle client connections and commands
        private DiscordSocketClient _client;
        public CommandService _commands;
        private IServiceProvider _services;
        private SocketGuild guild;

        //Get token through .txt file and StreamReader
        StreamReader readToken = new StreamReader(".txt");

        //Log channel info
        private ulong LogChannelID;
        private SocketTextChannel LogChannel;

        //Connect bot
        public async Task RunBotAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = readToken.ReadToEnd();


            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await TimerInterval();

            await Task.Delay(-1);

            //Bot is online
        }

        //Timer to check if it is time to scan html in string fullUrl
        private async Task TimerInterval()
        {
            TimeSpan time = new TimeSpan(0, 10, 0);
            PeriodicTimer timer = new PeriodicTimer(time);

            while (await timer.WaitForNextTickAsync())
            {
                if (ReturnSwedishTime().Hour == 8)
                    await ScanWebpage();
                if (finishedList.Count > 0)
                {
                    foreach (var item in finishedList)
                    {
                        await _client.GetGuild(1017373222972443453).GetTextChannel(103782375284880345).
                        SendMessageAsync(item.ToString());
                    }
                    finishedList.Clear();
                }
            }
        }

        //Method to init scan for lectures on site
        private async Task ScanWebpage()
        {
            response = CallUrl(url).Result;
            classesList = ParseHtml(response);
            finishedList.AddRange(ConvertList(classesList));
        }

        //Method to get raw html document from fullUrl
        private async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        //Method to parse HTML
        private List<string> ParseHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var transactions = htmlDoc.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Contains("label")).ToList();

            List<string> finishedList = new List<string>();
            foreach (var item in transactions)
            {
                finishedList.Add(item.InnerText);
            }

            return finishedList;
        }

        //Method to convert raw List of lines to List of Transaction objects
        private List<OnSiteLecture> ConvertList(List<string> rawList)
        {
            List<OnSiteLecture> newList = new List<OnSiteLecture>();
            int lines = rawList.Count;
            //Loop to go through 6 lines(One object) at a time
            for (int i = 0; i < lines - 6; i = i + 6)
            {
                //Check if lecture concerns class in campusClass string
                string checkSUT22 = HttpUtility.HtmlDecode(rawList[i + 4]).ToUpper();
                if (checkSUT22.Contains(campusClass))
                {
                    DateTime startTime = DateTime.Parse(HttpUtility.HtmlDecode(rawList[i]));
                    DateTime endTime = DateTime.Parse(HttpUtility.HtmlDecode(rawList[i + 1]));
                    string building = HttpUtility.HtmlDecode(rawList[i + 2]);
                    string classroom = HttpUtility.HtmlDecode(rawList[i + 3]);
                    string classroomAndSubject = HttpUtility.HtmlDecode(rawList[i + 4]);
                    string extraInfo = HttpUtility.HtmlDecode(rawList[i + 5]);
                    newList.Add(new OnSiteLecture(startTime, endTime, building, classroom, classroomAndSubject, extraInfo));
                }
            }
            return newList;
        }

        //Method to return local swedish time, daylight savings time adjusted up to year 2024
        private static DateTime ReturnSwedishTime()
        {
            DateTime nowUTC = DateTime.UtcNow;
            TimeSpan oneHour = new TimeSpan(1, 0, 0);
            DateTime nowCET = nowUTC.Add(oneHour);

            DateTime startSummerTime2022 = new DateTime(2022, 03, 27);
            DateTime endSummerTime2022 = new DateTime(2022, 10, 30);
            DateTime startSummerTime2023 = new DateTime(2023, 03, 26);
            DateTime endSummerTime2023 = new DateTime(2023, 10, 29);
            DateTime startSummerTime2024 = new DateTime(2024, 03, 31);
            DateTime endSummerTime2024 = new DateTime(2024, 10, 27);

            if (nowCET >= startSummerTime2022 && nowCET <= endSummerTime2022 || nowCET >= startSummerTime2023 && nowCET <= endSummerTime2023 ||
                nowCET >= startSummerTime2024 && nowCET <= endSummerTime2024)
            {
                nowCET = nowCET.Add(oneHour);
            }

            return nowCET;
        }

        //Log events to console
        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        //Register Commands Async
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        //Input and output logic
        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            var channel = _client.GetChannel(LogChannelID) as SocketTextChannel;

            //Log messages to console
            Console.WriteLine($"User {message.Author.Username} ({message.Author.Id}) wrote:\n{message.ToString()}");

            //Make client ignore own output
            if (message.Author.IsBot) return;

            int argPos = 0;

            //Set / char to execute commands in Commands class
            if (message.HasStringPrefix("/", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
            }

            //Make input text lowercase
            var text = message.ToString().ToLower();

            //Respond to certain words and phrases
            switch (text)
            {
                case "hej bot":
                    await message.Channel.SendMessageAsync("Hej " + message.Author.Username + "!");
                    break;
                case "blipp":
                    await message.Channel.SendMessageAsync("Blopp!");
                    break;
            }

            if (text.Contains("diesel") || text.Contains("bensin"))
            {
                await message.Channel.SendMessageAsync("Don't mention the bränslepriser...");
            }
        }
    }
}