using System.Text.RegularExpressions;
using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.IntervalMaps;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Triads;

new TriadInversionTests().RunAll();
new PentatonicShapeTests().RunAll();
new IntervalFunctionMapTests().RunAll();
Console.WriteLine("All tests passed.");

internal sealed class TriadInversionTests
{
    private static readonly IReadOnlyList<string> ExpectedGroupingOrder = ["E B G", "B G D", "G D A", "D A E"];

    private static readonly IReadOnlyDictionary<int, int> OpenPitchByStringIndex = new Dictionary<int, int>
    {
        [0] = 4,
        [1] = 11,
        [2] = 7,
        [3] = 2,
        [4] = 9,
        [5] = 4
    };

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ExpectedDisplayOrder = new Dictionary<string, IReadOnlyList<string>>
    {
        ["E B G"] = ["E", "B", "G"],
        ["B G D"] = ["B", "G", "D"],
        ["G D A"] = ["G", "D", "A"],
        ["D A E"] = ["D", "A", "E"]
    };

    public void RunAll()
    {
        var library = new TriadInversionLibrary();

        foreach (var root in MusicTheory.NaturalRoots)
        {
            foreach (var quality in Enum.GetValues<ChordQuality>())
            {
                var results = library.GetTriadInversions(root, quality);
                AssertEqual(4, results.Count, $"{root} {quality} has four string groupings");
                AssertSequenceEqual(ExpectedGroupingOrder, results.Select(result => result.Name), $"{root} {quality} string grouping order");

                foreach (var grouping in results)
                {
                    AssertDisplayOrder(grouping);
                    AssertEqual(3, grouping.Shapes.Count, $"{root} {quality} {grouping.Name} has three inversions");

                    foreach (var shape in grouping.Shapes)
                    {
                        AssertShapeUsesCorrectTriadTones(root, quality, grouping.Name, shape);
                        AssertBassMatchesInversion(grouping.Name, shape);
                        AssertCompactWindow(grouping.Name, shape);
                    }
                }
            }
        }
    }

    private static void AssertDisplayOrder(TriadGroupingResult grouping)
    {
        var expected = ExpectedDisplayOrder[grouping.Name];

        foreach (var shape in grouping.Shapes)
        {
            AssertSequenceEqual(expected, shape.Diagram.Strings, $"{grouping.Name} displays thinnest string on top");
        }
    }

    private static void AssertShapeUsesCorrectTriadTones(string root, ChordQuality quality, string groupingName, TriadShape shape)
    {
        var tones = MusicTheory.BuildTriad(root, quality).ToDictionary(tone => tone.Function, tone => tone.PitchClass);
        AssertSequenceEqual(["R", "3", "5"], shape.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{root} {quality} {groupingName} {shape.InversionName} has R, 3, and 5");

        foreach (var position in shape.Diagram.Positions)
        {
            Assert(position.SourceStringIndex is not null, $"{groupingName} {shape.InversionName} records source string index");

            var actualPitch = MusicTheory.Normalize(OpenPitchByStringIndex[position.SourceStringIndex!.Value] + position.Fret);
            var expectedPitch = tones[position.Label];
            AssertEqual(expectedPitch, actualPitch, $"{root} {quality} {groupingName} {shape.InversionName} {position.Label} pitch");
        }
    }

    private static void AssertBassMatchesInversion(string groupingName, TriadShape shape)
    {
        var bassPosition = shape.Diagram.Positions.MaxBy(position => position.SourceStringIndex);
        Assert(bassPosition is not null, $"{groupingName} {shape.InversionName} has a bass position");
        AssertEqual(shape.BassFunction, bassPosition!.Label, $"{groupingName} {shape.InversionName} bass matches inversion");
    }

    private static void AssertCompactWindow(string groupingName, TriadShape shape)
    {
        Assert(shape.MaxFret - shape.MinFret <= 3, $"{groupingName} {shape.InversionName} fits within four frets");

        foreach (var position in shape.Diagram.Positions)
        {
            Assert(position.Fret >= shape.Diagram.StartFret, $"{groupingName} {shape.InversionName} position starts inside diagram");
            Assert(position.Fret < shape.Diagram.StartFret + shape.Diagram.Length, $"{groupingName} {shape.InversionName} position ends inside diagram");
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message}. Expected {expected}, got {actual}.");
        }
    }

