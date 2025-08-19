using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Sketch_Bot.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.WebSocket.EventArgs;

namespace Sketch_Bot.Services
{
    public class AudioService
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lavaNode;
        private readonly DiscordSocketClient _socketClient;
        private readonly ILogger _logger;
        public readonly HashSet<ulong> VoteQueue;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        public readonly ConcurrentDictionary<ulong, ulong> TextChannels;

        public AudioService(
            LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode,
            DiscordSocketClient socketClient,
            ILogger<AudioService> logger)
        {
            _lavaNode = lavaNode;
            _socketClient = socketClient;
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            _logger = logger;
            TextChannels = new ConcurrentDictionary<ulong, ulong>();
            VoteQueue = [];
            _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
            _lavaNode.OnStats += OnStatsAsync;
            _lavaNode.OnPlayerUpdate += OnPlayerUpdateAsync;
            _lavaNode.OnTrackEnd += OnTrackEndAsync;
            _lavaNode.OnTrackStart += OnTrackStartAsync;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnTrackException += OnTrackExceptionAsync;
        }

        private Task OnTrackExceptionAsync(TrackExceptionEventArg arg)
        {
            return SendAndLogMessageAsync(arg.GuildId,
                $"Track {arg.Track.Title} encountered an error: {arg.Exception.Message}");
        }

        private Task OnTrackStartAsync(TrackStartEventArg arg)
        {
            return SendAndLogMessageAsync(arg.GuildId,
                $"Now playing: {arg.Track.Title}");
        }

        private async Task OnTrackEndAsync(TrackEndEventArg arg)
        {

            if (arg.Reason != TrackEndReason.Finished)
            {
                return;
            }

            LavaTrack track = arg.Track;
            await SendAndLogMessageAsync(arg.GuildId,$"Track {track.Title} ended!");

            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(arg.GuildId);
            var queue = player.GetQueue();
            if(queue.Count > 0)
            {
                var nextTrack = queue.FirstOrDefault();
                queue.RemoveAt(0);
                await player.PlayAsync(_lavaNode, nextTrack);
            }
        }

        private Task OnPlayerUpdateAsync(PlayerUpdateEventArg arg)
        {
            var voicechannel = _socketClient.Guilds.FirstOrDefault(g => g.Id == arg.GuildId)?.VoiceChannels.FirstOrDefault(x => x.ConnectedUsers.Select(x => x.Id).Contains(_socketClient.CurrentUser.Id));
            int connectedUsers = voicechannel?.ConnectedUsers.Count(x => x.Id != _socketClient.CurrentUser.Id) ?? 0;
            _logger.LogInformation("Guild latency: {0} Connected Users excluding the bot {1}", arg.Ping, connectedUsers);
            return Task.CompletedTask;
        }

        private Task OnStatsAsync(StatsEventArg arg)
        {
            _logger.LogInformation("{}", JsonSerializer.Serialize(arg));
            return Task.CompletedTask;
        }

        private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
        {
            _logger.LogCritical("{}", JsonSerializer.Serialize(arg));
            return Task.CompletedTask;
        }

        private Task SendAndLogMessageAsync(ulong guildId,
                                            string message)
        {
            _logger.LogInformation(message);
            if (!TextChannels.TryGetValue(guildId, out var textChannelId))
            {
                return Task.CompletedTask;
            }

            return (_socketClient
                    .GetGuild(guildId)
                    .GetChannel(textChannelId) as ITextChannel)
                .SendMessageAsync(message);
        }
        private Task OnTrackStuck(TrackStuckEventArg arg)
        {
            return SendAndLogMessageAsync(arg.GuildId,
                $"Track {arg.Track.Title} got stuck. Threshold: {arg.Threshold}");
        }
    }
}
