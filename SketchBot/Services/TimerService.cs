using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MySqlX.XDevAPI;
using Sketch_Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sketch_Bot.Services
{


    public class TimerService
    {
        private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();


        private CachingService _cachingService;

        public TimerService(DiscordSocketClient client, CachingService cachingService)
        {
            _cachingService = cachingService;
            timers.Add("Timer", new Timer(async _ =>
            {
                try
                {
                    int TotalMembers() => client.Guilds.Sum(x => x.MemberCount);
                    var numberOfGuilds = client.Guilds.Count;
                    await client.SetGameAsync(numberOfGuilds + " servers! | " + TotalMembers() + " users! | www.sketchbot.xyz");
                    await DiscordBots.UpdateDblStatsAsync(numberOfGuilds, client.CurrentUser.Id);
                    await DiscordBots.UpdateDiscordBotsGgStatsAsync(numberOfGuilds, client.CurrentUser.Id);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            },
            null,
            TimeSpan.FromMinutes(10),  //Time that message should fire after the timer is created
            TimeSpan.FromMinutes(30))); // Time after which message should repeat (use `Timeout.Infinite` for no repeat));
            timers.Add("Database", new Timer(async _ =>
            {
                bool dbStatusBefore = _cachingService.GetDBStatus();
                _cachingService.UpdateDBStatus();
                bool dbStatusAfter = _cachingService.GetDBStatus();
                if (dbStatusBefore != dbStatusAfter)
                {
                    Console.WriteLine($"Database status changed: {dbStatusBefore} -> {dbStatusAfter}");
                    if (dbStatusAfter)
                    {
                        _cachingService.ClearCache();
                        _cachingService.SetupBlackList();
                    }
                }
            },
            null,
            TimeSpan.FromMinutes(0),  // Time that message should fire after the timer is created
            TimeSpan.FromSeconds(10))); // Time after which message should repeat (use `Timeout.Infinite` for no repeat));
        }

        public void Stop()
        {
            timers["Timer"].Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart()
        {
            timers["Timer"].Change(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30));
        }
    }
}