    private static void AssertSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
    {
        var expectedArray = expected.ToArray();
        var actualArray = actual.ToArray();

        if (!expectedArray.SequenceEqual(actualArray))
        {
            throw new InvalidOperationException($"{message}. Expected [{string.Join(", ", expectedArray)}], got [{string.Join(", ", actualArray)}].");
        }
    }

    private static int LabelSortOrder(string label) => label switch
    {
        "R" => 0,
        "3" => 1,
        "5" => 2,
        _ => 99
    };
}

internal sealed class PentatonicShapeTests
{
    private static readonly IReadOnlyDictionary<int, int> OpenPitchByStringIndex = new Dictionary<int, int>
    {
        [0] = 4,
        [1] = 11,
        [2] = 7,
        [3] = 2,
        [4] = 9,
        [5] = 4
    };

    private static readonly IReadOnlyDictionary<PentatonicScaleKind, IReadOnlyList<int>> ExpectedIntervals = new Dictionary<PentatonicScaleKind, IReadOnlyList<int>>
    {
        [PentatonicScaleKind.MajorPentatonic] = [0, 2, 4, 7, 9],
        [PentatonicScaleKind.MinorPentatonic] = [0, 3, 5, 7, 10],
        [PentatonicScaleKind.MajorBlues] = [0, 2, 3, 4, 7, 9],
        [PentatonicScaleKind.MinorBlues] = [0, 3, 5, 6, 7, 10],
        [PentatonicScaleKind.MajorScale] = [0, 2, 4, 5, 7, 9, 11],
        [PentatonicScaleKind.NaturalMinor] = [0, 2, 3, 5, 7, 8, 10],
        [PentatonicScaleKind.Dorian] = [0, 2, 3, 5, 7, 9, 10],
        [PentatonicScaleKind.Phrygian] = [0, 1, 3, 5, 7, 8, 10],
        [PentatonicScaleKind.Lydian] = [0, 2, 4, 6, 7, 9, 11],
        [PentatonicScaleKind.Mixolydian] = [0, 2, 4, 5, 7, 9, 10],
        [PentatonicScaleKind.Locrian] = [0, 1, 3, 5, 6, 8, 10],
        [PentatonicScaleKind.HarmonicMinor] = [0, 2, 3, 5, 7, 8, 11],
        [PentatonicScaleKind.MelodicMinor] = [0, 2, 3, 5, 7, 9, 11]
    };

