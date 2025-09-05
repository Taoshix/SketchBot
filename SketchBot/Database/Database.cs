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
        public MySqlConnection? dbConnection;
        private Config config;

        public Database(bool shouldRunSetup = false)
        {
            config = Config.Load();
            var stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = config.DatabaseHost,
                UserID = config.DatabaseUsername,
                Password = config.DatabasePassword,
                SslMode = MySqlSslMode.Disabled,
                Pooling = false,
                AllowPublicKeyRetrieval = true
            };

            // Create the database if it doesn't exist
            if (shouldRunSetup)
            {
                var connectionStringNoDb = stringBuilder.ToString();
                using (var tempConnection = new MySqlConnection(connectionStringNoDb))
                {
                    tempConnection.Open();
                    using (var cmd = new MySqlCommand("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'sketchbot'", tempConnection))
                    {
                        var exists = cmd.ExecuteScalar() != null;
                        if (!exists)
                        {
                            using (var createCmd = new MySqlCommand("CREATE DATABASE `sketchbot`", tempConnection))
                            {
                                createCmd.ExecuteNonQuery();
                                Console.WriteLine("Created new sketchbot database");
                            }
                        }
                    }
                }
            }

            stringBuilder.Database = "sketchbot";
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
        public static bool CheckExistingUser(IGuildUser user)
        {
            var result = new List<string>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var query = $"SELECT * FROM `{realguildid}` WHERE user_id = @UserId";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                using (var userTable = cmd.ExecuteReader())
                {
                    while (userTable.Read())
                    {
                        var userId = (string)userTable["user_id"];
                        result.Add(userId);
                    }
                }
            }
            database.CloseConnection();
            return result.Count > 0;
        }
        public static void CreateSettingsTable()
        {
            var database = new Database();
            var str = @"
                CREATE TABLE IF NOT EXISTS `server_settings` (
                    id varchar(50) PRIMARY KEY,
                    prefix varchar(50) DEFAULT '?',
                    welcomechannel varchar(50) DEFAULT NULL,
                    modlogchannel varchar(50) DEFAULT NULL,
                    xpmultiplier int DEFAULT 1,
                    LevelupMessages int DEFAULT 1
                )";
            var table = database.FireCommand(str);
            database.CloseConnection();
        }
        public static void CreateBlacklistTable()
        {
            var database = new Database();
            var str = @"
                CREATE TABLE IF NOT EXISTS `blacklist` (
                    user_id varchar(50) PRIMARY KEY,
                    username varchar(50),
                    reason TEXT,
                    blacklister varchar(50)
                )";
            var table = database.FireCommand(str);
            database.CloseConnection();
        }
        public static void CreateStatsTable()
        {
            var database = new Database();
            var str = @"
                CREATE TABLE IF NOT EXISTS `stats` (
                    `servers` INT(10) NOT NULL DEFAULT '0',
                    `users` INT(10) NOT NULL DEFAULT '0',
                    `msg_since_startup` INT(10) NOT NULL DEFAULT '0',
                    `msg_per_min` FLOAT NOT NULL DEFAULT '0',
                    `startup_time` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    `cmd_since_startup` INT(10) NOT NULL DEFAULT '0',
                    `cmd_per_min` FLOAT NOT NULL DEFAULT '0',
                    `tao_avatar` TINYTEXT NULL DEFAULT NULL,
                    `tjamp_avatar` TINYTEXT NULL DEFAULT NULL
                )";
            var table = database.FireCommand(str);
            database.CloseConnection();
        }
        public static void UpdateStats(BotStats stats)
        {
            var database = new Database();
            try
            {
                var query = "UPDATE `stats` SET servers = @Servers, users = @Users, msg_since_startup = @MsgSinceStartup, msg_per_min = @MsgPerMin, startup_time = @StartUpTime, cmd_since_startup = @CmdsSinceStartup, cmd_per_min = @CmdsPerMin";
                using (var cmd = new MySqlCommand(query, database.dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Servers", stats.Servers);
                    cmd.Parameters.AddWithValue("@Users", stats.Users);
                    cmd.Parameters.AddWithValue("@MsgSinceStartup", stats.MsgSinceStartup);
                    cmd.Parameters.AddWithValue("@MsgPerMin", stats.MsgPerMin);
                    cmd.Parameters.AddWithValue("@StartUpTime", stats.StartUpTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@CmdsSinceStartup", stats.CmdsSinceStartup);
                    cmd.Parameters.AddWithValue("@CmdsPerMin", stats.CmdsPerMin);
                    cmd.ExecuteNonQuery();
                }
                database.CloseConnection();
            }
            catch (Exception ex)
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
                var query = "UPDATE `stats` SET tao_avatar = @Url1, tjamp_avatar = @Url2";
                using (var cmd = new MySqlCommand(query, database.dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Url1", url1);
                    cmd.Parameters.AddWithValue("@Url2", url2);
                    cmd.ExecuteNonQuery();
                }
                database.CloseConnection();
            }
            catch (Exception ex)
            {
                database.CloseConnection();
                Console.WriteLine(ex);
            }
        }
        public static void EnterUser(IGuildUser user)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            var query = $@"INSERT INTO `{realguildid}` (user_id, tokens, daily, level, xp) VALUES (@UserId, @Tokens, @Daily, @Level, @XP)";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                cmd.Parameters.AddWithValue("@Tokens", 100);
                cmd.Parameters.AddWithValue("@Daily", "0001-01-01 00:00:00");
                cmd.Parameters.AddWithValue("@Level", 1);
                cmd.Parameters.AddWithValue("@XP", 1);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static void CreateTable(ulong guildid)
        {
            var database = new Database();
            var str = $@"
                CREATE TABLE IF NOT EXISTS `{guildid}` (
                    user_id varchar(50),
                    tokens bigint(20),
                    daily datetime DEFAULT '0001-01-01 00:00:00',
                    level bigint(20) DEFAULT '1',
                    xp bigint(20) DEFAULT '1',
                    PRIMARY KEY (user_id)
                )";
            var table = database.FireCommand(str);
            database.CloseConnection();
        }
        public static UserStats? GetUserStats(IGuildUser user)
        {
            var result = new List<UserStats>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var query = $"SELECT * FROM `{realguildid}` WHERE user_id = @UserId";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                using (var userTable = cmd.ExecuteReader())
                {
                    while (userTable.Read())
                    {
                        var userId = Convert.ToUInt64(userTable["user_id"]);
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
                }
            }
            database.CloseConnection();
            return result.FirstOrDefault();
        }
        public static List<UserStats> GetAllUserStats(IGuildUser user)
        {
            var result = new List<UserStats>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var query = $"SELECT * FROM `{realguildid}` ORDER BY tokens DESC LIMIT 10000";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                using (var userTable = cmd.ExecuteReader())
                {
                    while (userTable.Read())
                    {
                        var userId = Convert.ToUInt64(userTable["user_id"]);
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
                }
            }
            database.CloseConnection();
            return result;
        }
        public static List<UserStats> GetAllUsersLeveling(IGuildUser user)
        {
            var result = new List<UserStats>();
            var database = new Database();
            var realguildid = user.Guild.Id;
            var query = $"SELECT * FROM `{realguildid}` ORDER BY level DESC, xp DESC LIMIT 10000";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                using (var userTable = cmd.ExecuteReader())
                {
                    while (userTable.Read())
                    {
                        var userId = Convert.ToUInt64(userTable["user_id"]);
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
                }
            }
            database.CloseConnection();
            return result;
        }
        public static List<Blacklist> GetAllBlacklistedUsers()
        {
            var result = new List<Blacklist>();
            var database = new Database();
            var query = "SELECT * FROM blacklist ORDER BY user_id DESC LIMIT 10000";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                using (var userTable = cmd.ExecuteReader())
                {
                    while (userTable.Read())
                    {
                        var userId = Convert.ToUInt64(userTable["user_id"]);
                        var reason = (string)userTable["reason"];
                        var blacklister = (string)userTable["blacklister"];

                        result.Add(new Blacklist
                        {
                            UserId = userId,
                            Reason = reason,
                            Blacklister = blacklister
                        });
                    }
                }
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
                var query = $"UPDATE `{realguildid}` SET xp = xp + @XP WHERE user_id = @UserId";
                using (var cmd = new MySqlCommand(query, database.dbConnection))
                {
                    cmd.Parameters.AddWithValue("@XP", xp);
                    cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                    cmd.ExecuteNonQuery();
                }
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
                var query = $"UPDATE `{realguildid}` SET level = level + @Level, xp = xp + @XP WHERE user_id = @UserId";
                using (var cmd = new MySqlCommand(query, database.dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Level", level);
                    cmd.Parameters.AddWithValue("@XP", xp);
                    cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                    cmd.ExecuteNonQuery();
                }
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
                var query = $"UPDATE `{realguildid}` SET tokens = tokens + @Tokens WHERE user_id = @UserId";
                using (var cmd = new MySqlCommand(query, database.dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Tokens", tokens);
                    cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                    cmd.ExecuteNonQuery();
                }
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
                var query = $"UPDATE `{realguildid}` SET tokens = tokens - @Tokens WHERE user_id = @UserId";
                using (var cmd = new MySqlCommand(query, database.dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Tokens", tokens);
                    cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                    cmd.ExecuteNonQuery();
                }
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
            var query = "INSERT INTO blacklist (user_id, username, reason, blacklister) VALUES (@UserId, @Username, @Reason, @Blacklister)";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                cmd.Parameters.AddWithValue("@Username", HelperFunctions.CapitalizeFirstLetter(user.Username));
                cmd.Parameters.AddWithValue("@Reason", reason);
                cmd.Parameters.AddWithValue("@Blacklister", HelperFunctions.CapitalizeFirstLetter(blacklister.Username));
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static void BlacklistDel(ulong Id)
        {
            var database = new Database();
            var query = "DELETE FROM blacklist WHERE user_id = @UserId";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", Id.ToString());
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static Blacklist? BlacklistCheck(ulong Id)
        {
            var result = new List<Blacklist>();
            var database = new Database();
            var query = "SELECT * FROM blacklist WHERE user_id = @UserId";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", Id.ToString());
                using (var blacklist = cmd.ExecuteReader())
                {
                    while (blacklist.Read())
                    {
                        var userId = Convert.ToUInt64(blacklist["user_id"]);
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
                }
            }
            database.CloseConnection();
            return result.FirstOrDefault();
        }
        public static void UpdateDailyTimestamp(IGuildUser user)
        {
            var database = new Database();
            var realguildid = user.Guild.Id;
            try
            {
                var query = $"UPDATE `{realguildid}` SET daily = curtime() WHERE user_id = @UserId";
                using (var cmd = new MySqlCommand(query, database.dbConnection))
                {
                    cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                    cmd.ExecuteNonQuery();
                }
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
            var query = $"DELETE FROM `{guildid}` WHERE user_id = @UserId";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@UserId", user.Id.ToString());
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
            return;
        }
    }
}
