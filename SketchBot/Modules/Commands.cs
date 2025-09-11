using Discord;
using Discord.Interactions;
using Discord.Rest;
//using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList;
using DiscordBotsList.Api;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
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
        private readonly InteractiveService _interactive;
        public Commands(DiscordBotsListService service, TimerService service2, StatService service3, CachingService service4, InteractionService service5, InteractiveService interactive)
        {
            _discordBotListService = service;
            _timerService = service2;
            _statService = service3;
            _cachingService = service4;
            _interactionService = service5;
            _interactive = interactive;
        }

        [SlashCommand("repeat", "Echo a message")]
        public async Task RepeatAsync(string input)
        {
            await DeferAsync();
            await FollowupAsync($"{Context.User.Mention} < {input}");
            
        }
        [RequireUserPermission(GuildPermission.SendTTSMessages)]
        [SlashCommand("repeattts", "Echo a message")]
        public async Task RepeatTTSAsync(string input)
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
            string timestamp = DateTime.Now.ToString("HH:mm:ss", ci);

            if (specialRatings.TryGetValue(input, out var special))
            {
                await FollowupAsync($"I rate {input} **{special.rating}** out of 100");
            }
            else
            {
                var rand = new Random();
                int randomScore = rand.Next(1001); // 0–1000
                double rating = randomScore / 10.0;
                await FollowupAsync($"I rate {input} **{rating}** out of 100");
            }
        }

        [SlashCommand("roll", "Rolls between x and y")]
        public async Task RollAsync(int min = 1, int max = 100)
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
        [SlashCommand("choose", "Makes the choice for you between a bunch of listed things seperated by , (comma)")]
        public async Task ChooseAsync([Summary("Choices")] string choices)
        {
            await DeferAsync();
            if (string.IsNullOrWhiteSpace(choices))
            {
                await FollowupAsync("You need to give me at least one choice!");
                return;
            }
            var splitChoices = choices.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splitChoices.Length == 0)
            {
                await FollowupAsync("You need to give me at least one choice!");
                return;
            }
            _rand = new Random();
            int randomIndex = _rand.Next(splitChoices.Length);
            string chosen = splitChoices[randomIndex];
            await FollowupAsync($"I choose: **{chosen}**");
        }
        [SlashCommand("hello", "Hello")]
        public async Task HelloAsync()
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
        public async Task DonateAsync()
        {
            await DeferAsync();
            await FollowupAsync("https://www.patreon.com/Sketch_Bot");
        }
        [SlashCommand("upvote", "Sends a link for upvoting the bot")]
        public async Task UpvoteAsync()
        {
            await DeferAsync();
            await ReplyAsync($"You can upvote the bot here https://discordbots.org/bot/{Context.Client.CurrentUser.Id}");
            var Api = _discordBotListService.DblApi(Context.Client.CurrentUser.Id);
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
        public async Task GambleAsync(long amount)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }

            var user = Context.User as IGuildUser;
            var userStats = Database.GetUserStats(user);
            long currentTokens = userStats?.Tokens ?? 0;

            if (amount > currentTokens)
            {
                await FollowupAsync("You don't have enough tokens");
                return;
            }
            if (amount < 1)
            {
                await FollowupAsync("The minimum amount of tokens is 1");
                return;
            }

            _rand = new Random();
            int RNG = _rand.Next(0, 100);
            bool won = RNG >= 53;

            if (won)
            {
                Database.AddTokens(user, amount);
                currentTokens += amount;
            }
            else
            {
                Database.RemoveTokens(user, amount);
                currentTokens -= amount;
            }

            var embedBuilder = new EmbedBuilder()
            {
                Title = won ? "You won!" : "You lost!",
                Description = $"You gambled {amount} tokens and rolled {RNG} and {(won ? "won" : "lost")}!\nYou now have {currentTokens} tokens!",
                Color = new Color(0, 0, 255)
            }.WithAuthor(author =>
            {
                author.Name = $"Gambling results - {Context.User.Username}";
                author.IconUrl = Context.User.GetAvatarUrl();
            });

            await FollowupAsync("", null, false, false, null, null, null, embedBuilder.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("gambleall", "Gambles all of your tokens")]
        public async Task GambleAllAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }

            var user = Context.User as IGuildUser;
            var userStats = Database.GetUserStats(user);
            long amount = userStats?.Tokens ?? 0;
            var currentTokens = amount;

            if (amount < 1)
            {
                await FollowupAsync("You dont have any tokens!");
                return;
            }

            _rand = new Random();
            int RNG = _rand.Next(0, 100);
            bool won = RNG >= 53;

            if (won)
            {
                Database.AddTokens(user, amount);
                currentTokens += amount;
            }
            else
            {
                Database.RemoveTokens(user, amount);
                currentTokens -= amount;
            }

            var embedBuilder = new EmbedBuilder()
            {
                Title = won ? "You won!" : "You lost!",
                Description = $"You gambled {amount} tokens and rolled {RNG} and {(won ? "won" : "lost")}!\nYou now have {currentTokens} tokens!",
                Color = new Color(0, 0, 255)
            }.WithAuthor(author =>
            {
                author.Name = $"Gambling results - {Context.User.Username}";
                author.IconUrl = Context.User.GetAvatarUrl();
            });

            await FollowupAsync("", null, false, false, null, null, null, embedBuilder.Build());
        }
        [SlashCommand("ping", "Pong")]
        public async Task PingAsync()
        {
            await DeferAsync();
            await FollowupAsync("Pong!");
        }
        [UserCommand("avatar")]
        public async Task UserAvatarAsync(IUser user)
        {
            await DeferAsync();
            await FollowupAsync(user.GetAvatarUrl(ImageFormat.Auto, 256));
        }
        [SlashCommand("avatar", "Get the avatar of a user")]
        public async Task AvatarAsync(IUser user = null)
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
        public async Task EightballAsync(string input)
        {
            await DeferAsync();
            string[] predictionsTexts =
            [
                "It is very unlikely.",
                "I don't think so...",
                "Yes !",
                "I don't know",
                "No.",
                "Without a doubt",
                "Pls don't",
                "Just give me your money!"
            ];
            _rand = new Random();
            int randomIndex = _rand.Next(predictionsTexts.Length);
            string text = predictionsTexts[randomIndex];
            await FollowupAsync(":8ball: **Question: **" + input + "\n**Answer: **" + text);
            
        }
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("kick", "Kicks someone from the server")]
        public async Task KickAsync(IGuildUser user, string reason = "No reason")
        {
            await DeferAsync();
            var currentUser = Context.User as IGuildUser;
            if (!currentUser.GuildPermissions.KickMembers)
            {
                await FollowupAsync("You do not have Guild permission KickMembers");
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
        public async Task BanAsync(IGuildUser user, string reason = "No reason")
        {
            await DeferAsync();
            var currentUser = Context.User as IGuildUser;
            if (!currentUser.GuildPermissions.BanMembers)
            {
                await FollowupAsync("You do not have Guild permission BanMembers");
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
        public async Task UnbanAsync(RestUser user, string reason = "No reason")
        {
            await DeferAsync();
            if ((Context.Client.CurrentUser as IGuildUser).GuildPermissions.BanMembers)
            {
                await Context.Guild.RemoveBanAsync(user);
                await FollowupAsync(user.Username + " has been unbanned" +
                    "\n" +
                    "\n" + reason);
            }
        }
        [SlashCommand("status", "Checks to see if a website is up")]
        public async Task StatusAsync(string websiteUrl = "http://sketchbot.xyz")
        {
            await DeferAsync();

            string description;

            try
            {
                if (!websiteUrl.StartsWith("https://") && !websiteUrl.StartsWith("http://"))
                {
                    websiteUrl = "https://" + websiteUrl;
                }
                using var httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(websiteUrl);
                if (response.IsSuccessStatusCode)
                {
                    description = "server is **online**";
                }
                else
                {
                    description = "server is **offline**";
                }
            }
            catch (WebException)
            {
                description = "connection is **not available**";
            }

            var embed = new EmbedBuilder
            {
                Title = "Status",
                Description = $"{websiteUrl} \n\n{description}",
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
        [RequireContext(ContextType.Guild)]
        [SlashCommand("nickname", "Changes your nickname")]
        public async Task NicknameAsync(IGuildUser targetUser, string newNickname)
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
        public async Task CatAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });//This is like the 'webbrowser' (?)
                string websiteUrl = "http://aws.random.cat/meow";
                client.BaseAddress = new Uri(websiteUrl);
                HttpResponseMessage response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string catImage = json["file"].ToString();
                await FollowupAsync(catImage);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("fox", "Sends a random fox image")]
        public async Task FoxAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler
                { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
                var websitee = "https://randomfox.ca/floof/";
                client.BaseAddress = new Uri(websitee);
                HttpResponseMessage response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string foxImage = json["image"].ToString();
                await FollowupAsync($"{foxImage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("birb", "Sends a random birb bird image")]
        public async Task BirbAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler
                { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
                var websitee = "https://random.birb.pw/img/";
                string websiteurl = "http://random.birb.pw/tweet.json/";
                client.BaseAddress = new Uri(websiteurl);
                HttpResponseMessage response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string birbImage = json["file"].ToString();
                await FollowupAsync($"{websitee}{birbImage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("duck", "Posts a random picture of a dog")]
        public async Task DuckAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });//This is like the 'webbrowser' (?)
                string websiteUrl = "https://random-d.uk/api/v1/random";
                client.BaseAddress = new Uri(websiteUrl);
                HttpResponseMessage response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string duckImage = json["url"].ToString();
                await FollowupAsync(duckImage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("dog", "Posts a random picture of a dog")]
        public async Task DogAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });//This is like the 'webbrowser' (?)
                string websiteurl = "https://random.dog/woof.json";
                client.BaseAddress = new Uri(websiteurl);
                HttpResponseMessage response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(result);
                string dogImage = json["url"].ToString();
                await FollowupAsync(dogImage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("calculate", "Calculates a math problem")]
        public async Task CalculateAsync(HelperFunctions.Calculation expression)
        {
            await DeferAsync();
            try
            {
                await FollowupAsync($"`{expression.Equation}` = {expression.Result}");
            }
            catch
            {
                await FollowupAsync(expression.Error);
            }
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("membercount", "Tells you how many users are in the guild")]
        public async Task MemberCountAsync()
        {
            await DeferAsync();

            var bots = Context.Guild.Users.Count(x => x.IsBot);
            var members = Context.Guild.MemberCount;
            double ratio = (double)bots / (double)members;
            double percentage = Math.Round((double)bots / (double)members, 3) * 100;
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Title = $"Member count for {Context.Guild.Name}",
                Description = $"{Context.Guild.MemberCount} Total members ({percentage}% bots)\n" +
                $"{Context.Guild.Users.Count(x => x.IsBot)} Bots\n" +
                $"{members - Context.Guild.Users.Count(x => x.IsBot)} Users\n" +
                $"{ratio} Bot to user ratio",
                Color = new Color(0, 0, 255)
            };
            await FollowupAsync("", null, false, false, null, null, null, embedBuilder.Build());
            
        }
        
        [RequireContext(ContextType.Guild)]
        [UserCommand("stats")]
        //[Alias("level","profile")]
        public async Task UserStatsAsync(IGuildUser user)
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
            var blacklistCheck = _cachingService.GetBlackList().Contains(user.Id);
            if (blacklistCheck)
            {
                await FollowupAsync("This user is blacklisted from using this command.");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            var displayName = user.Nickname ?? user.DisplayName;
            Database.CreateTable(Context.Guild.Id);
            var userCheckResult = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);

            if (!userCheckResult)
            {
                _cachingService.SetupUserInDatabase(user.Guild.Id, user as SocketGuildUser);
            }

            var userStats = Database.GetUserStats(user);
            embed.Title = "Stats for " + displayName;
            embed.Description = userStats.Tokens + " tokens:small_blue_diamond:" +
                "\nLevel " + userStats.Level +
                "\nXP " + userStats.XP + " out of " + XP.caclulateNextLevel(userStats.Level);
            await FollowupAsync("", embed: embed.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("stats", "Display a user's level and token")]
        //[Alias("level","profile")]
        public async Task SlashUserStatsAsync(IGuildUser user = null)
        {
            await DeferAsync();
            user ??= Context.User as IGuildUser;
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
            var blacklistCheck = _cachingService.GetBlackList().Contains(user.Id);
            if (blacklistCheck)
            {
                await FollowupAsync("This user is blacklisted from using this command.");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            var displayName = user.Nickname ?? user.DisplayName;
            Database.CreateTable(Context.Guild.Id);
            var userCheckResult = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);

            if (!userCheckResult)
            {
                _cachingService.SetupUserInDatabase(Context.Guild.Id, user as SocketGuildUser);
            }

            var userStats = Database.GetUserStats(user);
            embed.Title = "Stats for " + displayName;
            embed.Description = userStats.Tokens + " tokens:small_blue_diamond:" +
                "\nLevel " + userStats.Level +
                "\nXP " + userStats.XP + " out of " + XP.caclulateNextLevel(userStats.Level);
            var builtEmbed = embed.Build();
            await FollowupAsync("", [builtEmbed]);
        }
        [SlashCommand("invite", "Invite me to your server")]
        public async Task InviteAsync()
        {
            await DeferAsync();
            await FollowupAsync("**" + Context.User.Username + "**, use this URL to invite me" +
                $"\nhttps://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=1617578818631&scope=bot%20applications.commands");
        }
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("purge", "Purges messages from the channel")]
        public async Task PurgeAsync(uint amount)
        {
            await DeferAsync();
            if ((Context.User as IGuildUser).GuildPermissions.ManageMessages == true)
            {
                var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                await FollowupAsync($"Purge completed.", ephemeral:true);
                await (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages);
            }
            else
            {
                await FollowupAsync("You do not have guild permission ManageMessages", ephemeral:true);
                
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("award", "Give someone tokens")]
        public async Task AwardTokensAsync(IGuildUser guildUser, int tokens, string comment = "")
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var name = guildUser.Nickname ?? guildUser.DisplayName;
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var userExists = _cachingService.IsInDatabase(Context.Guild.Id, guildUser.Id);
                if (userExists)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 255)
                    };
                    Database.AddTokens(guildUser, tokens);
                    embed.Title = name + " was awarded " + tokens + " tokens!";
                    embed.Description = comment;
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                }
                else
                {
                    await FollowupAsync("This user is not in the database");
                }
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        [Custom_Preconditions.Ratelimit(1, 5, Custom_Preconditions.Measure.Minutes, Custom_Preconditions.RatelimitFlags.ApplyPerGuild)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("awardall", "Give everyone on the server some tokens")]
        public async Task AwardTokensToEveryoneAsync(int tokens, string comment = "")
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                await Context.Guild.DownloadUsersAsync();
                var users = Context.Guild.Users;
                foreach (var user in users)
                {
                    var isUserInDatabase = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);
                    if (!isUserInDatabase)
                    {
                        _cachingService.SetupUserInDatabase(Context.Guild.Id, user);
                    }
                    Database.AddTokens(user, tokens);
                }
                var embed = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 255)
                };

                embed.Title = "All users were awarded " + tokens + " tokens!";
                embed.Description = comment;
                var builtEmbed = embed.Build();
                await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        
        [RequireContext(ContextType.Guild)]
        [SlashCommand("serversettings", "View the server settings")]
        public async Task ViewSettingsAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            Serversettings? settings = _cachingService.GetServerSettings(Context.Guild.Id);

            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255),
                Title = "Server Settings"
            };

            embed.WithAuthor(author =>
            {
                author.Name = Context.Guild.Name;
                author.IconUrl = Context.Guild.IconUrl;
            });

            embed.WithFooter(footer =>
            {
                footer.Text = $"Guild ID: {Context.Guild.Id}";
                footer.IconUrl = Context.Guild.IconUrl;
            });
            embed.Timestamp = DateTimeOffset.Now;

            if (settings == null)
            {
                embed.Description = "No settings found for this server.";
            }
            else
            {
                embed.AddField("Prefix", string.IsNullOrEmpty(settings.Prefix) ? "?" : settings.Prefix)
                    .AddField("Welcome Channel", settings.WelcomeChannel != 0 ? $"<#{settings.WelcomeChannel}>" : "Not set", true)
                    .AddField("Modlog Channel", settings.ModlogChannel != 0 ? $"<#{settings.ModlogChannel}>" : "Not set", true)
                    .AddField("Levelup Messages", settings.LevelupMessages ? "Enabled" : "Disabled", true);
            }

            await FollowupAsync("", null, false, false, null, null, null, embed.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("pay", "Pay someone else some of your tokens")]
        public async Task PayTokensAsync(IGuildUser usertopay, int amount, string comment = "No comment")
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
                await Context.Guild.DownloadUsersAsync();
                user = Context.User as SocketGuildUser;
                userToPay = usertopay as SocketGuildUser;
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
                _cachingService.SetupUserInDatabase(Context.Guild.Id, user);
            }

            if (!userToPayInDb)
            {
                _cachingService.SetupUserInDatabase(Context.Guild.Id, userToPay);
            }

            var userStats = Database.GetUserStats(user);
            if (amount <= 0)
            {
                await FollowupAsync("Don't attempt to steal tokens from people!");
                return;
            }

            if (userStats.Tokens < amount)
            {
                await FollowupAsync("You don't have enough tokens to pay.");
                return;
            }

            Database.RemoveTokens(user, amount);
            Database.AddTokens(userToPay, amount);

            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255),
                Description = $"{user.Mention} has paid {usertopay.Mention} {amount} tokens!\n{comment}"
            }.Build();

            await FollowupAsync("", null, false, false, null, null, null, embed);
        }
        [SlashCommand("info", "Displays info about the bot")]
        public async Task BotInfoAsync()
        {
            await DeferAsync();
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            int totalMembers = Context.Client.Guilds.Sum(g => g.MemberCount);
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
                .WithUrl($"https://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=1617578818631&scope=bot%20applications.commands");
            }).AddField("Developers:", $"Bot developer: {await Context.Client.GetUserAsync(135446225565515776)}" +
                                    $"\nWeb developer: {await Context.Client.GetUserAsync(208624502878371840)}", true)
            .AddField("Other info:", "I am in " + Context.Client.Guilds.Count + " servers!" +
            "\n" + totalMembers + " members across all servers!" +
            "\nUptime: " + uptime.Days + " Days " + uptime.Hours + " Hours " + uptime.Minutes + " Minutes " + uptime.Seconds + " Seconds" +
            "\nAverage messages per min since startup: " + _statService.msgCounter/_statService.uptime.TotalMinutes, true)
            .AddField("My server:", "https://discord.gg/UPG8Vqb", true).AddField("Website:", "https://www.sketchbot.xyz", true);
            await FollowupAsync("", null, false, false, null, null, null, builder.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("serverinfo", "Displays info about the server")]
        public async Task GuildInfoAsync()
        {
            await DeferAsync();
            var guild = Context.Guild;
            try
            {
                await guild.DownloadUsersAsync();

                var textChannels = guild.TextChannels
                    .Select(x => (x.Id, x.Mention))
                    .Where(x => !guild.VoiceChannels.Select(y => y.Id).Contains(x.Id));

                var voiceChannels = guild.VoiceChannels
                    .Where(x => x is not SocketStageChannel)
                    .Select(x => (x.Id, x.Mention));

                var stageChannels = guild.VoiceChannels
                    .Where(x => x is SocketStageChannel)
                    .Select(x => (x.Id, x.Mention));

                var forumChannels = guild.Channels
                    .Where(x => x is SocketForumChannel)
                    .Select(x => (x.Id, x.Name));

                var mediaChannels = guild.Channels
                    .Where(x => x is SocketMediaChannel)
                    .Select(x => (x.Id, x.Name));

                // Separate forum threads from text channels
                var forumThreads = guild.ThreadChannels
                    .Where(x => x.ParentChannel is SocketForumChannel)
                    .Select(x => (x.Id, x.Mention));

                var textThreads = guild.ThreadChannels
                    .Where(x => x.ParentChannel is SocketTextChannel)
                    .Select(x => (x.Id, x.Mention));

                // Remove forum threads from text channels field
                var textChannelsAndThreadsEnumerable = textChannels
                    .Concat(textThreads)
                    .Where(x => !(guild.ThreadChannels.Any(t => t.Id == x.Id && t.ParentChannel is SocketForumChannel)))
                    .GroupBy(x => x.Id)
                    .Select(g => g.First().Mention);

                int realCount = guild.MemberCount;
                int userCount = guild.Users.Count(x => !x.IsBot);
                int botCount = guild.Users.Count(x => x.IsBot);
                int totalCount = userCount + botCount;
                int unavailableCount = realCount - totalCount;

                string memberCountField = unavailableCount > 0
                    ? $"{userCount} Users\n{botCount} Bots\n{unavailableCount} Unavailable"
                    : $"{userCount} Users\n{botCount} Bots";

                string textChannelsAndThreads = HelperFunctions.JoinWithLimit(textChannelsAndThreadsEnumerable, 1024, "\n");
                string voiceChannelsList = voiceChannels.Any()
                    ? HelperFunctions.JoinWithLimit(voiceChannels.Select(x => x.Mention), 1024, "\n")
                    : "None";
                string stageChannelsList = stageChannels.Any()
                    ? HelperFunctions.JoinWithLimit(stageChannels.Select(x => x.Mention), 1024, "\n")
                    : "None";
                string forumChannelsList = forumChannels.Any()
                    ? HelperFunctions.JoinWithLimit(forumChannels.Select(x => x.Name), 1024, "\n")
                    : "None";
                string mediaChannelsList = mediaChannels.Any()
                    ? HelperFunctions.JoinWithLimit(mediaChannels.Select(x => x.Name), 1024, "\n")
                    : "None";
                string forumThreadsList = forumThreads.Any()
                    ? HelperFunctions.JoinWithLimit(forumThreads.Select(x => x.Mention), 1024, "\n")
                    : "None";

                var pages = new List<IPageBuilder>();

                // Page 1: General summary
                pages.Add(new PageBuilder()
                    .WithTitle($"{guild.Name} - Overview")
                    .WithDescription($"Owner: {guild.Owner?.Mention ?? "Unknown"}\nMembers: {realCount}\nChannels: {guild.Channels.Count}\nRoles: {guild.Roles.Count}\nEmojis: {guild.Emotes.Count}\nStickers: {guild.Stickers.Count}")
                    .AddField($"Total Channels ({guild.Channels.Count})",
                        $"{textChannels.Count()} Text\n{voiceChannels.Count()} Voice\n{stageChannels.Count()} Stage\n{forumChannels.Count()} Forum\n{forumThreads.Count()} Forum threads\n{guild.CategoryChannels.Count} Category channels\n{mediaChannels.Count()} Media", true)
                    .AddField($"Member Count ({realCount})", memberCountField, true)
                    .AddField($"Verification Level", guild.VerificationLevel.ToString(), true)
                    .AddField($"Boost Level", $"{guild.PremiumTier} ({guild.PremiumSubscriptionCount} boosts)", true)
                    .AddField($"Region", string.IsNullOrWhiteSpace(guild.VoiceRegionId) ? "None" : guild.VoiceRegionId, true)
                    .AddField($"Vanity", guild.Features.HasVanityUrl ? guild.VanityURLCode : "No Vanity", true)
                    .AddField($"Icon URL", string.IsNullOrWhiteSpace(guild.IconUrl) ? "No Icon" : guild.IconUrl, true)
                    .AddField($"Banner URL", string.IsNullOrWhiteSpace(guild.BannerUrl) ? "No Banner" : guild.BannerUrl, true)
                    .WithThumbnailUrl(guild.IconUrl ?? "")
                    .WithColor(new Color(0, 255, 0))
                    .WithFooter($"ID: {guild.Id} | Server Created: {guild.CreatedAt:yyyy-MM-dd}")
                    .WithAuthor(guild.Name, guild.IconUrl)
                );

                // Page 2: Guild Features (all 'Has' boolean properties)
                var featureProps = guild.Features.GetType().GetProperties()
                    .Where(p => p.Name.StartsWith("Has", StringComparison.OrdinalIgnoreCase) && p.PropertyType == typeof(bool))
                    .ToList();
                var featuresPage = new PageBuilder()
                    .WithTitle($"{guild.Name} - Features")
                    .WithColor(new Color(0, 255, 0));
                foreach (var prop in featureProps)
                {
                    var value = (bool)prop.GetValue(guild.Features);
                    var displayName = prop.Name.StartsWith("Has") ? prop.Name.Substring(3) : prop.Name;
                    featuresPage.AddField(displayName, value ? "True" : "False", true);
                }
                if (guild.Features.Experimental != null)
                {
                    var experimentalProps = guild.Features.Experimental;
                    if (experimentalProps.Count > 0)
                    {
                        string expList = string.Join("\n", experimentalProps);
                        featuresPage.AddField("Experimental Features", expList, false);
                    }
                }
                pages.Add(featuresPage);

                // Page 3: Text Channels & Threads
                pages.Add(new PageBuilder()
                    .WithTitle($"{guild.Name} - Text Channels & Threads")
                    .WithDescription(textChannelsAndThreads.Any() ? HelperFunctions.JoinWithLimit(textChannelsAndThreadsEnumerable, 4096, "\n") : "None")
                    .WithAuthor($"Guild info for {guild.Name}:", guild.IconUrl)
                    .WithColor(new Color(0, 255, 0))
                );

                // Page 4: Voice, Stage, Forum, Media Channels
                pages.Add(new PageBuilder()
                    .WithTitle($"{guild.Name} - Other Channels")
                    .AddField($"Voice Channels ({voiceChannels.Count()})", voiceChannelsList)
                    .AddField($"Stage Channels ({stageChannels.Count()})", stageChannelsList)
                    .AddField($"Forum Channels ({forumChannels.Count()})", forumChannelsList)
                    .AddField($"Forum Threads ({forumThreads.Count()})", forumThreadsList)
                    .AddField($"Media Channels ({mediaChannels.Count()})", mediaChannelsList)
                    .WithAuthor($"Guild info for {guild.Name}:", guild.IconUrl)
                    .WithColor(new Color(0, 255, 0))
                );

                // Page 5: Roles (split if needed)
                var rolesList = guild.Roles.OrderByDescending(x => x.Position).Select(x => x.Mention).ToList();
                var rolesPages = HelperFunctions.SplitListByCharLimit(rolesList, 4096, "\n");
                for (int i = 0; i < rolesPages.Count; i++)
                {
                    pages.Add(new PageBuilder()
                        .WithTitle($"{guild.Name} - Roles (Page {i + 1}/{rolesPages.Count})")
                        .WithDescription(rolesPages[i])
                        .WithAuthor($"Guild info for {guild.Name}:", guild.IconUrl)
                        .WithColor(new Color(0, 255, 0))
                    );
                }

                // Page 6 & 7: Emojis & Stickers
                var emojis = guild.Emotes.Select(x => x.ToString()).ToList();
                var stickers = guild.Stickers.Select(x => x.Name).ToList();

                // Emojis page
                if (emojis.Count > 0)
                {
                    var emojiPages = HelperFunctions.SplitListByCharLimit(emojis, 4096, "");
                    for (int i = 0; i < emojiPages.Count; i++)
                    {
                        pages.Add(new PageBuilder()
                            .WithTitle($"{guild.Name} - Emojis ({emojis.Count}) (Page {i + 1}/{emojiPages.Count})")
                            .WithDescription(emojiPages[i])
                            .WithAuthor($"Guild info for {guild.Name}:", guild.IconUrl)
                            .WithColor(new Color(0, 255, 0))
                        );
                    }
                }
                else
                {
                    pages.Add(new PageBuilder()
                        .WithTitle($"{guild.Name} - Emojis ({emojis.Count})")
                        .WithDescription("None")
                        .WithAuthor($"Guild info for {guild.Name}:", guild.IconUrl)
                        .WithColor(new Color(0, 255, 0))
                    );
                }

                // Stickers page
                if (stickers.Count > 0)
                {
                    var stickerPages = HelperFunctions.SplitListByCharLimit(stickers, 4096, ", ");
                    for (int i = 0; i < stickerPages.Count; i++)
                    {
                        pages.Add(new PageBuilder()
                            .WithTitle($"{guild.Name} - Stickers ({stickers.Count}) (Page {i + 1}/{stickerPages.Count})")
                            .WithDescription(stickerPages[i])
                            .WithAuthor($"Guild info for {guild.Name}:", guild.IconUrl)
                            .WithColor(new Color(0, 255, 0))
                        );
                    }
                }
                else
                {
                    pages.Add(new PageBuilder()
                        .WithTitle($"{guild.Name} - Stickers ({stickers.Count})")
                        .WithDescription("None")
                        .WithAuthor($"Guild info for {guild.Name}:", guild.IconUrl)
                        .WithColor(new Color(0, 255, 0))
                    );
                }

                // Send paginator
                var paginator = new StaticPaginatorBuilder()
                    .AddUser(Context.User)
                    .WithPages(pages)
                    .WithFooter(PaginatorFooter.PageNumber)
                    .Build();

                await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(5), InteractionResponseType.DeferredChannelMessageWithSource);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await FollowupAsync($"{ex.GetType()}: {ex.Message}");
            }
        }
        [RequireContext(ContextType.Guild)]
        [UserCommand("userinfo")]
        public async Task UserInfoAsync(IUser user)
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
            await FollowupAsync("", null, false, false, null, null, null, builder.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("userinfo", "Displays information about the user")]
        public async Task SlashUserInfoAsync(IGuildUser user)
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
            await FollowupAsync("", null, false, false, null, null, null, builder.Build());
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
            int randomMessageIndex = _rand.Next(RandomMessages.Length);
            int selectedMessageNumber = randomMessageIndex + 1;
            string messageToSend = RandomMessages[randomMessageIndex];
            await FollowupAsync(messageToSend);
            
        }
    }
}