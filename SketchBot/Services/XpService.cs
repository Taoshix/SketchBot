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
        private readonly Timer _timer;
        public List<SocketGuildUser> userlist = new List<SocketGuildUser>();
        public XpService(DiscordSocketClient client)
        {
            _timer = new Timer(async _ =>
            {
                //Console.WriteLine(userlist.Count() + " users cleared!");
                userlist.Clear();
            },
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
        }
        public void addUser(SocketGuildUser user)
        {
            userlist.Add(user);
        }
        public List<SocketGuildUser> GetList()
        {
            return userlist;
        }
    }
}