    private static readonly IReadOnlyDictionary<PentatonicScaleKind, IReadOnlyDictionary<int, string>> ExpectedLabels = new Dictionary<PentatonicScaleKind, IReadOnlyDictionary<int, string>>
    {
        [PentatonicScaleKind.MajorPentatonic] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [4] = "3",
            [7] = "5",
            [9] = "6"
        },
        [PentatonicScaleKind.MinorPentatonic] = new Dictionary<int, string>
        {
            [0] = "R",
            [3] = "b3",
            [5] = "4",
            [7] = "5",
            [10] = "b7"
        },
        [PentatonicScaleKind.MajorBlues] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [3] = "b3",
            [4] = "3",
            [7] = "5",
            [9] = "6"
        },
        [PentatonicScaleKind.MinorBlues] = new Dictionary<int, string>
        {
            [0] = "R",
            [3] = "b3",
            [5] = "4",
            [6] = "b5",
            [7] = "5",
            [10] = "b7"
        },
        [PentatonicScaleKind.MajorScale] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [4] = "3",
            [5] = "4",
            [7] = "5",
            [9] = "6",
            [11] = "7"
        },
        [PentatonicScaleKind.NaturalMinor] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [3] = "b3",
            [5] = "4",
            [7] = "5",
            [8] = "b6",
            [10] = "b7"
        },
        [PentatonicScaleKind.Dorian] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [3] = "b3",
            [5] = "4",
            [7] = "5",
            [9] = "6",
            [10] = "b7"
        },
        [PentatonicScaleKind.Phrygian] = new Dictionary<int, string>
        {
            [0] = "R",
            [1] = "b2",
            [3] = "b3",
            [5] = "4",
            [7] = "5",
            [8] = "b6",
            [10] = "b7"
        },
        [PentatonicScaleKind.Lydian] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [4] = "3",
            [6] = "#4",
            [7] = "5",
            [9] = "6",
            [11] = "7"
        },
        [PentatonicScaleKind.Mixolydian] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [4] = "3",
            [5] = "4",
            [7] = "5",
            [9] = "6",
            [10] = "b7"
        },
        [PentatonicScaleKind.Locrian] = new Dictionary<int, string>
        {
            [0] = "R",
            [1] = "b2",
            [3] = "b3",
            [5] = "4",
            [6] = "b5",
            [8] = "b6",
            [10] = "b7"
        },
        [PentatonicScaleKind.HarmonicMinor] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [3] = "b3",
            [5] = "4",
            [7] = "5",
            [8] = "b6",
            [11] = "7"
        },
        [PentatonicScaleKind.MelodicMinor] = new Dictionary<int, string>
        {
            [0] = "R",
            [2] = "2",
            [3] = "b3",
            [5] = "4",
            [7] = "5",
            [9] = "6",
            [11] = "7"
        }
    };

    private static readonly Regex AnsiPattern = new(@"\e\[[0-9;]*m", RegexOptions.Compiled);

    public void RunAll()
    {
        var library = new PentatonicLibrary();

        foreach (var root in MusicTheory.ChromaticRoots)
        {
            foreach (var scaleKind in PentatonicLibrary.ScaleKinds)
            {
                var shapes = library.GetShapes(root, scaleKind);
                var expectedShapeCount = ExpectedShapeCount(scaleKind);
                TestAssert.Equal(expectedShapeCount, shapes.Count, $"{root} {scaleKind} has expected scale shape count");
                TestAssert.SequenceEqual(Enumerable.Range(1, expectedShapeCount), shapes.Select(shape => shape.Number), $"{root} {scaleKind} shape numbering");
                AssertRendererWraps(root, scaleKind, shapes);

                foreach (var shape in shapes)
                {
                    AssertPentatonicShape(root, scaleKind, shape);
                }
            }
        }
    }

    private static int ExpectedShapeCount(PentatonicScaleKind scaleKind) => scaleKind switch
    {
        PentatonicScaleKind.MajorPentatonic => 5,
        PentatonicScaleKind.MinorPentatonic => 5,
        PentatonicScaleKind.MajorBlues => 5,
        PentatonicScaleKind.MinorBlues => 5,
        _ => 7
    };

    private static void AssertRendererWraps(string root, PentatonicScaleKind scaleKind, IReadOnlyList<PentatonicShape> shapes)
    {
        var renderer = new FretboardRenderer();
        var diagrams = shapes
            .Select(shape => ($"Shape {shape.Number} ({shape.MinFret}-{shape.MaxFret})", shape.Diagram))
            .ToArray();
        var lines = renderer.RenderMany(diagrams, maxWidth: 80);

        TestAssert.True(lines.Any(string.IsNullOrEmpty), $"{root} {scaleKind} scale shapes wrap onto multiple rows at narrow width");

        foreach (var line in lines.Where(line => line.Length > 0))
        {
            var visibleLength = AnsiPattern.Replace(line, string.Empty).Length;
            TestAssert.True(visibleLength <= 80, $"{root} {scaleKind} wrapped scale line fits requested width");
        }
    }

    private static void AssertPentatonicShape(string root, PentatonicScaleKind scaleKind, PentatonicShape shape)
    {
        var rootPitch = MusicTheory.PitchClassFor(root);
        var scalePitchClasses = ExpectedIntervals[scaleKind]
            .Select(interval => MusicTheory.Normalize(rootPitch + interval))
            .ToHashSet();

        TestAssert.SequenceEqual(["E", "B", "G", "D", "A", "E"], shape.Diagram.Strings, $"{root} {scaleKind} shape {shape.Number} string order");
        TestAssert.True(shape.Diagram.Positions.Count >= 12, $"{root} {scaleKind} shape {shape.Number} has at least two notes per string");
        TestAssert.True(shape.Diagram.Positions.Any(position => position.Label == "R"), $"{root} {scaleKind} shape {shape.Number} contains a root");

        foreach (var position in shape.Diagram.Positions)
        {
            TestAssert.True(position.SourceStringIndex is not null, $"{root} {scaleKind} shape {shape.Number} records source string index");

            var actualPitch = MusicTheory.Normalize(OpenPitchByStringIndex[position.SourceStringIndex!.Value] + position.Fret);
            TestAssert.True(scalePitchClasses.Contains(actualPitch), $"{root} {scaleKind} shape {shape.Number} position is in the scale");

            var interval = MusicTheory.Normalize(actualPitch - rootPitch);
            var expectedLabel = ExpectedLabels[scaleKind][interval];
            TestAssert.Equal(expectedLabel, position.Label, $"{root} {scaleKind} shape {shape.Number} labels intervals correctly");
            TestAssert.True(position.Fret >= shape.Diagram.StartFret, $"{root} {scaleKind} shape {shape.Number} position starts inside diagram");
            TestAssert.True(position.Fret < shape.Diagram.StartFret + shape.Diagram.Length, $"{root} {scaleKind} shape {shape.Number} position ends inside diagram");
        }
    }
}

