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
using Victoria.Rest.Search;

namespace Sketch_Bot.Modules
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

            var searchResponse = await lavaNode.LoadTrackAsync(searchQuery);
            if (searchResponse.Type is SearchType.Empty or SearchType.Error)
            {
                await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
                return;
            }

            var track = searchResponse.Tracks.FirstOrDefault();
            if (player.GetQueue().Count == 0)
            {
                await player.PlayAsync(lavaNode, track);
                await ReplyAsync($"Now playing: {track.Title}");
                return;
            }

            player.GetQueue().Enqueue(track);
            await ReplyAsync($"Added {track.Title} to queue.");
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
                await player.StopAsync(lavaNode, player.Track);
                await ReplyAsync("No longer playing anything.");
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

            if (!audioService.VoteQueue.Add(Context.User.Id))
            {
                await ReplyAsync("You can't vote again.");
                return;
            }

            var percentage = audioService.VoteQueue.Count / voiceChannelUsers.Length * 100;
            if (percentage < 85)
            {
                await ReplyAsync("You need more than 85% votes to skip this song.");
                return;
            }

            try
            {
                var (skipped, currenTrack) = await player.SkipAsync(lavaNode);
                await ReplyAsync($"Skipped: {skipped.Title}\nNow Playing: {currenTrack.Title}");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
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
