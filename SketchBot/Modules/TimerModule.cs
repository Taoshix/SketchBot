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
    [RequireDevelopers]
    public class TimerModule : ModuleBase<SocketCommandContext>
    {
        private readonly TimerService _service;

        public TimerModule(TimerService service)
        {
            _service = service;
        }

        [Command("stoptimer")]
        public async Task StopTimerAsync()
        {
            _service.Stop();
            await ReplyAsync("Timer stopped.");
        }
        
        [Command("starttimer")]
        public async Task StartTimerAsync()
        {
            _service.Restart();
            await ReplyAsync("Timer (re)started.");
        }
    }
}
