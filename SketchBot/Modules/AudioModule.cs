using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;
using Discord.Audio;
using System.Diagnostics;
using System.Collections.Concurrent;
using YouTubeSearch;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Discord.WebSocket;
using Sketch_Bot;
using Sketch_Bot.Custom_Preconditions;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using Victoria;
using Victoria.Player;
using Victoria.Node;

namespace Sketch_Bot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public AudioModule(LavaNode lavaNode)
            => _lavaNode = lavaNode;
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        [Command("Lyrics", RunMode = RunMode.Async)]
        public async Task ShowGeniusLyrics()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                lyrics = await player.Track.FetchLyricsFromOvhAsync();
                if (string.IsNullOrWhiteSpace(lyrics))
                {
                    await ReplyAsync($"No lyrics found for {player.Track.Title}");
                    return;
                }
            }

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
            {
                if (Range.Contains(stringBuilder.Length))
                {
                    await ReplyAsync($"```{stringBuilder}```");
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }
            }

            //await ReplyAsync($"```{stringBuilder}```");
        }
        [Command("Join")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }
        [Alias("p")]
        [Command("Play")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var voiceState = Context.User as IVoiceState;
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                }
                catch (Exception exception)
                {
                    await ReplyAsync(exception.Message);
                }
            }
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var searchResponse = await _lavaNode.SearchAsync(Victoria.Responses.Search.SearchType.YouTube, searchQuery);//SearchYouTubeAsync(searchQuery);
            
            if (searchResponse.Status == Victoria.Responses.Search.SearchStatus.LoadFailed || searchResponse.Status == Victoria.Responses.Search.SearchStatus.NoMatches)
            {
                Console.WriteLine(searchResponse.Status);
                await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
                return;
            }
            var thePlayer = _lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player);
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    var duration = player.Track.Duration + player.Vueue.Select(x => x.Duration).TotalTime();
                    foreach (var track in searchResponse.Tracks)
                    {
                        player.Vueue.Enqueue(track);
                    }
                    var embed = new EmbedBuilder
                    {
                        Title = "Enqueued",
                        Description = $"{searchResponse.Tracks.Count} tracks. {searchResponse.Tracks.Select(x => x.Duration).TotalTime()} {Context.User.Mention}\n" +
                        $"Estimated time until playing `{duration}`",
                        Color = new Color(0, 0, 255)
                    }.Build();
                    await ReplyAsync("", false, embed);
                }
                else
                {
                    var track = searchResponse.Tracks.First();
                    var duration = player.Track.Duration + player.Vueue.Select(x => x.Duration).TotalTime();
                    player.Vueue.Enqueue(track);
                    var embed = new EmbedBuilder
                    {
                        Title = "Enqueued",
                        Description = $"[{track.Title}]({track.Url}) {track.Duration} {Context.User.Mention}\n" +
                        $"Estimated time until playing `{duration}`",
                        Color = new Color(0, 0, 255)
                    }.Build();
                    await ReplyAsync("", false, embed);
                }
            }
            else
            {
                var track = searchResponse.Tracks.First();
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    var duration = player.Track.Duration + player.Vueue.Select(x => x.Duration).TotalTime();
                    for (var i = 0; i < searchResponse.Tracks.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);
                            var embedd = new EmbedBuilder
                            {
                                Title = "Now Playing",
                                Description = $"[{track.Title}]({track.Url}) {track.Duration} {Context.User.Mention}",
                                Color = new Color(0,0,255)
                            }.Build();
                            await ReplyAsync("",false, embedd);
                        }
                        else
                        {
                            player.Vueue.Enqueue(searchResponse.Tracks.ElementAt(i));
                        }
                    }
                    var embed = new EmbedBuilder
                    {
                        Title = "Enqueued",
                        Description = $"{searchResponse.Tracks.Count} tracks. {searchResponse.Tracks.Select(x => x.Duration).TotalTime()} {Context.User.Mention}" +
                        $"Estimated time until playing `{duration}",
                        Color = new Color(0, 0, 255)
                    }.Build();
                    await ReplyAsync("", false, embed);
                }
                else
                {
                    await player.PlayAsync(track);
                    var embed = new EmbedBuilder
                    {
                        Title = "Now Playing",
                        Description = $"[{track.Title}]({track.Url}) {track.Duration} {Context.User.Mention}",
                        Color = new Color(0, 0, 255)
                    }.Build();
                    await ReplyAsync("", false, embed);
                }
            }

        }
        [Command("Remove")]
        public async Task RemoveSong(int index)
        {
            if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                if (player.Vueue.Count > 0)
                {
                    if (index <= player.Vueue.Count)
                    {
                        await ReplyAsync($"Removed {player.Vueue.ElementAt(index - 1).Title} from the queue");
                        player.Vueue.RemoveAt(index - 1);
                    }
                    else
                    {
                        await ReplyAsync($"No song at index `{index}`");
                    }
                }
                else
                {
                    await ReplyAsync("Queue is empty");
                }
            }
            else
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
        }
        [Command("Stop")]
        public async Task StopAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                var thePlayer = _lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player);
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    await player.StopAsync();
                    player.Vueue.Clear();
                    await ReplyAsync("The playback has been stopped and the queue has been cleared");
                }
                else
                {
                    await ReplyAsync("I am not playing anything");
                }
            }
            else
            {
                await ReplyAsync("I am not playing anything");
            }
        }
        [Command("Leave")]
        public async Task LeaveAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (_lavaNode.HasPlayer(Context.Guild))
            {
                var thePlayer = _lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player);
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    player.Vueue.Clear();
                    await player.StopAsync();
                }

            }
            await ReplyAsync($"Left {_lavaNode.Players.First(x => x.VoiceChannel.GuildId == Context.Guild.Id).VoiceChannel.Name}");
            await _lavaNode.LeaveAsync(_lavaNode.Players.First(x => x.VoiceChannel.GuildId == Context.Guild.Id).VoiceChannel);
        }
        [Command("Skip")]
        public async Task SkipAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                if (_lavaNode.Players.First(x => x.VoiceChannel.GuildId == Context.Guild.Id).Vueue.Count > 0)
                {
                    var thePlayer = _lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player);
                    if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                    {
                        await ReplyAsync($"Skipped {player.Track.Title}");
                        await player.SkipAsync();
                    }
                    else
                    {
                        await ReplyAsync("I am not playing anything");
                    }
                }
                else
                {
                    await ReplyAsync("Queue is empty");
                }
            }
        }
        [Command("seek")]
        public async Task SeekAsync(TimeSpan span)
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (_lavaNode.HasPlayer(Context.Guild))
            {
                var thePlayer = _lavaNode.TryGetPlayer(Context.Guild, out LavaPlayer<LavaTrack> player);
                await player.SeekAsync(span);
                await ReplyAsync($"Seeking to {span:hh\\:mm\\:ss}");
            }
            else
            {
                await ReplyAsync("I am not playing anything");
            }

        }
        [Alias("songs","q")]
        [Command("Queue")]
        public async Task QueueAsync()
        {
            if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                if (player.Vueue.Count > 0 || player.Track != null)
                {
                    var queue = player.Vueue.Cast<LavaTrack>();
                    string stringqueue = $"0. [{player.Track.Title}]({player.Track.Url}) {player.Track.Duration}\n";
                    int i = 1;
                    if (player.Vueue.Count > 0)
                    {
                        foreach (var track in queue)
                        {
                            stringqueue += $"\n{i}. [{track.Title}]({track.Url}) {track.Duration}";
                            i++;
                        }
                    }
                    var duration = player.Track.Duration + queue.Select(x => x.Duration).TotalTime();
                    EmbedBuilder builder = new EmbedBuilder
                    {
                        Title = $"Queue for {Context.Guild.Name}",
                        Color = new Color(0, 0, 255),
                        Description = stringqueue,
                        Timestamp = DateTime.Now,
                    }.WithFooter(footer =>
                    {
                        footer.Text = $"Duration: {duration}";
                    });
                    var embed = builder.Build();
                    await ReplyAsync("", false, embed);
                }
                else
                {
                    await ReplyAsync("I am not playing anything");
                }
            }
            else
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
        }
        [Command("Pause")]
        public async Task PauseAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                if(player.PlayerState == PlayerState.Playing)
                {
                    await player.PauseAsync();
                    await ReplyAsync("Paused playback :pause_button:");
                }
                else if(player.PlayerState == PlayerState.Paused)
                {
                    await ReplyAsync("I am already paused");
                }
            }
            else
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
        }
        [Command("Resume")]
        public async Task ResumeAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                if (player.PlayerState == PlayerState.Paused)
                {
                    await player.ResumeAsync();
                    await ReplyAsync("Resumed playback :arrow_forward:");
                }
                else if (player.PlayerState == PlayerState.Playing)
                {
                    await ReplyAsync("I am already playing");
                }
            }
            else
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
        }
        [Priority(1)]
        [Command("Volume")]
        public async Task VolumeAsync(ushort volume)
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            if (volume > 0 && volume <= 150)
            {
                if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
                {
                    await player.SetVolumeAsync(volume);
                    await ReplyAsync($":thumbsup:Volume has been set to {volume}");
                }
                else
                {
                    await ReplyAsync("I'm not connected to a voice channel.");
                    return;
                }
            }
            else
            {
                await ReplyAsync("Volume must be between 1 and 150!");
            }
        }
        [Priority(0)]
        [Command("Volume")]
        public async Task VolumeAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync($"Volume is currently set to `{player.Volume}`");
            }
            else
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
        }
        [Alias("np", "NowPlaying", "song")]
        [Command("Playing")]
        public async Task NowPlayingAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                bool overAnHour = player.Track.Duration > TimeSpan.FromHours(1);
                string position = player.Track.Position.ToString(@"mm\:ss");
                if (overAnHour)
                {
                    position = player.Track.Position.ToString(@"hh\:mm\:ss");
                }
                EmbedBuilder builder = new EmbedBuilder
                {
                    Title = player.Track.Title,
                    Description = $"{position}/{(overAnHour ? player.Track.Duration.ToString(@"hh\:mm\:ss") : player.Track.Duration.ToString(@"mm\:ss"))}",
                    Color = Color.DarkBlue,
                    Timestamp = DateTime.Now
                };
                var embed = builder.Build();
                await ReplyAsync("", false, embed);
            }
            else
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
        }
    }

















        /*
        // Scroll down further for the AudioService.
        // Like, way down
        private readonly AudioService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public AudioModule(AudioService service)
        {
            _service = service;
        }

        /* Get our AudioService from DI */
        //public AudioService AudioService { get; set; }

        /* All the below commands are ran via Lambda Expressions to keep this file as neat and closed off as possible. 
              We pass the AudioService Task into the section that would normally require an Embed as that's what all the
              AudioService Tasks are returning. */
        /*
                [Command("Join")]
                public async Task JoinAndPlay()
                    => await ReplyAsync("", false,
                        await AudioService.JoinOrPlayAsync((SocketGuildUser) Context.User, Context.Channel, Context.Guild.Id));

                [Command("Leave")]
                public async Task Leave()
                {
                    if (((SocketGuildUser) Context.User).VoiceChannel.Id == Context.Guild.CurrentUser.VoiceChannel.Id)
                    {
                        await ReplyAsync("", false, await AudioService.LeaveAsync(Context.Guild.Id));
                    }
                    else
                    {
                        var embed = await EmbedHandler.CreateErrorEmbed("Music, Leave", "You need to be in the same voice channel as me!");
                        await ReplyAsync("", false, embed);
                    }
                }


                [Command("Play")]
                public async Task Play([Remainder] string search)
                    => await ReplyAsync("", false,
                        await AudioService.JoinOrPlayAsync((SocketGuildUser) Context.User, Context.Channel, Context.Guild.Id,
                            search));

                [Command("Stop")]
                public async Task Stop()
                    => await ReplyAsync("", false, await AudioService.StopAsync(Context.Guild.Id));

                [Command("Queue")]
                public async Task List()
                    => await ReplyAsync("", false, await AudioService.ListAsync(Context.Guild.Id));

                [Command("Skip")]
                public async Task Delist(string id = null)
                    => await ReplyAsync("", false, await AudioService.SkipTrackAsync(Context.Guild.Id));

                [Command("Volume")]
                public async Task Volume(int volume)
                    => await ReplyAsync(await AudioService.VolumeAsync(Context.Guild.Id, volume));

                [Command("Pause")]
                public async Task Pause()
                    => await ReplyAsync(await AudioService.Pause(Context.Guild.Id));

                [Command("Resume")]
                public async Task Resume()
                    => await ReplyAsync(await AudioService.Pause(Context.Guild.Id));

            }*/
    }














