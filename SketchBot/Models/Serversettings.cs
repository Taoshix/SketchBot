using System;
using System.Collections.Generic;
using System.Text;

namespace Sketch_Bot.Models
{
    public class Serversettings
    {
        public string Prefix { get; set; }
        public ulong WelcomeChannel { get; set; }
        public string Words { get; set; }
        public ulong ModlogChannel { get; set; }
        public ulong roleId { get; set; }
        public int roleLevel { get; set; }
        public int XpMultiplier { get; set; }
        public bool LevelupMessages { get; set; }
    }
}
