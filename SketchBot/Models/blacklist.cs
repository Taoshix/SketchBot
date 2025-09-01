using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch_Bot.Models
{
    public class Blacklist
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }
        public string Reason { get; set; }
        public string Blacklister { get; set; }
    }
}
