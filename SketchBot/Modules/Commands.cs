using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Interactions;
using Discord.Rest;
//using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList;
using DiscordBotsList.Api;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json.Linq;
using Sketch_Bot.Custom_Preconditions;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using SketchBot.Handlers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UrbanDictionnet;

namespace Sketch_Bot.Modules
{
    public class Commands : InteractionModuleBase<SocketInteractionContext>
    {

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public static
        CultureInfo ci = CultureInfo.InvariantCulture;
        Random _rand;
        Stopwatch _stopwatch;

        private DiscordBotsListService _discordBotListService;
        private TimerService _timerService;
        private StatService _statService;
        private CachingService _cachingService;
        private InteractionService _interactionService;
        public Commands(DiscordBotsListService service, TimerService service2, StatService service3, CachingService service4, InteractionService service5)
        {
            _discordBotListService = service;
            _timerService = service2;
            _statService = service3;
            _cachingService = service4;
            _interactionService = service5;
        }

        [SlashCommand("repeat", "Echo a message")]
        public async Task testt(string input)
        {
            await DeferAsync();
            await FollowupAsync($"{Context.User.Mention} < {input}");
            
        }
        [RequireUserPermission(GuildPermission.SendTTSMessages)]
        [SlashCommand("repeattts", "Echo a message")]
        public async Task repeattts(string input)
        {
            await DeferAsync();
            await FollowupAsync($"{Context.User.Mention} < {input}", null, true);
            
        }
        [SlashCommand("rate", "Rates something out of 100")]
        public async Task Rate(string input)
        {
            await DeferAsync();
            var ci = CultureInfo.InvariantCulture;

            var specialRatings = new Dictionary<string, (double rating, string comment)>(StringComparer.OrdinalIgnoreCase)
    {
        { "hhx", (-1, "hhx") },
        { "mee6", (-1, "mee6") },
        { "stx", (-1, "stx") },
        { "the meaning of life", (42, "the meaning of life (out of 42)") },
        { "bush", (9, "bush (out of 11)") },
        { "htx", (101, "htx (out of 100)") },
        { "riskage", (100, "riskage (out of 100)") },
        { "riskage bot", (100, "riskage bot (out of 100)") },
        { "@Tjampen", (9999999, "Tjampen") },
        { "Tjampen", (9999999, "Tjampen") },
        { "<@208624502878371840>", (9999999, "Tjampen") },
        { "Taoshi", (2147483647, "Taoshi") },
        { "Taoshi#3480", (2147483647, "Taoshi") },
        { "@Taoshi", (2147483647, "Taoshi") },
        { "<@135446225565515776>", (2147483647, "Taoshi") }
    };

            string username = Context.User.Username;
            string guildName = Context.Guild?.Name ?? "DM";
            string timestamp = DateTime.Now.ToString("HH:mm:ss", ci);

            if (specialRatings.TryGetValue(input, out var special))
            {
                await FollowupAsync($"I rate {input} **{special.rating}** out of 100");
                Console.WriteLine($"{timestamp} Command     {username} just ran ?rate with success! and got {special.rating} ({special.comment}) ({guildName})");
            }
            else
            {
                var rand = new Random();
                int randomScore = rand.Next(1001); // 0–1000
                double rating = randomScore / 10.0;

                await FollowupAsync($"I rate {input} **{rating}** out of 100");
                Console.WriteLine($"{timestamp} Command     {username} just ran ?rate with success! and got {randomScore} ({input}) ({guildName})");
            }
        }

