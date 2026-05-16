using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.JazzChords;

public sealed class JazzChordLibrary(Random? random = null)
{
    private const int SearchFrets = 15;
    private const int WindowLength = 5;

    private static readonly TimeSignature FourFour = new(4, 4);

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
        new("Top four strings", [0, 1, 2, 3]),
        new("Middle four strings", [1, 2, 3, 4]),
        new("Low four strings", [2, 3, 4, 5])
    ];

    private static readonly IReadOnlyList<StringGrouping> ShellGroupings =
    [
        new("E B G", [0, 1, 2]),
        new("B G D", [1, 2, 3]),
        new("G D A", [2, 3, 4]),
        new("D A E", [3, 4, 5])
    ];

    private static readonly IReadOnlyList<StringGrouping> GuideToneGroupings =
    [
        new("E B", [0, 1]),
        new("B G", [1, 2]),
        new("G D", [2, 3]),
        new("D A", [3, 4]),
        new("A E", [4, 5])
    ];

    public static readonly IReadOnlyList<JazzChordQuality> Qualities =
    [
        new("Major 7", "maj7", ["R", "3", "5", "7"], "Stable major color"),
        new("Major 6", "6", ["R", "3", "5", "6"], "Swing and older standards"),
        new("Major 6/9", "6/9", ["R", "3", "6", "9"], "Bright tonic color; guitar voicings often omit the 5th"),
        new("Minor 7", "m7", ["R", "b3", "5", "b7"], "Minor ii and modal minor"),
        new("Minor 6", "m6", ["R", "b3", "5", "6"], "Minor tonic color"),
        new("Minor 9", "m9", ["R", "b3", "b7", "9"], "Minor 7 with a smoother top color; 5th often omitted"),
        new("Dominant 7", "7", ["R", "3", "5", "b7"], "V chord and blues color"),
        new("Dominant 9", "9", ["R", "3", "b7", "9"], "Common comping dominant; 5th often omitted"),
        new("Dominant 13", "13", ["R", "3", "b7", "13"], "Dominant with a 6/13 color; 5th/9th may be omitted"),
        new("7 flat 9", "7b9", ["R", "3", "b7", "b9"], "Altered dominant tension; 5th often omitted"),
        new("7 sharp 9", "7#9", ["R", "3", "b7", "#9"], "Bluesy altered dominant tension; 5th often omitted"),
        new("Minor 7 flat 5", "m7b5", ["R", "b3", "b5", "b7"], "Half-diminished ii in minor"),
        new("Diminished 7", "dim7", ["R", "b3", "b5", "bb7"], "Symmetric passing diminished")
    ];

    public static readonly IReadOnlyList<PresetJazzProgression> PresetProgressions =
    [
        Song(1, "Autumn Leaves", "standard", "Cm7 F7 A#maj7 D#maj7 Am7b5 D7 Gm6", 120, "major ii-V-I, minor ii-V-i, circle movement", chordLengths: [4, 4, 4, 4, 4, 4, 8]),
        Song(2, "Blue Bossa", "Kenny Dorham", "Cm7 Fm7 Dm7b5 G7b9 Cm7 D#m7 G#7 C#maj7 Dm7b5 G7b9", 132, "minor key, ii-V-i, modulation"),
        Song(3, "Fly Me To The Moon", "standard", "Am7 Dm7 G7 Cmaj7 Fmaj7 Bm7b5 E7 Am7", 120, "cycle movement, major ii-V-I, minor turnaround"),
        Song(4, "Misty", "Erroll Garner", "D#maj7 A#m7 D#7 G#maj7 G#m7 C#7 D#maj7 Cm7 Fm7 A#7", 82, "ballad changes, dominant resolutions, backdoor color"),
        Song(5, "Satin Doll", "Duke Ellington", "Dm7 G7 Dm7 G7 Em7 A7 Em7 A7 Cmaj7", 120, "repeated ii-V motion, dominant cycles"),
        Song(6, "Take The A Train", "Billy Strayhorn", "Cmaj7 D7 Dm7 G7 Cmaj7 A7 Dm7 G7", 150, "secondary dominants, tonic color, turnaround"),
        Song(7, "There Will Never Be Another You", "Harry Warren", "D#maj7 Dm7b5 G7 Cm7 A#m7 D#7 G#maj7 G#m7 C#7 D#maj7", 130, "major and minor ii-Vs, descending resolutions"),
        Song(8, "All The Things You Are", "Jerome Kern", "Fm7 A#m7 D#7 G#maj7 C#maj7 Dm7b5 G7 Cmaj7", 120, "circle movement, key centers, ii-V-I chains"),
        Song(9, "Tune Up", "Miles Davis", "Em7 A7 Dmaj7 Dm7 G7 Cmaj7 Cm7 F7 A#maj7", 150, "ii-V-I through several keys"),
        Song(10, "Rhythm Changes A", "I Got Rhythm form", "A#maj7 G7 Cm7 F7 Dm7 G7 Cm7 F7", 170, "I-VI-ii-V, turnarounds, rhythm changes")
    ];

    private readonly Random _random = random ?? Random.Shared;

    public IReadOnlyList<JazzChordVoicingGroup> GetVoicings(
        string root,
        JazzChordQuality quality,
        JazzVoicingMode voicingMode = JazzVoicingMode.Full)
    {
        var targetIntervals = IntervalsFor(quality, voicingMode);
        var chordTones = targetIntervals
            .Select(interval => new JazzChordTone(MusicTheory.Normalize(MusicTheory.PitchClassFor(root) + SemitonesFor(interval)), interval))
            .ToArray();
        var pitchClasses = chordTones.Select(tone => tone.PitchClass).ToHashSet();
        var results = new List<JazzChordVoicingGroup>();

        foreach (var grouping in GroupingsFor(voicingMode))
        {
            var positionsByString = grouping.StringIndexes
                .Select(stringIndex => PositionsForString(stringIndex, pitchClasses))
                .ToArray();

            var shapes = Cartesian(positionsByString)
                .Where(shape => shape.Select(position => position.PitchClass).Distinct().Count() == chordTones.Length)
                .Where(shape => shape.Max(position => position.Fret) - shape.Min(position => position.Fret) <= WindowLength - 1)
                .Select(shape => BuildVoicing(grouping, shape, chordTones))
                .OrderBy(shape => shape.MinFret == 0 ? 0 : shape.MinFret)
                .ThenBy(shape => shape.MaxFret)
                .Take(3)
                .ToArray();

            results.Add(new JazzChordVoicingGroup(grouping.Name, shapes));
        }

        return results;
    }

    public IReadOnlyList<JazzPracticeItem> BuildPhrase(
        IReadOnlyList<JazzChordSymbol> progression,
        JazzPracticeItem? previousItem = null,
        JazzVoicingMode voicingMode = JazzVoicingMode.Full)
    {
        if (progression.Count == 0)
        {
            throw new ArgumentException("Progression must contain at least one chord.", nameof(progression));
        }

        var phrase = new List<JazzPracticeItem>();
        var anchorFret = previousItem?.CenterFret;

        foreach (var chord in progression)
        {
            var candidates = GetVoicings(chord.Root, chord.Quality, voicingMode)
                .SelectMany(grouping => grouping.Voicings.Select(voicing => new JazzPracticeItem(chord, grouping.Name, voicing)))
                .ToArray();
            var item = PickNear(candidates, anchorFret);
            phrase.Add(item);
            anchorFret = item.CenterFret;
        }

        return phrase;
    }

    public IReadOnlyList<JazzChordSymbol> BuildRandomProgression()
    {
        var keyRoot = MusicTheory.ChromaticRoots[_random.Next(MusicTheory.ChromaticRoots.Count)];
        var rootPitch = MusicTheory.PitchClassFor(keyRoot);
        var majorSeven = Quality("maj7");
        var minorSeven = Quality("m7");
        var dominantSeven = Quality("7");
        var dominantNine = Quality("9");
        var minorSevenFlatFive = Quality("m7b5");
        var dominantFlatNine = Quality("7b9");
        var minorSix = Quality("m6");

        var templates = new[]
        {
            new[] { (2, minorSeven), (7, dominantSeven), (0, majorSeven), (0, majorSeven) },
            new[] { (2, minorSeven), (7, dominantNine), (0, majorSeven), (9, dominantSeven) },
            new[] { (9, minorSeven), (2, dominantSeven), (7, minorSeven), (0, dominantSeven) },
            new[] { (11, minorSevenFlatFive), (4, dominantFlatNine), (9, minorSix), (9, minorSix) },
            new[] { (0, majorSeven), (9, dominantSeven), (2, minorSeven), (7, dominantSeven) }
        };

        var template = templates[_random.Next(templates.Length)];
        return template
            .Select(chord => new JazzChordSymbol(MusicTheory.NameFor(rootPitch + chord.Item1), chord.Item2))
            .ToArray();
    }

    public static JazzChordSymbol ParseChord(string value)
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
            throw new ArgumentException($"'{value}' is not a supported jazz chord. Try Cmaj7, Dm7, G7, G9, Bm7b5, or C#dim7.", nameof(value));
        }

        return new JazzChordSymbol(root, quality);
    }

    public IReadOnlyList<JazzChordSymbol> ParseProgression(string input)
    {
        var parts = input
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            throw new ArgumentException("Enter at least one jazz chord, for example Dm7 G7 Cmaj7.", nameof(input));
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

    public static JazzChordQuality Quality(string suffix)
    {
        return Qualities.Single(quality => quality.Suffix.Equals(suffix, StringComparison.OrdinalIgnoreCase));
    }

    public static PresetJazzProgression? GetPresetProgression(int number)
    {
        return PresetProgressions.FirstOrDefault(progression => progression.Number == number);
    }

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

    public static IReadOnlyList<string> IntervalsFor(JazzChordQuality quality, JazzVoicingMode voicingMode) => voicingMode switch
    {
        JazzVoicingMode.GuideTones => GuideToneIntervalsFor(quality),
        JazzVoicingMode.Shell => ShellIntervalsFor(quality),
        _ => quality.Intervals
    };

    public static string TeachingHintFor(JazzChordSymbol chord, JazzVoicingMode voicingMode)
    {
        var intervals = IntervalsFor(chord.Quality, voicingMode);
        var role = chord.Quality.Suffix switch
        {
            "maj7" or "6" or "6/9" => "tonic color",
            "m7" or "m9" => "minor color, often a ii chord",
            "m7b5" => "half-diminished color, usually ii in minor",
            "7" or "9" or "13" or "7b9" or "7#9" => "dominant color that wants to resolve",
            "dim7" => "passing diminished color",
            _ => chord.Quality.Use
        };

        return voicingMode switch
        {
            JazzVoicingMode.GuideTones => $"{chord.DisplayName}: guide tones {string.Join(" ", intervals)} define the chord color; {role}.",
            JazzVoicingMode.Shell => $"{chord.DisplayName}: shell voicing {string.Join(" ", intervals)} gives the essential comping grip; {role}.",
            _ => $"{chord.DisplayName}: full voicing {string.Join(" ", intervals)} adds the chord color tones; {role}."
        };
    }

    public static string GuideToneSummary(JazzChordSymbol chord)
    {
        return string.Join(" ", GuideToneIntervalsFor(chord.Quality)
            .Select(interval => $"{interval}={MusicTheory.NameForInterval(chord.Root, interval)}"));
    }

    public static string GuideToneMovement(JazzChordSymbol current, JazzChordSymbol next)
    {
        var currentNotes = GuideToneNotes(current);
        var nextNotes = GuideToneNotes(next);
        var common = currentNotes.Select(note => note.Note).Intersect(nextNotes.Select(note => note.Note)).ToArray();
        var commonText = common.Length == 0
            ? "no common guide tone"
            : $"common tone {string.Join("/", common)}";

        return $"{current.DisplayName} {FormatGuideToneNotes(currentNotes)} -> {next.DisplayName} {FormatGuideToneNotes(nextNotes)} ({commonText})";
    }

    private static IReadOnlyList<(string Interval, string Note)> GuideToneNotes(JazzChordSymbol chord)
    {
        return GuideToneIntervalsFor(chord.Quality)
            .Select(interval => (interval, MusicTheory.NameForInterval(chord.Root, interval)))
            .ToArray();
    }

    private static string FormatGuideToneNotes(IReadOnlyList<(string Interval, string Note)> notes)
    {
        return string.Join(" ", notes.Select(note => $"{note.Interval}={note.Note}"));
    }

    private static IReadOnlyList<StringGrouping> GroupingsFor(JazzVoicingMode voicingMode) => voicingMode switch
    {
        JazzVoicingMode.GuideTones => GuideToneGroupings,
        JazzVoicingMode.Shell => ShellGroupings,
        _ => Groupings
    };

    private static IReadOnlyList<string> GuideToneIntervalsFor(JazzChordQuality quality)
    {
        if (quality.Intervals.Contains("b5"))
        {
            return ["b3", "b5"];
        }

        if (quality.Intervals.Contains("7"))
        {
            return [quality.Intervals.Contains("b3") ? "b3" : "3", "7"];
        }

        if (quality.Intervals.Contains("b7"))
        {
            return [quality.Intervals.Contains("b3") ? "b3" : "3", "b7"];
        }

        if (quality.Intervals.Contains("bb7"))
        {
            return ["b3", "bb7"];
        }

        if (quality.Intervals.Contains("6"))
        {
            return [quality.Intervals.Contains("b3") ? "b3" : "3", "6"];
        }

        return quality.Intervals.Take(2).ToArray();
    }

    private static IReadOnlyList<string> ShellIntervalsFor(JazzChordQuality quality)
    {
        var guideTones = GuideToneIntervalsFor(quality);
        return ["R", .. guideTones];
    }

    private JazzPracticeItem PickNear(IReadOnlyList<JazzPracticeItem> candidates, double? anchorFret)
    {
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("No jazz chord voicings are available for this chord.");
        }

        if (anchorFret is null)
        {
            var playable = candidates
                .Where(candidate => candidate.Voicing.MinFret > 0 && candidate.Voicing.MaxFret <= 9)
                .DefaultIfEmpty()
                .Where(candidate => candidate is not null)
                .Cast<JazzPracticeItem>()
                .ToArray();

            return playable[_random.Next(playable.Length)];
        }

        var bestDistance = candidates.Min(candidate => Math.Abs(candidate.CenterFret - anchorFret.Value));
        var nearby = candidates
            .Where(candidate => Math.Abs(candidate.CenterFret - anchorFret.Value) <= bestDistance + 2.5)
            .OrderBy(candidate => Math.Abs(candidate.CenterFret - anchorFret.Value))
            .ThenBy(candidate => candidate.Voicing.MinFret)
            .Take(5)
            .ToArray();

        return nearby[_random.Next(nearby.Length)];
    }

    private static JazzChordVoicing BuildVoicing(
        StringGrouping grouping,
        IReadOnlyList<StringPosition> shape,
        IReadOnlyList<JazzChordTone> chordTones)
    {
        var minFret = shape.Min(position => position.Fret);
        var maxFret = shape.Max(position => position.Fret);
        var startFret = minFret == 0 ? 0 : Math.Max(1, maxFret - WindowLength + 1);
        var strings = grouping.StringIndexes.Select(index => Tuning[index].Name).ToArray();
        var positions = shape.Select((position, displayStringIndex) =>
        {
            var tone = chordTones.First(tone => tone.PitchClass == position.PitchClass);
            return new FretPosition(displayStringIndex, position.Fret, tone.Label, SourceStringIndex: position.StringIndex);
        }).ToArray();

        var lowToHigh = string.Join("-", shape.Reverse().Select(position => chordTones.First(tone => tone.PitchClass == position.PitchClass).Label));
        return new JazzChordVoicing(lowToHigh, minFret, maxFret, new FretboardDiagram(strings, startFret, WindowLength, positions));
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

    private static PresetJazzProgression Song(
        int number,
        string title,
        string artist,
        string progressionText,
        int bpm,
        string concepts,
        TimeSignature? timeSignature = null,
        IReadOnlyList<int>? chordLengths = null)
    {
        var meter = timeSignature ?? FourFour;
        return new PresetJazzProgression(number, title, artist, progressionText, bpm, meter, chordLengths ?? OneMeasureEach(progressionText, meter.BeatsPerBar), concepts);
    }

    private static IReadOnlyList<int> OneMeasureEach(string progressionText, int beatsPerMeasure)
    {
        var chordCount = progressionText
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Length;

        return Enumerable.Repeat(beatsPerMeasure, chordCount).ToArray();
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

        return normalized switch
        {
            "Bb" => "A#",
            "Db" => "C#",
            "Eb" => "D#",
            "Gb" => "F#",
            "Ab" => "G#",
            _ => normalized
        };
    }

    private sealed record GuitarString(string Name, int PitchClass);

    private sealed record StringGrouping(string Name, IReadOnlyList<int> StringIndexes);

    private sealed record StringPosition(int StringIndex, int Fret, int PitchClass);

    private sealed record JazzChordTone(int PitchClass, string Label);
}

