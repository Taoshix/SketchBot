using Discord;
using Discord.Commands;
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
using Sketch_Bot.Models;
using Urban.NET;
using JikanDotNet;
using Sketch_Bot.Custom_Preconditions;
using System.Reflection;
using Sketch_Bot.Services;

namespace Sketch_Bot.Modules
{
    public class InteractiveModule : InteractiveBase<SocketCommandContext>
    {

        private readonly Jikan _jikan;
        private CachingService _service;

        public InteractiveModule(Jikan jikan, CachingService service)
        {
            _jikan = jikan;
            _service = service;
        }

        [Command("help",RunMode = RunMode.Async)]
        public async Task Test_Paginator()
        {
            string prefix;
            try
            {
                prefix = _service.GetPrefix(Context.Guild.Id);
            }
            catch
            {
                prefix = "?";
            }
            List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>()
            {
                new PaginatedMessage.Page()
                {
                    Title = "Silly Commands",
                    Description = $"{prefix}help - Prints this message" +
                    $"\n{prefix}hello - Says Hi" +
                    $"\n{prefix}status <url> - Run a status check on any website" +
                    $"\n{prefix}8ball <question> - Try your luck!" +
                    $"\n{prefix}dab - Sends a random picture of dabbing" +
                    $"\n{prefix}meme <template>, <top text>, <buttom text> - Generates a meme using the meme template" +
                    $"\n{prefix}expose <user> - Expose someone!" +
                    $"\n{prefix}repeat <message> - Repeats what you typed" +
                    $"\n{prefix}repeattts <message> - Repeats what you typed but in TTS!" +
                    $"\n{prefix}riskage - Gives you a riskage" +
                    $"\n{prefix}riskage spil - Sends a link to the game based on Riskage!" +
                    $"\n{prefix}info - Info" +
                    $"\n{prefix}rate <anything> - Rates something out of 10" +
                    $"\n{prefix}roll <min> <max> - Rolls between <min> and <max>" +
                    $"\n{prefix}jojo - Jojo's bizzare adventure" +
                    $"\n{prefix}pia - Kast sko efter pia" +
                    $"\n{prefix}random - Posts a random message"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Misc Commands",
                    Description = $"\n{prefix}avatar <user> - Gets the avatar of the user" +
                    $"\n{prefix}donate - Sends a link to our Patreon page" +
                    $"\n{prefix}emote <emote> - Enlarges emote" +
                    $"\n{prefix}modlogchannel - Sends a link to the current modlog channel" +
                    $"\n{prefix}welcomechannel - Sends a link to the current welcome channel" +
                    $"\n{prefix}membercount - Tells you how many people are on this server" +
                    $"\n{prefix}serverinfo - Gives info about the server" +
                    $"\n{prefix}userinfo - Gives info about a user" +
                    $"\n{prefix}roleinfo <role> - Gives info about a role" +
                    $"\n{prefix}rolemembers <role> - Gives you the list of members in a specific role" +
                    $"\n{prefix}textowner <message> - Sends a message to the owner of this bot" +
                    $"\n{prefix}invite - Invite me to your server" +
                    $"\n{prefix}upvote - Gives you a link to the bot's upvote page" +
                    $"\n{prefix}urban <word>" +
                    $"\n{prefix}youtube <query>"
                },
                new PaginatedMessage.Page()
                {
                    Title = "MyAnimeList Commands",
                    Description = $"{prefix}anime <query> - Searches Anime from MyAnimeList.net" +
                    $"\n{prefix}manga <query> - Searches Manga from MyAnimeList.net" +
                    $"\n{prefix}mal user <user> Searches a user on MyAnimeList.net"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Music Commands",
                    Description = $"{prefix}play <query> - Searches on YouTube and plays the song" +
                    $"\n{prefix}join - Joins the voicechannel you are currently in" +
                    $"\n{prefix}stop - Stops the music and clears the queue" +
                    $"\n{prefix}skip - Skips the current song" +
                    $"\n{prefix}np - Displays the current song" +
                    $"\n{prefix}queue - Shows the queue" +
                    $"\n{prefix}volume <value> - Sets the volume to the given value (0-150)" +
                    $"\n{prefix}pause - pauses the song" +
                    $"\n{prefix}resume - resumes playing the song"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Animal API Commands",
                    Description = $"{prefix}birb - Posts a random birb" +
                    $"\n{prefix}cat - Posts a random cat" +
                    $"\n{prefix}dog - Posts a random dog" +
                    $"\n{prefix}duck - Posts a random duck" +
                    $"\n{prefix}fox - Posts a random fox"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Image Manipulation Commands",
                    Description = $"Image can be an attachment or a url" +
                    $"\n{prefix}blur <factor> <image> - Blurs the given image by the given factor" +
                    $"\n{prefix}brightness <factor> <image> - Alters the brightness of the given image by the given factor" +
                    $"\n{prefix}contrast <factor> <image> - Alters the contrast of the given image by the given factor" +
                    $"\n{prefix}crop <width> <height> <image> - Crops the image to the given width & height" +
                    $"\n{prefix}pixelate <factor> <image> - Pixelates the given image by the given factor" +
                    $"\n{prefix}resize <width> <height> <image> - Resizes the given image to the given width and height" +
                    $"\n{prefix}saturate <factor> <image> - Alters the saturation of the given image by the given factor" +
                    $"\n{prefix}invert <image> - Inverts the colors of the given image" +
                    $"\n{prefix}oil <image> - Converts the image into oil painting" +
                    $"\n{prefix}grayscale <image> - Puts the given image in grayscale" +
                    $"\n{prefix}flip <image> - Flips the given image upside-down" +
                    $"\n{prefix}sepia <image> - Applies a Sepia filter to the given image" +
                    $"\n{prefix}upscale <image> - Upscales the given image to x2" +
                    $"\n{prefix}rotate <degrees> <image> - Rotates the given image by the given number of degrees" +
                    $"\n{prefix}imagetext <image> <text> - Draws text on the given image" +
                    $"\n{prefix}skew <x> <y> <image> - Skews the given image by x and y degrees"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Moderation Commands",
                    Description = $"{prefix}purge <amount> - Deletes messages in bulk" +
                    $"\n{prefix}kick <user> <reason> - Kicks a user" +
                    $"\n{prefix}ban <user> <reason> - Bans a user" +
                    $"\n{prefix}unban <user> <reason> - Unbans a user" +
                    $"\n{prefix}setprefix <new prefix> - Sets the new prefix for this server" +
                    $"\n{prefix}setwelcome - Use this in the channel you want to have welcome messages in" +
                    $"\n{prefix}unsetwelcome - Disables the welcome messages" +
                    $"\n{prefix}setmodlog - Use this in the channel you want to have the mod-log in" +
                    $"\n{prefix}unsetmodlog - Disables the mod-log" +
                    $"\n{prefix}banword <word> - Will add a word to delete if its written in chat" +
                    $"\n{prefix}unbanword <word> - Will remove a word to delete if its written in chat" +
                    $"\n{prefix}bannedwords - Gives you the list of all of the banned words on the server" +
                    $"\n{prefix}disablelevelmsg - Disables level-up messages" +
                    $"\n{prefix}enablelevelmsg - Enables level-up messages" +
                    $"\n{prefix}slowmode <seconds> - Sets the slowmode of the current channel to the given interval"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Currency Commands",
                    Description = $"{prefix}tokens <target> - See how many tokens you or your target have" +
                    $"\n{prefix}award <user> <amount> <comment> - Award the user with tokens" +
                    $"\n{prefix}awardall <amount> <comment> - Award everyone with tokens!" +
                    $"\n{prefix}daily - Claim your daily tokens!" +
                    $"\n{prefix}pay <target> <amount> <comment> - Pay your target tokens" +
                    $"\n{prefix}leaderboard tokens <page> - Shows the leaderboard for this server" +
                    $"\n{prefix}gamble <amount> - Gamble your tokens!" +
                    $"\n{prefix}stats <user> - Shows the stats of a user" +
                    $"\n{prefix}resetuser <user> - Resets a user's stats"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Leveling Commands",
                    Description = $"{prefix}resetuser <user> - Resets a user's stats" +
                    $"\n{prefix}stats <user> - Shows the stats of a user" +
                    $"\n{prefix}leaderboard leveling <page> - Shows the leaderboard for this server"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Role Commands",
                    Description = $"{prefix}addrole leveling <role> <level>" +
                    $"\n{prefix}removerole leveling <role>"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Osu! Commands",
                    Description = $"{prefix}osu <gamemode> <osu username> - Shows their osu! profile/stats" +
                    $"\n{prefix}osutop <gamemode> <osu username> - Shows their top 10 scores"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Bonus Commands",
                    Description = $"{prefix}frede - Checks if you are Frede or not" +
                    $"\n{prefix}scarce - Check if you are DaRealScarce or not" +
                    $"\n{prefix}frede er sur" +
                    $"\n{prefix}daddy - Daddy" +
                    $"\n{prefix}fredrik - 42" +
                    $"\n{prefix}rune - The hero" +
                    $"\n{prefix}vuk - Memorial" +
                    $"\n{prefix}play <song> - Plays a song" +
                    $"\n{prefix}ping - Pong!" +
                    $"\n{prefix}paginator <words> - Make a Paginated message with the words (seperate pages with , (comma)" +
                    $"\n{prefix}count <number>"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Calculator Commands",
                    Description = $"{prefix}calc+ <numbers to add> - Add numbers" +
                    $"\n{prefix}calc- <numbers to sumtract> - Subtract numbers" +
                    $"\n{prefix}calc* <numbers to multiply> - multiply numbers" +
                    $"\n{prefix}calc/ <numbers to divide> -  Divide numbers" +
                    $"\n{prefix}calcaverage <numbers> - Finds the average value fóf the numbers" +
                    $"\n{prefix}calcarea <radius in degress> - Calculates the area of a circle with radius" +
                    $"\n{prefix}calcomkreds <diameter> - Calculates the circumference of a circle with diameter" +
                    $"\n{prefix}calcsqrt <number> - Finds the squareroot of the number" +
                    $"\n{prefix}calcpow <number> <exponent> - Calculates the power of a number"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Calculator Commands Advanced",
                    Description = $"{prefix}calccos <angel in degress> - Calculates the cosine with an angel" +
                    $"\n{prefix}calcsin <angel in degress> - Calculates the sine with an angel" +
                    $"\n{prefix}calctan <angel in degress> - Calculates the tangent with an angel" +
                    $"\n{prefix}calcacos <cos value [-1,1]> - Calculates the angel in degress with the cosine value" +
                    $"\n{prefix}calcasin <sin value [-1,1]> - Calculates the angel in degress with the sine value" +
                    $"\n{prefix}calcatan <tan value> - Calculates the angel in degress with the tanget value" +
                    $"\n{prefix}calcatan2 <cathetus 1> <cathetus 2> - Calculates the angel in degress with the 2 cathetus'"
                },
                new PaginatedMessage.Page()
                {
                    Title = "Top Secret Developer Section",
                    Description = "Nothing to see here!"
                },
            };
            PaginatedMessage message = new PaginatedMessage
            {
                Color = new Color(0, 0, 255),
                Pages = pages
            };
            //var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };
            var reactions = new ReactionList
            {
                First = true,
                Last = true,
                Forward = true,
                Backward = true,
                Jump = true
            };
            await PagedReplyAsync(message, reactions);
        }
        [Command("paginator",RunMode = RunMode.Async)]
        public async Task paginator([Remainder] string str = "")
        {
            string[] words = str.Split(",");
            if (!words.Any())
            {
                await ReplyAsync(Context.User.Mention + " You gotta give me something to paginate");
            }
            else
            {
                var Pages = new List<PaginatedMessage.Page>();
                foreach (var word in words)
                {
                    Pages.Add(new PaginatedMessage.Page()
                    {
                        Title = $"{Context.User.Username}'s Paginator",
                        Description = $"{word}"
                    });
                }
                var message = new PaginatedMessage
                {
                    Title = "Your paginator",
                    Color = new Color(0, 0, 255),
                    Content = "You made these pages containing one word each unless you seperated it with a comma" +
                    "\n\nOnly the user who sent the command can use the paginator.",
                    Pages = Pages
                };
                var reactions = new ReactionList
                {
                    First = true,
                    Last = true,
                    Forward = true,
                    Backward = true,
                    Jump = true
                };
                await PagedReplyAsync(message, reactions);
            }
        }

