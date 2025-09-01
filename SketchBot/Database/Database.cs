using Discord;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sketch_Bot.Models;
using Discord.Rest;
using Newtonsoft.Json;
using System.IO;

namespace Sketch_Bot
{
    public class Database
    {

        private MySqlConnection? dbConnection;
        private Config config;

        public Database()
        {
            config = Config.Load();
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = config.DatabaseHost;
            stringBuilder.UserID = config.DatabaseUsername;
            stringBuilder.Password = config.DatabasePassword;
            stringBuilder.Database = "sketchbot";
            stringBuilder.SslMode = MySqlSslMode.Disabled;
            stringBuilder.Pooling = false;
            stringBuilder.AllowPublicKeyRetrieval = true;


            var connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            try
            {
                dbConnection.Open();
            }
            catch (Exception)
            {
                dbConnection = null;
            }
        }

        public bool IsDatabaseConnected()
        {
            var status = dbConnection != null;
            if (status)
            {
                dbConnection.Close();
            }
            return status;
        }
        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            {
                Console.WriteLine("Cant connect (FireCommand) Sketchbot");
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, dbConnection);
            
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }
        public void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }
        public static void CloseAllConnections()
        {
            MySqlConnection.ClearAllPools();
        }
        public static List<string> CheckExistingUser(IGuildUser user) // TODO refactor to boolean return
        {
            var result = new List<string>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` WHERE user_id = '{0}'", user.Id, realguildid.ToString());
            var userTable = database.FireCommand(str);
            
            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];

                result.Add(userId);
            }
            database.CloseConnection();
            return result;
        }
        public static void CreateSettingsTable()
        {
            var database = new Database();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `server_settings` (id varchar(50) PRIMARY KEY, prefix varchar(50) DEFAULT '?', welcomechannel varchar(50) DEFAULT NULL, modlogchannel varchar(50) DEFAULT NULL, xpmultiplier int DEFAULT 1, LevelupMessages int DEFAULT 1)");
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void UpdateStats(BotStats stats)
        {
            var database = new Database();
            try
            {
                var strings = string.Format("UPDATE `stats` SET servers = {0}, users = '{1}', msg_since_startup = '{2}', msg_per_min = '{3}', startup_time = '{4}', cmd_since_startup = '{5}', cmd_per_min = '{6}'", stats.Servers, stats.Users, stats.MsgSinceStartup, stats.MsgPerMin, $"{stats.StartUpTime.Year}-{stats.StartUpTime.Month}-{stats.StartUpTime.Day} {stats.StartUpTime.Hour}:{stats.StartUpTime.Minute}:{stats.StartUpTime.Second}", stats.CmdsSinceStartup, stats.CmdsPerMin);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
            }
            catch(Exception ex)
            {
                database.CloseConnection();
                Console.WriteLine(ex);
            }
            return;
        }
        public static void UpdateProfilePicture(string url1, string url2)
        {
            var database = new Database();
            try
            {
                var strings = string.Format("UPDATE `stats` SET tao_avatar = '{0}', tjamp_avatar = '{1}'", url1, url2);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
            }
            catch(Exception ex)
            {
                database.CloseConnection();
                Console.WriteLine(ex);
            }
        }
        public static void EnterUser(IGuildUser user)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("INSERT INTO `{1}` (user_id, tokens, daily, level, xp ) VALUES ('{0}', '100', '0001-01-01 00:00:00', '1', '1')", user.Id, realguildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void CreateTable(ulong guildid)
        {
            var database = new Database();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}` (user_id varchar(50), tokens bigint(20), daily datetime DEFAULT '0001-01-01 00:00:00', level bigint(20) DEFAULT '1', xp bigint(20) DEFAULT '1', PRIMARY KEY (user_id))", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static List<UserStats> GetUserStats(IGuildUser user)
        {
            var result = new List<UserStats>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` WHERE user_id = '{0}'", user.Id, realguildid);
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (ulong)userTable["user_id"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new UserStats
                {
                    UserId = userId,
                    Tokens = currentTokens,
                    Daily = daily,
                    Level = level,
                    XP = xp
                });
            }
            database.CloseConnection();

            return result;

        }
        public static List<UserStats> GetAllUserStats(IGuildUser user)
        {
            var result = new List<UserStats>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("SELECT * FROM `{0}` ORDER BY tokens DESC LIMIT 10000", realguildid);
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (ulong)userTable["user_id"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new UserStats
                {
                    UserId = userId,
                    Tokens = currentTokens,
                    Daily = daily,
                    Level = level,
                    XP = xp
                });
            }
            database.CloseConnection();

            return result;
        }
        public static List<UserStats> GetAllUsersLeveling(IGuildUser user)
        {
            var result = new List<UserStats>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("SELECT * FROM `{0}` ORDER BY level DESC, xp DESC LIMIT 10000;", realguildid);
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (ulong)userTable["user_id"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new UserStats
                {
                    UserId = userId,
                    Tokens = currentTokens,
                    Daily = daily,
                    Level = level,
                    XP = xp
                });
            }
            database.CloseConnection();

            return result;
        }
        public static List<Blacklist> GetAllBlacklistedUsers()
        {
            var result = new List<Blacklist>();
            var database = new Database();
            var str = string.Format("SELECT * FROM blacklist ORDER BY user_id DESC LIMIT 10000");
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (ulong)userTable["user_id"];
                var reason = (string)userTable["reason"];
                var blacklister = (string)userTable["blacklister"];

                result.Add(new Blacklist
                {
                    UserId = userId,
                    Reason = reason,
                    Blacklister = blacklister
                });
            }
            database.CloseConnection();

            return result;
        }
        public static void AddXP(IGuildUser user, long xp)
        {
            var realguildid = user.Guild.Id;
            var database = new Database();
            try
            {
                var strings = string.Format("UPDATE `{0}` SET xp = xp + {1} WHERE user_id = {2}", realguildid, xp, user.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                database.CloseConnection();
                return;
            }
        }
        public static void LevelUp(IGuildUser user, long xp, long level)
        {
            var realguildid = user.Guild.Id;
            var database = new Database();
            try
            {
                var strings = string.Format("UPDATE `{0}` SET level = level + {3}, xp = xp + {1} WHERE user_id = '{2}'", realguildid, xp, user.Id, level);
                var reader = database.FireCommand(strings);
                reader.Close(); 
                database.CloseConnection();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                database.CloseConnection();
                return;
            }
        }
        public static void AddTokens(IGuildUser user, long tokens)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            try
            {
                var strings = string.Format("UPDATE `{2}` SET tokens = tokens + '{1}' WHERE user_id = '{0}'", user.Id, tokens, realguildid);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                database.CloseConnection();
                return;
            }
        }
        public static void RemoveTokens(IGuildUser user, long tokens)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            try
            {
                var strings = string.Format("UPDATE `{2}` SET tokens = tokens - '{1}' WHERE user_id = '{0}'", user.Id, tokens, realguildid);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                database.CloseConnection();
                return;
            }
        }
        public static void BlacklistAdd(RestUser user, string reason, IUser blacklister)
        {
            var database = new Database();

            var str = string.Format("INSERT INTO blacklist (user_id, username, reason, blacklister ) VALUES ('{0}', '{1}', '{2}', '{3}')", user.Id, HelperFunctions.CapitalizeFirstLetter(user.Username), reason, HelperFunctions.CapitalizeFirstLetter(blacklister.Username));
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void BlacklistDel(ulong Id)
        {
            var database = new Database();

            var str = string.Format("DELETE FROM blacklist WHERE (user_id ) = '{0}'", Id);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static List<Blacklist> BlacklistCheck(ulong Id)
        {
            var result = new List<Blacklist>();
            var database = new Database();

            var str = string.Format("SELECT * FROM blacklist WHERE user_id = '{0}'", Id);
            var blacklist = database.FireCommand(str);

            while (blacklist.Read())
            {
                var userId = (ulong)blacklist["user_id"];
                var userName = (string)blacklist["username"];
                var reason = (string)blacklist["reason"];
                var blacklister = (string)blacklist["blacklister"];

                result.Add(new Blacklist
                {
                    UserId = userId,
                    Username = userName,
                    Reason = reason,
                    Blacklister = blacklister
                });
            }
            database.CloseConnection();
            return result;
        }
        public static void UpdateDailyTimestamp(IGuildUser user)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            try
            {
                var strings = string.Format("UPDATE `{0}` SET daily = curtime() WHERE user_id = '{1}'", realguildid, user.Id);
                var reader = database.FireCommand(strings);
                reader.Close();
                database.CloseConnection();
            }
            catch (Exception)
            {
                database.CloseConnection();
            }
        }
        public static void DeleteUser(IGuildUser user)
        {
            var guildid = user.Guild.Id;
            var database = new Database();

            var str = string.Format("DELETE FROM `{1}` WHERE (user_id ) = '{0}'", user.Id, guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
    }
}
