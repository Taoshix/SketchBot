using Discord;
using Discord.Addons.Interactive;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.Net.Rest;
using Discord.Rest;
using Discord.WebSocket;
using JikanDotNet;
using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MoreLinq.Extensions;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using OsuSharp;
using OsuSharp.Extensions;
using Sketch_Bot.Models;
using Sketch_Bot.Modules;
using Sketch_Bot.Services;
using Sketch_Bot.TypeConverters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Victoria;
using static Sketch_Bot.Models.HelperFunctions;

namespace Sketch_Bot
{
    public class Program
    {
        Random rand;
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;

        private readonly CommandService _commands;
        private ServiceProvider _provider;
        private readonly Jikan _jikan;
        private Config _config;

        ulong _channelid;

        public bool _databaseActive;

        private Program()
        {
            _config = Config.Load();
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
            });
            _provider = new ServiceCollection()
                .AddSingleton(_client)
                .AddLogging(x => {
                    x.ClearProviders();
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Trace);
                })
                .AddLavaNode(x =>
                {
                    x.SelfDeaf = false;
                })
                .AddSingleton<AudioService>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<TimerService>()
                .AddSingleton<StatService>()
                .AddSingleton<MemeService>()
                .AddSingleton<OsuService>()
                .AddSingleton<XpService>()
                .AddSingleton<CachingService>()
                .AddSingleton<Jikan>()
                .AddSingleton<DiscordBotsListService>()
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Info,
                    CaseSensitiveCommands = false
                }))
                
                .AddSingleton(new InteractionService(_client, new InteractionServiceConfig
                {
                    DefaultRunMode = Discord.Interactions.RunMode.Async,
                    
                }))
                .BuildServiceProvider();
            
            
            //_provider.AddService(new InteractionService(_provider.GetService<DiscordSocketClient>()));

            _client = _provider.GetRequiredService<DiscordSocketClient>();
            _interactionService = _provider.GetRequiredService<InteractionService>();
            _commands = _provider.GetRequiredService<CommandService>();
            _jikan = _provider.GetRequiredService<Jikan>();
            _databaseActive = _provider.GetRequiredService<CachingService>()._dbConnected;
            _provider.GetRequiredService<StatService>().AddCache(_provider.GetRequiredService<CachingService>());
            _interactionService.AddTypeConverter<Calculation>(new CalculationConverter());
            _interactionService.AddTypeConverter<ulong>(new UlongConverter());

            _client.Log += Logger;
            _commands.Log += Logger;
            _client.Ready += OnReady;
            _client.Ready += GuildCountReady;
            _interactionService.SlashCommandExecuted += SlashCommandExecuted;
            _client.JoinedGuild += GuildCount;
            _client.LeftGuild += GuildCount;
            _client.UserJoined += Welcome;
            _client.MessageReceived += giveXP;
            _client.JoinedGuild += JoinedNewServer;
            _client.MessageReceived += MessageDelete;
            _client.MessageReceived += HandleCommandAsync;
            _client.UserUpdated += UpdateProfilePictures;
            _client.InteractionCreated += HandleInteraction;
            _client.ButtonExecuted += MyButtonHandler;
            _client.SelectMenuExecuted += MyMenuHandler;
            //_commands.CommandExecuted += OnCommandExecutedAsync;
        }

        private static Task Logger(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
        public async Task MainAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

            
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
                                           
        }
        async Task HandleInteraction(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(_client, arg);

            if (context.User.IsBot) return;

            if (_provider.GetService<CachingService>()._blacklist.Contains(context.User.Id))
            {
                Console.WriteLine($"Blacklisted user {context.User.Username} tried to use a command");
                await context.Interaction.RespondAsync("You are currently blacklisted!", ephemeral: true);
            }
            await _interactionService.ExecuteCommandAsync(context, _provider);
            
        }
        async Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)} {arg2.User.Username} ran /{arg1.Name}");
            if (!arg3.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)} {arg2.User.Username} Failed to run /{arg1.Name} due to {arg3.ErrorReason}");
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await arg2.Interaction.RespondAsync($"Unmet Precondition: {arg3.ErrorReason}");
                        break;
                    case InteractionCommandError.UnknownCommand:
                        await arg2.Interaction.RespondAsync("Unknown command");
                        break;
                    case InteractionCommandError.BadArgs:
                        await arg2.Interaction.RespondAsync("Invalid number or arguments");
                        break;
                    case InteractionCommandError.Exception:
                        await arg2.Interaction.RespondAsync($"Command exception:{arg3.ErrorReason}");
                        break;
                    case InteractionCommandError.Unsuccessful:
                        await arg2.Interaction.RespondAsync("Command could not be executed");
                        break;
                    default:
                        break;
                }
            }
        }
        private Task HandleCommandAsync(SocketMessage arg)
        {
            _ = Task.Run(async () =>
            {
                var msg = arg as SocketUserMessage;
                if (msg == null) return;
                if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;
                int pos = 0;
                var prefix = _config.Prefix;
                var auther = msg.Author;
                var socketguild = (auther as SocketGuildUser)?.Guild;
                if (_provider.GetRequiredService<CachingService>()._dbConnected)
                {
                    var cachingService = _provider.GetRequiredService<CachingService>();
                    if (socketguild != null)
                    {
                        cachingService.SetupPrefixes(socketguild);
                        prefix = cachingService.GetPrefix(socketguild.Id);
                    }
                }
                if (msg.HasStringPrefix(prefix, ref pos) || msg.HasMentionPrefix(_client.CurrentUser, ref pos))
                {
                    // Create a Command Context.
                    var context = new SocketCommandContext(_client, msg);
                    if (_provider.GetRequiredService<CachingService>()._dbConnected)
                    {
                        var cache = _provider.GetService<CachingService>();
                        var blacklisted = cache.GetBlackList();
                        if (!blacklisted.Contains(context.User.Id))
                        {
                            var result = await _commands.ExecuteAsync(context, pos, _provider);
                        }
                    }
                    else
                    {
                        var result = await _commands.ExecuteAsync(context, pos, _provider);
                    }
                }
            });
            return Task.CompletedTask;
        }
        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            var baseId = component.Data.CustomId.Split(':').FirstOrDefault();
            var args = component.Data.CustomId.Split(':').Skip(1).ToArray();
            switch (baseId)
            {
                case "reset-user":
                    if (!_provider.GetRequiredService<CachingService>()._dbConnected)
                    {
                        await component.RespondAsync("Database is not connected!");
                        return;
                    }
                    if (component.User.Id != ulong.Parse(args.FirstOrDefault()))
                    {
                        await component.RespondAsync("You can only reset your own user data!", ephemeral: true);
                        return;
                    }
                    Database.DeleteUser(component.User as SocketGuildUser);
                    Database.EnterUser(component.User as SocketGuildUser);
                    await component.RespondAsync($"{component.User.Mention} Your user data has been reset!");
                    break;
            }
        }
        public async Task MyMenuHandler(SocketMessageComponent arg)
        {
            var text = string.Join(", ", arg.Data.Values);
            await arg.RespondAsync($"You have selected {text}");
        }
        public int TotalMembers() => _client.Guilds.Sum(x => x.MemberCount);
        private Task GuildCountReady()
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                var numberOfGuilds = _client.Guilds.Count;
                await _client.SetGameAsync(numberOfGuilds + " servers! | " + TotalMembers() + " users! | www.sketchbot.xyz");
                await DiscordBots.UpdateStats(numberOfGuilds);
                await DiscordBots.UpdateStats2(numberOfGuilds);
            });
            return Task.CompletedTask;
        }
        private async Task OnReady()
        {
            ;
            var cachingservice = _provider.GetRequiredService<CachingService>();
            await _provider.UseLavaNodeAsync();
            if (_provider.GetRequiredService<CachingService>()._dbConnected)
            {
                Database.CreateSettingsTable();
                cachingservice.SetupBlackList();
            }
            await _interactionService.RegisterCommandsGloballyAsync();
        }
        private Task GuildCount(SocketGuild socketGuild)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                var numberOfGuilds = _client.Guilds.Count;
                await _client.SetGameAsync(numberOfGuilds + " servers! | " + TotalMembers() + " users! | www.sketchbot.xyz");
                //await DiscordBots.UpdateStats(numberOfGuilds);
                //await DiscordBots.UpdateStats2(numberOfGuilds);
            });
            return Task.CompletedTask;
        }

        private async void CommandLine()
        {
            var read = Console.ReadLine();
            IList<string> split = Regex.Split(read, @"\s+");
            string str;
            try
            {
                _channelid = ulong.Parse(read.Substring(0, split[0].Length));
                str = read.Remove(0, split[0].Length + 1);
            }
            catch
            {
                str = read;
            }
            
            try
            {
                await ((ISocketMessageChannel)_client.GetChannel(_channelid)).SendMessageAsync(str);
            }
            catch
            {

            }
            CommandLine();
        }
        public async Task JoinedNewServer(SocketGuild socketGuild)
        {
            if (!_provider.GetRequiredService<CachingService>()._dbConnected)
                return;

            try
            {
                var guildId = socketGuild.Id.ToString();
                var settings = ServerSettingsDB.GetSettings(guildId);
                int levelup = socketGuild.MemberCount >= 100 ? 0 : 1;

                if (!settings.Any())
                    ServerSettingsDB.MakeSettings(guildId, levelup);

                ServerSettingsDB.CreateTableWords(guildId);
                Database.CreateTable(guildId);

                foreach (var channel in socketGuild.TextChannels)
                {
                    try
                    {
                        await channel.SendMessageAsync(
                            "Thanks for inviting me to your server!" +
                            "\nIf you want a channel for welcome messages, go to the channel and type `/setwelcome`, to remove it type `/unsetwelcome`" +
                            "\nIf you want to kick/bans logged in a channel when you use the kick/ban command, go to the channel and type `/setmodlog`, to remove it type `/unsetmodlog`" +
                            "\nIf you need help or have any questions, join my support server" +
                            "\nhttps://discord.gg/UPG8Vqb" +
                            "\nhttp://sketchbot.xyz" +
                            "\n**If you are one of these people that hates levelup messages, then you disable them with `/DisableLevelMsg`**" +
                            "\n**Levelup messages are disabled by default if your server has more than 100 members**"
                        );
                        break;
                    }
                    catch (HttpException)
                    {
                        // Try next channel
                    }
                }
            }
            catch (Exception ex)
            {
                var taoshiUser = _client.GetUser(135446225565515776);
                var embed = new EmbedBuilder()
                    .WithTitle("Unable to do setup!")
                    .WithDescription(ex.ToString())
                    .WithColor(new Color(255, 0, 0))
                    .Build();
                await taoshiUser.SendMessageAsync("", false, embed);
            }
        }
        private Task giveXP(SocketMessage msg)
        {
            _ = Task.Run(async () =>
            {
                _provider.GetService<StatService>().msgCounter++;

                var cachingService = _provider.GetRequiredService<CachingService>();
                if (!cachingService._dbConnected)
                {
                    Console.WriteLine("XP Service Error: Database is down!");
                    return;
                }

                try
                {
                    var user = msg.Author;
                    var socketGuild = (user as SocketGuildUser)?.Guild;
                    if (user.IsBot || user.IsWebhook || socketGuild == null || cachingService.GetBlackList().Contains(user.Id))
                        return;

                    cachingService.SetupUserInDatabase(socketGuild, (SocketGuildUser)user);

                    ServerSettingsDB.CreateTableRole(socketGuild.Id.ToString());

                    var guildUser = (SocketGuildUser)user;
                    var xpService = _provider.GetService<XpService>();
                    if (xpService.GetList().Contains(guildUser))
                        return;

                    rand = new Random();
                    int xp = rand.Next(5, 15);
                    int tokens = rand.Next(1, 4);
                    var userData = Database.GetUserStatus(guildUser).FirstOrDefault();
                    if (userData == null)
                    {
                        Console.WriteLine("UserData is null!");
                        return;
                    }

                    Database.ChangeTokens(guildUser, tokens);
                    xpService.addUser(guildUser);

                    var xpToLevelUp = XP.caclulateNextLevel(userData.Level);
                    if (userData.XP >= xpToLevelUp)
                    {
                        int addLevels = 0;
                        while (userData.XP >= XP.caclulateNextLevel(userData.Level + addLevels))
                            addLevels++;

                        Database.levelUp(guildUser, xp, addLevels);

                        var newLevel = userData.Level + addLevels;
                        var rolesToAward = ServerSettingsDB.GetRoles(socketGuild.Id.ToString());
                        var rolesBefore = guildUser.Roles;
                        List<SocketRole> newRoles = new();

                        if (rolesToAward.Count > 0)
                        {
                            var filteredRoles = rolesToAward.Where(x => x.roleLevel <= newLevel).OrderByDescending(o => o.roleLevel);
                            var roles = filteredRoles.Select(x => socketGuild.GetRole(ulong.Parse(x.roleId))).ToList();
                            await guildUser.AddRolesAsync(roles);
                            newRoles.AddRange(roles.Where(r => r != null));
                        }

                        bool showLevelupMsg = ServerSettingsDB.GetSettings(socketGuild.Id.ToString()).FirstOrDefault().LevelupMessages;
                        if (showLevelupMsg)
                        {
                            var embed = new EmbedBuilder().WithColor(new Color(0x4d006d));
                            if (newRoles.Count == 0)
                            {
                                embed.AddField("Leveled up!", $"{user.Mention} has leveled up to level {newLevel}!");
                            }
                            else
                            {
                                var newRoleMentions = newRoles.Where(r => !rolesBefore.Any(rb => rb.Id == r.Id)).Select(r => r.Mention);
                                embed.AddField("Leveled up!", $"{user.Mention} has leveled up to level {newLevel}!\n\nLevel up rewards:\n {string.Join("\n", newRoleMentions)}");
                            }
                            await msg.Channel.SendMessageAsync(embed: embed.Build());
                        }
                    }
                    else if (!user.IsBot)
                    {
                        Database.addXP(guildUser, xp);
                    }
                }
                catch (Exception ex)
                {
                    Database.CreateTable((msg.Author as SocketGuildUser)?.Guild.Id.ToString());
                }
            });
            return Task.CompletedTask;
        }
        public Task Welcome(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                var cachingService = _provider.GetRequiredService<CachingService>();
                if (!cachingService._dbConnected || user.IsBot)
                    return;

                // Ensure user is in database
                if (!Database.CheckExistingUser(user).Any())
                    Database.EnterUser(user);

                // Get welcome channel
                var settingsTable = ServerSettingsDB.GetSettings(user.Guild.Id.ToString());
                if (!settingsTable.Any())
                {
                    ServerSettingsDB.MakeSettings(user.Guild.Id.ToString(), user.Guild.MemberCount >= 100 ? 0 : 1);
                }
                var settings = ServerSettingsDB.GetSettings(user.Guild.Id.ToString()).FirstOrDefault();
                if (settings == null || string.IsNullOrEmpty(settings.WelcomeChannel))
                    return;

                if (!ulong.TryParse(settings.WelcomeChannel, out var channelId))
                    return;

                var channel = user.Guild.GetTextChannel(channelId);
                if (channel == null)
                    return;

                // Get invite info
                var guild = _client.GetGuild(user.Guild.Id) as IGuild;
                var invites = await guild.GetInvitesAsync();
                var invite = invites.OrderBy(x => x.CreatedAt?.UtcDateTime ?? DateTime.MinValue)
                                    .ThenBy(x => x.Uses)
                                    .FirstOrDefault();

                var inviteUrl = invite?.Url ?? "N/A";
                var inviter = invite?.Inviter?.ToString() ?? "Unknown";

                // Build and send embed
                var embed = new EmbedBuilder()
                    .WithTitle("Member Joined")
                    .WithDescription($"Welcome {user.Mention} to {user.Guild.Name}! Enjoy your stay!\nJoined with: {inviteUrl}\nReferred by: {inviter}")
                    .WithColor(new Color(0x26E4A2))
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            });
            return Task.CompletedTask;
        }
        private Task MessageDelete(SocketMessage socketMessage)
        {
            _ = Task.Run(async () =>
            {
                var cachingService = _provider.GetRequiredService<CachingService>();
                if (!cachingService._dbConnected) return;

                var author = socketMessage.Author;
                var socketGuild = (author as SocketGuildUser)?.Guild;
                if (socketGuild == null) return;

                cachingService.SetupBadWords(socketGuild);
                var badWords = cachingService.GetBadWords(socketGuild.Id).ToArray();

                string embedDesc = socketMessage.Embeds?.FirstOrDefault()?.Description ?? string.Empty;

                bool containsBadWord = badWords.Any(word => socketMessage.Content.Contains(word)) ||
                                       badWords.Any(word => embedDesc.Contains(word));

                if (!containsBadWord) return;

                bool authorIsBot = author.IsBot;
                bool authorHasManageMessages = (author as SocketGuildUser)?.GuildPermissions.ManageMessages ?? false;


                if (!authorHasManageMessages && !authorIsBot)
                {
                    Console.WriteLine(socketMessage.Content);
                    await socketMessage.DeleteAsync();
                }
            });
            return Task.CompletedTask;
        }
        private Task UpdateProfilePictures(SocketUser user, SocketUser user2)
        {
            _ = Task.Run(async () =>
            { 
                if(user.Id == 135446225565515776 || user.Id == 208624502878371840)
                {
                    var tao = _client.GetUser(135446225565515776).GetAvatarUrl();
                    var tjamp = _client.GetUser(208624502878371840).GetAvatarUrl();

                    Database.UpdateProfilePicture(tao, tjamp);
                    TempDB.UpdateProfilePicture(tao, tjamp);
                }
            });
            return Task.CompletedTask;
        }
    }
}

