using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace SketchBot.Models
{
    public class Config
    {
        public string Prefix { get; set; }
        public string Token { get; set; }
        public string OsuApiKey { get; set; }
        public int OsuApiId { get; set; }
        public string DblApiKey { get; set; }
        public string DbggApiKey { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseHost { get; set; }
        public string IMGFlip { get; set; }
        public string TempDBUsername { get; set; }
        public string TempDBPassword { get; set; }
        public string TempDBHost { get; set; }
        public string TempDBDatabase { get; set; }

        // Loads config from environment variables, then config.json, then fallback to empty string/0
        public static Config Load(string path = "config.json")
        {
            Config fileConfig = null;
            if (!File.Exists(path))
            {
                CreateDefaultConfigFile(path);
            }
            if (File.Exists(path))
            {
                try
                {
                    fileConfig = JsonSerializer.Deserialize<Config>(File.ReadAllText(path));
                }
                catch { }
            }
            return new Config
            {
                Prefix = Environment.GetEnvironmentVariable("SKETCHBOT_PREFIX") ?? fileConfig?.Prefix ?? string.Empty,
                Token = Environment.GetEnvironmentVariable("SKETCHBOT_TOKEN") ?? fileConfig?.Token ?? string.Empty,
                OsuApiKey = Environment.GetEnvironmentVariable("SKETCHBOT_OSU_API_KEY") ?? fileConfig?.OsuApiKey ?? string.Empty,
                OsuApiId = int.TryParse(Environment.GetEnvironmentVariable("SKETCHBOT_OSU_API_ID"), out var osuId) ? osuId : fileConfig?.OsuApiId ?? 0,
                DblApiKey = Environment.GetEnvironmentVariable("SKETCHBOT_DBL_API_KEY") ?? fileConfig?.DblApiKey ?? string.Empty,
                DbggApiKey = Environment.GetEnvironmentVariable("SKETCHBOT_DBGG_API_KEY") ?? fileConfig?.DbggApiKey ?? string.Empty,
                DatabaseUsername = Environment.GetEnvironmentVariable("SKETCHBOT_DATABASE_USERNAME") ?? fileConfig?.DatabaseUsername ?? string.Empty,
                DatabasePassword = Environment.GetEnvironmentVariable("SKETCHBOT_DATABASE_PASSWORD") ?? fileConfig?.DatabasePassword ?? string.Empty,
                DatabaseHost = Environment.GetEnvironmentVariable("SKETCHBOT_DATABASE_HOST") ?? fileConfig?.DatabaseHost ?? string.Empty,
                IMGFlip = Environment.GetEnvironmentVariable("SKETCHBOT_IMGFLIP") ?? fileConfig?.IMGFlip ?? string.Empty,
                TempDBUsername = Environment.GetEnvironmentVariable("SKETCHBOT_TEMPDB_USERNAME") ?? fileConfig?.TempDBUsername ?? string.Empty,
                TempDBPassword = Environment.GetEnvironmentVariable("SKETCHBOT_TEMPDB_PASSWORD") ?? fileConfig?.TempDBPassword ?? string.Empty,
                TempDBHost = Environment.GetEnvironmentVariable("SKETCHBOT_TEMPDB_HOST") ?? fileConfig?.TempDBHost ?? string.Empty,
                TempDBDatabase = Environment.GetEnvironmentVariable("SKETCHBOT_TEMPDB_DATABASE") ?? fileConfig?.TempDBDatabase ?? string.Empty
            };
        }

        // Creates a default config.json file with placeholder values
        public static void CreateDefaultConfigFile(string path = "config.json")
        {
            var defaultConfig = new Config
            {
                Prefix = "?",
                Token = "YOUR_TOKEN_HERE",
                OsuApiKey = "YOUR_OSU_API_KEY_HERE", // osu.ppy.sh API key
                OsuApiId = 0,
                DblApiKey = "YOUR_DBL_API_KEY_HERE", // Discord Bot List API key
                DbggApiKey = "YOUR_DBGG_API_KEY_HERE", // Discord Bots GG API key
                DatabaseUsername = "dbuser",
                DatabasePassword = "dbpassword",
                DatabaseHost = "localhost",
                IMGFlip = "YOUR_IMGFLIP_KEY_HERE", 
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
