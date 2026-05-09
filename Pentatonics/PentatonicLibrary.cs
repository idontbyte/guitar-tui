using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Pentatonics;

public sealed class PentatonicLibrary
{
    private const int FretSearchLimit = 24;
    private const int DiatonicWindowLength = 5;

    private static readonly IReadOnlyList<GuitarString> Tuning =
    [
        new("E", 4),
        new("B", 11),
        new("G", 7),
        new("D", 2),
        new("A", 9),
        new("E", 4)
    ];

    public static readonly IReadOnlyList<PentatonicScaleKind> ScaleKinds =
    [
        PentatonicScaleKind.MajorPentatonic,
        PentatonicScaleKind.MinorPentatonic,
        PentatonicScaleKind.MajorBlues,
        PentatonicScaleKind.MinorBlues,
        PentatonicScaleKind.MajorScale,
        PentatonicScaleKind.NaturalMinor,
        PentatonicScaleKind.Dorian,
        PentatonicScaleKind.Phrygian,
        PentatonicScaleKind.Lydian,
        PentatonicScaleKind.Mixolydian,
        PentatonicScaleKind.Locrian,
        PentatonicScaleKind.HarmonicMinor,
        PentatonicScaleKind.MelodicMinor
    ];

    private static readonly IReadOnlyDictionary<PentatonicScaleKind, ScaleDefinition> Scales = new Dictionary<PentatonicScaleKind, ScaleDefinition>
    {
        [PentatonicScaleKind.MajorPentatonic] = new(
            "Major pentatonic",
            [
                new(0, "R"),
                new(2, "2"),
                new(4, "3"),
                new(7, "5"),
                new(9, "6")
            ]),
        [PentatonicScaleKind.MinorPentatonic] = new(
            "Minor pentatonic",
            [
                new(0, "R"),
                new(3, "b3"),
                new(5, "4"),
                new(7, "5"),
                new(10, "b7")
            ]),
        [PentatonicScaleKind.MajorBlues] = new(
            "Major blues",
            [
                new(0, "R"),
                new(2, "2"),
                new(3, "b3"),
                new(4, "3"),
                new(7, "5"),
                new(9, "6")
            ],
            PentatonicScaleKind.MajorPentatonic),
        [PentatonicScaleKind.MinorBlues] = new(
            "Minor blues",
            [
                new(0, "R"),
                new(3, "b3"),
                new(5, "4"),
                new(6, "b5"),
                new(7, "5"),
                new(10, "b7")
            ],
            PentatonicScaleKind.MinorPentatonic),
        [PentatonicScaleKind.MajorScale] = new(
            "Major scale (Ionian)",
            [
                new(0, "R"),
                new(2, "2"),
                new(4, "3"),
                new(5, "4"),
                new(7, "5"),
                new(9, "6"),
                new(11, "7")
            ]),
        [PentatonicScaleKind.NaturalMinor] = new(
            "Natural minor (Aeolian)",
            [
                new(0, "R"),
                new(2, "2"),
                new(3, "b3"),
                new(5, "4"),
                new(7, "5"),
                new(8, "b6"),
                new(10, "b7")
            ]),
        [PentatonicScaleKind.Dorian] = new(
            "Dorian",
            [
                new(0, "R"),
                new(2, "2"),
                new(3, "b3"),
                new(5, "4"),
                new(7, "5"),
                new(9, "6"),
                new(10, "b7")
            ]),
        [PentatonicScaleKind.Phrygian] = new(
            "Phrygian",
            [
                new(0, "R"),
                new(1, "b2"),
                new(3, "b3"),
                new(5, "4"),
                new(7, "5"),
                new(8, "b6"),
                new(10, "b7")
            ]),
        [PentatonicScaleKind.Lydian] = new(
            "Lydian",
            [
                new(0, "R"),
                new(2, "2"),
                new(4, "3"),
                new(6, "#4"),
                new(7, "5"),
                new(9, "6"),
                new(11, "7")
            ]),
        [PentatonicScaleKind.Mixolydian] = new(
            "Mixolydian",
            [
                new(0, "R"),
                new(2, "2"),
                new(4, "3"),
                new(5, "4"),
                new(7, "5"),
                new(9, "6"),
                new(10, "b7")
            ]),
        [PentatonicScaleKind.Locrian] = new(
            "Locrian",
            [
                new(0, "R"),
                new(1, "b2"),
                new(3, "b3"),
                new(5, "4"),
                new(6, "b5"),
                new(8, "b6"),
                new(10, "b7")
            ]),
        [PentatonicScaleKind.HarmonicMinor] = new(
            "Harmonic minor",
            [
                new(0, "R"),
                new(2, "2"),
                new(3, "b3"),
                new(5, "4"),
                new(7, "5"),
                new(8, "b6"),
                new(11, "7")
            ]),
        [PentatonicScaleKind.MelodicMinor] = new(
            "Melodic minor",
            [
                new(0, "R"),
                new(2, "2"),
                new(3, "b3"),
                new(5, "4"),
                new(7, "5"),
                new(9, "6"),
                new(11, "7")
            ])
    };

