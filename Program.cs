using ToonVil_Card_Generator.CardGeneration;
using static ToonVil_Card_Generator.CardGeneration.PrepareText;
using static ToonVil_Card_Generator.CardGeneration.PrepareImage;
using System.Drawing;
using System.Reflection;

namespace ToonVil_Card_Generator;

public enum CardType
{
    Ability,
    Activate,
    ActivateCost,
    AbilityActivate,
    AbilityActivateCost
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ToonVil Card Generator");
        
        // Setup variables and clean intermediaries
        Color textColor = ColorTranslator.FromHtml("#d7b06c");
        SizeIcon();
        CleanIntermediaries();

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
        // List<string> cardData = MiscHelper.GetTextFilesLines("Cards");
        // foreach (string card in cardData)
        // {
        //     // Title, Cost, Strength, On Play Ability, Activate Ability, Activate Cost Text, Type, Top Right, Bottom Right, Deck Name, Gains Action Symbol
        //     string[] cardSplit = card.Split("\t");
        //     string title = cardSplit[0];
        //     string cost = cardSplit[1];
        //     string strength = cardSplit[2];
        //     string ability = cardSplit[3];
        //     string activateAbility = cardSplit[4];
        //     string activateCost = cardSplit[5];
        //     string type = cardSplit[6];
        //     string topRight = cardSplit[7];
        //     string bottomRight = cardSplit[8];
        //     string deck = cardSplit[9];
        //     string gainsAction = cardSplit[10];

            
        // }

        // // Cleanup
        // CleanImageIntermediaryFinal();

        // TEMPORARY

        // John Doe - test
        DrawTitle("John Doe", titleFont, textColor, 1230, 166);
		DrawAbility("Die. DividingLine_ Player: Die, but more glamorously.", "", "", "", abilityFont, textColor, 1230, 668, keywordsAndColors);
		DrawText("5", strengthFont, textColor, 1230, 690, "Strength", keywordsAndColors);
        DrawText("Hero", typeFont, textColor, 1230, 690, "Type", keywordsAndColors);
		SizeCardImage("John Doe");
        CombineImages("John Doe", "Fate");
        CleanIntermediaries();

        // Cleanup
        CleanImageIntermediaryFinal();
    }
}
