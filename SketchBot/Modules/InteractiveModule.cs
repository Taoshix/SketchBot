using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using JikanDotNet;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using Sketch_Bot.Custom_Preconditions;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Urban.NET;
using UrbanDictionnet;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace Sketch_Bot.Modules
{
    public class InteractiveModule : InteractionModuleBase<SocketInteractionContext>
    {

        private readonly Jikan _jikan;
        private readonly CachingService _cache;
        private readonly InteractiveService _interactive;
        private readonly DiscordBotsListService _discordBotsListService;

        public InteractiveModule(Jikan jikan, CachingService cache, InteractiveService interactive, DiscordBotsListService discordBotsListService)
        {
            _jikan = jikan;
            _cache = cache;
            _interactive = interactive;
            _discordBotsListService = discordBotsListService;
        }
        [SlashCommand("paginator", "Makes a paginator using your input seperated by comma")]
        public async Task PaginateAsync(string input)
        {
            await DeferAsync();
            string[] words = input.Split(",");
            if (words.Length == 0 || string.IsNullOrWhiteSpace(words[0]))
            {
                await FollowupAsync(Context.User.Mention + " You gotta give me something to paginate");
                return;
            }
            var pages = new List<IPageBuilder>();

            // Add a page for each word
            foreach (var word in words)
            {
                pages.Add(new PageBuilder()
                    .WithTitle($"{Context.User.Username}'s Paginator")
                    .WithDescription(word.Trim()));
            }
            var paginator = new StaticPaginatorBuilder()
                .AddUser(Context.User)
                .WithPages(pages)
                .WithFooter(PaginatorFooter.PageNumber)
                .Build();
            await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(5), InteractionResponseType.DeferredChannelMessageWithSource);

        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("rolemembers", "Displays the members of a role")]
        public async Task RoleMembersAsync(SocketRole role)
        {
            await DeferAsync();
            await role.Guild.DownloadUsersAsync();
            var members = role.Members.OrderBy(o => o.DisplayName).Select(x => x.Mention).ToList();
            if (members.Count <= 0)
            {
                await FollowupAsync($"{role.Name} has 0 members");
                return;
            }

            var memberStrings = members.ChunkBy(50);
            List<string> pages = new List<string>();
            List<List<string>> pages2 = new List<List<string>>();
            foreach (var list in memberStrings)
            {
                for (int i = 0; i < list.Count; i += 2)
                {
                    pages.Add(string.Join("\n", string.Join(" ", list.Skip(i).Take(2))));
                }
            }
            pages2 = pages.ChunkBy(25);
            List<string> pages3 = new List<string>();
            foreach (var list2 in pages2)
            {
                pages3.Add(string.Join("\n", list2));
            }
            var pageBuilders = new List<IPageBuilder>();
            foreach (var page in pages3)
            {
                pageBuilders.Add(new PageBuilder()
                    .WithTitle($"{role.Members.Count()} members (showing max 50 per page)")
                    .WithDescription(page)
                    .WithAuthor(new EmbedAuthorBuilder
                    {
                        IconUrl = Context.Guild.IconUrl,
                        Name = role.Name
                    })
                    .WithColor(role.Color));
            }
            var paginator = new StaticPaginatorBuilder()
                .AddUser(Context.User)
                .WithPages(pageBuilders)
                .WithFooter(PaginatorFooter.PageNumber)
                .Build();
            await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(5), InteractionResponseType.DeferredChannelMessageWithSource);

        }
        [SlashCommand("urban", "Search UrbanDictionary for the input term")]
        public async Task UrbanAsync(string word)
        {
            await DeferAsync();
            try
            {
                UrbanService client = new UrbanService();
                var data = await client.Data(word);
                var pageBuilders = new List<IPageBuilder>();
                foreach (var item in data.List)
                {
                    var builder = new PageBuilder()
                        .WithTitle(item.Word)
                        .AddField("Definition", item.Definition)
                        .AddField("Example", item.Example)
                        .AddField("Rating", $"\n\n\\👍{item.ThumbsUp} \\👎{item.ThumbsDown}");
                    pageBuilders.Add(builder);
                }
                var paginator = new StaticPaginatorBuilder()
                    .AddUser(Context.User)
                    .WithPages(pageBuilders)
                    .WithFooter(PaginatorFooter.PageNumber)
                    .Build();
                await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(5), InteractionResponseType.DeferredChannelMessageWithSource);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
            }

        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [SlashCommand("resetuser", "Resets a user's stats")]
        public async Task ResetUserStatsAsync(IGuildUser user)
        {
            await DeferAsync();
            if (!_cache._dbConnected)
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
            var nukeEmoji = new Emoji("💣");
            var builder = new ComponentBuilder()
                .WithButton("Confirm Reset", $"reset-user:{Context.User.Id}:{user.Id}", ButtonStyle.Danger, nukeEmoji);
            var promptMessage = await FollowupAsync($"Are you sure you want to reset stats for {user.Mention}?", components: builder.Build());

            // Wait for a button press from the command invoker
            var result = await _interactive.NextMessageComponentAsync(
                x => x.Message.Id == promptMessage.Id && x.User.Id == Context.User.Id && x.Data.CustomId.StartsWith($"reset-user:{Context.User.Id}:{user.Id}"),
                timeout: TimeSpan.FromSeconds(15));

            if (result.IsSuccess)
            {
                Database.DeleteUser(user);
                Database.EnterUser(user);
                await result.Value.UpdateAsync(x =>
                {
                    x.Content = $"{user.Mention}'s stats have been reset by {Context.User.Mention}.";
                    x.Components = new ComponentBuilder().Build(); // Remove buttons
                });
            }
            else
            {
                // Disable the button after timeout
                var disabledBuilder = new ComponentBuilder()
                    .WithButton("Confirm Reset", $"reset-user:{Context.User.Id}:{user.Id}", ButtonStyle.Danger, nukeEmoji, disabled: true);
                await promptMessage.ModifyAsync(msg =>
                {
                    msg.Content = $"Reset cancelled or timed out.";
                    msg.Components = disabledBuilder.Build();
                });
            }
        }
        [SlashCommand("daily", "Claim your daily or give it to another person")]
        public async Task ClaimDailyAsync(IGuildUser user = null)
        {
            await DeferAsync();

            if (!_cache._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            user ??= Context.User as IGuildUser;
            if (user.IsBot)
            {
                await FollowupAsync("Bots don't have stats");
                return;
            }
            var isUserInDatabase = _cache.IsInDatabase(Context.Guild.Id, user.Id);
            if (!isUserInDatabase)
            {
                _cache.SetupUserInDatabase(Context.Guild.Id, user as SocketGuildUser);
            }
            var userStats = Database.GetUserStats(user);
            DateTime now = DateTime.Now;
            DateTime daily = userStats.Daily;
            int difference = DateTime.Compare(daily, now);

            bool canClaim = (userStats?.Daily.ToString() == "0001-01-01 00:00:00") ||
                            (daily.DayOfYear < now.DayOfYear && difference < 0) ||
                            (difference >= 0) ||
                            (daily.Year < now.Year);

            if (!canClaim)
            {
                TimeSpan diff = now - daily;
                TimeSpan di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);
                await FollowupAsync($"Your tokens refresh in {di} !");
                return;
            }
            int amount = 50;
            var dblApi = _discordBotsListService.DblApi(Context.Client.CurrentUser.Id);
            bool hasVoted = await dblApi.HasVoted(Context.User.Id);

            if (hasVoted)
            {
                amount *= 4;
                Database.UpdateDailyTimestamp(user);
                if (user.Id != Context.User.Id)
                {
                    var _rand = new Random();
                    int giveBonus = _rand.Next(amount * 2);
                    amount += giveBonus;
                    await FollowupAsync($"You have given {user.Nickname ?? user.Username} {amount} daily tokens! (4x vote bonus) (+{giveBonus} generosity bonus)");
                }
                else
                {
                    await FollowupAsync($"You received your {amount} tokens! (4x vote bonus)");
                }
                Database.AddTokens(user, amount);
            }
            else
            {
                var builder = new ComponentBuilder()
                    .WithButton("Claim Daily Tokens", $"daily-confirm:{Context.User.Id}:{user.Id}", ButtonStyle.Primary, emote: new Emoji("💰"));
                var promptMessage = await FollowupAsync(
                    $"You would have gotten 4x more tokens if you have voted today. See /upvote\nDo you want to claim your daily anyway?",
                    components: builder.Build()
                );

                // Wait for a button press from the command invoker
                var result = await _interactive.NextMessageComponentAsync(
                    x => x.Message.Id == promptMessage.Id && x.User.Id == Context.User.Id && x.Data.CustomId.StartsWith($"daily-confirm:{Context.User.Id}:{user.Id}"),
                    timeout: TimeSpan.FromSeconds(15));

                if (result.IsSuccess)
                {
                    Database.UpdateDailyTimestamp(user);
                    Database.AddTokens(user, amount);
                    await result.Value.UpdateAsync(x =>
                    {
                        x.Content = user.Id != Context.User.Id
                            ? $"You have given {user.Nickname ?? user.Username} {amount} daily tokens!"
                            : $"You received your {amount} tokens!";
                        x.Components = new ComponentBuilder().Build(); // Remove buttons
                    });
                }
                else
                {
                    // Disable the button after timeout
                    var disabledBuilder = new ComponentBuilder().WithButton("Claim Daily Tokens", $"daily-confirm:{Context.User.Id}:{user.Id}", ButtonStyle.Primary, emote: new Emoji("💰"), disabled: true);
                    await promptMessage.ModifyAsync(msg =>
                    {
                        msg.Content = "Prompt Expired.";
                        msg.Components = disabledBuilder.Build();
                    });
                }
                return;
            }
        }
    }
}
