using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Sketch_Bot.Models;

namespace Sketch_Bot.Services
{
    public class StatService
    {
        List<float> msgPerMin = new List<float>();
        public int msgCounter;
        private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        private DiscordSocketClient _client;
        public CachingService _cache;
        public TimeSpan uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
        public int cmdCounter;

        public StatService(DiscordSocketClient client)
        {
            _client = client;
            timers.Add("Update Stats", new Timer(async _ =>
            {
                try
                {
                    if (_cache._dbConnected)
                    {
                        Database.UpdateStats(buildBotStats());
                        TempDB.UpdateStats(buildBotStats());
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine(ex);
                }
            },
            null,
            TimeSpan.FromSeconds(5),  // 4) Time that message should fire after the timer is created
            TimeSpan.FromSeconds(10))); // 5) Time after which message should repeat (use `Timeout.Infinite` for no repeat)););
        }

        public List<float> GetMsgPerMin()
        {
            return msgPerMin;
        }
        public float MsgPerMinAverage()
        {
            return msgPerMin.Average();
        }
        public BotStats buildBotStats()
        {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("EN_US");
            var averagePerMin = (msgCounter / (float)uptime.TotalMinutes).ToString("0.000");
            var averageCmdPerMin = (cmdCounter / (float)uptime.TotalMinutes).ToString("0.000");

            uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            BotStats stats = new BotStats(_client.Guilds.Count, _client.Guilds.Sum(x => x.MemberCount), msgCounter, cmdCounter, averagePerMin, averageCmdPerMin, Process.GetCurrentProcess().StartTime);
            return stats;
        }
        public void AddCache(CachingService cache)
        {
            _cache = cache;
        }
    }
}
