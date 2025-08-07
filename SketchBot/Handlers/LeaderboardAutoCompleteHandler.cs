using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.Handlers
{
    public class LeaderboardAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // Create a collection with suggestions for autocomplete
            IEnumerable<AutocompleteResult> results = new[]
            {
            new AutocompleteResult("Levels", "leveling"),
            new AutocompleteResult("Tokens", "tokens")
        };

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}
