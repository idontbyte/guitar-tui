using GuitarResourcesTui.Fretboards;

namespace GuitarResourcesTui.Triads;

public sealed class TriadProgressionGameLibrary(
    TriadInversionLibrary triads,
    Random? random = null,
    TriadVoicingKind voicingKind = TriadVoicingKind.Close)
{
    private static readonly TimeSignature FourFour = new(4, 4);
    private static readonly TimeSignature ThreeFour = new(3, 4);
    private static readonly TimeSignature SixEight = new(6, 8);

    public static readonly IReadOnlyList<PresetChordProgression> PresetProgressions =
    [
        Song(1, "Wonderwall", "Oasis", "Em G D A", 87, chordLengths: [2, 2, 2, 2]),
        Song(2, "Perfect", "Ed Sheeran", "G Em C D", 95, SixEight),
        Song(3, "Hallelujah", "Leonard Cohen / Jeff Buckley", "C Am C Am F G C G", 56, SixEight),
        Song(4, "Can't Help Falling In Love", "Elvis Presley", "C Em Am F C G", 100, ThreeFour),
        Song(5, "Let It Be", "The Beatles", "C G Am F", 72),
        Song(6, "No Woman No Cry", "Bob Marley", "C G Am F", 78),
        Song(7, "I'm Yours", "Jason Mraz", "G D Em C", 76),
        Song(8, "Someone Like You", "Adele", "A E F#m D", 68),
        Song(9, "Yellow", "Coldplay", "B F# E B", 86),
        Song(10, "Creep", "Radiohead", "G B C Cm", 92),
        Song(11, "Zombie", "The Cranberries", "Em C G D", 83),
        Song(12, "House Of The Rising Sun", "The Animals", "Am C D F Am E Am E", 120, SixEight),
        Song(13, "Smells Like Teen Spirit", "Nirvana", "F A# G# C#", 117),
        Song(14, "Sweet Child O' Mine", "Guns N' Roses", "D C G D", 125),
        Song(15, "Hotel California", "Eagles", "Bm F# A E G D Em F#", 74),
        Song(16, "Knockin' On Heaven's Door", "Bob Dylan", "G D Am G D C", 70),
        Song(17, "Hey Jude", "The Beatles", "F C C F A# F C F", 74),
        Song(18, "All Along The Watchtower", "Jimi Hendrix / Bob Dylan", "C#m B A B", 114),
        Song(19, "Wish You Were Here", "Pink Floyd", "G D Am C", 61),
        Song(20, "Sweet Home Alabama", "Lynyrd Skynyrd", "D C G G", 98),
        Song(21, "Brown Eyed Girl", "Van Morrison", "G C G D", 150),
        Song(22, "Stand By Me", "Ben E. King", "G Em C D", 118),
        Song(23, "Every Breath You Take", "The Police", "A F#m D E", 117),
        Song(24, "With Or Without You", "U2", "D A Bm G", 110),
        Song(25, "Don't Stop Believin'", "Journey", "E B C#m A", 119),
        Song(26, "Horse With No Name", "America", "Em D", 124),
        Song(27, "Bad Moon Rising", "Creedence Clearwater Revival", "D A G D", 90),
        Song(28, "Have You Ever Seen The Rain", "Creedence Clearwater Revival", "C G C F", 116),
        Song(29, "Proud Mary", "Creedence Clearwater Revival", "D A Bm G", 120),
        Song(30, "Free Fallin'", "Tom Petty", "D G D A", 84),
        Song(31, "Take It Easy", "Eagles", "G D C G", 138),
        Song(32, "Losing My Religion", "R.E.M.", "Am Em Am Em", 126),
        Song(33, "What's My Age Again?", "blink-182", "G D Em C", 158),
        Song(34, "Boulevard Of Broken Dreams", "Green Day", "Em G D A", 84),
        Song(35, "Good Riddance", "Green Day", "G C D D", 95),
        Song(36, "She Will Be Loved", "Maroon 5", "C G Am F", 102),
        Song(37, "Love Story", "Taylor Swift", "D A Bm G", 119),
        Song(38, "You Belong With Me", "Taylor Swift", "F# C# G#m B", 130),
        Song(39, "Blank Space", "Taylor Swift", "F Dm A# C", 96),
        Song(40, "Wildest Dreams", "Taylor Swift", "C Em D D", 140),
        Song(41, "Thinking Out Loud", "Ed Sheeran", "D D G A", 79),
        Song(42, "Photograph", "Ed Sheeran", "D Bm A G", 108),
        Song(43, "The A Team", "Ed Sheeran", "G D Em C", 85),
        Song(44, "Love Yourself", "Justin Bieber", "E B C#m A", 100),
        Song(45, "Stay With Me", "Sam Smith", "Am F C C", 84),
        Song(46, "Counting Stars", "OneRepublic", "Am C G F", 122),
        Song(47, "Radioactive", "Imagine Dragons", "Am C G D", 68),
        Song(48, "Demons", "Imagine Dragons", "D A Bm G", 90),
        Song(49, "Viva La Vida", "Coldplay", "C D G Em", 138),
        Song(50, "The Scientist", "Coldplay", "Dm A# F C", 74),
        Song(51, "Fix You", "Coldplay", "C Em Am G", 69),
        Song(52, "Chasing Cars", "Snow Patrol", "A E D A", 104),
        Song(53, "Iris", "Goo Goo Dolls", "Bm G D A", 78),
        Song(54, "Mr. Brightside", "The Killers", "C F Am G", 148),
        Song(55, "Use Somebody", "Kings Of Leon", "C C Em F", 138),
        Song(56, "Seven Nation Army", "The White Stripes", "Em G C B", 124),
        Song(57, "Come As You Are", "Nirvana", "Em D Em D", 120),
        Song(58, "About A Girl", "Nirvana", "Em G Em G", 132),
        Song(59, "Lithium", "Nirvana", "E G# C#m A", 123),
        Song(60, "Everlong", "Foo Fighters", "D Bm G D", 158),
        Song(61, "Times Like These", "Foo Fighters", "D Am C Em", 145),
        Song(62, "Learn To Fly", "Foo Fighters", "B F# E E", 136),
        Song(63, "Scar Tissue", "Red Hot Chili Peppers", "F Dm F Dm", 89),
        Song(64, "Californication", "Red Hot Chili Peppers", "Am F C G", 96),
        Song(65, "Otherside", "Red Hot Chili Peppers", "Am F C G", 123),
        Song(66, "Under The Bridge", "Red Hot Chili Peppers", "E B C#m G#m", 84),
        Song(67, "All The Small Things", "blink-182", "C G F G", 149),
        Song(68, "Don't Look Back In Anger", "Oasis", "C G Am E F G C Am", 82),
        Song(69, "Champagne Supernova", "Oasis", "A G D E", 75),
        Song(70, "Live Forever", "Oasis", "G D Am C", 92),
        Song(71, "Hey There Delilah", "Plain White T's", "D F#m Bm G", 104),
        Song(72, "Riptide", "Vance Joy", "Am G C C", 102),
        Song(73, "Ho Hey", "The Lumineers", "C F C G", 80),
        Song(74, "Little Talks", "Of Monsters And Men", "Am F C G", 103),
        Song(75, "Skinny Love", "Bon Iver / Birdy", "Am C F C", 76),
        Song(76, "Wagon Wheel", "Old Crow Medicine Show", "G D Em C", 148),
        Song(77, "Jolene", "Dolly Parton", "C#m E B C#m", 110),
        Song(78, "Ring Of Fire", "Johnny Cash", "G C G D", 105),
        Song(79, "Folsom Prison Blues", "Johnny Cash", "E E A E", 110),
        Song(80, "Take Me Home, Country Roads", "John Denver", "G Em D C", 82),
        Song(81, "Leaving On A Jet Plane", "John Denver", "G C G C", 72),
        Song(82, "Blowin' In The Wind", "Bob Dylan", "G C D G", 93),
        Song(83, "Mr. Tambourine Man", "Bob Dylan / The Byrds", "D G A D", 124),
        Song(84, "Like A Rolling Stone", "Bob Dylan", "C Dm Em F G", 96),
        Song(85, "Tears In Heaven", "Eric Clapton", "A E F#m D", 80),
        Song(86, "Layla", "Eric Clapton / Derek And The Dominos", "Dm A# C Dm", 116),
        Song(87, "Wonderful Tonight", "Eric Clapton", "G D C D", 96),
        Song(88, "Purple Haze", "Jimi Hendrix", "E G A E", 109),
        Song(89, "Hey Joe", "Jimi Hendrix", "C G D A E", 84),
        Song(90, "Johnny B. Goode", "Chuck Berry", "A D A E", 168),
        Song(91, "Twist And Shout", "The Beatles", "D G A G", 126),
        Song(92, "Here Comes The Sun", "The Beatles", "A D E A", 129),
        Song(93, "Yesterday", "The Beatles", "F Em A Dm", 97),
        Song(94, "Something", "The Beatles", "C Em Am C", 66),
        Song(95, "Blackbird", "The Beatles", "G Am G C", 94),
        Song(96, "Sweet Caroline", "Neil Diamond", "E A B E", 64),
        Song(97, "What's Up", "4 Non Blondes", "A Bm D A", 134),
        Song(98, "Torn", "Natalie Imbruglia", "F A# Dm C", 96),
        Song(99, "Complicated", "Avril Lavigne", "Dm A# F C", 78),
        Song(100, "21 Guns", "Green Day", "Dm A# F C", 80)
    ];

    private static readonly IReadOnlyDictionary<string, string> FlatRootAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Bb"] = "A#",
        ["Db"] = "C#",
        ["Eb"] = "D#",
        ["Gb"] = "F#",
        ["Ab"] = "G#"
    };

    private readonly Random _random = random ?? Random.Shared;

    public IReadOnlyList<ChordSymbol> ParseProgression(string input)
    {
        var parts = input
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            throw new ArgumentException("Enter at least one chord, for example Am, C, G, D.", nameof(input));
        }

        return parts.Select(ParseChord).ToArray();
    }

    public IReadOnlyList<int> ParseChordLengths(string input, int chordCount, int defaultLength)
    {
        if (chordCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chordCount), "Chord count must be greater than zero.");
        }

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

    public PresetChordProgression? GetPresetProgression(int number)
    {
        return PresetProgressions.FirstOrDefault(progression => progression.Number == number);
    }

    private static PresetChordProgression Song(
        int number,
        string title,
        string artist,
        string progressionText,
        int bpm,
        TimeSignature? timeSignature = null,
        IReadOnlyList<int>? chordLengths = null)
    {
        var meter = timeSignature ?? FourFour;
        return new PresetChordProgression(number, title, artist, progressionText, bpm, meter, chordLengths ?? OneMeasureEach(progressionText, meter.BeatsPerBar));
    }

    private static IReadOnlyList<int> OneMeasureEach(string progressionText, int beatsPerMeasure)
    {
        var chordCount = progressionText
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Length;

        return Enumerable.Repeat(beatsPerMeasure, chordCount).ToArray();
    }

    public IReadOnlyList<TriadPracticeItem> BuildPhrase(
        IReadOnlyList<ChordSymbol> progression,
        TriadPracticeItem? previousItem = null)
    {
        if (progression.Count == 0)
        {
            throw new ArgumentException("Progression must contain at least one chord.", nameof(progression));
        }

        var phrase = new List<TriadPracticeItem>();
        var anchorFret = previousItem?.CenterFret;

        foreach (var chord in progression)
        {
            var candidates = GetCandidates(chord).ToArray();
            var item = PickNear(candidates, anchorFret);
            phrase.Add(item);
            anchorFret = item.CenterFret;
        }

        return phrase;
    }

    private IEnumerable<TriadPracticeItem> GetCandidates(ChordSymbol chord)
    {
        return triads
            .GetTriadShapes(chord.Root, chord.Quality, voicingKind)
            .SelectMany(grouping => grouping.Shapes.Select(shape => new TriadPracticeItem(chord, grouping.Name, shape)));
    }

    private TriadPracticeItem PickNear(IReadOnlyList<TriadPracticeItem> candidates, double? anchorFret)
    {
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("No triad shapes are available for this chord.");
        }

        if (anchorFret is null)
        {
            var playable = candidates
                .Where(candidate => candidate.Shape.MinFret > 0 && candidate.Shape.MaxFret <= 9)
                .DefaultIfEmpty()
                .Where(candidate => candidate is not null)
                .Cast<TriadPracticeItem>()
                .ToArray();

            return playable[_random.Next(playable.Length)];
        }

        var bestDistance = candidates.Min(candidate => Math.Abs(candidate.CenterFret - anchorFret.Value));
        var nearby = candidates
            .Where(candidate => Math.Abs(candidate.CenterFret - anchorFret.Value) <= bestDistance + 2.5)
            .OrderBy(candidate => Math.Abs(candidate.CenterFret - anchorFret.Value))
            .ThenBy(candidate => candidate.Shape.MinFret)
            .Take(5)
            .ToArray();

        return nearby[_random.Next(nearby.Length)];
    }

    private static ChordSymbol ParseChord(string value)
    {
        var chord = value.Trim();
        var quality = ChordQuality.Major;

        if (chord.EndsWith("minor", StringComparison.OrdinalIgnoreCase))
        {
            quality = ChordQuality.Minor;
            chord = chord[..^5];
        }
        else if (chord.EndsWith("min", StringComparison.OrdinalIgnoreCase))
        {
            quality = ChordQuality.Minor;
            chord = chord[..^3];
        }
        else if (chord.EndsWith('m') && !chord.EndsWith("maj", StringComparison.OrdinalIgnoreCase))
        {
            quality = ChordQuality.Minor;
            chord = chord[..^1];
        }
        else if (chord.EndsWith("major", StringComparison.OrdinalIgnoreCase))
        {
            chord = chord[..^5];
        }
        else if (chord.EndsWith("maj", StringComparison.OrdinalIgnoreCase))
        {
            chord = chord[..^3];
        }

        chord = NormalizeRootName(chord);

        if (!MusicTheory.ChromaticRoots.Contains(chord))
        {
            throw new ArgumentException($"'{value}' is not a supported chord. Use roots A-G with optional #/b and optional m, for example F#m or Bb.");
        }

        return new ChordSymbol(chord, quality);
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
}