internal sealed class IntervalFunctionMapTests
{
    private static readonly IReadOnlyDictionary<int, int> OpenPitchByStringIndex = new Dictionary<int, int>
    {
        [0] = 4,
        [1] = 11,
        [2] = 7,
        [3] = 2,
        [4] = 9,
        [5] = 4
    };

    private static readonly IReadOnlyDictionary<int, string> ExpectedLabels = new Dictionary<int, string>
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

    public void RunAll()
    {
        var library = new IntervalFunctionMapLibrary();

        foreach (var root in MusicTheory.ChromaticRoots)
        {
            foreach (var startFret in new[] { 0, 3, 5, 7, 12, 19 })
            {
                AssertIntervalMap(root, startFret, library.BuildMap(root, startFret));
            }
        }

        var clamped = library.BuildMap("A", 30);
        TestAssert.Equal(IntervalFunctionMapLibrary.MaxStartFret, clamped.StartFret, "Interval map clamps high start frets");
    }

    private static void AssertIntervalMap(string root, int startFret, FretboardDiagram diagram)
    {
        var rootPitch = MusicTheory.PitchClassFor(root);
        TestAssert.Equal(startFret, diagram.StartFret, $"{root} interval map start fret");
        TestAssert.Equal(IntervalFunctionMapLibrary.DefaultWindowLength, diagram.Length, $"{root} interval map length");
        TestAssert.SequenceEqual(["E", "B", "G", "D", "A", "E"], diagram.Strings, $"{root} interval map string order");
        TestAssert.Equal(30, diagram.Positions.Count, $"{root} interval map labels every fret in the window");

        foreach (var position in diagram.Positions)
        {
            TestAssert.True(position.SourceStringIndex is not null, $"{root} interval map records source string index");
            TestAssert.True(position.Fret >= diagram.StartFret, $"{root} interval map position starts inside diagram");
            TestAssert.True(position.Fret < diagram.StartFret + diagram.Length, $"{root} interval map position ends inside diagram");

            var pitch = MusicTheory.Normalize(OpenPitchByStringIndex[position.SourceStringIndex!.Value] + position.Fret);
            var interval = MusicTheory.Normalize(pitch - rootPitch);
            TestAssert.Equal(ExpectedLabels[interval], position.Label, $"{root} interval map label at string {position.SourceStringIndex} fret {position.Fret}");
        }
    }
}

internal static class TestAssert
{
    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void Equal<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message}. Expected {expected}, got {actual}.");
        }
    }

    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
    {
        var expectedArray = expected.ToArray();
        var actualArray = actual.ToArray();

        if (!expectedArray.SequenceEqual(actualArray))
        {
            throw new InvalidOperationException($"{message}. Expected [{string.Join(", ", expectedArray)}], got [{string.Join(", ", actualArray)}].");
        }
    }
}
