using ToonVil_Card_Generator.CardGeneration;
using System.Drawing;
using System.Reflection;

namespace ToonVil_Card_Generator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ToonVil Card Generator");

        // TEMPORARY

        // Setup
        Font titleFont = FontLoader.GetFont("candyBeans.otf", 110);
		Font abilityFont = FontLoader.GetFont("cartoonToyTurbo.otf", 80);
		Font elementFont = FontLoader.GetFont("candyBeans.otf", 110);
        Color textColor = ColorTranslator.FromHtml("#d7b06c");
        PrepareImage.SizeIcon();
        PrepareImage.CleanIntermediaries();

        // Spongebob Squarepants
        PrepareText.DrawText("Spongebob Squarepants", titleFont, textColor, 1286, "Title");
		PrepareText.DrawText("Spongebob Squarepants cannot be defeated or discarded. If $Plankton$ is at Spongebob Squarepants' location, the cost to play Allies is increased by 2 Power.", abilityFont, textColor, 1286, "Ability");
		PrepareText.DrawText("5", elementFont, textColor, 1286, "Strength");
		PrepareImage.SizeCardImage("Spongebob Squarepants");
        PrepareImage.CombineImages("Spongebob Squarepants", "Fate");
        PrepareImage.CleanIntermediaries();

        // Filing Cabinet
        PrepareText.DrawText("Filing Cabinet", titleFont, textColor, 1286, "Title");
		PrepareText.DrawText("Play the top card of the \\Plan deck.", abilityFont, textColor, 1286, "Ability");
		PrepareText.DrawText("2", elementFont, textColor, 1286, "Cost");
		PrepareImage.SizeCardImage("Filing Cabinet");
        PrepareImage.CombineImages("Filing Cabinet", "Villain");
        PrepareImage.CleanIntermediaries();

        // Plan Z
        PrepareText.DrawText("Plan Z", titleFont, textColor, 1286, "Title");
		PrepareText.DrawText("Find and play King Neptune's Crown to Shell City. Move Spongebob Squarepants to the Krusty Krab.", abilityFont, textColor, 1286, "Ability");
		PrepareText.DrawText("1", elementFont, textColor, 1286, "TopRightElement");
		PrepareImage.SizeCardImage("Plan Z");
        PrepareImage.CombineImages("Plan Z", "Plan");
        PrepareImage.CleanIntermediaries();

        // Cleanup
        PrepareImage.CleanIntermediariesFinal();
    }
}
