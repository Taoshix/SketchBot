using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using SketchBot.Custom_Preconditions;
using SketchBot.Services;
using SketchBot.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TagLib.Matroska;
using Victoria;
using Victoria.Enums;
using Victoria.Rest;
using Victoria.Rest.Search;
using YouTubeSearch;

namespace SketchBot.InteractionBasedModules
{
    [RequireContext(ContextType.Guild)]
    public class AudioModule(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode, AudioService audioService) : ModuleBase<SocketCommandContext>
    {
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        [Command("Join")]
        public async Task JoinAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await lavaNode.JoinAsync(voiceState.VoiceChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");

                audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.ToString());
            }
        }

        [Command("Leave")]
        public async Task LeaveAsync()
        {
            var voiceChannel = (Context.User as IVoiceState).VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync("Not sure which voice channel to disconnect from.");
                return;
            }

            try
            {
                await lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync($"I've left {voiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Play")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            // remove whitespace after first : if there is one
            // "ytsearch: query" -> "ytsearch:query"
            int colonIndex = searchQuery.IndexOf(':');
            if (colonIndex != -1 && colonIndex + 1 < searchQuery.Length && searchQuery[colonIndex + 1] == ' ')
            {
                searchQuery = searchQuery.Remove(colonIndex + 1, 1);
            }

            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null)
            {
                var voiceState = Context.User as IVoiceState;
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }

                try
                {
                    player = await lavaNode.JoinAsync(voiceState.VoiceChannel);
                    await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
                    audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
                }
                catch (Exception exception)
                {
                    await ReplyAsync(exception.Message);
                }
            }
            // Auto disconnect does not properly destroy the player and making it null so we need to use the existing player to reconnect
            if (!player.State.IsConnected && Context.User as IVoiceState != null)
            {
                player = await lavaNode.JoinAsync((Context.User as IVoiceState).VoiceChannel);
            }
            var searchResponse = await lavaNode.LoadTrackAsync(searchQuery);
            if (searchResponse.Type is SearchType.Empty or SearchType.Error)
            {
                await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`. Please check if the query/link is correct.\n" +
                    $"You can use a direct link or make a search query like demonstrated below:\n" +
                    $"Here is a list of prefixes for searching:\n" +
                    $"`ytsearch:` for YouTube\n" +
                    $"`ytmsearch:` for YouTubeMusic\n" +
                    $"`scsearch:` for SoundCloud\n" +
                    $"Example `ytsearch: Guitar, Loneliness and Blue Planet`");
                return;
            }

            var track = searchResponse.Tracks.FirstOrDefault();
            if (player.GetQueue().Count == 0 && player.Track == null)
            {
                await player.PlayAsync(lavaNode, track, false);
                return;
            }

            player.GetQueue().Enqueue(track);
            await ReplyAsync($"Added [{track.Title}]({track.Url}) to queue.");
        }

        [Command("Queue")]
        public async Task ShowQueueAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var queue = player.GetQueue();
            var queueList = new StringBuilder();
            TimeSpan totalDuration = TimeSpan.Zero;

            if (player.Track != null)
            {
                queueList.AppendLine($"`0.` [{player.Track.Title}]({player.Track.Url})");
                queueList.AppendLine("----------------------------------");
                totalDuration += player.Track.Duration - player.Track.Position;
            }

            if (queue.Any())
            {
                foreach (var (track, index) in queue.Select((track, index) => (track, index)))
                {
                    totalDuration += track.Duration;
                    queueList.AppendLine($"`{index + 1}.` [{track.Title}]({track.Url})");
                }
            }

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.Name = $"Queue for {Context.Guild.Name}";
                    author.IconUrl = Context.Guild.IconUrl;
                })
                .WithTitle($"{queue.Count} track(s) in queue - {totalDuration:hh\\:mm\\:ss}")
                .WithDescription(queueList.Length == 0 ? "The queue is empty." : queueList.ToString())
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("NowPlaying"), Alias("NP")]
        public async Task NowPlayingAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (player.Track == null)
            {
                await ReplyAsync("I'm not currently playing anything.");
                return;
            }
            var track = player.Track;

            string durationFormat = track.Duration.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss";
            string positionFormat = track.Position.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss";
            string positionStr = track.Position.ToString(positionFormat);
            string durationStr = track.Duration.ToString(durationFormat);

            var embed = new EmbedBuilder()
                .WithTitle("Now Playing")
                .WithDescription(track.Title)
                .WithUrl(track.Url)
                .WithThumbnailUrl(track.Artwork)
                .AddField("Progress", $"{positionStr} / {durationStr} ({(track.Position / track.Duration * 100).ToString("0.00")}%)")
                .AddField("Author", track.Author)
                .WithColor(Color.Blue)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("Pause"), RequirePlayer]
        public async Task PauseAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player.IsPaused && player.Track != null)
            {
                await ReplyAsync("I cannot pause when I'm not playing anything!");
                return;
            }

            try
            {
                await player.PauseAsync(lavaNode);
                await ReplyAsync($"Paused: {player.Track.Title}");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Resume"), RequirePlayer]
        public async Task ResumeAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (!player.IsPaused && player.Track != null)
            {
                await ReplyAsync("I cannot resume when I'm not playing anything!");
                return;
            }

            try
            {
                await player.ResumeAsync(lavaNode, player.Track);
                await ReplyAsync($"Resumed: {player.Track.Title}");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Stop"), RequirePlayer]
        public async Task StopAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (!player.State.IsConnected || player.Track == null)
            {
                await ReplyAsync("Woah, can't stop won't stop.");
                return;
            }

            try
            {
                player.GetQueue().Clear();
                await player.SeekAsync(lavaNode, player.Track.Duration); // Workaround: Seek to the end of the track to clear it
                //await player.StopAsync(lavaNode, player.Track);
                await ReplyAsync("No longer playing anything and queue has been cleared.");
                var voiceChannel = (Context.User as IVoiceState).VoiceChannel;
                if (voiceChannel != null)
                {
                    await lavaNode.LeaveAsync(voiceChannel);
                }
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Skip"), RequirePlayer]
        public async Task SkipAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (!player.State.IsConnected)
            {
                await ReplyAsync("Woaaah there, I can't skip when nothing is playing.");
                return;
            }

            var voiceChannelUsers = Context.Guild.CurrentUser.VoiceChannel
                .Users
                .Where(x => !x.IsBot)
                .ToArray();
            /*
            if (!audioService.VoteQueue.Add(Context.User.Id))
            {
                await ReplyAsync("You can't vote again.");
                return;
            }

            float percentage = (float)audioService.VoteQueue.Count / voiceChannelUsers.Length;
            if (percentage < 0.85)
            {
                await ReplyAsync($"You need more than 85% votes to skip this song. ({percentage * 100}%)");
                return;
            }
            */
            try
            {
                // workaround for Victoria's skip method not skipping properly
                var queue = player.GetQueue();
                if (queue.Count > 0)
                {
                    var skipped = player.Track;
                    var nextTrack = queue.FirstOrDefault();
                    queue.RemoveAt(0);
                    await ReplyAsync($"Skipped: {skipped.Title}");
                    await player.PlayAsync(lavaNode, nextTrack, false);
                }
                else
                {
                    await ReplyAsync("No more tracks in the queue to skip to.");
                }

                //var (skipped, currenTrack) = await player.SkipAsync(lavaNode);
                //await ReplyAsync($"Skipped: {skipped.Title}\nNow Playing: {currenTrack.Title}");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }
        [Command("Volume"), RequirePlayer]
        public async Task VolumeAsync(int volume)
        {
            if (volume < 0 || volume > 100)
            {
                await ReplyAsync("Volume must be between 0 and 100.");
                return;
            }
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            try
            {
                await player.SetVolumeAsync(lavaNode, volume);
                await ReplyAsync($"Volume set to {volume}%.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }
        [Command("Seek"), RequirePlayer]
        public async Task SeekAsync([Remainder] string time)
        {
            if (string.IsNullOrWhiteSpace(time))
            {
                await ReplyAsync("Please provide a time to seek to (e.g., 00:01:30 or 5:30).\nFormat: [hh:]mm:ss");
                return;
            }
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (player.Track == null)
            {
                await ReplyAsync("I'm not currently playing anything.");
                return;
            }
            TimeSpan seekTime = TimeSpan.Zero;
            string[] formats = { "hh\\:mm\\:ss", "m\\:ss", "mm\\:ss" };
            bool parsed = false;
            foreach (var fmt in formats)
            {
                if (TimeSpan.TryParseExact(time, fmt, CultureInfo.InvariantCulture, out seekTime))
                {
                    parsed = true;
                    break;
                }
            }
            if (!parsed)
            {
                await ReplyAsync("Invalid time format. Please use hh:mm:ss or mm:ss (e.g., 5:30 for 5 minutes 30 seconds).");
                return;
            }
            if (seekTime < TimeSpan.Zero || seekTime > player.Track.Duration)
            {
                await ReplyAsync($"Seek time must be between 0 and {player.Track.Duration}.");
                return;
            }
            try
            {
                await player.SeekAsync(lavaNode, seekTime);
                await ReplyAsync($"Seeked to {seekTime} in {player.Track.Title}.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }
        [Command("playerstate")]
        public async Task PlayerStateAsync(ulong guildId = 0)
        {
            if (guildId == 0 || Context.User.Id != 135446225565515776)
            {
                guildId = Context.Guild.Id;
            }
            var player = await lavaNode.TryGetPlayerAsync(guildId);
            var voiceChannel = Context.Client.GetGuild(guildId)?.VoiceChannels.FirstOrDefault(x => x.ConnectedUsers.Select(x => x.Id).Contains(Context.Client.CurrentUser.Id));
            if (player == null || voiceChannel == null)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            var queue = player.GetQueue();
            TimeSpan totalDuration = TimeSpan.Zero;
            if (player.Track != null)
            {
                totalDuration += player.Track.Duration - player.Track.Position;
            }
            if (queue.Any())
            {
                foreach (var track in queue)
                {
                    totalDuration += track.Duration;
                }
            }
            string durationFormat = player.Track.Duration.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss";
            string positionFormat = player.Track.Position.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss";
            string positionStr = player.Track.Position.ToString(positionFormat);
            string durationStr = player.Track.Duration.ToString(durationFormat);
            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.Name = $"Player State for {Context.Client.GetGuild(guildId)?.Name ?? "Unknown Guild"}";
                    author.IconUrl = Context.Client.GetGuild(guildId)?.IconUrl;
                })
                .AddField("Is Connected", player.State.IsConnected, true)
                .AddField("Ping", player.State.Ping, true)
                .AddField("Is Paused", player.IsPaused, true)
                .AddField("Is Playing", player.Track != null, true)
                .AddField($"Queue ({queue.Count} - {totalDuration:hh\\:mm\\:ss})", queue.Count > 0 ? HelperFunctions.JoinWithLimit(queue.Select(x => $"[{x.Title}]({x.Url})"), 1024, "\n") : "Empty")
                .AddField("Current Track", $"[{player.Track?.Title}]({player.Track?.Url})" ?? "No track playing", true)
                .AddField("Player Volume", player.Volume, true)
                .AddField("Track Duration", player.Track != null ? $"{positionStr} / {durationStr} ({(player.Track.Position / player.Track.Duration * 100).ToString("0.00")}%)" : "N/A")
                .AddField($"VC - {(voiceChannel != null ? $"{voiceChannel.Name} ({voiceChannel.ConnectedUsers.Count})" : "Not connected")}", voiceChannel != null ? HelperFunctions.JoinWithLimit(voiceChannel.ConnectedUsers.Select(x => $"{x.DisplayName} ({x.Username} - {x.Id})"), 1024, "\n") : "N/A")
                .WithFooter(footer => footer.Text = $"Guild ID: {guildId}")
                .WithCurrentTimestamp()
                .WithColor(Color.Blue)
                .Build();
            await ReplyAsync(embed: embed);
        }
        [RequireDevelopers]
        [Command("lavaplayers")]
        public async Task LavaPlayersAsync()
        {
            var players = await lavaNode.GetPlayersAsync();
            if (!players.Any())
            {
                await ReplyAsync("No active players.");
                return;
            }
            var embed = new EmbedBuilder()
                .WithTitle($"Active Lavalink Players ({players.Count})")
                .WithDescription(HelperFunctions.JoinWithLimit(players.Select(x => $"**{Context.Client.GetGuild(x.GuildId).Name}** - `{x.GuildId}` - {(x.State.IsConnected ? x.Track != null ? $"Playing: [{x.Track.Title}]({x.Track.Url})" : "Connected, not playing" : "Not connected")}"), 4096, "\n"))
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .Build();
            await ReplyAsync(embed: embed);
        }
        [Command("lavastats")]
        public async Task PlayerStatsAsync()
        {
            var stats = await lavaNode.GetLavalinkStatsAsync();

            var embed = new EmbedBuilder()
                .WithTitle("Lavalink Stats")
                .AddField("Players", stats.Players, true)
                .AddField("Playing Players", stats.PlayingPlayers, true)
                .AddField("Uptime", HelperFunctions.FormatTimeSpan(TimeSpan.FromMilliseconds(stats.Uptime)))
                .AddField("Memory Usage", $"{stats.Memory.Used / 1024 / 1024} MB", true)
                .AddField("CPU Usage", $"{stats.Cpu.LavalinkLoad}%", true)
                .WithColor(Color.Blue)
                .Build();
            await ReplyAsync(embed: embed);
        }
        [Command("ClearQueue"), RequirePlayer]
        public async Task ClearQueueAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            player.GetQueue().Clear();
            await ReplyAsync("Cleared the queue.");
        }
        [Command("ShuffleQueue"), RequirePlayer]
        public async Task ShuffleQueueAsync()
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            var queue = player.GetQueue();
            if (queue.Count == 0)
            {
                await ReplyAsync("The queue is empty, nothing to shuffle.");
                return;
            }
            queue.Shuffle();
            await ReplyAsync("Shuffled the queue.");
        }
        [Command("Remove"), RequirePlayer]
        public async Task RemoveAtAsync(int index)
        {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected)
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (index == 0)
            {
                await ReplyAsync("You cannot remove the currently playing track using this command. Use `skip` command to skip current track.");
                return;
            }
            var queue = player.GetQueue();
            index--; // Convert to 0-based index since queue is 0-based but user input is 1-based from the queue command
            if (!queue.Any())
            {
                await ReplyAsync("The queue is empty, nothing to remove.");
                return;
            }
            if (index < 0 || index >= queue.Count)
            {
                await ReplyAsync($"Invalid index. Please provide a number between 1 and {queue.Count - 1}.");
                return;
            }
            var removedTrack = queue.ElementAt(index);
            queue.RemoveAt(index);
            await ReplyAsync($"Removed {removedTrack.Title} from the queue at index {index+1}.");
        }
    }
}