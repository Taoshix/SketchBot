using Discord;
using Discord.Interactions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SketchBot.Custom_Preconditions;
using SketchBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.InteractionBasedModules
{
    public class AnimalAPIModule : InteractionModuleBase<SocketInteractionContext>
    {
        InteractiveService _interactive;
        public AnimalAPIModule(InteractiveService interactiveService)
        {
            _interactive = interactiveService;
        }

        [Ratelimit(1, 5, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers)]
        [SlashCommand("cat", "Sends a random cat image")]
        public async Task CatAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
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
                await FollowupAsync("The API didn't return anything");
            }
        }
        [Ratelimit(1, 5, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers)]
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
                await FollowupAsync("The API didn't return anything");
            }
        }
        [Ratelimit(1, 5, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers)]
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
                await FollowupAsync("The API didn't return anything");
            }
        }
        [Ratelimit(1, 5, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers)]
        [SlashCommand("duck", "Posts a random picture of a dog")]
        public async Task DuckAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
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
                await FollowupAsync("The API didn't return anything");
            }
        }
        [Ratelimit(1, 5, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers)]
        [SlashCommand("dog", "Posts a random picture of a dog")]
        public async Task DogAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
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
                await FollowupAsync("The API didn't return anything");
            }
        }
        [Ratelimit(1, 5, Measure.Seconds, RatelimitFlags.NoLimitForDevelopers)]
        [SlashCommand("neko", "Sends random Nekos")]
        public async Task NekoAsync()
        {
            await DeferAsync();
            try
            {
                using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
                string websiteurl = "https://api.nekosapi.com/v4/images/random?limit=5&rating=safe";
                client.BaseAddress = new Uri(websiteurl);
                HttpResponseMessage response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                var nekoList = JsonConvert.DeserializeObject<List<NekoAPIImage>>(result);

                if (nekoList == null || nekoList.Count == 0)
                {
                    await FollowupAsync("No neko images found.");
                    return;
                }

                var pages = new List<IPageBuilder>();
                foreach (var neko in nekoList)
                {
                    var page = new PageBuilder()
                        .WithTitle("Random Neko Image")
                        .WithDescription($"Id: {neko.Id}\n" +
                        $"Artist: {neko.ArtistName ?? "Unknown"}\n" +
                        $"Rating: {neko.Rating}\n" +
                        $"Dominant Color: RGB({neko.ColorDominant.R}, {neko.ColorDominant.G}, {neko.ColorDominant.B})\n" +
                        $"Tags: {string.Join(", ", neko.Tags ?? new List<string>())}\n")
                        .WithImageUrl(neko.Url)
                        .WithUrl(neko.SourceUrl)
                        .WithCurrentTimestamp()
                        .WithAuthor("Nekos API", "https://nekosapi.com/branding/logo/logo.png", "https://nekosapi.com")
                        .WithColor(Color.Purple);
                    pages.Add(page);
                }

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
                await FollowupAsync("The API didn't return anything");
            }
        }
    }
}
