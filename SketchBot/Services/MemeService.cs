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
using Sketch_Bot.Models;

namespace Sketch_Bot.Services
{
    public class MemeService
    {
        DiscordSocketClient _client;
        Config _config;
        ImgFlipOptions options = new ImgFlipOptions();

        ImgFlipService service;
        public MemeService(DiscordSocketClient client, Config config)
        {
            _client = client;
            _config = config;
        }
        public ImgFlipService GetMemeService()
        {
            options.Username = "Taoshi";
            options.Password = _config.IMGFlip;
            service = new ImgFlipService(options);
            return service;
        }
    }
}
