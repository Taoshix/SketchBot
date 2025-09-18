using Discord;
using Discord.Interactions;
using Discord.Rest;
using SketchBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.InteractionBasedModules
{
    public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CachingService _cachingService;
        public ModerationModule(CachingService Cache)
        {
            _cachingService = Cache;
        }

        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("kick", "Kicks someone from the server")]
        public async Task KickAsync(
            [Summary("User", "The user to kick")] IGuildUser user,
            [Summary("Reason", "The reason for the kick")] string reason = "No reason")
        {
            await DeferAsync();
            var currentUser = Context.User as IGuildUser;
            var embed = new EmbedBuilder()
                .WithColor(new Color(0x4900ff))
                .WithTitle($"{user.Username} has been kicked from {user.Guild.Name}")
                .WithDescription($"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}\n**Kicked by: **{Context.User.Mention}!\n**Reason: **{reason}");
            await user.KickAsync(reason);
            await FollowupAsync("", null, false, false, null, null, null, embed.Build());
            var serverSettings = _cachingService.GetServerSettings(Context.Guild.Id);
            var modLogChannelId = serverSettings?.ModlogChannel ?? 0;
            if (modLogChannelId != 0)
            {
                var modLogChannel = Context.Guild.GetTextChannel(modLogChannelId);
                if (modLogChannel != null)
                {
                    await modLogChannel.SendMessageAsync("", false, embed.Build());
                }
            }
        }
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("ban", "Bans someone from the server")]
        public async Task BanAsync(
            [Summary("User", "The user to ban")] IGuildUser user,
            [Summary("Reason", "The reason for the ban")] string reason = "No reason specified")
        {
            await DeferAsync();
            var currentUser = Context.User as IGuildUser;
            var embed = new EmbedBuilder()
                .WithColor(new Color(0x4900ff))
                .WithTitle($"{user.Username} has been banned from {user.Guild.Name}")
                .WithDescription($"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}\n**Banned by: **{Context.User.Mention}!\n**Reason: **{reason}");
            await Context.Guild.AddBanAsync(user, 7, reason);
            await FollowupAsync("", null, false, false, null, null, null, embed.Build());
            var serverSettings = _cachingService.GetServerSettings(Context.Guild.Id);
            var modLogChannelId = serverSettings?.ModlogChannel ?? 0;
            if (modLogChannelId != 0)
            {
                var modLogChannel = Context.Guild.GetTextChannel(modLogChannelId);
                if (modLogChannel != null)
                {
                    await modLogChannel.SendMessageAsync("", false, embed.Build());
                }
            }
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("setwelcome", "Sets the welcome channel for welcome messages")]
        public async Task SetWelcomeChannelAsync([Summary("Channel", "The channel to set as welcome channel")] ITextChannel channel)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            _cachingService.SetWelcomeChannel(Context.Guild.Id, channel.Id);
            await FollowupAsync($"{channel.Mention} will be the new welcome channel 👍");
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
            _cachingService.SetWelcomeChannel(Context.Guild.Id, 0);
            await FollowupAsync("Welcome messages has been disabled");
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("setlevelmsg", "Enables or disables level up messages")]
        public async Task SetLevelMessagesAsync([Summary("Enabled", "Enable or disable level up messages")] bool enabled)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            _cachingService.SetLevelupMessages(Context.Guild.Id, enabled);
            await FollowupAsync($"Levelup messages are now {(enabled ? "enabled" : "disabled")}!");
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("setmodlog", "Sets the modlog channel")]
        public async Task SetModlogChannelAsync([Summary("Channel", "The channel to set as modlog channel")] ITextChannel channel)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            _cachingService.SetModlogChannel(Context.Guild.Id, channel.Id);
            await FollowupAsync($"{channel.Mention} will be the new mod-log channel 👍");
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [SlashCommand("unsetmodlog", "Disables the mod logging")]
        public async Task UnsetModlogChannelAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            _cachingService.SetModlogChannel(Context.Guild.Id, 0);
            await FollowupAsync("Mod-log disabled");
        }
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        [SlashCommand("nickname", "Changes your nickname")]
        public async Task NicknameAsync(
            [Summary("NewNickname", "The new nickname to set")] string newNickname,
            [Summary("TargetUser", "The user to change nickname for (defaults to yourself)")] IGuildUser targetUser = null)
        {
            await DeferAsync();

            var guild = Context.Guild;
            var botUser = guild.GetUser(Context.Client.CurrentUser.Id);
            var commandUser = Context.User as IGuildUser;

            // If no target specified, default to self
            if (targetUser == null)
                targetUser = commandUser;

            if (botUser.Hierarchy <= targetUser.Hierarchy)
            {
                await FollowupAsync("I cannot change the nickname of someone with a higher or equal role than me.");
                return;
            }
            if (commandUser.Hierarchy <= targetUser.Hierarchy && targetUser != commandUser)
            {
                await FollowupAsync("You cannot change the nickname of someone with a higher or equal role than you.");
                return;
            }

            try
            {
                await targetUser.ModifyAsync(x => x.Nickname = newNickname);
                await FollowupAsync($"Nickname for {targetUser.Mention} changed to **{newNickname}**!");
            }
            catch
            {
                await FollowupAsync("Failed to change nickname. This may be due to role hierarchy or permissions.");
            }
        }
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("slowmode", "Sets the slowmode of a channel to the specified seconds")]
        public async Task SetSlowmodeAsync([Summary("Seconds", "The slowmode interval in seconds")] int seconds)
        {
            if (seconds < 21600)
            {
                await ((ITextChannel)Context.Channel).ModifyAsync(x => x.SlowModeInterval = seconds);
                await Context.Channel.SendMessageAsync($"Slowmode is now set to {seconds} seconds");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Interval must be less than or equal to 6 hours.");
            }
        }
    }
}
