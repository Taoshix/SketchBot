using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sketch_Bot.Models
{
    public class Config
    {
        public string Prefix { get; set; }
        public string Token { get; set; }
        public string OsuApiKey { get; set; }
        public int OsuApiId { get; set; }
        public string DblApiKey { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseHost { get; set; }
        public string IMGFlip { get; set; }
        public int CaseNumber { get; set; }
        public string TempDBUsername { get; set; }
        public string TempDBPassword { get; set; }
        public string TempDBHost { get; set; }
        public string TempDBDatabase { get; set; }
        
    }
}
