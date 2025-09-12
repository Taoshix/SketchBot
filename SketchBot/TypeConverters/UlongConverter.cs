using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.TypeConverters
{
    internal sealed class UlongConverter : TypeConverter<ulong>
    {
        public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;
        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            if (ulong.TryParse((string)option.Value, out var result))
                return Task.FromResult(TypeConverterResult.FromSuccess(result));

            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"Value {option.Value} cannot be converted to UInt64"));
        }
    }
}