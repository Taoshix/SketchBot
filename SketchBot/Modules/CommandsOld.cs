using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UrbanDictionnet;
using System.Diagnostics;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Sketch_Bot.Custom_Preconditions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using DiscordBotsList;
using DiscordBotsList.Api;
using System.Threading;
using System.IO;

namespace Sketch_Bot.Modules
{
    public class CommandsOld : ModuleBase<SocketCommandContext>
    {

        public static
        CultureInfo ci = CultureInfo.InvariantCulture;
        Random _rand;
        Stopwatch _stopwatch;

        public int TotalMembers() => Context.Client.Guilds.Sum(x => x.MemberCount);

        private DiscordBotsListService _service;
        private TimerService _timerService;
        private StatService _statService;
        private CachingService _cachingService;
        public CommandsOld(DiscordBotsListService service, TimerService service2, StatService service3, CachingService service4)
        {
            _service = service;
            _timerService = service2;
            _statService = service3;
            _cachingService = service4;
        }
        [Command("roll")]
        public async Task roll(int min = 1, int max = 100)
        {
            _rand = new Random();
            try
            {
                if (min > max)
                {
                    await Context.Channel.SendMessageAsync("The minimum value must not be over the maximum value!");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?roll and failed! (MinOverMaxException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                }
                else
                {
                    var rng = _rand.Next(min, max);
                    await Context.Channel.SendMessageAsync($"{Context.User.Username} rolled {rng} ({min}-{max})");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?roll with success! and rolled " + rng + "(" + min + "-" + max + ")" + " (" + Context.Guild?.Name ?? "DM" + ")");
                }
            }
            catch (IndexOutOfRangeException)
            {
                await Context.Channel.SendMessageAsync("The number has to be between 0 and 2147483647!");
            }
        }
        [Command("hello")]
        public async Task hello()
        {
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                await Context.Channel.SendMessageAsync("Hello developer! How are you doing today?");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Hi! " + Context.User.Username);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?hello with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("donate")]
        public async Task donate()
        {
            await ReplyAsync("https://www.patreon.com/Sketch_Bot");
        }
        [Command("upvote")]
        public async Task upvote()
        {
            await ReplyAsync("You can upvote the bot here https://discordbots.org/bot/369865463670374400");
            var Api = _service.DblApi();
            if (await Api.HasVoted(Context.User.Id))
            {
                await Context.Channel.SendMessageAsync("Thanks for voting today!");
            }
            else
            {
                await ReplyAsync("You have not voted today");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("gamble", RunMode = RunMode.Async)]
        public async Task gamble(long amount)
        {
            _rand = new Random();
            var currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
            if (amount > currentTokens) await ReplyAsync("You don't have enough tokens");
            else if (amount < 1) await ReplyAsync("The minimum amount of tokens is 1");
            else
            {
                var RNG = _rand.Next(0, 100);
                if (RNG >= 53)
                {
                    Database.ChangeTokens(Context.User as IGuildUser, amount);
                    currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You won!",
                        Description = $"You gambled {amount} tokens and won!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
                    }.WithAuthor(author =>
                    {
                        author.Name = "Gambling results - " + Context.User.Username;
                        author.WithIconUrl(Context.User.GetAvatarUrl());
                    }
                    );
                    var embed = builder.Build();
                    await ReplyAsync("", false, embed);
                }
                else
                {
                    Database.RemoveTokens(Context.User as IGuildUser, amount);
                    currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You lost!",
                        Description = $"You gambled {amount} tokens and lost!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
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
        }
        [RequireContext(ContextType.Guild)]
        [Command("gamble all", RunMode = RunMode.Async)]
        public async Task gambleall()
        {
            long amount = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
            _rand = new Random();
            var currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
            if (amount < 1) await ReplyAsync("The minimum amount of tokens is 1");
            else
            {
                var RNG = _rand.Next(0, 100);
                if (RNG >= 53)
                {
                    Database.ChangeTokens(Context.User as IGuildUser, amount);
                    currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You won!",
                        Description = $"You gambled {amount} tokens and won!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
                    }.WithAuthor(author =>
                    {
                        author.Name = "Gambling results - " + Context.User.Username;
                        author.WithIconUrl(Context.User.GetAvatarUrl());
                    }
                    );
                    var embed = builder.Build();
                    await ReplyAsync("", false, embed);
                }
                else
                {
                    Database.RemoveTokens(Context.User as IGuildUser, amount);
                    currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You lost!",
                        Description = $"You gambled {amount} tokens and lost!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
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
        }
        [Command("ping", RunMode = RunMode.Async)]
        public async Task ping()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            var message = await Context.Channel.SendMessageAsync("hello!");
            _stopwatch.Stop();
            await message.DeleteAsync();
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            embedBuilder.Title = "Pong!";
            embedBuilder.AddField("Gateway latency", Context.Client.Latency + "ms");
            var time = _stopwatch.ElapsedMilliseconds;
            embedBuilder.AddField("Execution time", time + "ms");
            var embed = embedBuilder.Build();
            await Context.Channel.SendMessageAsync("", false, embed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?ping with success! " + Context.Client.Latency + "ms" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("riskage", RunMode = RunMode.Async)]
        public async Task riskage()
        {
            await Context.Channel.SendFileAsync("DAB/riskage.jpg");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?riskage with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("daddy", RunMode = RunMode.Async)]
        public async Task daddy()
        {
            await Context.Channel.SendFileAsync("DAB/Marco.png");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?daddy with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("avatar", RunMode = RunMode.Async)]
        public async Task avatar([Remainder] SocketUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }
            await ReplyAsync(user.GetAvatarUrl(ImageFormat.Auto, 256));
        }
        [Command("8ball", RunMode = RunMode.Async)]
        public async Task eightball([Remainder] string input)
        {
            string[] predictionsTexts = new string[]
            {
                "It is very unlikely.",
                "I don't think so...",
                "Yes !",
                "I don't know",
                "No.",
                "Without a doubt",
                "Pls don't",
                "Just give me your money!"
            };
            _rand = new Random();
            int randomIndex = _rand.Next(predictionsTexts.Length);
            string text = predictionsTexts[randomIndex];
            await ReplyAsync(":8ball: **Question: **" + input + "\n**Answer: **" + text);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?8ball with success! " + "(" + input + ") (" + text + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [Command("kick", RunMode = RunMode.Async)]
        public async Task kick(IGuildUser user, [Remainder] string reason = "No reason")
        {

            if (user != null)
            {
                if (user.Id != 135446225565515776 || user.Id != 208624502878371840 || (Context.Client.CurrentUser as IGuildUser).GuildPermissions.KickMembers)
                {
                    var casenumberstring = System.IO.File.ReadAllText("casenumber.json");
                    var casenumber = Int32.Parse(casenumberstring);
                    var gld = Context.Guild as SocketGuild;
                    var embed = new EmbedBuilder(); ///starts embed///
                    embed.WithColor(new Color(0x4900ff)); ///hexacode colours ///
                    embed.Title = $" {user.Username} has been kicked from {user.Guild.Name}"; ///who was kicked///
                    embed.Description = $"**Username: **{user.Username}" +
                        $"\n**Guild Name: **{user.Guild.Name}" +
                        $"\n**Kicked by: **{Context.User.Mention}!" +
                        $"\n**Reason: **{reason}"; ///embed values///
                    var builtEmbed = embed.Build();
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     "
                        + Context.User.Username + " just ran ?kick with success! and kicked "
                        + (user as IGuildUser).Username + " with the reason: " + (reason) + " (" + Context.Guild.Name + ")");
                    await (user as IGuildUser).KickAsync(reason);
                    await Context.Channel.SendMessageAsync("", false, builtEmbed);///sends embed///
                    if (ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault().ModlogChannel != "(NULL)")
                    {
                        var moderationchannel = Context.Guild.GetTextChannel(UInt64.Parse(ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault()?.ModlogChannel));
                        var embed2 = new EmbedBuilder();
                        embed2.Color = new Color(244, 66, 66);
                        //embed2.WithColor(new Color(0xb72707));
                        embed2.WithAuthor(auther =>
                        { auther.Name = user.Id.ToString(); });
                        embed2.Title = "Kick";
                        embed2.Description = reason;
                        embed2.AddField("Responsible Moderator", Context.User.Username + " (" + Context.User.Mention + ")")
                            .AddField("Victim", user.Username + " (" + user.Mention + ")");
                        var builtEmbed2 = embed2.Build();
                        await moderationchannel.SendMessageAsync("", false, builtEmbed2);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync("I don't have the permission to do so!");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("?kick <user> <reason>");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?kick and failed! (NoTargetException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [Command("ban", RunMode = RunMode.Async)]
        public async Task banAsync(IGuildUser user, [Remainder] string reason = "No reason")
        {
            if (user != null)
            {
                if (user.Id != 135446225565515776 || user.Id != 208624502878371840 || (Context.Client.CurrentUser as IGuildUser).GuildPermissions.BanMembers)
                {

                    var gld = Context.Guild as SocketGuild;
                    var embed = new EmbedBuilder(); ///starts embed///
                    embed.WithColor(new Color(0x4900ff)); ///hexacode colours ///
                    embed.Title = $" {user.Username} has been banned from {user.Guild.Name}"; ///who was kicked///
                    embed.Description = $"**Username: **{user.Username}" +
                        $"\n**Guild Name: **{user.Guild.Name}" +
                        $"\n**banned by: **{Context.User.Mention}!" +
                        $"\n**Reason: **{reason}"; ///embed values///
                    var builtEmbed = embed.Build();
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     "
                        + Context.User.Username + " just ran ?ban with success! and banned "
                        + (user as IGuildUser).Username + " with the reason: " + (reason) + " (" + Context.Guild?.Name ?? "DM" + ")");
                    await Context.Guild.AddBanAsync(user, 7, reason);
                    await Context.Channel.SendMessageAsync("", false, builtEmbed);///sends embed///

                    if (ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault()?.ModlogChannel != "(NULL)")
                    {
                        var moderationchannel = (Context.Guild.GetTextChannel(UInt64.Parse(ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault().ModlogChannel)));
                        var embed2 = new EmbedBuilder();
                        embed2.WithColor(new Color(0xb72707));
                        embed2.WithAuthor(auther =>
                        { auther.Name = user.Id.ToString(); });
                        embed2.Title = "Ban";
                        embed2.Description = reason;
                        embed2.AddField("Responsible Moderator", Context.User.Username + " (" + Context.User.Mention + ")")
                            .AddField("Victim", user.Username + " (" + user.Mention + ")");
                        var builtEmbed2 = embed2.Build();
                        await moderationchannel.SendMessageAsync("", false, builtEmbed2);
                    }

                }
                else
                {
                    await Context.Channel.SendMessageAsync("I don't have the permission to do so!");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("?ban <user> <reason>");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?ban and failed! (NoTargetException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [Command("unban", RunMode = RunMode.Async)]
        public async Task unbanAsync(RestUser user, [Remainder] string reason = "No reason")
        {
            var casenumberstring = System.IO.File.ReadAllText("casenumber.json");
            var casenumber = Int32.Parse(casenumberstring);
            await Context.Guild.RemoveBanAsync(user);
            await ReplyAsync(user.Username + " has been unbanned" +
                "\n" +
                "\n" + reason);
            if (Context.Guild.Id == 380670135045849089)
            {
                var moderationchannel = Context.Guild.GetTextChannel(430639577334808576);
                var embed2 = new EmbedBuilder();
                embed2.WithColor(new Color(0x42f483));
                embed2.WithAuthor(auther =>
                { auther.Name = user.Id.ToString(); });
                embed2.Title = "Unban | Case #" + casenumber;
                embed2.Description = reason;
                embed2.AddField("Responsible Moderator", Context.User.Username + " (" + Context.User.Mention + ")")
                    .AddField("Victim", user.Username + " (" + user.Mention + ")");
                var builtEmbed2 = embed2.Build();
                await moderationchannel.SendMessageAsync("", false, builtEmbed2);
                casenumber++;
                System.IO.File.WriteAllText("casenumber.json", casenumber.ToString());
            }

        }
        [Command("riskage spil", RunMode = RunMode.Async)]
        public async Task riskagespil()
        {
            await Context.Channel.SendMessageAsync("https://scratch.mit.edu/projects/176501177/");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?riskage spil with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("pia", RunMode = RunMode.Async)]
        public async Task piasko()
        {
            await Context.Channel.SendMessageAsync("http://gonzoft.com/spil/dk/games/kaste_sko_pk/sheepspilley.html");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?pia with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("status", RunMode = RunMode.Async)]
        public async Task status(string website = "http://sketchbot.xyz")
        {
            try
            {
                if (!website.StartsWith("https://") && !website.StartsWith("http://"))
                {
                    website = "https://" + website;
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(website);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (HttpStatusCode.OK == response.StatusCode)
                {
                    // Website is Online
                    response.Close();
                    var embed = new EmbedBuilder();
                    embed.Title = ("Status");
                    embed.Description = ($"{website} \n\nserver is **online**");
                    embed.Timestamp = (DateTime.Now);
                    embed.WithFooter(footer =>
                    {
                        footer
                        .WithText("Requested by " + Context.User.Username)
                            .WithIconUrl(Context.User.GetAvatarUrl());
                    });
                    embed.ThumbnailUrl = (Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 1024));
                    var builtEmbed = embed.Build();
                    await Context.Channel.SendMessageAsync("", false, builtEmbed);
                }
                else
                {
                    // Website if Offline
                    response.Close();
                    var embed = new EmbedBuilder();
                    embed.Title = ("Status");
                    embed.Description = ($"{website} \n\nserver is **offline**");
                    embed.Timestamp = (DateTime.Now);
                    embed.WithFooter(footer =>
                    {
                        footer
                        .WithText("Requested by " + Context.User.Username)
                            .WithIconUrl(Context.User.GetAvatarUrl());
                    });
                    embed.ThumbnailUrl = (Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 1024));
                    var builtEmbed = embed.Build();
                    await Context.Channel.SendMessageAsync("", false, builtEmbed);
                }
            }
            catch (WebException)

            {
                // Connection is not available
                var embed = new EmbedBuilder();
                embed.Title = ("Status");
                embed.Description = ($"{website} \n\nconnection is **not available**");
                embed.Timestamp = (DateTime.Now);
                embed.WithFooter(footer =>
                {
                    footer
                    .WithText("Requested by " + Context.User.Username)
                        .WithIconUrl(Context.User.GetAvatarUrl());
                });
                embed.ThumbnailUrl = (Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 1024));
                var builtEmbed = embed.Build();
                await Context.Channel.SendMessageAsync("", false, builtEmbed);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Command     " + Context.User.Username + " just ran ?status with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("vuk", RunMode = RunMode.Async)]
        public async Task vuk()
        {
            await Context.Channel.SendMessageAsync("In memory of Vuk");
            await Context.Channel.SendFileAsync("DAB/vuk.jpg");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?vuk with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireContext(ContextType.Guild)]
        [Command("nickname", RunMode = RunMode.Async)]
        public async Task nickname(params string[] text)
        {
            string newNickname = string.Join(" ", text);
            try
            {
                await (Context.User as IGuildUser)?.ModifyAsync(x => x.Nickname = newNickname);
                await Context.Channel.SendMessageAsync("Your Nickname is now **" + newNickname + "!**");
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Unable to change nickname.");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?nickname with success! (RoleHigherException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [Command("frede", RunMode = RunMode.Async)]
        public async Task frede()
        {
            if (Context.User.Id == 199564148856717312)
            {
                await Context.Channel.SendMessageAsync("You are Frede!");
                await (Context.User as IGuildUser)?.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "FredeDenGrimme"));
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?frede with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
            else
            {
                await Context.Channel.SendMessageAsync("You are not Frede!");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?frede and failed! (NotFredeException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [Custom_Preconditions.Ratelimit(1,1, Custom_Preconditions.Measure.Minutes,Custom_Preconditions.RatelimitFlags.ApplyPerGuild)]
        [Command("count", RunMode = RunMode.Async)]
        public async Task countto(int countto)
        {
            countto = Math.Abs(countto);
            if (countto >= 1 && countto <= 100)
            {
                try
                {
                    TimeSpan t = TimeSpan.FromSeconds(countto);
                    string answer = $"{t.Days:D2}days:{t.Hours:D2}hours:{t.Minutes:D2}min:{t.Seconds:D2}sec";
                    var message = "Counting to " + countto + " | Estimated time until counting is complete: " + answer + " | WARNING ratelimit does occur!";
                    var result = await ReplyAsync(message);
                    await Task.Delay(1000);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?count with success! (" + countto + ") (" + Context.Guild?.Name ?? "DM" + ")");
                    for (int i = 1; i < countto + 1; i++)
                    {
                        int counting = i;
                        await result.ModifyAsync(x => x.Content = counting.ToString());
                        await Task.Delay(1000);
                        //await Context.Channel.SendMessageAsync("" + counting);
                    }
                }
                catch (OverflowException)
                {
                    await Context.Channel.SendMessageAsync("Integer overflow!");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?count and failed! (IntegerOverflowException) (" + Context.Guild?.Name ?? "DM" + ")");
                }
            }
            else if (countto >= 100)
            {
                await ReplyAsync("Max number is 100 for now");
            }
        }
        [Command("scarce", RunMode = RunMode.Async)]
        public async Task scarce()
        {
            if (Context.User.Id == 249316431148089354)
            {
                await Context.Channel.SendMessageAsync("You are DaRealScarce#1234!");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?scarce with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
            else
            {
                await Context.Channel.SendMessageAsync("You are not DaRealScarce#1234!");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?scarce and failed! (NotDaRealScarceException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [Command("cat", RunMode = RunMode.Async)]
        public async Task Cat()
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))//This is like the 'webbrowser' (?)
                {
                    string websiteUrl = "http://aws.random.cat/meow";
                    client.BaseAddress = new Uri(websiteUrl);
                    HttpResponseMessage response = client.GetAsync("").Result;
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(result);
                    string CatImage = json["file"].ToString();
                    await ReplyAsync(CatImage);
                }
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?cat with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
            catch (Exception ex)
            {
                await EmbedHandler.CreateErrorEmbed("cat", ex.ToString());
            }
        }
        [Command("fox", RunMode = RunMode.Async)]
        public async Task fox()
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                ) //This is like the 'webbrowser' (?)
                {
                    var websitee = "https://randomfox.ca/floof/";
                    client.BaseAddress = new Uri(websitee);
                    HttpResponseMessage response = client.GetAsync("").Result;
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(result);
                    string catImage = json["image"].ToString();
                    await ReplyAsync($"{catImage}");
                }
            }
            catch (Exception ex)
            {
                await EmbedHandler.CreateErrorEmbed("fox", ex.ToString());
            }
        }
        [Command("birb", RunMode = RunMode.Async)]
        public async Task birb()
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                ) //This is like the 'webbrowser' (?)
                {
                    var websitee = "https://random.birb.pw/img/";
                    string websiteurl = "http://random.birb.pw/tweet.json/";
                    client.BaseAddress = new Uri(websiteurl);
                    HttpResponseMessage response = client.GetAsync("").Result;
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(result);
                    string CatImage = json["file"].ToString();
                    await ReplyAsync($"{websitee}{CatImage}");
                }
            }
            catch (Exception ex)
            {
                await EmbedHandler.CreateErrorEmbed("birb", ex.ToString());
            }
        }
        [Command("calc+", RunMode = RunMode.Async)]
        public async Task calcplus(params double[] tal)
        {
            await Context.Channel.SendMessageAsync("Answer: " + tal.Sum());
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calc+ with success! (" + tal.Sum() + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        /*[Command("calc",RunMode = RunMode.Async)]
        public async Task calc(string expression)
        {
            var input = Regex.Replace(expression, "", "");
            var result = await CSharpScript.EvaluateAsync(expression);
            await ReplyAsync(result.ToString());
        }*/
        [Command("calcaverage", RunMode = RunMode.Async)]
        public async Task calcaverage(params double[] tal)
        {
            await Context.Channel.SendMessageAsync("Answer: " + tal.Average());
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcaverage with success! (" + tal.Average() + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calc*", RunMode = RunMode.Async)]
        public async Task calcgange(params double[] tal)
        {
            double prod = 1;
            foreach (double value in tal)
            {
                prod *= value;
            }
            await Context.Channel.SendMessageAsync("Answer: " + prod);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calc* with success! (" + prod + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calc/", RunMode = RunMode.Async)]
        public async Task calcdivide(params double[] tal)
        {
            try
            {
                double prod = tal.First();
                foreach (double value in tal.Skip(1))
                {
                    prod /= value;
                }
                if (Double.IsPositiveInfinity(prod) || Double.IsNegativeInfinity(prod))
                {
                    throw new DivideByZeroException("Im sorry! I can't divide by 0!");
                }
                await Context.Channel.SendMessageAsync("Answer: " + prod);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calc/ with success! (" + prod + ") (" + Context.Guild?.Name ?? "DM" + ")");
            }
            catch (DivideByZeroException)
            {
                await Context.Channel.SendMessageAsync("Im sorry! I can't divide by 0!");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calc+ and failed! (DivideByZeroException) (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [Command("calc-", RunMode = RunMode.Async)]
        public async Task calcminus(params double[] tal)
        {
            try
            {
                double result = tal.First();
                foreach (int value in tal.Skip(1))
                {
                    result -= value;
                }
                await Context.Channel.SendMessageAsync("Answer: " + result);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calc- with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
            }
            catch (Exception)
            {
                await Context.Channel.SendMessageAsync(Context.User.Mention + " you gotta give me something to calculate");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calc- and failed! (InvalidInputException) " + "(" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [Command("calcarea", RunMode = RunMode.Async)]
        public async Task calcarea(double radius)
        {
            double result = Math.PI * radius * radius;
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcarea with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcomkreds", RunMode = RunMode.Async)]
        public async Task calcomkreds(double diameter)
        {
            double result = Math.PI * diameter;
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcomkreds with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calccos", RunMode = RunMode.Async)]
        public async Task calccos(double vinkel, double multiplier = 1)
        {
            double result = Math.Cos(vinkel * (Math.PI / 180.0)) * multiplier;
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calccos with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcsin", RunMode = RunMode.Async)]
        public async Task calcsin(double vinkel, double multiplier = 1)
        {
            double result = Math.Sin(vinkel * (Math.PI / 180.0)) * multiplier;
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcsin with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calctan", RunMode = RunMode.Async)]
        public async Task calctan(double vinkel, double multiplier = 1)
        {
            double result = Math.Tan(vinkel * (Math.PI / 180.0)) * multiplier;
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calctan with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcsqrt", RunMode = RunMode.Async)]
        public async Task calcsqrt(double tal)
        {
            double result = Math.Sqrt(tal);
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcsqrt with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcpow", RunMode = RunMode.Async)]
        public async Task calcpow(double tal1, double tal2)
        {
            double result = Math.Pow(tal1, tal2);
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcpow with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcacos", RunMode = RunMode.Async)]
        public async Task calcacos(double tal)
        {
            double result = Math.Acos(tal) * (180 / Math.PI);
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcacos with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcasin", RunMode = RunMode.Async)]
        public async Task calcasin(double tal)
        {
            double result = Math.Asin(tal) * (180 / Math.PI);
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcasin with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcatan", RunMode = RunMode.Async)]
        public async Task calcatan(double tal)
        {
            double result = Math.Atan(tal) * (180 / Math.PI);
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcatan with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("calcatan2", RunMode = RunMode.Async)]
        public async Task calcatan2(double tal, double tal2)
        {
            double result = Math.Atan2(tal, tal2);
            await Context.Channel.SendMessageAsync("Answer " + result);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?calcatan2 with success! (" + result + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [Command("Pi", RunMode = RunMode.Async)]
        public async Task pi()
        {
            await Context.Channel.SendMessageAsync("3.141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067982148086513282306647093844609550582231725359408128481117450284102701938521105559644622948954930381964428810975665933446128475648233786783165271201909145648566923460348610454326648213393607260249141273724587006606315588174881520920962829254091715364367892590360011330530548820466521384146951941511609433057270365759591953092186117381932611793105118548074462379962749567351885752724891227938183011949129833673362440656643086021394946395224737190702179860943702770539217176293176752384674818467669405132000568127145263560827785771342757789609173637178721468440901224953430146549585371050792279689258923542019956112129021960864034418159813629774771309960518707211349999998372978049951059731732816096318595024459455346908302642522308253344685035261931188171010003137838752886587533208381420617177669147303598253490428755468731159562863882353787593751957781857780532171226806613001927876611195909216420198938095257201065485863278865936153381827968230301952035301852968995773622599413891249721775283479131515574857242454150695950829533116861727855889075098381754637464939319255060400927701671139009848824012858361603563707660104710181942955596198946767837449448255379774726847104047534646208046684259069491293313677028989152104752162056966024058038150193511253382430035587640247496473263914199272604269922796782354781636009341721641219924586315030286182974555706749838505494588586926995690927210797509302955321165344987202755960236480665499119881834797753566369807426542527862551818417574672890977772793800081647060016145249192173217214772350141441973568548161361157352552133475741849468438523323907394143334547762416862518983569485562099219222184272550254256887671790494601653466804988627232791786085784383827967976681454100953883786360950680064225125205117392984896084128488626945604241965285022210661186306744278622039194945047123713786960956364371917287467764657573962413890865832645995813390478027590");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?pi with success! " + "(" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireContext(ContextType.Guild)]
        [Command("membercount", RunMode = RunMode.Async)]
        public async Task memcount()
        {
            var bots = Context.Guild.Users.Count(x => x.IsBot);
            var members = Context.Guild.MemberCount;
            double ratio = (double)bots / (double)members;
            double percentage = Math.Round((double)bots / (double)members, 3) * 100;
            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = $"Member count for {Context.Guild.Name}",
                Description = $"{Context.Guild.MemberCount} Total members ({percentage}% bots)\n" +
                $"{Context.Guild.Users.Count(x => x.IsBot)} Bots\n" +
                $"{members - Context.Guild.Users.Count(x => x.IsBot)} Users\n" +
                $"{ratio} Bot to user ratio",
                Color = new Color(0, 0, 255)
            };
            var embed = builder.Build();
            await ReplyAsync("", false, embed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?membercount with success! (" + Context.Guild.MemberCount + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireContext(ContextType.Guild)]
        [Command("tokens", RunMode = RunMode.Async)]
        public async Task userstatus([Remainder] IGuildUser user = null)
        {
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            if (user == null)
            {
                user = Context.User as IGuildUser;
            }
            Database.CreateTable(Context.Guild.Id.ToString());
            var result = Database.CheckExistingUser(user);

            if (!result.Any())
            {
                Database.EnterUser(user);
            }

            var userTable = Database.GetUserStatus(user);

            embed.Description = (user.Mention + " has " + userTable.FirstOrDefault().Tokens + " tokens to spend!:small_blue_diamond:");
            var builtEmbed = embed.Build();
            await Context.Channel.SendMessageAsync("", false, builtEmbed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?tokens with success! (" + userTable.FirstOrDefault().Tokens + ") (" + Context.Guild.Name + ")");
        }
        [RequireContext(ContextType.Guild)]
        [Alias("leaderboards")]
        [Command("leaderboard", RunMode = RunMode.Async)]
        public async Task leaderboard(string typeupper = "", int index = 1)
        {
            if (!Context.User.IsBot)
            {
                string type = typeupper.ToLower();
                if (type == "tokens")
                {
                    if (index >= 1)
                    {
                        int pagelimit = index - index + 10 * index - 10;
                        IUser user = Context.User;
                        var embed = new EmbedBuilder()
                        {
                            Color = new Color(0, 0, 255)
                        };
                        var list = Database.GetAllUsersTokens(user as IGuildUser);
                        var foreachedlist = new List<string>();
                        foreach (var item in list.Skip(pagelimit).Take(10))
                        {
                            int position = list.IndexOf(item) + 1;
                            string padded = position.ToString() + ".";
                            string userName;
                            var currentUser = Context.Guild.Users.FirstOrDefault(x => x.Id == ulong.Parse(item.UserId));
                            if (currentUser == null)
                            {
                                userName = $"Unknown({item.UserId})";
                            }
                            else
                            {
                                userName = currentUser.Nickname ?? currentUser.Username;
                            }
                            string leftside = padded.PadRight(4) + userName;
                            foreachedlist.Add(leftside.PadRight(25 + 19 - item.Tokens.ToString().Length) + item.Tokens.ToString());
                        }
                        double decimalnumber = list.Count / 10.0D;
                        var celing = Math.Ceiling(decimalnumber);
                        if (index > celing)
                        {
                            await ReplyAsync("This page is empty");
                        }
                        else
                        {
                            var arrayedlist = foreachedlist.ToArray();
                            string longstring = String.Join("\n", arrayedlist);
                            embed.Title = "Token leaderboard for " + Context.Guild.Name;
                            embed.Description = ("```css\n" + longstring + "\n```");
                            embed.WithFooter("Page " + index + "/" + celing);
                            var builtEmbed = embed.Build();
                            await ReplyAsync("", false, builtEmbed);
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?leaderboard tokens with success! (" + index + ") (" + Context.Guild?.Name ?? "DM" + ")");
                        }
                    }
                    else
                    {
                        await ReplyAsync("Nice try, but I don't want syntax errors with MySQL");
                    }
                }
                else if (type == "leveling")
                {
                    if (index >= 1)
                    {
                        int pageLimit = index - index + 10 * index - 10;
                        IUser user = Context.User;
                        var embed = new EmbedBuilder()
                        {
                            Color = new Color(0, 0, 255)
                        };
                        var list = Database.GetAllUsersLeveling(user as IGuildUser);
                        var foreachedlist = new List<string>();
                        foreach (var item in list.Skip(pageLimit).Take(10))
                        {
                            int position = list.IndexOf(item) + 1;
                            string padded = position.ToString() + ".";
                            string userName;
                            var currentUser = Context.Guild.Users.FirstOrDefault(x => x.Id == ulong.Parse(item.UserId));
                            if(currentUser == null)
                            {
                                userName = $"Unknown({item.UserId})";
                            }
                            else
                            {
                                userName = currentUser.Nickname ?? currentUser.Username;
                            }
                            string leftside = padded.PadRight(4) + userName;
                            string levelandxp = item.Level.ToString() + " " + item.XP.ToString() + "/" + XP.caclulateNextLevel(item.Level);
                            foreachedlist.Add(leftside.PadRight(25 + 10 - item.Level.ToString().Length) + " " + levelandxp);
                        }
                        double decimalnumber = list.Count / 10.0D;
                        var celing = Math.Ceiling(decimalnumber);
                        if (celing < index)
                        {
                            await ReplyAsync("This page is empty");
                        }
                        else
                        {
                            var arrayedlist = foreachedlist.ToArray();
                            string longstring = String.Join("\n", arrayedlist);
                            embed.Title = "Leveling leaderboard for " + Context.Guild.Name;
                            embed.Description = ("```css\n" + longstring + "\n```");
                            embed.WithFooter("Page " + index + "/" + celing);
                            var builtEmbed = embed.Build();
                            await ReplyAsync("", false, builtEmbed);
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?leaderboard leveling with success! (" + index + ") (" + Context.Guild.Name + ")");
                        }
                    }
                    else
                    {
                        await ReplyAsync("Nice try, but I don't want syntax errors with MySQL");
                    }
                }
                else
                {
                    await ReplyAsync("Usage: " + ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault().Prefix + "leaderboard <type> <page>" +
                        "\nAvailable types:" +
                        "\nTokens, Leveling");
                }
            }
            else
            {
                await ReplyAsync("Bots don't have stats");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("Expose", RunMode = RunMode.Async)]
        public async Task expose(IGuildUser user = null)
        {
            if (user != null)
            {
                await Context.Channel.SendMessageAsync(user.Mention + " just got exposed <:exposed:357837551886925844>");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?expose with success! and exposed (" + user.Username + ") (" + Context.Guild.Name + ")");
            }
            else
            {
                await Context.Channel.SendMessageAsync((Context.User as IGuildUser)?.Mention + " just got exposed");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?expose with success! and exposed himself!" + " (" + Context.Guild.Name + ")");
            }
        }
        [Command("invite", RunMode = RunMode.Async)]
        public async Task invite()
        {
            await Context.Channel.SendMessageAsync("**" + Context.User.Username + "**, use this URL to invite me" +
                "\nhttps://discordapp.com/oauth2/authorize?client_id=369865463670374400&scope=bot&permissions=8");
            if (Context.Guild != null)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?invite with success!" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?invite with success!" + " (" + Context.User + " DM" + ")");
            }
        }
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [Command("purge", RunMode = RunMode.Async)]
        public async Task purge(uint amount)
        {
            if ((Context.User as IGuildUser).GuildPermissions.ManageMessages == true || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                await (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages);
                const int delay = 3000;
                var m = await this.ReplyAsync($"Purge completed. _This message will be deleted in {delay / 1000} seconds._");
                await Task.Delay(delay);
                await m.DeleteAsync();
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?purge with success! and deleted " + amount + " messages" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                await Context.Channel.SendMessageAsync("You do not have guild permission ManageMessages");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?purge and failed! (InsufficientPermissionsException)" + " (" + Context.Guild.Name + ")");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("award", RunMode = RunMode.Async)]
        public async Task Award(SocketGuildUser user, int tokens, [Remainder] string comment = null)
        {
            var name = (user as IGuildUser).Nickname ?? user.Username;
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild == true || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var result = Database.CheckExistingUser(user);
                if (result.Count() <= 1)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 255)
                    };
                    Database.ChangeTokens(user, tokens);
                    if (comment != null)
                    {
                        embed.Title = (name + " was awarded " + tokens + " tokens!");
                        embed.Description = (comment);
                        var builtEmbed = embed.Build();
                        await ReplyAsync("", false, builtEmbed);
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + name + " just ran ?award with success!" + " (" + tokens + ") (" + comment + ") (" + Context.Guild.Name + ")");
                    }
                    else
                    {
                        embed.Title = (name + " was awarded " + tokens + " tokens!");
                        var builtEmbed = embed.Build();
                        await ReplyAsync("", false, builtEmbed);
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + name + " just ran ?award with success!" + " (" + tokens + ")" + " (" + Context.Guild.Name + ")");
                    }
                }
            }
            else
            {
                await ReplyAsync("You do not have permission!");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("awardxp", RunMode = RunMode.Async)]
        public async Task awardxp(IUser user, int xptogive, [Remainder] string reason = "No reason")
        {
            var name = ((IGuildUser)user).Nickname ?? user.Username;
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild == true || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var result = Database.CheckExistingUser(user as IGuildUser);
                if (result.Count() <= 1)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 255)
                    };
                    Database.addXP(user as IGuildUser, xptogive);
                    embed.Title = (name + " was awarded " + xptogive + " xp!");
                    embed.Description = (reason);
                    var builtEmbed = embed.Build();
                    await ReplyAsync("", false, builtEmbed);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + name + " just ran ?award with success!" + " (" + xptogive + ") (" + reason + ") (" + Context.Guild.Name + ")");
                }
            }
        }
        [Custom_Preconditions.Ratelimit(1,5,Custom_Preconditions.Measure.Minutes,Custom_Preconditions.RatelimitFlags.ApplyPerGuild)]
        [RequireContext(ContextType.Guild)]
        [Command("awardall", RunMode = RunMode.Async)]
        public async Task awardall(int tokens, [Remainder] string comment = "")
        {
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild == true || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                await ReplyAsync("Handing out tokens.....");
                var users = Context.Guild.Users;
                foreach (var user in users)
                {
                    var result = Database.CheckExistingUser(user);
                    if (result.Count() <= 1)
                    {
                        Database.ChangeTokens(user, tokens);
                    }
                }
                var embed = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 255)
                };
                if (comment != "")
                {
                    embed.Title = ("All users were awarded " + tokens + " tokens!");
                    embed.Description = (comment);
                    var builtEmbed = embed.Build();
                    await ReplyAsync("", false, builtEmbed);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?awardall with success!" + " (" + tokens + ") (" + comment + ") (" + Context.Guild.Name + ")");
                }
                else
                {
                    embed.Title = ("All users were awarded " + tokens + " tokens!");
                    var builtEmbed = embed.Build();
                    await ReplyAsync("", false, builtEmbed);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?awardall with success!" + " (" + tokens + ") (" + Context.Guild.Name + ")");
                }
            }
            else
            {
                await ReplyAsync("You do not have permission!");
            }
        }

        [Command("calc 2+2-1")]
        public async Task Bigshaq()
        {
            await Context.Channel.SendMessageAsync("2 + 2 = 4 - 1 that's 3 quick maths!");
        }
        [RequireContext(ContextType.Guild)]
        [Command("daily", RunMode = RunMode.Async)]
        public async Task Daily([Remainder] IGuildUser user = null)
        {

            if (user == null)
            {
                user = Context.User as IGuildUser;
            }
            // These lines checks if the user exits, if not we add him into the database
            var result = Database.CheckExistingUser(user);
            if (!result.Any())
            {
                Database.EnterUser(user);
            }
            var tableName = Database.GetUserStatus(Context.User as IGuildUser); // We get the user status

            DateTime now = DateTime.Now; // We get the actual time
            DateTime daily = tableName.FirstOrDefault().Daily;
            int difference = DateTime.Compare(daily, now);
            if ((tableName.FirstOrDefault()?.Daily.ToString() == "0001-01-01 00:00:00") || (daily.DayOfYear < now.DayOfYear && difference < 0 || difference >= 0 || daily.Year < now.Year))
            {
                int amount = 50; // The amount of credits the user is gonna receive, in uint of you followed BossDarkReaper advises or in int
                if (await _service.DblApi().HasVoted(Context.User.Id))
                {
                    amount *= 4;
                    await ReplyAsync("Thanks for voting today, here is a bonus");
                }
                else
                {
                    await ReplyAsync($"You would have gotten 4x more tokens if you have voted today. See {ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault()?.Prefix}upvote");
                }
                Database.ChangeDaily(Context.User as IGuildUser);
                if (user != Context.User as IGuildUser)
                {
                    _rand = new Random();
                    amount += _rand.Next(amount * 2);
                    await ReplyAsync($"You have given {user.Nickname ?? user.Username} {amount} daily tokens!");
                    Database.ChangeTokens(user, amount);
                }
                else
                {
                    Database.ChangeTokens(user, amount); // We add the tokens to the user
                    await ReplyAsync($"You received your {amount} tokens!");
                }
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?daily with success!" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                TimeSpan diff = now - daily; // This line compute the difference of time between the two dates

                // This line prevents "Your credits refresh in 00:18:57.0072170 !"
                TimeSpan di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);

                await ReplyAsync($"Your tokens refresh in {di} !");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?daily and failed!" + " (NotRefreshedException) (" + Context.Guild.Name + ")");
            }
        }
        [RequireContext(ContextType.Guild)]
        [Command("pay", RunMode = RunMode.Async)]
        public async Task pay(IGuildUser usertopay, int amount, [Remainder] string comment = null)
        {
            var user = Context.User as SocketGuildUser;
            var result = Database.CheckExistingUser(user);
            var result2 = Database.CheckExistingUser(usertopay);
            var result3 = _cachingService.GetBlackList();
            if (!result3.Contains(user.Id))
            {
                if (result.Any())
                {
                    if (result2.Count >= 1)
                    {
                        var userTable = Database.GetUserStatus(user);
                        var userToPay = Database.GetUserStatus(usertopay);
                        if (amount > 0)
                        {
                            if (userTable.FirstOrDefault().Tokens >= amount)
                            {
                                Database.RemoveTokens(user, amount);
                                Database.ChangeTokens(usertopay, amount);
                                if (comment != null)
                                {
                                    var embed = new EmbedBuilder()
                                    {
                                        Color = new Color(0, 0, 255)
                                    };
                                    embed.Description = (user.Mention + " has paid " + usertopay.Mention + " " + amount + " tokens!" +
                                        "\n" + comment);
                                    var builtEmbed = embed.Build();
                                    await Context.Channel.SendMessageAsync("", false, builtEmbed);
                                }
                                else
                                {
                                    var embed = new EmbedBuilder()
                                    {
                                        Color = new Color(0, 0, 255)
                                    };
                                    embed.Description = (user.Mention + " has paid " + usertopay.Mention + " " + amount + " tokens!");
                                    var builtEmbed = embed.Build();
                                    await Context.Channel.SendMessageAsync("", false, builtEmbed);
                                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?pay with success!" + " (" + Context.Guild.Name + ")");
                                }
                            }
                        }
                        else
                        {
                            await ReplyAsync("Dont attempt to steal tokens from people!");
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?pay and failed!" + " (TriedToStealTokensException) (" + Context.Guild.Name + ")");
                        }
                    }
                    else
                    {
                        await ReplyAsync("Target user not in the database! adding user...");
                        Database.EnterUser(usertopay);
                        await ReplyAsync("User added! try running the command again");
                    }
                }
                else
                {
                    await ReplyAsync("User not in the database! adding user...");
                    Database.EnterUser(user);
                    await ReplyAsync("User added! try running the command again");
                }
            }
            else
            {
                await ReplyAsync("You can't pay blacklisted users!");
            }
        }
        [Command("info", RunMode = RunMode.Async)]
        public async Task info()
        {
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            int totalmembers = 0;
            foreach (var guild in Context.Client.Guilds)
            {
                totalmembers += guild.MemberCount;
            }
            _rand = new Random();
            var builder = new EmbedBuilder()
                    .WithTitle("Info about " + Context.Client.CurrentUser.Username + ":")
                    .WithDescription("\nBeep boop... I am " + Context.Client.CurrentUser.Username +
                "\nI have some commands that you can find with slash commands" +
                "\nI am a bit sketchy so watch out and be careful" +
                "\nMy date of manufactor is 19/10/2017")
                    .WithColor(new Color((uint)_rand.Next(0x0, 0xFFFFFF)))
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(footer =>
                    {
                        footer
                        .WithText("Requested by " + Context.User.Username)
                            .WithIconUrl(Context.User.GetAvatarUrl());
                    })
            .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
            .WithAuthor(author =>
            {
                author
                .WithName("Info:")
                .WithUrl("https://discordapp.com/oauth2/authorize?client_id=369865463670374400&scope=bot&permissions=8");
            }).AddField("Developers:", $"Bot developer: {Context.Client.GetUser(135446225565515776)}" +
                                    $"\nWeb developer: {Context.Client.GetUser(208624502878371840)}", true)
            .AddField("Other info:", "I am in " + Context.Client.Guilds.Count + " servers!" +
            "\n" + totalmembers + " members across all servers!" +
            "\nUptime: " + uptime.Days + " Days " + uptime.Hours + " Hours " + uptime.Minutes + " Minutes " + uptime.Seconds + " Seconds" +
            "\nAverage messages per min since startup: " + _statService.msgCounter/_statService.uptime.TotalMinutes, true)
            .AddField("My server:", "https://discord.gg/UPG8Vqb", true).AddField("Website:", "https://www.sketchbot.xyz", true);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync("", false, embed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?info with success!" + " (" + Context.Guild.Name ?? "(DM)" + ")");
        }
        [RequireContext(ContextType.Guild)]
        [Command("serverinfo")]
        public async Task serverinfo()
        {
            var guild = Context.Guild;
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255),
                Timestamp = guild.CreatedAt,
            };
            builder.WithAuthor(author =>
            {
                author.Name = guild.Name;
                author.IconUrl = guild.IconUrl;
            });
            builder.WithFooter(footer =>
            {
                footer.Text = $"ID: {guild.Id} | Server Created";
            });
            builder.AddField("Owner", guild.Owner.Username + guild.Owner.Discriminator,true)
                .AddField("Region", guild.VoiceRegionId, true)
                .AddField("Channel Categories", guild.CategoryChannels.Count, true)
                .AddField("Text Channels", guild.TextChannels.Count,true)
                .AddField("Voice Channels", guild.VoiceChannels.Count, true)
                .AddField("Members", guild.MemberCount, true)
                .AddField("Humans", guild.Users.Count(x => !x.IsBot),true)
                .AddField("Bots", guild.Users.Count(x => x.IsBot), true)
                .AddField("Roles", guild.Roles.Count,true);
            var embed = builder.Build();
            await ReplyAsync("", false, embed);
        }
        [RequireContext(ContextType.Guild)]
        [Command("userinfo")]
        public async Task userinfo(SocketGuildUser user = null)
        {
            if(user == null)
            {
                user = Context.User as SocketGuildUser;
            }
            EmbedBuilder builder = new EmbedBuilder()
            {
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = new Color(0, 0, 255),
                Timestamp = DateTime.Now
            };
            builder.WithAuthor(author =>
            {
                author.Name = user.Username + user.Discriminator;
                author.IconUrl = user.GetAvatarUrl();
            });
            builder.WithFooter(footer =>
            {
                footer.Text = $"ID: {user.Id}";
            });
            var users = Context.Guild.Users;
            var userslist = users.OrderBy(o => o.JoinedAt).ToList();
            var joinedpos = userslist.FindIndex(x => x.Id == user.Id);
            builder.AddField("Joined", user.JoinedAt, true).AddField("Join Position", joinedpos, true)
                .AddField("Registered", user.CreatedAt)
                .AddField($"Roles [{user.Roles.Count}]", string.Join(" ", user.Roles.Select(x => x.Mention)));
            var embed = builder.Build();
            await ReplyAsync("", false, embed);
        }
        [Command("JoJo")]
        public async Task Jojosbizzareadventure()
        {
            _rand = new Random();
            var dir = new DirectoryInfo("Jojo");
            var dirFiles = dir.GetFiles();
            int fileIndex = _rand.Next(dirFiles.Length);
            int pictureNumber = fileIndex + 1;
            string fileToPost = dirFiles[fileIndex].FullName;
            await Context.Channel.SendMessageAsync("Jojo's bizzare adventure " + pictureNumber);
            await Context.Channel.SendFileAsync(fileToPost);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?Jojo with success! " + pictureNumber + " (" + Context.Guild.Name + ")");
        }
        [Command("random")]
        public async Task Random()
        {
            _rand = new Random();

            var RandomMessages = new string[]
{
            "æøå",
            "Ecks dee",
            "123456789",
            "This is a list",
            "Random Message",
            "RNG",
            "HTX > HHX > STX",
            "STX = Trash",
            "Mee6 kan ikke commands med flere ord",
            "?help",
            "Ingress",
            "Pokemon GO",
            "C#",
            "Maple",
            "Mimouniboi",
            "9gag",
            "ArrayList",
            "Mikerosoft Certified Technician",
            "Emo",
            "Anime",
            "133769420",
            "MLG",
            "K-WHY-S",
            "Chrunchyroll",
            "Agoraen",
            "Desperat skole hjælp",
            "Discord",
            "Lidl",
            "Kantinen er overpriced",
            "<:sandwich:355316315780677632>",
            "xD",
            ":icecube:",
            "DAB",
            "Programmering",
            "Gajs",
            "Gajs, der er time",
            "Produktudvikling",
            "Naturvidenskabeligtgrundforløb",
            "boi",
            "fuccboi",
            "Hatsune Miku",
            "Itslearning",
            "Riskage",
            "How much Ris can a Rischuck chuck if the Rischuck could chuck Rishi",
            "Newtonmeter",
            "Alle realtal",
            "Den tomme mængde",
            "Dramaalert",
            "Scarce",
            "Hey what's up guys it's Scarce here",
            "Naturvidenskabelig metode",
            "Ulduar",
            "niceme.me",
            "Rishi",
            "Nibba",
            "Plagierkontroler",
            "Mee6 er lårt",
            "World of Warcraft",
            "Blizzard",
            "Elevplan",
            "Nielsen",
            "yaaaarrr boi",
            "Waps",
            "Riskage spil",
            "AWS",
            "Amazon",
            "Ebay",
            "Aliexpress",
            "Nordisk film",
            "Bone's",
            "EC2",
            "GIF",
            "Instances",
            "Storage",
            "S3",
            "NVM",
            "Database",
            "dEcLaN.eXe",
            "Mojo",
            "SQL",
            "Hello World",
            "HTML",
            "CSS",
            "PHP",
            "JS",
            "T H I C C",
            "Samfundsfag = sovetime",
            "Vuk",
            "Vektorfunktioner",
            "Vukterfunktioner",
            "In memory of Vuk",
            "Sweet Silence",
            "Gucci gang",
            "Osu!",
            "What is up AutismAlert nation",
            "LinusTechTips",
            "Scrapyard wars",
            "Tunnelbear",
            "One-energy cola",
            "Water",
            "Such message very random",
            "4:3 Stretched",
            "Black bars",
            "Java",
            "Javascript",
            "Eclipse",
            "This list is getting looooooooooooooong",
            "Craigslist",
            "SLI",
            "Intel & nVidia > AMD",
            "Razer",
            "Razer blackwidow chroma",
            "Stationspizza",
            "Linus",
            "Windows",
            "Macbook",
            "Linux",
            "Raspberry Pi",
            "Arduino",
            "LCD",
            "Jonte-bro",
            "Password",
            "O2Auth",
            "discordapp.com",
            "discord.gg",
            "Sodapoppin",
            "2147483647",
            "4294967295",
            "Battlefield Heroes",
            "Fortnite",
            "Rema 1000",
            "Fakta",
            "PUBG",
            "Playerunknown's Battlegrounds",
            "Far Cry 5",
            "Far Cry 4",
            "Far Cry 3",
            "PewDiePie",
            "Nick Crompton",
            "England er min by",
            "England is my city",
            "?riskage spil",
            "777",
            "Jackpot",
            "Luke",
            "Thomas Jefferson Chance Morris",
            "420",
            "1337",
            "69",
            "Gaming-linjen",
            "IT-Videnskab",
            "Hearthstone",
            "Its everyday bro",
            "Snapchat",
            "Telegram",
            "Minecraft commandblocks",
            "Ninja",
            "#weebconfirmed",
            "Kommunikationsmodeller",
            "What's 9 + 10? 21!",
            "Divine spirit",
            "Innerfire",
            "Legendary",
            "Legiondary",
            "Legend",
            "Cities: Skyline",
            "Test",
            "Execute",
            "Leeeeeeerrroooooyyyyyy Jeeeeeeeeeeeeeenkinsssss",
            "C'Thun",
            "Standard > Wild",
            "Duplicates",
            "Wallpaper Engine",
            "Deez nuts",
            "Insert meme here",
            "Ultrasaur",
            "Ban",
            "Kick",
            "Help! My creator forces me to respond to commands",
            "Jonaser",
            "The ting goes skrrraa",
            "Random message number 200",
            "Gulag",
            "Tyskland",
            "Morten",
};
            int randomIndex = _rand.Next(RandomMessages.Length);
            int messageNumber = randomIndex + 1;
            string msgToPost = RandomMessages[randomIndex];
            await Context.Channel.SendMessageAsync(msgToPost);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?random with success! " + messageNumber + " (" + Context.Guild.Name + ")");
        }
    }
}