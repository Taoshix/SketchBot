using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBotsList;
using DiscordBotsList.Api;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json.Linq;
using SketchBot.Custom_Preconditions;
using SketchBot.Database;
using SketchBot.Handlers;
using SketchBot.Models;
using SketchBot.Services;
using SketchBot.Utils;
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
using YouTubeSearch;

namespace SketchBot.InteractionBasedModules
{
    public class MiscellaneousModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordBotsListService _discordBotListService;
        private readonly TimerService _timerService;
        private readonly StatService _statService;
        private readonly CachingService _cachingService;
        private readonly InteractionService _interactionService;
        private readonly InteractiveService _interactive;
        private readonly MemeService _memeService;
        private Random _rand;

        public MiscellaneousModule(
            DiscordBotsListService discordBotListService,
            TimerService timerService,
            StatService statService,
            CachingService cachingService,
            InteractionService interactionService,
            InteractiveService interactive,
            MemeService memeService)
        {
            _discordBotListService = discordBotListService;
            _timerService = timerService;
            _statService = statService;
            _cachingService = cachingService;
            _interactionService = interactionService;
            _interactive = interactive;
            _memeService = memeService;
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

            if (specialRatings.TryGetValue(input, out var special))
            {
                await FollowupAsync($"I rate {input} **{special.rating}** out of 100");
            }
            else
            {
                var rand = new Random();
                double rating = rand.Next(1001) / 10.0;
                await FollowupAsync($"I rate {input} **{rating}** out of 100");
            }
        }

