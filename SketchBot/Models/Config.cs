using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

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

        // Creates a default config.json file with placeholder values
        public static void CreateDefaultConfigFile(string path = "config.json")
        {
            var defaultConfig = new Config
            {
                Prefix = "!",
                Token = "YOUR_TOKEN_HERE",
                OsuApiKey = "YOUR_OSU_API_KEY_HERE",
                OsuApiId = 0,
                DblApiKey = "YOUR_DBL_API_KEY_HERE",
                DatabaseUsername = "dbuser",
                DatabasePassword = "dbpassword",
                DatabaseHost = "localhost",
                IMGFlip = "YOUR_IMGFLIP_KEY_HERE",
                CaseNumber = 0,
                TempDBUsername = "tempdbuser",
                TempDBPassword = "tempdbpassword",
                TempDBHost = "localhost",
                TempDBDatabase = "tempdatabase"
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(defaultConfig, options);
            File.WriteAllText(path, json);
        }
    }
}
