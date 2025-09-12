using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchBot.Utils
{
    /// <summary>
    /// Helper methods for the lists.
    /// </summary>
    public static class ListExtensions
    {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public static IEnumerable<IEnumerable<T>> GroupSelect<T>(this IEnumerable<T> list, int groupSize)
        {
            return list
                .Select((t, i) => new { t, i })
                .GroupBy(x => x.i / groupSize, x => x.t);
        }
        public static TimeSpan TotalTime(this IEnumerable<TimeSpan> TheCollection)
        {
            int i = 0;
            int TotalSeconds = 0;

            var ArrayDuration = TheCollection.ToArray();

            for (i = 0; i < ArrayDuration.Length; i++)
            {
                TotalSeconds = (int)ArrayDuration[i].TotalSeconds + TotalSeconds;
            }

            return TimeSpan.FromSeconds(TotalSeconds);
        }
    }
}
