using System.Text.RegularExpressions;
using GuitarResourcesTui.Arpeggios;
using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.IntervalMaps;
using GuitarResourcesTui.JazzChords;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Practice;
using GuitarResourcesTui.Shredding;
using GuitarResourcesTui.Triads;
using GuitarResourcesTui.Tui;

new TriadInversionTests().RunAll();
new FretboardRendererTests().RunAll();
new TriadProgressionGameTests().RunAll();
new BackingSynthTests().RunAll();
new PentatonicShapeTests().RunAll();
new IntervalFunctionMapTests().RunAll();
new JazzChordLibraryTests().RunAll();
new ArpeggioLibraryTests().RunAll();
new PracticeCoachPlannerTests().RunAll();
new ShredLibraryTests().RunAll();
Console.WriteLine("All tests passed.");

internal sealed class FretboardRendererTests
{
    public void RunAll()
    {
        var renderer = new FretboardRenderer();
        var diagram = new FretboardDiagram(
            ["E"],
            StartFret: 1,
            Length: 2,
            [new FretPosition(0, 1, "R")]);

        var highlighted = renderer.Render(diagram, highlighted: true);

        TestAssert.True(
            highlighted.Any(line => line.Contains("\e[38;5;16;48;5;250m----", StringComparison.Ordinal)),
            "highlighted empty frets keep dark string lines against the light background");
        TestAssert.True(
            highlighted.Any(line => line.Contains("\e[38;5;16;48;5;250m|", StringComparison.Ordinal)),
            "highlighted fret separators keep dark contrast against the light background");

        renderer.NoteLabelMode = NoteLabelMode.FretNumbers;
        var fretNumberLines = renderer.Render(diagram);
        TestAssert.True(
            fretNumberLines.Any(line => line.Contains("\e[1;38;5;46m  1 ", StringComparison.Ordinal)),
            "fret-number note labels keep the interval color");

        renderer.NoteLabelMode = NoteLabelMode.Markers;
        var markerLines = renderer.Render(diagram);
        TestAssert.True(
            markerLines.Any(line => line.Contains("\e[1;38;5;46m  X ", StringComparison.Ordinal)),
            "marker note labels keep the interval color");

        var accidentalDiagram = new FretboardDiagram(
            ["E"],
            StartFret: 1,
            Length: 2,
            [new FretPosition(0, 1, "b3")]);

        renderer.NoteLabelMode = NoteLabelMode.IntervalNames;
        renderer.AccidentalDisplayMode = AccidentalDisplayMode.Unicode;
        TestAssert.True(
            renderer.Render(accidentalDiagram).Any(line => line.Contains("♭3", StringComparison.Ordinal)),
            "unicode accidental mode renders compact flat symbols");

        renderer.AccidentalDisplayMode = AccidentalDisplayMode.Ascii;
        TestAssert.True(
            renderer.Render(accidentalDiagram).Any(line => line.Contains("b3", StringComparison.Ordinal)),
            "ascii accidental mode renders Windows-safe flat labels");
    }
}

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