    public IReadOnlyList<PentatonicShape> GetShapes(string root, ChordQuality quality)
    {
        var kind = quality == ChordQuality.Major
            ? PentatonicScaleKind.MajorPentatonic
            : PentatonicScaleKind.MinorPentatonic;

        return GetShapes(root, kind);
    }

    public IReadOnlyList<PentatonicShape> GetShapes(string root, PentatonicScaleKind kind)
    {
        var rootPitch = MusicTheory.PitchClassFor(root);
        var scale = Scales[kind];
        var anchorIntervals = Scales[scale.AnchorKind].Intervals;
        var rootFret = FirstFretForPitch(Tuning[^1].PitchClass, rootPitch);
        var anchors = BuildAnchors(rootFret, anchorIntervals);

        return anchors.Select((anchor, index) =>
        {
            var positions = BuildShapePositions(rootPitch, scale.Intervals, anchorIntervals, anchor);
            var minFret = positions.Min(position => position.Fret);
            var maxFret = positions.Max(position => position.Fret);
            var diagram = new FretboardDiagram(
                Tuning.Select(guitarString => guitarString.Name).ToArray(),
                minFret,
                maxFret - minFret + 1,
                positions);

            return new PentatonicShape(index + 1, minFret, maxFret, diagram);
        }).ToArray();
    }

    public static string NameFor(PentatonicScaleKind kind) => Scales[kind].Name;

    public static PentatonicScaleKind ToggleMajorMinor(PentatonicScaleKind kind) => kind switch
    {
        PentatonicScaleKind.MajorPentatonic => PentatonicScaleKind.MinorPentatonic,
        PentatonicScaleKind.MinorPentatonic => PentatonicScaleKind.MajorPentatonic,
        PentatonicScaleKind.MajorBlues => PentatonicScaleKind.MinorBlues,
        PentatonicScaleKind.MinorBlues => PentatonicScaleKind.MajorBlues,
        PentatonicScaleKind.MajorScale => PentatonicScaleKind.NaturalMinor,
        PentatonicScaleKind.NaturalMinor => PentatonicScaleKind.MajorScale,
        _ => kind
    };

    private static IReadOnlyList<PentatonicAnchor> BuildAnchors(int rootFret, IReadOnlyList<PentatonicInterval> intervals)
    {
        var anchors = intervals
            .Select((interval, degree) => new PentatonicAnchor(degree, rootFret + interval.Semitones))
            .ToArray();

        var lastDegreeFret = rootFret - (12 - intervals[^1].Semitones);
        if (lastDegreeFret < 0)
        {
            lastDegreeFret += 12;
        }

        anchors[^1] = new PentatonicAnchor(intervals.Count - 1, lastDegreeFret);
        return anchors;
    }

    private static IReadOnlyList<FretPosition> BuildShapePositions(
        int rootPitch,
        IReadOnlyList<PentatonicInterval> scaleIntervals,
        IReadOnlyList<PentatonicInterval> anchorIntervals,
        PentatonicAnchor anchor)
    {
        if (anchorIntervals.Count >= 7)
        {
            return BuildWindowShapePositions(rootPitch, scaleIntervals, anchor.Fret, DiatonicWindowLength);
        }

        var nextAnchorFret = NextAnchorFret(anchor, anchorIntervals);
        var targetCenter = (anchor.Fret + nextAnchorFret) / 2.0;
        var labelsByPitchClass = scaleIntervals
            .ToDictionary(interval => MusicTheory.Normalize(rootPitch + interval.Semitones), interval => interval.Label);
        var scalePitchClasses = labelsByPitchClass.Keys.ToHashSet();
        var anchorPitchClasses = anchorIntervals
            .Select(interval => MusicTheory.Normalize(rootPitch + interval.Semitones))
            .ToHashSet();
        var positions = new List<FretPosition>();

        for (var displayString = 0; displayString < Tuning.Count; displayString++)
        {
            var anchorPositions = ScalePositionsForString(Tuning[displayString], anchorPitchClasses);
            var stringPositions = ScalePositionsForString(Tuning[displayString], scalePitchClasses);
            var pair = PickPairForBox(anchorPositions, anchor.Fret, nextAnchorFret, targetCenter);

            foreach (var fret in stringPositions.Where(fret => fret >= pair[0] && fret <= pair[1]))
            {
                var pitch = MusicTheory.Normalize(Tuning[displayString].PitchClass + fret);
                var label = labelsByPitchClass[pitch];
                positions.Add(new FretPosition(displayString, fret, label, SourceStringIndex: displayString));
            }
        }

        return positions;
    }

