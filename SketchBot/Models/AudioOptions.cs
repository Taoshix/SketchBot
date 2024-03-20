using System;
using System.Collections.Generic;
using System.Text;
using Discord;


namespace Sketch_Bot.Models
{
    public class AudioOptions
    {
        public bool Shuffle { get; set; }
        public bool RepeatTrack { get; set; }
        public IUser Summoner { get; set; }
    }
}
