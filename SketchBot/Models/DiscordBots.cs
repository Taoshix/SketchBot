using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using DiscordBotsList.Api;
using System.IO;
using Newtonsoft.Json;

namespace Sketch_Bot.Models
{
    public static class DiscordBots
    {
        public static async Task UpdateStats(int servercount)
        {
            try
            {
                Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
                AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(369865463670374400,
                    config.DblApiKey);
                var me = await DblApi.GetMeAsync();
                await me.UpdateStatsAsync(servercount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public static async Task UpdateStats2(int servercount)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://discord.bots.gg/api/v1/bots/369865463670374400/stats");
                Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
                request.ContentType = "application/json";
                request.Method = "POST";
                request.Headers.Add("Authorization",
                    config.DblApiKey);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = "{ \"guildCount\" : " + servercount + " }";

                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                var httpResponse = (HttpWebResponse) request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    Console.WriteLine(responseText);

                    //Now you have your response.
                    //or false depending on information in the response     
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