public enum TriadVoicingKind
{
    Close,
    Spread
}

public static class TriadVoicingExtensions
{
    public static IReadOnlyList<TriadGroupingResult> GetTriadShapes(
        this TriadInversionLibrary triads,
        string root,
        ChordQuality quality,
        TriadVoicingKind voicingKind)
    {
        return voicingKind == TriadVoicingKind.Spread
            ? triads.GetSpreadTriads(root, quality)
            : triads.GetTriadInversions(root, quality);
    }
}

public sealed record ChordSymbol(string Root, ChordQuality Quality)
{
    public string DisplayName => Quality == ChordQuality.Minor ? $"{Root}m" : Root;
}

public sealed record PresetChordProgression(
    int Number,
    string Title,
    string Artist,
    string ProgressionText,
    int Bpm,
    TimeSignature TimeSignature,
    IReadOnlyList<int> ChordLengths)
{
    public string Name => $"{Title} - {Artist}";

    public string MenuText => $"{Number:000}. {Title} - {Artist}: {ProgressionText}";
}

public sealed record TimeSignature(int BeatsPerBar, int BeatUnit)
{
    public string DisplayName => $"{BeatsPerBar}/{BeatUnit}";
}

public sealed record TriadPracticeItem(ChordSymbol Chord, string GroupingName, TriadShape Shape)
{
    private static readonly IReadOnlyList<string> FullFretboardStrings = ["E", "B", "G", "D", "A", "E"];

    public double CenterFret => (Shape.MinFret + Shape.MaxFret) / 2.0;

    public FretboardDiagram Diagram => new(
        FullFretboardStrings,
        Shape.Diagram.StartFret,
        Shape.Diagram.Length,
        Shape.Diagram.Positions
            .Where(position => position.SourceStringIndex is not null)
            .Select(position => position with { StringIndex = position.SourceStringIndex!.Value })
            .ToArray());

    public string Title => $"{Chord.DisplayName} {Shape.InversionName} {GroupingName} ({Shape.MinFret}-{Shape.MaxFret})";
}
