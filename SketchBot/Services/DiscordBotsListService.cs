using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using System.Linq;
using Sketch_Bot.Models;
using DiscordBotsList;
using DiscordBotsList.Api;
using Newtonsoft.Json;

namespace Sketch_Bot.Services
{
    public class DiscordBotsListService
    {
        private AuthDiscordBotListApi _dblApi;
        public DiscordBotsListService()
        {

        }
        public AuthDiscordBotListApi DblApi(ulong botId)
        {
            if(_dblApi == null)
            {
                Config config = Config.Load();
                AuthDiscordBotListApi dblApi = new AuthDiscordBotListApi(botId,
                    config.DblApiKey);
                _dblApi = dblApi;
            }
            return _dblApi;
        }
    }
}
