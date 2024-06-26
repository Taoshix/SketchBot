﻿using System;
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
            Config config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("config.json"));
            AuthDiscordBotListApi dblApi = new AuthDiscordBotListApi(369865463670374400,
                config.DblApiKey);
            _dblApi = dblApi;
        }
        public AuthDiscordBotListApi DblApi()
        {
            return _dblApi;
        }
    }
}
