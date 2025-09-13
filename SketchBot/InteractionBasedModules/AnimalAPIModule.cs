using Discord.Interactions;
using Newtonsoft.Json.Linq;
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
        public AnimalAPIModule()
        { 

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
    }
}
