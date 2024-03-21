using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Discord;
using Discord.Audio;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using Sketch_Bot.Models;
using Victoria;
using Discord.WebSocket;
using Victoria.Node;
using Victoria.Player;
using Victoria.Node.EventArgs;
using Microsoft.Extensions.Logging;

namespace Sketch_Bot.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private readonly LavaNode _lavaNode;
        private readonly ILogger _logger;

        public AudioService(LavaNode lavaNode, ILoggerFactory loggerFactory)
        {
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            _logger = loggerFactory.CreateLogger<LavaNode>();
            _lavaNode = lavaNode;
            _lavaNode.OnTrackEnd += OnTrackEndAsync;
            _lavaNode.OnTrackStart += OnTrackStartAsync;
            _lavaNode.OnStatsReceived += OnStatsReceivedAsync;
            _lavaNode.OnUpdateReceived += OnUpdateReceivedAsync;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
            _lavaNode.OnTrackStuck += OnTrackStuckAsync;
            _lavaNode.OnTrackException += OnTrackExceptionAsync;
        }
        public async Task OnTrackStarted(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value))
            {
                return;
            }

            if (value.IsCancellationRequested)
            {
                return;
            }

            value.Cancel(true);
            await arg.Player.TextChannel.SendMessageAsync("Auto disconnect has been cancelled!");
        }
        public async Task InitiateDisconnectAsync(LavaPlayer<LavaTrack> player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            await player.TextChannel.SendMessageAsync($"Auto disconnect initiated! Disconnecting in {timeSpan}...");
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await player.TextChannel.SendMessageAsync("Thanks for using the music feature.");
        }
        private static Task OnTrackExceptionAsync(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            arg.Player.Vueue.Enqueue(arg.Track);
            return arg.Player.TextChannel.SendMessageAsync($"{arg.Track} has been requeued because it threw an exception.");
        }

        private static Task OnTrackStuckAsync(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            arg.Player.Vueue.Enqueue(arg.Track);
            return arg.Player.TextChannel.SendMessageAsync($"{arg.Track} has been requeued because it got stuck.");
        }

        private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
        {
            Console.WriteLine($"{arg.Code} {arg.Reason}");
            return Task.CompletedTask;
        }

        private Task OnStatsReceivedAsync(StatsEventArg arg)
        {
            Console.WriteLine($"Players: {arg.Players}\nPlaying Players: {arg.PlayingPlayers}\nUptime: {arg.Uptime}");
            return Task.CompletedTask;
        }

        private static Task OnUpdateReceivedAsync(UpdateEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            return arg.Player.TextChannel.SendMessageAsync(
                $"Player update received: {arg.Position}/{arg.Track?.Duration}");
        }

        private static Task OnTrackStartAsync(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            return arg.Player.TextChannel.SendMessageAsync($"Started playing {arg.Track}.");
        }

        private static Task OnTrackEndAsync(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            return arg.Player.TextChannel.SendMessageAsync($"Finished playing {arg.Track}.");
        }




















        /*
        //private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        //public static IAudioClient client;


        private Lavalink _lavalink;
        private DiscordSocketClient _client;

        public AudioService(Lavalink lavalink, DiscordSocketClient client)
        {
            _lavalink = lavalink;
            _client = client;
        }

        private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> _lazyOptions
            = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

        private ConcurrentDictionary<ulong, AudioOptions> Options
            => _lazyOptions.Value;

        public async Task<int> number()
        {
            return _lavalink.ConnectedNodes;
        }

        public async Task forcenode()
        {
            var node = await _lavalink.AddNodeAsync(_client).ConfigureAwait(false);
            node.TrackFinished += OnFinshed;
        }
        public async Task newnode(Lavalink newlavalink)
        {
            _lavalink = newlavalink;
        }
        public async Task<Embed> JoinOrPlayAsync(SocketGuildUser user, IMessageChannel textChannel, ulong guildId, string query = null)
        {
            //Check If User Is Connected To Voice Channel.
            if (user.VoiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");

            //Check if user who used !Join is a user that has already summoned the Bot.
            /*if (Options.TryGetValue(user.Guild.Id, out var options) && options.Summoner.Id != user.Id)
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", $"I can't join another voice channel until {options.Summoner} disconnects me.");*/

        //If The user hasn't provided a Search string from the !Play command, then they must have used the !Join command.
        //Join the voice channel the user is in.

        /*
            if (query == null)
            {
                Console.WriteLine(_lavalink.ConnectedNodes); // returns 0
                await _lavalink.DefaultNode.ConnectAsync(user.VoiceChannel, textChannel /*This Param is Optional, Only used If we want to bind the Bot to a TextChannel For commands.*///);/*
        /*Options.TryAdd(user.Guild.Id, new AudioOptions
        {
            Summoner = user
        });
        await LoggingService.LogInformationAsync("Music", $"Now connected to {user.VoiceChannel.Name} and bound to {textChannel.Name}.");
        return await EmbedHandler.CreateBasicEmbed("Music", $"Now connected to {user.VoiceChannel.Name} and bound to {textChannel.Name}.", Color.Blue);
    }
    else
    {
        try
        {
            //Try get the player. If it returns null then the user has used the command !Play without using the command !Join.
            var player = _lavalink.DefaultNode.GetPlayer(guildId);
            if (player == null)
            {
                //User Used Command !Play before they used !Join
                //So We Create a Connection To The Users Voice Channel.
                await _lavalink.DefaultNode.ConnectAsync(user.VoiceChannel, textChannel);
                Options.TryAdd(user.Guild.Id, new AudioOptions
                {
                    Summoner = user
                });
                //Now we can set the player to out newly created player.
                player = _lavalink.DefaultNode.GetPlayer(guildId);
            }

            //Find The Youtube Track the User requested.
            var search = await _lavalink.DefaultNode.SearchYouTubeAsync(query);

            //If we couldn't find anything, tell the user.
            if (search.LoadResultType == LoadResultType.NoMatches)
                return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");

            //Get the first track from the search results.
            var track = search.Tracks.FirstOrDefault();

            //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
            if (player.CurrentTrack != null && player.IsPlaying || player.IsPaused)
            {
                player.Queue.Enqueue(track);
                await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the music queue.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", Color.Blue);
            }
            //Player was not playing anything, so lets play the requested track.
            await player.PlayAsync(track,false);
            await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Uri}");
            return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Uri}", Color.Blue);
        }
        //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
        catch (Exception ex)
        {
            return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", ex.ToString());
        }
    }

}

/*This is ran when a user uses the command Leave.
    Task Returns an Embed which is used in the command call. *//*
public async Task<Embed> LeaveAsync(ulong guildId)
{
    try
    {
        //Get The Player Via GuildID.
        var player = _lavalink.DefaultNode.GetPlayer(guildId);

        //if The Player is playing, Stop it.
        if (player.IsPlaying)
            await player.StopAsync();

        //Leave the voice channel.
        var channelName = player.VoiceChannel.Name;
        await _lavalink.DefaultNode.DisconnectAsync(guildId);
        await LoggingService.LogInformationAsync("Music", $"Bot has left {channelName}.");
        return await EmbedHandler.CreateBasicEmbed("Music", $"I've left {channelName}.", Color.Blue);
    }
    //Tell the user about the error so they can report it back to us.
    catch (InvalidOperationException ex)
    {
        return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.ToString());
    }
}
/*
public async Task<Embed> Lyrics(ulong guildId)
{
    try
    {
        var player = _lavalink.DefaultNode.GetPlayer(guildId);
        player.
    }
    catch (Exception ex)
    {
        return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
    }
}
*/
        /*This is ran when a user uses the command List 
            Task Returns an Embed which is used in the command call. *//*
        public async Task<Embed> ListAsync(ulong guildId)
        {
            try
            {
                var prefix = ServerSettingsDB.GetPrefix(guildId.ToString()).FirstOrDefault()?.Prefix;
                /* Create a string builder we can use to format how we want our list to be displayed. */
        /*var descriptionBuilder = new StringBuilder();

        /* Get The Player and make sure it isn't null. */
        /*var player = _lavalink.DefaultNode.GetPlayer(guildId);
        if (player == null)
            return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check {prefix}Help for info on how to use the bot.");

        if (player.IsPlaying)
        {
            /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
                In this situation we simply return an embed that displays the current track instead. */
        /*if (player.Queue.Count < 1 && player.CurrentTrack != null)
        {
            return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.CurrentTrack.Title}", "Nothing Else Is Queued.", Color.Blue);
        }
        else
        {
            /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
             *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
        /*var trackNum = 2;
        foreach (var track in player.Queue.Items)
        {
            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Uri}) - {track.Id}\n");
            trackNum++;
        }
        return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})\n{descriptionBuilder.ToString()}", Color.Blue);
    }
}
else
{
    return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now. If this is an error, Please Contact Draxis.");
}
}
catch (Exception ex)
{
return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
}

}

/*This is ran when a user uses the command Skip 
Task Returns an Embed which is used in the command call. */
        /*
    public async Task<Embed> SkipTrackAsync(ulong guildId)
    {
        try
        {
            var prefix = ServerSettingsDB.GetPrefix(guildId.ToString()).FirstOrDefault().Prefix;
            var player = _lavalink.DefaultNode.GetPlayer(guildId);
            if (player == null)
                return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{prefix}Help for info on how to use the bot.");
            if (player.Queue.Count < 1)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only One or No songs currently playing." +
                    $"\n\nDid you mean {prefix}Stop?");
            }
            else
            {
                try
                {
                    var currentTrack = player.CurrentTrack;
                    await player.SkipAsync();
                    await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
                    return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skipped {currentTrack.Title}", Color.Blue);
                }
                catch (Exception ex)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.ToString());
                }

            }
        }
        catch (Exception ex)
        {
            return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.ToString());
        }
    }

    /*This is ran when a user uses the command Stop 
        Task Returns an Embed which is used in the command call. */
        /*
    public async Task<Embed> StopAsync(ulong guildId)
    {
        try
        {
            var prefix = ServerSettingsDB.GetPrefix(guildId.ToString()).FirstOrDefault().Prefix;
            var player = _lavalink.DefaultNode.GetPlayer(guildId);
            if (player == null)
                return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{prefix}Help for info on how to use the bot.");
            /* Check if the player exists, if it does, check if it is playing.
                 If it is playing, we can stop.
            if (player.IsPlaying)
                await player.StopAsync();
            /* Not sure if this is required as I think player.StopAsync(); clears the queue anyway. 
            foreach (var track in player.Queue.Items)
                player.Queue.Dequeue();
            await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
            return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
        }
        catch (Exception ex)
        {
            return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.ToString());
        }
    }

    /*This is ran when a user uses the command Volume 
        Task Returns a String which is used in the command call. */
        /*
    public async Task<string> VolumeAsync(ulong guildId, int volume)
    {
        if (volume >= 150 || volume <= 0)
        {
            return $"Volume must be between 0 and 150.";
        }
        try
        {
            var player = _lavalink.DefaultNode.GetPlayer(guildId);
            await player.SetVolumeAsync(volume);
            await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}");
            return $"Volume has been set to {volume}.";
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }

    public async Task<string> Pause(ulong guildId)
    {
        try
        {
            var player = _lavalink.DefaultNode.GetPlayer(guildId);
            if (player.IsPaused)
            {
                await player.PauseAsync();
                return $"**Resumed:** Now Playing {player.CurrentTrack.Title}";
            }

            await player.PauseAsync();
            return $"**Paused:** {player.CurrentTrack.Title}, what a bamboozle.";
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }

    public async Task<string> Resume(ulong guildId)
    {
        try
        {
            var player = _lavalink.DefaultNode.GetPlayer(guildId);
            if (!player.IsPaused)
                await player.PauseAsync();
            return $"**Resumed:** {player.CurrentTrack.Title}";
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }

    public async Task OnFinshed(LavaPlayer player, LavaTrack track, TrackReason reason)
    {
        if (reason is TrackReason.LoadFailed || reason is TrackReason.Cleanup)
            return;
        player.Queue.TryDequeue(out LavaTrack nextTrack);

        if (nextTrack is null)
        {
            await LoggingService.LogInformationAsync("Music", "Bot has stopped playback.");
            await player.StopAsync();
        }
        else
        {
            await player.PlayAsync(nextTrack);
            await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {nextTrack.Title} - {nextTrack.Uri}");
            await player.TextChannel.SendMessageAsync("", false, await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{nextTrack.Title}]({nextTrack.Uri})", Color.Blue));
        }
    }
    public async Task<Embed> DisplayStatsAsync()
    {
        var node = _lavalink.DefaultNode.Stats;
        var embed = await Task.Run(() => new EmbedBuilder()
            .WithTitle("Lavalink Stats")
            .WithCurrentTimestamp()
            .WithColor(Color.DarkMagenta)
            .AddField("Uptime", node.Uptime, true));
        return embed.Build();
    }
    /*
    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            return;
        }
        if (target.Guild.Id != guild.Id)
        {
            return;
        }

        var audioClient = await target.ConnectAsync();

        if (ConnectedChannels.TryAdd(guild.Id, audioClient))
        {
            // If you add a method to log happenings from this service,
            // you can uncomment these commented lines to make use of that.
            // await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
        }
    }
    public async Task LeaveAudio(IGuild guild)
    {
        IAudioClient client;
        if (ConnectedChannels.TryRemove(guild.Id, out client))
        {
            await client.StopAsync();
            //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            return;
        }
    }

    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
    {
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
            using (var output = CreateStream(path).StandardOutput.BaseStream)
            using (var stream = client.CreatePCMStream(AudioApplication.Music))
            {
                try { await output.CopyToAsync(stream); }
                finally { await stream.FlushAsync(); }
            }
        }
    }
    public async Task SendLinkAudioAsync(IGuild guild, IMessageChannel channel, string url)
    {
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            var output = CreateLinkStream(url).StandardOutput.BaseStream;
            var stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
            await output.CopyToAsync(stream);
            await stream.FlushAsync().ConfigureAwait(false);
        }
    }
    public async Task SendLinkAsync(IGuild guild, IMessageChannel channel, string path)
    {
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            var output = CreateLinkStream(path).StandardOutput.BaseStream;
            var stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024); //, 128 * 1024 helps with low-end computers streaming.
            await output.CopyToAsync(stream);
            await stream.FlushAsync().ConfigureAwait(false);
        }
    }
    public async Task SendYTAsync(IGuild guild, IMessageChannel channel, VideoInfo info)
    {
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            var AudioStream = GetAudioStream(info);

            var Output = CreateYTStream(AudioStream.Url).StandardOutput.BaseStream;
            var Stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
            await Output.CopyToAsync(Stream);
            await Stream.FlushAsync();
        }
    }
    public async Task StopAudioAsync(IGuild guild)
    {
        await client.StopAsync();
        return;
    }
    private Process CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
    private Process CreateLinkStream(string url)
    {
        Process currentsong = new Process();

        currentsong.StartInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = $"/usr/local/bin/youtube-dl -o - {url} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        currentsong.Start();
        return currentsong;
    }
    private Process CreateYTStream(string FilePath)
    {
        return Process.Start(new ProcessStartInfo()
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -i {FilePath} -reconnect 1 -reconnect_at_eof 1 -reconnect_streamed 1 -reconnect_delay_max 2 -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
    }
    private AudioStreamInfo GetAudioStream(VideoInfo VideoInfo)
    {
        foreach (var AudioStream in VideoInfo.AudioStreams)
        {
            if (AudioStream.AudioEncoding == AudioEncoding.Opus)
            {
                return AudioStream;
            }
        }
        return null;
    }
    */
    }
}
