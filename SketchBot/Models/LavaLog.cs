using System;
using System.Collections.Generic;
using System.Text;

namespace Sketch_Bot.Models
{
    public class LavaLog
    {
        public int PlayingPlayers { get; set; }
        public string Op { get; set; }
        public struct memory
        {
            public ulong Reserved;
            public ulong Used;
            public ulong Free;
            public ulong allocated;
        }
        public memory Memory { get; set; }
        public int Players { get; set; }
        public struct cpu
        {
            public int Cores;
            public double SystemLoad;
            public double LavalinkLoad;
        }
        public cpu Cpu { get; set; }
        public ulong Uptime { get; set; }
        public struct state
        {
            public ulong Position;
            public ulong Time;
            public ulong GuildId;
        }
        public state State { get; set; }
    }
}
