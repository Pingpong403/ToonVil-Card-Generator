using ToonVil_Card_Generator.CardGeneration;
using System.Drawing;
using System.Reflection;

namespace ToonVil_Card_Generator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ToonVil Card Generator");
        Font titleFont = FontLoader.GetFont("candyBeans.otf", 110);
		Font abilityFont = FontLoader.GetFont("cartoonToyTurbo.otf", 80);
		Font elementFont = FontLoader.GetFont("candyBeans.otf", 110);
        Color textColor = ColorTranslator.FromHtml("#d7b06c");
        PrepareText.DrawText("Spongebob Squarepants", titleFont, textColor, 1286, "Title");
		PrepareText.DrawText("Spongebob Squarepants cannot be defeated or discarded. If $Plankton$ is at Spongebob Squarepants' location, all card costs are increased by 2 Power.", abilityFont, textColor, 1286, "Ability");
		PrepareText.DrawText("5", elementFont, textColor, 1286, "Strength");

		PrepareImage.SizeImage("Spongebob Squarepants");

        PrepareImage.CombineImages("Spongebob Squarepants", "Fate");
    }
}
