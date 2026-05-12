using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.IntervalMaps;

public sealed class IntervalFunctionMapLibrary
{
    public const int DefaultWindowLength = 9;
    public const int MaxStartFret = 15;
    public const int MaxAnchorFret = 19;

    private static readonly IReadOnlyList<GuitarString> Tuning =
    [
        new("E", 4),
        new("B", 11),
        new("G", 7),
        new("D", 2),
        new("A", 9),
        new("E", 4)
    ];

    private static readonly IReadOnlyDictionary<int, string> LabelsByInterval = new Dictionary<int, string>
    {
        [0] = "R",
        [1] = "b2",
        [2] = "2",
        [3] = "b3",
        [4] = "3",
        [5] = "4",
        [6] = "b5",
        [7] = "5",
        [8] = "b6",
        [9] = "6",
        [10] = "b7",
        [11] = "7"
    };

    public static readonly IReadOnlyList<string> IntervalLabels = ["R", "b2", "2", "b3", "3", "4", "b5", "5", "b6", "6", "b7", "7"];

    public FretboardDiagram BuildMap(string root, int anchorFret, int length = DefaultWindowLength)
    {
        return BuildMap(root, anchorFret, labels: null, includeRoot: true, length);
    }

    public FretboardDiagram BuildLookup(
        string root,
        int anchorFret,
        IReadOnlySet<string> labels,
        bool includeRoot = true,
        int length = DefaultWindowLength)
    {
        return BuildMap(root, anchorFret, labels, includeRoot, length);
    }

    public static string NameForInterval(string label) => label switch
    {
        "R" => "root",
        "b2" => "flat two",
        "2" => "second",
        "b3" => "flat three",
        "3" => "third",
        "4" => "fourth",
        "b5" => "flat five",
        "5" => "fifth",
        "b6" => "flat six",
        "6" => "sixth",
        "b7" => "flat seven",
        "7" => "seventh",
        _ => label
    };

    private FretboardDiagram BuildMap(
        string root,
        int anchorFret,
        IReadOnlySet<string>? labels,
        bool includeRoot,
        int length)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Interval map length must be at least one fret.");
        }

        var clampedAnchorFret = Math.Clamp(anchorFret, 0, MaxAnchorFret);
        var clampedStartFret = Math.Clamp(clampedAnchorFret - length / 2, 0, MaxStartFret);
        var rootPitch = MusicTheory.PitchClassFor(root);
        var positions = new List<FretPosition>();

        for (var stringIndex = 0; stringIndex < Tuning.Count; stringIndex++)
        {
            for (var fret = clampedStartFret; fret < clampedStartFret + length; fret++)
            {
                var pitch = MusicTheory.Normalize(Tuning[stringIndex].PitchClass + fret);
                var interval = MusicTheory.Normalize(pitch - rootPitch);
                var label = LabelsByInterval[interval];
                if (labels is not null && !labels.Contains(label) && !(includeRoot && label == "R"))
                {
                    continue;
                }

                positions.Add(new FretPosition(
                    stringIndex,
                    fret,
                    label,
                    SourceStringIndex: stringIndex));
            }
        }

        return new FretboardDiagram(
            Tuning.Select(guitarString => guitarString.Name).ToArray(),
            clampedStartFret,
            length,
            positions);
    }

    private sealed record GuitarString(string Name, int PitchClass);
}
