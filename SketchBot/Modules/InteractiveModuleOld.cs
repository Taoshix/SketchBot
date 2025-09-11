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
using Fergun.Interactive;
using Sketch_Bot.Models;
using Urban.NET;
using JikanDotNet;
using Sketch_Bot.Custom_Preconditions;
using System.Reflection;
using Sketch_Bot.Services;
using Fergun.Interactive.Pagination;

namespace Sketch_Bot.Modules
{
    public class InteractiveModuleOld : ModuleBase<SocketCommandContext>
    {

        private readonly Jikan _jikan;
        private readonly CachingService _cache;
        private readonly InteractiveService _interactive;

        public InteractiveModuleOld(Jikan jikan, CachingService cache, InteractiveService interactive)
        {
            _jikan = jikan;
            _cache = cache;
            _interactive = interactive;
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task Test_Paginator()
        {
            string prefix = _cache.GetServerSettings(Context.Guild.Id)?.Prefix ?? "?";

            IPageBuilder[] pages = [
                new PageBuilder()
                    .WithTitle("Silly Commands")
                    .WithDescription($"{prefix}help - Prints this message" +
                    $"\n{prefix}hello - Says Hi" +
                    $"\n{prefix}status <url> - Run a status check on any website" +
                    $"\n{prefix}8ball <question> - Try your luck!" +
                    $"\n{prefix}dab - Sends a random picture of dabbing" +
                    $"\n{prefix}meme <template>, <top text>, <buttom text> - Generates a meme using the meme template" +
                    $"\n{prefix}expose <user> - Expose someone!" +
                    $"\n{prefix}repeat <message> - Repeats what you typed" +
                    $"\n{prefix}repeattts <message> - Repeats what you typed but in TTS!" +
                    $"\n{prefix}riskage - Gives you a riskage" +
                    $"\n{prefix}riskage spil - Sends a link to the game based on Riskage!" +
                    $"\n{prefix}info - Info" +
                    $"\n{prefix}rate <anything> - Rates something out of 10" +
                    $"\n{prefix}roll <min> <max> - Rolls between <min> and <max>" +
                    $"\n{prefix}jojo - Jojo's bizare adventure" +
                    $"\n{prefix}pia - Kast sko efter pia" +
                    $"\n{prefix}random - Posts a random message"),
                new PageBuilder()
                    .WithTitle("Misc Commands")
                    .WithDescription($"\n{prefix}avatar <user> - Gets the avatar of the user" +
                    $"\n{prefix}donate - Sends a link to our Patreon page" +
                    $"\n{prefix}emote <emote> - Enlarges emote" +
                    $"\n{prefix}modlogchannel - Sends a link to the current modlog channel" +
                    $"\n{prefix}welcomechannel - Sends a link to the current welcome channel" +
                    $"\n{prefix}membercount - Tells you how many people are on this server" +
                    $"\n{prefix}serverinfo - Gives info about the server" +
                    $"\n{prefix}userinfo - Gives info about a user" +
                    $"\n{prefix}roleinfo <role> - Gives info about a role" +
                    $"\n{prefix}rolemembers <role> - Gives you the list of members in a specific role" +
                    $"\n{prefix}textowner <message> - Sends a message to the owner of this bot" +
                    $"\n{prefix}invite - Invite me to your server" +
                    $"\n{prefix}upvote - Gives you a link to the bot's upvote page" +
                    $"\n{prefix}urban <word>" +
                    $"\n{prefix}youtube <query>"),
                new PageBuilder()
                    .WithTitle("MyAnimeList Commands")
                    .WithDescription($"{prefix}anime <query> - Searches Anime from MyAnimeList.net" +
                    $"\n{prefix}manga <query> - Searches Manga from MyAnimeList.net" +
                    $"\n{prefix}mal user <user> Searches a user on MyAnimeList.net"),
                new PageBuilder()
                    .WithTitle("Music Commands")
                    .WithDescription($"{prefix}play <query> - Searches on YouTube and plays the song" +
                    $"\n{prefix}join - Joins the voicechannel you are currently in" +
                    $"\n{prefix}stop - Stops the music and clears the queue" +
                    $"\n{prefix}skip - Skips the current song" +
                    $"\n{prefix}np - Displays the current song" +
                    $"\n{prefix}queue - Shows the queue" +
                    $"\n{prefix}volume <value> - Sets the volume to the given value (0-150)" +
                    $"\n{prefix}pause - pauses the song" +
                    $"\n{prefix}resume - resumes playing the song"),
                new PageBuilder()
                    .WithTitle("Animal API Commands")
                    .WithDescription($"{prefix}birb - Posts a random birb" +
                    $"\n{prefix}cat - Posts a random cat" +
                    $"\n{prefix}dog - Posts a random dog" +
                    $"\n{prefix}duck - Posts a random duck" +
                    $"\n{prefix}fox - Posts a random fox"),
                new PageBuilder()
                    .WithTitle("Image Manipulation Commands")
                    .WithDescription($"Image can be an attachment or a url" +
                    $"\n{prefix}blur <factor> <image> - Blurs the given image by the given factor" +
                    $"\n{prefix}brightness <factor> <image> - Alters the brightness of the given image by the given factor" +
                    $"\n{prefix}contrast <factor> <image> - Alters the contrast of the given image by the given factor" +
                    $"\n{prefix}crop <width> <height> <image> - Crops the image to the given width & height" +
                    $"\n{prefix}pixelate <factor> <image> - Pixelates the given image by the given factor" +
                    $"\n{prefix}resize <width> <height> <image> - Resizes the given image to the given width and height" +
                    $"\n{prefix}saturate <factor> <image> - Alters the saturation of the given image by the given factor" +
                    $"\n{prefix}invert <image> - Inverts the colors of the given image" +
                    $"\n{prefix}oil <image> - Converts the image into oil painting" +
                    $"\n{prefix}grayscale <image> - Puts the given image in grayscale" +
                    $"\n{prefix}flip <image> - Flips the given image upside-down" +
                    $"\n{prefix}sepia <image> - Applies a Sepia filter to the given image" +
                    $"\n{prefix}upscale <image> - Upscales the given image to x2" +
                    $"\n{prefix}rotate <degrees> <image> - Rotates the given image by the given number of degrees" +
                    $"\n{prefix}imagetext <image> <text> - Draws text on the given image" +
                    $"\n{prefix}skew <x> <y> <image> - Skews the given image by x and y degrees"),
                new PageBuilder()
                    .WithTitle("Moderation Commands")
                    .WithDescription($"{prefix}purge <amount> - Deletes messages in bulk" +
                    $"\n{prefix}kick <user> <reason> - Kicks a user" +
                    $"\n{prefix}ban <user> <reason> - Bans a user" +
                    $"\n{prefix}unban <user> <reason> - Unbans a user" +
                    $"\n{prefix}setprefix <new prefix> - Sets the new prefix for this server" +
                    $"\n{prefix}setwelcome - Use this in the channel you want to have welcome messages in" +
                    $"\n{prefix}unsetwelcome - Disables the welcome messages" +
                    $"\n{prefix}setmodlog - Use this in the channel you want to have the mod-log in" +
                    $"\n{prefix}unsetmodlog - Disables the mod-log" +
                    $"\n{prefix}banword <word> - Will add a word to delete if its written in chat" +
                    $"\n{prefix}unbanword <word> - Will remove a word to delete if its written in chat" +
                    $"\n{prefix}bannedwords - Gives you the list of all of the banned words on the server" +
                    $"\n{prefix}disablelevelmsg - Disables level-up messages" +
                    $"\n{prefix}enablelevelmsg - Enables level-up messages" +
                    $"\n{prefix}slowmode <seconds> - Sets the slowmode of the current channel to the given interval"),
                new PageBuilder()
                    .WithTitle("Currency Commands")
                    .WithDescription($"{prefix}tokens <target> - See how many tokens you or your target have" +
                    $"\n{prefix}award <user> <amount> <comment> - Award the user with tokens" +
                    $"\n{prefix}awardall <amount> <comment> - Award everyone with tokens!" +
                    $"\n{prefix}daily - Claim your daily tokens!" +
                    $"\n{prefix}pay <target> <amount> <comment> - Pay your target tokens" +
                    $"\n{prefix}leaderboard tokens <page> - Shows the leaderboard for this server" +
                    $"\n{prefix}gamble <amount> - Gamble your tokens!" +
                    $"\n{prefix}stats <user> - Shows the stats of a user" +
                    $"\n{prefix}resetuser <user> - Resets a user's stats"),
                new PageBuilder()
                    .WithTitle("Leveling Commands")
                    .WithDescription($"{prefix}resetuser <user> - Resets a user's stats" +
                    $"\n{prefix}stats <user> - Shows the stats of a user" +
                    $"\n{prefix}leaderboard leveling <page> - Shows the leaderboard for this server"),
                new PageBuilder()
                    .WithTitle("Role Commands")
                    .WithDescription($"{prefix}addrole leveling <role> <level>" +
                    $"\n{prefix}removerole leveling <role>"),
                new PageBuilder()
                    .WithTitle("Osu! Commands")
                    .WithDescription($"{prefix}osu <gamemode> <osu username> - Shows their osu! profile/stats" +
                    $"\n{prefix}osutop <gamemode> <osu username> - Shows their top 10 scores"),
                new PageBuilder()
                    .WithTitle("Bonus Commands")
                    .WithDescription($"{prefix}frede - Checks if you are Frede or not" +
                    $"\n{prefix}scarce - Check if you are DaRealScarce or not" +
                    $"\n{prefix}frede er sur" +
                    $"\n{prefix}daddy - Daddy" +
                    $"\n{prefix}fredrik - 42" +
                    $"\n{prefix}rune - The hero" +
                    $"\n{prefix}vuk - Memorial" +
                    $"\n{prefix}play <song> - Plays a song" +
                    $"\n{prefix}ping - Pong!" +
                    $"\n{prefix}paginator <words> - Make a Paginated message with the words (seperate pages with , (comma)" +
                    $"\n{prefix}count <number>"),
                new PageBuilder()
                    .WithTitle("Calculator Commands")
                    .WithDescription($"{prefix}calc+ <numbers to add> - Add numbers" +
                    $"\n{prefix}calc- <numbers to sumtract> - Subtract numbers" +
                    $"\n{prefix}calc* <numbers to multiply> - multiply numbers" +
                    $"\n{prefix}calc/ <numbers to divide> -  Divide numbers" +
                    $"\n{prefix}calcaverage <numbers> - Finds the average value fóf the numbers" +
                    $"\n{prefix}calcarea <radius in degress> - Calculates the area of a circle with radius" +
                    $"\n{prefix}calcomkreds <diameter> - Calculates the circumference of a circle with diameter" +
                    $"\n{prefix}calcsqrt <number> - Finds the squareroot of the number" +
                    $"\n{prefix}calcpow <number> <exponent> - Calculates the power of a number"),
                new PageBuilder()
                    .WithTitle("Calculator Commands Advanced")
                    .WithDescription($"{prefix}calccos <angel in degress> - Calculates the cosine with an angel" +
                    $"\n{prefix}calcsin <angel in degress> - Calculates the sine with an angel" +
                    $"\n{prefix}calctan <angel in degress> - Calculates the tangent with an angel" +
                    $"\n{prefix}calcacos <cos value [-1,1]> - Calculates the angel in degress with the cosine value" +
                    $"\n{prefix}calcasin <sin value [-1,1]> - Calculates the angel in degress with the sine value" +
                    $"\n{prefix}calcatan <tan value> - Calculates the angel in degress with the tanget value" +
                    $"\n{prefix}calcatan2 <cathetus 1> <cathetus 2> - Calculates the angel in degress with the 2 cathetus'"),
                new PageBuilder()
                    .WithTitle("Top Secret Developer Section")
                    .WithDescription("Nothing to see here!"),
            ];

            var paginator = new StaticPaginatorBuilder()
                .AddUser(Context.User)
                .WithPages(pages)
                .WithFooter(PaginatorFooter.PageNumber)
                .Build();

            await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));

        }
        [Command("paginator", RunMode = RunMode.Async)]
        public async Task paginator([Remainder] string str = "")
        {
            string[] words = str.Split(",");
            if (!words.Any() || (words.Length == 1 && string.IsNullOrWhiteSpace(words[0])))
            {
                await ReplyAsync(Context.User.Mention + " You gotta give me something to paginate");
            }
            else
            {
                var pages = new List<IPageBuilder>();
                // Add an intro page
                pages.Add(new PageBuilder()
                    .WithTitle("Your paginator")
                    .WithColor(new Color(0, 0, 255))
                    .WithDescription("You made these pages containing one word each unless you separated it with a comma" +
                                    "\n\nOnly the user who sent the command can use the paginator."));
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
                await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("rolemembers", RunMode = RunMode.Async)]
        public async Task rolemembers([Remainder] SocketRole role)
        {
            var members = role.Members.OrderBy(o => o.Nickname ?? o.Username).Select(x => x.Mention).ToList();
            if (members.Count <= 0)
            {
                await ReplyAsync($"{role.Name} has 0 members");
            }
            else
            {
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
                await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
            }
        }
        [Alias("define")]
        [Command("urban", RunMode = RunMode.Async)]
        public async Task urban([Remainder] string word)
        {
            if (Context.Guild.Id == 264445053596991498 && !((ITextChannel)Context.Channel).IsNsfw)
            {
                await ReplyAsync("NSFW channel == false");
            }
            else
            {
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
                    await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                }
            }
        }
        [Ratelimit(1, 5, Measure.Seconds)]
        [Command("anime", RunMode = RunMode.Async)]
        public async Task anime([Remainder] string name)
        {
            try
            {

                var anime = await _jikan.SearchAnimeAsync(name);
                if (anime == null)
                {
                    await ReplyAsync("The API didn't return anything :(");
                    return;
                }
                var results = anime.Data;
                if (results.Any())
                {
                    var pageBuilders = new List<IPageBuilder>();
                    int NSFW = 0;
                    foreach (var result in results)
                    {
                        if ((result.Rating == "Rx" && !(Context.Channel as ITextChannel).IsNsfw) || result == null)
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
                    await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
                }
                else
                {
                    await ReplyAsync("The API didn't return anything :(");
                }

            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
        }
        [Ratelimit(1, 5, Measure.Seconds)]
        [Command("manga", RunMode = RunMode.Async)]
        public async Task manga([Remainder] string name)
        {
            try
            {

                var manga = await _jikan.SearchMangaAsync(name);
                if (manga == null)
                {
                    await ReplyAsync("The API didn't return anything :(");
                    return;
                }
                var results = manga.Data;
                if (results.Any())
                {
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
                    await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));
                }
                else
                {
                    await ReplyAsync("The API didn't return anything :(");
                }

            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
        }
        [Group("mal")]
        public class MyAnimeListModule : ModuleBase<SocketCommandContext>
        {
            private readonly Jikan _jikan;
            private readonly InteractiveService _interactive;

            public MyAnimeListModule(Jikan jikan, InteractiveService interactive)
            {
                _jikan = jikan;
                _interactive = interactive;
            }
            [Priority(-1)]
            [Command]
            public async Task MyAsyncTask()
            {
                await ReplyAsync("See the help section with MyAnimeList commands");
            }
            [Alias("profile")]
            [Priority(1)]
            [Command("user", RunMode = RunMode.Async)]
            [Summary("Searches for a user on MyAnimeList.net")]
            public async Task MALUserAsync([Remainder] string username)
            {
                // Helper to ensure no null/empty/whitespace field values
                string SafeField(object value)
                {
                    if (value == null) return "N/A";
                    var str = value.ToString();
                    return string.IsNullOrWhiteSpace(str) ? "N/A" : str;
                }

                var user = await _jikan.GetUserProfileAsync(username);
                if (user == null)
                {
                    await ReplyAsync("The API didn't return anything");
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
                        .WithDescription(SafeField(about?.Data?.About?.Replace("<br>","")))
                        .AddField("User Id", SafeField(user.Data?.MalId), true)
                        .AddField("Gender", SafeField(user.Data?.Gender), true)
                        .AddField("Location", SafeField(user.Data?.Location), true)
                        .AddField("Last Online", SafeField(user.Data?.LastOnline), true)
                        .AddField("Join Date", SafeField(user.Data?.Joined), true)
                        .AddField("Birthday", SafeField(user.Data?.Birthday), true)
                        .WithAuthor(new EmbedAuthorBuilder
                        {
                            Name = SafeField(user.Data?.Username),
                            IconUrl = user.Data?.Images.JPG.ImageUrl,
                            Url = SafeField(user.Data?.Url)
                        })
                        .WithThumbnailUrl(user.Data?.Images.JPG.ImageUrl)
                        .WithColor(new Color(0, 0, 255)),
                    new PageBuilder()
                        .WithTitle("Anime Statistics")
                        .AddField("Total Anime Entries", SafeField(stats?.Data?.AnimeStatistics?.TotalEntries))
                        .AddField("Total Episodes Watched", SafeField(stats?.Data?.AnimeStatistics?.EpisodesWatched), true)
                        .AddField("Mean Score", SafeField(stats?.Data?.AnimeStatistics?.MeanScore), true)
                        .AddField("Completed", SafeField(stats?.Data?.AnimeStatistics?.Completed))
                        .AddField("Watching", SafeField(stats?.Data?.AnimeStatistics?.Watching))
                        .AddField("Plan to Watch", SafeField(stats?.Data?.AnimeStatistics?.PlanToWatch), true)
                        .AddField("On Hold", SafeField(stats?.Data?.AnimeStatistics?.OnHold), true)
                        .AddField("Dropped", SafeField(stats?.Data?.AnimeStatistics?.Dropped), true)
                        .AddField("Rewatched", SafeField(stats?.Data?.AnimeStatistics?.Rewatched), true)
                        .AddField("Days Watched", SafeField(stats?.Data?.AnimeStatistics?.DaysWatched)),
                    new PageBuilder()
                        .WithTitle("Manga Statistics")
                        .AddField("Total Manga Entries", SafeField(stats?.Data?.MangaStatistics?.TotalEntries))
                        .AddField("Total Chapters Read", SafeField(stats?.Data?.MangaStatistics?.ChaptersRead), true)
                        .AddField("Mean Score", SafeField(stats?.Data?.MangaStatistics?.MeanScore), true)
                        .AddField("Completed", SafeField(stats?.Data?.MangaStatistics?.Completed))
                        .AddField("Reading", SafeField(stats?.Data?.MangaStatistics?.Reading))
                        .AddField("Plan to Read", SafeField(stats?.Data?.MangaStatistics?.PlanToRead), true)
                        .AddField("On Hold", SafeField(stats?.Data?.MangaStatistics?.OnHold), true)
                        .AddField("Dropped", SafeField(stats?.Data?.MangaStatistics?.Dropped), true)
                        .AddField("Reread", SafeField(stats?.Data?.MangaStatistics?.Reread), true),
                    // Only add WithImageUrl if imageUrl is not null or empty
                    (string.IsNullOrEmpty(imageUrl)
                        ? new PageBuilder()
                            .WithTitle("Favorites")
                            .AddField($"Favorite Anime ({favorites?.Data?.Anime.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Anime.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite anime"], 1024, "\n")))
                            .AddField($"Favorite Manga ({favorites?.Data?.Manga.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Manga.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite manga"], 1024, "\n")))
                            .AddField($"Favorite Character(s) ({favorites?.Data?.Characters.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Characters.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite characters"], 1024, "\n")), true)
                            .AddField($"Favorite People ({favorites?.Data?.People.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.People.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite people"], 1024, "\n")), true)
                        : new PageBuilder()
                            .WithTitle("Favorites")
                            .WithImageUrl(imageUrl)
                            .AddField($"Favorite Anime ({favorites?.Data?.Anime.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Anime.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite anime"], 1024, "\n")))
                            .AddField($"Favorite Manga ({favorites?.Data?.Manga.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Manga.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite manga"], 1024, "\n")))
                            .AddField($"Favorite Character(s) ({favorites?.Data?.Characters.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.Characters.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite characters"], 1024, "\n")), true)
                            .AddField($"Favorite People ({favorites?.Data?.People.Count ?? 0})", SafeField(HelperFunctions.JoinWithLimit(favorites?.Data?.People.Select(x => $"[{x.Title}]({x.Url})") ?? ["No favorite people"], 1024, "\n")), true)
                    ),
                    new PageBuilder()
                        .WithTitle("User History")
                        .AddField("Name", SafeField(HelperFunctions.JoinWithLimit(history?.Data?.Select(x => $"[{x.Metadata.Name}]({x.Metadata.Url})") ?? ["N/A"], 1024, "\n")), true)
                        .AddField("Increment", SafeField(HelperFunctions.JoinWithLimit(history?.Data?.Select(x => x.Increment.ToString()) ?? ["N/A"], 1024, "\n")), true)
                        .AddField("Date", SafeField(HelperFunctions.JoinWithLimit(history?.Data?.Select(x => x.Date?.ToString() ?? "N/A") ?? ["N/A"], 1024, "\n")), true),
                    new PageBuilder()
                        .WithTitle($"Friends ({friends?.Data?.Count ?? 0})")
                        .WithDescription(SafeField(HelperFunctions.JoinWithLimit(friends?.Data?.Select(x => $"[{x.User.Username}]({x.User.Url})") ?? ["N/A"], 1024, "\n")))
                };
                var paginator = new StaticPaginatorBuilder()
                    .AddUser(Context.User)
                    .WithPages(pages)
                    .WithFooter(PaginatorFooter.PageNumber)
                    .Build();
                await _interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromMinutes(5));


            }
        }
    }
}
