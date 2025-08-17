using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch_Bot.Services
{
    public class CachingService
    {
        private DiscordSocketClient _client;
        public Dictionary<ulong, string> _prefixes = new Dictionary<ulong, string>();
        public Dictionary<ulong, List<string>> _badWords = new Dictionary<ulong, List<string>>();
        public Dictionary<ulong, List<ulong>> _usersInDatabase = new Dictionary<ulong, List<ulong>>();
        public List<ulong> _blacklist = new List<ulong>();
        public bool _dbConnected = true;

        public CachingService(DiscordSocketClient client)
        {
            _client = client;
            UpdateDBStatus();
        }
        public void UpdateDBStatus()
        {
            var db = new Database();
            var status = db.isDatabaseConnected();
            _dbConnected = status;
        }
        public bool GetDBStatus()
        {
            return _dbConnected;
        }
        public void SetupPrefixes(SocketGuild guild)
        {
            if (!_dbConnected)
            {
                Console.WriteLine("DB not connected");
                return;
            }

            if (_prefixes.ContainsKey(guild.Id))
                return;

            int levelup = guild.MemberCount >= 100 ? 0 : 1;
            try
            {
                var result = ServerSettingsDB.GetSettings(guild.Id.ToString());
                if (result.Any())
                {
                    _prefixes.Add(guild.Id, result.First().Prefix);
                    return;
                }

                CreateGuildTablesAndSettings(guild.Id.ToString(), levelup);
                var gottenPrefix = ServerSettingsDB.GetSettings(guild.Id.ToString());
                var prefix = gottenPrefix.FirstOrDefault()?.Prefix ?? "?";
                Console.WriteLine($"Prefix: {prefix}");
                Console.WriteLine("Prefix has been set up");
                _prefixes.Add(guild.Id, prefix);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (ex.Message.Contains(guild.Id.ToString()))
                {
                    CreateGuildTablesAndSettings(guild.Id.ToString(), levelup);
                    var gottenPrefix = ServerSettingsDB.GetSettings(guild.Id.ToString());
                    var prefix = gottenPrefix.FirstOrDefault()?.Prefix ?? "?";
                    Console.WriteLine($"Prefix: {prefix}");
                    Console.WriteLine("Prefix has been set up");
                    _prefixes.Add(guild.Id, prefix);
                }
            }
        }

        private void CreateGuildTablesAndSettings(string guildId, int levelup)
        {
            Console.WriteLine("Creating Role Table...");
            ServerSettingsDB.CreateTableRole(guildId);
            Console.WriteLine("Creating Words Table...");
            ServerSettingsDB.CreateTableWords(guildId);
            Console.WriteLine("Making Settings...");
            ServerSettingsDB.MakeSettings(guildId, levelup);
            Console.WriteLine("Gettings Prefix...");
        }
        public void SetupUserInDatabase(SocketGuild guild, SocketGuildUser user)
        {
            if (!_dbConnected)
            {
                Console.WriteLine("DB not connected");
                return;
            }

            if (!_usersInDatabase.ContainsKey(guild.Id))
            {
                // Initialize the user list for this guild if not present
                _usersInDatabase[guild.Id] = new List<ulong>();
            }

            if (!IsInDatabase(guild.Id, user.Id) && !user.IsBot)
            {
                Database.EnterUser(user);
                _usersInDatabase[guild.Id].Add(user.Id);
            }
        }
        public Dictionary<ulong, List<ulong>> GetDatabaseUsers()
        {
            return _usersInDatabase;
        }
        public void AddUser(ulong guildId, ulong userId)
        {
            if (!_usersInDatabase.ContainsKey(guildId))
            {
                _usersInDatabase.Add(guildId, new List<ulong>());
            }
            _usersInDatabase[guildId].Add(userId);
        }
        public void RemoveUser(ulong guildId, ulong userId)
        {
            if (_usersInDatabase.ContainsKey(guildId))
            {
                _usersInDatabase[guildId].Remove(userId);
            }
        }
        public void AddUser(string guildId, ulong userId)
        {
            ulong guildIdParsed = ulong.Parse(guildId);
            if (!_usersInDatabase.ContainsKey(guildIdParsed))
            {
                _usersInDatabase.Add(guildIdParsed, new List<ulong>());
            }
            _usersInDatabase[guildIdParsed].Add(userId);
        }
        public void RemoveUser(string guildId, ulong userId)
        {
            ulong guildIdParsed = ulong.Parse(guildId);
            if (_usersInDatabase.ContainsKey(guildIdParsed))
            {
                _usersInDatabase[guildIdParsed].Remove(userId);
            }
        }
        public bool IsInDatabase(ulong guildId, ulong userId)
        {
            // Check if the user is a bot
            var user = _client.Guilds.FirstOrDefault(x => x.Id == guildId).GetUser(userId);
            if (user == null || user.IsBot)
            {
                return false; // Ignore bots
            }

            // Check if the user is already cached for this guild
            if (_usersInDatabase.TryGetValue(guildId, out var userList))
            {
                if (userList.Contains(userId))
                {
                    return true;
                }
            }
            else
            {
                // Initialize the user list for this guild if not present
                _usersInDatabase[guildId] = new List<ulong>();
            }

            // Check if the user exists in the database
            var result = Database.CheckExistingUser(user);
            if (!result.Any())
            {
                return false;
            }

            // Optionally, add the user to the cache if found in the database
            _usersInDatabase[guildId].Add(userId);

            return true;
        }
        public void SetupBadWords(SocketGuild guild)
        {
            if (!_dbConnected)
            {
                Console.WriteLine("DB not connected");
                return;
            }

            if (_badWords.ContainsKey(guild.Id))
                return;

            var userTable = ServerSettingsDB.GetWords(guild.Id.ToString());
            var words = userTable.Select(w => w.Words).Where(w => !string.IsNullOrWhiteSpace(w)).ToList();

            _badWords.Add(guild.Id, words);
        }
        public void SetupBlackList()
        {
            if (!_dbConnected)
            {
                Console.WriteLine("DB not connected");
                return;
            }

            _blacklist.Clear();

            var blacklistTable = Database.GetAllBlacklistedUsers();
            foreach (var element in blacklistTable)
            {
                if (ulong.TryParse(element.UserId, out var userId))
                {
                    if (!_blacklist.Contains(userId))
                    {
                        _blacklist.Add(userId);
                    }
                }
            }
        }
        public void AddToBlacklist(ulong id)
        {
            _blacklist.Add(id);
        }
        public void RemoveFromBlacklist(ulong id)
        {
            _blacklist.Remove(id);
        }
        public List<ulong> GetBlackList()
        {
            return _blacklist;
        }
        public List<string> GetBadWords(ulong Id)
        {
            return _badWords[Id];
        }
        public string GetPrefix(ulong Id)
        {
            SetupPrefixes(_client.GetGuild(Id));
            return _prefixes[Id];
        }
        public void UpdatePrefix(ulong Id, string prefix)
        {
            _prefixes[Id] = prefix;
        }
        public void UpdateBadWords(ulong Id, List<string> words)
        {
            _badWords[Id] = words;
        }
        public void ClearCache()
        {
            _prefixes.Clear();
            _badWords.Clear();
            _usersInDatabase.Clear();
            _blacklist.Clear();
        }
    }
}
