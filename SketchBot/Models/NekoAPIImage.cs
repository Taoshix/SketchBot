using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.Models
{
    public class NekoAPIImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Rating { get; set; }

        [JsonProperty("color_dominant")]
        [JsonConverter(typeof(NekoAPIColorConverter))]
        public NekoAPIColor ColorDominant { get; set; }

        [JsonProperty("color_palette")]
        [JsonConverter(typeof(NekoAPIColorListConverter))]
        public List<NekoAPIColor> ColorPalette { get; set; }

        [JsonProperty("artist_name")]
        public string ArtistName { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("source_url")]
        public string SourceUrl { get; set; }
    }

    public class NekoAPIColor
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
}
