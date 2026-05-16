using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Arpeggios;

public sealed class ArpeggioLibrary(Random? random = null)
{
    private const int MaxStartFret = 12;
    private const int WindowLength = 6;

    private static readonly IReadOnlyList<GuitarString> Tuning =
    [
        new("E", 4),
        new("B", 11),
        new("G", 7),
        new("D", 2),
        new("A", 9),
        new("E", 4)
    ];

    private static readonly IReadOnlyDictionary<string, string> FlatRootAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Bb"] = "A#",
        ["Db"] = "C#",
        ["Eb"] = "D#",
        ["Gb"] = "F#",
        ["Ab"] = "G#"
    };

    public static readonly IReadOnlyList<ArpeggioQuality> Qualities =
    [
        new("Major triad", string.Empty, ["R", "3", "5"], "The notes of a major chord, played one at a time."),
        new("Minor triad", "m", ["R", "b3", "5"], "The notes of a minor chord, played one at a time."),
        new("Major 7", "maj7", ["R", "3", "5", "7"], "A major chord with a smooth major seventh color."),
        new("Dominant 7", "7", ["R", "3", "5", "b7"], "The V-chord sound used in blues, jazz, and tension-to-release lines."),
        new("Minor 7", "m7", ["R", "b3", "5", "b7"], "A minor chord with a flat seventh, common in rock, funk, and jazz."),
        new("Minor 7 flat 5", "m7b5", ["R", "b3", "b5", "b7"], "A half-diminished sound, especially useful in minor ii-V-i progressions."),
        new("Diminished 7", "dim7", ["R", "b3", "b5", "bb7"], "A tense symmetrical arpeggio often used for passing lines.")
    ];

    private readonly Random _random = random ?? Random.Shared;

    public IReadOnlyList<ArpeggioShape> GetShapes(string root, ArpeggioQuality quality)
    {
        return new[] { 0, 3, 5, 7, 9, 12 }
            .Select((startFret, index) => BuildShape(index + 1, root, quality, startFret))
            .ToArray();
    }

    public FretboardDiagram BuildMap(
        string root,
        ArpeggioQuality quality,
        int startFret,
        int length = WindowLength,
        IReadOnlySet<string>? selectedIntervals = null)
    {
        var rootPitch = MusicTheory.PitchClassFor(root);
        var selected = selectedIntervals ?? quality.Intervals.ToHashSet();
        var intervalByPitch = quality.Intervals
            .Where(selected.Contains)
            .ToDictionary(interval => MusicTheory.Normalize(rootPitch + SemitonesFor(interval)), interval => interval);
        var strings = Tuning.Select(item => item.Name).ToArray();
        var positions = new List<FretPosition>();

        for (var stringIndex = 0; stringIndex < Tuning.Count; stringIndex++)
        {
            for (var fret = startFret; fret < startFret + length; fret++)
            {
                var pitch = MusicTheory.Normalize(Tuning[stringIndex].PitchClass + fret);
                if (intervalByPitch.TryGetValue(pitch, out var label))
                {
                    positions.Add(new FretPosition(stringIndex, fret, label, SourceStringIndex: stringIndex));
                }
            }
        }

        return new FretboardDiagram(strings, startFret, length, positions);
    }

    public ArpeggioChordSymbol ParseChord(string value)
    {
        var chord = value.Trim();
        if (chord.Length == 0)
        {
            throw new ArgumentException("Chord cannot be blank.", nameof(value));
        }

        var rootLength = chord.Length >= 2 && (chord[1] == '#' || chord[1] == 'b') ? 2 : 1;
        var root = NormalizeRootName(chord[..rootLength]);
        var suffix = chord[rootLength..];
        var quality = Qualities.FirstOrDefault(candidate => candidate.Suffix.Equals(suffix, StringComparison.OrdinalIgnoreCase));

        if (!MusicTheory.ChromaticRoots.Contains(root) || quality is null)
        {
            throw new ArgumentException($"'{value}' is not a supported arpeggio chord. Try C, Cm, Cmaj7, C7, Cm7, Cm7b5, or Cdim7.", nameof(value));
        }

        return new ArpeggioChordSymbol(root, quality);
    }

    public IReadOnlyList<ArpeggioChordSymbol> ParseProgression(string input)
    {
        var parts = input
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            throw new ArgumentException("Enter at least one chord, for example Dm7 G7 Cmaj7.", nameof(input));
        }

        return parts.Select(ParseChord).ToArray();
    }

    public IReadOnlyList<int> ParseChordLengths(string input, int chordCount, int defaultLength)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Enumerable.Repeat(defaultLength, chordCount).ToArray();
        }

        var lengths = input
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part =>
            {
                if (!int.TryParse(part, out var value) || value < 1 || value > 32)
                {
                    throw new ArgumentException("Chord lengths must be whole numbers from 1 to 32 beats.", nameof(input));
                }

                return value;
            })
            .ToArray();

        if (lengths.Length != chordCount)
        {
            throw new ArgumentException($"Enter either no chord lengths or exactly {chordCount} values.", nameof(input));
        }

        return lengths;
    }

    public ArpeggioPrompt BuildPrompt(IReadOnlyList<ArpeggioChordSymbol> progression, IReadOnlySet<string> targetIntervals)
    {
        if (progression.Count == 0)
        {
            throw new ArgumentException("Progression must contain at least one chord.", nameof(progression));
        }

        var chord = progression[_random.Next(progression.Count)];
        var intervals = chord.Quality.Intervals
            .Where(targetIntervals.Contains)
            .DefaultIfEmpty("R")
            .ToArray();
        var target = intervals[_random.Next(intervals.Length)];
        return new ArpeggioPrompt(chord, target, NoteNameFor(chord.Root, target));
    }

    public static ArpeggioQuality Quality(string suffix)
    {
        return Qualities.Single(quality => quality.Suffix.Equals(suffix, StringComparison.OrdinalIgnoreCase));
    }

    public static string NoteNameFor(string root, string interval)
    {
        return MusicTheory.NameForInterval(root, interval);
    }

    public static string NotesFor(ArpeggioChordSymbol chord)
    {
        return string.Join(" ", chord.Quality.Intervals.Select(interval => NoteNameFor(chord.Root, interval)));
    }

    public static string TeachingHintFor(ArpeggioChordSymbol chord)
    {
        return $"{chord.DisplayName}: {string.Join(" ", chord.Quality.Intervals)} = {NotesFor(chord)}. Use these notes as strong landing points when this chord is sounding.";
    }

    public static string TargetHintFor(string interval) => interval switch
    {
        "R" => "The root sounds like home for the chord.",
        "3" => "The 3rd gives a major chord its bright character.",
        "b3" => "The flat 3rd gives a minor chord its darker character.",
        "5" => "The 5th is stable and easy to hear, but usually less colorful than the 3rd or 7th.",
        "b5" => "The flat 5th is tense and points strongly toward diminished or half-diminished sounds.",
        "7" => "The major 7th is a smooth, jazzy color tone.",
        "b7" => "The flat 7th creates dominant or minor-7 color and often wants to move onward.",
        "bb7" => "The diminished 7th is tense and symmetrical, useful for passing diminished lines.",
        _ => "This is a chord tone, so it will sound connected to the current chord."
    };

    public static int SemitonesFor(string interval) => interval switch
    {
        "R" => 0,
        "b2" or "b9" => 1,
        "2" or "9" => 2,
        "b3" or "#9" => 3,
        "3" => 4,
        "4" or "11" => 5,
        "b5" or "#11" => 6,
        "5" => 7,
        "b6" or "b13" => 8,
        "6" or "13" or "bb7" => 9,
        "b7" => 10,
        "7" => 11,
        _ => 0
    };

    public static ArpeggioChordSymbol FromTriadChord(ChordSymbol chord)
    {
        return new ArpeggioChordSymbol(chord.Root, chord.Quality == ChordQuality.Minor ? Quality("m") : Quality(string.Empty));
    }

    private ArpeggioShape BuildShape(int number, string root, ArpeggioQuality quality, int startFret)
    {
        var diagram = BuildMap(root, quality, startFret, WindowLength);
        var positions = diagram.Positions;
        var minFret = positions.Count == 0 ? startFret : positions.Min(position => position.Fret);
        var maxFret = positions.Count == 0 ? startFret + WindowLength - 1 : positions.Max(position => position.Fret);
        return new ArpeggioShape(number, startFret, minFret, maxFret, diagram);
    }

    private static string NormalizeRootName(string root)
    {
        var trimmed = root.Trim();
        var normalized = trimmed.Length switch
        {
            0 => trimmed,
            1 => trimmed.ToUpperInvariant(),
            _ => char.ToUpperInvariant(trimmed[0]) + trimmed[1..]
        };

        return FlatRootAliases.GetValueOrDefault(normalized, normalized);
    }

    private sealed record GuitarString(string Name, int PitchClass);
}

public sealed record ArpeggioQuality(
    string Name,
    string Suffix,
    IReadOnlyList<string> Intervals,
    string Use);

public sealed record ArpeggioChordSymbol(string Root, ArpeggioQuality Quality)
{
    public string DisplayName => $"{Root}{Quality.Suffix}";
}

public sealed record ArpeggioShape(
    int Number,
    int StartFret,
    int MinFret,
    int MaxFret,
    FretboardDiagram Diagram);

public sealed record ArpeggioPrompt(
    ArpeggioChordSymbol Chord,
    string TargetInterval,
    string TargetNote);