        [SlashCommand("roll", "Rolls between x and y")]
        public async Task RollAsync(int min = 1, int max = 100)
        {
            await DeferAsync();
            _rand = new Random();
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

        [SlashCommand("choose", "Makes the choice for you between a bunch of listed things")]
        public async Task ChooseAsync([Summary("Choices", "Each choice is separated by , (comma)")] string choices)
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
            string chosen = splitChoices[_rand.Next(splitChoices.Length)];
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
            var api = _discordBotListService.DblApi(Context.Client.CurrentUser.Id);
            if (await api.HasVoted(Context.User.Id))
            {
                await FollowupAsync("Thanks for voting today!");
            }
            else
            {
                await FollowupAsync("You have not voted today");
            }
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
            user ??= Context.User;
            var embed = new EmbedBuilder()
                .WithColor(new Color(0x4900ff))
                .WithTitle($"{user.Username}'s Avatar")
                .WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 256));
            await FollowupAsync(embed: embed.Build());
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
            string text = predictionsTexts[_rand.Next(predictionsTexts.Length)];
            await FollowupAsync($":8ball: **Question: **{input}\n**Answer: **{text}");
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
                description = response.IsSuccessStatusCode ? "server is **online**" : "server is **offline**";
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
            await FollowupAsync(embed: embed.Build());
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
            double ratio = bots / (double)members;
            double percentage = Math.Round(ratio, 3) * 100;
            var embedBuilder = new EmbedBuilder()
            {
                Title = $"Member count for {Context.Guild.Name}",
                Description = $"{members} Total members ({percentage}% bots)\n" +
                              $"{bots} Bots\n" +
                              $"{members - bots} Users\n" +
                              $"{ratio} Bot to user ratio",
                Color = new Color(0, 0, 255)
            };
            await FollowupAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("invite", "Invite me to your server")]
        public async Task InviteAsync()
        {
            await DeferAsync();
            await FollowupAsync($"**{Context.User.Username}**, use this URL to invite me" +
                $"\nhttps://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=1617578818631&scope=bot%20applications.commands");
        }

        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("purge", "Purges messages from the channel")]
        public async Task PurgeAsync(uint amount)
        {
            await DeferAsync();
            if ((Context.User as IGuildUser).GuildPermissions.ManageMessages)
            {
                var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                await FollowupAsync("Purge completed.", ephemeral: true);
                await (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages);
            }
            else
            {
                await FollowupAsync("You do not have guild permission ManageMessages", ephemeral: true);
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
            var settings = _cachingService.GetServerSettings(Context.Guild.Id);
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
                    .AddField("Levelup Messages", settings.LevelupMessages ? "Enabled" : "Disabled", true)
                    .AddField("XP Multiplier", settings.XpMultiplier);
            }
            await FollowupAsync(embed: embed.Build());
        }

        [SlashCommand("botinfo", "Displays info about the bot")]
        public async Task BotInfoAsync()
        {
            await DeferAsync();
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            int totalMembers = Context.Client.Guilds.Sum(g => g.MemberCount);
            _rand = new Random();
            string formattedUptime = $"{uptime.Days:D2}:{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
            double avgMsgPerMin = _statService.uptime.TotalMinutes > 0
                ? Math.Round(_statService.msgCounter / _statService.uptime.TotalMinutes, 2)
                : 0.00;

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
                })
                .AddField("Developers:", $"Bot developer: {await Context.Client.GetUserAsync(135446225565515776)}" +
                                         $"\nWeb developer: {await Context.Client.GetUserAsync(208624502878371840)}", true)
                .AddField("Other info:", $"I am currently in **{Context.Client.Guilds.Count}** servers!\n" +
                    $"**{totalMembers}** members across all servers!\n" +
                    $"Uptime: {formattedUptime} (DD:HH:mm:ss)\n" +
                    $"Average messages per min since startup: **{avgMsgPerMin}**", true)
                .AddField("Website:", "https://www.sketchbot.xyz")
                .AddField("My server:", "https://discord.gg/UPG8Vqb", true);
            await FollowupAsync(embed: builder.Build());
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
                var forumThreads = guild.ThreadChannels
                    .Where(x => x.ParentChannel is SocketForumChannel)
                    .Select(x => (x.Id, x.Mention));
                var textThreads = guild.ThreadChannels
                    .Where(x => x.ParentChannel is SocketTextChannel)
                    .Select(x => (x.Id, x.Mention));
                var textChannelsAndThreadsEnumerable = textChannels
                    .Concat(textThreads)
                    .Where(x => !guild.ThreadChannels.Any(t => t.Id == x.Id && t.ParentChannel is SocketForumChannel))
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
                    var displayName = prop.Name.StartsWith("Has") ? prop.Name[3..] : prop.Name;
                    featuresPage.AddField(displayName, value ? "True" : "False", true);
                }
                if (guild.Features.Experimental != null && guild.Features.Experimental.Count > 0)
                {
                    string expList = string.Join("\n", guild.Features.Experimental);
                    featuresPage.AddField("Experimental Features", expList, false);
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
            var builder = new EmbedBuilder()
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
            var roles = string.Join('\n', ((IGuildUser)user).RoleIds.Select(role => $"<@&{role}>"));

            builder.AddField("Joined", ((IGuildUser)user).JoinedAt, true)
                .AddField("Join Position", joinedpos, true)
                .AddField("Registered", user.CreatedAt)
                .AddField($"Roles [{((IGuildUser)user).RoleIds.Count}]", roles);
            await FollowupAsync(embed: builder.Build());
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("userinfo", "Displays information about the user")]
        public async Task SlashUserInfoAsync(IGuildUser user)
        {
            await DeferAsync();
            var builder = new EmbedBuilder()
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
            var roles = string.Join('\n', user.RoleIds.Select(role => $"<@&{role}>"));

            builder.AddField("Joined", user.JoinedAt, true)
                .AddField("Join Position", joinedpos, true)
                .AddField("Registered", user.CreatedAt)
                .AddField($"Roles [{user.RoleIds.Count}]", roles);
            await FollowupAsync(embed: builder.Build());
        }
        [SlashCommand("youtube", "Searches YouTube and returns the first result")]
        public async Task YouTubeSearchAsync(string searchquery)
        {
            await DeferAsync();
            var items = new VideoSearch();
            var item = items.GetVideos(searchquery, 1);
            string url = item.Result.First().getUrl();
            await FollowupAsync(url);
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("roleinfo", "Displays info about a role")]
        public async Task RoleInfoAsync(IRole role)
        {
            await DeferAsync();
            var rolePermissionsList = role.Permissions.ToList();
            string permissionsListString = string.Join("\n", rolePermissionsList);
            var embed = new EmbedBuilder()
            {
                Title = $"Role info for {role.Name}",
                Color = role.Color
            };
            embed.AddField("Id", role.Id);
            embed.AddField("Position", role.Position, true);
            embed.AddField("Members", ((SocketRole)role).Members.Count(), true);
            embed.AddField("Mentionable?", role.IsMentionable);
            embed.AddField("Hoisted?", role.IsHoisted, true);
            embed.AddField("Permissions", permissionsListString);
            embed.AddField("Color", role.Color, true);
            embed.AddField("Role creation date", role.CreatedAt.DateTime.ToString("dd/MM/yy HH:mm:ss"), true);
            await FollowupAsync("", embed: embed.Build());
        }
        [SlashCommand("activity", "Launch a discord activity in a voice channel!")]
        public async Task CreateDiscordActivityAsync(IVoiceChannel chan, DefaultApplications app)
        {
            await DeferAsync();
            var invite = await chan.CreateInviteToApplicationAsync(app);
            await Context.Interaction.FollowupAsync(invite.Url);
        }
        [SlashCommand("emote", "Enlargens an emote")]
        public async Task ShowEmoteAsync(string emote)
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
            await FollowupAsync("", embed: embed);
        }

        [Ratelimit(1, 10, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers | RatelimitFlags.ApplyPerGuild)]
        [SlashCommand("memegen", "Generates a meme")]
        public async Task GenerateMemeAsync([Summary("Template", "The name of the meme template you wish to use"), Autocomplete(typeof(MemeAutoCompleteHandler))] string templateName, string topText, string bottomText)
        {
            await DeferAsync();
            var service = _memeService.GetMemeService();
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

        [SlashCommand("random", "Sends a random message")]
        public async Task Random()
        {
            await DeferAsync();
            _rand = new Random();
            var randomMessages = new[]
            {
                "æøå", "Ecks dee", "123456789", "This is a list", "Random Message", "RNG", "HTX > HHX > STX",
                "STX = Trash", "Mee6 kan ikke commands med flere ord", "?help", "Ingress", "Pokemon GO", "C#",
                "Maple", "Mimouniboi", "9gag", "ArrayList", "Mikerosoft Certified Technician", "Emo", "Anime",
                "133769420", "MLG", "K-WHY-S", "Chrunchyroll", "Agoraen", "Desperat skole hjælp", "Discord",
                "Lidl", "Kantinen er overpriced", "<:sandwich:355316315780677632>", "xD", ":icecube:", "DAB",
                "Programmering", "Gajs", "Gajs, der er time", "Produktudvikling", "Naturvidenskabeligtgrundforløb",
                "boi", "fuccboi", "Hatsune Miku", "Itslearning", "Riskage",
                "How much Ris can a Rischuck chuck if the Rischuck could chuck Rishi", "Newtonmeter",
                "Alle realtal", "Den tomme mængde", "Dramaalert", "Scarce", "Hey what's up guys it's Scarce here",
                "Naturvidenskabelig metode", "Ulduar", "niceme.me", "Rishi", "Nibba", "Plagierkontroler",
                "Mee6 er lårt", "World of Warcraft", "Blizzard", "Elevplan", "Nielsen", "yaaaarrr boi", "Waps",
                "Riskage spil", "AWS", "Amazon", "Ebay", "Aliexpress", "Nordisk film", "Bone's", "EC2", "GIF",
                "Instances", "Storage", "S3", "NVM", "Database", "dEcLaN.eXe", "Mojo", "SQL", "Hello World",
                "HTML", "CSS", "PHP", "JS", "T H I C C", "Samfundsfag = sovetime", "Vuk", "Vektorfunktioner",
                "Vukterfunktioner", "In memory of Vuk", "Sweet Silence", "Gucci gang", "Osu!",
                "What is up AutismAlert nation", "LinusTechTips", "Scrapyard wars", "Tunnelbear",
                "One-energy cola", "Water", "Such message very random", "4:3 Stretched", "Black bars", "Java",
                "Javascript", "Eclipse", "This list is getting looooooooooooooong", "Craigslist", "SLI",
                "Intel & nVidia > AMD", "Razer", "Razer blackwidow chroma", "Stationspizza", "Linus", "Windows",
                "Macbook", "Linux", "Raspberry Pi", "Arduino", "LCD", "Jonte-bro", "Password", "O2Auth",
                "discordapp.com", "discord.gg", "Sodapoppin", "2147483647", "4294967295", "Battlefield Heroes",
                "Fortnite", "Rema 1000", "Fakta", "PUBG", "Playerunknown's Battlegrounds", "Far Cry 5",
                "Far Cry 4", "Far Cry 3", "PewDiePie", "Nick Crompton", "England er min by", "England is my city",
                "?riskage spil", "777", "Jackpot", "Luke", "Thomas Jefferson Chance Morris", "420", "1337", "69",
                "Gaming-linjen", "IT-Videnskab", "Hearthstone", "Its everyday bro", "Snapchat", "Telegram",
                "Minecraft commandblocks", "Ninja", "#weebconfirmed", "Kommunikationsmodeller",
                "What's 9 + 10? 21!", "Divine spirit", "Innerfire", "Legendary", "Legiondary", "Legend",
                "Cities: Skyline", "Test", "Execute", "Leeeeeeerrroooooyyyyyy Jeeeeeeeeeeeeeenkinsssss", "C'Thun",
                "Standard > Wild", "Duplicates", "Wallpaper Engine", "Deez nuts", "Insert meme here", "Ultrasaur",
                "Ban", "Kick", "Help! My creator forces me to respond to commands", "Jonaser", "The ting goes skrrraa",
                "Random message number 200", "Gulag", "Tyskland", "Morten",
            };
            string messageToSend = randomMessages[_rand.Next(randomMessages.Length)];
            await FollowupAsync(messageToSend);
        }
    }
}