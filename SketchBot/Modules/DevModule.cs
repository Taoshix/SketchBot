using Discord;
using Discord.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using Sketch_Bot.Custom_Preconditions;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.IO;
using System.Net;
using Discord.Rest;

namespace Sketch_Bot.Modules
{
    public class DevModule : ModuleBase<SocketCommandContext>
    {
        public static CultureInfo ci = CultureInfo.InvariantCulture;
        Random _rand;
        Stopwatch _stopwatch;

        private DiscordBotsListService _service;
        private TimerService _timerService;
        private StatService _statService;
        private CachingService _cachingService;
        public DevModule(DiscordBotsListService service, TimerService service2, StatService service3, CachingService service4)
        {
            _service = service;
            _timerService = service2;
            _statService = service3;
            _cachingService = service4;
        }

        [RequireDevelopers]
        [Command("eval", RunMode = RunMode.Async)]
        public async Task Eval([Remainder] string code)
        {
            code = code.Replace("```cs", "");
            code = code.Replace("```", "");
            var stopwatch = new Stopwatch();
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    stopwatch.Start();
                    var references = new List<MetadataReference>();
                    var referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
                    foreach (var referencedAssembly in referencedAssemblies)
                        references.Add(MetadataReference.CreateFromFile(Assembly.Load(referencedAssembly).Location));
                    var scriptoptions = ScriptOptions.Default.WithReferences(references);
                    var emoji = new Emoji("✅");
                    GlobalsOld globals = new GlobalsOld { Context = Context, Guild = Context.Guild, DblApi = _service.DblApi() };
                    //object o = await CSharpScript.EvaluateAsync(@"using System;using System.Linq;using System.Threading.Tasks;using System.Collections.Generic;using Discord.WebSocket;using Discord;using System.Net;using System.Net.Http;using OsuSharp;using OsuSharp.UserEndpoint;using OsuSharp.Misc;using OsuSharp.Entities;try{" + @code + "} catch (exception) {}", scriptoptions, globals);
                    object o = await CSharpScript.EvaluateAsync(@"using System;using System.Linq;using System.Threading.Tasks;using System.Collections.Generic;using System.IO;using Discord.WebSocket;using Discord;using System.Net;using System.Net.Http;using DiscordBotsList.Api;" + @code, scriptoptions, globals);
                    stopwatch.Stop();
                    if (o == null)
                    {
                        await Context.Message.AddReactionAsync(emoji);
                        await ReplyAsync($"{stopwatch.ElapsedMilliseconds}ms");
                    }
                    else
                    {
                        await ReplyAsync("", embed: new EmbedBuilder().WithTitle("Result:").WithDescription(o.ToString()).WithFooter(footer => footer.Text = $"{stopwatch.ElapsedMilliseconds}ms").Build());
                    }
                }
                catch (Exception e)
                {
                    var emoji2 = new Emoji("❌");
                    await ReplyAsync("", embed: new EmbedBuilder().WithTitle("Error:").WithDescription($"{e.GetType().ToString()}: {e.Message}\nFrom: {e.Source}").Build());
                    await Context.Message.AddReactionAsync(emoji2);
                }
            }
        }
        [RequireDevelopers]
        [Command("bash", RunMode = RunMode.Async)]
        public async Task bashAsync([Remainder] string cmd)
        {
            var result = HelperFunctions.Bash(cmd);
            await ReplyAsync(result);
        }
        [RequireDevelopers]
        [Command("checkowner")]
        public async Task checkowner(ulong id)
        {
            var guilds = Context.Client.Guilds;
            var guilds2 = guilds.Where(x => x.OwnerId == id).ToList();
            if (guilds2.Any())
            {
                foreach (var guildd in guilds2)
                {
                    await Context.Channel.SendMessageAsync(guildd.Name + "    " + guildd.Id);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("No servers found!");
            }
        }
        [RequireDevelopers]
        [Command("mutualserver")]
        public async Task mutualservers(ulong id)
        {
            var guilds = Context.Client.Guilds;
            var castedguilds = guilds.Cast<SocketGuild>();
            var guilds2 = castedguilds.Where(x => x.Users.FirstOrDefault(z => z.Id == id)?.Id == id).ToList();
            List<string> foundservers = new List<string>();
            if (guilds2.Any())
            {
                foreach (var guildd in guilds2)
                {
                    foundservers.Add(guildd.Name + " - " + guildd.Id);
                }
                var fulltext = string.Join("\n", foundservers);
                var user = await Context.Client.GetUserAsync(id);
                EmbedBuilder builder = new EmbedBuilder()
                {
                    Title = $"{guilds2.Count} mutual servers found",
                    Description = fulltext,
                    Color = new Discord.Color(0, 0, 255),
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = user.Username,
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                await Context.Channel.SendMessageAsync("No servers found!");
            }
        }
        [RequireDevelopers]
        [Command("updatepfp", RunMode = RunMode.Async)]
        public async Task blurple([Remainder] string url = "")
        {
            try
            {
                if (Context.Message.Attachments.Any())
                {
                    url = Context.Message.Attachments.FirstOrDefault().Url;
                }
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(url);
                await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Discord.Image(stream));
            }
            catch (Exception ex)
            {
                await ReplyAsync("Unable to download/verify the URL");
            }
        }
        [RequireDevelopers]
        [Command("im lazy", RunMode = RunMode.Async)]
        public async Task ImLazy()
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }
            await ReplyAsync("Updating all of the tables so you don't have to!");
            var guilds = Context.Client.Guilds;
            foreach (var guild in guilds)
            {
                try
                {
                    ServerSettingsDB.UpdateAllTables(guild.Id);
                    await Task.Delay(50);
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message +
                        "\nCreating table if not exists...");
                    ServerSettingsDB.CreateTableWords(guild.Id);
                    await Task.Delay(50);
                    ServerSettingsDB.MakeSettings(guild.Id, 1);
                    await Task.Delay(50);
                    continue;
                }
            }
            await ReplyAsync("Done!");
        }
        [RequireDevelopers]
        [Command("fix db", RunMode = RunMode.Async)]
        public async Task fixDb()
        {
            Stopwatch s = new Stopwatch();
            await ReplyAsync("Updating Database...");
            s.Start();
            var guilds = Context.Client.Guilds;
            foreach (var guild in guilds)
            {
                try
                {
                    Database.CreateTable(guild.Id);
                    var gottenprefix = ServerSettingsDB.GetSettings(guild.Id);
                    if (!gottenprefix.Any())
                    {
                        ServerSettingsDB.MakeSettings(guild.Id, 1);
                        ServerSettingsDB.CreateTableWords(guild.Id);
                    }
                }
                catch (Exception ex)
                {
                    await EmbedHandler.CreateBasicEmbed(ex.GetType().ToString(), ex.Message, Discord.Color.Red);
                    continue;
                }
            }
            s.Stop();
            await ReplyAsync($"Done {s.ElapsedMilliseconds}ms");
        }
        [RequireDevelopersSilent]
        [Command("serverlist", RunMode = RunMode.Async)]
        public async Task test2(int index = 1)
        {
            int pagelimit = index - index + 20 * index - 20;
            List<string> guildNames = new List<string>();
            var guilds = Context.Client.Guilds;
            foreach (var guild in guilds.Skip(pagelimit).Take(20))
            {
                guildNames.Add(guild.Name + " - " + guild.Id);
            }
            var serverInfo = string.Join("\n", guildNames);
            if (serverInfo.Length > 2000)
            {
                await Context.User.SendMessageAsync("Guild list > 2000 characters");
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.Title = "Server list";
                embed.Description = serverInfo;
                var builtEmbed = embed.Build();
                await Context.User.SendMessageAsync("", false, builtEmbed);
            }
        }
        [RequireDevelopersSilent]
        [Command("serverlist", RunMode = RunMode.Async)]
        public async Task serverlist([Remainder] string search)
        {

            List<string> guildNames = new List<string>();
            var guilds = Context.Client.Guilds;
            foreach (var guild in guilds.Where(x => x.Name.ToLower().Contains(search.ToLower())).Take(20))
            {
                guildNames.Add(guild.Name + " - " + guild.Id);
            }
            var fulltext = string.Join("\n", guildNames);
            if (fulltext.Length > 2000)
            {
                await ReplyAsync("Guild list > 2000 characters");
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.Title = "Server list";
                embed.Description = fulltext;
                var builtEmbed = embed.Build();
                await ReplyAsync("", false, builtEmbed);
            }

        }
        [RequireDevelopers]
        [Command("Reply", RunMode = RunMode.Async)]
        public async Task replyasync(ulong id, [Remainder] string message)
        {
            if (Context.User.Id == 135446225565515776)
            {
                var user = Context.Client.GetUser(id);
                await user.SendMessageAsync(message);
                var embedbuilder = new EmbedBuilder()
                {
                    Title = "Message sent",
                    Description = "Taoshi replied to " + user.Id + " (" + user.Username + ")" +
                    "\n\n" + message,
                    Color = new Color(0, 0, 255)
                };
                var embed = embedbuilder.Build();
                await ReplyAsync("Message sent!");
                await Context.Client.GetUser(208624502878371840).SendMessageAsync("", false, embed);
            }
            else
            {
                var user = Context.Client.GetUser(id);
                await user.SendMessageAsync(message);
                var embedbuilder = new EmbedBuilder()
                {
                    Title = "Message sent",
                    Description = "Tjampen replied to " + user.Id + " (" + user.Username + ")" +
                    "\n\n" + message,
                    Color = new Color(0, 0, 255)
                };
                var embed = embedbuilder.Build();
                await ReplyAsync("Message sent!");
                await Context.Client.GetUser(135446225565515776).SendMessageAsync("", false, embed);
            }

        }
        [RequireDevelopers]
        [Command("blacklist", RunMode = RunMode.Async)]
        public async Task blacklist(RestUser user, [Remainder] string reason = "No reason")
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }
            if (user.Id != 135446225565515776 && user.Id != 208624502878371840)
            {
                if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
                {
                    var result = _cachingService.GetBlackList();
                    if (!result.Contains(user.Id))
                    {
                        Database.BlacklistAdd(user, reason, Context.User);
                        var embedraw = new EmbedBuilder()
                        {
                            Color = new Color(0, 0, 0)
                        };
                        embedraw.Title = "Blacklist";
                        embedraw.Description = user.Mention + " has been blacklisted!" +
                            "\n" +
                            "\nReason: " + reason;
                        var embed = embedraw.Build();
                        await Context.Channel.SendMessageAsync("", false, embed);
                        _cachingService.AddToBlacklist(user.Id);
                    }
                }
                else
                {
                    await ReplyAsync("You have to be a developer to do that!");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("Nice try");
            }
        }
        [RequireDevelopers]
        [Command("blacklistid", RunMode = RunMode.Async)]
        public async Task blacklistid(ulong id, [Remainder] string reason = "No reason")
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }
            var user = await Context.Client.Rest.GetUserAsync(id);
            if (user.Id != 135446225565515776 && user.Id != 208624502878371840)
            {
                if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
                {
                    var result = _cachingService.GetBlackList();
                    if (!result.Contains(user.Id))
                    {
                        Database.BlacklistAdd(user, reason, Context.User);
                        var embedraw = new EmbedBuilder()
                        {
                            Color = new Color(0, 0, 0)
                        };
                        embedraw.Title = ("Blacklist");
                        embedraw.Description = user.Mention + " has been blacklisted!" +
                            "\n" +
                            "\nReason: " + reason;
                        var embed = embedraw.Build();
                        await Context.Channel.SendMessageAsync("", false, embed);
                        _cachingService.AddToBlacklist(user.Id);
                    }

                }
                else
                {
                    await ReplyAsync("You have be a developer to do that!");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("Nice try");
            }
        }
        [RequireDevelopers]
        [Command("unblacklist", RunMode = RunMode.Async)]
        public async Task unblacklist(RestUser user)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var result = _cachingService.GetBlackList();
                if (result.Contains(user.Id))
                {
                    Database.BlacklistDel(user.Id);
                    var embedRaw = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 0)
                    };
                    embedRaw.Title = "Blacklist";
                    embedRaw.Description = user.Mention + " has been removed from the blacklist!";
                    var embed = embedRaw.Build();
                    await Context.Channel.SendMessageAsync("", false, embed);
                    _cachingService.RemoveFromBlacklist(user.Id);
                }
                else
                {
                    var embedRaw = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 0)
                    };
                    embedRaw.Title = "Blacklist";
                    embedRaw.Description = user.Mention + " is not on the blacklist";
                    var embed = embedRaw.Build();
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
            }
            else
            {
                await ReplyAsync("You have be a developer to do that!");
            }
        }
        [RequireDevelopers]
        [Command("unblacklistid", RunMode = RunMode.Async)]
        public async Task unblacklistid(ulong id)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }
            var user = await Context.Client.Rest.GetUserAsync(id);
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var result = _cachingService.GetBlackList();
                if (result.Contains(user.Id))
                {
                    Database.BlacklistDel(id);
                    var embedraw = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 0)
                    };
                    embedraw.Title = "Blacklist";
                    embedraw.Description = user?.Mention ?? id + " has been removed from the blacklist!";
                    var embed = embedraw.Build();
                    await Context.Channel.SendMessageAsync("", false, embed);
                    _cachingService.RemoveFromBlacklist(id);
                }
                else
                {
                    var embedraw = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 0)
                    };
                    embedraw.Title = "Blacklist";
                    embedraw.Description = user?.Mention ?? id + " is not on the blacklist";
                    var embed = embedraw.Build();
                    await Context.Channel.SendMessageAsync("", false, embed);
                }
            }
            else
            {
                await ReplyAsync("You have be a developer to do that!");
            }
        }
        [RequireDevelopers]
        [Command("Blacklistcheck", RunMode = RunMode.Async)]
        public async Task blacklistcheck(RestUser user)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }
            var result = Database.BlacklistCheck(user.Id);
            if (!result.Any())
            {
                var embedraw = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 0)
                };
                embedraw.Title = "Blacklist Check";
                embedraw.Description = user.Username + " is not on the blacklist!";
                var embed = embedraw.Build();
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var embedraw = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 0)
                };
                embedraw.Title = "Blacklist Check";
                embedraw.Description = user.Username + " is blacklisted!" +
                    "\n" +
                    "\n*Reason:* " + result.FirstOrDefault().Reason +
                    "\n\nBlacklisted by " + result.FirstOrDefault().Blacklister;
                var embed = embedraw.Build();
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }
        [RequireDevelopers]
        [Command("Blacklistcheckid", RunMode = RunMode.Async)]
        public async Task blacklistcheckid(ulong id)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }
            var user = await Context.Client.Rest.GetUserAsync(id);
            var result = Database.BlacklistCheck(id);
            if (!result.Any())
            {
                var embedraw = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 0)
                };
                embedraw.Title = "Blacklist Check";
                embedraw.Description = user?.Username ?? id + " is not on the blacklist!";
                var embed = embedraw.Build();
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var embedraw = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 0)
                };
                embedraw.Title = "Blacklist Check";
                embedraw.Description = user?.Username ?? id + " is blacklisted!" +
                    "\n" +
                    "\n*Reason:* " + result.FirstOrDefault().Reason +
                    "\n\nBlacklisted by " + result.FirstOrDefault().Blacklister;
                var embed = embedraw.Build();
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }
    }
}
