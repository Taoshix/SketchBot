using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using DiscordBotsList.Api;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;

namespace Sketch_Bot.Models
{
    public static class DiscordBots
    {
        public static async Task UpdateStats(int servercount)
        {
            try
            {
                Config config = Config.Load();
                AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(369865463670374400,
                    config.DblApiKey);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(servercount);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
            }
        }
        public static async Task UpdateStats2(int servercount)
        {
            try
            {
                Config config = Config.Load();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", config.DbggApiKey);
                    var json = JsonConvert.SerializeObject(new { guildCount = servercount });
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("https://discord.bots.gg/api/v1/bots/369865463670374400/stats", content);
                    var responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseText);    
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
