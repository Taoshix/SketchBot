using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotsList.Api;

namespace Sketch_Bot.Models
{
    public class Globals
    {
        public SocketInteractionContext Context;
        public SocketGuild Guild;
        public AuthDiscordBotListApi DblApi;
        public IServiceProvider ServiceProvider;
    }
}
