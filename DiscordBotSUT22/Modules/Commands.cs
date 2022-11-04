//Console application Discord bot with periodic webscraping functions
//Developed in .NET 6 with Discord.NET framework
//Martin Qvarnström SUT22 Campus Varberg, mqvarnstrom80@gmail.com, github: qvarnstr0m

//Commands.cs class to handle custom /commands

using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotSUT22.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("testcommand")]
        public async Task Comandi()
        {
            await ReplyAsync("This a /testcommand");
        }
    }
}
