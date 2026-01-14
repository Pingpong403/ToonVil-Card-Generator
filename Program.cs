using ToonVil_Card_Generator.CardGeneration;
using static ToonVil_Card_Generator.CardGeneration.PrepareText;
using System.Drawing;
using System.Reflection;

namespace ToonVil_Card_Generator;

class Program
{
    static void Main(string[] args)
    {
        // TESTING
        // Font font = FontLoader.GetFont("cartoonToyTurbo.otf", 80);
        // string[] subjects =
        // {
        //     "Menace",
        //     "Menace menace",
        //     "Menace Menace",
        //     "I am not your queen"
        // };
        // foreach (string subject in subjects)
        // {
        //     Console.Write($"{subject} -> ");
        //     foreach (int length in PrepareText.GetWordLengths(subject, font, 1286))
        //     {
        //         Console.Write($"{length} ");
        //     }
        //     Console.WriteLine();
        //     Console.WriteLine();
        // }
        // return;

        Console.WriteLine("ToonVil Card Generator");

        // Load all the fonts
        string titleFontFile = ConfigHelper.GetConfigValue("card", "titleFont");
        int titleFontSize = int.Parse(ConfigHelper.GetConfigValue("card", "titleFontSize"));
        string abilityFontFile = ConfigHelper.GetConfigValue("card", "abilityFont");
        int abilityFontSize = int.Parse(ConfigHelper.GetConfigValue("card", "abilityFontSize"));
        string elementFontFile = ConfigHelper.GetConfigValue("card", "elementFont");
        int costFontSize = int.Parse(ConfigHelper.GetConfigValue("card", "costFontSize"));
        int strengthFontSize = int.Parse(ConfigHelper.GetConfigValue("card", "strengthFontSize"));
        int topRightFontSize = int.Parse(ConfigHelper.GetConfigValue("card", "topRightFontSize"));
        int bottomRightFontSize = int.Parse(ConfigHelper.GetConfigValue("card", "bottomRightFontSize"));
        string typeFontFile = ConfigHelper.GetConfigValue("card", "typeFont");
        int typeFontSize = int.Parse(ConfigHelper.GetConfigValue("card", "typeFontSize"));
        Font titleFont = FontLoader.GetFont(titleFontFile, titleFontSize);
		Font abilityFont = FontLoader.GetFont(abilityFontFile, abilityFontSize);
        Font typeFont = FontLoader.GetFont(typeFontFile, typeFontSize);
		Font costFont = FontLoader.GetFont(elementFontFile, costFontSize);
        Font strengthFont = FontLoader.GetFont(elementFontFile, strengthFontSize);
        Font topRightFont = FontLoader.GetFont(elementFontFile, topRightFontSize);
        Font bottomRightFont = FontLoader.GetFont(elementFontFile, bottomRightFontSize);

        // Load all the keywords and their colors
        Dictionary<string, string> keywordsAndColors = KeywordHelper.GetColorMapping();

        // TEMPORARY

        // Setup
        Color textColor = ColorTranslator.FromHtml("#d7b06c");
        PrepareImage.SizeIcon();
        PrepareImage.CleanIntermediaries();

        // // Spongebob Squarepants
        PrepareText.DrawText("Spongebob Squarepants the Sponge Detective", titleFont, textColor, 1230, 166, "Title", keywordsAndColors);
		PrepareText.DrawText("Spongebob Squarepants the Sponge Detective is locked to Plankton's Universe. Spongebob Squarepants the Sponge Detective cannot be defeated or discarded. If Plankton is at Spongebob Squarepants the Sponge Detective's location, the cost to play Allies is increased by 2 Power. When Plankton plays Effects, instead of paying their cost, he places a Power from the Supply on Spongebob Squarepants the Sponge Detective. When Spongebob Squarepants the Sponge Detective has 5 Power on him, Plankton must pay 10 Power. If he does, he wins the game. If he does not, instead place Spongebob Squarepants the Sponge Detective on top of the Fate Deck.", abilityFont, textColor, 1230, 690, "Ability", keywordsAndColors);
		PrepareText.DrawText("5", strengthFont, textColor, 1230, 690, "Strength", keywordsAndColors);
		PrepareImage.SizeCardImage("Spongebob Squarepants");
        PrepareImage.CombineImages("Spongebob Squarepants", "Fate");
        PrepareImage.CleanIntermediaries();

        // // Filing Cabinet
        PrepareText.DrawText("Filing Cabinet", titleFont, textColor, 1230, 690, "Title", keywordsAndColors);
		PrepareText.DrawText("Play the top card of the \\Plan deck.", abilityFont, textColor, 1230, 690, "Ability", keywordsAndColors);
		PrepareText.DrawText("2", costFont, textColor, 1230, 690, "Cost", keywordsAndColors);
		PrepareImage.SizeCardImage("Filing Cabinet");
        PrepareImage.CombineImages("Filing Cabinet", "Villain");
        PrepareImage.CleanIntermediaries();

        // // Plan Z
        PrepareText.DrawText("Plan Z", titleFont, textColor, 1230, 690, "Title", keywordsAndColors);
		PrepareText.DrawText("Find and play King Neptune's Crown to Shell City. Move Spongebob Squarepants to the Krusty Krab.", abilityFont, textColor, 1230, 690, "Ability", keywordsAndColors);
		PrepareText.DrawText("1", topRightFont, textColor, 1230, 690, "TopRightElement", keywordsAndColors);
		PrepareImage.SizeCardImage("Plan Z");
        PrepareImage.CombineImages("Plan Z", "Plan");
        PrepareImage.CleanIntermediaries();

        // // Cleanup
        PrepareImage.CleanImageIntermediaryFinal();
    }
}
