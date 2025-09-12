using System;
using System.Collections.Generic;
using System.Text;

namespace SketchBot.Models
{
    public class Serversettings()
    {
        public ulong GuildId { get; set; } = 0;
        public string Prefix { get; set; } = "?";
        public ulong WelcomeChannel { get; set; } = 0;
        public string Words { get; set; } = "";
        public ulong ModlogChannel { get; set; } = 0;
        public ulong RoleId { get; set; } = 0;
        public int RoleLevel { get; set; } = 0;
        public int XpMultiplier { get; set; } = 1;
        public bool LevelupMessages { get; set; } = true;
    }
}
