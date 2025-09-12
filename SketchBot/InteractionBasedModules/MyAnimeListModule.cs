using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using JikanDotNet;
using SketchBot.Custom_Preconditions;
using SketchBot.Services;
using SketchBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.InteractionBasedModules
{
    public class MyAnimeListModule : InteractionModuleBase<SocketInteractionContext>
    {

        private readonly Jikan _jikan;
        private readonly InteractiveService _interactive;

        public MyAnimeListModule(Jikan jikan, InteractiveService interactive)
        {
            _jikan = jikan;
            _interactive = interactive;
        }

        [Ratelimit(1, 5, Measure.Seconds)]
        [SlashCommand("anime", "Shows information about an Anime")]
        public async Task AnimeAsync(string name)
        {
            await DeferAsync();
            try
            {
                var anime = await _jikan.SearchAnimeAsync(name);
                if (anime == null)
                {
                    await FollowupAsync("The API didn't return anything.");
                    return;
                }
                var results = anime.Data;
                if (results.Count == 0)
                {
                    await FollowupAsync("The API didn't return anything.");
                    return;
                }
                var pageBuilders = new List<IPageBuilder>();
                int NSFW = 0;
                foreach (var result in results)
                {
                    if (result.Rating == "Rx" && !(Context.Channel as ITextChannel).IsNsfw || result == null)
                    {
                        NSFW++;
                        continue;
                    }
                    try
                    {
                        var builder = new PageBuilder()
                            .WithDescription(result.Synopsis ?? "null")
                            .AddField("Score", $"{result.Score}" ?? "N/A", true)
                            .AddField("Airing", $"{result.Airing}" ?? "N/A", true)
                            .AddField("Type", $"{result.Type}" ?? "N/A", true)
                            .AddField("Episodes", $"{result.Episodes}" ?? "N/A", true)
                            .AddField("Start Date", result.Aired?.From != null ? $"{result.Aired.From.Value.Day}/{result.Aired.From.Value.Month}/{result.Aired.From.Value.Year}" : "N/A", true)
                            .AddField("End Date", result.Aired?.To != null ? $"{result.Aired.To.Value.Day}/{result.Aired.To.Value.Month}/{result.Aired.To.Value.Year}" : "N/A", true)
                            .AddField("Rated", $"{result.Rating}", true)
                            .WithAuthor(new EmbedAuthorBuilder
                            {
                                Name = result.Titles.FirstOrDefault()?.Title ?? "null",
                                Url = result.Url,
                                IconUrl = result.Images?.JPG?.ImageUrl
                            })
                            .WithImageUrl(result.Images?.JPG?.ImageUrl ?? "")
                            .WithColor(new Color(0, 0, 255));
                        pageBuilders.Add(builder);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                var paginator = new StaticPaginatorBuilder()
                    .AddUser(Context.User)
                    .WithPages(pageBuilders)
                    .WithFooter(PaginatorFooter.PageNumber)
                    .Build();
                await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(5), InteractionResponseType.DeferredChannelMessageWithSource);


            }
            catch (Exception ex)
            {
                await FollowupAsync(ex.ToString());
            }
        }
        [Ratelimit(1, 5, Measure.Seconds)]
        [SlashCommand("manga", "Shows information about a Manga")]
        public async Task MangaAsync(string name)
        {
            await DeferAsync();
            try
            {
                var manga = await _jikan.SearchMangaAsync(name);
                if (manga == null)
                {
                    await FollowupAsync("The API didn't return anything.");
                    return;
                }
                var results = manga.Data;
                if (results.Count == 0)
                {
                    await FollowupAsync("The API didn't return anything.");
                    return;
                }
                var pageBuilders = new List<IPageBuilder>();
                foreach (var result in results)
                {
                    try
                    {
                        var builder = new PageBuilder()
                            .WithDescription(result.Synopsis ?? "null")
                            .AddField("Score", $"{result.Score}" ?? "N/A", true)
                            .AddField("Publishing", $"{result.Publishing}" ?? "N/A", true)
                            .AddField("Type", "{$result.Type}" ?? "N/A", true)
                            .AddField("Chapters", $"{result.Chapters}" ?? "N/A", true)
                            .AddField("Volumes", $"{result.Volumes}" ?? "N/A", true)
                            .AddField("Start Date", result.Published?.From != null ? $"{result.Published.From.Value.Day}/{result.Published.From.Value.Month}/{result.Published.From.Value.Year}" : "N/A", true)
                            .AddField("End Date", result.Published?.To != null ? $"{result.Published.To.Value.Day}/{result.Published.To.Value.Month}/{result.Published.To.Value.Year}" : "N/A", true)
                            .WithAuthor(new EmbedAuthorBuilder
                            {
                                Name = result.Titles.FirstOrDefault()?.Title ?? "null",
                                Url = result.Url,
                                IconUrl = result.Images?.JPG?.ImageUrl
                            })
                            .WithImageUrl(result.Images?.JPG?.ImageUrl ?? "")
                            .WithColor(new Color(0, 0, 255));
                        pageBuilders.Add(builder);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                var paginator = new StaticPaginatorBuilder()
                    .AddUser(Context.User)
                    .WithPages(pageBuilders)
                    .WithFooter(PaginatorFooter.PageNumber)
                    .Build();
                await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(5), InteractionResponseType.DeferredChannelMessageWithSource);
            }
            catch (Exception ex)
            {
                await FollowupAsync(ex.ToString());
            }
        }
        [Ratelimit(1, 5, Measure.Seconds)]
        [SlashCommand("maluser", "Searches for a user on MyAnimeList.net")]
        public async Task MALUserAsync(string username)
        {
            await DeferAsync();
            var user = await _jikan.GetUserProfileAsync(username);
            if (user == null)
            {
                await FollowupAsync("The API didn't return anything");
                return;
            }

            var about = await _jikan.GetUserAboutAsync(username);
            var friends = await _jikan.GetUserFriendsAsync(username);
            var history = await _jikan.GetUserHistoryAsync(username);
            var favorites = await _jikan.GetUserFavoritesAsync(username);
            var stats = await _jikan.GetUserStatisticsAsync(username);
            string imageUrl = favorites?.Data?.Anime.Count > 0 ? favorites?.Data?.Anime.FirstOrDefault()?.Images.JPG.ImageUrl ?? "" : "";
            var pages = new List<IPageBuilder>
                {
                    new PageBuilder()
                        .WithTitle("Profile")
                        .WithDescription(HelperFunctions.SafeField(about?.Data?.About?.Replace("<br>","")))
                        .AddField("User Id", HelperFunctions.SafeField(user.Data?.MalId), true)
                        .AddField("Gender", HelperFunctions.SafeField(user.Data?.Gender), true)
                        .AddField("Location", HelperFunctions.SafeField(user.Data?.Location), true)
                        .AddField("Last Online", HelperFunctions.SafeField(user.Data?.LastOnline), true)
                        .AddField("Join Date", HelperFunctions.SafeField(user.Data?.Joined), true)
                        .AddField("Birthday", HelperFunctions.SafeField(user.Data?.Birthday), true)
                        .WithAuthor(new EmbedAuthorBuilder
                        {
                            Name = HelperFunctions.SafeField(user.Data?.Username),
                            IconUrl = user.Data?.Images.JPG.ImageUrl,
                            Url = HelperFunctions.SafeField(user.Data?.Url)
                        })
                        .WithThumbnailUrl(user.Data?.Images.JPG.ImageUrl)
                        .WithColor(new Color(0, 0, 255)),
                    new PageBuilder()
                        .WithTitle("Anime Statistics")
                        .AddField("Total Anime Entries", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.TotalEntries))
                        .AddField("Total Episodes Watched", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.EpisodesWatched), true)
                        .AddField("Mean Score", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.MeanScore), true)
                        .AddField("Completed", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.Completed))
                        .AddField("Watching", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.Watching))
                        .AddField("Plan to Watch", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.PlanToWatch), true)
                        .AddField("On Hold", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.OnHold), true)
                        .AddField("Dropped",HelperFunctions. SafeField(stats?.Data?.AnimeStatistics?.Dropped), true)
                        .AddField("Rewatched", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.Rewatched), true)
                        .AddField("Days Watched", HelperFunctions.SafeField(stats?.Data?.AnimeStatistics?.DaysWatched)),
                    new PageBuilder()
                        .WithTitle("Manga Statistics")
                        .AddField("Total Manga Entries", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.TotalEntries))
                        .AddField("Total Chapters Read", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.ChaptersRead), true)
                        .AddField("Mean Score", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.MeanScore), true)
                        .AddField("Completed", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.Completed))
                        .AddField("Reading", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.Reading))
                        .AddField("Plan to Read", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.PlanToRead), true)
                        .AddField("On Hold", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.OnHold), true)
                        .AddField("Dropped", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.Dropped), true)
                        .AddField("Reread", HelperFunctions.SafeField(stats?.Data?.MangaStatistics?.Reread), true),
                    // Only add WithImageUrl if imageUrl is not null or empty
                    string.IsNullOrEmpty(imageUrl)
                        ? new PageBuilder()
                            .WithTitle("Favorites")
                            .AddField($"Favorite Anime ({favorites?.Data?.Anime.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Anime.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite anime"], 1024, "\n")))
                            .AddField($"Favorite Manga ({favorites?.Data?.Manga.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Manga.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite manga"], 1024, "\n")))
                            .AddField($"Favorite Character(s) ({favorites?.Data?.Characters.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Characters.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite characters"], 1024, "\n")), true)
                            .AddField($"Favorite People ({favorites?.Data?.People.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.People.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite people"], 1024, "\n")), true)
                        : new PageBuilder()
                            .WithTitle("Favorites")
                            .WithImageUrl(imageUrl)
                            .AddField($"Favorite Anime ({favorites?.Data?.Anime.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Anime.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite anime"], 1024, "\n")))
                            .AddField($"Favorite Manga ({favorites?.Data?.Manga.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Manga.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite manga"], 1024, "\n")))
                            .AddField($"Favorite Character(s) ({favorites?.Data?.Characters.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Characters.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite characters"], 1024, "\n")), true)
                            .AddField($"Favorite People ({favorites?.Data?.People.Count ?? 0})", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.People.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite people"], 1024, "\n")), true)
                    ,
                    new PageBuilder()
                        .WithTitle("User History")
                        .AddField("Name", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(history?.Data?.Select(x => $"[{x.Metadata.Name}]({x.Metadata.Url})") ?? ["N/A"], 1024, "\n")), true)
                        .AddField("Increment", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(history?.Data?.Select(x => x.Increment.ToString()) ?? ["N/A"], 1024, "\n")), true)
                        .AddField("Date", HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(history?.Data?.Select(x => x.Date?.ToString() ?? "N/A") ?? ["N/A"], 1024, "\n")), true),
                    new PageBuilder()
                        .WithTitle($"Friends ({friends?.Data?.Count ?? 0})")
                        .WithDescription(HelperFunctions.SafeField(HelperFunctions.JoinWithLimit(friends?.Data?.Select(x => $"[{x.User.Username}]({x.User.Url})") ?? ["N/A"], 1024, "\n")))
                };
            var paginator = new StaticPaginatorBuilder()
                .AddUser(Context.User)
                .WithPages(pages)
                .WithFooter(PaginatorFooter.PageNumber)
                .Build();
            await _interactive.SendPaginatorAsync(paginator, Context.Interaction, TimeSpan.FromMinutes(5), InteractionResponseType.DeferredChannelMessageWithSource);

        }
    }
}
