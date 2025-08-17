using System;
using System.Collections.Generic;
using System.Text;
using System.Threading; // 1) Add this namespace
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using System.Linq;
using Sketch_Bot.Models;

namespace Sketch_Bot.Services
{


    public class TimerService
    {
        private Dictionary<string, Timer> timers = new Dictionary<string, Timer>(); // 2) Add a field like this
                                                           // This example only concerns a single timer.
                                                           // If you would like to have multiple independant timers,
                                                           // you could use a collection such as List<Timer>,
                                                           // or even a Dictionary<string, Timer> to quickly get
                                                           // a specific Timer instance by name.


        private CachingService _cachingService;

        //string Message = "not NULL (Get rekt Declan xD)";
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
                    await DiscordBots.UpdateStats(numberOfGuilds);
                    await DiscordBots.UpdateStats2(numberOfGuilds);
                    
                    // 3) Any code you want to periodically run goes here, for example:
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            },
            null,
            TimeSpan.FromMinutes(10),  // 4) Time that message should fire after the timer is created
            TimeSpan.FromMinutes(30))); // 5) Time after which message should repeat (use `Timeout.Infinite` for no repeat));
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
            TimeSpan.FromMinutes(0),  // 4) Time that message should fire after the timer is created
            TimeSpan.FromSeconds(10))); // 5) Time after which message should repeat (use `Timeout.Infinite` for no repeat));
        }

        public void Stop() // 6) Example to make the timer stop running
        {
            timers["Timer"].Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Restart() // 7) Example to restart the timer
        {
            timers["Timer"].Change(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30));
        }
    }
}
