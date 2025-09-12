using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SketchBot.Utils.HelperFunctions;

namespace SketchBot.TypeConverters
{
    public class CalculationConverter : TypeConverter<Calculation>
    {
        private readonly Regex _charEscape;

        public CalculationConverter()
            => _charEscape = new(@"[a-z]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override ApplicationCommandOptionType GetDiscordType()
            => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            string @string = (option.Value as string)!;

            string errorReason = string.Empty;
            if (string.IsNullOrEmpty(@string))
                errorReason = "The expression cannot be empty.";

            if (_charEscape.IsMatch(@string))
                errorReason = "The expression can only contain numbers and operators.";

            return Task.FromResult(TypeConverterResult.FromSuccess(new Calculation(@string, errorReason)));
        }
    }
}
