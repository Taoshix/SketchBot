using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Sketch_Bot.Custom_Preconditions;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
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
        private LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lavaNode;
        private IServiceProvider _serviceProvider;
        public DevModule(DiscordBotsListService service, TimerService service2, StatService service3, CachingService service4, LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode, IServiceProvider serviceProvider)
        {
            _service = service;
            _timerService = service2;
            _statService = service3;
            _cachingService = service4;
            _lavaNode = lavaNode;
            _serviceProvider = serviceProvider;
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
            var result = HelperFunctions.Bash(cmd);
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
                await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Discord.Image(stream));
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
                Database.BlacklistDel(id);
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
                await ReplyAsync("Guild not found.");
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
                    .Where(x => !(guild.ThreadChannels.Any(t => t.Id == x.Id && t.ParentChannel is SocketForumChannel)))
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

                var embed = new EmbedBuilder()
                    .AddField("Owner", guild.Owner?.Mention ?? "Unknown", true)
                    .AddField($"Member Count ({realCount})", memberCountField, true)
                    .AddField($"Total Channels ({guild.Channels.Count})",
                        $"{textChannels.Count()} Text\n" +
                        $"{voiceChannels.Count()} Voice\n" +
                        $"{stageChannels.Count()} Stage\n" +
                        $"{forumChannels.Count()} Forum\n" +
                        $"{forumThreads.Count()} Forum threads\n" +
                        $"{guild.CategoryChannels.Count} Category channels\n" +
                        $"{mediaChannels.Count()} Media", true)
                    .AddField($"Categories ({guild.CategoryChannels.Count})",
                        guild.CategoryChannels.Count == 0
                            ? "None"
                            : HelperFunctions.JoinWithLimit(guild.CategoryChannels.Select(x => x.Name), 1024, "\n"), true)
                    .AddField($"Emojis ({guild.Emotes.Count})",
                        guild.Emotes.Count == 0
                            ? "None"
                            : HelperFunctions.JoinWithLimit(guild.Emotes.Select(x => x.ToString()), 1024, ""), true)
                    .AddField($"Text Channels ({textChannelsAndThreadsEnumerable.Count()})",
                        guild.TextChannels.Count == 0
                            ? "None"
                            : textChannelsAndThreads)
                    .AddField($"Voice Channels ({voiceChannels.Count()})", voiceChannelsList, true)
                    .AddField($"Stage Channels ({stageChannels.Count()})", stageChannelsList, true)
                    .AddField($"Forum Channels ({forumChannels.Count()})", forumChannelsList, true)
                    .AddField($"Forum Threads ({forumThreads.Count()})", forumThreadsList, true)
                    .AddField($"Media Channels ({mediaChannels.Count()})", mediaChannelsList, true)
                    .AddField($"Roles ({guild.Roles.Count})", guild.Roles.Count == 0 ? "None" : HelperFunctions.JoinWithLimit(guild.Roles.OrderByDescending(x => x.Position).Select(x => x.Name), 1024, guild.Roles.Count > 10 ? " " : "\n"))
                    .AddField("Verification Level", guild.VerificationLevel.ToString(), true)
                    .AddField("Boost Level", $"{guild.PremiumTier} ({guild.PremiumSubscriptionCount} boosts)", true)
                    .AddField("Region", string.IsNullOrWhiteSpace(guild.VoiceRegionId) ? "None" : guild.VoiceRegionId, true)
                    .AddField("Vanity", guild.Features.HasVanityUrl ? guild.VanityURLCode : "No Vanity", true)
                    .AddField("Icon URL", string.IsNullOrWhiteSpace(guild.IconUrl) ? "No Icon" : guild.IconUrl, true)
                    .AddField("Banner URL", string.IsNullOrWhiteSpace(guild.BannerUrl) ? "No Banner" : guild.BannerUrl, true)
                    .WithThumbnailUrl(guild.IconUrl ?? "")
                    .WithColor(new Color(0, 255, 0))
                    .WithFooter(footer =>
                    {
                        footer.Text = $"ID: {guild.Id} | Server Created";
                        footer.IconUrl = guild.IconUrl;
                    })
                    .WithAuthor(author =>
                    {
                        author.Name = guild.Name;
                        author.IconUrl = guild.IconUrl;
                    });

                embed.Timestamp = guild.CreatedAt;
                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await ReplyAsync($"{ex.GetType()}: {ex.Message}");
            }
        }
        [RequireDevelopers]
        [Command("cachestatus", RunMode = RunMode.Async)]
        public async Task CacheStatusAsync()
        {
            int guildsWithUsers = _cachingService._usersInDatabase.Count;
            int totalUsersInDatabase = _cachingService._usersInDatabase.Values.Sum(list => list.Count);
            int guildsWithBadWords = _cachingService._badWords.Count;
            int totalBadWords = _cachingService._badWords.Values.Sum(list => list.Count);

            int cachedBlacklistChecksCount = _cachingService._cachedBlacklistChecks.Count;
            int cachedBlacklistChecksNullCount = _cachingService._cachedBlacklistChecks.Values.Count(x => x == null);

            var embed = new EmbedBuilder()
                .WithTitle("Cache Status")
                .WithColor(new Color(0, 255, 0))
                .AddField("Database Connected", _cachingService._dbConnected ? "Yes" : "No", true)
                .AddField("Cached Server Settings", _cachingService._cachedServerSettings.Count, true)
                .AddField("Cached Blacklist Checks", $"{cachedBlacklistChecksCount} (Not blacklisted: {cachedBlacklistChecksNullCount})", true)
                .AddField("Blacklisted Users", _cachingService._blacklist.Count, true)
                .AddField("Users in Database (Guilds/Users)", $"{guildsWithUsers} / {totalUsersInDatabase}", true)
                .AddField("Bad Words (Guilds/Words)", $"{guildsWithBadWords} / {totalBadWords}", true)
                .WithCurrentTimestamp()
                .Build();
            await ReplyAsync("", false, embed);
        }
    }
}
