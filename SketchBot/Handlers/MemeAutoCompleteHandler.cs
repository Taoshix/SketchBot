using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchBot.Handlers
{
    public class MemeAutoCompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            // Create a collection with suggestions for autocomplete
            IEnumerable<AutocompleteResult> results = new[]
            {
            new AutocompleteResult("Drake Hotline Bling", "Drake Hotline Bling"),
            new AutocompleteResult("Distracted Boyfriend", "Distracted Boyfriend"),
            new AutocompleteResult("Two Buttons", "Two Buttons"),
            new AutocompleteResult("Left Exit 12 Off Ramp", "Left Exit 12 Off Ramp"),
            new AutocompleteResult("Change My Mind", "Change My Mind"),
            new AutocompleteResult("Batman Slapping Robin", "Batman Slapping Robin"),
            new AutocompleteResult("UNO Draw 25 Cards", "UNO Draw 25 Cards"),
            new AutocompleteResult("Running Away Balloon", "Running Away Balloon"),
            new AutocompleteResult("One Does Not Simply", "One Does Not Simply"),
            new AutocompleteResult("Waiting Skeleton", "Waiting Skeleton"),
            new AutocompleteResult("Expanding Brain", "Expanding Brain"),
            new AutocompleteResult("Mocking Spongebob", "Mocking Spongebob"),
            new AutocompleteResult("Disaster Girl", "Disaster Girl"),
            new AutocompleteResult("Boardroom Meeting Suggestion", "Boardroom Meeting Suggestion"),
            new AutocompleteResult("Woman Yelling At Cat", "Woman Yelling At Cat"),
            new AutocompleteResult("X, X Everywhere", "X, X Everywhere"),
            new AutocompleteResult("Ancient Aliens", "Ancient Aliens"),
            new AutocompleteResult("Buff Doge vs. Cheems", "Buff Doge vs. Cheems"),
            new AutocompleteResult("Gru's Plan", "Gru's Plan"),
            new AutocompleteResult("Roll Safe Think About It", "Roll Safe Think About It"),
            new AutocompleteResult("Bernie I Am Once Again Asking For Your Support", "Bernie I Am Once Again Asking For Your Support"),
            new AutocompleteResult("Blank Nut Button", "Blank Nut Button"),
            new AutocompleteResult("Futurama Fry", "Futurama Fry"),
            new AutocompleteResult("Tuxedo Winnie The Pooh", "Tuxedo Winnie The Pooh"),
            new AutocompleteResult("Epic Handshake", "Epic Handshake"),
            new AutocompleteResult("Inhaling Seagull", "Inhaling Seagull"),
            new AutocompleteResult("Surprised Pikachu", "Surprised Pikachu"),
            new AutocompleteResult("Leonardo Dicaprio Cheers", "Leonardo Dicaprio Cheers"),
            new AutocompleteResult("Sad Pablo Escobar", "Sad Pablo Escobar"),
            new AutocompleteResult("Hide the Pain Harold", "Hide the Pain Harold"),
            new AutocompleteResult("Is This A Pigeon", "Is This A Pigeon"),
            new AutocompleteResult("The Scroll Of Truth", "The Scroll Of Truth"),
            new AutocompleteResult("The Rock Driving", "The Rock Driving"),
            new AutocompleteResult("The Most Interesting Man In The World", "The Most Interesting Man In The World"),
            new AutocompleteResult("Monkey Puppet", "Monkey Puppet"),
            new AutocompleteResult("Y'all Got Any More Of That", "Y'all Got Any More Of That"),
            new AutocompleteResult("Panik Kalm Panik", "Panik Kalm Panik"),
            new AutocompleteResult("Always Has Been", "Always Has Been"),
            new AutocompleteResult("Oprah You Get A", "Oprah You Get A"),
            new AutocompleteResult("Marked Safe From", "Marked Safe From"),
            new AutocompleteResult("Doge", "Doge"),
            new AutocompleteResult("Bad Luck Brian", "Bad Luck Brian"),
            new AutocompleteResult("Third World Skeptical Kid", "Third World Skeptical Kid"),
            new AutocompleteResult("American Chopper Argument", "American Chopper Argument"),
            new AutocompleteResult("First World Problems", "First World Problems"),
            new AutocompleteResult("I Bet He's Thinking About Other Women", "I Bet He's Thinking About Other Women"),
            new AutocompleteResult("They're The Same Picture", "They're The Same Picture"),
            new AutocompleteResult("Grandma Finds The Internet", "Grandma Finds The Internet"),
            new AutocompleteResult("Trump Bill Signing", "Trump Bill Signing"),
            new AutocompleteResult("Finding Neverland", "Finding Neverland"),
            new AutocompleteResult("Unsettled Tom", "Unsettled Tom"),
            new AutocompleteResult("Evil Kermit", "Evil Kermit"),
            new AutocompleteResult("This Is Where I'd Put My Trophy If I Had One", "This Is Where I'd Put My Trophy If I Had One"),
            new AutocompleteResult("This Is Fine", "This Is Fine"),
            new AutocompleteResult("Success Kid", "Success Kid"),
            new AutocompleteResult("Y U No", "Y U No"),
            new AutocompleteResult("Star Wars Yoda", "Star Wars Yoda"),
            new AutocompleteResult("Who Killed Hannibal", "Who Killed Hannibal"),
            new AutocompleteResult("That Would Be Great", "That Would Be Great"),
            new AutocompleteResult("Bike Fall", "Bike Fall"),
            new AutocompleteResult("Brace Yourselves X is Coming", "Brace Yourselves X is Coming"),
            new AutocompleteResult("Spongebob Ight Imma Head Out", "Spongebob Ight Imma Head Out"),
            new AutocompleteResult("Grumpy Cat", "Grumpy Cat"),
            new AutocompleteResult("Creepy Condescending Wonka", "Creepy Condescending Wonka"),
            new AutocompleteResult("But That's None Of My Business", "But That's None Of My Business"),
            new AutocompleteResult("Clown Applying Makeup", "Clown Applying Makeup"),
            new AutocompleteResult("X All The Y", "X All The Y"),
            new AutocompleteResult("Don't You Squidward", "Don't You Squidward"),
            new AutocompleteResult("Captain Picard Facepalm", "Captain Picard Facepalm"),
            new AutocompleteResult("Sleeping Shaq", "Sleeping Shaq"),
            new AutocompleteResult("Laughing Leo", "Laughing Leo"),
            new AutocompleteResult("Philosoraptor", "Philosoraptor"),
            new AutocompleteResult("Third World Success Kid", "Third World Success Kid"),
            new AutocompleteResult("Evil Toddler", "Evil Toddler"),
            new AutocompleteResult("Matrix Morpheus", "Matrix Morpheus"),
            new AutocompleteResult("Black Girl Wat", "Black Girl Wat"),
            new AutocompleteResult("Hard To Swallow Pills", "Hard To Swallow Pills"),
            new AutocompleteResult("Laughing Men In Suits", "Laughing Men In Suits"),
            new AutocompleteResult("Who Would Win?", "Who Would Win?"),
            new AutocompleteResult("Picard Wtf", "Picard Wtf"),
            new AutocompleteResult("Yo Dawg Heard You", "Yo Dawg Heard You"),
            new AutocompleteResult("Too Damn High", "Too Damn High"),
            new AutocompleteResult("10 Guy", "10 Guy"),
            new AutocompleteResult("Dr Evil Laser", "Dr Evil Laser"),
            new AutocompleteResult("I'll Just Wait Here", "I'll Just Wait Here"),
            new AutocompleteResult("Am I The Only One Around Here", "Am I The Only One Around Here"),
            new AutocompleteResult("Face You Make Robert Downey Jr", "Face You Make Robert Downey Jr"),
            new AutocompleteResult("Put It Somewhere Else Patrick", "Put It Somewhere Else Patrick"),
            new AutocompleteResult("Look At Me", "Look At Me"),
            new AutocompleteResult("Imagination Spongebob", "Imagination Spongebob"),
            new AutocompleteResult("Be Like Bill", "Be Like Bill"),
            new AutocompleteResult("Bad Pun Dog", "Bad Pun Dog"),
            new AutocompleteResult("Jack Sparrow Being Chased", "Jack Sparrow Being Chased"),
            new AutocompleteResult("Mugatu So Hot Right Now", "Mugatu So Hot Right Now"),
            new AutocompleteResult("I Should Buy A Boat Cat", "I Should Buy A Boat Cat"),
            new AutocompleteResult("Sparta Leonidas", "Sparta Leonidas"),
            new AutocompleteResult("See Nobody Cares", "See Nobody Cares"),
            new AutocompleteResult("Aaaaand Its Gone", "Aaaaand Its Gone"),
            new AutocompleteResult("Maury Lie Detector", "Maury Lie Detector"),
            new AutocompleteResult("Confession Bear", "Confession Bear"),
};

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}
