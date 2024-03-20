using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch_Bot.Models;
using OsuSharp;
using OsuSharp.Interfaces;
using OsuSharp.Models;
using System.Threading.Tasks;
using OsuSharp.Domain;
using Services;

namespace Sketch_Bot.Services
{
    public class OsuService : IOsuService
    {
        private readonly IOsuClient _client;

        public OsuService(IOsuClient client)
        {
            _client = client;
        }

        public async IAsyncEnumerable<IBeatmapset> GetLastRankedBeatmapsetsAsync(int count)
        {
            var builder = new BeatmapsetsLookupBuilder()
                .WithGameMode(GameMode.Osu)
                .WithConvertedBeatmaps()
                .WithCategory(BeatmapsetCategory.Ranked);

            await foreach (var beatmap in _client.EnumerateBeatmapsetsAsync(builder, BeatmapSorting.Ranked_Desc))
            {
                yield return beatmap;

                count--;
                if (count == 0)
                {
                    break;
                }
            }
        }

        public async Task<string> GetUserAvatarUrlAsync(string username)
        {
            var user = await _client.GetUserAsync(username);
            return user.AvatarUrl.ToString();
        }
    }
}
