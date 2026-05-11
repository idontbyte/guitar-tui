using GuitarResourcesTui.Fretboards;

namespace GuitarResourcesTui.Triads;

public sealed class TriadProgressionGameLibrary(TriadInversionLibrary triads, Random? random = null)
{
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
            .GetTriadInversions(chord.Root, chord.Quality)
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

public sealed record ChordSymbol(string Root, ChordQuality Quality)
{
    public string DisplayName => Quality == ChordQuality.Minor ? $"{Root}m" : Root;
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
