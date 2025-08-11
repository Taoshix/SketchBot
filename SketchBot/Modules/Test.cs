using Discord;
//using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using Sketch_Bot.Custom_Preconditions;
using YouTubeSearch;
using OsuSharp;
using System.Diagnostics;
//using OsuSharp.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using ImgFlip4NET;
using Discord.Interactions;

namespace Sketch_Bot.Modules
{
    public class Test : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionService _service;
        private MemeService _service2;
        private CachingService _cachingService;

        public Test(InteractionService service, MemeService service2, CachingService service3)           /* Create a constructor for the InteractionService dependency */
        {
            _service = service;
            _service2 = service2;
            _cachingService = service3;
        }
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("addrole", "Add a role for leveling")]
        public async Task Addrole(IRole role, int level)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.CreateTableRole(Context.Guild.Id.ToString());
            ServerSettingsDB.AddRole(Context.Guild.Id.ToString(), role.Id.ToString(), level);
            await FollowupAsync(role.Name + " has been added! If anyone reaches level " + level + " they will recieve the role!");
        }
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("removerole", "Remove a role for leveing")]
        public async Task Removerole(IRole role)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.CreateTableRole(Context.Guild.Id.ToString());
            ServerSettingsDB.RemoveRole(Context.Guild.Id.ToString(), role.Id.ToString());
            await FollowupAsync(role.Name + " has been removed");
        }
        [SlashCommand("youtube", "Searches YouTube and returns the first result")]
        public async Task youtube(string searchquery)
        {
            await DeferAsync();
            var items = new VideoSearch();
            var item = items.GetVideos(searchquery, 1);
            string url = item.Result.First().getUrl();
            await FollowupAsync(url);
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("roleinfo", "Displays info about a role")]
        public async Task roleinfo(IRole role)
        {
            await DeferAsync();
            var rolelist = role.Permissions.ToList();
            string roleliststring = String.Join("\n", rolelist);
            var embed = new EmbedBuilder()
            {
                Color = role.Color
            };
            embed.Title = ("Role info for " + role.Name);
            embed.AddField("Id", role.Id);
            embed.AddField("Position", role.Position, true);
            embed.AddField("Members", ((SocketRole)role).Members.Count(), true);
            embed.AddField("Mentionable?", role.IsMentionable);
            embed.AddField("Hoisted?",role.IsHoisted, true);
            embed.AddField("Permissions", roleliststring);
            embed.AddField("Color", role.Color, true);
            embed.AddField("Role creation date", role.CreatedAt.DateTime.ToString("dd/MM/yy HH:mm:ss"), true);
            var builtEmbed = embed.Build();
            await FollowupAsync("", embed: builtEmbed);
        }
        [SlashCommand("rune", "Rune............")]
        public async Task rune()
        {
            await DeferAsync();
            await FollowupAsync("The man of 2017. The hero we don't need, but deserve.");
        }
        [RequireContext(ContextType.Guild)]
        [UserCommand("stats")]
        //[Alias("level","profile")]
        public async Task userstatus(IUser user)
        {
            await DeferAsync();
            if (user.IsBot)
            {
                await FollowupAsync("Bots don't have stats");
                return;
            }
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Color = new Discord.Color(0, 0, 255)
            };
            var name = (user as IGuildUser).Nickname ?? user.Username;
            Database.CreateTable(Context.Guild.Id.ToString());
            var result = Database.CheckExistingUser(user as IGuildUser);

            if (!result.Any())
            {
                Database.EnterUser(user as IGuildUser);
            }

            var userTable = Database.GetUserStatus(user as IGuildUser) ?? throw new ArgumentNullException("Database.GetUserStatus(user)");
            embed.Title = "Stats for " + name;
            embed.Description = userTable.FirstOrDefault().Tokens + " tokens:small_blue_diamond:" +
                "\nLevel " + userTable.FirstOrDefault().Level +
                "\nXP " + userTable.FirstOrDefault().XP + " out of " + XP.caclulateNextLevel(userTable.FirstOrDefault().Level);
            var builtEmbed = embed.Build();
            await FollowupAsync("", [builtEmbed]);
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("setwelcome", "Sets the welcome channel for welcome messages")]
        public async Task setwelcome()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var channel = Context.Channel;
            ServerSettingsDB.SetWelcomeChannel(channel.Id.ToString(), Context.Guild.Id.ToString());
            await FollowupAsync("This will be the new welcome channel 👍");
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("unsetwelcome", "Disables welcome messages")]
        public async Task unsetwelcome()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.SetWelcomeChannel("(NULL)", Context.Guild.Id.ToString());
            await FollowupAsync("Welcome messages has been disabled");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("disablelevelmsg", "Disables level up messages")]
        public async Task disableleveling()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.UpdateLevelupMessagesBool(Context.Guild.Id.ToString(), 0);
            await FollowupAsync("Levelup messages are now disabled!");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("enablelevelmsg", "Enables level up messages")]
        public async Task enableleveling()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.UpdateLevelupMessagesBool(Context.Guild.Id.ToString(), 1);
            await FollowupAsync("Levelup messages are now enabled!");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("setmodlog", "Sets the modlog channel")]
        public async Task setmodlog()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            if (((IGuildUser) Context.User).GuildPermissions.ManageChannels)
            {
                var channel = Context.Channel;
                ServerSettingsDB.SetModlogChannel(channel.Id.ToString(), Context.Guild.Id.ToString());
                await FollowupAsync("This will be the new mod-log channel 👍");
            }
            else
            {
                await FollowupAsync("You don't have `ManageChannels` permission");
            }
        }
        [SlashCommand("duck", "Posts a random picture of a dog")]
        public async Task duck()
        {
            await DeferAsync();
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))//This is like the 'webbrowser' (?)
            {
                string websiteUrl = "https://random-d.uk/api/v1/random";
                client.BaseAddress = new Uri(websiteUrl);
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string catImage = json["url"].ToString();
                await FollowupAsync(catImage);
            }
        }
        [SlashCommand("dog", "Posts a random picture of a dog")]
        public async Task dog()
        {
            await DeferAsync();
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))//This is like the 'webbrowser' (?)
            {
                string websiteurl = "https://random.dog/woof.json";
                client.BaseAddress = new Uri(websiteurl);
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string catImage = json["url"].ToString();
                await FollowupAsync(catImage);
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("unsetmodlog", "Disables the mod logging")]
        public async Task unsetmodlog()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            if (((IGuildUser) Context.User).GuildPermissions.ManageChannels || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                ServerSettingsDB.SetModlogChannel("(NULL)", Context.Guild.Id.ToString());
                await FollowupAsync("Mod-log disabled");
            }
            else
            {
                await FollowupAsync("You don't have `ManageChannels` permission");
            }
        }
        /*
        [Ratelimit(1,3, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers)]
        [SlashCommand("osu", "Lookup osu! stats of a user")]
        public async Task osu(string gamemode, string user)
        {
            await DeferAsync();
            try
            {
                string gamemodename;
                IOsuApi instance = new OsuApi(new OsuSharpConfiguration
                {
                    ApiKey = File.ReadAllText("osu api key.json"),
                    ModsSeparator = "|",
                    MaxRequests = 4,
                    TimeInterval = TimeSpan.FromSeconds(8),
                    LogLevel = LoggingLevel.Debug
                });
                var osuuser = await instance.GetUserByNameAsync(user, GameMode.Standard);
                if (gamemode.ToLower() == "0" || gamemode.ToLower() == "std" || gamemode.ToLower() == "standard")
                {
                    osuuser = await instance.GetUserByNameAsync(user, GameMode.Standard);
                    gamemodename = "Standard";
                }
                else if(gamemode.ToLower() == "1" || gamemode.ToLower() == "taiko")
                {
                    osuuser = await instance.GetUserByNameAsync(user, GameMode.Taiko);
                    gamemodename = "Taiko";
                }
                else if(gamemode.ToLower() == "2" || gamemode.ToLower() == "ctb" || gamemode.ToLower() == "catchthebeat")
                {
                    osuuser = await instance.GetUserByNameAsync(user, GameMode.Catch);
                    gamemodename = "CTB";
                }
                else if(gamemode.ToLower() == "3" || gamemode.ToLower() == "mania")
                {
                    osuuser = await instance.GetUserByNameAsync(user, GameMode.Mania);
                    gamemodename = "Mania";
                }
                else
                {
                    await FollowupAsync("Invalid gamemode\nTry std taiko ctb mania");
                    return;
                }
                if (osuuser != null)
                {
                    var roundlevel = Math.Floor(osuuser.Level);
                    var rawlevel = osuuser.Level;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Color = new Discord.Color(0, 0, 255),
                        ThumbnailUrl = $"https://a.ppy.sh/{osuuser.Userid}",
                        Description =
                        $"▸ **Global Rank:** #{(osuuser.GlobalRank).ToString("N0", new NumberFormatInfo() { NumberGroupSizes = new[] { 3 }, NumberGroupSeparator = "." })} ({osuuser.Country}#{(osuuser.RegionalRank).ToString("N0", new NumberFormatInfo() { NumberGroupSizes = new[] { 3 }, NumberGroupSeparator = "." })})" +
                        $"\n▸ **Level:** {Math.Floor(osuuser.Level)} ({Math.Round((rawlevel - roundlevel) * 100, 2)}%)" +
                        $"\n▸ **Total PP:** {osuuser.Pp}" +
                        $"\n▸ **Accuracy:** {Math.Round(osuuser.Accuracy, 2)}%" +
                        $"\n▸ **Playcount:** {osuuser.PlayCount}" +
                        $"\n▸ **Total Score:** {(osuuser.TotalScore).ToString("N0", new NumberFormatInfo() { NumberGroupSizes = new[] { 3 }, NumberGroupSeparator = "." })}" +
                        $"\n▸ **Ranked Score:** {(osuuser.RankedScore).ToString("N0", new NumberFormatInfo() { NumberGroupSizes = new[] { 3 }, NumberGroupSeparator = "." })}" +
                        $"\n\n**Play history:**",
                        ImageUrl = $"https://osu.ppy.sh/pages/include/profile-graphactivity.php?_jpg_csimd=1&u={osuuser.Userid}"
                    };
                    builder.WithAuthor(auther =>
                    {
                        auther.Name = $"osu! {gamemodename} Profile for {osuuser.Username}";
                        auther.IconUrl = $"https://osu.ppy.sh/images/flags/{osuuser.Country}.png";
                        auther.Url = $"https://osu.ppy.sh/u/{osuuser.Userid}";
                    });
                    var embed = builder.Build();
                    await FollowupAsync("", embed:embed);
                }
                else
                {
                    await FollowupAsync("User not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                await FollowupAsync($"Something went wrong! ({ex.GetType().ToString()})" +
                    $"\n{ex.Message}");
            }
        }
        [SlashCommand("osutop", "Top 10 osu plays of a user")]
        public async Task osutop(string gamemode, string user)
        {
            await DeferAsync();
            try
            {
                string gamemodename;
                IOsuApi instance = new OsuApi(new OsuSharpConfiguration
                {
                    ApiKey = File.ReadAllText("osu api key.json"),
                    ModsSeparator = "|",
                    MaxRequests = 4,
                    TimeInterval = TimeSpan.FromSeconds(8),
                    LogLevel = LoggingLevel.Debug
                });
                List<UserBest> bests = await instance.GetUserBestByUsernameAsync(user, GameMode.Standard, 5);
                if (gamemode.ToLower() == "0" || gamemode.ToLower() == "std" || gamemode.ToLower() == "standard")
                {
                    bests = await instance.GetUserBestByUsernameAsync(user, GameMode.Standard, 10);
                    gamemodename = "Standard";
                }
                else if (gamemode.ToLower() == "1" || gamemode.ToLower() == "taiko")
                {
                    bests = await instance.GetUserBestByUsernameAsync(user, GameMode.Taiko, 10);
                    gamemodename = "Taiko";
                }
                else if (gamemode.ToLower() == "2" || gamemode.ToLower() == "ctb" || gamemode.ToLower() == "catchthebeat" || gamemode.ToLower() == "catch")
                {
                    bests = await instance.GetUserBestByUsernameAsync(user, GameMode.Catch, 10);
                    gamemodename = "CTB";
                }
                else if (gamemode.ToLower() == "3" || gamemode.ToLower() == "mania")
                {
                    bests = await instance.GetUserBestByUsernameAsync(user, GameMode.Mania, 10);
                    gamemodename = "Mania";
                }
                else
                {
                    await FollowupAsync("Invalid gamemode\nTry std taiko ctb mania");
                    return;
                }
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithAuthor(auther =>
                {
                    auther.Name = $"Top 10 {gamemodename} scores for {bests.FirstOrDefault().Username}";
                });
                
                int cnt = 1;
                foreach (UserBest best in bests)
                {

                    builder.AddField($"{cnt}. https://osu.ppy.sh/b/{best.BeatmapId}",$"**Score:** {best.ScorePoints}" +
                        $"\n**Accuracy:** {Math.Round(best.Accuracy, 2)}" +
                        $"\n**Max Combo:** {best.MaxCombo}x" +
                        $"\n**PP:** {best.Pp}" +
                        $"\n**FC?** {best.Perfect}" +
                        $"\n**Misses:** {best.Miss}x");
                    cnt++;
                }
                var embed = builder.Build();
                await FollowupAsync("", embed:embed);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Something went wrong! ({ex.GetType().ToString()})");
                Console.WriteLine(ex.StackTrace);
            }
        }
        */
        [SlashCommand("activity", "Launch a discord activity in a voice channel!")]
        public async Task Activity(IVoiceChannel chan, DefaultApplications app)
        {
            await DeferAsync();
            var invite = await chan.CreateInviteToApplicationAsync(app);
            await Context.Interaction.FollowupAsync(invite.Url);
        }
        [SlashCommand("emote", "Enlargens an emote")]
        public async Task emote(string emote)
        {
            await DeferAsync();
            var emo = Emote.Parse(emote);
            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = emo.Name,
                ImageUrl = emo.Url,
                Url = emo.Url
            };
            var embed = builder.Build();
            await FollowupAsync("", embed:embed);
        }
        
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("slowmode", "Sets the slowmode of a channel to the input seconds")]
        public async Task slowmode(int seconds)
        {
            if (seconds < 21600)
            {
                await ((ITextChannel) Context.Channel).ModifyAsync(x => x.SlowModeInterval = seconds);
                await Context.Channel.SendMessageAsync($"Slowmode is now set to {seconds} seconds");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Interval must be less than or equal to 6 hours.");
            }
        }
        [Ratelimit(1, 2, Measure.Seconds, RatelimitFlags.None)]
        [SlashCommand("memegen", "Generates a meme")]
        public async Task MemeAsync(string templateName, string topText, string bottomText)
        {
            await DeferAsync();
            var service = _service2.GetMemeService();
            var template = await service.GetMemeTemplateAsync(templateName);
            if (template == null)
            {
                await FollowupAsync("Template not found.\nhttps://api.imgflip.com/popular_meme_ids");
            }
            else
            {
                var meme = await service.CreateMemeAsync(template.Id, topText, bottomText);
                await FollowupAsync(meme.ImageUrl);
            }
        }
    }
}
