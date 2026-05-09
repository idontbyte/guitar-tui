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

    public static int PitchClassFor(string note) => PitchClasses[note];

    public static string NameFor(int pitchClass) => SharpNames[Normalize(pitchClass)];

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
}

public sealed record TriadTone(int PitchClass, string NoteName, string Function);
