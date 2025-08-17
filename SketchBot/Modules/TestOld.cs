using Discord;
using Discord.Commands;
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
using System.Diagnostics;
using OsuSharp.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using ImgFlip4NET;

namespace Sketch_Bot.Modules
{
    public class TestOld : ModuleBase<ICommandContext>
    {
        private CommandService _service;
        private MemeService _service2;
        private CachingService _service3;

        public TestOld(CommandService service, MemeService service2, CachingService service3)           /* Create a constructor for the commandservice dependency */
        {
            _service = service;
            _service2 = service2;
            _service3 = service3;
        }
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("addrole")]
        [Summary("add a role for leveling")]
        public async Task addrole(string type, SocketRole role, int level)
        {
            if (type.ToLower() == "leveling")
            {
                ServerSettingsDB.CreateTableRole(Context.Guild.Id.ToString());
                ServerSettingsDB.AddRole(Context.Guild.Id.ToString(), role.Id.ToString(), level);
                await ReplyAsync(role.Name + " has been added! If anyone reaches level " + level + " they will recieve the role!");
            }
            else
            {
                await ReplyAsync("Usage: ?addrole leveling <role> <level>");
            }
        }
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("removerole")]
        public async Task removerole(string type, SocketRole role)
        {
            if (type.ToLower() == "leveling") //if i ever wanted shoplist to work, i would have to add a new if statement here
            {
                ServerSettingsDB.CreateTableRole(Context.Guild.Id.ToString());
                ServerSettingsDB.RemoveRole(Context.Guild.Id.ToString(), role.Id.ToString());
                await ReplyAsync(role.Name + " has been removed");
            }
            else
            {
                await ReplyAsync("Usage: ?removerole leveling <role>");
            }
        }
        [Command("youtube", RunMode = RunMode.Async)]
        [Alias("yt")]
        public async Task youtube([Remainder] string searchquery)
        {
            var items = new VideoSearch();
            var item = items.GetVideos(searchquery, 1);
            string url = item.Result.First().getUrl();
            await ReplyAsync(url);
        }
        [Command("allcommands", RunMode = RunMode.Async)]
        [Remarks("Shows a list of all available commands per module.")]
        public async Task HelpAsync()
        {
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                string prefix = ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault().Prefix;  /* put your chosen prefix here */
                var builder = new EmbedBuilder()
                {
                    Color = new Discord.Color(114, 137, 218),
                    Description = "These are the commands you can use"
                };

                foreach (var module in _service.Modules) /* we are now going to loop through the modules taken from the service we initiated earlier ! */
                {
                    string description = null;
                    foreach (var cmd in module.Commands) /* and now we loop through all the commands per module aswell, oh my! */
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context); /* gotta check if they pass */
                        if (result.IsSuccess)
                            description += $"{prefix}{cmd.Aliases.First()}\n"; /* if they DO pass, we ADD that command's first alias (aka it's actual name) to the description tag of this embed */
                    }

                    if (!string.IsNullOrWhiteSpace(description)) /* if the module wasn't empty, we go and add a field where we drop all the data into! */
                    {
                        builder.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                            x.IsInline = false;
                        });
                    }

                }
                await Context.Channel.SendMessageAsync("", false, builder.Build()); /* then we send it to the user. */
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("roleinfo", RunMode = RunMode.Async)]
        public async Task roleinfo([Remainder] SocketRole role)
        {
            var rolelist = role.Permissions.ToList();
            string roleliststring = String.Join("\n", rolelist);
            var embed = new EmbedBuilder()
            {
                Color = role.Color
            };
            embed.Title = ("Role info for " + role.Name);
            embed.AddField("Id", role.Id);
            embed.AddField("Position", role.Position, true);
            embed.AddField("Members", role.Members.Count(), true);
            embed.AddField("Mentionable?", role.IsMentionable);
            embed.AddField("Hoisted?", role.IsHoisted, true);
            embed.AddField("Permissions", roleliststring);
            embed.AddField("Color", role.Color, true);
            embed.AddField("Role creation date", role.CreatedAt.DateTime.ToString("dd/MM/yy HH:mm:ss"), true);
            var builtEmbed = embed.Build();
            await ReplyAsync("", false, builtEmbed);
        }
        [Command("rune")]
        public async Task rune()
        {
            await ReplyAsync("The man of 2017. The hero we don't need, but deserve.");
        }
        [RequireContext(ContextType.Guild)]
        [Command("stats", RunMode = RunMode.Async)]
        [Alias("level", "profile")]
        public async Task userstatus([Remainder] IGuildUser user = null)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Discord.Color(0, 0, 255)
            };
            if (user == null)
            {
                user = Context.User as IGuildUser;
            }
            var name = user.Nickname ?? user.Username;
            Database.CreateTable(Context.Guild.Id.ToString());
            var result = Database.CheckExistingUser(user);

            if (!result.Any())
            {
                Database.EnterUser(user);
            }

            var userTable = Database.GetUserStatus(user) ?? throw new ArgumentNullException("Database.GetUserStatus(user)");
            embed.Title = ("Stats for " + name);
            embed.Description = (userTable.FirstOrDefault().Tokens + " tokens:small_blue_diamond:" +
                "\nLevel " + userTable.FirstOrDefault().Level +
                "\nXP " + userTable.FirstOrDefault().XP + " out of " + XP.caclulateNextLevel(userTable.FirstOrDefault().Level));
            var builtEmbed = embed.Build();
            await Context.Channel.SendMessageAsync("", false, builtEmbed);
        }

        [RequireContext(ContextType.Guild)]
        [Command("setwelcome", RunMode = RunMode.Async)]
        public async Task setwelcome()
        {
            if (((IGuildUser)Context.User).GuildPermissions.ManageChannels || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var channel = Context.Channel;
                ServerSettingsDB.SetWelcomeChannel(channel.Id.ToString(), Context.Guild.Id.ToString());
                await ReplyAsync("This will be the new welcome channel 👍");
            }
            else
            {
                await ReplyAsync("You don't have `ManageChannels` permission");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("unsetwelcome", RunMode = RunMode.Async)]
        public async Task unsetwelcome()
        {
            if (((IGuildUser)Context.User).GuildPermissions.ManageChannels || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var channel = Context.Channel;
                ServerSettingsDB.SetWelcomeChannel("(NULL)", Context.Guild.Id.ToString());
                await ReplyAsync("Welcome messages has been disabled");
            }
            else
            {
                await ReplyAsync("You don't have `ManageChannels` permission");
            }
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("banword", RunMode = RunMode.Async)]
        public async Task banword([Remainder] string word)
        {
            if (word.Contains("'") || word.Contains(";") || word.Contains("#"))
            {
                await ReplyAsync("I don't like that character!");
            }
            else
            {
                await ReplyAsync("The word `" + word + "` has been banned and will now be deleted when written");
                var words = _service3.GetBadWords(Context.Guild.Id);
                words.Add(word);
                _service3.UpdateBadWords(Context.Guild.Id, words);
                ServerSettingsDB.AddWord(Context.Guild.Id.ToString(), word);
            }
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("unbanword", RunMode = RunMode.Async)]
        public async Task unbanword([Remainder] string word)
        {
            if (word.Contains("'") || word.Contains(";") || word.Contains("#"))
            {
                await ReplyAsync("I don't like that character!");
            }
            else
            {
                ServerSettingsDB.DelWord(Context.Guild.Id.ToString(), word);
                var words = _service3.GetBadWords(Context.Guild.Id);
                words.Remove(word);
                _service3.UpdateBadWords(Context.Guild.Id, words);
                await ReplyAsync("The word `" + word + "` has been unbanned and will no longer be deleted when written");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("bannedwords", RunMode = RunMode.Async)]
        public async Task bannedwordlist()
        {
            var words = ServerSettingsDB.GetWords(Context.Guild.Id.ToString());
            var bannedWords = words.Select(x => x.Words);
            if (words.Any())
            {
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Title = $"List of banned words on {Context.Guild.Name}",
                    Description = $"{string.Join("\n", bannedWords)}",
                    Color = new Discord.Color(255, 0, 0)
                };
                var embed = builder.Build();
                await ReplyAsync("", false, embed);
            }
            else
            {
                await ReplyAsync("No words banned on this server");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("welcomechannel", RunMode = RunMode.Async)]
        public async Task welcomechannel()
        {
            var userTable = ServerSettingsDB.GetSettings(Context.Guild.Id.ToString());
            var channel = userTable.FirstOrDefault()?.WelcomeChannel;
            if (channel == "(NULL)" || channel == null)
            {
                await ReplyAsync("Welcome channel is not enabled. use `?setwelcome` in a channel to enable it");
            }
            else
            {
                var parsedChannel = await Context.Guild.GetTextChannelAsync(UInt64.Parse(channel));
                await ReplyAsync(parsedChannel.Mention);
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("modlogchannel", RunMode = RunMode.Async)]
        public async Task modlogchannel()
        {
            var usertable = ServerSettingsDB.GetSettings(Context.Guild.Id.ToString());
            var channel = usertable.FirstOrDefault()?.ModlogChannel;
            if (channel == "(NULL)" || string.IsNullOrEmpty(channel))
            {
                await ReplyAsync("Mod-log is not enabled. use `?setmodlog` in a channel to enable it");
            }
            else
            {
                var parsedchannel = await Context.Guild.GetTextChannelAsync(ulong.Parse(channel));
                await ReplyAsync(parsedchannel.Mention);
            }
        }
        [Command("gamble life", RunMode = RunMode.Async)]
        public async Task Danniboi()
        {
            Random rand = new Random();
            var rng = rand.Next(0, 100);
            if (rng >= 55)
            {
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Title = "You won!",
                    Description = $"You gambled your life and won!" +
                    $"\nHave fun with a life!",
                    Color = new Discord.Color(0, 0, 255)
                }.WithAuthor(auther =>
                {
                    auther.Name = "Gambling results - " + Context.User.Username;
                    auther.WithIconUrl(Context.User.GetAvatarUrl());
                }
                );
                var embed = builder.Build();
                await ReplyAsync("", false, embed);
            }
            else
            {
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Title = "You lost!",
                    Description = $"You gambled your life and lost!\n" +
                    $"Have fun with no life!",
                    Color = new Discord.Color(0, 0, 255)
                }.WithAuthor(auther =>
                {
                    auther.Name = "Gambling results - " + Context.User.Username;
                    auther.WithIconUrl(Context.User.GetAvatarUrl());
                }
                );
                var embed = builder.Build();
                await ReplyAsync("", false, embed);
            }
        }
        
        [RequireContext(ContextType.Guild)]
        [Command("DisableLevelMsg", RunMode = RunMode.Async)]
        public async Task disableleveling()
        {
            ServerSettingsDB.UpdateLevelupMessagesBool(Context.Guild.Id.ToString(), 0);
            await ReplyAsync("Levelup messages are now disabled!");
        }
        [RequireContext(ContextType.Guild)]
        [Command("EnableLevelMsg", RunMode = RunMode.Async)]
        public async Task enableleveling()
        {
            ServerSettingsDB.UpdateLevelupMessagesBool(Context.Guild.Id.ToString(), 1);
            await ReplyAsync("Levelup messages are now enabled!");
        }
        [RequireContext(ContextType.Guild)]
        [Command("setmodlog", RunMode = RunMode.Async)]
        public async Task setmodlog()
        {
            if (((IGuildUser) Context.User).GuildPermissions.ManageChannels || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var channel = Context.Channel;
                ServerSettingsDB.SetModlogChannel(channel.Id.ToString(), Context.Guild.Id.ToString());
                await ReplyAsync("This will be the new mod-log channel 👍");
            }
            else
            {
                await ReplyAsync("You don't have `ManageChannels` permission");
            }
        }
        [Command("duck",RunMode = RunMode.Async)]
        public async Task duck()
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))//This is like the 'webbrowser' (?)
            {
                string websiteUrl = "https://random-d.uk/api/v1/random";
                client.BaseAddress = new Uri(websiteUrl);
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string catImage = json["url"].ToString();
                await ReplyAsync(catImage);
            }
        }
        [Command("dog", RunMode = RunMode.Async)]
        public async Task dog()
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))//This is like the 'webbrowser' (?)
            {
                string websiteurl = "https://random.dog/woof.json";
                client.BaseAddress = new Uri(websiteurl);
                HttpResponseMessage response = client.GetAsync("").Result;
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string catImage = json["url"].ToString();
                await ReplyAsync(catImage);
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("unsetmodlog", RunMode = RunMode.Async)]
        public async Task unsetmodlog()
        {
            if (((IGuildUser) Context.User).GuildPermissions.ManageChannels || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                ServerSettingsDB.SetModlogChannel("(NULL)", Context.Guild.Id.ToString());
                await ReplyAsync("Mod-log disabled");
            }
            else
            {
                await ReplyAsync("You don't have `ManageChannels` permission");
            }
        }
        [Command("emote")]
        public async Task lel(string striing)
        {
            var emo = Emote.Parse(striing);
            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = emo.Name,
                ImageUrl = emo.Url,
                Url = emo.Url
            };
            var embed = builder.Build();
            await ReplyAsync("", false, embed);
        }
        
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Command("slowmode")]
        public async Task slowmode(int time)
        {
            if (time < 21600)
            {
                await ((ITextChannel) Context.Channel).ModifyAsync(x => x.SlowModeInterval = time);
                await Context.Channel.SendMessageAsync($"Slowmode is now set to {time} seconds");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Interval must be less than or equal to 6 hours.");
            }
        }

        [Ratelimit(1, 2, Measure.Seconds, RatelimitFlags.None)]
        [Command("meme", RunMode = RunMode.Async)]
        public async Task MemeAsync([Remainder] string text)
        {
            try
            {
                string memeName = text.Split(",")[0];
                string topText = text.Split(",")[1];
                string bottomText = text.Split(",")[2];
                var service = _service2.GetMemeService();
                var template = await service.GetMemeTemplateAsync(memeName);
                if (template == null)
                {
                    await ReplyAsync("Template not found.\nhttps://api.imgflip.com/popular_meme_ids");
                }
                else
                {
                    var meme = await service.CreateMemeAsync(template.Id, topText, bottomText);
                    await ReplyAsync(meme.ImageUrl);
                }
            }
            catch(IndexOutOfRangeException)
            {
                await ReplyAsync($"Usage: {ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault().Prefix ?? "?"}meme <template name>, <top text>, <bottom text>\nEach argument is seperated by comma ,\nhttps://api.imgflip.com/popular_meme_ids for a list of templates");
            }

        }
    }
}