    private static IReadOnlyList<FretPosition> BuildWindowShapePositions(
        int rootPitch,
        IReadOnlyList<PentatonicInterval> scaleIntervals,
        int startFret,
        int length)
    {
        var labelsByPitchClass = scaleIntervals
            .ToDictionary(interval => MusicTheory.Normalize(rootPitch + interval.Semitones), interval => interval.Label);
        var scalePitchClasses = labelsByPitchClass.Keys.ToHashSet();
        var positions = new List<FretPosition>();

        for (var displayString = 0; displayString < Tuning.Count; displayString++)
        {
            var stringPositions = ScalePositionsForString(Tuning[displayString], scalePitchClasses);

            foreach (var fret in stringPositions.Where(fret => fret >= startFret && fret < startFret + length))
            {
                var pitch = MusicTheory.Normalize(Tuning[displayString].PitchClass + fret);
                var label = labelsByPitchClass[pitch];
                positions.Add(new FretPosition(displayString, fret, label, SourceStringIndex: displayString));
            }
        }

        return positions;
    }

    private static int NextAnchorFret(PentatonicAnchor anchor, IReadOnlyList<PentatonicInterval> intervals)
    {
        if (anchor.Degree == intervals.Count - 1)
        {
            return anchor.Fret + (12 - intervals[^1].Semitones);
        }

        return anchor.Fret + (intervals[anchor.Degree + 1].Semitones - intervals[anchor.Degree].Semitones);
    }

    private static IReadOnlyList<int> ScalePositionsForString(GuitarString guitarString, IReadOnlySet<int> scalePitchClasses)
    {
        var positions = new List<int>();

        for (var fret = 0; fret <= FretSearchLimit; fret++)
        {
            var pitch = MusicTheory.Normalize(guitarString.PitchClass + fret);
            if (scalePitchClasses.Contains(pitch))
            {
                positions.Add(fret);
            }
        }

        return positions;
    }

    private static IReadOnlyList<int> PickPairForBox(IReadOnlyList<int> positions, int anchorFret, int nextAnchorFret, double targetCenter)
    {
        var candidates = positions
            .Zip(positions.Skip(1), (first, second) => new ScalePair(first, second))
            .Where(pair => pair.First >= anchorFret - 1)
            .Where(pair => pair.Second <= nextAnchorFret + 1)
            .ToArray();

        if (candidates.Length == 0)
        {
            candidates = positions
                .Zip(positions.Skip(1), (first, second) => new ScalePair(first, second))
                .Where(pair => pair.Second - pair.First <= 4)
                .ToArray();
        }

        var selected = candidates
            .OrderBy(pair => Math.Abs(pair.Center - targetCenter))
            .ThenBy(pair => pair.First)
            .First();

        return [selected.First, selected.Second];
    }

    private static int FirstFretForPitch(int openPitch, int targetPitch)
    {
        for (var fret = 0; fret < 12; fret++)
        {
            if (MusicTheory.Normalize(openPitch + fret) == targetPitch)
            {
                return fret;
            }
        }

        throw new InvalidOperationException("Pitch class lookup failed.");
    }

    private sealed record GuitarString(string Name, int PitchClass);

    private sealed record PentatonicInterval(int Semitones, string Label);

    private sealed record ScaleDefinition(
        string Name,
        IReadOnlyList<PentatonicInterval> Intervals,
        PentatonicScaleKind? AnchorOverride = null)
    {
        public PentatonicScaleKind AnchorKind => AnchorOverride ?? InferKindFromName();

        private PentatonicScaleKind InferKindFromName() => Name switch
        {
            "Major pentatonic" => PentatonicScaleKind.MajorPentatonic,
            "Minor pentatonic" => PentatonicScaleKind.MinorPentatonic,
            "Major scale (Ionian)" => PentatonicScaleKind.MajorScale,
            "Natural minor (Aeolian)" => PentatonicScaleKind.NaturalMinor,
            "Dorian" => PentatonicScaleKind.Dorian,
            "Phrygian" => PentatonicScaleKind.Phrygian,
            "Lydian" => PentatonicScaleKind.Lydian,
            "Mixolydian" => PentatonicScaleKind.Mixolydian,
            "Locrian" => PentatonicScaleKind.Locrian,
            "Harmonic minor" => PentatonicScaleKind.HarmonicMinor,
            "Melodic minor" => PentatonicScaleKind.MelodicMinor,
            _ => throw new InvalidOperationException($"Scale {Name} needs an explicit anchor kind.")
        };
    }

    private sealed record PentatonicAnchor(int Degree, int Fret);

    private sealed record ScalePair(int First, int Second)
    {
        public double Center => (First + Second) / 2.0;
    }
}

public sealed record PentatonicShape(int Number, int MinFret, int MaxFret, FretboardDiagram Diagram);
