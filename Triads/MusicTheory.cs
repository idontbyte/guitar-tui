namespace GuitarResourcesTui.Triads;

public static class MusicTheory
{
    public static readonly IReadOnlyList<string> NaturalRoots = ["A", "B", "C", "D", "E", "F", "G"];
    public static readonly IReadOnlyList<string> ChromaticRoots = ["A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"];

    private static readonly IReadOnlyDictionary<string, int> PitchClasses = new Dictionary<string, int>
    {
        ["C"] = 0,
        ["C#"] = 1,
        ["D"] = 2,
        ["D#"] = 3,
        ["E"] = 4,
        ["F"] = 5,
        ["F#"] = 6,
        ["G"] = 7,
        ["G#"] = 8,
        ["A"] = 9,
        ["A#"] = 10,
        ["B"] = 11
    };

    private static readonly string[] SharpNames = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

    private static readonly IReadOnlyDictionary<char, int> NaturalPitchClasses = new Dictionary<char, int>
    {
        ['C'] = 0,
        ['D'] = 2,
        ['E'] = 4,
        ['F'] = 5,
        ['G'] = 7,
        ['A'] = 9,
        ['B'] = 11
    };

    private static readonly char[] LetterNames = ['C', 'D', 'E', 'F', 'G', 'A', 'B'];

    public static int PitchClassFor(string note) => PitchClasses[note];

    public static string NameFor(int pitchClass) => SharpNames[Normalize(pitchClass)];

    public static string NameForInterval(string root, string interval)
    {
        var rootLetter = char.ToUpperInvariant(root[0]);
        var rootLetterIndex = Array.IndexOf(LetterNames, rootLetter);
        if (rootLetterIndex < 0)
        {
            return NameFor(PitchClassFor(root) + IntervalSemitones(interval));
        }

        var targetLetter = LetterNames[(rootLetterIndex + DiatonicSteps(interval)) % LetterNames.Length];
        var targetPitch = Normalize(PitchClassFor(root) + IntervalSemitones(interval));
        var naturalPitch = NaturalPitchClasses[targetLetter];
        var accidental = NormalizeToNearestAccidental(targetPitch - naturalPitch);

        return accidental switch
        {
            -2 => $"{targetLetter}bb",
            -1 => $"{targetLetter}b",
            0 => targetLetter.ToString(),
            1 => $"{targetLetter}#",
            2 => $"{targetLetter}##",
            _ => NameFor(targetPitch)
        };
    }

    public static IReadOnlyList<TriadTone> BuildTriad(string root, ChordQuality quality)
    {
        var rootPitch = PitchClassFor(root);
        var thirdInterval = quality == ChordQuality.Major ? 4 : 3;

        return
        [
            new TriadTone(rootPitch, NameFor(rootPitch), "R"),
            new TriadTone(Normalize(rootPitch + thirdInterval), NameFor(rootPitch + thirdInterval), "3"),
            new TriadTone(Normalize(rootPitch + 7), NameFor(rootPitch + 7), "5")
        ];
    }

    public static string InversionName(string bassFunction) => bassFunction switch
    {
        "R" => "Root",
        "3" => "1st inv",
        "5" => "2nd inv",
        _ => bassFunction
    };

    public static int Normalize(int pitchClass) => ((pitchClass % 12) + 12) % 12;

    private static int IntervalSemitones(string interval) => interval switch
    {
        "R" => 0,
        "b2" or "b9" => 1,
        "2" or "9" => 2,
        "b3" => 3,
        "#9" => 3,
        "3" => 4,
        "4" or "11" => 5,
        "b5" => 6,
        "#11" => 6,
        "5" => 7,
        "b6" or "b13" => 8,
        "6" or "13" => 9,
        "bb7" => 9,
        "b7" => 10,
        "7" => 11,
        _ => 0
    };

    private static int DiatonicSteps(string interval) => interval switch
    {
        "R" => 0,
        "b2" or "2" or "b9" or "9" or "#9" => 1,
        "b3" or "3" => 2,
        "4" or "11" or "#11" => 3,
        "b5" or "5" => 4,
        "b6" or "6" or "b13" or "13" => 5,
        "bb7" or "b7" or "7" => 6,
        _ => 0
    };

    private static int NormalizeToNearestAccidental(int accidental)
    {
        while (accidental > 6)
        {
            accidental -= 12;
        }

        while (accidental < -6)
        {
            accidental += 12;
        }

        return accidental;
    }
}

public sealed record TriadTone(int PitchClass, string NoteName, string Function);
