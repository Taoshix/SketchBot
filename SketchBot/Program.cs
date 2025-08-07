using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using MoreLinq.Extensions;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions;
using Discord.Commands;
using System.Reflection;
using System.Threading;
using Sketch_Bot.Modules;
using System.Diagnostics;
using System.Timers;
using JikanDotNet;
using Discord.Net;
using Discord.Net.Rest;
using Discord.Addons.Interactive;
using Sketch_Bot.Services;
using Sketch_Bot.Models;
using System.Text.RegularExpressions;
using OsuSharp;
using Discord.Rest;
using Victoria;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Sockets;
using Newtonsoft.Json;
using Discord.Interactions;
using Sketch_Bot.TypeConverters;
using static Sketch_Bot.Models.HelperFunctions;
using Microsoft.Extensions.Hosting;
using OsuSharp.Extensions;
using Microsoft.Extensions.Logging;
using System.Globalization;

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
        private AudioService _audioService;
        private LavaNode _lavaNode;
        private ILogger _loggerFactory;

        private Program()
        {
            if (!System.IO.File.Exists("config.json"))
            {
                Console.WriteLine("Config file not found!");
                Config.CreateDefaultConfigFile();
                Console.WriteLine("Created new default config.json file, please fill it out before running the bot again.");
                return;
            }
            _config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("config.json"));
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
            });
            _provider = new ServiceCollection()
                .AddLavaNode(x => {
                    x.SelfDeaf = false;
                })
                .AddSingleton(_client)
                .AddSingleton<AudioService>()
                .AddSingleton<LavaNode>()
                .AddLavaNode()
                .AddLogging(x => {
                    x.ClearProviders();
                    x.SetMinimumLevel(LogLevel.Trace);
                })
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
            //_audioService = _provider.GetRequiredService<AudioService>();
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
            if (!System.IO.File.Exists("config.json"))
            {
                Console.WriteLine("Config file not found!");
                Config.CreateDefaultConfigFile();
                Console.WriteLine("Created new default config.json file, please fill it out before running the bot again.");
                return;
            }
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
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)} {arg2.User.Username} Ran {arg1.Name} {string.Join(" ", arg1.FlattenedParameters)}");
            if (!arg3.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture)} {arg2.User.Username} Failed to run {arg1.Name} due to {arg3.ErrorReason}");
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
            var cachingservice = _provider.GetRequiredService<CachingService>();
            await _provider.UseLavaNodeAsync();
            if (_provider.GetRequiredService<CachingService>()._dbConnected)
                cachingservice.SetupBlackList();
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
        public async Task CreateNewTable(SocketGuild socketGuild)
        {
            if (_provider.GetRequiredService<CachingService>()._dbConnected)
            {
                var cache = _provider.GetRequiredService<CachingService>();
                string tablename = socketGuild.Id.ToString();
                Database.CreateTable(tablename);
                var castedguild = socketGuild as IGuild;
                var socketusers = await castedguild.GetUsersAsync();
                var castedusers = socketusers;
                foreach (var users in castedusers)
                {
                    if (!users.IsBot && !cache.IsInDatabase(tablename, users.Id))
                    {
                        var result = Database.CheckExistingUser(users);

                        if (!result.Any())
                        {
                            Database.EnterUser(users);
                            cache.AddUser(tablename, users.Id);
                        }
                    }
                }
            }
        }
        public async Task JoinedNewServer(SocketGuild socketGuild)
        {
            if (_provider.GetRequiredService<CachingService>()._dbConnected)
            {
                try
                {
                    var guildId = socketGuild.Id.ToString();
                    ServerSettingsDB.CreateTable(guildId);
                    //await Task.Delay(50);
                    var prefix = ServerSettingsDB.GetPrefix(socketGuild.Id.ToString());
                    int levelup = 1;
                    if (socketGuild.MemberCount >= 100)
                    {
                        levelup = 0;
                    }
                    if (!prefix.Any())
                    {
                        ServerSettingsDB.MakeSettings(guildId, levelup);
                    }
                    //await Task.Delay(50);
                    ServerSettingsDB.CreateTableWords(guildId);
                    //await Task.Delay(50);
                    Database.CreateTable(guildId);
                    //await Task.Delay(50);
                    foreach (var channel in socketGuild.TextChannels)
                    {
                        try
                        {
                            await channel.SendMessageAsync("Thanks for inviting me to your server!" +
                            "\nIf you want a channel for welcome messages, go to the channel and type " + "`/setwelcome`, to remove it type `/unsetwelcome`" +
                            "\nIf you want to kick/bans logged in a channel when you use the kick/ban command, go to the channel and type `/setmodlog`, to remove it type `/unsetmodlog`" +
                            "\nIf you need help or have any questions, join my support server" +
                            "\nhttps://discord.gg/UPG8Vqb" +
                            "\nhttp://sketchbot.xyz" +
                            "\n" +
                            "\n**If you are one of these people that hates levelup messages, then you disable them with `/DisableLevelMsg`**" +
                            "\n**Levelup messages are disabled by default if your server has more than 100 members**");
                            break;
                        }
                        catch (HttpException)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var Taoshi = _client.GetUser(135446225565515776);
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "Unable to do setup!",
                        Description = ex.ToString(),
                        Color = new Color(255, 0, 0)
                    };
                    var embed = builder.Build();
                    await Taoshi.SendMessageAsync("", false, embed);
                }
            }
        }
        private Task giveXP(SocketMessage msg) /*Creates the method*/
        {
            _ = Task.Run(async () =>
            {
                _provider.GetService<StatService>().msgCounter++;
                
                if (_provider.GetRequiredService<CachingService>()._dbConnected)
                {
                    try
                    {
                        var user = msg.Author; /*Sets variable user to msg.author*/
                        var socketGuild = (user as SocketGuildUser)?.Guild;
                        var gotRole = false;
                        var cache = _provider.GetService<CachingService>();
                        var blacklisted = cache.GetBlackList();
                        List<SocketRole> RoleIdsField = new List<SocketRole>();
                        if (user.IsBot || user.IsWebhook || socketGuild == null || blacklisted.Contains(user.Id)) return;
                        if (!cache.IsInDatabase(socketGuild.Id, user.Id))
                        {
                            cache.SetupUserInDatabase(socketGuild, (SocketGuildUser)user);
                        }
                        ServerSettingsDB.CreateTableRole(socketGuild.Id.ToString());
                        /*Gets the users Data from the database*/
                        var guildUser = (SocketGuildUser)msg.Author;
                        var xpService = _provider.GetService<XpService>();
                        if (!xpService.GetList().Contains(guildUser))
                        {
                            rand = new Random();
                            var xp = rand.Next(5, 15);
                            var tokens = rand.Next(1, 4);
                            var userData = Database.GetUserStatus(user).FirstOrDefault();
                            Database.ChangeTokens(user, tokens);
                            xpService.addUser(guildUser);
                            //var xprate = ServerSettingsDB.GetXpRate(socketGuild.Id.ToString()).FirstOrDefault().XpMultiplier;
                            if (userData == null)
                            {
                                Console.WriteLine("UserData is null!");
                                return;
                            }

                            var xpToLevelUp = XP.caclulateNextLevel(userData.Level); /*sets xpToLevelUp to the number calculateNextLevel returns we pass through the users level*/
                            //var xprate = ServerSettingsDB.GetXpRate(socketGuild.Id.ToString()).FirstOrDefault().XpMultiplier;
                            if (userData.XP >= xpToLevelUp) /*Checks if the users xp is greater or equal to the xp required to level up*/
                            {
                                int addLevels = 0;
                                while (userData.XP >= xpToLevelUp)
                                {
                                    xpToLevelUp = XP.caclulateNextLevel(userData.Level + addLevels);
                                    if (userData.XP >= xpToLevelUp)
                                    {
                                        addLevels++;
                                    }
                                }
                                Database.levelUp(user, xp, addLevels);
                                /*Levels up the user using the levelUp method we created */

                                //var roleId = ServerSettingsDB.GetRole(socketGuild.Id.ToString(), Database.GetUserStatus(user).FirstOrDefault().Level).FirstOrDefault().roleId;
                                var level = Database.GetUserStatus(user).FirstOrDefault().Level;
                                var rolesToAward = ServerSettingsDB.GetRoles(socketGuild.Id.ToString());
                                var rolesBefore = (user as SocketGuildUser).Roles;
                                if (rolesToAward.Count >= 1)
                                {
                                    var filteredRoles = rolesToAward.Where(x => x.roleLevel <= level).OrderByDescending(o => o.roleLevel);
                                    var guild = (user as SocketGuildUser).Guild;
                                    var roles = filteredRoles.Select(x => guild.GetRole(ulong.Parse(x.roleId)));
                                    await (user as SocketGuildUser).AddRolesAsync(roles);
                                    gotRole = true;
                                    RoleIdsField.AddRange(roles);
                                    if (!RoleIdsField.Any())
                                    {
                                        gotRole = false;
                                    }
                                }
                                var isLeveling = ServerSettingsDB.GetLevelupMessageBool(socketGuild.Id.ToString()).FirstOrDefault().LevelupMessages;
                                if (isLeveling)
                                {
                                    var embed = new EmbedBuilder(); /*Creates a embedBuilder*/
                                    if (gotRole == false)
                                    {
                                        embed.WithColor(new Color(0x4d006d)).AddField(y =>            /*Creates a embed with a colour then we add a field*/
                                        {
                                            //var userData2 = Database.GetUserStatus(user).FirstOrDefault(); /*We create a new variable for userData because we just updated the users level im unsure if this is required I just did it to be on the safe side*/
                                            y.Name = "Leveled up!"; /*sets the fields name to leveled up*/
                                            y.Value = $"{user.Mention} has leveled up to level {level}!";  /*We set the value to this string*/
                                        });
                                    }
                                    else
                                    {
                                        var filteredRoles = RoleIdsField.Where(p => !rolesBefore.Any(p2 => p2.Id == p.Id));
                                        var roles = filteredRoles.Select(x => x.Mention);
                                        var fulltext = string.Join("\n", roles);
                                        embed.WithColor(new Color(0x4d006d)).AddField(y =>            /*Creates a embed with a colour then we add a field*/
                                        {
                                            //var userData2 = Database.GetUserStatus(user).FirstOrDefault(); /*We create a new variable for userData because we just updated the users level im unsure if this is required I just did it to be on the safe side*/
                                            y.Name = "Leveled up!"; /*sets the fields name to leveled up*/
                                            y.Value = $"{user.Mention} has leveled up to level {level}!" +
                                                      $"\n" +
                                                      $"\nLevel up rewards:\n {fulltext}";
                                        });
                                    }
                                    var builtEmbed = embed.Build();
                                    await msg.Channel.SendMessageAsync("", embed: builtEmbed); /*Sends the embed*/
                                }
                            }
                            else if (!user.IsBot)/*else if the user is not a bot then its just gonna add xp*/
                            {
                                Database.addXP(user, xp); /*adds the xp to the user*/
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Database.CreateTable((msg.Author as SocketGuildUser).Guild.Id.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("XP Service Error: Database is down!");
                }
            });
            return Task.CompletedTask;
        }
        public Task Welcome(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                if (_provider.GetRequiredService<CachingService>()._dbConnected)
                {
                    if (!user.IsBot)
                    {
                        var result = Database.CheckExistingUser(user);
                        if (!result.Any())
                        {
                            Database.EnterUser(user);
                        }
                    }
                    rand = new Random();
                    var id = user.Guild.Id;
                    var guildName = _client.GetGuild(id) as IGuild;
                    var inviteUsed = await guildName.GetInvitesAsync();
                    var orderedInviteUsed = inviteUsed.OrderBy(x => x.CreatedAt.Value.ToUnixTimeMilliseconds()).ThenBy(x => x.Uses).FirstOrDefault();
                    var userTable = ServerSettingsDB.GetWelcomeChannel(user.Guild.Id.ToString());
                    await Task.Delay(100);
                    var channell = userTable.FirstOrDefault().WelcomeChannel;
                    await Task.Delay(100);
                    if (channell == "(NULL)" || channell == null) return;
                    await Task.Delay(100);
                    var channelid = ulong.Parse(channell);
                    await Task.Delay(100);
                    var channel = user.Guild.GetTextChannel(channelid);
                    await Task.Delay(100);
                    var builder = new EmbedBuilder()
                    .WithTitle("Member Joined")
                    .WithDescription($"Welcome {user.Mention} to {user.Guild.Name} Enjoy your stay!\nJoined with: {orderedInviteUsed.Url}\nReferred by: {orderedInviteUsed.Inviter}")
                    .WithColor(new Color(0x26E4A2))
                    .WithThumbnailUrl(user.GetAvatarUrl());
                    await Task.Delay(100);
                    var embed = builder.Build();
                    await Task.Delay(100);
                    await channel.SendMessageAsync("", false, embed);
                }
            });
            return Task.CompletedTask;
        }
        private Task MessageDelete(SocketMessage socketMessage)
        {
            _ = Task.Run(async () =>
            {
                if (_provider.GetRequiredService<CachingService>()._dbConnected)
                {
                    var auther = socketMessage.Author;
                    var socketguild = (auther as SocketGuildUser)?.Guild;
                    if (socketguild != null)
                    {
                        var cachingservice = _provider.GetRequiredService<CachingService>();
                        cachingservice.SetupBadWords(socketguild);
                        var words = cachingservice.GetBadWords(socketguild.Id);
                        var stringarray = words.ToArray();
                        string embedDesc;
                        if (socketMessage.Embeds == null)
                        {
                            embedDesc = socketMessage.Embeds?.FirstOrDefault().Description;
                        }
                        else
                        {
                            embedDesc = "";
                        }
                        if (stringarray.Any(socketMessage.Content.Contains) || stringarray.Any(embedDesc.Contains))
                        {
                            if (socketguild.Id == 448335330534359061)
                            {
                                if (auther.IsBot)
                                {
                                    if (auther.Id != 369865463670374400)
                                    {
                                        if (socketMessage.Content != "")
                                        {
                                            Console.WriteLine(socketMessage.Content);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Embed Deleted");
                                        }
                                        await socketMessage.DeleteAsync();
                                    }
                                }
                                else if (!((SocketGuildUser)auther).GuildPermissions.ManageMessages)
                                {
                                    Console.WriteLine(socketMessage.Content);
                                    await socketMessage.DeleteAsync();
                                }
                            }
                            else
                            {
                                if (!((SocketGuildUser)auther).GuildPermissions.ManageMessages && !((SocketGuildUser)auther).IsBot)
                                {
                                    Console.WriteLine(socketMessage.Content);
                                    await socketMessage.DeleteAsync();
                                }
                            }
                        }
                    }
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

