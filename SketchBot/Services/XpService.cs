using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using System.Linq;

namespace Sketch_Bot.Services
{
    public class XpService
    {
        private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        public List<SocketGuildUser> userlist = new List<SocketGuildUser>();
        private List<ulong> userIds = new List<ulong>();
        private int totalEventCount = 0;
        private int uniqueEventCount = 0;
        public XpService(DiscordSocketClient client)
        {
            timers.Add("main xp loop", new Timer(async _ =>
            {
                //Console.WriteLine(userlist.Count() + " users cleared!");
                userlist.Clear();
            },
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1)));
            timers.Add("event count display", new Timer(async _ =>
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {totalEventCount} ({uniqueEventCount} unique) xp events in the last 15 minutes!");
                totalEventCount = 0;
                uniqueEventCount = 0;
                userIds.Clear();
            },
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(15)));
        }
        public void AddUser(SocketGuildUser user)
        {
            userlist.Add(user);
            if (!userIds.Contains(user.Id))
            {
                userIds.Add(user.Id);
                uniqueEventCount++;
            }
            totalEventCount++;
        }
        public List<SocketGuildUser> GetList()
        {
            return userlist;
        }
    }
}
