using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using SketchBot.Custom_Preconditions;
using SketchBot.Database;
using SketchBot.Services;
using SketchBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.InteractionBasedModules
{
    public class EconomyModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CachingService _cachingService;
        private Random _rand;

        public EconomyModule(CachingService Cache)
        {
            _cachingService = Cache;
        }

        [RequireContext(ContextType.Guild)]
        [UserCommand("stats")]
        public async Task UserStatsAsync(IGuildUser user)
        {
            await DeferAsync();
            if (user.IsBot)
            {
                await FollowupAsync("Bots don't have stats");
                return;
            }
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var blacklistCheck = _cachingService.GetBlackList().Contains(user.Id);
            if (blacklistCheck)
            {
                await FollowupAsync("This user is blacklisted from using this command.");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            var displayName = user.Nickname ?? user.DisplayName;
            StatsDB.CreateTable(Context.Guild.Id);
            var userCheckResult = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);

            if (!userCheckResult)
            {
                _cachingService.SetupUserInDatabase(user.Guild.Id, user as SocketGuildUser);
            }

            var userStats = StatsDB.GetUserStats(user);
            embed.Title = "Stats for " + displayName;
            embed.Description = userStats.Tokens + " tokens:small_blue_diamond:" +
                "\nLevel " + userStats.Level +
                "\nXP " + userStats.XP + " out of " + XP.caclulateNextLevel(userStats.Level);
            await FollowupAsync("", embed: embed.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("stats", "Display a user's level and token")]
        public async Task SlashUserStatsAsync([Summary("User", "Specifies the user you want to stat check. Defaults to yourself")]IGuildUser user = null)
        {
            await DeferAsync();
            user ??= Context.User as IGuildUser;
            if (user.IsBot)
            {
                await FollowupAsync("Bots don't have stats");
                return;
            }
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var blacklistCheck = _cachingService.GetBlackList().Contains(user.Id);
            if (blacklistCheck)
            {
                await FollowupAsync("This user is blacklisted from using this command.");
                return;
            }
            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255)
            };
            var displayName = user.Nickname ?? user.DisplayName;
            StatsDB.CreateTable(Context.Guild.Id);
            var userCheckResult = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);

            if (!userCheckResult)
            {
                _cachingService.SetupUserInDatabase(Context.Guild.Id, user as SocketGuildUser);
            }

            var userStats = StatsDB.GetUserStats(user);
            embed.Title = "Stats for " + displayName;
            embed.Description = userStats.Tokens + " tokens:small_blue_diamond:" +
                "\nLevel " + userStats.Level +
                "\nXP " + userStats.XP + " out of " + XP.caclulateNextLevel(userStats.Level);
            var builtEmbed = embed.Build();
            await FollowupAsync("", [builtEmbed]);
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("gamble", "Gamble tokens")]
        public async Task GambleAsync([Summary("Amount", "The number of tokens you want to gamble")] long amount)
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }

            var user = Context.User as IGuildUser;
            var userStats = StatsDB.GetUserStats(user);
            long currentTokens = userStats?.Tokens ?? 0;

            if (amount > currentTokens)
            {
                await FollowupAsync("You don't have enough tokens");
                return;
            }
            if (amount < 1)
            {
                await FollowupAsync("The minimum amount of tokens is 1");
                return;
            }

            _rand = new Random();
            int RNG = _rand.Next(0, 100);
            bool won = RNG >= 53;

            if (won)
            {
                StatsDB.AddTokens(user, amount);
                currentTokens += amount;
            }
            else
            {
                StatsDB.RemoveTokens(user, amount);
                currentTokens -= amount;
            }

            var embedBuilder = new EmbedBuilder()
            {
                Title = won ? "You won!" : "You lost!",
                Description = $"You gambled {amount} tokens and rolled {RNG} and {(won ? "won" : "lost")}!\nYou now have {currentTokens} tokens!",
                Color = new Color(0, 0, 255)
            }.WithAuthor(author =>
            {
                author.Name = $"Gambling results - {Context.User.Username}";
                author.IconUrl = Context.User.GetAvatarUrl();
            });

            await FollowupAsync("", null, false, false, null, null, null, embedBuilder.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("gambleall", "Gambles all of your tokens")]
        public async Task GambleAllAsync()
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }

            var user = Context.User as IGuildUser;
            var userStats = StatsDB.GetUserStats(user);
            long amount = userStats?.Tokens ?? 0;
            var currentTokens = amount;

            if (amount < 1)
            {
                await FollowupAsync("You dont have any tokens!");
                return;
            }

            _rand = new Random();
            int RNG = _rand.Next(0, 100);
            bool won = RNG >= 53;

            if (won)
            {
                StatsDB.AddTokens(user, amount);
                currentTokens += amount;
            }
            else
            {
                StatsDB.RemoveTokens(user, amount);
                currentTokens -= amount;
            }

            var embedBuilder = new EmbedBuilder()
            {
                Title = won ? "You won!" : "You lost!",
                Description = $"You gambled {amount} tokens and rolled {RNG} and {(won ? "won" : "lost")}!\nYou now have {currentTokens} tokens!",
                Color = new Color(0, 0, 255)
            }.WithAuthor(author =>
            {
                author.Name = $"Gambling results - {Context.User.Username}";
                author.IconUrl = Context.User.GetAvatarUrl();
            });

            await FollowupAsync("", null, false, false, null, null, null, embedBuilder.Build());
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("award", "Give someone tokens")]
        public async Task AwardTokensAsync(
            [Summary("User", "The user to award tokens to")] IGuildUser guildUser,
            [Summary("Tokens", "The number of tokens to award")] int tokens,
            [Summary("Comment", "A comment to include with the award")] string comment = ""
        )
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            var name = guildUser.Nickname ?? guildUser.DisplayName;
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                var userExists = _cachingService.IsInDatabase(Context.Guild.Id, guildUser.Id);
                if (userExists)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 0, 255)
                    };
                    StatsDB.AddTokens(guildUser, tokens);
                    embed.Title = name + " was awarded " + tokens + " tokens!";
                    embed.Description = comment;
                    var builtEmbed = embed.Build();
                    await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
                }
                else
                {
                    await FollowupAsync("This user is not in the database");
                }
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        [Ratelimit(1, 5, Measure.Minutes, RatelimitFlags.ApplyPerGuild | RatelimitFlags.NoLimitForDevelopers)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("awardall", "Give everyone on the server some tokens")]
        public async Task AwardTokensToEveryoneAsync(
            [Summary("Tokens", "The number of tokens to award to everyone")] int tokens,
            [Summary("Comment", "A comment to include with the award")] string comment = ""
        )
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            if (((IGuildUser)Context.User).GuildPermissions.ManageGuild || Context.User.Id == 135446225565515776 || Context.User.Id == 208624502878371840)
            {
                await Context.Guild.DownloadUsersAsync();
                var users = Context.Guild.Users;
                foreach (var user in users)
                {
                    var isUserInDatabase = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);
                    if (!isUserInDatabase)
                    {
                        _cachingService.SetupUserInDatabase(Context.Guild.Id, user);
                    }
                    StatsDB.AddTokens(user, tokens);
                }
                var embed = new EmbedBuilder()
                {
                    Color = new Color(0, 0, 255)
                };

                embed.Title = "All users were awarded " + tokens + " tokens!";
                embed.Description = comment;
                var builtEmbed = embed.Build();
                await FollowupAsync("", null, false, false, null, null, null, builtEmbed);
            }
            else
            {
                await FollowupAsync("You do not have permission!");
            }
        }
        [RequireContext(ContextType.Guild)]
        [SlashCommand("pay", "Pay someone else some of your tokens")]
        public async Task PayTokensAsync(
            [Summary("User", "The user you want to pay tokens to")] IGuildUser usertopay,
            [Summary("Amount", "The number of tokens to pay")] int amount,
            [Summary("Comment", "A comment to include with the payment")] string comment = "No comment"
        )
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }

            var user = Context.User as SocketGuildUser;
            var userToPay = usertopay as SocketGuildUser;
            if (user == null || userToPay == null)
            {
                await Context.Guild.DownloadUsersAsync();
                user = Context.User as SocketGuildUser;
                userToPay = usertopay as SocketGuildUser;
            }

            if (_cachingService.GetBlackList().Contains(usertopay.Id))
            {
                await FollowupAsync("You can't pay blacklisted users!");
                return;
            }

            bool userInDb = _cachingService.IsInDatabase(Context.Guild.Id, user.Id);
            bool userToPayInDb = _cachingService.IsInDatabase(Context.Guild.Id, userToPay.Id);

            if (!userInDb)
            {
                _cachingService.SetupUserInDatabase(Context.Guild.Id, user);
            }

            if (!userToPayInDb)
            {
                _cachingService.SetupUserInDatabase(Context.Guild.Id, userToPay);
            }

            var userStats = StatsDB.GetUserStats(user);
            if (amount <= 0)
            {
                await FollowupAsync("Don't attempt to steal tokens from people!");
                return;
            }

            if (userStats.Tokens < amount)
            {
                await FollowupAsync("You don't have enough tokens to pay.");
                return;
            }

            StatsDB.RemoveTokens(user, amount);
            StatsDB.AddTokens(userToPay, amount);

            var embed = new EmbedBuilder()
            {
                Color = new Color(0, 0, 255),
                Description = $"{user.Mention} has paid {usertopay.Mention} {amount} tokens!\n{comment}"
            }.Build();

            await FollowupAsync("", null, false, false, null, null, null, embed);
        }
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [SlashCommand("addrole", "Add a role for leveling")]
        public async Task AddRoleAsync(
            [Summary("Role", "The role to award for reaching a level")] IRole role,
            [Summary("Level", "The level required to receive the role")] int level
        )
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
        public async Task RemoveRoleAsync(
            [Summary("Role", "The role to remove from level rewards")] IRole role
        )
        {
            await DeferAsync();
            if (!_cachingService._dbConnected)
            {
                await FollowupAsync("Database is down, please try again later");
                return;
            }
            ServerSettingsDB.CreateTableRole(Context.Guild.Id);
            ServerSettingsDB.RemoveRole(Context.Guild.Id, role.Id);
            await FollowupAsync(role.Name + " has been removed from levelup rewards");
        }
    }
}