internal sealed class TriadProgressionGameTests
{
    public void RunAll()
    {
        var game = new TriadProgressionGameLibrary(new TriadInversionLibrary(), new Random(7));

        var progression = game.ParseProgression("Am, C G D");
        AssertEqual(4, progression.Count, "progression parser accepts commas and spaces");
        AssertEqual("A", progression[0].Root, "Am root");
        AssertEqual(ChordQuality.Minor, progression[0].Quality, "Am quality");
        AssertEqual("C", progression[1].DisplayName, "C display name");
        AssertEqual("G", progression[2].DisplayName, "G display name");
        AssertEqual("D", progression[3].DisplayName, "D display name");
        AssertEqual("A#", game.ParseProgression("Bb")[0].Root, "flat roots normalize to sharp names");

        var phrase = game.BuildPhrase(progression);
        AssertEqual(progression.Count, phrase.Count, "phrase has one triad per chord");

        for (var index = 0; index < phrase.Count; index++)
        {
            var item = phrase[index];
            AssertEqual(progression[index], item.Chord, "phrase keeps progression order");
            Assert(item.Shape.MaxFret - item.Shape.MinFret <= 3, $"{item.Title} remains compact");
            AssertSequenceEqual(["E", "B", "G", "D", "A", "E"], item.Diagram.Strings, $"{item.Title} renders on the full fretboard");
            AssertSequenceEqual(["R", "3", "5"], item.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{item.Title} contains a full triad");
        }

        var nextPhrase = game.BuildPhrase(progression, phrase[^1]);
        AssertEqual(progression.Count, nextPhrase.Count, "next phrase has one triad per chord");
        Assert(Math.Abs(nextPhrase[0].CenterFret - phrase[^1].CenterFret) <= 6, "next phrase starts near previous phrase");
        AssertFilteredPhrase(game, progression, TriadInversionFilter.RootPosition, "R");
        AssertFilteredPhrase(game, progression, TriadInversionFilter.FirstInversion, "3");
        AssertFilteredPhrase(game, progression, TriadInversionFilter.SecondInversion, "5");
        AssertEqual(TriadInversionFilter.RootPosition, TriadInversionFilter.All.Next(), "inversion filter cycles from all to root");
        AssertEqual(TriadInversionFilter.FirstInversion, TriadInversionFilter.RootPosition.Next(), "inversion filter cycles from root to first inversion");
        AssertEqual(TriadInversionFilter.SecondInversion, TriadInversionFilter.FirstInversion.Next(), "inversion filter cycles from first to second inversion");
        AssertEqual(TriadInversionFilter.All, TriadInversionFilter.SecondInversion.Next(), "inversion filter cycles back to all");

        AssertEqual(100, TriadProgressionGameLibrary.PresetProgressions.Count, "game has 100 preset progressions");
        foreach (var preset in TriadProgressionGameLibrary.PresetProgressions)
        {
            AssertEqual(preset.Number, TriadProgressionGameLibrary.PresetProgressions[preset.Number - 1].Number, $"{preset.Name} preset number matches list position");
            var presetProgression = game.ParseProgression(preset.ProgressionText);
            Assert(presetProgression.Count > 0, $"{preset.Name} parses");
            AssertEqual(presetProgression.Count, preset.ChordLengths.Count, $"{preset.Name} has one length per chord");
            Assert(preset.Bpm >= 30 && preset.Bpm <= 240, $"{preset.Name} BPM is playable");
            Assert(preset.TimeSignature.BeatsPerBar > 0, $"{preset.Name} has a meter");
        }
        Assert(TryFindPreset(TriadProgressionGameLibrary.PresetProgressions, "089. Hey Joe - Jimi Hendrix: C G D A E", allowListPosition: false, out var heyJoe), "preset lookup accepts copied menu lines");
        AssertEqual(89, heyJoe.Number, "copied Hey Joe menu line selects song 89");
        Assert(TryFindPreset(TriadProgressionGameLibrary.PresetProgressions, "Hey Joe", allowListPosition: false, out heyJoe), "preset lookup accepts song titles");
        AssertEqual(89, heyJoe.Number, "Hey Joe title selects song 89");
        var cowboySongs = TriadProgressionGameLibrary.PresetProgressions
            .Where(preset => preset.ProgressionText
                .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .All(chord => new[] { "A", "Am", "A7", "B7", "C", "C7", "D", "Dm", "D7", "E", "Em", "E7", "F", "G", "G7" }.Contains(chord)))
            .ToArray();
        Assert(TryFindPreset(cowboySongs, "15", allowListPosition: true, out var cowboySong), "filtered preset lookup accepts visible list positions");
        AssertEqual(cowboySongs[14].Number, cowboySong.Number, "filtered list position 15 selects the 15th visible song");

        AssertSequenceEqual([4, 2, 2, 8], game.ParseChordLengths("4 2 2 8", 4, 4), "custom chord lengths parse");
        AssertSequenceEqual([3, 3, 3, 3], game.ParseChordLengths("", 4, 3), "blank chord lengths use default");

        var spreadGame = new TriadProgressionGameLibrary(new TriadInversionLibrary(), new Random(7), TriadVoicingKind.Spread);
        var spreadPhrase = spreadGame.BuildPhrase(progression);
        AssertEqual(progression.Count, spreadPhrase.Count, "spread phrase has one triad per chord");

        foreach (var item in spreadPhrase)
        {
            Assert(new[] { "E B D", "B G A", "G D E" }.Contains(item.GroupingName), $"{item.Title} uses a spread string grouping");
            Assert(item.Shape.MaxFret - item.Shape.MinFret <= 4, $"{item.Title} fits in a reachable spread window");
            AssertSequenceEqual(["E", "B", "G", "D", "A", "E"], item.Diagram.Strings, $"{item.Title} renders on the full fretboard");
            AssertSequenceEqual(["R", "3", "5"], item.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{item.Title} contains a full spread triad");
            Assert(new[] { "R53", "3R5", "53R" }.Contains(LowToHighFunctions(item)), $"{item.Title} uses a real spread-triad order");
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

    private static void AssertFilteredPhrase(
        TriadProgressionGameLibrary game,
        IReadOnlyList<ChordSymbol> progression,
        TriadInversionFilter filter,
        string expectedBassFunction)
    {
        var phrase = game.BuildPhrase(progression, inversionFilter: filter);
        AssertEqual(progression.Count, phrase.Count, $"{filter} phrase has one triad per chord");

        foreach (var item in phrase)
        {
            AssertEqual(expectedBassFunction, item.Shape.BassFunction, $"{item.Title} uses the requested inversion filter");
        }
    }

    private static bool TryFindPreset(
        IReadOnlyList<PresetChordProgression> presets,
        string input,
        bool allowListPosition,
        out PresetChordProgression preset)
    {
        var method = typeof(App).GetMethod(
            "TryFindPreset",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            [typeof(IReadOnlyList<PresetChordProgression>), typeof(string), typeof(bool), typeof(PresetChordProgression).MakeByRefType()]);
        Assert(method is not null, "TryFindPreset exists");

        object?[] arguments = [presets, input, allowListPosition, null];
        var found = (bool)method!.Invoke(null, arguments)!;
        preset = (PresetChordProgression)arguments[3]!;
        return found;
    }

    private static string LowToHighFunctions(TriadPracticeItem item)
    {
        return string.Concat(item.Diagram.Positions
            .OrderByDescending(position => position.SourceStringIndex)
            .Select(position => position.Label));
    }
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

        AssertScaleWindowSeparatesChordRootFromScaleRoot(library);
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

    private static void AssertScaleWindowSeparatesChordRootFromScaleRoot(PentatonicLibrary library)
    {
        var diagram = library.BuildScaleWindow(
            "B",
            PentatonicScaleKind.NaturalMinor,
            startFret: 0,
            length: 13,
            new ChordSymbol("A", ChordQuality.Major));

        foreach (var position in diagram.Positions)
        {
            var pitch = MusicTheory.Normalize(OpenPitchByStringIndex[position.SourceStringIndex!.Value] + position.Fret);

            if (pitch == MusicTheory.PitchClassFor("A"))
            {
                TestAssert.Equal("R", position.Label, "scale song overlay labels current chord roots as R");
            }
            else if (pitch == MusicTheory.PitchClassFor("B"))
            {
                TestAssert.Equal("1", position.Label, "scale song overlay labels scale roots as 1 when they are not current chord roots");
            }
        }
    }
}

internal sealed class BackingSynthTests
{
    public void RunAll()
    {
        var frequencies = GuitarChordFrequencies(new ChordSymbol("C", ChordQuality.Major)).ToArray();

        AssertEqual(6, frequencies.Length, "C major synth chord has six guitar strings");
        AssertNear(MidiToFrequency(48), frequencies[0], "C major synth root is C3");
        AssertNear(MidiToFrequency(52), frequencies[1], "C major synth third is E3");
        AssertNear(MidiToFrequency(55), frequencies[2], "C major synth fifth is G3");
        AssertNear(MidiToFrequency(60), frequencies[3], "C major synth octave is C4");
        AssertNear(MidiToFrequency(64), frequencies[4], "C major synth high third is E4");
        AssertNear(MidiToFrequency(67), frequencies[5], "C major synth high fifth is G4");
    }

    private static IEnumerable<double> GuitarChordFrequencies(ChordSymbol chord)
    {
        var method = typeof(App).GetMethod(
            "GuitarChordFrequencies",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            [typeof(ChordSymbol)]);
        Assert(method is not null, "GuitarChordFrequencies exists");
        return (IEnumerable<double>)method!.Invoke(null, [chord])!;
    }

    private static double MidiToFrequency(int midiNote) => 440d * Math.Pow(2d, (midiNote - 69) / 12d);

    private static void AssertNear(double expected, double actual, string message)
    {
        if (Math.Abs(expected - actual) > 0.001)
        {
            throw new InvalidOperationException($"{message}. Expected {expected}, got {actual}.");
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message}. Expected {expected}, got {actual}.");
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
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
            foreach (var anchorFret in new[] { 0, 3, 5, 7, 12, 19 })
            {
                AssertIntervalMap(root, anchorFret, library.BuildMap(root, anchorFret));
            }
        }

        var clamped = library.BuildMap("A", 30);
        TestAssert.Equal(IntervalFunctionMapLibrary.MaxStartFret, clamped.StartFret, "Interval map clamps high anchor frets");
    }

    private static void AssertIntervalMap(string root, int anchorFret, FretboardDiagram diagram)
    {
        var rootPitch = MusicTheory.PitchClassFor(root);
        var expectedStartFret = Math.Clamp(anchorFret - IntervalFunctionMapLibrary.DefaultWindowLength / 2, 0, IntervalFunctionMapLibrary.MaxStartFret);
        TestAssert.Equal(expectedStartFret, diagram.StartFret, $"{root} interval map centers the anchor fret when possible");
        TestAssert.Equal(IntervalFunctionMapLibrary.DefaultWindowLength, diagram.Length, $"{root} interval map length");
        TestAssert.SequenceEqual(["E", "B", "G", "D", "A", "E"], diagram.Strings, $"{root} interval map string order");
        TestAssert.Equal(54, diagram.Positions.Count, $"{root} interval map labels every fret in the window");

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

internal sealed class JazzChordLibraryTests
{
    public void RunAll()
    {
        var library = new JazzChordLibrary(new Random(11));

        var progression = library.ParseProgression("Dm7 G7 Cmaj7 A7b9");
        TestAssert.Equal(4, progression.Count, "jazz parser accepts common progression text");
        TestAssert.Equal("Dm7", progression[0].DisplayName, "minor 7 display");
        TestAssert.Equal("G7", progression[1].DisplayName, "dominant 7 display");
        TestAssert.Equal("Cmaj7", progression[2].DisplayName, "major 7 display");
        TestAssert.Equal("A7b9", progression[3].DisplayName, "altered dominant display");
        TestAssert.Equal("A#maj7", library.ParseProgression("Bbmaj7")[0].DisplayName, "flat jazz roots normalize to sharp names");

        foreach (var quality in JazzChordLibrary.Qualities)
        {
            var groups = library.GetVoicings("C", quality);
            TestAssert.Equal(3, groups.Count, $"{quality.Suffix} has three string-grouping buckets");
            TestAssert.True(groups.Any(group => group.Voicings.Count > 0), $"{quality.Suffix} finds at least one playable voicing");

            foreach (var voicing in groups.SelectMany(group => group.Voicings))
            {
                TestAssert.True(voicing.MaxFret - voicing.MinFret <= 4, $"{quality.Suffix} voicing fits a five-fret window");
                TestAssert.SequenceEqual(quality.Intervals.OrderBy(LabelSortOrder), voicing.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{quality.Suffix} voicing has the expected chord tones");
            }

            AssertVoicingMode(library, quality, JazzVoicingMode.GuideTones, JazzChordLibrary.IntervalsFor(quality, JazzVoicingMode.GuideTones));
            AssertVoicingMode(library, quality, JazzVoicingMode.Shell, JazzChordLibrary.IntervalsFor(quality, JazzVoicingMode.Shell));
        }

        var phrase = library.BuildPhrase(progression);
        TestAssert.Equal(progression.Count, phrase.Count, "jazz phrase has one item per chord");

        foreach (var item in phrase)
        {
            TestAssert.SequenceEqual(["E", "B", "G", "D", "A", "E"], item.Diagram.Strings, $"{item.Title} renders on the full fretboard");
            TestAssert.SequenceEqual(item.Chord.Quality.Intervals.OrderBy(LabelSortOrder), item.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{item.Title} contains the full jazz chord");
        }

        var guideTonePhrase = library.BuildPhrase(progression, voicingMode: JazzVoicingMode.GuideTones);
        foreach (var item in guideTonePhrase)
        {
            TestAssert.SequenceEqual(JazzChordLibrary.IntervalsFor(item.Chord.Quality, JazzVoicingMode.GuideTones).OrderBy(LabelSortOrder), item.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{item.Title} contains only guide tones");
        }
        TestAssert.SequenceEqual(["b3", "b5"], JazzChordLibrary.IntervalsFor(JazzChordLibrary.Quality("m7b5"), JazzVoicingMode.GuideTones), "half-diminished essential tones include the flat fifth");
        TestAssert.SequenceEqual(["R", "b3", "b5"], JazzChordLibrary.IntervalsFor(JazzChordLibrary.Quality("dim7"), JazzVoicingMode.Shell), "diminished shell tones include the flat fifth");

        var shellPhrase = library.BuildPhrase(progression, voicingMode: JazzVoicingMode.Shell);
        foreach (var item in shellPhrase)
        {
            TestAssert.SequenceEqual(JazzChordLibrary.IntervalsFor(item.Chord.Quality, JazzVoicingMode.Shell).OrderBy(LabelSortOrder), item.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{item.Title} contains only shell tones");
        }

        var randomProgression = library.BuildRandomProgression();
        TestAssert.Equal(4, randomProgression.Count, "random jazz arrangement has four chords");
        TestAssert.Equal(randomProgression.Count, library.BuildPhrase(randomProgression).Count, "random jazz arrangement can build practice voicings");
        TestAssert.Equal(JazzVoicingMode.Shell, JazzVoicingMode.GuideTones.Next(), "jazz voicing mode cycles from guide tones to shells");
        TestAssert.Equal(JazzVoicingMode.Full, JazzVoicingMode.Shell.Next(), "jazz voicing mode cycles from shells to full");
        TestAssert.Equal(JazzVoicingMode.GuideTones, JazzVoicingMode.Full.Next(), "jazz voicing mode cycles back to guide tones");
        TestAssert.Equal("b3=F b7=C", JazzChordLibrary.GuideToneSummary(progression[0]), "guide tone summary names intervals and notes");
        TestAssert.True(JazzChordLibrary.GuideToneMovement(progression[0], progression[1]).Contains("common tone F", StringComparison.Ordinal), "guide tone movement calls out common tones");

        TestAssert.Equal(10, JazzChordLibrary.PresetProgressions.Count, "jazz library has ten preset progressions");
        foreach (var preset in JazzChordLibrary.PresetProgressions)
        {
            var presetProgression = library.ParseProgression(preset.ProgressionText);
            TestAssert.True(presetProgression.Count > 0, $"{preset.Name} parses");
            TestAssert.Equal(presetProgression.Count, preset.ChordLengths.Count, $"{preset.Name} has one length per chord");
            TestAssert.True(preset.Bpm >= 30 && preset.Bpm <= 240, $"{preset.Name} BPM is playable");
            TestAssert.True(!string.IsNullOrWhiteSpace(preset.Concepts), $"{preset.Name} explains the study concepts");
        }

        TestAssert.SequenceEqual([4, 2, 2, 8], library.ParseChordLengths("4 2 2 8", 4, 4), "jazz custom chord lengths parse");
        TestAssert.SequenceEqual([3, 3, 3, 3], library.ParseChordLengths("", 4, 3), "blank jazz chord lengths use default");
    }

    private static void AssertVoicingMode(
        JazzChordLibrary library,
        JazzChordQuality quality,
        JazzVoicingMode voicingMode,
        IReadOnlyList<string> expectedIntervals)
    {
        var groups = library.GetVoicings("C", quality, voicingMode);
        TestAssert.True(groups.Any(group => group.Voicings.Count > 0), $"{quality.Suffix} {voicingMode.DisplayName()} finds at least one playable voicing");

        foreach (var voicing in groups.SelectMany(group => group.Voicings))
        {
            TestAssert.SequenceEqual(expectedIntervals.OrderBy(LabelSortOrder), voicing.Diagram.Positions.Select(position => position.Label).OrderBy(LabelSortOrder), $"{quality.Suffix} {voicingMode.DisplayName()} uses the teaching tone set");
        }
    }

    private static int LabelSortOrder(string label)
    {
        var semitone = JazzChordLibrary.SemitonesFor(label);
        return label == "R" ? 0 : semitone + 1;
    }
}

internal sealed class ArpeggioLibraryTests
{
    public void RunAll()
    {
        var library = new ArpeggioLibrary(new Random(19));

        TestAssert.Equal(7, ArpeggioLibrary.Qualities.Count, "arpeggio library has foundational triad and seventh arpeggios");
        TestAssert.SequenceEqual(["R", "3", "5"], ArpeggioLibrary.Quality(string.Empty).Intervals, "major arpeggio formula");
        TestAssert.SequenceEqual(["R", "b3", "5", "b7"], ArpeggioLibrary.Quality("m7").Intervals, "minor 7 arpeggio formula");

        var progression = library.ParseProgression("Dm7 G7 Cmaj7 Cdim7");
        TestAssert.Equal(4, progression.Count, "arpeggio parser accepts common chord symbols");
        TestAssert.Equal("Dm7", progression[0].DisplayName, "minor 7 display");
        TestAssert.Equal("G7", progression[1].DisplayName, "dominant 7 display");
        TestAssert.Equal("Cmaj7", progression[2].DisplayName, "major 7 display");
        TestAssert.Equal("Cdim7", progression[3].DisplayName, "diminished 7 display");
        TestAssert.Equal("A#maj7", library.ParseProgression("Bbmaj7")[0].DisplayName, "flat roots normalize in arpeggio parser");

        var cMajor = new ArpeggioChordSymbol("C", ArpeggioLibrary.Quality(string.Empty));
        TestAssert.Equal("C E G", ArpeggioLibrary.NotesFor(cMajor), "major arpeggio notes use formula intervals");
        var cMinorSeven = new ArpeggioChordSymbol("C", ArpeggioLibrary.Quality("m7"));
        TestAssert.Equal("C Eb G Bb", ArpeggioLibrary.NotesFor(cMinorSeven), "minor arpeggio notes use correct flat spellings");
        TestAssert.Equal("D#", MusicTheory.NameForInterval("C", "#9"), "sharp ninth spells as raised second");
        TestAssert.Equal("Bbb", MusicTheory.NameForInterval("C", "bb7"), "diminished seventh spells as double-flat seventh");
        TestAssert.True(ArpeggioLibrary.TeachingHintFor(cMajor).Contains("landing points", StringComparison.Ordinal), "arpeggio teaching hint explains use");

        foreach (var quality in ArpeggioLibrary.Qualities)
        {
            var shapes = library.GetShapes("C", quality);
            TestAssert.Equal(6, shapes.Count, $"{quality.Name} has six position maps");

            foreach (var shape in shapes)
            {
                TestAssert.Equal(6, shape.Diagram.Length, $"{quality.Name} position map has six frets");
                TestAssert.SequenceEqual(["E", "B", "G", "D", "A", "E"], shape.Diagram.Strings, $"{quality.Name} renders on the full fretboard");
                TestAssert.True(shape.Diagram.Positions.All(position => quality.Intervals.Contains(position.Label)), $"{quality.Name} map only contains chord tones");
            }
        }

        var phrase = library.BuildPhrase(progression);
        TestAssert.Equal(progression.Count, phrase.Count, "arpeggio phrase has one shape per chord");
        foreach (var item in phrase)
        {
            TestAssert.SequenceEqual(["E", "B", "G", "D", "A", "E"], item.Diagram.Strings, $"{item.Title} renders on all six strings");
            TestAssert.Equal(6, item.Diagram.Length, $"{item.Title} uses a compact six-fret arpeggio shape");
            TestAssert.True(item.Diagram.Positions.All(position => item.Chord.Quality.Intervals.Contains(position.Label)), $"{item.Title} only shows chord tones");
        }

        var nextPhrase = library.BuildPhrase(progression, phrase[^1]);
        TestAssert.Equal(progression.Count, nextPhrase.Count, "next arpeggio phrase has one shape per chord");
        TestAssert.True(Math.Abs(nextPhrase[0].CenterFret - phrase[^1].CenterFret) <= 6, "next arpeggio phrase starts near the previous shape");

        var target = library.BuildPrompt(progression, new HashSet<string>(["3", "b3", "7", "b7"]));
        TestAssert.True(target.Chord.Quality.Intervals.Contains(target.TargetInterval), "target prompt chooses a chord tone");
        TestAssert.True(!string.IsNullOrWhiteSpace(target.TargetNote), "target prompt names the target note");

        var triadChord = new ChordSymbol("A", ChordQuality.Minor);
        TestAssert.Equal("Am", ArpeggioLibrary.FromTriadChord(triadChord).DisplayName, "triad chord maps to matching arpeggio");
        TestAssert.SequenceEqual([4, 2, 2, 8], library.ParseChordLengths("4 2 2 8", 4, 4), "arpeggio chord lengths parse");
        TestAssert.SequenceEqual([3, 3, 3, 3], library.ParseChordLengths("", 4, 3), "blank arpeggio chord lengths use default");
    }
}

internal sealed class PracticeCoachPlannerTests
{
    public void RunAll()
    {
        var emptyLog = new PracticeSessionLog();
        var plan = PracticeCoachPlanner.BuildPlan(
            new PracticeSessionRequest(10, PracticeFocus.Mixed),
            emptyLog,
            new DateOnly(2026, 5, 15));

        TestAssert.Equal(10, plan.DurationMinutes, "practice planner keeps requested duration");
        TestAssert.Equal(4, plan.Blocks.Count, "10-minute practice sessions have four blocks");
        TestAssert.Equal(PracticeBlockKind.Tuner, plan.Blocks[0].Kind, "practice sessions start with tuning");
        TestAssert.Equal(10, plan.Blocks.Sum(block => block.Minutes), "practice block minutes add up to the session length");
        TestAssert.True(plan.Blocks.Select(block => block.Id).Distinct().Count() == plan.Blocks.Count, "practice planner does not duplicate blocks in a session");

        var jazzPlan = PracticeCoachPlanner.BuildPlan(
            new PracticeSessionRequest(20, PracticeFocus.Jazz),
            emptyLog,
            new DateOnly(2026, 5, 15));
        TestAssert.True(jazzPlan.Blocks.Any(block => block.Kind is PracticeBlockKind.JazzGuideTones or PracticeBlockKind.JazzShellVoicings), "jazz practice includes jazz chord work");
        TestAssert.True(jazzPlan.Blocks.Any(block => block.Kind is PracticeBlockKind.ArpeggioTargeting or PracticeBlockKind.ArpeggioSong or PracticeBlockKind.IntervalTargets), "jazz practice includes note-targeting work");

        var shredPlan = PracticeCoachPlanner.BuildPlan(
            new PracticeSessionRequest(10, PracticeFocus.Shredding),
            emptyLog,
            new DateOnly(2026, 5, 15));
        TestAssert.True(shredPlan.Blocks.Any(block => block.Kind == PracticeBlockKind.ShredSpeed), "shredding practice includes speed work");

        var log = new PracticeSessionLog
        {
            Entries =
            [
                new PracticeSessionEntry(
                    new DateTimeOffset(2026, 5, 14, 10, 0, 0, TimeSpan.Zero),
                    10,
                    PracticeFocus.Mixed,
                    [
                        new PracticeBlockResult("interval-targets", PracticeBlockKind.IntervalTargets, "Intervals", "Target intervals", 3, PracticeDifficulty.Hard),
                        new PracticeBlockResult("triad-connected", PracticeBlockKind.TriadMovement, "Triads", "Connected triad movement", 3, PracticeDifficulty.Easy)
                    ])
            ]
        };

        var biasedPlan = PracticeCoachPlanner.BuildPlan(
            new PracticeSessionRequest(10, PracticeFocus.Mixed),
            log,
            new DateOnly(2026, 5, 15));
        TestAssert.True(biasedPlan.Blocks.Any(block => block.Id == "interval-targets"), "hard blocks are rotated back into later practice");
        TestAssert.SequenceEqual(["Intervals"], PracticeCoachPlanner.HardAreas(log), "hard areas summarize review bias");
    }
}

internal sealed class ShredLibraryTests
{
    public void RunAll()
    {
        TestAssert.True(ShredLibrary.Drills.Count >= 10, "shred library has a useful starter drill set");
        TestAssert.True(ShredLibrary.Drills.Any(drill => drill.Category == ShredDrillCategory.FingerIndependence), "shred library includes finger independence");
        TestAssert.True(ShredLibrary.Drills.Any(drill => drill.Category == ShredDrillCategory.Picking), "shred library includes picking");
        TestAssert.True(ShredLibrary.Drills.Any(drill => drill.Category == ShredDrillCategory.ScaleSequencing), "shred library includes scale sequencing");

        var drill = ShredLibrary.Drills.Single(item => item.Id == "chromatic-1234");
        var exercise = ShredLibrary.BuildExercise(drill, 5);
        TestAssert.Equal(24, exercise.Notes.Count, "chromatic 1234 covers six strings");
        TestAssert.SequenceEqual([5, 6, 7, 8], exercise.Notes.Take(4).Select(note => note.Fret), "finger numbers map to adjacent frets");
        TestAssert.SequenceEqual([1, 2, 3, 4], exercise.Notes.Take(4).Select(note => note.Finger), "exercise preserves finger order");
        TestAssert.SequenceEqual([PickStroke.Down, PickStroke.Up, PickStroke.Down, PickStroke.Up], exercise.Notes.Take(4).Select(note => note.PickStroke), "exercise applies alternate picking");

        var tab = ShredLibrary.RenderTab(exercise);
        TestAssert.Equal(6, tab.Count, "tab renders six strings");
        TestAssert.True(tab.Any(line => line.Contains("-5--6--7--8", StringComparison.Ordinal)), "tab includes the fret pattern");
        TestAssert.True(ShredLibrary.RenderFingerLine(exercise)[1].Contains("D U D U", StringComparison.Ordinal), "finger line includes pick strokes");

        var outside = ShredLibrary.BuildExercise(ShredLibrary.Drills.Single(item => item.Id == "outside-picking"), 7);
        TestAssert.Equal(6, outside.Notes.Count, "outside picking is a compact two-string cell");
        TestAssert.SequenceEqual([1, 0, 1, 0, 1, 0], outside.Notes.Select(note => note.StringIndex), "outside picking crosses strings inside the cell");

        var state = new ShredTempoState(drill.Id, 80, 0, 90, 0);
        var update1 = ShredLibrary.ApplyAttempt(state, ShredAttemptResult.Clean);
        TestAssert.Equal(80, update1.State.CurrentBpm, "one clean rep holds tempo");
        TestAssert.Equal(1, update1.State.CleanStreak, "one clean rep increments clean streak");
        var update2 = ShredLibrary.ApplyAttempt(update1.State, ShredAttemptResult.Clean);
        var update3 = ShredLibrary.ApplyAttempt(update2.State, ShredAttemptResult.Clean);
        TestAssert.Equal(85, update3.State.CurrentBpm, "three clean reps raise tempo");
        TestAssert.Equal(80, update3.State.TopCleanBpm, "top clean tempo records completed clean tempo");
        var messy = ShredLibrary.ApplyAttempt(update3.State, ShredAttemptResult.Messy);
        TestAssert.Equal(80, messy.State.CurrentBpm, "messy attempt drops tempo");
        TestAssert.Equal(0, messy.State.CleanStreak, "messy attempt resets clean streak");

        var log = new ShredPracticeLog();
        ShredLibrary.RecordAttempt(log, drill, update3.State, ShredAttemptResult.Clean, new DateTimeOffset(2026, 5, 15, 12, 0, 0, TimeSpan.Zero));
        TestAssert.Equal(1, log.Records.Count, "shred log records attempts");
        TestAssert.Equal(80, log.Records[0].TopCleanBpm, "shred log stores top clean tempo");
        var started = ShredLibrary.StartingTempoFor(drill, log);
        TestAssert.Equal(70, started.CurrentBpm, "speed builder restarts below top clean tempo");

        var workout = ShredLibrary.BuildDailyWorkout(log);
        TestAssert.Equal(5, workout.Count, "daily shred workout has five drills");
        TestAssert.True(workout.Select(item => item.Id).Distinct().Count() == workout.Count, "daily shred workout does not duplicate drills");
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
