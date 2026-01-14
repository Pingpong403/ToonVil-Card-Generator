using ToonVil_Card_Generator.CardGeneration;
using static ToonVil_Card_Generator.CardGeneration.PrepareText;
using static ToonVil_Card_Generator.CardGeneration.PrepareImage;
using System.Drawing;
using System.Reflection;

namespace ToonVil_Card_Generator;

class Program
{
    static void Main(string[] args)
    {
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

        // Go through each line of Cards.txt and build cards

        // TEMPORARY

        // Setup
        Color textColor = ColorTranslator.FromHtml("#d7b06c");
        SizeIcon();
        CleanIntermediaries();

        // Spongebob Squarepants
        DrawText("Spongebob Squarepants", titleFont, textColor, 1230, 166, "Title", keywordsAndColors);
		DrawText("Spongebob Squarepants cannot be defeated or discarded. DividingLine_ Plankton: The cost to play Allies is increased by 2 Power.", abilityFont, textColor, 1230, 668, "Ability", keywordsAndColors);
		DrawText("5", strengthFont, textColor, 1230, 690, "Strength", keywordsAndColors);
        DrawText("Hero", typeFont, textColor, 1230, 690, "Type", keywordsAndColors);
		SizeCardImage("Spongebob Squarepants");
        CombineImages("Spongebob Squarepants", "Fate");
        CleanIntermediaries();

        // Filing Cabinet
        DrawText("Filing Cabinet", titleFont, textColor, 1230, 690, "Title", keywordsAndColors);
		DrawText("Play the top card of the \\Plan deck.", abilityFont, textColor, 1230, 690, "Ability", keywordsAndColors);
		DrawText("2", costFont, textColor, 1230, 690, "Cost", keywordsAndColors);
        DrawText("Item", typeFont, textColor, 1230, 690, "Type", keywordsAndColors);
		SizeCardImage("Filing Cabinet");
        CombineImages("Filing Cabinet", "Villain");
        CleanIntermediaries();

        // Plan Z
        DrawText("Plan Z", titleFont, textColor, 1230, 690, "Title", keywordsAndColors);
		DrawText("Find and play King Neptune's Crown to Shell City. Move Spongebob Squarepants to the Krusty Krab.", abilityFont, textColor, 1230, 690, "Ability", keywordsAndColors);
		DrawText("1", topRightFont, textColor, 1230, 690, "TopRightElement", keywordsAndColors);
        DrawText("Plan", typeFont, textColor, 1230, 690, "Type", keywordsAndColors);
		SizeCardImage("Plan Z");
        CombineImages("Plan Z", "Plan");
        CleanIntermediaries();

        // Cleanup
        CleanImageIntermediaryFinal();
    }
}
