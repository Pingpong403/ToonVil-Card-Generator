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
        int titleAreaMaxHeight = int.Parse(ConfigHelper.GetConfigValue("card", "titleAreaMaxHeight"));
        int textAreaMaxWidth = int.Parse(ConfigHelper.GetConfigValue("card", "textAreaMaxWidth"));
        int textAreaMaxHeight = int.Parse(ConfigHelper.GetConfigValue("card", "abilityAreaMaxHeight"));
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
        List<string> cardData = MiscHelper.GetTextFilesLines("Cards");
        foreach (string card in cardData)
        {
            // Title, Cost, Strength, On Play Ability, Activate Ability, Activate Cost Text, Type, Top Right, Bottom Right, Deck Name, Gains Action Symbol
            string[] cardSplit = card.Split("\t");
            string title = cardSplit[0];
            string cost = cardSplit[1];
            string strength = cardSplit[2];
            string ability = cardSplit[3];
            string activateAbility = cardSplit[4];
            string activateCost = cardSplit[5];
            string type = cardSplit[6];
            string topRight = cardSplit[7];
            string bottomRight = cardSplit[8];
            string deck = cardSplit[9];
            string gainsAction = cardSplit[10];
            
            DrawTitle(title, titleFont, textColor, textAreaMaxWidth, titleAreaMaxHeight);
            DrawAbility(ability, activateAbility, activateCost, gainsAction, abilityFont, textColor, textAreaMaxWidth, textAreaMaxHeight, keywordsAndColors);
            DrawText(cost, costFont, textColor, textAreaMaxWidth, textAreaMaxHeight, "Strength", keywordsAndColors);
            DrawText(strength, strengthFont, textColor, textAreaMaxWidth, textAreaMaxHeight, "Cost", keywordsAndColors);
            DrawText(topRight, topRightFont, textColor, textAreaMaxWidth, textAreaMaxHeight, "TopRight", keywordsAndColors);
            DrawText(bottomRight, bottomRightFont, textColor, textAreaMaxWidth, textAreaMaxHeight, "BottomRight", keywordsAndColors);
            DrawText(type, typeFont, textColor, textAreaMaxWidth, textAreaMaxHeight, "Type", keywordsAndColors);

            SizeCardImage(title);
            CombineImages(title, deck);
            CleanIntermediaries();
        }

        // Cleanup
        CleanImageIntermediaryFinal();
    }
}
