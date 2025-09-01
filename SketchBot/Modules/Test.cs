using Discord;
//using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using Sketch_Bot.Custom_Preconditions;
using YouTubeSearch;
using OsuSharp;
using System.Diagnostics;
//using OsuSharp.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Sketch_Bot.Models;
using Sketch_Bot.Services;
using ImgFlip4NET;
using Discord.Interactions;

namespace Sketch_Bot.Modules
{
    public class Test : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionService _service;
        private MemeService _service2;
        private CachingService _cachingService;

        public Test(InteractionService service, MemeService service2, CachingService service3)           /* Create a constructor for the InteractionService dependency */
        {
            _service = service;
            _service2 = service2;
            _cachingService = service3;
        }
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("addrole", "Add a role for leveling")]
        public async Task AddRoleAsync(IRole role, int level)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.CreateTableRole(Context.Guild.Id);
            ServerSettingsDB.AddRole(Context.Guild.Id, role.Id, level);
            await FollowupAsync(role.Name + " has been added! If anyone reaches level " + level + " they will recieve the role!");
        }
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("removerole", "Remove a role for leveing")]
        public async Task RemoveRoleAsync(IRole role)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.CreateTableRole(Context.Guild.Id);
            ServerSettingsDB.RemoveRole(Context.Guild.Id, role.Id);
            await FollowupAsync(role.Name + " has been removed");
        }
        [SlashCommand("youtube", "Searches YouTube and returns the first result")]
        public async Task YouTubeSearchAsync(string searchquery)
        {
            await DeferAsync();
            var items = new VideoSearch();
            var item = items.GetVideos(searchquery, 1);
            string url = item.Result.First().getUrl();
            await FollowupAsync(url);
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("roleinfo", "Displays info about a role")]
        public async Task RoleInfoAsync(IRole role)
        {
            await DeferAsync();
            var rolePermissionsList = role.Permissions.ToList();
            string permissionsListString = string.Join("\n", rolePermissionsList);
            var embed = new EmbedBuilder()
            {
                Title = $"Role info for {role.Name}",
                Color = role.Color
            };
            embed.AddField("Id", role.Id);
            embed.AddField("Position", role.Position, true);
            embed.AddField("Members", ((SocketRole)role).Members.Count(), true);
            embed.AddField("Mentionable?", role.IsMentionable);
            embed.AddField("Hoisted?",role.IsHoisted, true);
            embed.AddField("Permissions", permissionsListString);
            embed.AddField("Color", role.Color, true);
            embed.AddField("Role creation date", role.CreatedAt.DateTime.ToString("dd/MM/yy HH:mm:ss"), true);
            await FollowupAsync("", embed: embed.Build());
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("setwelcome", "Sets the welcome channel for welcome messages")]
        public async Task SetWelcomeChannelAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var channel = Context.Channel;
            ServerSettingsDB.SetWelcomeChannel(channel.Id, Context.Guild.Id);
            await FollowupAsync("This will be the new welcome channel 👍");
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("unsetwelcome", "Disables welcome messages")]
        public async Task UnsetWelcomeChannelAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.SetWelcomeChannel(0, Context.Guild.Id);
            await FollowupAsync("Welcome messages has been disabled");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("disablelevelmsg", "Disables level up messages")]
        public async Task DisableLevelMessagesAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.UpdateLevelupMessagesBool(Context.Guild.Id, 0);
            await FollowupAsync("Levelup messages are now disabled!");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("enablelevelmsg", "Enables level up messages")]
        public async Task EnableLevelMessagesAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.UpdateLevelupMessagesBool(Context.Guild.Id, 1);
            await FollowupAsync("Levelup messages are now enabled!");
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("setmodlog", "Sets the modlog channel")]
        public async Task SetModlogChannelAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            if (((IGuildUser) Context.User).GuildPermissions.ManageChannels)
            {
                var channel = Context.Channel;
                ServerSettingsDB.SetModlogChannel(channel.Id, Context.Guild.Id);
                await FollowupAsync("This will be the new mod-log channel 👍");
            }
            else
            {
                await FollowupAsync("You don't have `ManageChannels` permission");
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("unsetmodlog", "Disables the mod logging")]
        public async Task UnsetModlogChannelAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            if (((IGuildUser) Context.User).GuildPermissions.ManageChannels || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                ServerSettingsDB.SetModlogChannel(0, Context.Guild.Id);
                await FollowupAsync("Mod-log disabled");
            }
            else
            {
                await FollowupAsync("You don't have `ManageChannels` permission");
            }
        }
        [SlashCommand("activity", "Launch a discord activity in a voice channel!")]
        public async Task CreateDiscordActivityAsync(IVoiceChannel chan, DefaultApplications app)
        {
            await DeferAsync();
            var invite = await chan.CreateInviteToApplicationAsync(app);
            await Context.Interaction.FollowupAsync(invite.Url);
        }
        [SlashCommand("emote", "Enlargens an emote")]
        public async Task ShowEmoteAsync(string emote)
        {
            await DeferAsync();
            var emo = Emote.Parse(emote);
            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = emo.Name,
                ImageUrl = emo.Url,
                Url = emo.Url
            };
            var embed = builder.Build();
            await FollowupAsync("", embed:embed);
        }
        
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("slowmode", "Sets the slowmode of a channel to the input seconds")]
        public async Task SetSlowmodeAsync(int seconds)
        {
            if (seconds < 21600)
            {
                await ((ITextChannel) Context.Channel).ModifyAsync(x => x.SlowModeInterval = seconds);
                await Context.Channel.SendMessageAsync($"Slowmode is now set to {seconds} seconds");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Interval must be less than or equal to 6 hours.");
            }
        }
        [Ratelimit(1, 2, Measure.Seconds, RatelimitFlags.None)]
        [SlashCommand("memegen", "Generates a meme")]
        public async Task GenerateMemeAsync(string templateName, string topText, string bottomText)
        {
            await DeferAsync();
            var service = _service2.GetMemeService();
            var template = await service.GetMemeTemplateAsync(templateName); // TODO: Make a list and auto complete this
            if (template == null)
            {
                await FollowupAsync("Template not found.\nhttps://api.imgflip.com/popular_meme_ids");
            }
            else
            {
                var meme = await service.CreateMemeAsync(template.Id, topText, bottomText);
                await FollowupAsync(meme.ImageUrl);
            }
        }
    }
}
