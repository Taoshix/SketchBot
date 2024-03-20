using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Sketch_Bot.Custom_Preconditions;
using Sketch_Bot.Services;

namespace Sketch_Bot.Modules
{
    public class TimerModule : ModuleBase<SocketCommandContext>
    {
        private readonly TimerService _service;

        public TimerModule(TimerService service) // Make sure to configure your DI with your TimerService instance
        {
            _service = service;
        }
        [RequireDevelopers]
        [Command("stoptimer")]
        public async Task StopCmd()
        {
            _service.Stop();
            await ReplyAsync("Timer stopped.");
        }
        [RequireDevelopers]
        [Command("starttimer")]
        public async Task RestartCmd()
        {
            _service.Restart();
            await ReplyAsync("Timer (re)started.");
        }
    }
}
