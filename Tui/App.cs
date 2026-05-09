using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.IntervalMaps;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Tui;

public sealed class App(TriadInversionLibrary triads, PentatonicLibrary pentatonics, IntervalFunctionMapLibrary intervalMaps)
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
            Console.WriteLine("3. Interval function map");
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
                case "3":
                    ShowIntervalFunctionMap();
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

    private void ShowIntervalFunctionMap()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Interval function map");

            var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            while (true)
            {
                var anchorFretChoice = ReadMenuChoice("Choose an anchor fret", ["0", "3", "5", "7", "9", "12"], allowBack: true);
                if (anchorFretChoice is null)
                {
                    break;
                }

                var anchorFret = int.Parse(anchorFretChoice);

                while (true)
                {
                    var diagram = intervalMaps.BuildMap(root, anchorFret);

                    Console.Clear();
                    WriteHeader($"{root} interval function map ({diagram.StartFret}-{diagram.StartFret + diagram.Length - 1})");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals are relative to {root}");
                    Console.WriteLine($"Anchor fret: {anchorFret}");
                    Console.WriteLine("N/P = move anchor fret, B = back, Q = main menu");
                    Console.WriteLine();

                    foreach (var line in _renderer.Render(diagram))
                    {
                        Console.WriteLine(line);
                    }

                    Console.WriteLine();
                    Console.Write("Command > ");

                    switch (Console.ReadKey(intercept: true).Key)
                    {
                        case ConsoleKey.N:
                        case ConsoleKey.RightArrow:
                            anchorFret = Math.Min(IntervalFunctionMapLibrary.MaxAnchorFret, anchorFret + 1);
                            break;
                        case ConsoleKey.P:
                        case ConsoleKey.LeftArrow:
                            anchorFret = Math.Max(0, anchorFret - 1);
                            break;
                        case ConsoleKey.B:
                        case ConsoleKey.Escape:
                            goto ChooseAnchor;
                        case ConsoleKey.Q:
                            return;
                    }
                }

            ChooseAnchor:
                continue;
            }
        }
    }

    private void ShowTriadInversions()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Triad inversions");

            var root = ReadMenuChoice("Choose a root", MusicTheory.NaturalRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            while (true)
            {
                var qualityChoice = ReadMenuChoice("Choose a quality", ["Major", "Minor"], allowBack: true);
                if (qualityChoice is null)
                {
                    break;
                }

                var quality = Enum.Parse<ChordQuality>(qualityChoice);

                while (true)
                {
                    Console.Clear();
                    WriteHeader($"{root} {quality} triad inversions");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, {Third}3{Reset} = third, {Fifth}5{Reset} = fifth, [common] = lower-position shape");
                    Console.WriteLine("B = back, Q = main menu");
                    Console.WriteLine();

                    foreach (var grouping in triads.GetTriadInversions(root, quality))
                    {
                        Console.WriteLine(grouping.Name);
                        Console.WriteLine(new string('-', grouping.Name.Length));

                        var diagrams = grouping.Shapes
                            .Select(shape => ($"{shape.InversionName} ({shape.MinFret}-{shape.MaxFret}){CommonTriadSuffix(shape)}", shape.Diagram))
                            .ToArray();

                        foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth()))
                        {
                            Console.WriteLine(line);
                        }

                        Console.WriteLine();
                    }

                    Console.Write("Command > ");
                    switch (Console.ReadKey(intercept: true).Key)
                    {
                        case ConsoleKey.B:
                        case ConsoleKey.Escape:
                            goto ChooseQuality;
                        case ConsoleKey.Q:
                            return;
                    }
                }

            ChooseQuality:
                continue;
            }
        }
    }

    private void ShowPentatonicShapes()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Scale shapes");

            var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            while (true)
            {
                var scaleName = ReadMenuChoice("Choose a scale", PentatonicLibrary.ScaleKinds.Select(PentatonicLibrary.NameFor).ToArray(), allowBack: true);
                if (scaleName is null)
                {
                    break;
                }

                var scaleKind = PentatonicLibrary.ScaleKinds.Single(kind => PentatonicLibrary.NameFor(kind) == scaleName);

                while (true)
                {
                    var shapes = pentatonics.GetShapes(root, scaleKind);

                    Console.Clear();
                    WriteHeader($"{root} {PentatonicLibrary.NameFor(scaleKind)} shapes");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals show scale degrees ({Pentatonic}2/4/6/7/flats{Reset}, {Third}3/b3{Reset}, {BlueNote}#4/b5{Reset}, {Fifth}5{Reset})");
                    Console.WriteLine("T = toggle major/minor, B = back, Q = main menu");
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
                        case ConsoleKey.B:
                        case ConsoleKey.Escape:
                            goto ChooseScale;
                        case ConsoleKey.Q:
                            return;
                    }
                }

            ChooseScale:
                continue;
            }
        }
    }

    private static string? ReadMenuChoice(string prompt, IReadOnlyList<string> options, bool allowBack = false)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            for (var index = 0; index < options.Count; index++)
            {
                Console.WriteLine($"{index + 1}. {options[index]}");
            }
            if (allowBack)
            {
                Console.WriteLine("B. Back");
            }
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (allowBack && input is not null && input.Equals("B", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

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

    private static string CommonTriadSuffix(TriadShape shape)
    {
        return shape.MinFret > 0 && shape.MaxFret <= 8 ? " [common]" : string.Empty;
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
