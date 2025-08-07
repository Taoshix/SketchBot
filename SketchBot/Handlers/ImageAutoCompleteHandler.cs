using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.Handlers
{
    public class ImageAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // Create a collection with suggestions for autocomplete
            IEnumerable<AutocompleteResult> results = new[]
            {
            new AutocompleteResult("Invert", "invert"),
            new AutocompleteResult("Grayscale", "grayscale"),
            new AutocompleteResult("Sepia", "sepia"),
            new AutocompleteResult("Blur", "blur"),
            new AutocompleteResult("Sharpen", "sharpen"),
            new AutocompleteResult("Brightness", "brightness"),
            new AutocompleteResult("Contrast", "contrast"),
            new AutocompleteResult("Glow", "glow"),
            new AutocompleteResult("Upscale", "upscale")
        };

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}
