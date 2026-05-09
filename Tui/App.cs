using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Tui;

public sealed class App(TriadInversionLibrary triads, PentatonicLibrary pentatonics)
{
    private const string Reset = "\e[0m";
    private const string Root = "\e[1;38;5;46m";
    private const string Third = "\e[1;38;5;220m";
    private const string Fifth = "\e[1;38;5;39m";
    private const string Pentatonic = "\e[1;38;5;213m";
    private const string BlueNote = "\e[1;38;5;51m";

    private readonly FretboardRenderer _renderer = new();

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Guitar Resources");
            Console.WriteLine("1. Triad inversions");
            Console.WriteLine("2. Scale shapes");
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowTriadInversions();
                    break;
                case "2":
                    ShowPentatonicShapes();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("That choice is not on the menu.");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }

    private void ShowTriadInversions()
    {
        Console.Clear();
        WriteHeader("Triad inversions");

        var root = ReadMenuChoice("Choose a root", MusicTheory.NaturalRoots);
        var qualityChoice = ReadMenuChoice("Choose a quality", ["Major", "Minor"]);
        var quality = Enum.Parse<ChordQuality>(qualityChoice);

        Console.Clear();
        WriteHeader($"{root} {quality} triad inversions");
        Console.WriteLine($"Labels: {Root}R{Reset} = root, {Third}3{Reset} = third, {Fifth}5{Reset} = fifth");
        Console.WriteLine();

        foreach (var grouping in triads.GetTriadInversions(root, quality))
        {
            Console.WriteLine(grouping.Name);
            Console.WriteLine(new string('-', grouping.Name.Length));

            var diagrams = grouping.Shapes
                .Select(shape => ($"{shape.InversionName} ({shape.MinFret}-{shape.MaxFret})", shape.Diagram))
                .ToArray();

            foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth()))
            {
                Console.WriteLine(line);
            }

            Console.WriteLine();
        }

        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey(intercept: true);
    }

    private void ShowPentatonicShapes()
    {
        Console.Clear();
        WriteHeader("Scale shapes");

        var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots);
        var scaleName = ReadMenuChoice("Choose a scale", PentatonicLibrary.ScaleKinds.Select(PentatonicLibrary.NameFor).ToArray());
        var scaleKind = PentatonicLibrary.ScaleKinds.Single(kind => PentatonicLibrary.NameFor(kind) == scaleName);

        while (true)
        {
            var shapes = pentatonics.GetShapes(root, scaleKind);

            Console.Clear();
            WriteHeader($"{root} {PentatonicLibrary.NameFor(scaleKind)} shapes");
            Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals show scale degrees ({Pentatonic}2/4/6/b7{Reset}, {Third}3/b3{Reset}, {BlueNote}b5{Reset}, {Fifth}5{Reset})");
            Console.WriteLine("T = toggle major/minor, Q = back");
            Console.WriteLine();

            var diagrams = shapes
                .Select(shape => ($"Shape {shape.Number} ({shape.MinFret}-{shape.MaxFret})", shape.Diagram))
                .ToArray();

            foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth()))
            {
                Console.WriteLine(line);
            }

            Console.WriteLine();
            Console.Write("Command > ");

            switch (Console.ReadKey(intercept: true).Key)
            {
                case ConsoleKey.T:
                    scaleKind = PentatonicLibrary.ToggleMajorMinor(scaleKind);
                    break;
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    private static string ReadMenuChoice(string prompt, IReadOnlyList<string> options)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            for (var index = 0; index < options.Count; index++)
            {
                Console.WriteLine($"{index + 1}. {options[index]}");
            }
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out var optionNumber) && optionNumber >= 1 && optionNumber <= options.Count)
            {
                return options[optionNumber - 1];
            }

            if (input is not null && options.Any(option => option.Equals(input, StringComparison.OrdinalIgnoreCase)))
            {
                return options.Single(option => option.Equals(input, StringComparison.OrdinalIgnoreCase));
            }

            Console.WriteLine("That choice is not on the menu.");
            Console.WriteLine();
        }
    }

    private static void WriteHeader(string title)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
        Console.WriteLine();
    }

    private static int GetUsableConsoleWidth()
    {
        if (Console.IsOutputRedirected)
        {
            return 120;
        }

        return Math.Max(40, Console.WindowWidth - 1);
    }
}
