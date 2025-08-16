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
            Console.WriteLine("Creating Table...");
            ServerSettingsDB.CreateTable(guildId);
            Console.WriteLine("Creating Role Table...");
            ServerSettingsDB.CreateTableRole(guildId);
            Console.WriteLine("Creating Words Table...");
            ServerSettingsDB.CreateTableWords(guildId);
            Console.WriteLine("Making Settings...");
            ServerSettingsDB.MakeSettings(guildId, levelup);
            Console.WriteLine("Gettings Prefix...");
        }
        public void SetupUserInDatabase(SocketGuild guild ,SocketGuildUser user)
        {
            if (_dbConnected)
            {
                if (!_usersInDatabase.ContainsKey(guild.Id))
                {
                    _usersInDatabase.Add(guild.Id, new List<ulong>());
                }
                if (!IsInDatabase(guild.Id, user.Id))
                {
                    var result = Database.CheckExistingUser(user); /*We check if the database contains the user*/
                    if (result.Count <= 0 && user.IsBot == false) /*Checks if result contains anyone and checks if the user is not a bot*/
                    {
                        Database.EnterUser(user);  /*Enters the user*/
                    }
                    _usersInDatabase[guild.Id].Add(user.Id);
                }
            }
            else
            {
                Console.WriteLine("DB not connected");
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
            if (!_client.GetUser(userId).IsBot)
            {
                if (!_usersInDatabase.ContainsKey(guildId))
                {
                    _usersInDatabase.Add(guildId, new List<ulong>());
                }
                return _usersInDatabase[guildId].Contains(userId);
            }
            else
            {
                return true;
            }
            
        }
        public bool IsInDatabase(string guildId, ulong userId)
        {
            ulong guildIdParsed = ulong.Parse(guildId);
            if (!_client.GetUser(userId).IsBot)
            {
                if (!_usersInDatabase.ContainsKey(guildIdParsed))
                {
                    _usersInDatabase.Add(guildIdParsed, new List<ulong>());
                }
                return _usersInDatabase[guildIdParsed].Contains(userId);
            }
            else
            {
                return true;
            }

        }
        public void SetupBadWords(SocketGuild guild)
        {
            if (_dbConnected)
            {
                if (!_badWords.ContainsKey(guild.Id))
                {
                    var userTable = ServerSettingsDB.GetWords(guild.Id.ToString());
                    if (!userTable.Any())
                    {
                        _badWords.Add(guild.Id, new List<string>());
                        return;
                    }
                    List<string> words = new List<string>();
                    foreach (var word in userTable)
                    {
                        words.Add(word.Words);
                    }
                    if (!words.Any())
                    {
                        _badWords.Add(guild.Id, new List<string>());
                        return;
                    }
                    _badWords.Add(guild.Id, words);
                }
            }
            else
            {
                Console.WriteLine("DB not connected");
            }
        }
        public void SetupBlackList()
        {
            if (_dbConnected)
            {
                var userTable = Database.GetAllBlacklistedUsers();
                foreach (var element in userTable)
                {
                    _blacklist.Add(ulong.Parse(element.UserId));
                }
            }
            else
            {
                Console.WriteLine("DB not connected");
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
