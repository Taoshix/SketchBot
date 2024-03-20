using System;
using System.Collections.Generic;
using System.Text;

namespace Sketch_Bot.Models
{
    public class BotStats
    {
        public int Servers { get; set; }
        public int Users { get; set; }
        public int MsgSinceStartup { get; set; }
        public int CmdsSinceStartup { get; set; }
        public string MsgPerMin { get; set; }
        public string CmdsPerMin { get; set; }
        public DateTime StartUpTime { get; set; }

        public BotStats(int servers, int users, int msgsincestartup, int cmdssincestartup, string msgpermin, string cmdspermin, DateTime startuptime)
        {
            Servers = servers;
            Users = users;
            MsgSinceStartup = msgsincestartup;
            CmdsSinceStartup = cmdssincestartup;
            MsgPerMin = msgpermin;
            CmdsPerMin = cmdspermin;
            StartUpTime = startuptime;
        }
    }
}