        [RequireContext(ContextType.Guild)]
        [Command("rolemembers", RunMode = RunMode.Async)]
        public async Task rolemembers([Remainder] SocketRole role)
        {
            var members = role.Members.OrderBy(o => o.Nickname ?? o.Username).Select(x => x.Mention).ToList();
            if (members.Count <= 0)
            {
                await ReplyAsync($"{role.Name} has 0 members");
            }
            else
            {
                var memberStrings = members.ChunkBy(50);
                List<string> pages = new List<string>();
                List<List<string>> pages2 = new List<List<string>>();
                foreach (var list in memberStrings)
                {
                    for (int i = 0; i < list.Count; i += 2)
                    {
                        pages.Add(string.Join("\n", string.Join(" ", list.Skip(i).Take(2))));
                    }

                    
                    //pages.Add(String.Join("\n", list));
                }

                pages2 = pages.ChunkBy(25);
                List<string> pages3 = new List<string>();
                foreach (var list2 in pages2)
                {
                    pages3.Add(string.Join("\n", list2));
                }
                EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = role.Name
                };
                var pages4 = new List<PaginatedMessage.Page>();
                foreach(var page in pages3)
                {
                    pages4.Add(new PaginatedMessage.Page()
                    {
                        Title = $"{role.Members.Count()} members (showing max 50 per page)",
                        Description = page
                    });
                }
                PaginatedMessage msg = new PaginatedMessage
                {
                    Title = $"{role.Members.Count()} members (showing max 50 per page)",
                    Color = role.Color,
                    Pages = pages4,
                    Author = authorBuilder
                };
                var reactions = new ReactionList
                {
                    First = true,
                    Last = true,
                    Forward = true,
                    Backward = true,
                    Jump = true
                };
                await PagedReplyAsync(msg, reactions);
            }
        }
        [Alias("define")]
        [Command("urban", RunMode = RunMode.Async)]
        public async Task urban([Remainder] string word)
        {
            if (Context.Guild.Id == 264445053596991498 && !((ITextChannel) Context.Channel).IsNsfw)
            {
                await ReplyAsync("NSFW channel == false");
            }
            else
            {
                try
                {
                    UrbanService client = new UrbanService();
                    var data = await client.Data(word);
                    var Pages = new List<PaginatedMessage.Page>();
                    foreach (var item in data.List)
                    {
                        /*
                        myList.Add(item.Definition + "\n\n" +
                                   item.Example +
                                   "\n\n\\👍" + item.ThumbsUp + " \\👎" + item.ThumbsDown);
                                   */
                        Pages.Add(new PaginatedMessage.Page()
                        {
                            Title = item.Word,
                            Fields =
                            {
                                new EmbedFieldBuilder()
                                {
                                    Name = "Definition",
                                    Value = item.Definition
                                },
                                new EmbedFieldBuilder()
                                {
                                    Name = "Example",
                                    Value = item.Example
                                },
                                new EmbedFieldBuilder()
                                {
                                    Name = "Rating",
                                    Value = "\n\n\\👍" + item.ThumbsUp + " \\👎" + item.ThumbsDown
                                }
                            },
                        });
                    }
                    
                    var paginator = new PaginatedMessage
                    {
                        Color = new Color(0, 0, 255),
                        Pages = Pages
                    };

                    var reactions = new ReactionList
                    {
                        First = true,
                        Last = true,
                        Forward = true,
                        Backward = true,
                        Jump = true
                    };
                    await PagedReplyAsync(paginator, reactions);
                }
                catch (Exception e)
                {
                    await ReplyAsync(e.Message);
                }
            }
        }
        [Ratelimit(1, 5, Measure.Seconds)]
        [Command("anime", RunMode = RunMode.Async)]
        public async Task anime([Remainder] string name)
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    var anime = await _jikan.SearchAnimeAsync(name);
                    if(anime == null)
                    {
                        await ReplyAsync("The API didn't return anything :(");
                        return;
                    }
                    var results = anime.Data;
                    if (results.Any())
                    {
                        var Pages = new List<PaginatedMessage.Page>();
                        int NSFW = 0;
                        foreach (var result in results)
                        {
                            if ((result.Rating == "Rx" && !(Context.Channel as ITextChannel).IsNsfw) || result == null)
                            {
                                NSFW++;
                                continue;
                            }
                            try
                            {
                                Pages.Add(new PaginatedMessage.Page()
                                {
                                    
                                    Description = result.Synopsis ?? "null",
                                    Fields =
                                    {
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Score",
                                            Value = $"{result.Score}" ?? "N/A",
                                            IsInline = true
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Airing",
                                            Value = $"{result.Airing}" ?? "N/A",
                                            IsInline = true
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Type",
                                            Value = $"{result.Type}" ?? "N/A",
                                            IsInline = true
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Episodes",
                                            Value = $"{result.Episodes}" ?? "N/A",
                                            IsInline = true
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Start Date",
                                            Value = $"{result.Aired.From.Value.Day}/{result.Aired.From.Value.Month}/{result.Aired.From.Value.Year}" ?? "N/A",
                                            IsInline = true
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "End Date",
                                            Value = $"{result.Aired.To.Value.Day}/{result.Aired.To.Value.Month}/{result.Aired.To.Value.Year}" ?? "N/A",
                                            IsInline = true
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Rated",
                                            Value = $"{result.Rating}",
                                            IsInline = true
                                        }
                                    },
                                    Author = new EmbedAuthorBuilder
                                    {
                                        Name = $"{result.Title}" ?? "null",
                                        Url = $"{result.Url}",
                                        IconUrl = $"{result.Images.JPG.ImageUrl}"
                                    },
                                    ImageUrl = result.Images.JPG.ImageUrl ?? "",
                                });
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                        var paginator = new PaginatedMessage
                        {
                            Color = new Color(0, 0, 255),
                            Pages = Pages,
                            TimeStamp = DateTime.Now,
                            Content = NSFW == 0 ? null : $"{NSFW} NSFW results hidden. To view those, go to a NSFW channel and try again."
                        };

                        var reactions = new ReactionList
                        {
                            First = true,
                            Last = true,
                            Forward = true,
                            Backward = true,
                            Jump = true
                        };
                        await PagedReplyAsync(paginator, reactions);

                    }
                    else
                    {
                        await ReplyAsync("The API didn't return anything :(");
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
        }
        [Ratelimit(1, 5, Measure.Seconds)]
        [Command("manga", RunMode = RunMode.Async)]
        public async Task manga([Remainder] string name)
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    var anime = await _jikan.SearchMangaAsync(name);
                    if (anime == null)
                    {
                        await ReplyAsync("The API didn't return anything :(");
                        return;
                    }
                    var results = anime.Data;
                    if (results.Any())
                    {
                        var Pages = new List<PaginatedMessage.Page>();

                        foreach (var result in results)
                        {
                            try
                            {
                                Pages.Add(new PaginatedMessage.Page()
                                {
                                    
                                    Description = result.Synopsis ?? "null",
                                    Fields =
                                {
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Score",
                                        Value = $"{result.Score}" ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Publishing",
                                        Value = $"{result.Publishing}" ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Type",
                                        Value = $"{result.Type}" ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Chapters",
                                        Value = $"{result.Chapters}" ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Volumes",
                                        Value = $"{result.Volumes}" ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Start Date",
                                        Value = $"{result.Published.From.Value.Day}/{result.Published.From.Value.Month}/{result.Published.From.Value.Year}" ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "End Date",
                                        Value = $"{result.Published.To.Value.Day}/{result.Published.To.Value.Month}/{result.Published.To.Value.Year}" ?? "N/A",
                                        IsInline = true
                                    }
                                },
                                    Author = new EmbedAuthorBuilder
                                    {
                                        Name = $"{result.Title}" ?? "null",
                                        Url = $"{result.Url}",
                                        IconUrl = $"{result.Images.JPG.ImageUrl}"
                                    },
                                    ImageUrl = result.Images.JPG.ImageUrl ?? "",
                                });
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                        var paginator = new PaginatedMessage
                        {
                            Color = new Color(0, 0, 255),
                            Pages = Pages,
                            TimeStamp = DateTime.Now
                        };

                        var reactions = new ReactionList
                        {
                            First = true,
                            Last = true,
                            Forward = true,
                            Backward = true,
                            Jump = true
                        };
                        await PagedReplyAsync(paginator, reactions);

                    }
                    else
                    {
                        await ReplyAsync("The API didn't return anything :(");
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.ToString());
            }
        }
        [Group("mal")]
        public class MyAnimeListModule : InteractiveBase<SocketCommandContext>
        {
            private readonly Jikan _jikan;

