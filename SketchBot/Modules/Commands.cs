﻿using Discord;
//using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UrbanDictionnet;
using System.Diagnostics;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Sketch_Bot.Custom_Preconditions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using DiscordBotsList;
using DiscordBotsList.Api;
using System.Threading;
using Discord.Interactions;
using System.Data;
using System.IO;

namespace Sketch_Bot.Modules
{
    public class Commands : InteractionModuleBase<SocketInteractionContext>
    {

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public static
        CultureInfo ci = CultureInfo.InvariantCulture;
        Random _rand;
        Stopwatch _stopwatch;

        private DiscordBotsListService _discordBotListService;
        private TimerService _timerService;
        private StatService _statService;
        private CachingService _cachingService;
        private InteractionService _interactionService;
        public Commands(DiscordBotsListService service, TimerService service2, StatService service3, CachingService service4, InteractionService service5)
        {
            _discordBotListService = service;
            _timerService = service2;
            _statService = service3;
            _cachingService = service4;
            _interactionService = service5;
        }

        [SlashCommand("repeat", "Echo a message")]
        public async Task testt(string input)
        {
            await DeferAsync();
            await FollowupAsync($"{Context.User.Mention} < {input}");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?repeat with success! and repeated " + input + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireUserPermission(GuildPermission.SendTTSMessages)]
        [SlashCommand("repeattts", "Echo a message")]
        public async Task repeattts(string input)
        {
            await DeferAsync();
            await FollowupAsync($"{Context.User.Mention} < {input}", null, true);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?repeat with success! and repeated " + input + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [SlashCommand("rate", "Rates something out of 100")]
        public async Task rate(string input)
        {
            await DeferAsync();
            _rand = new Random();
            int randomFileIndex = _rand.Next(1001);
            double rating = randomFileIndex / 10.0;
            if (input == "hhx")
            {
                await FollowupAsync("I rate " + input + " **" + "-1" + "** out of 100");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got -1 out of 10 (hhx)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
            else if (input.ToLower() == "mee6")
            {
                await FollowupAsync("I rate " + input + " **" + "-1" + "** out of 100");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got -1 out of 10 (hhx)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
            else switch (input)
                {
                    case "stx":
                        await FollowupAsync("I rate " + input + " **" + "-1" + "** out of 100");
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got -1 out of 10 (stx)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                        break;
                    case "the meaning of life":
                        await FollowupAsync("I rate " + input + " **" + "42" + "** out of 42");
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 42 out of 42 (the meaning of life)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                        break;
                    case "bush":
                        await FollowupAsync("I rate " + input + " **" + "9" + "** out of 11");
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 9 out of 11 (bush)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                        break;
                    case "@Taoshi":
                        await FollowupAsync("I rate " + input + " **" + "2147483647" + "** out of 100");
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 2147483647 out of 10 (Taoshi)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                        break;
                    case "Taoshi#3480":
                        await FollowupAsync("I rate " + input + " **" + "2147483647" + "** out of 100");
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 2147483647 out of 10 (Taoshi)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                        break;
                    case "Taoshi":
                        await FollowupAsync("I rate " + input + " **" + "2147483647" + "** out of 100");
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 2147483647 out of 10 (Taoshi)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                        break;
                    default:
                        {
                            if (input.ToLower() == "taoshi")
                            {
                                await FollowupAsync("I rate " + input + " **" + "2147483647" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 2147483647 out of 10 (Taoshi)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else if (input == "<@135446225565515776>")
                            {
                                await FollowupAsync("I rate " + input + " **" + "2147483647" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 2147483647 out of 10 (Taoshi)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else if (input == "htx")
                            {
                                await FollowupAsync("I rate " + input + " **" + "101" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 11 out of 10 (htx)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else if (input == "riskage")
                            {
                                await FollowupAsync("I rate " + input + " **" + "100" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 10 out of 10 (riskage)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else if (input == "riskage bot")
                            {
                                await FollowupAsync("I rate " + input + " **" + "100" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 10 out of 10 (riskage bot)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else if (input == "@Tjampen")
                            {
                                await FollowupAsync("I rate " + input + " **" + "9999999" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 10 out of 10 (riskage bot)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else if (input == "Tjampen")
                            {
                                await FollowupAsync("I rate " + input + " **" + "9999999" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 10 out of 10 (riskage bot)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else if (input == "<@208624502878371840>")
                            {
                                await FollowupAsync("I rate " + input + " **" + "9999999" + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got 10 out of 10 (riskage bot)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }
                            else
                            {
                                await FollowupAsync("I rate " + input + " **" + rating + "** out of 100");
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?rate with success! and got " + randomFileIndex + " out of 10 " + "(" + input + ")" + " (" + Context.Guild?.Name ?? "DM" + ")");
                            }

                            break;
                        }
                }
        }
        [SlashCommand("roll", "Rolls between x and y")]
        public async Task roll(int min = 1, int max = 100)
        {
            await DeferAsync();
            _rand = new Random();
            try
            {
                if (min > max)
                {
                    await FollowupAsync("The minimum value must not be over the maximum value!");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?roll and failed! (MinOverMaxException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                }
                else
                {
                    var rng = _rand.Next(min, max);
                    await FollowupAsync($"{Context.User.Username} rolled {rng} ({min}-{max})");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?roll with success! and rolled " + rng + "(" + min + "-" + max + ")" + " (" + Context.Guild?.Name ?? "DM" + ")");
                }
            }
            catch (IndexOutOfRangeException)
            {
                await FollowupAsync("The number has to be between 0 and 2147483647!");
            }
        }
        [SlashCommand("hello", "Hello")]
        public async Task hello()
        {
            await DeferAsync();
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                await FollowupAsync("Hello developer! How are you doing today?");
            }
            else
            {
                await FollowupAsync("Hi! " + Context.User.Username);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?hello with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [SlashCommand("donate", "Sends a link for donations")]
        public async Task donate()
        {
            await DeferAsync();
            await FollowupAsync("https://www.patreon.com/Sketch_Bot");
        }
        [SlashCommand("upvote", "Sends a link for upvoting the bot")]
        public async Task upvote()
        {
            await DeferAsync();
            await ReplyAsync("You can upvote the bot here https://discordbots.org/bot/369865463670374400");
            var Api = _discordBotListService.DblApi();
            if (await Api.HasVoted(Context.User.Id))
            {
                await FollowupAsync("Thanks for voting today!");
            }
            else
            {
                await FollowupAsync("You have not voted today");
            }
        }
        [SlashCommand("gamble", "Gamble tokens")]
        public async Task gamble(long amount)
        {
            await DeferAsync();
            _rand = new Random();
            var currentTokens = Database.GetUserStatus(Context.User).FirstOrDefault().Tokens;
            if (amount > currentTokens) await FollowupAsync("You don't have enough tokens");
            else if (amount < 1) await FollowupAsync("The minimum amount of tokens is 1");
            else
            {
                var RNG = _rand.Next(0, 100);
                if (RNG >= 53)
                {
                    Database.ChangeTokens(Context.User, amount);
                    currentTokens = Database.GetUserStatus(Context.User).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You won!",
                        Description = $"You gambled {amount} tokens and won!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
                    }.WithAuthor(author =>
                    {
                        author.Name = "Gambling results - " + Context.User.Username;
                        author.WithIconUrl(Context.User.GetAvatarUrl());
                    }
                    );
                    var embed = builder.Build();
                    await FollowupAsync("", null, false, false, null, null, null, embed);
                }
                else
                {
                    Database.RemoveTokens(Context.User, amount);
                    currentTokens = Database.GetUserStatus(Context.User).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You lost!",
                        Description = $"You gambled {amount} tokens and lost!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
                    }.WithAuthor(auther =>
                    {
                        auther.Name = "Gambling results - " + Context.User.Username;
                        auther.WithIconUrl(Context.User.GetAvatarUrl());
                    }
                    );
                    var embed = builder.Build();
                    await FollowupAsync("", null, false, false, null, null, null, embed);
                }
            }
        }
        [SlashCommand("gambleall", "Gambles all of your tokens")]
        public async Task gambleall()
        {
            await DeferAsync();
            long amount = Database.GetUserStatus(Context.User).FirstOrDefault().Tokens;
            _rand = new Random();
            var currentTokens = Database.GetUserStatus(Context.User).FirstOrDefault().Tokens;
            if (amount < 1) await FollowupAsync("The minimum amount of tokens is 1");
            else
            {
                var RNG = _rand.Next(0, 100);
                if (RNG >= 53)
                {
                    Database.ChangeTokens(Context.User, amount);
                    currentTokens = Database.GetUserStatus(Context.User).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You won!",
                        Description = $"You gambled {amount} tokens and won!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
                    }.WithAuthor(author =>
                    {
                        author.Name = "Gambling results - " + Context.User.Username;
                        author.WithIconUrl(Context.User.GetAvatarUrl());
                    }
                    );
                    var embed = builder.Build();
                    await FollowupAsync("", null, false, false, null, null, null, embed);
                }
                else
                {
                    Database.RemoveTokens(Context.User, amount);
                    currentTokens = Database.GetUserStatus(Context.User).FirstOrDefault().Tokens;
                    EmbedBuilder builder = new EmbedBuilder()
                    {
                        Title = "You lost!",
                        Description = $"You gambled {amount} tokens and lost!\n" +
                        $"You now have {currentTokens} tokens!",
                        Color = new Color(0, 0, 255)
                    }.WithAuthor(auther =>
                    {
                        auther.Name = "Gambling results - " + Context.User.Username;
                        auther.WithIconUrl(Context.User.GetAvatarUrl());
                    }
                    );
                    var embed = builder.Build();
                    await FollowupAsync("", null, false, false, null, null, null, embed);
                }
            }
        }
        [SlashCommand("ping", "Pong")]
        public async Task ping()
        {
            await DeferAsync();
            await FollowupAsync("Pong!");
            /*
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            var message = await RespondAsync("hello!");
            _stopwatch.Stop();
            await message.DeleteAsync();
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            embedBuilder.Title = "Pong!";
            embedBuilder.AddField("Gateway latency", Context.Client.Latency + "ms");
            var time = _stopwatch.ElapsedMilliseconds;
            embedBuilder.AddField("Execution time", time + "ms");
            var embed = embedBuilder.Build();
            await RespondAsync("", null,false,false,null,null,null,embed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?ping with success! " + Context.Client.Latency + "ms" + " (" + Context.Guild?.Name ?? "DM" + ")");
        */
        }
        [SlashCommand("riskage", "Sends you a riskage")]
        public async Task riskage()
        {
            await DeferAsync();
            await FollowupWithFileAsync("DAB/riskage.jpg");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?riskage with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [UserCommand("avatar")]
        public async Task avatar(IUser user)
        {
            await DeferAsync();
            await FollowupAsync(user.GetAvatarUrl(ImageFormat.Auto, 256));
        }
        [SlashCommand("eightball", "Ask the 8ball a question")]
        public async Task eightball(string input)
        {
            await DeferAsync();
            string[] predictionsTexts = new string[]
            {
                "It is very unlikely.",
                "I don't think so...",
                "Yes !",
                "I don't know",
                "No.",
                "Without a doubt",
                "Pls don't",
                "Just give me your money!"
            };
            _rand = new Random();
            int randomIndex = _rand.Next(predictionsTexts.Length);
            string text = predictionsTexts[randomIndex];
            await FollowupAsync(":8ball: **Question: **" + input + "\n**Answer: **" + text);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?8ball with success! " + "(" + input + ") (" + text + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("kick", "Kicks someone from the server")]
        public async Task kick(IGuildUser user, string reason = "No reason")
        {
            await DeferAsync();
            if (((IGuildUser)Context.User).GuildPermissions.KickMembers)
            {
                if (user != null)
                {
                    if (user.Id != 135446225565515776 || user.Id != 208624502878371840 || (Context.Client.CurrentUser as IGuildUser).GuildPermissions.KickMembers)
                    {
                        if (reason != null)
                        {
                            var embed = new EmbedBuilder(); ///starts embed///
                            embed.WithColor(new Color(0x4900ff)); ///hexacode colours ///
                            embed.Title = $" {user.Username} has been kicked from {user.Guild.Name}"; ///who was kicked///
                            embed.Description = $"**Username: **{user.Username}" +
                                $"\n**Guild Name: **{user.Guild.Name}" +
                                $"\n**Kicked by: **{Context.User.Mention}!" +
                                $"\n**Reason: **{reason}"; ///embed values///
                            var builtEmbed = embed.Build();
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     "
                                + Context.User.Username + " just ran ?kick with success! and kicked "
                                + (user as IGuildUser).Username + " with the reason: " + (reason) + " (" + Context.Guild.Name + ")");
                            await (user as IGuildUser).KickAsync(reason);
                            await FollowupAsync("", null, false, false, null, null, null, builtEmbed);///sends embed///
                            /*
                            if (ServerSettingsDB.GetModlogChannel(Context.Guild.Id.ToString()).FirstOrDefault().ModlogChannel != "(NULL)")
                            {
                                var moderationchannel = Context.Guild.GetTextChannel(UInt64.Parse(ServerSettingsDB.GetModlogChannel(Context.Guild.Id.ToString()).FirstOrDefault()?.ModlogChannel));
                                var embed2 = new EmbedBuilder();
                                embed2.Color = new Color(244, 66, 66);
                                //embed2.WithColor(new Color(0xb72707));
                                embed2.WithAuthor(auther =>
                                { auther.Name = user.Id.ToString(); });
                                embed2.Title = "Kick";
                                embed2.Description = reason;
                                embed2.AddField("Responsible Moderator", Context.User.Username + " (" + Context.User.Mention + ")")
                                    .AddField("Victim", user.Username + " (" + user.Mention + ")");
                                var builtEmbed2 = embed2.Build();
                                await moderationchannel.SendMessageAsync("", false, builtEmbed2);
                            }
                            */
                        }
                        else
                        {
                            await user.KickAsync(reason);
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?kick with success! and kicked " + (user as IGuildUser).Username + " (" + Context.Guild?.Name ?? "DM" + ")");
                        }
                    }
                    else
                    {
                        await FollowupAsync("I don't have the permission to do so!");
                    }
                }
                else
                {
                    await FollowupAsync("/kick <user> <reason>");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?kick and failed! (NoTargetException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                }
            }
            else
            {
                await FollowupAsync("You do not have Guild permission KickMembers");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?kick and failed! (InsufficientPermissionsException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("ban", "Bans someone from the server")]
        public async Task banAsync(IGuildUser user, string reason = "No reason")
        {
            await DeferAsync();
            if (((IGuildUser)Context.User).GuildPermissions.BanMembers == true)
            {
                if (user != null)
                {
                    if (user.Id != 135446225565515776 || user.Id != 208624502878371840 || (Context.Client.CurrentUser as IGuildUser).GuildPermissions.BanMembers)
                    {
                        if (reason != null)
                        {
                            var embed = new EmbedBuilder(); ///starts embed///
                            embed.WithColor(new Color(0x4900ff)); ///hexacode colours ///
                            embed.Title = $" {user.Username} has been banned from {user.Guild.Name}"; ///who was kicked///
                            embed.Description = $"**Username: **{user.Username}" +
                                $"\n**Guild Name: **{user.Guild.Name}" +
                                $"\n**banned by: **{Context.User.Mention}!" +
                                $"\n**Reason: **{reason}"; ///embed values///
                            var builtEmbed = embed.Build();
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     "
                                + Context.User.Username + " just ran ?ban with success! and banned "
                                + (user as IGuildUser).Username + " with the reason: " + (reason) + " (" + Context.Guild?.Name ?? "DM" + ")");
                            await Context.Guild.AddBanAsync(user, 7, reason);
                            await FollowupAsync("", null, false, false, null, null, null, builtEmbed);///sends embed///
                            /*
                            if (ServerSettingsDB.GetModlogChannel(Context.Guild.Id.ToString()).FirstOrDefault()?.ModlogChannel != "(NULL)")
                            {
                                var moderationchannel = (Context.Guild.GetTextChannel(UInt64.Parse(ServerSettingsDB.GetSettings(Context.Guild.Id.ToString()).FirstOrDefault().ModlogChannel)));
                                var embed2 = new EmbedBuilder();
                                embed2.WithColor(new Color(0xb72707));
                                embed2.WithAuthor(auther =>
                                { auther.Name = user.Id.ToString(); });
                                embed2.Title = "Ban";
                                embed2.Description = reason;
                                embed2.AddField("Responsible Moderator", Context.User.Username + " (" + Context.User.Mention + ")")
                                    .AddField("Victim", user.Username + " (" + user.Mention + ")");
                                var builtEmbed2 = embed2.Build();
                                await moderationchannel.SendMessageAsync("", false, builtEmbed2);
                            }
                            */
                        }
                        else
                        {
                            await Context.Guild.AddBanAsync(user, 7, reason);
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?ban with success! and banned " + (user as IGuildUser).Username + " (" + Context.Guild?.Name ?? "DM" + ")");
                        }
                    }
                    else
                    {
                        await FollowupAsync("I don't have the permission to do so!");
                    }
                }
                else
                {
                    await FollowupAsync("/ban <user> <reason>");
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?ban and failed! (NoTargetException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
                }
            }
            else
            {
                await FollowupAsync("You do not have Guild permission BanMembers");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?ban and failed! (InsufficientPermissionsException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("unban", "Unbans someone from the server")]
        public async Task unbanAsync(RestUser user, string reason = "No reason")
        {
            await DeferAsync();
            if (Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840 || (Context.Client.CurrentUser as IGuildUser).GuildPermissions.BanMembers)
            {
                await Context.Guild.RemoveBanAsync(user);
                await FollowupAsync(user.Username + " has been unbanned" +
                    "\n" +
                    "\n" + reason);
                /*
                if (Context.Guild.Id == 380670135045849089)
                {
                    var moderationchannel = Context.Guild.GetTextChannel(430639577334808576);
                    var embed2 = new EmbedBuilder();
                    embed2.WithColor(new Color(0x42f483));
                    embed2.WithAuthor(auther =>
                    { auther.Name = user.Id.ToString(); });
                    embed2.Title = "Unban | Case #" + casenumber;
                    embed2.Description = reason;
                    embed2.AddField("Responsible Moderator", Context.User.Username + " (" + Context.User.Mention + ")")
                        .AddField("Victim", user.Username + " (" + user.Mention + ")");
                    var builtEmbed2 = embed2.Build();
                    await moderationchannel.SendMessageAsync("", false, builtEmbed2);
                    casenumber++;
                    System.IO.File.WriteAllText("casenumber.json", casenumber.ToString());
                }
                */
            }
        }
        /*
        [SlashCommand("riskage spil", "Placeholder description")]
        public async Task riskagespil()
        {
            await FollowupAsync("https://scratch.mit.edu/projects/176501177/");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?riskage spil with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [SlashCommand("pia", "Placeholder description")]
        public async Task piasko()
        {
            await FollowupAsync("http://gonzoft.com/spil/dk/games/kaste_sko_pk/sheepspilley.html");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?pia with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        */
        [SlashCommand("status", "Checks to see if a website is up")]
        public async Task status(string websiteUrl = "http://sketchbot.xyz")
        {
            await DeferAsync();
            try
            {
                if (!websiteUrl.StartsWith("https://") && !websiteUrl.StartsWith("http://"))
                {
                    websiteUrl = "https://" + websiteUrl;
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(websiteUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (HttpStatusCode.OK == response.StatusCode)
                {
                    // Website is Online
                    response.Close();
                    var embed = new EmbedBuilder();
                    embed.Title = ("Status");
                    embed.Description = ($"{websiteUrl} \n\nserver is **online**");
                    embed.Timestamp = (DateTime.Now);
                    embed.WithFooter(footer =>
                    {
                        footer
                        .WithText("Requested by " + Context.User.Username)
                            .WithIconUrl(Context.User.GetAvatarUrl());
                    });
                    embed.ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 1024);
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                }
                else
                {
                    // Website if Offline
                    response.Close();
                    var embed = new EmbedBuilder();
                    embed.Title = ("Status");
                    embed.Description = ($"{websiteUrl} \n\nserver is **offline**");
                    embed.Timestamp = (DateTime.Now);
                    embed.WithFooter(footer =>
                    {
                        footer
                        .WithText("Requested by " + Context.User.Username)
                            .WithIconUrl(Context.User.GetAvatarUrl());
                    });
                    embed.ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 1024);
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                }
            }
            catch (WebException)

            {
                // Connection is not available
                var embed = new EmbedBuilder();
                embed.Title = ("Status");
                embed.Description = ($"{websiteUrl} \n\nconnection is **not available**");
                embed.Timestamp = (DateTime.Now);
                embed.WithFooter(footer =>
                {
                    footer
                    .WithText("Requested by " + Context.User.Username)
                        .WithIconUrl(Context.User.GetAvatarUrl());
                });
                embed.ThumbnailUrl = (Context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 1024));
                var builtEmbed = embed.Build();
                await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
            }
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Command     " + Context.User.Username + " just ran ?status with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        /*
        [SlashCommand("vuk", "Placeholder description")]
        public async Task vuk()
        {
            await RespondAsync("In memory of Vuk");
            await Context.Channel.SendFileAsync("DAB/vuk.jpg");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?vuk with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
        }
        */
        [RequireContext(ContextType.Guild)]
        [SlashCommand("nickname", "Changes your nickname")]
        public async Task nickname(string input)
        {
            await DeferAsync();
            string newNickname = string.Join(" ", input);
            try
            {
                await (Context.User as IGuildUser)?.ModifyAsync(x => x.Nickname = newNickname);
            }
            catch
            {

            }
            if ((Context.User as IGuildUser)?.Nickname == newNickname)
            {
                await (Context.User as IGuildUser)?.ModifyAsync(x => x.Nickname = newNickname);
                await FollowupAsync("Your Nickname is now **" + newNickname + "!**");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?nickname with success! (" + newNickname + ") (" + Context.Guild?.Name ?? "DM" + ")");
            }
            else
            {
                await FollowupAsync("Your role is higher than mine or you already have the same nickname");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?nickname with success! (RoleHigherException)" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
        }
        [SlashCommand("cat", "Placeholder description")]
        public async Task Cat()
        {
            await DeferAsync();
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))//This is like the 'webbrowser' (?)
                {
                    string websiteUrl = "http://aws.random.cat/meow";
                    client.BaseAddress = new Uri(websiteUrl);
                    HttpResponseMessage response = client.GetAsync("").Result;
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(result);
                    string CatImage = json["file"].ToString();
                    await FollowupAsync(CatImage);
                }
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?cat with success!" + " (" + Context.Guild?.Name ?? "DM" + ")");
            }
            catch (Exception ex)
            {
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("fox", "Placeholder description")]
        public async Task fox()
        {
            await DeferAsync();
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                ) //This is like the 'webbrowser' (?)
                {
                    var websitee = "https://randomfox.ca/floof/";
                    client.BaseAddress = new Uri(websitee);
                    HttpResponseMessage response = client.GetAsync("").Result;
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(result);
                    string catImage = json["image"].ToString();
                    await FollowupAsync($"{catImage}");
                }
            }
            catch
            {
                await FollowupAsync("API didn't return anything");
            }
        }
        [SlashCommand("birb", "Placeholder description")]
        public async Task birb()
        {
            await DeferAsync();
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                ) //This is like the 'webbrowser' (?)
                {
                    var websitee = "https://random.birb.pw/img/";
                    string websiteurl = "http://random.birb.pw/tweet.json/";
                    client.BaseAddress = new Uri(websiteurl);
                    HttpResponseMessage response = client.GetAsync("").Result;
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(result);
                    string CatImage = json["file"].ToString();
                    await FollowupAsync($"{websitee}{CatImage}");
                }
            }
            catch
            {
                await FollowupAsync("API didn't return anything");
            }
        }

        [SlashCommand("calculate", "calculates a math problem")]
        public async Task calculateAsync(HelperFunctions.Calculation expression)
        {
            await DeferAsync();
            try
            {
                await FollowupAsync(expression.Result.ToString());
            }
            catch
            {
                await FollowupAsync(expression.Error);
            }
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("membercount", "Tells you how many users are in the guild")]
        public async Task memcount()
        {
            await DeferAsync();

            var bots = Context.Guild.Users.Count(x => x.IsBot);
            var members = Context.Guild.MemberCount;
            double ratio = (double)bots / (double)members;
            double percentage = Math.Round((double)bots / (double)members, 3) * 100;
            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = $"Member count for {Context.Guild.Name}",
                Description = $"{Context.Guild.MemberCount} Total members ({percentage}% bots)\n" +
                $"{Context.Guild.Users.Count(x => x.IsBot)} Bots\n" +
                $"{members - Context.Guild.Users.Count(x => x.IsBot)} Users\n" +
                $"{ratio} Bot to user ratio",
                Color = new Color(0, 0, 255)
            };
            var embed = builder.Build();
            await FollowupAsync("", null, false, false, null, null, null, embed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?membercount with success! (" + Context.Guild.MemberCount + ") (" + Context.Guild?.Name ?? "DM" + ")");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("tokens", "Shows you how many tokens you have")]
        public async Task userstatus(IGuildUser user = null)
        {
            await DeferAsync();
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            if (user == null)
            {
                user = Context.User as IGuildUser;
            }
            Database.CreateTable(Context.Guild.Id.ToString());
            var result = Database.CheckExistingUser(user);

            if (!result.Any())
            {
                Database.EnterUser(user);
            }

            var userTable = Database.GetUserStatus(user);

            embed.Description = (user.Mention + " has " + userTable.FirstOrDefault().Tokens + " tokens to spend!:small_blue_diamond:");
            var builtEmbed = embed.Build();
            await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?tokens with success! (" + userTable.FirstOrDefault().Tokens + ") (" + Context.Guild.Name + ")");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("leaderboard", "Server leaderboard of Tokens or Leveling")]
        public async Task leaderboard(string type, int index = 1)
        {
            await DeferAsync();
            type = type.ToLower();
            string[] types = {"tokens", "leveling"};
            index = index > 0 ? index : 1;
            int pagelimit = index - index + 10 * index - 10;
            IUser user = Context.User;
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            var list = Database.GetAllUsersTokens(user);
            var foreachedlist = new List<string>();
            if (types.Contains(type))
            {
                foreach (var item in list.Skip(pagelimit).Take(10))
                {
                    int position = list.IndexOf(item) + 1;
                    string padded = position.ToString() + ".";
                    string userName;
                    var currentUser = Context.Guild.Users.FirstOrDefault(x => x.Id == ulong.Parse(item.UserId));
                    if (currentUser == null)
                    {
                        userName = $"Unknown({item.UserId})";
                    }
                    else
                    {
                        userName = currentUser.Nickname ?? currentUser.Username;
                    }
                    string leftside = padded.PadRight(4) + userName;
                    string levelandxp = item.Level.ToString() + " " + item.XP.ToString() + "/" + XP.caclulateNextLevel(item.Level);
                    foreachedlist.Add(type == "tokens" ? (leftside.PadRight(25 + 19 - item.Tokens.ToString().Length) + item.Tokens.ToString()) : leftside.PadRight(25 + 10 - item.Level.ToString().Length) + " " + levelandxp);
                }
                double decimalnumber = list.Count / 10.0D;
                var celing = Math.Ceiling(decimalnumber);
                if (index > celing)
                {
                    await FollowupAsync("This page is empty");
                }
                else
                {
                    var arrayedlist = foreachedlist.ToArray();
                    string longstring = string.Join("\n", arrayedlist);
                    embed.Title = $"{type} leaderboard for {Context.Guild.Name}";
                    embed.Description = ($"```css\n{longstring}\n```");
                    embed.WithFooter($"Page {index}/{celing}");
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?leaderboard tokens with success! (" + index + ") (" + Context.Guild?.Name ?? "DM" + ")");
                }
            }
            else
            {
                await FollowupAsync("Usage: /leaderboard <type> <page>" +
                    "\nAvailable types:" +
                    "\nTokens, Leveling");
            }
        }
        [RequireContext(ContextType.Guild)]
        [UserCommand("expose")]
        public async Task expose(IUser user)
        {
            await DeferAsync();
            await FollowupAsync(user.Mention + " just got exposed <:exposed:357837551886925844>");
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?expose with success! and exposed (" + user.Username + ") (" + Context.Guild.Name + ")");
        }
        [SlashCommand("invite", "Invite me to your server")]
        public async Task invite()
        {
            await DeferAsync();
            await FollowupAsync("**" + Context.User.Username + "**, use this URL to invite me" +
                "\nhttps://discord.com/api/oauth2/authorize?client_id=369865463670374400&permissions=1617578818631&scope=bot%20applications.commands");
            if (Context.Guild != null)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?invite with success!" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?invite with success!" + " (" + Context.User + " DM" + ")");
            }
        }
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("purge", "Purges messages from the channel")]
        public async Task purge(uint amount)
        {
            await DeferAsync();
            if ((Context.User as IGuildUser).GuildPermissions.ManageMessages == true)
            {
                var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                await (Context.Channel as ITextChannel)?.DeleteMessagesAsync(messages);
                const int delay = 3000;
                var m = await ReplyAsync($"Purge completed. _This message will be deleted in {delay / 1000} seconds._");
                await Task.Delay(delay);
                await m.DeleteAsync();
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?purge with success! and deleted " + amount + " messages" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                await FollowupAsync("You do not have guild permission ManageMessages");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?purge and failed! (InsufficientPermissionsException)" + " (" + Context.Guild.Name + ")");
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("award", "Give someone tokens")]
        public async Task Award(IUser user, int tokens, string comment = null)
        {
            await DeferAsync();
            var name = (user as IGuildUser).Nickname ?? user.Username;
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild == true || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var result = Database.CheckExistingUser(user);
                if (result.Count() <= 1)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 255)
                    };
                    Database.ChangeTokens(user, tokens);
                    if (comment != null)
                    {
                        embed.Title = (name + " was awarded " + tokens + " tokens!");
                        embed.Description = (comment);
                        var builtEmbed = embed.Build();
                        await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + name + " just ran ?award with success!" + " (" + tokens + ") (" + comment + ") (" + Context.Guild.Name + ")");
                    }
                    else
                    {
                        embed.Title = (name + " was awarded " + tokens + " tokens!");
                        var builtEmbed = embed.Build();
                        await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + name + " just ran ?award with success!" + " (" + tokens + ")" + " (" + Context.Guild.Name + ")");
                    }
                }
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        [Custom_Preconditions.Ratelimit(1,5,Custom_Preconditions.Measure.Minutes,Custom_Preconditions.RatelimitFlags.ApplyPerGuild)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("awardall", "Give everyone on the server some tokens")]
        public async Task awardall(int tokens, string comment = "")
        {
            await DeferAsync();
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild == true || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                await ReplyAsync("Handing out tokens.....");
                var users = Context.Guild.Users;
                foreach (var user in users)
                {
                    var result = Database.CheckExistingUser(user);
                    if (result.Count() <= 1)
                    {
                        Database.ChangeTokens(user, tokens);
                    }
                }
                var embed = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 255)
                };
                if (comment != "")
                {
                    embed.Title = ("All users were awarded " + tokens + " tokens!");
                    embed.Description = (comment);
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?awardall with success!" + " (" + tokens + ") (" + comment + ") (" + Context.Guild.Name + ")");
                }
                else
                {
                    embed.Title = ("All users were awarded " + tokens + " tokens!");
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?awardall with success!" + " (" + tokens + ") (" + Context.Guild.Name + ")");
                }
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("daily", "Claim your daily")]
        public async Task Daily(IGuildUser user = null)
        {
            await DeferAsync();
            if (user == null)
            {
                user = Context.User as IGuildUser;
            }
            // These lines checks if the user exits, if not we add him into the database
            var result = Database.CheckExistingUser(user);
            if (!result.Any())
            {
                Database.EnterUser(user);
            }
            var tableName = Database.GetUserStatus(Context.User); // We get the user status

            DateTime now = DateTime.Now; // We get the actual time
            DateTime daily = tableName.FirstOrDefault().Daily;
            int difference = DateTime.Compare(daily, now);
            if ((tableName.FirstOrDefault()?.Daily.ToString() == "0001-01-01 00:00:00") || (daily.DayOfYear < now.DayOfYear && difference < 0 || difference >= 0 || daily.Year < now.Year))
            {
                int amount = 50; // The amount of credits the user is gonna receive, in uint of you followed BossDarkReaper advises or in int
                if (await _discordBotListService.DblApi().HasVoted(Context.User.Id))
                {
                    amount *= 4;
                    await FollowupAsync("Thanks for voting today, here is a bonus");
                }
                else
                {
                    await FollowupAsync($"You would have gotten 4x more tokens if you have voted today. See /upvote");
                }
                Database.ChangeDaily(Context.User);
                if (user != Context.User as IGuildUser)
                {
                    _rand = new Random();
                    amount += _rand.Next(amount * 2);
                    await FollowupAsync($"You have given {user.Nickname ?? user.Username} {amount} daily tokens!");
                    Database.ChangeTokens(user, amount);
                }
                else
                {
                    Database.ChangeTokens(user, amount); // We add the tokens to the user
                    await FollowupAsync($"You received your {amount} tokens!");
                }
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?daily with success!" + " (" + Context.Guild.Name + ")");
            }
            else
            {
                TimeSpan diff = now - daily; // This line compute the difference of time between the two dates

                // This line prevents "Your credits refresh in 00:18:57.0072170 !"
                TimeSpan di = new TimeSpan(23 - diff.Hours, 60 - diff.Minutes, 60 - diff.Seconds);

                await FollowupAsync($"Your tokens refresh in {di} !");
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?daily and failed!" + " (NotRefreshedException) (" + Context.Guild.Name + ")");
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("pay", "Pay someone else some of your tokens")]
        public async Task pay(IUser usertopay, int amount, string comment = null)
        {
            await DeferAsync();
            var user = Context.User as SocketGuildUser;
            var result = Database.CheckExistingUser(user);
            var result2 = Database.CheckExistingUser(usertopay);
            var result3 = _cachingService.GetBlackList();
            if (!result3.Contains(user.Id))
            {
                if (result.Any())
                {
                    if (result2.Count >= 1)
                    {
                        var userTable = Database.GetUserStatus(user);
                        var userToPay = Database.GetUserStatus(usertopay);
                        if (amount > 0)
                        {
                            if (userTable.FirstOrDefault().Tokens >= amount)
                            {
                                Database.RemoveTokens(user, amount);
                                Database.ChangeTokens(usertopay, amount);
                                if (comment != null)
                                {
                                    var embed = new EmbedBuilder()
                                    {
                                        Color = new Color(0, 0, 255)
                                    };
                                    embed.Description = (user.Mention + " has paid " + usertopay.Mention + " " + amount + " tokens!" +
                                        "\n" + comment);
                                    var builtEmbed = embed.Build();
                                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                                }
                                else
                                {
                                    var embed = new EmbedBuilder()
                                    {
                                        Color = new Color(0, 0, 255)
                                    };
                                    embed.Description = (user.Mention + " has paid " + usertopay.Mention + " " + amount + " tokens!");
                                    var builtEmbed = embed.Build();
                                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?pay with success!" + " (" + Context.Guild.Name + ")");
                                }
                            }
                        }
                        else
                        {
                            await FollowupAsync("Dont attempt to steal tokens from people!");
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?pay and failed!" + " (TriedToStealTokensException) (" + Context.Guild.Name + ")");
                        }
                    }
                    else
                    {
                        await FollowupAsync("Target user not in the database! adding user...");
                        Database.EnterUser((IGuildUser)usertopay);
                        await FollowupAsync("User added! try running the command again");
                    }
                }
                else
                {
                    await FollowupAsync("User not in the database! adding user...");
                    Database.EnterUser(user);
                    await FollowupAsync("User added! try running the command again");
                }
            }
            else
            {
                await FollowupAsync("You can't pay blacklisted users!");
            }
        }
        [SlashCommand("info", "Displays info about the bot")]
        public async Task info()
        {
            await DeferAsync();
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            int? totalmembers = 0;
            foreach (var guild in Context.Client.Guilds)
            {
                totalmembers += guild.MemberCount;
            }
            _rand = new Random();
            var builder = new EmbedBuilder()
                    .WithTitle("Info about " + Context.Client.CurrentUser.Username + ":")
                    .WithDescription("\nBeep boop... I am " + Context.Client.CurrentUser.Username +
                "\nI have some commands that you can find with slash commands" +
                "\nI am a bit sketchy so watch out and be careful" +
                "\nMy date of manufactor is 19/10/2017")
                    .WithColor(new Color((uint)_rand.Next(0x0, 0xFFFFFF)))
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(footer =>
                    {
                        footer
                        .WithText("Requested by " + Context.User.Username)
                            .WithIconUrl(Context.User.GetAvatarUrl());
                    })
            .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
            .WithAuthor(author =>
            {
                author
                .WithName("Info:")
                .WithUrl("https://discordapp.com/oauth2/authorize?client_id=369865463670374400&scope=bot&permissions=8");
            }).AddField("Developers:", $"Bot developer: {await Context.Client.GetUserAsync(135446225565515776)}" +
                                    $"\nWeb developer: {await Context.Client.GetUserAsync(208624502878371840)}", true)
            .AddField("Other info:", "I am in " + Context.Client.Guilds.Count + " servers!" +
            "\n" + totalmembers + " members across all servers!" +
            "\nUptime: " + uptime.Days + " Days " + uptime.Hours + " Hours " + uptime.Minutes + " Minutes " + uptime.Seconds + " Seconds" +
            "\nAverage messages per min since startup: " + _statService.msgCounter/_statService.uptime.TotalMinutes, true)
            .AddField("My server:", "https://discord.gg/UPG8Vqb", true).AddField("Website:", "https://www.sketchbot.xyz", true);
            var embed = builder.Build();
            await FollowupAsync("", null,false,false,null,null,null,embed);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?info with success!" + " (" + Context.Guild.Name ?? "(DM)" + ")");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("serverinfo", "Displays info about the server")]
        public async Task serverinfo()
        {
            await DeferAsync();
            var guild = Context.Guild;
            EmbedBuilder builder = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255),
                Timestamp = guild.CreatedAt,
            };
            builder.WithAuthor(author =>
            {
                author.Name = guild.Name;
                author.IconUrl = guild.IconUrl;
            });
            builder.WithFooter(footer =>
            {
                footer.Text = $"ID: {guild.Id} | Server Created";
            });
            builder.AddField("Owner", guild.Owner.Username + guild.Owner.Discriminator,true)
                .AddField("Region", guild.VoiceRegionId, true)
                .AddField("Channel Categories", guild.CategoryChannels.Count, true)
                .AddField("Text Channels", guild.TextChannels.Count,true)
                .AddField("Voice Channels", guild.VoiceChannels.Count, true)
                .AddField("Members", guild.MemberCount, true)
                .AddField("Humans", guild.Users.Count(x => !x.IsBot),true)
                .AddField("Bots", guild.Users.Count(x => x.IsBot), true)
                .AddField("Roles", guild.Roles.Count,true);
            var embed = builder.Build();
            await FollowupAsync("", null,false,false,null,null,null,embed);
        }
        [RequireContext(ContextType.Guild)]
        [UserCommand("userinfo")]
        public async Task userinfo(IUser user)
        {
            await DeferAsync();
            EmbedBuilder builder = new EmbedBuilder()
            {
                ThumbnailUrl = user.GetAvatarUrl(),
                Color = new Color(0, 0, 255),
                Timestamp = DateTime.Now
            };
            builder.WithAuthor(author =>
            {
                author.Name = user.Username + user.Discriminator;
                author.IconUrl = user.GetAvatarUrl();
            });
            builder.WithFooter(footer =>
            {
                footer.Text = $"ID: {user.Id}";
            });
            var users = Context.Guild.Users;
            var userslist = users.OrderBy(o => o.JoinedAt).ToList();
            var joinedpos = userslist.FindIndex(x => x.Id == user.Id);
            var roles = "";
            foreach(var role in ((IGuildUser)user).RoleIds)
            {
                roles += $"<@&{role}>\n";
            }

            builder.AddField("Joined", ((IGuildUser)user).JoinedAt, true).AddField("Join Position", joinedpos, true)
                .AddField("Registered", user.CreatedAt)
                .AddField($"Roles [{((IGuildUser)user).RoleIds.Count}]", roles);
            var embed = builder.Build();
            await FollowupAsync("", null,false,false,null,null,null,embed);
        }
        [SlashCommand("jojo", "Sends a random JoJo image")]
        public async Task Jojosbizzareadventure()
        {
            await DeferAsync();
            _rand = new Random();
            var dir = new DirectoryInfo("Jojo");
            var dirFiles = dir.GetFiles();
            int fileIndex = _rand.Next(dirFiles.Length);
            int pictureNumber = fileIndex + 1;
            string fileToPost = dirFiles[fileIndex].FullName;
            await Context.Channel.SendFileAsync(fileToPost, "Jojo's bizzare adventure " + pictureNumber);
            await FollowupAsync();
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?Jojo with success! " + pictureNumber + " (" + Context.Guild.Name + ")");
        }
        [SlashCommand("random", "Sends a random message")]
        public async Task Random()
        {
            await DeferAsync();
            _rand = new Random();

            var RandomMessages = new string[]
{
            "æøå",
            "Ecks dee",
            "123456789",
            "This is a list",
            "Random Message",
            "RNG",
            "HTX > HHX > STX",
            "STX = Trash",
            "Mee6 kan ikke commands med flere ord",
            "?help",
            "Ingress",
            "Pokemon GO",
            "C#",
            "Maple",
            "Mimouniboi",
            "9gag",
            "ArrayList",
            "Mikerosoft Certified Technician",
            "Emo",
            "Anime",
            "133769420",
            "MLG",
            "K-WHY-S",
            "Chrunchyroll",
            "Agoraen",
            "Desperat skole hjælp",
            "Discord",
            "Lidl",
            "Kantinen er overpriced",
            "<:sandwich:355316315780677632>",
            "xD",
            ":icecube:",
            "DAB",
            "Programmering",
            "Gajs",
            "Gajs, der er time",
            "Produktudvikling",
            "Naturvidenskabeligtgrundforløb",
            "boi",
            "fuccboi",
            "Hatsune Miku",
            "Itslearning",
            "Riskage",
            "How much Ris can a Rischuck chuck if the Rischuck could chuck Rishi",
            "Newtonmeter",
            "Alle realtal",
            "Den tomme mængde",
            "Dramaalert",
            "Scarce",
            "Hey what's up guys it's Scarce here",
            "Naturvidenskabelig metode",
            "Ulduar",
            "niceme.me",
            "Rishi",
            "Nibba",
            "Plagierkontroler",
            "Mee6 er lårt",
            "World of Warcraft",
            "Blizzard",
            "Elevplan",
            "Nielsen",
            "yaaaarrr boi",
            "Waps",
            "Riskage spil",
            "AWS",
            "Amazon",
            "Ebay",
            "Aliexpress",
            "Nordisk film",
            "Bone's",
            "EC2",
            "GIF",
            "Instances",
            "Storage",
            "S3",
            "NVM",
            "Database",
            "dEcLaN.eXe",
            "Mojo",
            "SQL",
            "Hello World",
            "HTML",
            "CSS",
            "PHP",
            "JS",
            "T H I C C",
            "Samfundsfag = sovetime",
            "Vuk",
            "Vektorfunktioner",
            "Vukterfunktioner",
            "In memory of Vuk",
            "Sweet Silence",
            "Gucci gang",
            "Osu!",
            "What is up AutismAlert nation",
            "LinusTechTips",
            "Scrapyard wars",
            "Tunnelbear",
            "One-energy cola",
            "Water",
            "Such message very random",
            "4:3 Stretched",
            "Black bars",
            "Java",
            "Javascript",
            "Eclipse",
            "This list is getting looooooooooooooong",
            "Craigslist",
            "SLI",
            "Intel & nVidia > AMD",
            "Razer",
            "Razer blackwidow chroma",
            "Stationspizza",
            "Linus",
            "Windows",
            "Macbook",
            "Linux",
            "Raspberry Pi",
            "Arduino",
            "LCD",
            "Jonte-bro",
            "Password",
            "O2Auth",
            "discordapp.com",
            "discord.gg",
            "Sodapoppin",
            "2147483647",
            "4294967295",
            "Battlefield Heroes",
            "Fortnite",
            "Rema 1000",
            "Fakta",
            "PUBG",
            "Playerunknown's Battlegrounds",
            "Far Cry 5",
            "Far Cry 4",
            "Far Cry 3",
            "PewDiePie",
            "Nick Crompton",
            "England er min by",
            "England is my city",
            "?riskage spil",
            "777",
            "Jackpot",
            "Luke",
            "Thomas Jefferson Chance Morris",
            "420",
            "1337",
            "69",
            "Gaming-linjen",
            "IT-Videnskab",
            "Hearthstone",
            "Its everyday bro",
            "Snapchat",
            "Telegram",
            "Minecraft commandblocks",
            "Ninja",
            "#weebconfirmed",
            "Kommunikationsmodeller",
            "What's 9 + 10? 21!",
            "Divine spirit",
            "Innerfire",
            "Legendary",
            "Legiondary",
            "Legend",
            "Cities: Skyline",
            "Test",
            "Execute",
            "Leeeeeeerrroooooyyyyyy Jeeeeeeeeeeeeeenkinsssss",
            "C'Thun",
            "Standard > Wild",
            "Duplicates",
            "Wallpaper Engine",
            "Deez nuts",
            "Insert meme here",
            "Ultrasaur",
            "Ban",
            "Kick",
            "Help! My creator forces me to respond to commands",
            "Jonaser",
            "The ting goes skrrraa",
            "Random message number 200",
            "Gulag",
            "Tyskland",
            "Morten",
};
            int randomFileIndex = _rand.Next(RandomMessages.Length);
            int messageNumber = randomFileIndex + 1;
            string fileToPost = RandomMessages[randomFileIndex];
            await FollowupAsync(fileToPost);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", ci) + " Command     " + Context.User.Username + " just ran ?random with success! " + messageNumber + " (" + Context.Guild.Name + ")");
        }
    }
}