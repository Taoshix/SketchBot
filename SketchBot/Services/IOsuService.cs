using OsuSharp.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SketchBot.Services
{
    internal interface IOsuService
    {
        IAsyncEnumerable<IBeatmapset> GetLastRankedBeatmapsetsAsync(int count);
        Task<string> GetUserAvatarUrlAsync(string username);
    }
}