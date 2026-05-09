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

    public FretboardDiagram BuildMap(string root, int anchorFret, int length = DefaultWindowLength)
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
                positions.Add(new FretPosition(
                    stringIndex,
                    fret,
                    LabelsByInterval[interval],
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
