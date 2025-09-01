using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch_Bot.Models
{
    public class UserStats
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }
        public long Tokens { get; set; }
        public DateTime Daily { get; set; }
        public long XP { get; set; }
        public long Level { get; set; }
    }
}