/*
// You *MUST* mark these commands with 'RunMode.Async'
// otherwise the bot will not respond until the Task times out.
[Command("join", RunMode = RunMode.Async)]
public async Task JoinCmd()
{
    IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
    if (channel != null)
    {
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        await Context.Channel.SendMessageAsync("Joined " + (Context.User as IVoiceState).VoiceChannel.Name);
    }
    else
    {
        await Context.Channel.SendMessageAsync("You need to be in a voice channel first!");
    }
}
// Remember to add preconditions to your commands,
// this is merely the minimal amount necessary.
// Adding more commands of your own is also encouraged.
[Command("leave", RunMode = RunMode.Async)]
public async Task LeaveCmd()
{
    await _service.LeaveAudio(Context.Guild);
    await Context.Channel.SendMessageAsync("Left " + (Context.User as IVoiceState).VoiceChannel.Name);
}
[Command("playtest", RunMode = RunMode.Async)]
public async Task play(params string[] keywords)
{
    try
    {
        IVoiceChannel channel = (Context.User as IVoiceState).VoiceChannel;
        if (channel != null)
        {
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            string searchquery = String.Join(" ", keywords);
            await Context.Channel.SendMessageAsync($"<:youtube:396008245359149057> **Searching** :mag_right: `{searchquery}`");
            var items = new VideoSearch();
            var item = items.SearchQuery(searchquery, 1);
            string url = item.First().Url;
            string rawtitle = item.First().Title;
            var thingy = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            byte[] bytes = thingy.GetBytes(rawtitle);
            var title = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?play with success! (" + searchquery + ") (" + rawtitle + ") (" + Context.Guild.Name + ")");

            if (!queue2.ContainsKey(Context.Guild.Id))
            {
                queue2.Add(Context.Guild.Id, queue);
                queue2[Context.Guild.Id].Add(url);
            }
            else
            {
                queue2[Context.Guild.Id].Add(url);
            }

            if (queue2[Context.Guild.Id].Count == 1)
            {
                await Context.Channel.SendMessageAsync($"**Playing** :notes: `{rawtitle}` - Now! " + item.First().Duration);
            }
            else
            {
                await ReplyAsync($"Added `{rawtitle}` to the queue");
            }

            if (queue2[Context.Guild.Id].Count == 1)
            {
                foreach (var song in queue2[Context.Guild.Id])
                {
                    await _service.SendLinkAudioAsync(Context.Guild, Context.Channel, song);
                    queue2[Context.Guild.Id].RemoveAt(0);
                }
            }
        }
        else
        {
            await Context.Channel.SendMessageAsync("You need to be in a voice channel first!");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?play and failed! (NotInVoiceChannelException)" + " (" + Context.Guild.Name + ")");
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just cancelled ?play! (" + ex.GetType().ToString() + ") (" + Context.Guild.Name + ")");
        Console.WriteLine(ex.StackTrace);
    }
}
[Command("queue", RunMode = RunMode.Async)]
public async Task queuee()
{
    EmbedBuilder builder = new EmbedBuilder()
    {
        Title = $"Queue for {Context.Guild.Name}",
        Color = new Color(0, 0, 255),
        Description = string.Join("\n", queue2[Context.Guild.Id])
    };
    var embed = builder.Build();
    await ReplyAsync("", false, embed);
}
/*public async Task handlequeueAsync()
{
    repeat:
    {
        if (currentlyplaying == 0)
        {
            currentlyplaying = 1;

            queue.RemoveAt(0);
            currentlyplaying = 0;
        }
        else
        {
            goto repeat;
        }
        goto repeat;
    }
}*/
/*
[Command("stop", RunMode = RunMode.Async)]
public async Task stopcmd()
{
    await _service.StopAudioAsync(Context.Guild);
}
[Command("queue", RunMode = RunMode.Async)]
public async Task queuelist()
{
    string queuelist = String.Join("`\n`", queue.ToArray());
    await Context.Channel.SendMessageAsync("Queue for " + Context.Guild.Name + "\n" + queuelist);
}
private Process CreateLinkStream(string url)
{
    Process currentsong = new Process();

    currentsong.StartInfo = new ProcessStartInfo
    {
        FileName = "bash",
        Arguments = $"youtube-dl -o - {url} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        CreateNoWindow = true
    };

    currentsong.Start();
    return currentsong;
}
        [Command("gulag", RunMode = RunMode.Async)]
    public async Task gulag(IGuildUser user = null)
    {
        if (Context.Guild.Id == 377403749754339329)
        {
            if (user != null)
            {
                var currentVoiceChannel = (user as IGuildUser).VoiceChannel;
                if (currentVoiceChannel != null)
                {
                    await Context.Channel.SendMessageAsync("To the Gulag with you!" +
                        "\nhttps://cdn.discordapp.com/attachments/415082173759225856/431436282930135040/tenor.gif");
                    await (user as IGuildUser).ModifyAsync(x => x.ChannelId = 378630009629310976);
                    await Task.Delay(200);
                    await _service.JoinAudio(Context.Guild, (Context.User as IGuildUser).VoiceChannel);
                    await _service.SendLinkAudioAsync(Context.Guild, Context.Channel, "https://www.youtube.com/watch?v=U06jlgpMtQs");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?gulag with success!" + " (" + Context.Guild.Name + ")");
                }
                else
                {
                    await Context.Channel.SendMessageAsync((user as IGuildUser).Username + " is not in a voice channel!");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?gulag and failed! (NotInVoiceChannelException)" + " (" + Context.Guild.Name + ")");
                }
            }
            else
            {
                user = (Context.User as IGuildUser);
                var currentVoiceChannel = (user as IGuildUser).VoiceChannel;
                if (currentVoiceChannel != null)
                {
                    await Context.Channel.SendMessageAsync("To the Gulag with you!");
                    await (user as IGuildUser).ModifyAsync(x => x.ChannelId = 378630009629310976);
                    await Task.Delay(200);
                    await _service.JoinAudio(Context.Guild, (Context.User as IGuildUser).VoiceChannel);
                    await _service.SendLinkAudioAsync(Context.Guild, Context.Channel, "https://www.youtube.com/watch?v=U06jlgpMtQs");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?gulag with success!" + " (" + Context.Guild.Name + ")");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("You are not in a voice channel!");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?gulag and failed! (NotInVoiceChannelException)" + " (" + Context.Guild.Name + ")");
                }
            }
        }
        else
        {
            await Context.Channel.SendMessageAsync("Wrong server buddy!");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?gulag and failed! (WrongServerException)" + " (" + Context.Guild.Name + ")");
        }
    }
[Command("rickroll", RunMode = RunMode.Async)]
public async Task rickroll(IGuildUser user = null)
{
    try
    {
        rand = new Random();
        rickrolls = new string[]
        {
            "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            "https://www.youtube.com/watch?v=lXMskKTw3Bc",
            "https://www.youtube.com/watch?v=gzSxBoxxzVM"
        };
        if (user != null)
        {
            var currentVoiceChannel = (user as IGuildUser).VoiceChannel;
            if (currentVoiceChannel != null)
            {
                await Context.Channel.SendMessageAsync("Rickrolling...");
                await _service.JoinAudio(Context.Guild, (user as IGuildUser).VoiceChannel);
                int randomFileIndex = rand.Next(rickrolls.Length);
                int pictureNumber = randomFileIndex + 1;
                string fileToPost = rickrolls[randomFileIndex];
                await _service.SendLinkAudioAsync(Context.Guild, Context.Channel, fileToPost);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rickroll with success!" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                await Context.Channel.SendMessageAsync("your target must be in a voice channel before i can rickroll");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rickroll and failed! (NotInVoiceChannelException)" + " (" + Context.Guild.Name + ")");
            }
        }
        else
        {
            user = (Context.User as IGuildUser);
            var currentVoiceChannel = (user as IGuildUser).VoiceChannel;
            if (currentVoiceChannel != null)
            {
                await Context.Channel.SendMessageAsync("Rickrolling...");
                await _service.JoinAudio(Context.Guild, (Context.User as IGuildUser).VoiceChannel);
                int randomFileIndex = rand.Next(rickrolls.Length);
                int pictureNumber = randomFileIndex + 1;
                string fileToPost = rickrolls[randomFileIndex];
                await _service.SendLinkAudioAsync(Context.Guild, Context.Channel, fileToPost);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rickroll with success!" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                await Context.Channel.SendMessageAsync("You must be in a voice channel before i can rickroll!");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rickroll and failed! (NotInVoiceChannelException)" + " (" + Context.Guild.Name + ")");
            }
        }
    }
    catch (Exception)
    {
        await Context.Channel.SendMessageAsync("Rickroll canceled!");
        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just cancelled rickroll!" + " (" + Context.Guild.Name + ")");
    }
}*/