public sealed record JazzChordQuality(
    string Name,
    string Suffix,
    IReadOnlyList<string> Intervals,
    string Use);

public enum JazzVoicingMode
{
    GuideTones,
    Shell,
    Full
}

public static class JazzVoicingModeExtensions
{
    public static string DisplayName(this JazzVoicingMode mode) => mode switch
    {
        JazzVoicingMode.GuideTones => "guide tones",
        JazzVoicingMode.Shell => "shell voicings",
        JazzVoicingMode.Full => "full voicings",
        _ => mode.ToString()
    };

    public static JazzVoicingMode Next(this JazzVoicingMode mode) => mode switch
    {
        JazzVoicingMode.GuideTones => JazzVoicingMode.Shell,
        JazzVoicingMode.Shell => JazzVoicingMode.Full,
        _ => JazzVoicingMode.GuideTones
    };
}

public sealed record JazzChordSymbol(string Root, JazzChordQuality Quality)
{
    public string DisplayName => $"{Root}{Quality.Suffix}";
}

public sealed record JazzChordVoicingGroup(string Name, IReadOnlyList<JazzChordVoicing> Voicings);

public sealed record JazzChordVoicing(string LowToHigh, int MinFret, int MaxFret, FretboardDiagram Diagram);

public sealed record JazzPracticeItem(JazzChordSymbol Chord, string GroupingName, JazzChordVoicing Voicing)
{
    private static readonly IReadOnlyList<string> FullFretboardStrings = ["E", "B", "G", "D", "A", "E"];

    public double CenterFret => (Voicing.MinFret + Voicing.MaxFret) / 2.0;

    public FretboardDiagram Diagram => new(
        FullFretboardStrings,
        Voicing.Diagram.StartFret,
        Voicing.Diagram.Length,
        Voicing.Diagram.Positions
            .Where(position => position.SourceStringIndex is not null)
            .Select(position => position with { StringIndex = position.SourceStringIndex!.Value })
            .ToArray());

    public string Title => $"{Chord.DisplayName} {Voicing.LowToHigh} {GroupingName} ({Voicing.MinFret}-{Voicing.MaxFret})";
}

public sealed record PresetJazzProgression(
    int Number,
    string Title,
    string Artist,
    string ProgressionText,
    int Bpm,
    TimeSignature TimeSignature,
    IReadOnlyList<int> ChordLengths,
    string Concepts)
{
    public string Name => $"{Title} - {Artist}";

    public string MenuText => $"{Number:000}. {Title} - {Artist}: {ProgressionText} [{Concepts}]";
}