            public MyAnimeListModule(Jikan jikan)
            {
                _jikan = jikan;
            }
            [Priority(-1)]
            [Command]
            public async Task MyAsyncTask()
            {
                await ReplyAsync("See the help section with MyAnimeList commands");
            }
            [Ratelimit(1, 5, Measure.Seconds, RatelimitFlags.ApplyPerGuild)]
            [Alias("profile")]
            [Priority(1)]
            [Command("user", RunMode = RunMode.Async)]
            [Summary("Searches for a user on MyAnimeList.net")]
            public async Task UserAsync([Remainder] string username)
            {
                using (Context.Channel.EnterTypingState())
                {
                    var user = await _jikan.GetUserProfileAsync(username);
                    if (user != null)
                    {
                        var about = await _jikan.GetUserAboutAsync(username);
                        var friends = await _jikan.GetUserFriendsAsync(username);
                        var history = await _jikan.GetUserHistoryAsync(username);
                        var favorites = await _jikan.GetUserFavoritesAsync(username);
                        var stats = await _jikan.GetUserStatisticsAsync(username);
                        string imageUrl = favorites.Data.Anime.Count < 0 ? favorites.Data.Anime.FirstOrDefault().Images.JPG.ImageUrl : "https://cdn.discordapp.com/attachments/422745236100349973/758271903336759306/unknown.png";
                        PaginatedMessage message = new PaginatedMessage()
                        {
                            Author = new EmbedAuthorBuilder()
                            {
                                Name = user.Data.Username,
                                IconUrl = user.Data.Images.JPG.ImageUrl,
                                Url = user.Data.Url
                            },
                            Color = new Color(0, 0, 255),
                            TimeStamp = DateTime.Now,
                            ThumbnailUrl = user.Data.Images.JPG.ImageUrl,
                            Pages = new List<PaginatedMessage.Page>()
                        {
                            new PaginatedMessage.Page()
                            {
                                Title = "Profile",
                                Description = about.Data.About.Replace("<br>",""),
                                
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "User Id",
                                        Value = user.Data.MalId,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Gender",
                                        Value = string.IsNullOrEmpty($"{user.Data.Gender}") ? "N/A" : $"{user.Data.Gender}",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Location",
                                        Value = string.IsNullOrEmpty($"{user.Data.Location}") ? "N/A" : $"{user.Data.Location}",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Last Online",
                                        Value = user.Data.LastOnline.ToString() ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Join Date",
                                        Value = user.Data.Joined.ToString() ?? "N/A",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Birthday",
                                        Value = string.IsNullOrEmpty($"{user.Data.Birthday}") ? "N/A" : $"{user.Data.Birthday}",
                                        IsInline = true
                                    }
                                }
                            },
                            new PaginatedMessage.Page()
                            {
                                Title = "Anime Statistics",
                                
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Total Anime Entries",
                                        Value = stats.Data.AnimeStatistics.TotalEntries ?? 0
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Total Episodes Watched",
                                        Value = stats.Data.AnimeStatistics.EpisodesWatched ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Mean Score",
                                        Value = stats.Data.AnimeStatistics.MeanScore ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Completed",
                                        Value = stats.Data.AnimeStatistics.Completed ?? 0
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Watching",
                                        Value = stats.Data.AnimeStatistics.Watching ?? 0
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Plan to Watch",
                                        Value = stats.Data.AnimeStatistics.PlanToWatch ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "On Hold",
                                        Value = stats.Data.AnimeStatistics.OnHold ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Dropped",
                                        Value = stats.Data.AnimeStatistics.Dropped ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Rewatched",
                                        Value = stats.Data.AnimeStatistics.Rewatched ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Days Watched",
                                        Value = stats.Data.AnimeStatistics.DaysWatched ?? 0,
                                    },
                                }
                            },
                            new PaginatedMessage.Page()
                            {
                                Title = "Manga Statistics",
                                
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Total Manga Entries",
                                        Value = stats.Data.MangaStatistics.TotalEntries ?? 0,
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Total Chapters Read",
                                        Value = stats.Data.MangaStatistics.ChaptersRead ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Mean Score",
                                        Value = stats.Data.MangaStatistics.MeanScore ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Completed",
                                        Value = stats.Data.MangaStatistics.Completed ?? 0,
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Reading",
                                        Value = stats.Data.MangaStatistics.Reading ?? 0
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Plan to Read",
                                        Value = stats.Data.MangaStatistics.PlanToRead ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "On Hold",
                                        Value = stats.Data.MangaStatistics.OnHold ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Dropped",
                                        Value = stats.Data.MangaStatistics.Dropped ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Reread",
                                        Value = stats.Data.MangaStatistics.Reread ?? 0,
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Days Read",
                                        Value = stats.Data.MangaStatistics.DaysRead ?? 0
                                    },
                                }
                            },
                            new PaginatedMessage.Page()
                            {
                                Title = "Favorites",
                                
                                ImageUrl = imageUrl,
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Favorite Anime",
                                        Value = HelperFunctions.CutString($"{(string.IsNullOrEmpty($"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.Anime.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.Anime.Take(10).Select(x => x.Url), "(", ")")))}") ? "Empty" : $"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.Anime.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.Anime.Take(10).Select(x => x.Url), "(", ")")))}")}", 1024) ?? "null",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Favorite Manga",
                                        Value = HelperFunctions.CutString($"{(string.IsNullOrEmpty($"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.Manga.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.Manga.Take(10).Select(x => x.Url), "(", ")")))}") ? "Empty" : $"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.Manga.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.Manga.Take(10).Select(x => x.Url), "(", ")")))}")}", 1024) ?? "null",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Favorite Character(s)",
                                        Value = HelperFunctions.CutString($"{(string.IsNullOrEmpty($"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.Characters.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.Characters.Take(10).Select(x => x.Url), "(", ")")))}") ? "Empty" : $"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.Characters.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.Characters.Take(10).Select(x => x.Url), "(", ")")))}")}", 1024) ?? "null",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Favorite People",
                                        Value = HelperFunctions.CutString($"{(string.IsNullOrEmpty($"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.People.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.People.Take(10).Select(x => x.Url), "(", ")")))}") ? "Empty" : $"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(favorites.Data.People.Take(10).Select(x => x.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(favorites.Data.People.Take(10).Select(x => x.Url), "(", ")")))}")}", 1024) ?? "null",
                                        IsInline = true
                                    }
                                }
                            },
                            new PaginatedMessage.Page()
                            {
                                Title = "User History",
                                
                                Fields = new List<EmbedFieldBuilder>()
                                {
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Name",
                                        Value = HelperFunctions.CutString($"{(string.IsNullOrEmpty($"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(history.Data.Take(10).Select(x => x.Metadata.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(history.Data.Take(10).Select(x => x.Metadata.Url), "(", ")")))}") ? "Empty" : $"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(history.Data.Take(10).Select(x => x.Metadata.Name), "[", "]"), HelperFunctions.AddToStartAndEnding(history.Data.Take(10).Select(x => x.Metadata.Url), "(", ")")))}")}", 1024)  ?? "null",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Increment",
                                        Value = HelperFunctions.CutString($"{(string.IsNullOrEmpty(string.Join("\n", history.Data.Take(10).Select(x => x.Increment))) ? "Empty" : string.Join("\n", history.Data.Take(10).Select(x => x.Increment)))}", 1024) ?? "null",
                                        IsInline = true
                                    },
                                    new EmbedFieldBuilder()
                                    {
                                        Name = "Date",
                                        Value = HelperFunctions.CutString($"{(string.IsNullOrEmpty(string.Join("\n", history.Data.Take(10).Select(x => x.Date))) ? "Empty" : string.Join("\n", history.Data.Take(10).Select(x => x.Date)))}", 1024) ?? "null",
                                        IsInline = true
                                    },
                                }
                            },
                            new PaginatedMessage.Page()
                            {
                                Title = "Friends",
                                
                                Description = $"{(friends?.Data.Count == null ? 0 : friends.Data.Count)} friends\n" +
                                HelperFunctions.CutString($"{(string.IsNullOrEmpty($"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(friends.Data.Take(15).Select(x => x.User.Username), "[", "]"), HelperFunctions.AddToStartAndEnding(friends.Data.Take(15).Select(x => x.User.Url), "(", ")")))}") ? "Empty" : $"{string.Join("\n", HelperFunctions.MergeListStrings(HelperFunctions.AddToStartAndEnding(friends.Data.Take(15).Select(x => x.User.Username), "[", "]"), HelperFunctions.AddToStartAndEnding(friends.Data.Take(15).Select(x => x.User.Url), "(", ")")))}")}", 1024)  ?? "null",
                            },
                        }
                        };
                        await PagedReplyAsync(message, new ReactionList()
                        {
                            First = true,
                            Last = true,
                            Forward = true,
                            Backward = true,
                            Jump = true
                        });
                    }
                    else
                    {
                        await ReplyAsync("The API didn't return anything :(");
                    }
                }
            }
        }
    }
}
