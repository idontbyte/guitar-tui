using GuitarResourcesTui.Triads;
using GuitarResourcesTui.Tui.Printing;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private void ShowPrintableResourcesMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Printable resources");
            WriteDescriptionLine("Create PDF-ready sheets you can keep beside the guitar.");
            Console.WriteLine();
            WriteMenuOption("1", "Triad song-chord lookup guide", "Print one compact guide for finding triads across common song chords.");
            WriteMenuOption("2", "Focused triad inversion sheet", "Print one root and quality across adjacent string sets.");
            WriteMenuOption("3", "Arpeggio sheet", "Print movable arpeggio maps for a chosen chord type.");
            WriteMenuOption("4", "Jazz sheets", "Open printable voicing, ii-V-I, and standards study worksheets.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a resource > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    PrintTriadSongChordLookupGuide();
                    break;
                case "2":
                    PrintFocusedTriadInversionSheet();
                    break;
                case "3":
                    ShowArpeggioShapeReference(printOnly: true);
                    break;
                case "4":
                    ShowPrintableJazzSheets();
                    break;
                case "B":
                case "b":
                    return;
                default:
                    Console.WriteLine("That choice is not on the menu.");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }

    private void PrintTriadSongChordLookupGuide()
    {
        try
        {
            var filePath = TriadSongLookupGuidePrinter.Print(triads);
            Console.WriteLine();
            Console.WriteLine($"Opened printable guide: {filePath}");
            Console.WriteLine("Use your browser's print dialog if it does not open automatically.");
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine($"Could not open the printable guide automatically: {exception.Message}");
        }

        Console.WriteLine("Press any key to continue.");
        Console.ReadKey(intercept: true);
    }

    private void PrintFocusedTriadInversionSheet()
    {
        var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
        if (root is null)
        {
            return;
        }

        var qualityChoice = ReadMenuChoice("Choose a quality", ["Major", "Minor"], allowBack: true);
        if (qualityChoice is null)
        {
            return;
        }

        var quality = Enum.Parse<ChordQuality>(qualityChoice);
        PrintTriadSheet(root, quality, triads.GetTriadInversions(root, quality));
    }
}
