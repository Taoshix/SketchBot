using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using DiscordBotsList.Api;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;

namespace SketchBot.Models
{
    public static class DiscordBots
    {
        public static async Task UpdateDblStatsAsync(int servercount, ulong botId)
        {
            try
            {
                Config config = Config.Load();
                AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(botId,
                    config.DblApiKey);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(servercount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static async Task UpdateDiscordBotsGgStatsAsync(int servercount, ulong botId)
        {
            try
            {
                Config config = Config.Load();
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", config.DbggApiKey);
                var json = JsonConvert.SerializeObject(new { guildCount = servercount });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"https://discord.bots.gg/api/v1/bots/{botId}/stats", content);
                var responseText = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseText);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
