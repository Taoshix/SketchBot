using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ImgFlip4NET;
using Newtonsoft.Json;
using SketchBot.Models;

namespace SketchBot.Services
{
    public class MemeService
    {
        DiscordSocketClient _client;
        Config _config;
        ImgFlipOptions options = new ImgFlipOptions();

        ImgFlipService service;
        public MemeService(DiscordSocketClient client)
        {
            _client = client;
        }
        public ImgFlipService GetMemeService()
        {
            _config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("config.json"));
            options.Username = "Taoshi";
            options.Password = _config.IMGFlip;
            service = new ImgFlipService(options);
            return service;
        }
    }
}
