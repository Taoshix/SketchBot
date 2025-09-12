using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotsList.Api;
using SketchBot.Services;
using Victoria;

namespace SketchBot.Utils
{
    public class Globals
    {
        public SocketCommandContext Context;
        public SocketGuild Guild;
        public AuthDiscordBotListApi DblApi;
        public IServiceProvider ServiceProvider;
        public LavaNode<LavaPlayer<LavaTrack>, LavaTrack> LavaNode;
        public CachingService CachingService;
    }
}
