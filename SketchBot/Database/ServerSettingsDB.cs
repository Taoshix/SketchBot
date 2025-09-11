using Discord;
using Discord.WebSocket;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sketch_Bot.Models;
using Newtonsoft.Json;
using System.IO;

namespace Sketch_Bot
{
    public class ServerSettingsDB
    {
        public MySqlConnection? dbConnection;
        private Config config;
        public ServerSettingsDB(bool shouldRunSetup = false)
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
                try
                {
                    var connectionStringNoDb = stringBuilder.ToString();
                    using var tempConnection = new MySqlConnection(connectionStringNoDb);
                    tempConnection.Open();
                    using var cmd = new MySqlCommand("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'server settings'", tempConnection);
                    var exists = cmd.ExecuteScalar() != null;
                    if (!exists)
                    {
                        using var createCmd = new MySqlCommand("CREATE DATABASE `server settings`", tempConnection);
                        createCmd.ExecuteNonQuery();
                        Console.WriteLine("Created new server settings database");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during database setup: {ex.Message}");
                }
            }

            stringBuilder.Database = "server settings";
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
                Console.WriteLine("Cant connect (FireCommand) Server Settings");
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
        public static void CreateTableRole(ulong guildid)
        {
            var database = new ServerSettingsDB();
            var str = $"CREATE TABLE IF NOT EXISTS `{guildid}_roles` (roleId varchar(50), level int, PRIMARY KEY (roleId))";
            var table = database.FireCommand(str);
            database.CloseConnection();
        }
        public static void CreateTableWords(ulong guildid)
        {
            var database = new ServerSettingsDB();
            var str = $"CREATE TABLE IF NOT EXISTS `{guildid}_words` (words text)";
            var table = database.FireCommand(str);
            database.CloseConnection();
        }
        public static Serversettings MakeSettings(ulong guildid, int levelup)
        {
            var database = new Database();
            var query = "INSERT INTO `server_settings` (id, prefix, welcomechannel, modlogchannel, xpmultiplier, LevelupMessages) VALUES (@Id, '?', 0, 0, 1, @Levelup)";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Id", guildid);
                cmd.Parameters.AddWithValue("@Levelup", levelup);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
            Serversettings settings = new Serversettings
            {
                GuildId = guildid,
                Prefix = "?",
                WelcomeChannel = 0,
                ModlogChannel = 0,
                XpMultiplier = 1,
                LevelupMessages = levelup == 1
            };
            return settings;
        }
        public static void UpdateAllTables(ulong guilid)
        {
            var database = new ServerSettingsDB();
            var str = $"ALTER TABLE `{guilid}` ADD COLUMN `LevelupMessages` INT NOT NULL DEFAULT '1' AFTER `xpmultiplier`; ";
            var table = database.FireCommand(str);
            database.CloseConnection();
        }
        public static void UpdateXprate(ulong guildid, int rate)
        {
            var database = new Database();
            var query = "UPDATE `server_settings` SET xpmultiplier = @Rate WHERE id = @Id";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Rate", rate);
                cmd.Parameters.AddWithValue("@Id", guildid);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
            return;
        }
        public static void AddWord(ulong guildid, string words)
        {
            var database = new ServerSettingsDB();
            var query = $"INSERT INTO `{guildid}_words` (words) VALUES (@Words)";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Words", words);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static void AddRole(ulong guildid, ulong roleId, int level)
        {
            var database = new ServerSettingsDB();
            var query = $"INSERT INTO `{guildid}_roles` (roleId, level) VALUES (@RoleId, @Level)";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.Parameters.AddWithValue("@Level", level);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static void RemoveRole(ulong guildid, ulong roleId)
        {
            var database = new ServerSettingsDB();
            var query = $"DELETE FROM `{guildid}_roles` WHERE roleId = @RoleId";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static void DelWord(ulong guildid, string words)
        {
            var database = new ServerSettingsDB();
            var query = $"DELETE FROM `{guildid}_words` WHERE words = @Words";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Words", words);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static void UpdatePrefix(ulong guildid, string newprefix)
        {
            var database = new Database();
            var query = "UPDATE `server_settings` SET prefix = @Prefix WHERE id = @Id";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Prefix", newprefix);
                cmd.Parameters.AddWithValue("@Id", guildid);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
            return;
        }
        public static Serversettings? GetSettings(ulong guildid)
        {
            var result = new List<Serversettings>();
            var database = new Database();
            var query = "SELECT * FROM `server_settings` WHERE id = @Id";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Id", guildid);
                using var table = cmd.ExecuteReader();
                while (table.Read())
                {
                    var prefix = table["prefix"] == DBNull.Value ? string.Empty : (string)table["prefix"];
                    var welcomechannel = table["welcomechannel"] == DBNull.Value ? 0ul : Convert.ToUInt64(table["welcomechannel"]);
                    var modlogchannel = table["modlogchannel"] == DBNull.Value ? 0ul : Convert.ToUInt64(table["modlogchannel"]);
                    var xprate = table["xpmultiplier"] == DBNull.Value ? 1 : (int)table["xpmultiplier"];
                    var levelupmessages = table["LevelupMessages"] == DBNull.Value ? 1 : (int)table["LevelupMessages"];

                    result.Add(new Serversettings
                    {
                        GuildId = guildid,
                        Prefix = prefix,
                        ModlogChannel = modlogchannel,
                        WelcomeChannel = welcomechannel,
                        XpMultiplier = xprate,
                        LevelupMessages = levelupmessages == 1
                    });
                }
            }
            database.CloseConnection();
            return result.FirstOrDefault();
        }
        public static void UpdateLevelupMessagesBool(ulong guildid, int boool)
        {
            var database = new Database();
            var query = "UPDATE `server_settings` SET LevelupMessages = @Bool WHERE id = @Id";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Bool", boool);
                cmd.Parameters.AddWithValue("@Id", guildid);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
            return;
        }
        public static void SetWelcomeChannel(ulong guildid, ulong channelId)
        {
            var database = new Database();
            var query = "UPDATE `server_settings` SET welcomechannel = @WelcomeChannel WHERE id = @Id";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@WelcomeChannel", channelId);
                cmd.Parameters.AddWithValue("@Id", guildid);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static void SetModlogChannel(ulong guildid, ulong channelId)
        {
            var database = new Database();
            var query = "UPDATE `server_settings` SET modlogchannel = @ModlogChannel WHERE id = @Id";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@ModlogChannel", channelId);
                cmd.Parameters.AddWithValue("@Id", guildid);
                cmd.ExecuteNonQuery();
            }
            database.CloseConnection();
        }
        public static List<Serversettings> GetRole(ulong guildid, long level)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var query = $"SELECT * FROM `{guildid}_roles` WHERE level = @Level";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                cmd.Parameters.AddWithValue("@Level", level);
                using var table = cmd.ExecuteReader();
                while (table.Read())
                {
                    var roleId = Convert.ToUInt64(table["roleId"]);
                    result.Add(new Serversettings
                    {
                        RoleId = roleId,
                    });
                }
            }
            database.CloseConnection();
            return result;
        }
        public static List<Serversettings> GetRoles(ulong guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var query = $"SELECT * FROM `{guildid}_roles`";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                using var table = cmd.ExecuteReader();
                while (table.Read())
                {
                    var roleId = Convert.ToUInt64(table["roleId"]);
                    var roleLevel = (int)table["level"];
                    result.Add(new Serversettings
                    {
                        RoleId = roleId,
                        RoleLevel = roleLevel
                    });
                }
            }
            database.CloseConnection();
            return result;
        }
        public static List<Serversettings> GetWords(ulong guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var query = $"SELECT * FROM `{guildid}_words`";
            using (var cmd = new MySqlCommand(query, database.dbConnection))
            {
                using var table = cmd.ExecuteReader();
                while (table.Read())
                {
                    var words = (string)table["words"];
                    result.Add(new Serversettings
                    {
                        Words = words,
                    });
                }
            }
            database.CloseConnection();
            return result;
        }
    }
}