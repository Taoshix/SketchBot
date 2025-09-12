using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MoreLinq.Extensions;
using SketchBot.Custom_Preconditions;
using SketchBot.Database;
using SketchBot.Services;
using SketchBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TagLib.Asf;
using Victoria;

namespace SketchBot.TextBasedModules
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
        private LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lavaNode;
        private IServiceProvider _serviceProvider;
        private readonly InteractiveService _interactive;
        public DevModule(DiscordBotsListService service, TimerService service2, StatService service3, CachingService service4, LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode, IServiceProvider serviceProvider, InteractiveService interactive)
        {
            _service = service;
            _timerService = service2;
            _statService = service3;
            _cachingService = service4;
            _lavaNode = lavaNode;
            _serviceProvider = serviceProvider;
            _interactive = interactive;
        }

        [RequireDevelopers]
        [Command("eval", RunMode = RunMode.Async)]
        public async Task EvalAsync([Remainder] string code)
        {
            code = code.Replace("```cs", "");
            code = code.Replace("```", "");
            code = code.Replace("`", "");
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
                    Globals globals = new Globals { Context = Context, Guild = Context.Guild, DblApi = _service.DblApi(Context.Client.CurrentUser.Id), ServiceProvider = _serviceProvider, LavaNode = _lavaNode, CachingService = _cachingService};
                    //object o = await CSharpScript.EvaluateAsync(@"using System;using System.Linq;using System.Threading.Tasks;using System.Collections.Generic;using Discord.WebSocket;using Discord;using System.Net;using System.Net.Http;using OsuSharp;using OsuSharp.UserEndpoint;using OsuSharp.Misc;using OsuSharp.Entities;try{" + @code + "} catch (exception) {}", scriptoptions, globals);
                    object o = await CSharpScript.EvaluateAsync(@"using System;using System.Linq;using System.Threading.Tasks;using System.Collections.Generic;using System.IO;using Discord.WebSocket;using Discord;using System.Net;using System.Net.Http;using DiscordBotsList.Api;using Victoria; using Microsoft.Extensions.DependencyInjection;" + @code, scriptoptions, globals);
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
        public async Task BashAsync([Remainder] string cmd)
        {
            var result = cmd.Bash();
            await ReplyAsync(result);
        }
        [RequireDevelopers]
        [Command("checkowner")]
        public async Task CheckOwnerAsync(ulong id)
        {
            var guilds = Context.Client.Guilds.Where(g => g.OwnerId == id).ToList();
            if (guilds.Any())
            {
                var embed
                    = new EmbedBuilder()
                    .WithTitle($"Guilds Owned by {Context.Client.GetUser(id)?.Username ?? id.ToString()} ({guilds.Count})")
                    .WithDescription(string.Join("\n", guilds.Select(g => $"{g.Name} - {g.Id}")))
                    .WithColor(new Color(0, 255, 0))
                    .Build();
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                await Context.Channel.SendMessageAsync("No servers found!");
            }
        }
        [RequireDevelopers]
        [Command("mutualserver")]
        public async Task ListMutualGuildsAsync(ulong id)
        {
            var user = await Context.Client.GetUserAsync(id) as SocketUser;
            var mutualGuilds = user?.MutualGuilds ?? Context.Client.Guilds.Where(guild => guild.Users.Any(user => user.Id == id)).ToList();

            if (!mutualGuilds.Any())
            {
                await Context.Channel.SendMessageAsync("No servers found!");
                return;
            }

            var foundServers = mutualGuilds
                .Select(guild => $"{guild.Name} - {guild.Id}")
                .ToList();

            // Discord embed description max length is 4096
            var descriptionBuilder = new StringBuilder();
            int count = 0;
            foreach (var server in foundServers)
            {
                if (descriptionBuilder.Length + server.Length + 1 > 4096)
                    break;
                descriptionBuilder.AppendLine(server);
                count++;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"{count} mutual servers found")
                .WithDescription(descriptionBuilder.ToString())
                .WithColor(new Color(0, 0, 255))
                .WithAuthor(user?.Username ?? id.ToString(), user?.GetAvatarUrl())
                .Build();

            await Context.Channel.SendMessageAsync("", false, embed);

            // If there are more servers, notify user
            if (count < foundServers.Count)
            {
                await Context.Channel.SendMessageAsync($"Note: Only the first {count} servers are shown due to embed description length limit.");
            }
        }
        [RequireDevelopers]
        [Command("updatepfp", RunMode = RunMode.Async)]
        public async Task SetAvatarAsync([Remainder] string url = "")
        {
            try
            {
                if (Context.Message.Attachments.Any())
                {
                    url = Context.Message.Attachments.First().Url;
                }

                if (string.IsNullOrWhiteSpace(url))
                {
                    await ReplyAsync("No URL or attachment provided.");
                    return;
                }

                using var httpClient = new HttpClient();
                using var stream = await httpClient.GetStreamAsync(url);
                if (stream == null)
                {
                    await ReplyAsync("Unable to download/verify the URL");
                    return;
                }
                await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(stream));
            }
            catch
            {
                await ReplyAsync("Unable to download/verify the URL");
            }
        }
        [RequireDevelopersSilent]
        [Command("serverlist", RunMode = RunMode.Async)]
        public async Task ServerListPageAsync(int index = 1)
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
        public async Task ServerListSearchAsync([Remainder] string search)
        {

            List<string> guildNames = new List<string>();
            var guilds = Context.Client.Guilds;
            foreach (var guild in guilds.Where(x => x.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)).Take(20))
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
        public async Task ReplyToDMAsync(ulong id, [Remainder] string message)
        {
            var user = Context.Client.GetUser(id);
            if (user == null)
            {
                await ReplyAsync("User not found.");
                return;
            }

            await user.SendMessageAsync(message);

            string senderName = Context.User.Id == 135446225565515776 ? "Taoshi" : "Tjampen";
            var notifyId = Context.User.Id == 135446225565515776 ? 208624502878371840 : 135446225565515776;

            var embed = new EmbedBuilder()
                .WithTitle("Message sent")
                .WithDescription($"{senderName} replied to {user.Id} ({user.Username})\n\n{message}")
                .WithColor(new Color(0, 0, 255))
                .Build();

            await ReplyAsync("Message sent!");
            var notifyUser = Context.Client.GetUser((ulong)notifyId);
            if (notifyUser != null)
            {
                await notifyUser.SendMessageAsync("", false, embed);
            }
        }
        [RequireDevelopers]
        [Command("blacklist", RunMode = RunMode.Async)]
        public async Task BlacklistUserAsync(RestUser user, [Remainder] string reason = "No reason")
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }

            // Prevent blacklisting the bot owners
            var ownerIds = new[] { 135446225565515776UL, 208624502878371840UL };
            if (ownerIds.Contains(user.Id))
            {
                await Context.Channel.SendMessageAsync("Nice try");
                return;
            }

            var blacklist = _cachingService.GetBlackList();
            if (blacklist.Contains(user.Id))
            {
                var blacklistCheck = _cachingService.GetBlacklistCheck(user.Id);
                var alreadyEmbed = new EmbedBuilder()
                    .WithTitle("Blacklist")
                    .WithDescription($"{user.Mention} is already blacklisted by {blacklistCheck.Blacklister}.")
                    .WithColor(new Color(0, 0, 0))
                    .AddField("Reason", blacklistCheck.Reason)
                    .Build();
                await Context.Channel.SendMessageAsync("", false, alreadyEmbed);
                return;
            }

            _cachingService.AddToBlacklist(user, reason, Context.User);
            var embed = new EmbedBuilder()
                .WithTitle("Blacklist")
                .WithDescription($"{user.Mention} has been blacklisted!\n\nReason: {reason}")
                .WithColor(new Color(0, 0, 0))
                .Build();
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        [RequireDevelopers]
        [Command("blacklistid", RunMode = RunMode.Async)]
        public async Task BlacklistUserByIdAsync(ulong id, [Remainder] string reason = "No reason")
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }

            var user = await Context.Client.Rest.GetUserAsync(id);
            var ownerIds = new[] { 135446225565515776UL, 208624502878371840UL };

            if (ownerIds.Contains(user.Id))
            {
                await Context.Channel.SendMessageAsync("Nice try");
                return;
            }

            var blacklist = _cachingService.GetBlackList();
            if (blacklist.Contains(user.Id))
            {
                var blacklistCheck = _cachingService.GetBlacklistCheck(user.Id);
                var alreadyEmbed = new EmbedBuilder()
                    .WithTitle("Blacklist")
                    .WithDescription($"{user.Mention} is already blacklisted by {blacklistCheck.Blacklister}.")
                    .WithColor(new Color(0, 0, 0))
                    .AddField("Reason", blacklistCheck.Reason)
                    .Build();
                await Context.Channel.SendMessageAsync("", false, alreadyEmbed);
                return;
            }

            _cachingService.AddToBlacklist(user, reason, Context.User);
            var embed = new EmbedBuilder()
                .WithTitle("Blacklist")
                .WithDescription($"{user.Mention} has been blacklisted!\n\nReason: {reason}")
                .WithColor(new Color(0, 0, 0))
                .Build();

            await Context.Channel.SendMessageAsync("", false, embed);
        }
        [RequireDevelopers]
        [Command("unblacklist", RunMode = RunMode.Async)]
        public async Task UnblacklistUserAsync(RestUser user)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }

            var blacklist = _cachingService.GetBlackList();
            var embedBuilder = new EmbedBuilder { Color = new Color(0, 0, 0), Title = "Blacklist" };

            if (blacklist.Contains(user.Id))
            {
                embedBuilder.Description = $"{user.Mention} has been removed from the blacklist!";
                _cachingService.RemoveFromBlacklist(user.Id);
            }
            else
            {
                embedBuilder.Description = $"{user.Mention} is not on the blacklist";
            }

            var embed = embedBuilder.Build();
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        [RequireDevelopers]
        [Command("unblacklistid", RunMode = RunMode.Async)]
        public async Task UnblacklistUserByIdAsync(ulong id)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }

            var user = await Context.Client.Rest.GetUserAsync(id);
            var blacklist = _cachingService.GetBlackList();
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithTitle("Blacklist");

            if (blacklist.Contains(user.Id))
            {
                StatsDB.BlacklistDel(id);
                embedBuilder.Description = $"{user?.Mention ?? id.ToString()} has been removed from the blacklist!";
                _cachingService.RemoveFromBlacklist(id);
            }
            else
            {
                embedBuilder.Description = $"{user?.Mention ?? id.ToString()} is not on the blacklist";
            }

            await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }
        [RequireDevelopers]
        [Command("Blacklistcheck", RunMode = RunMode.Async)]
        public async Task BlacklistCheckAsync(RestUser user)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }

            var blacklistCheck = _cachingService.GetBlacklistCheck(user.Id);
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithTitle("Blacklist Check");

            if (blacklistCheck == null)
            {
                embedBuilder.Description = $"{user.Mention} is not on the blacklist!";
            }
            else
            {
                embedBuilder.Description = $"{user.Mention} is blacklisted!" +
                    $"\n\n*Reason:* {blacklistCheck.Reason}" +
                    $"\n\nBlacklisted by {blacklistCheck.Blacklister}";
            }

            await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }
        [RequireDevelopers]
        [Command("Blacklistcheckid", RunMode = RunMode.Async)]
        public async Task BlacklistCheckByIdAsync(ulong id)
        {
            if (!_cachingService._dbConnected)
            {
                await ReplyAsync("Database is down, please try again later");
                return;
            }

            var user = await Context.Client.Rest.GetUserAsync(id);
            var blacklistCheck = _cachingService.GetBlacklistCheck(id);
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithTitle("Blacklist Check");

            if (blacklistCheck == null)
            {
                embedBuilder.Description = $"{user?.Username ?? id.ToString()} is not on the blacklist!";
            }
            else
            {
                embedBuilder.Description = $"{user?.Username ?? id.ToString()} is blacklisted!" +
                    $"\n\n*Reason:* {blacklistCheck.Reason}" +
                    $"\n\nBlacklisted by {blacklistCheck.Blacklister}";
            }

            await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
        }
        [RequireDevelopers]
        [Command("topservers", RunMode = RunMode.Async)]
        public async Task TopServersAsync(int amount = 20)
        {
            var servers = Context.Client.Guilds.OrderByDescending(x => x.MemberCount).ToList();
            List<string> serverList = new List<string>();
            int position = 1;
            foreach (var server in servers.Take(amount))
            {
                string positionString = $"{position}\\.";
                var percentage = Math.Round(server.Users.Count(x => x.IsBot) / (double)server.MemberCount * 100D, 2);
                serverList.Add($"{positionString,-4} {server.Name} - {server.MemberCount} members ({percentage}% bots)");
                position++;
            }
            var topServersList = HelperFunctions.JoinWithLimit(serverList, 2048, "\n");
            var embed = new EmbedBuilder()
                .WithTitle($"Top **{amount}** Servers")
                .WithDescription(topServersList)
                .WithColor(new Color(0, 0, 255))
                .Build();
            await ReplyAsync("", false, embed);
        }
        [RequireDevelopers]
        [Command("guildinfo", RunMode = RunMode.Async)]
        public async Task GuildInfoAsync(ulong guildId)
        {
            var guild = Context.Client.GetGuild(guildId);
            if (guild == null)
            {
                await ReplyAsync("Guild not found");
                return;
            }
            try
            {
                await guild.DownloadUsersAsync();

                var textChannels = guild.TextChannels
                    .Select(x => (x.Id, x.Name))
                    .Where(x => !guild.VoiceChannels.Select(y => y.Id).Contains(x.Id));

                var voiceChannels = guild.VoiceChannels
                    .Where(x => x is not SocketStageChannel)
                    .Select(x => (x.Id, x.Name));

                var stageChannels = guild.VoiceChannels
                    .Where(x => x is SocketStageChannel)
                    .Select(x => (x.Id, x.Name));

                var forumChannels = guild.Channels
                    .Where(x => x is SocketForumChannel)
                    .Select(x => (x.Id, x.Name));

                var mediaChannels = guild.Channels
                    .Where(x => x is SocketMediaChannel)
                    .Select(x => (x.Id, x.Name));

                // Separate forum threads from text channels
                var forumThreads = guild.ThreadChannels
                    .Where(x => x.ParentChannel is SocketForumChannel)
                    .Select(x => (x.Id, x.Name));

                var textThreads = guild.ThreadChannels
                    .Where(x => x.ParentChannel is SocketTextChannel)
                    .Select(x => (x.Id, x.Name));

                // Remove forum threads from text channels field
                var textChannelsAndThreadsEnumerable = textChannels
                    .Concat(textThreads)
                    .Where(x => !guild.ThreadChannels.Any(t => t.Id == x.Id && t.ParentChannel is SocketForumChannel))
                    .GroupBy(x => x.Id)
                    .Select(g => g.First().Name);

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
                    ? HelperFunctions.JoinWithLimit(voiceChannels.Select(x => x.Name), 1024, "\n")
                    : "None";
                string stageChannelsList = stageChannels.Any()
                    ? HelperFunctions.JoinWithLimit(stageChannels.Select(x => x.Name), 1024, "\n")
                    : "None";
                string forumChannelsList = forumChannels.Any()
                    ? HelperFunctions.JoinWithLimit(forumChannels.Select(x => x.Name), 1024, "\n")
                    : "None";
                string mediaChannelsList = mediaChannels.Any()
                    ? HelperFunctions.JoinWithLimit(mediaChannels.Select(x => x.Name), 1024, "\n")
                    : "None";
                string forumThreadsList = forumThreads.Any()
                    ? HelperFunctions.JoinWithLimit(forumThreads.Select(x => x.Name), 1024, "\n")
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
                var rolesList = guild.Roles.OrderByDescending(x => x.Position).Select(x => x.Name).ToList();
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
                var taoshi = Context.Client.GetUser(135446225565515776);
                var tjampen = Context.Client.GetUser(208624502878371840);
                var paginator = new StaticPaginatorBuilder()
                    .WithUsers(taoshi, tjampen)
                    .WithPages(pages)
                    .WithFooter(PaginatorFooter.PageNumber)
                    .Build();

                await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await ReplyAsync($"{ex.GetType()}: {ex.Message}");
            }
        }
        [RequireDevelopers]
        [Command("cachestatus", RunMode = RunMode.Async)]
        public async Task CacheStatusAsync()
        {
            // Page 1: Summary (current version)
            int guildsWithUsers = _cachingService._usersInDatabase.Count;
            int totalUsersInDatabase = _cachingService._usersInDatabase.Values.Sum(list => list.Count);
            int guildsWithBadWords = _cachingService._badWords.Count;
            int totalBadWords = _cachingService._badWords.Values.Sum(list => list.Count);

            int cachedBlacklistChecksCount = _cachingService._cachedBlacklistChecks.Count;
            int cachedBlacklistChecksNullCount = _cachingService._cachedBlacklistChecks.Values.Count(x => x == null);

            long memoryBytes = Process.GetCurrentProcess().PrivateMemorySize64;
            double memoryMB = memoryBytes / (1024.0 * 1024.0);

            var pages = new List<IPageBuilder>();

            // Page 1: Summary
            var summaryEmbed = new PageBuilder()
                .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                .WithColor(new Color(0, 255, 0))
                .AddField("Database Connected", _cachingService._dbConnected ? "Yes" : "No", true)
                .AddField("Cached Server Settings", _cachingService._cachedServerSettings.Count, true)
                .AddField("Cached Blacklist Checks", $"{cachedBlacklistChecksCount} (Not blacklisted: {cachedBlacklistChecksNullCount})", true)
                .AddField("Blacklisted Users", _cachingService._blacklist.Count, true)
                .AddField("Users in Database (Guilds/Users)", $"{guildsWithUsers} / {totalUsersInDatabase}", true)
                .AddField("Bad Words (Guilds/Words)", $"{guildsWithBadWords} / {totalBadWords}", true)
                .WithCurrentTimestamp()
                .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB");

            pages.Add(summaryEmbed);

            // Page 2+: Users in Database (Each guild is a field, split every 25 fields)
            var guildFields = new List<(string Name, string Value)>();
            foreach (var kvp in _cachingService._usersInDatabase)
            {
                var guild = Context.Client.GetGuild(kvp.Key);
                string guildName = guild?.Name ?? $"Unknown ({kvp.Key})";
                var userNames = kvp.Value
                    .Select(userId =>
                    {
                        var user = guild?.GetUser(userId) ?? Context.Client.GetUser(userId);
                        return user != null ? $"{user.Username} ({userId})" : $"Unknown ({userId})";
                    })
                    .ToList();
                string value = userNames.Count > 0
                    ? HelperFunctions.JoinWithLimit(userNames, 1024, "\n")
                    : "No users";
                guildFields.Add((guildName, value));
            }
            if (guildFields.Count == 0)
            {
                pages.Add(new PageBuilder()
                    .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                    .WithTitle("Users in Database")
                    .WithDescription("No cached users in StatsDB.")
                    .WithColor(new Color(0, 255, 0))
                    .WithCurrentTimestamp()
                    .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB"));
            }
            else
            {
                for (int i = 0; i < guildFields.Count; i += 25)
                {
                    var pageFields = guildFields.Skip(i).Take(25).ToList();
                    var page = new PageBuilder()
                        .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                        .WithTitle("Cached users in Database")
                        .WithColor(new Color(0, 255, 0))
                        .WithCurrentTimestamp()
                        .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB");
                    foreach (var field in pageFields)
                    {
                        if (!string.IsNullOrWhiteSpace(field.Value))
                        {
                            page.AddField(field.Name, field.Value, false);
                        }
                    }
                    pages.Add(page);
                }
            }

            // Page 3: Cached Server Settings (GuildId -> Settings, grouped by property)
            var serverSettings = _cachingService._cachedServerSettings.Values.ToList();
            if (serverSettings.Count == 0)
            {
                pages.Add(new PageBuilder()
                    .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                    .WithTitle("Cached Server Settings")
                    .WithDescription("No cached server settings.")
                    .WithColor(new Color(0, 255, 0))
                    .WithCurrentTimestamp()
                    .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB"));
            }
            else
            {
                // Split server settings into pages of 25 fields each
                var settingsFields = new List<(string Name, string Value)>();
                foreach (var s in serverSettings)
                {
                    var guild = Context.Client.GetGuild(s.GuildId);
                    string guildName = guild?.Name ?? $"Unknown ({s.GuildId})";
                    var value = $"Prefix: {s.Prefix}\n" +
                        $"Welcome Channel: {(s.WelcomeChannel != 0 ? $"<#{s.WelcomeChannel}>" : "Not Set")}\n" +
                        $"Modlog Channel: {(s.ModlogChannel != 0 ? $"<#{s.ModlogChannel}>" : "Not Set")}\n" +
                        $"XP Multiplier: {s.XpMultiplier}x\n" +
                        $"Level Up Messages: {s.LevelupMessages}";
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        settingsFields.Add((guildName, value));
                    }
                }
                for (int i = 0; i < settingsFields.Count; i += 25)
                {
                    var pageFields = settingsFields.Skip(i).Take(25).ToList();
                    var settingsEmbed = new PageBuilder()
                        .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                        .WithTitle("Cached Server Settings")
                        .WithColor(new Color(0, 255, 0))
                        .WithCurrentTimestamp()
                        .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB");
                    foreach (var field in pageFields)
                    {
                        if (!string.IsNullOrWhiteSpace(field.Value))
                        {
                            settingsEmbed.AddField(field.Name, field.Value, false);
                        }
                    }
                    pages.Add(settingsEmbed);
                }
            }

            // Page 4: Cached Blacklist Checks (UserId -> Username - true/false)
            var blacklistChecksDetails = new StringBuilder();
            foreach (var kvp in _cachingService._cachedBlacklistChecks)
            {
                var user = Context.Client.GetUser(kvp.Key);
                string username = user?.Username ?? "Unknown";
                bool isBlacklisted = kvp.Value != null;
                blacklistChecksDetails.AppendLine($"{username} ({kvp.Key}) - {isBlacklisted.ToString().ToLower()}");
            }
            if (blacklistChecksDetails.Length == 0)
                blacklistChecksDetails.Append("No cached blacklist checks.");

            pages.Add(new PageBuilder()
                .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                .WithTitle("Cached Blacklist Checks")
                .WithDescription(blacklistChecksDetails.ToString().Length > 4096 ? blacklistChecksDetails.ToString().Substring(0, 4090) + "..." : blacklistChecksDetails.ToString())
                .WithColor(new Color(0, 255, 0))
                .WithCurrentTimestamp()
                .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB"));

            // Page 5: Blacklisted Users (UserId -> Username)
            var blacklistDetails = new StringBuilder();
            foreach (var userId in _cachingService._blacklist)
            {
                var user = Context.Client.GetUser(userId);
                string username = user?.Username ?? "Unknown";
                blacklistDetails.AppendLine($"{username} ({userId})");
            }
            if (blacklistDetails.Length == 0)
                blacklistDetails.Append("No blacklisted users.");

            pages.Add(new PageBuilder()
                .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                .WithTitle("Blacklisted Users")
                .WithDescription(blacklistDetails.ToString().Length > 4096 ? blacklistDetails.ToString().Substring(0, 4090) + "..." : blacklistDetails.ToString())
                .WithColor(new Color(0, 255, 0))
                .WithCurrentTimestamp()
                .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB"));

            // Page 6: Bad Words (GuildName - bad word)
            var badWordsFields = new List<(string Name, string Value)>();
            foreach (var kvp in _cachingService._badWords)
            {
                var guild = Context.Client.GetGuild(kvp.Key);
                string guildName = guild?.Name ?? "Unknown";
                string value = HelperFunctions.JoinWithLimit(kvp.Value, 1024, "\n");
                if (!string.IsNullOrWhiteSpace(value))
                {
                    badWordsFields.Add((guildName, value));
                }
            }
            if (badWordsFields.Count == 0)
            {
                var badWordsPage = new PageBuilder()
                    .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                    .WithTitle("Bad Words")
                    .WithDescription("No bad words cached.")
                    .WithColor(new Color(0, 255, 0))
                    .WithCurrentTimestamp()
                    .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB");
                pages.Add(badWordsPage);
            }
            else
            {
                for (int i = 0; i < badWordsFields.Count; i += 25)
                {
                    var pageFields = badWordsFields.Skip(i).Take(25).ToList();
                    var badWordsPage = new PageBuilder()
                        .WithAuthor($"Cache Status - Total Bot Memory Usage: {memoryMB:F2} MB")
                        .WithTitle("Bad Words")
                        .WithColor(new Color(0, 255, 0))
                        .WithCurrentTimestamp()
                        .WithFooter($"Total Bot Memory Usage: {memoryMB:F2} MB");
                    foreach (var field in pageFields)
                    {
                        if (!string.IsNullOrWhiteSpace(field.Value))
                        {
                            badWordsPage.AddField(field.Name, field.Value, false);
                        }
                    }
                    pages.Add(badWordsPage);
                }
            }

            // Send paginator
            var taoshi = Context.Client.GetUser(135446225565515776);
            var tjampen = Context.Client.GetUser(208624502878371840);
            var paginator = new StaticPaginatorBuilder()
                .WithUsers(taoshi, tjampen)
                .WithPages(pages)
                .WithFooter(PaginatorFooter.PageNumber)
                .Build();

            await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
        }
    }
}
