using GuitarResourcesTui.Fretboards;

namespace GuitarResourcesTui.Triads;

public sealed class TriadInversionLibrary
{
    private const int SearchFrets = 15;
    private const int WindowLength = 4;

    private static readonly IReadOnlyList<GuitarString> Tuning =
    [
        new("E", 4),
        new("B", 11),
        new("G", 7),
        new("D", 2),
        new("A", 9),
        new("E", 4)
    ];

    private static readonly IReadOnlyList<StringGrouping> Groupings =
    [
        new("E B G", [0, 1, 2]),
        new("B G D", [1, 2, 3]),
        new("G D A", [2, 3, 4]),
        new("D A E", [3, 4, 5])
    ];

    public IReadOnlyList<TriadGroupingResult> GetTriadInversions(string root, ChordQuality quality)
    {
        var chordTones = MusicTheory.BuildTriad(root, quality);
        var chordPitchClasses = chordTones.Select(tone => tone.PitchClass).ToHashSet();
        var results = new List<TriadGroupingResult>();

        foreach (var grouping in Groupings)
        {
            var positionsByString = grouping.StringIndexes
                .Select(stringIndex => PositionsForString(stringIndex, chordPitchClasses))
                .ToArray();

            var shapes = Cartesian(positionsByString)
                .Where(shape => shape.Select(position => position.PitchClass).Distinct().Count() == 3)
                .Where(shape => shape.Max(position => position.Fret) - shape.Min(position => position.Fret) <= WindowLength - 1)
                .Select(shape => BuildShape(grouping, shape, chordTones))
                .ToList();

            var inversionShapes = chordTones
                .Select(tone => PickBestShapeForInversion(shapes, tone.Function))
                .Where(shape => shape is not null)
                .Cast<TriadShape>()
                .ToArray();

            results.Add(new TriadGroupingResult(grouping.Name, inversionShapes));
        }

        return results;
    }

    private static TriadShape? PickBestShapeForInversion(IEnumerable<TriadShape> shapes, string bassFunction)
    {
        return shapes
            .Where(shape => shape.BassFunction == bassFunction)
            .OrderBy(shape => shape.MinFret == 0 ? 0 : shape.MinFret)
            .ThenBy(shape => shape.MaxFret)
            .FirstOrDefault();
    }

    private static TriadShape BuildShape(StringGrouping grouping, IReadOnlyList<StringPosition> shape, IReadOnlyList<TriadTone> chordTones)
    {
        var minFret = shape.Min(position => position.Fret);
        var maxFret = shape.Max(position => position.Fret);
        var startFret = minFret == 0 ? 0 : Math.Max(1, maxFret - WindowLength + 1);
        var strings = grouping.StringIndexes.Select(index => Tuning[index].Name).ToArray();
        var bassStringIndex = grouping.StringIndexes.Count - 1;
        var bassPitch = shape[bassStringIndex].PitchClass;
        var bassFunction = chordTones.Single(tone => tone.PitchClass == bassPitch).Function;

        var positions = shape.Select((position, displayStringIndex) =>
        {
            var tone = chordTones.Single(tone => tone.PitchClass == position.PitchClass);
            return new FretPosition(displayStringIndex, position.Fret, tone.Function, SourceStringIndex: position.StringIndex);
        }).ToArray();

        var diagram = new FretboardDiagram(strings, startFret, WindowLength, positions);
        return new TriadShape(MusicTheory.InversionName(bassFunction), bassFunction, minFret, maxFret, diagram);
    }

    private static IReadOnlyList<StringPosition> PositionsForString(int stringIndex, IReadOnlySet<int> chordPitchClasses)
    {
        var openPitch = Tuning[stringIndex].PitchClass;
        var positions = new List<StringPosition>();

        for (var fret = 0; fret <= SearchFrets; fret++)
        {
            var pitch = MusicTheory.Normalize(openPitch + fret);
            if (chordPitchClasses.Contains(pitch))
            {
                positions.Add(new StringPosition(stringIndex, fret, pitch));
            }
        }

        return positions;
    }

    private static IEnumerable<IReadOnlyList<StringPosition>> Cartesian(IReadOnlyList<IReadOnlyList<StringPosition>> sources)
    {
        IEnumerable<IReadOnlyList<StringPosition>> result = [[]];

        foreach (var source in sources)
        {
            result = result.SelectMany(existing => source.Select(item => existing.Concat([item]).ToArray()));
        }

        return result;
    }

    private sealed record GuitarString(string Name, int PitchClass);

    private sealed record StringGrouping(string Name, IReadOnlyList<int> StringIndexes);

    private sealed record StringPosition(int StringIndex, int Fret, int PitchClass);
}

public sealed record TriadGroupingResult(string Name, IReadOnlyList<TriadShape> Shapes);

public sealed record TriadShape(
    string InversionName,
    string BassFunction,
    int MinFret,
    int MaxFret,
    FretboardDiagram Diagram);
