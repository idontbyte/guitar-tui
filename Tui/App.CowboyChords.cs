using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private static readonly IReadOnlyDictionary<string, int[]> CowboyChordFrets = new Dictionary<string, int[]>
    {
        ["A"] = [0, 2, 2, 2, 0, -1],
        ["Am"] = [0, 1, 2, 2, 0, -1],
        ["A7"] = [0, 2, 0, 2, 0, -1],
        ["B7"] = [2, 0, 2, 1, 2, -1],
        ["C"] = [0, 1, 0, 2, 3, -1],
        ["C7"] = [0, 1, 3, 2, 3, -1],
        ["D"] = [2, 3, 2, 0, -1, -1],
        ["Dm"] = [1, 3, 2, 0, -1, -1],
        ["D7"] = [2, 1, 2, 0, -1, -1],
        ["E"] = [0, 0, 1, 2, 2, 0],
        ["Em"] = [0, 0, 0, 2, 2, 0],
        ["E7"] = [0, 0, 1, 0, 2, 0],
        ["F"] = [1, 1, 2, 3, -1, -1],
        ["G"] = [3, 0, 0, 0, 2, 3],
        ["G7"] = [1, 0, 0, 0, 2, 3]
    };

    private static readonly IReadOnlyList<string> CowboyChordOrder =
    [
        "A", "Am", "A7", "B7", "C", "C7", "D", "Dm", "D7", "E", "Em", "E7", "F", "G", "G7"
    ];

    private void ShowCowboyChordsMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Cowboy chords");
            WriteDescriptionLine("Cowboy chords are the familiar open-position grips used in countless songs.");
            Console.WriteLine();
            WriteMenuOption("1", "Chord reference", "See the common open chords with root/third/fifth labels.");
            WriteMenuOption("2", "Song mode", "Practise changing between open chords in real progressions with click and backing.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowCowboyChordReference();
                    break;
                case "2":
                    ShowCowboyChordSongMode();
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

    private void ShowCowboyChordReference()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Cowboy chord reference");
            WriteDescriptionLine("These diagrams show which strings to fret, which strings stay open, and which strings are muted.");
            Console.WriteLine($"Labels: {Root}R{Reset} = root, {Third}3/b3{Reset} = third, {Fifth}5{Reset} = fifth, X = muted string");
            Console.WriteLine($"{NoteLabelCommandText()}, B/Q = back");
            Console.WriteLine();

            var diagrams = CowboyChordOrder
                .Select(chordName => (chordName, BuildCowboyChordDiagram(chordName)))
                .ToArray();
            var cellWidth = diagrams.Max(_renderer.MeasureTitledDiagramWidth);

            foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth(), maxColumns: 4, cellWidth: cellWidth))
            {
                Console.WriteLine(line);
            }

            switch (Console.ReadKey(intercept: true).Key)
            {
                case ConsoleKey.L:
                    ToggleNoteLabelMode();
                    break;
                case ConsoleKey.B:
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    private void ShowCowboyChordSongMode()
    {
        var setup = ReadCowboyChordSongSetup();
        if (setup is null)
        {
            return;
        }

        ShowChordDiagramSongGame("Cowboy chord song mode", setup, BuildCowboySongDiagram);
    }

    private TriadProgressionSetup? ReadCowboyChordSongSetup()
    {
        var songs = TriadProgressionGameLibrary.PresetProgressions
            .Where(IsCowboyChordSong)
            .ToArray();

        return ReadPresetProgressionSetup(
            "Cowboy chord song select",
            songs,
            "Choose a listed song number, list position, song title, or B > ",
            allowListPosition: true);
    }

    private static bool IsCowboyChordSong(PresetChordProgression preset)
    {
        return preset.ProgressionText
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(CowboyChordFrets.ContainsKey);
    }

    private static FretboardDiagram BuildCowboySongDiagram(ChordSymbol chord)
    {
        return BuildCowboyChordDiagram(chord.DisplayName);
    }

    private static FretboardDiagram BuildCowboyChordDiagram(string chordName)
    {
        var frets = CowboyChordFrets[chordName];
        var labelsByPitchClass = CowboyChordLabels(chordName);
        var positions = new List<FretPosition>();
        var tuning = new[] { 4, 11, 7, 2, 9, 4 };
        var strings = new[] { "E", "B", "G", "D", "A", "E" };

        for (var stringIndex = 0; stringIndex < frets.Length; stringIndex++)
        {
            var fret = frets[stringIndex];
            if (fret < 0)
            {
                positions.Add(new FretPosition(stringIndex, 0, "X", IsMuted: true, SourceStringIndex: stringIndex));
                continue;
            }

            var pitch = MusicTheory.Normalize(tuning[stringIndex] + fret);
            positions.Add(new FretPosition(
                stringIndex,
                fret,
                labelsByPitchClass.GetValueOrDefault(pitch, string.Empty),
                SourceStringIndex: stringIndex));
        }

        return new FretboardDiagram(strings, 0, 4, positions);
    }

    private static IReadOnlyDictionary<int, string> CowboyChordLabels(string chordName)
    {
        var hasSeventh = chordName.EndsWith('7');
        var baseName = hasSeventh ? chordName[..^1] : chordName;
        var quality = baseName.EndsWith('m') ? ChordQuality.Minor : ChordQuality.Major;
        var root = quality == ChordQuality.Minor ? baseName[..^1] : baseName;
        var rootPitch = MusicTheory.PitchClassFor(root);
        var thirdInterval = quality == ChordQuality.Minor ? 3 : 4;
        var labels = new Dictionary<int, string>
        {
            [rootPitch] = "R",
            [MusicTheory.Normalize(rootPitch + thirdInterval)] = quality == ChordQuality.Minor ? "b3" : "3",
            [MusicTheory.Normalize(rootPitch + 7)] = "5"
        };

        if (hasSeventh)
        {
            labels[MusicTheory.Normalize(rootPitch + 10)] = "b7";
        }

        return labels;
    }
}