        [SlashCommand("roll", "Rolls between x and y")]
        public async Task roll(int min = 1, int max = 100)
        {
            await DeferAsync();
            _rand = new Random();
            try
            {
                if (min > max)
                {
                    await FollowupAsync("The minimum value must not be over the maximum value!");
                    
                }
                else
                {
                    var rng = _rand.Next(min, max);
                    await FollowupAsync($"{Context.User.Username} rolled {rng} ({min}-{max})");
                    
                }
            }
            catch (IndexOutOfRangeException)
            {
                await FollowupAsync("The number has to be between 0 and 2147483647!");
            }
        }
        [SlashCommand("hello", "Hello")]
        public async Task hello()
        {
            await DeferAsync();
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                await FollowupAsync("Hello developer! How are you doing today?");
            }
            else
            {
                await FollowupAsync("Hi! " + Context.User.Username);
            }
            
        }
        [SlashCommand("donate", "Sends a link for donations")]
        public async Task donate()
        {
            await DeferAsync();
            await FollowupAsync("https://www.patreon.com/Sketch_Bot");
        }
        [SlashCommand("upvote", "Sends a link for upvoting the bot")]
        public async Task upvote()
        {
            await DeferAsync();
            await ReplyAsync("You can upvote the bot here https://discordbots.org/bot/369865463670374400");
            var Api = _discordBotListService.DblApi();
            if (await Api.HasVoted(Context.User.Id))
            {
                await FollowupAsync("Thanks for voting today!");
            }
            else
            {
                await FollowupAsync("You have not voted today");
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("gamble", "Gamble tokens")]
        public async Task gamble(long amount)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            _rand = new Random();
            var currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
            if (amount > currentTokens) await FollowupAsync("You don't have enough tokens");
            else if (amount < 1) await FollowupAsync("The minimum amount of tokens is 1");
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
                    await FollowupAsync("", null, false, false, null, null, null, embed);
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
                    await FollowupAsync("", null, false, false, null, null, null, embed);
                }
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("gambleall", "Gambles all of your tokens")]
        public async Task gambleall()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            long amount = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
            _rand = new Random();
            var currentTokens = Database.GetUserStatus(Context.User as IGuildUser).FirstOrDefault().Tokens;
            if (amount < 1) await FollowupAsync("The minimum amount of tokens is 1");
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
                    await FollowupAsync("", null, false, false, null, null, null, embed);
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
                    await FollowupAsync("", null, false, false, null, null, null, embed);
                }
            }
        }
        [SlashCommand("ping", "Pong")]
        public async Task ping()
        {
            await DeferAsync();
            await FollowupAsync("Pong!");
            /*
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            var message = await RespondAsync("hello!");
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
            await RespondAsync("", null,false,false,null,null,null,embed);
            
        */
        }
        [UserCommand("avatar")]
        public async Task avatar(IUser user)
        {
            await DeferAsync();
            await FollowupAsync(user.GetAvatarUrl(ImageFormat.Auto, 256));
        }
        [SlashCommand("avatar", "Get the avatar of a user")]
        public async Task avatarAsync(IUser user = null)
        {
            await DeferAsync();
            if (user == null)
            {
                user = Context.User;
            }
            var embed = new EmbedBuilder()
                .WithColor(new Color(0x4900ff))
                .WithTitle($"{user.Username}'s Avatar")
                .WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 256));
            await FollowupAsync("", null, false, false, null, null, null, embed.Build());
        }
        [SlashCommand("eightball", "Ask the 8ball a question")]
        public async Task eightball(string input)
        {
            await DeferAsync();
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
            await FollowupAsync(":8ball: **Question: **" + input + "\n**Answer: **" + text);
            
        }
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("kick", "Kicks someone from the server")]
        public async Task kick(IGuildUser user, string reason = "No reason")
        {
            await DeferAsync();
            var currentUser = Context.User as IGuildUser;
            if (currentUser == null || !currentUser.GuildPermissions.KickMembers)
            {
                await FollowupAsync("You do not have Guild permission KickMembers");
                return;
            }
            if (user == null)
            {
                await FollowupAsync("/kick <user> <reason>");
                return;
            }
            if (!(Context.Client.CurrentUser as IGuildUser).GuildPermissions.KickMembers)
            {
                await FollowupAsync("I don't have the permission to do so!");
                return;
            }
            var embed = new EmbedBuilder()
                .WithColor(new Color(0x4900ff))
                .WithTitle($"{user.Username} has been kicked from {user.Guild.Name}")
                .WithDescription($"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}\n**Kicked by: **{Context.User.Mention}!\n**Reason: **{reason}");
            await user.KickAsync(reason);
            await FollowupAsync("", null, false, false, null, null, null, embed.Build());
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("ban", "Bans someone from the server")]
        public async Task banAsync(IGuildUser user, string reason = "No reason")
        {
            await DeferAsync();
            var currentUser = Context.User as IGuildUser;
            if (currentUser == null || !currentUser.GuildPermissions.BanMembers)
            {
                await FollowupAsync("You do not have Guild permission BanMembers");
                return;
            }
            if (user == null)
            {
                await FollowupAsync("/ban <user> <reason>");
                return;
            }
            if (!(Context.Client.CurrentUser as IGuildUser).GuildPermissions.BanMembers)
            {
                await FollowupAsync("I don't have the permission to do so!");
                return;
            }
            var embed = new EmbedBuilder()
                .WithColor(new Color(0x4900ff))
                .WithTitle($"{user.Username} has been banned from {user.Guild.Name}")
                .WithDescription($"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}\n**Banned by: **{Context.User.Mention}!\n**Reason: **{reason}");
            await Context.Guild.AddBanAsync(user, 7, reason);
            await FollowupAsync("", null, false, false, null, null, null, embed.Build());
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("unban", "Unbans someone from the server")]
        public async Task unbanAsync(RestUser user, string reason = "No reason")
        {
            await DeferAsync();
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840 || (Context.Client.CurrentUser as IGuildUser).GuildPermissions.BanMembers)
            {
                await Context.Guild.RemoveBanAsync(user);
                await FollowupAsync(user.Username + " has been unbanned" +
                    "\n" +
                    "\n" + reason);
            }
        }
        [SlashCommand("status", "Checks to see if a website is up")]
        public async Task status(string websiteUrl = "http://sketchbot.xyz")
        {
            await DeferAsync();
            async Task SendStatusEmbed(string url, string status, string description)
            {
                var embed = new EmbedBuilder
                {
                    Title = "Status",
                    Description = $"{url} \n\n{description}",
                    Timestamp = DateTime.Now,
                    ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 1024)
                };
                embed.WithFooter(footer =>
                {
                    footer
                        .WithText("Requested by " + Context.User.Username)
                        .WithIconUrl(Context.User.GetAvatarUrl());
                });
                await FollowupAsync("", null, false, false, null, null, null, embed.Build());
            }

            try
            {
                if (!websiteUrl.StartsWith("https://") && !websiteUrl.StartsWith("http://"))
                {
                    websiteUrl = "https://" + websiteUrl;
                }
                using (var httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(websiteUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        await SendStatusEmbed(websiteUrl, "online", "server is **online**");
                    }
                    else
                    {
                        await SendStatusEmbed(websiteUrl, "offline", "server is **offline**");
                    }
                }
            }
            catch (WebException)
            {
                await SendStatusEmbed(websiteUrl, "not available", "connection is **not available**");
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Command     " + Context.User.Username + " just ran ?status with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("nickname", "Changes your nickname")]
        public async Task nickname(IGuildUser targetUser, string newNickname)
        {
            await DeferAsync();

            var guild = Context.Guild;
            var botUser = guild.GetUser(Context.Client.CurrentUser.Id);
            var commandUser = Context.User as IGuildUser;

            if (!botUser.GuildPermissions.ManageNicknames)
            {
                await FollowupAsync("I do not have permission to manage nicknames.");
                return;
            }

            // If no target specified, default to self
            if (targetUser == null)
                targetUser = commandUser;

            if (!commandUser.GuildPermissions.ManageNicknames)
            {
                await FollowupAsync("You do not have permission to manage nicknames.");
                return;
            }

            // Check role hierarchy
            if (botUser.Hierarchy <= targetUser.Hierarchy)
            {
                await FollowupAsync("I cannot change the nickname of someone with a higher or equal role than me.");
                return;
            }
            if (commandUser.Hierarchy <= targetUser.Hierarchy && targetUser != commandUser)
            {
                await FollowupAsync("You cannot change the nickname of someone with a higher or equal role than you.");
                return;
            }

            try
            {
                await targetUser.ModifyAsync(x => x.Nickname = newNickname);
                await FollowupAsync($"Nickname for {targetUser.Mention} changed to **{newNickname}**!");
            }
            catch
            {
                await FollowupAsync("Failed to change nickname. This may be due to role hierarchy or permissions.");
            }
        }
        [SlashCommand("cat", "Sends a random cat image")]
        public async Task Cat()
        {
            await DeferAsync();
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
                    await FollowupAsync(CatImage);
                }
                
            }
            catch (Exception ex)
            {
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("fox", "Sends a random fox image")]
        public async Task fox()
        {
            await DeferAsync();
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
                    await FollowupAsync($"{catImage}");
                }
            }
            catch
            {
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("birb", "Sends a random birb bird image")]
        public async Task birb()
        {
            await DeferAsync();
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
                    await FollowupAsync($"{websitee}{CatImage}");
                }
            }
            catch
            {
                await FollowupAsync("API didn't return anything");
            }
        }

        [SlashCommand("calculate", "Calculates a math problem")]
        public async Task calculateAsync(HelperFunctions.Calculation expression)
        {
            await DeferAsync();
            try
            {
                await FollowupAsync(expression.Result.ToString());
            }
            catch
            {
                await FollowupAsync(expression.Error);
            }
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("membercount", "Tells you how many users are in the guild")]
        public async Task memcount()
        {
            await DeferAsync();

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
            await FollowupAsync("", null, false, false, null, null, null, embed);
            
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("tokens", "Shows you how many tokens you have")]
        public async Task userstatus(IGuildUser user = null)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            
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
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255),
                Description = (user.Mention + " has " + userTable.FirstOrDefault().Tokens + " tokens to spend!:small_blue_diamond:")
            };
            var builtEmbed = embed.Build();
            await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
            
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("leaderboard", "Server leaderboard of Tokens or Leveling")]
        public async Task leaderboard([Summary("Type"), Autocomplete(typeof(LeaderboardAutocompleteHandler))] string type, int index = 1)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            type = type.ToLower();
            string[] types = { "tokens", "leveling" };
            index = index > 0 ? index : 1;
            int pagelimit = index - index + 10 * index - 10;
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            var list = Database.GetAllUsersTokens(Context.User as IGuildUser);
            var foreachedlist = new List<string>();
            if (types.Contains(type))
            {
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
                    string levelandxp = item.Level.ToString() + " " + item.XP.ToString() + "/" + XP.caclulateNextLevel(item.Level);
                    foreachedlist.Add(type == "tokens" ? (leftside.PadRight(25 + 19 - item.Tokens.ToString().Length) + item.Tokens.ToString()) : leftside.PadRight(25 + 10 - item.Level.ToString().Length) + " " + levelandxp);
                }
                double decimalnumber = list.Count / 10.0D;
                var celing = Math.Ceiling(decimalnumber);
                if (index > celing)
                {
                    await FollowupAsync("This page is empty");
                }
                else
                {
                    var arrayedlist = foreachedlist.ToArray();
                    string longstring = string.Join("\n", arrayedlist);
                    embed.Title = $"{type} leaderboard for {Context.Guild.Name}";
                    embed.Description = ($"```css\n{longstring}\n```");
                    embed.WithFooter($"Page {index}/{celing}");
                    var builtEmbed = embed.Build();
                }
            }
            else
            {
                await FollowupAsync("Usage: /leaderboard <type> <page>" +
                    "\nAvailable types:" +
                    "\nTokens, Leveling");
            }
        }
        [SlashCommand("invite", "Invite me to your server")]
        public async Task invite()
        {
            await DeferAsync();
            await FollowupAsync("**" + Context.User.Username + "**, use this URL to invite me" +
                "\nhttps://discord.com/api/oauth2/authorize?client_id=369865463670374400&permissions=1617578818631&scope=bot%20applications.commands");
        }
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("purge", "Purges messages from the channel")]
        public async Task purge(uint amount)
        {
            await DeferAsync();
            if ((Context.User as IGuildUser).GuildPermissions.ManageMessages == true)
            {
                var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                await (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages);
                await FollowupAsync($"Purge completed.", ephemeral:true);
            }
            else
            {
                await FollowupAsync("You do not have guild permission ManageMessages", ephemeral:true);
                
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("award", "Give someone tokens")]
        public async Task Award(IUser user, int tokens, string comment = null)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var guildUser = user as IGuildUser;
            var name = guildUser.Nickname ?? guildUser.Username;
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild == true || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var result = Database.CheckExistingUser(guildUser);
                if (result.Count() <= 1)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 255)
                    };
                    Database.ChangeTokens(guildUser, tokens);
                    if (comment != null)
                    {
                        embed.Title = (name + " was awarded " + tokens + " tokens!");
                        embed.Description = (comment);
                        var builtEmbed = embed.Build();
                        await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                        
                    }
                    else
                    {
                        embed.Title = (name + " was awarded " + tokens + " tokens!");
                        var builtEmbed = embed.Build();
                        await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                    }
                }
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        [Custom_Preconditions.Ratelimit(1,5,Custom_Preconditions.Measure.Minutes,Custom_Preconditions.RatelimitFlags.ApplyPerGuild)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("awardall", "Give everyone on the server some tokens")]
        public async Task awardall(int tokens, string comment = "")
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
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
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                    
                }
                else
                {
                    embed.Title = ("All users were awarded " + tokens + " tokens!");
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                }
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("daily", "Claim your daily")]
        public async Task Daily(IGuildUser user = null)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
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
                if (await _discordBotListService.DblApi().HasVoted(Context.User.Id)) // TODO: Make a confirmation system if the user has not voted today
                {
                    amount *= 4;
                    await FollowupAsync("Thanks for voting today, here is a bonus");
                }
                else
                {
                    await FollowupAsync($"You would have gotten 4x more tokens if you have voted today. See /upvote"); 
                }
                Database.ChangeDaily(Context.User as IGuildUser);
                if (user != Context.User as IGuildUser)
                {
                    _rand = new Random();
                    amount += _rand.Next(amount * 2);
                    await FollowupAsync($"You have given {user.Nickname ?? user.Username} {amount} daily tokens!");
                    Database.ChangeTokens(user, amount);
                }
                else
                {
                    Database.ChangeTokens(user, amount); // We add the tokens to the user
                    await FollowupAsync($"You received your {amount} tokens!");
                }
            }
            else
            {
                TimeSpan diff = now - daily; // This line compute the difference of time between the two dates

                // This line prevents "Your credits refresh in 00:18:57.0072170 !"
                TimeSpan di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);

                await FollowupAsync($"Your tokens refresh in {di} !");
            }
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [SlashCommand("resetuser", "Resets a user's stats")]
        public async Task resetuser(IUser user)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            if (user.Id == 135446225565515776)
            {
                await FollowupAsync("No!" +
                    "\nMaybe I'll reset your stats instead if you are not careful");
                return;
            }
            var builder = new ComponentBuilder()
                .WithButton("Confirm Reset", $"reset-user:{Context.User.Id}", ButtonStyle.Danger);
            var promptMessage = await FollowupAsync("You sure?", components: builder.Build());
            await Task.Delay(8000);
            var disabledBuilder = new ComponentBuilder()
                .WithButton("Confirm Reset", $"reset-user:{Context.User.Id}", ButtonStyle.Danger, disabled: true);
            await promptMessage.ModifyAsync(msg => 
            {
                msg.Components = disabledBuilder.Build();
            });

        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("pay", "Pay someone else some of your tokens")]
        public async Task pay(IUser usertopay, int amount, string comment = "No comment")
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }

            var user = Context.User as SocketGuildUser;
            var userToPay = usertopay as SocketGuildUser;
            if (user == null || userToPay == null)
            {
                await FollowupAsync("Both users must be members of this server.");
                return;
            }

            if (_cachingService.GetBlackList().Contains(usertopay.Id))
            {
                await FollowupAsync("You can't pay blacklisted users!");
                return;
            }

            bool userInDb = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);
            bool userToPayInDb = _cachingService.IsInDatabase(Context.Guild.Id, userToPay.Id);

            if (!userInDb)
            {
                var followup = await FollowupAsync("User not in the database! Adding user...");
                Database.EnterUser(user);
                await followup.ModifyAsync(msg => 
                {
                    msg.Content = "User added! Try running the command again.";
                });
                return;
            }

            if (!userToPayInDb)
            {
                var followup = await FollowupAsync("Target user not in the database! Adding user...");
                Database.EnterUser(userToPay);
                await followup.ModifyAsync(msg => 
                {
                    msg.Content = "Target user added! Try running the command again.";
                });
                return;
            }

            var userTable = Database.GetUserStatus(user);
            if (amount <= 0)
            {
                await FollowupAsync("Don't attempt to steal tokens from people!");
                return;
            }

            if (userTable.FirstOrDefault().Tokens < amount)
            {
                await FollowupAsync("You don't have enough tokens to pay.");
                return;
            }

            Database.RemoveTokens(user, amount);
            Database.ChangeTokens(userToPay, amount);

            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255),
                Description = $"{user.Mention} has paid {usertopay.Mention} {amount} tokens!\n{comment}"
            }.Build();

            await FollowupAsync("", null, false, false, null, null, null, embed);
        }
        [SlashCommand("info", "Displays info about the bot")]
        public async Task info()
        {
            await DeferAsync();
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            int? totalmembers = 0;
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
            }).AddField("Developers:", $"Bot developer: {await Context.Client.GetUserAsync(135446225565515776)}" +
                                    $"\nWeb developer: {await Context.Client.GetUserAsync(208624502878371840)}", true)
            .AddField("Other info:", "I am in " + Context.Client.Guilds.Count + " servers!" +
            "\n" + totalmembers + " members across all servers!" +
            "\nUptime: " + uptime.Days + " Days " + uptime.Hours + " Hours " + uptime.Minutes + " Minutes " + uptime.Seconds + " Seconds" +
            "\nAverage messages per min since startup: " + _statService.msgCounter/_statService.uptime.TotalMinutes, true)
            .AddField("My server:", "https://discord.gg/UPG8Vqb", true).AddField("Website:", "https://www.sketchbot.xyz", true);
            var embed = builder.Build();
            await FollowupAsync("", null,false,false,null,null,null,embed);
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("serverinfo", "Displays info about the server")]
        public async Task serverinfo()
        {
            await DeferAsync();
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
            await FollowupAsync("", null,false,false,null,null,null,embed);
        }
        [RequireContext(ContextType.Guild)]
        [UserCommand("userinfo")]
        public async Task userinfo(IUser user)
        {
            await DeferAsync();
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
            var roles = "";
            foreach(var role in ((IGuildUser)user).RoleIds)
            {
                roles += $"<@&{role}>\n";
            }

            builder.AddField("Joined", ((IGuildUser)user).JoinedAt, true).AddField("Join Position", joinedpos, true)
                .AddField("Registered", user.CreatedAt)
                .AddField($"Roles [{((IGuildUser)user).RoleIds.Count}]", roles);
            var embed = builder.Build();
            await FollowupAsync("", null,false,false,null,null,null,embed);
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("userinfo", "Displays information about the user")]
        public async Task slashuserinfo(IUser user)
        {
            await DeferAsync();
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
            var roles = "";
            foreach (var role in ((IGuildUser)user).RoleIds)
            {
                roles += $"<@&{role}>\n";
            }

            builder.AddField("Joined", ((IGuildUser)user).JoinedAt, true).AddField("Join Position", joinedpos, true)
                .AddField("Registered", user.CreatedAt)
                .AddField($"Roles [{((IGuildUser)user).RoleIds.Count}]", roles);
            var embed = builder.Build();
            await FollowupAsync("", null, false, false, null, null, null, embed);
        }
        [SlashCommand("jojo", "Sends a random JoJo image")]
        public async Task Jojosbizzareadventure()
        {
            await DeferAsync();
            _rand = new Random();
            var dir = new DirectoryInfo("Jojo");
            var dirFiles = dir.GetFiles();
            int fileIndex = _rand.Next(dirFiles.Length);
            int pictureNumber = fileIndex + 1;
            string fileToPost = dirFiles[fileIndex].FullName;
            await Context.Channel.SendFileAsync(fileToPost, "Jojo's bizzare adventure " + pictureNumber);
            await FollowupAsync();
        }
        [SlashCommand("random", "Sends a random message")]
        public async Task Random()
        {
            await DeferAsync();
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
            int randomFileIndex = _rand.Next(RandomMessages.Length);
            int messageNumber = randomFileIndex + 1;
            string fileToPost = RandomMessages[randomFileIndex];
            await FollowupAsync(fileToPost);
            
        }
    }
}