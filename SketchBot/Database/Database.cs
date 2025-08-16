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

        private MySqlConnection dbConnection;
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


            var connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            var ping = dbConnection.Ping();
            if (ping)
            {
                dbConnection.Open();
            }
            else
            {
                Console.WriteLine("Cant connect to database");
                dbConnection = null;
            }
        }

        public bool isDatabaseConnected()
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
                Console.WriteLine("Cant connect (FireCommand)");
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, dbConnection);
            
            var mySqlReader = command.ExecuteReader();
            //Console.WriteLine($"Riskage Database: {queryCount}\n{query}");
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
        public static List<string> CheckExistingUser(IGuildUser user)
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
            var database = new Database(); /*Sets up a connection to the database*/
            try
            {
                var strings = string.Format("UPDATE `stats` SET servers = {0}, users = '{1}', msg_since_startup = '{2}', msg_per_min = '{3}', startup_time = '{4}', cmd_since_startup = '{5}', cmd_per_min = '{6}'", stats.Servers, stats.Users, stats.MsgSinceStartup, stats.MsgPerMin, $"{stats.StartUpTime.Year}-{stats.StartUpTime.Month}-{stats.StartUpTime.Day} {stats.StartUpTime.Hour}:{stats.StartUpTime.Minute}:{stats.StartUpTime.Second}", stats.CmdsSinceStartup, stats.CmdsPerMin); /*This is your SQL string*/
                var reader = database.FireCommand(strings);/*fires the command*/
                reader.Close(); /*Closes the reader*/
                database.CloseConnection(); /*Closes the connection*/
            }
            catch(Exception ex)
            {
                database.CloseConnection(); /*Closes the connection*/
                Console.WriteLine(ex);
            }
            return;
        }
        public static void UpdateProfilePicture(string url1, string url2)
        {
            var database = new Database();
            try
            {
                var strings = string.Format("UPDATE `stats` SET tao_avatar = '{0}', tjamp_avatar = '{1}'", url1, url2); /*This is your SQL string*/
                var reader = database.FireCommand(strings);/*fires the command*/
                reader.Close(); /*Closes the reader*/
                database.CloseConnection(); /*Closes the connection*/
            }
            catch(Exception ex)
            {
                database.CloseConnection();
                Console.WriteLine(ex);
            }
        }
        public static void EnterUser(IGuildUser user)
        {
            //Console.WriteLine($"Entering User {user} {user.Guild.Name}");
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("INSERT INTO `{1}` (user_id, tokens, daily, level, xp ) VALUES ('{0}', '100', '0001-01-01 00:00:00', '1', '1')", user.Id, realguildid.ToString());
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void CreateTable(string guildid)
        {
            var database = new Database();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}` (user_id varchar(50), tokens bigint(20), daily datetime DEFAULT '0001-01-01 00:00:00', level bigint(20) DEFAULT '1', xp bigint(20) DEFAULT '1', PRIMARY KEY (user_id))", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static List<userTable> GetUserStatus(IGuildUser user)
        {
            var result = new List<userTable>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` WHERE user_id = '{0}'", user.Id, realguildid.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new userTable
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
        public static List<userTable> GetAllUsersTokens(IGuildUser user)
        {
            //int pagelimit = numberOfPositions-numberOfPositions+10*numberOfPositions-10;
            var result = new List<userTable>();
            //Console.WriteLine("Getting all users");
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` ORDER BY tokens DESC LIMIT 10000", user.Id, realguildid.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new userTable
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
        public static List<userTable> GetAllUsersLeveling(IGuildUser user)
        {
            //int pagelimit = numberOfPositions - numberOfPositions + 10 * numberOfPositions - 10;
            var result = new List<userTable>();
            //Console.WriteLine("Getting all users");
            var database = new Database();
            var realguildid = user.Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` ORDER BY level DESC, xp DESC LIMIT 10000;", user.Id, realguildid.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new userTable
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
        public static List<blacklist> GetAllBlacklistedUsers()
        {
            var result = new List<blacklist>();
            //Console.WriteLine("Getting all users");
            var database = new Database();
            var str = string.Format("SELECT * FROM blacklist ORDER BY user_id DESC LIMIT 10000");
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var reason = (string)userTable["reason"];
                var blacklister = (string)userTable["blacklister"];

                result.Add(new blacklist
                {
                    UserId = userId,
                    Reason = reason,
                    Blacklister = blacklister
                });
            }
            database.CloseConnection();

            return result;
        }
        public static void addXP(IGuildUser user, long xp)/*Creates a new method with IUser and int xp as its params*/
        {
            var realguildid = user.Guild.Id;
            var database = new Database(); /*Sets up a connection to the database*/
            try /*Tries this*/
            {
                var strings = string.Format("UPDATE `{0}` SET xp = xp + {1} WHERE user_id = {2}",realguildid.ToString(),xp,user.Id.ToString()); /*This is your SQL string*/
                var reader = database.FireCommand(strings);/*fires the command*/
                reader.Close(); /*Closes the reader*/
                database.CloseConnection(); /*Closes the connection*/
                return;
            }
            catch (Exception ex)/*Catches any errors*/
            {
                Console.WriteLine(ex);
                database.CloseConnection(); /*Closes the connection if there is any errors*/
                return;
            }
        }
        public static void levelUp(IGuildUser user, long xp, long level)
        {
            var realguildid = user.Guild.Id;
            var database = new Database();
            try/*Tries the following code*/
            {
                var strings = string.Format("UPDATE `{0}` SET level = level + {3}, xp = xp + {1} WHERE user_id = {2}", realguildid.ToString(), xp, user.Id.ToString(), level);/*this is your sql string*/
                var reader = database.FireCommand(strings); /*Fires the command*/
                reader.Close(); /*Closes the reader*/
                database.CloseConnection(); /*Closes the connections*/
                return;
            }
            catch (Exception e)/*Catches any errors*/
            {
                Console.WriteLine(e);
                database.CloseConnection(); /*Closes the connection*/
                return;
            }
        }
        public static void ChangeTokens(IGuildUser user, long tokens)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            try
            {
                var strings = string.Format("UPDATE `{2}` SET tokens = tokens + '{1}' WHERE user_id = {0}", user.Id, tokens, realguildid.ToString());
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
                var strings = string.Format("UPDATE `{2}` SET tokens = tokens - '{1}' WHERE user_id = {0}", user.Id, tokens, realguildid.ToString());
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

            var str = string.Format("INSERT INTO blacklist (user_id, username, reason, blacklister ) VALUES ('{0}', '{1}', '{2}', '{3}')", user.Id, user.Username, reason, blacklister.Username);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void BlacklistDel(ulong Id)
        {
            var database = new Database();

            var str = string.Format("DELETE FROM blacklist WHERE (user_id ) = " + Id, Id);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static List<blacklist> BlacklistCheck(ulong Id)
        {
            var result = new List<blacklist>();
            var database = new Database();

            var str = string.Format("SELECT * FROM blacklist WHERE user_id = '{0}'", Id);
            var blacklist = database.FireCommand(str);

            while (blacklist.Read())
            {
                var userId = (string)blacklist["user_id"];
                var userName = (string)blacklist["username"];
                var reason = (string)blacklist["reason"];
                var blacklister = (string)blacklist["blacklister"];

                result.Add(new blacklist
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
        public static void ChangeDaily(IGuildUser user)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            try
            {
                var strings = string.Format($"UPDATE `{realguildid}` SET daily = curtime() WHERE user_id = '{user.Id}'");
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
            Console.WriteLine("Deleting user");
            var guildid = user.Guild.Id.ToString();
            var database = new Database();

            var str = string.Format("DELETE FROM `{1}` WHERE (user_id ) = " + user.Id, user.Id, guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
    }
}
