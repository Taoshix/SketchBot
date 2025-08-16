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
            _dbConnected = UpdateDBStatus();
        }
        public bool UpdateDBStatus()
        {
            var db = new Database();
            var status = db.isDatabaseConnected();
            return status;
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
                var result = ServerSettingsDB.GetPrefix(guild.Id.ToString());
                if (result.Count > 0)
                {
                    _prefixes.Add(guild.Id, result.First().Prefix);
                    return;
                }

                CreateGuildTablesAndSettings(guild.Id.ToString(), levelup);
                var gottenPrefix = ServerSettingsDB.GetPrefix(guild.Id.ToString());
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
                    var gottenPrefix = ServerSettingsDB.GetPrefix(guild.Id.ToString());
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

            if (!_usersInDatabase.TryGetValue(guild.Id, out var userList))
            {
                userList = new List<ulong>();
                _usersInDatabase[guild.Id] = userList;
            }

            if (!IsInDatabase(guild.Id, user.Id))
            {
                var result = Database.CheckExistingUser(user);
                if (result.Count == 0 && !user.IsBot)
                {
                    Database.EnterUser(user);
                }
                userList.Add(user.Id);
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
            var user = _client.GetUser(userId);
            if (user == null || user.IsBot)
            {
                return true; // Bots are always considered to be in the database so we don't track them
            }

            if (!_usersInDatabase.TryGetValue(guildId, out var userList))
            {
                userList = new List<ulong>();
                _usersInDatabase[guildId] = userList;
            }

            return userList.Contains(userId);
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

            var userTable = Database.GetAllBlacklistedUsers();
            foreach (var element in userTable)
            {
                if (ulong.TryParse(element.UserId, out var userId))
                {
                    _blacklist.Add(userId);
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
    }
}
