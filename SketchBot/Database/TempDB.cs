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
    public class TempDB
    {
        private string server;
        private string database;
        private string username;
        private string password;
        private MySqlConnection? dbConnection;
        
        public TempDB()
        {
            Config config = Config.Load();
            server = config.TempDBHost;
            database = config.TempDBDatabase;
            username = config.TempDBUsername;
            password = config.TempDBPassword;
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = server;
            stringBuilder.UserID = username;
            stringBuilder.Password = password;
            stringBuilder.Database = database;
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
        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            {
                Console.WriteLine("Cant connect (FireCommand) TempDB");
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
        public static List<string> CheckExistingUser(IUser user)
        {
            var result = new List<string>();
            var database = new TempDB();
            var realguildid = (user as IGuildUser).Guild.Id;
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
        public static void UpdateStats(BotStats stats)
        {
            var database = new TempDB(); /*Sets up a connection to the database*/
            try
            {
                var strings = string.Format("UPDATE `sketchstats` SET servers = {0}, users = '{1}', msg_since_startup = '{2}', msg_per_min = '{3}', startup_time = '{4}'", stats.Servers, stats.Users, stats.MsgSinceStartup, stats.MsgPerMin, $"{stats.StartUpTime.Year}-{stats.StartUpTime.Month}-{stats.StartUpTime.Day} {stats.StartUpTime.Hour}:{stats.StartUpTime.Minute}:{stats.StartUpTime.Second}"); /*This is your SQL string*/
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
            var database = new TempDB();
            try
            {
                var strings = string.Format("UPDATE `sketchstats` SET tao_avatar = '{0}', tjamp_avatar = '{1}'", url1, url2); /*This is your SQL string*/
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
        public static string EnterUser(IGuildUser user)
        {
            //Console.WriteLine($"Entering User {user} {user.Guild.Name}");
            var database = new TempDB();
            var realguildid = user.Guild.Id;
            var str = string.Format("INSERT INTO `{2}` (user_id, username, tokens, daily, level, xp ) VALUES ('{0}', '{1}', '100', '0001-01-01 00:00:00', '1', '1')", user.Id, HelperFunctions.StripUnicodeCharactersFromStringWithMatches(user.Username).Replace(";","").Replace("'",""), realguildid.ToString());
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }
        public static string CreateTable(string guildid)
        {
            var database = new TempDB();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}` (user_id varchar(50), username varchar(100), tokens bigint(20), daily datetime DEFAULT '0001-01-01 00:00:00', level bigint(20) DEFAULT '1', xp bigint(20) DEFAULT '1', PRIMARY KEY (user_id))", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }
        public static List<UserStats> GetUserStatus(IUser user)
        {
            var result = new List<UserStats>();
            var database = new TempDB();
            var realguildid = (user as IGuildUser).Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` WHERE user_id = '{0}'", user.Id, realguildid.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var userName = (string)userTable["username"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new UserStats
                {
                    UserId = userId,
                    Username = userName,
                    Tokens = currentTokens,
                    Daily = daily,
                    Level = level,
                    XP = xp
                });
            }
            database.CloseConnection();

            return result;

        }
        public static List<UserStats> GetAllUsersTokens(IUser user)
        {
            //int pagelimit = numberOfPositions-numberOfPositions+10*numberOfPositions-10;
            var result = new List<UserStats>();
            //Console.WriteLine("Getting all users");
            var database = new TempDB();
            var realguildid = (user as IGuildUser).Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` ORDER BY tokens DESC LIMIT 10000", user.Id, realguildid.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var userName = (string)userTable["username"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new UserStats
                {
                    UserId = userId,
                    Username = userName,
                    Tokens = currentTokens,
                    Daily = daily,
                    Level = level,
                    XP = xp
                });
            }
            database.CloseConnection();

            return result;
        }
        public static List<UserStats> GetAllUsersLeveling(IUser user)
        {
            //int pagelimit = numberOfPositions - numberOfPositions + 10 * numberOfPositions - 10;
            var result = new List<UserStats>();
            //Console.WriteLine("Getting all users");
            var database = new TempDB();
            var realguildid = (user as IGuildUser).Guild.Id;
            var str = string.Format("SELECT * FROM `{1}` ORDER BY level DESC, xp DESC LIMIT 10000;", user.Id, realguildid.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var userName = (string)userTable["username"];
                var currentTokens = (long)userTable["tokens"];
                var daily = (DateTime)userTable["daily"];
                var level = (long)userTable["level"];
                var xp = (long)userTable["xp"];

                result.Add(new UserStats
                {
                    UserId = userId,
                    Username = userName,
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
            //Console.WriteLine("Getting all users");
            var database = new TempDB();
            var str = string.Format("SELECT * FROM blacklist ORDER BY user_id DESC LIMIT 10000");
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var userId = (string)userTable["user_id"];
                var userName = (string)userTable["username"];
                var reason = (string)userTable["reason"];
                var blacklister = (string)userTable["blacklister"];

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
        public static void addXP(IUser user, long xp)/*Creates a new method with IUser and int xp as its params*/
        {
            var realguildid = (user as IGuildUser).Guild.Id;
            var database = new TempDB(); /*Sets up a connection to the database*/
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
        public static void levelUp(IUser user, long xp, long level)
        {
            var realguildid = (user as IGuildUser).Guild.Id;
            var database = new TempDB();
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
        public static void ChangeTokens(IUser user, long tokens)
        {
            var database = new TempDB();
            var realguildid = (user as IGuildUser).Guild.Id;
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
        public static void RemoveTokens(IUser user, long tokens)
        {
            var database = new TempDB();
            var realguildid = (user as IGuildUser).Guild.Id;
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
        public static string BlacklistAdd(RestUser user, string reason, IUser blacklister)
        {
            var database = new TempDB();

            var str = string.Format("INSERT INTO blacklist (user_id, username, reason, blacklister ) VALUES ('{0}', '{1}', '{2}', '{3}')", user.Id, user.Username, reason, blacklister.Username);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }
        public static string BlacklistDel(ulong Id)
        {
            var database = new TempDB();

            var str = string.Format("DELETE FROM blacklist WHERE (user_id ) = " + Id, Id);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }
        public static List<Blacklist> BlacklistCheck(ulong Id)
        {
            var result = new List<Blacklist>();
            var database = new TempDB();

            var str = string.Format("SELECT * FROM blacklist WHERE user_id = '{0}'", Id);
            var blacklist = database.FireCommand(str);

            while (blacklist.Read())
            {
                var userId = (string)blacklist["user_id"];
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
        public static void ChangeDaily(IUser user)
        {
            var database = new TempDB();
            string realguildid = (user as IGuildUser).Guild.Id.ToString();
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
            var guildid = (user as IGuildUser).Guild.Id.ToString();
            var database = new TempDB();

            var str = string.Format("DELETE FROM `{1}` WHERE (user_id ) = " + user.Id, user.Id, guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
    }
}
