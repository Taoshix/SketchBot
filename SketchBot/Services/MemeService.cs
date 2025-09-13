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
        private readonly DiscordSocketClient _client;
        private Config _config;
        private ImgFlipOptions _options;
        private ImgFlipService _service;

        public MemeService(DiscordSocketClient client)
        {
            _client = client;
        }

        public ImgFlipService GetMemeService()
        {
            if (_service != null)
                return _service;

            _config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("config.json"));
            _options = new ImgFlipOptions
            {
                Username = "Taoshi",
                Password = _config.IMGFlip
            };
            _service = new ImgFlipService(_options);
            return _service;

        }
    }
}
