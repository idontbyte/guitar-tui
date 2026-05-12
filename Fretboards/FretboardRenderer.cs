using System.Text;
using System.Text.RegularExpressions;

namespace GuitarResourcesTui.Fretboards;

public sealed class FretboardRenderer
{
    private const int DiagramGap = 3;
    private const string Reset = "\e[0m";
    private const string Dim = "\e[2m";
    private const string FretNumber = "\e[38;5;244m";
    private const string Nut = "\e[38;5;230m";
    private const string NutColumn = "\e[38;5;230;48;5;238m";
    private const string StringName = "\e[38;5;117m";
    private const string Root = "\e[1;38;5;46m";
    private const string Third = "\e[1;38;5;220m";
    private const string Fifth = "\e[1;38;5;39m";
    private const string Pentatonic = "\e[1;38;5;213m";
    private const string BlueNote = "\e[1;38;5;51m";
    private const string Muted = "\e[1;38;5;196m";
    private const string HighlightRoot = "\e[1;38;5;16;48;5;46m";
    private const string HighlightThird = "\e[1;38;5;16;48;5;220m";
    private const string HighlightFifth = "\e[1;38;5;16;48;5;39m";
    private const string HighlightOther = "\e[1;38;5;16;48;5;231m";
    private const string HighlightEmpty = "\e[38;5;236;48;5;235m";

    private static readonly Regex AnsiPattern = new(@"\e\[[0-9;]*m", RegexOptions.Compiled);

    public IReadOnlyList<string> Render(FretboardDiagram diagram, bool highlighted = false)
    {
        if (diagram.Length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(diagram), "Fretboard length must be at least one fret.");
        }

        var markers = diagram.Positions
            .GroupBy(position => (position.StringIndex, position.Fret))
            .ToDictionary(group => group.Key, group => group.Last());

        var lines = new List<string>
        {
            RenderFretNumbers(diagram.StartFret, diagram.Length)
        };

        for (var stringIndex = 0; stringIndex < diagram.Strings.Count; stringIndex++)
        {
            lines.Add(RenderString(diagram, stringIndex, markers, highlighted));
        }

        return lines;
    }

    public IReadOnlyList<string> RenderMany(
        IReadOnlyList<(string Title, FretboardDiagram Diagram)> diagrams,
        int? maxWidth = null,
        IReadOnlySet<int>? highlightedIndexes = null,
        int? maxColumns = null,
        int? cellWidth = null)
    {
        if (diagrams.Count == 0)
        {
            return Array.Empty<string>();
        }

        var rendered = diagrams
            .Select((diagram, index) => RenderTitledDiagram(diagram, highlightedIndexes?.Contains(index) == true, cellWidth))
            .ToArray();
        var rows = WrapRenderedDiagrams(rendered, maxWidth, maxColumns);

        var output = new List<string>();

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            if (rowIndex > 0)
            {
                output.Add(string.Empty);
            }

            output.AddRange(RenderDiagramRow(rows[rowIndex]));
        }

        return output;
    }

    public int MeasureTitledDiagramWidth((string Title, FretboardDiagram Diagram) item)
    {
        var body = Render(item.Diagram);
        return Math.Max(VisibleLength(item.Title), body.Max(VisibleLength));
    }

    private string[] RenderTitledDiagram((string Title, FretboardDiagram Diagram) item, bool highlighted, int? minimumWidth)
    {
        var body = Render(item.Diagram, highlighted);
        var width = Math.Max(minimumWidth ?? 0, Math.Max(VisibleLength(item.Title), body.Max(VisibleLength)));
        var title = highlighted
            ? PadRightVisible(HighlightTitleChord(item.Title), width)
            : PadRightVisible(item.Title, width);
        return [title, .. body.Select(line => PadRightVisible(line, width))];
    }

    private static string HighlightTitleChord(string title)
    {
        var chordStart = title.StartsWith("> ", StringComparison.Ordinal) ? 2 : 0;
        while (chordStart < title.Length && title[chordStart] == ' ')
        {
            chordStart++;
        }

        if (chordStart >= title.Length)
        {
            return title;
        }

        var chordEnd = title.IndexOf(' ', chordStart);
        if (chordEnd < 0)
        {
            chordEnd = title.Length;
        }

        return title[..chordStart] + Color(title[chordStart..chordEnd], Third) + title[chordEnd..];
    }

    private static IReadOnlyList<IReadOnlyList<string[]>> WrapRenderedDiagrams(IReadOnlyList<string[]> rendered, int? maxWidth, int? maxColumns)
    {
        var rows = new List<IReadOnlyList<string[]>>();
        var current = new List<string[]>();
        var currentWidth = 0;
        var columnLimit = maxColumns.GetValueOrDefault(int.MaxValue);

        foreach (var diagram in rendered)
        {
            var width = VisibleLength(diagram[0]);
            var nextWidth = current.Count == 0 ? width : currentWidth + DiagramGap + width;

            if (current.Count > 0 && (current.Count >= columnLimit || (maxWidth is not null && nextWidth > maxWidth.Value)))
            {
                rows.Add(current.ToArray());
                current.Clear();
                currentWidth = 0;
            }

            current.Add(diagram);
            currentWidth = currentWidth == 0 ? width : currentWidth + DiagramGap + width;
        }

        if (current.Count > 0)
        {
            rows.Add(current.ToArray());
        }

        return rows;
    }

    private static IReadOnlyList<string> RenderDiagramRow(IReadOnlyList<string[]> rendered)
    {
        var maxHeight = rendered.Max(lines => lines.Length);
        var output = new List<string>();

        for (var row = 0; row < maxHeight; row++)
        {
            var parts = rendered.Select(lines => row < lines.Length ? lines[row] : new string(' ', VisibleLength(lines[0])));
            output.Add(string.Join(new string(' ', DiagramGap), parts));
        }

        return output;
    }

    private static string RenderFretNumbers(int startFret, int length)
    {
        var builder = new StringBuilder("    ");

        for (var fret = startFret; fret < startFret + length; fret++)
        {
            builder.Append(Color(fret.ToString().PadLeft(3).PadRight(5), fret == 0 ? Nut : FretNumber));
        }

        return builder.ToString().TrimEnd();
    }

    private static string RenderString(
        FretboardDiagram diagram,
        int stringIndex,
        IReadOnlyDictionary<(int StringIndex, int Fret), FretPosition> markers,
        bool highlighted)
    {
        var builder = new StringBuilder();
        builder.Append(Color(diagram.Strings[stringIndex].PadLeft(2), StringName));
        builder.Append(Color(" |", diagram.StartFret == 0 ? Nut : Dim));

        for (var fret = diagram.StartFret; fret < diagram.StartFret + diagram.Length; fret++)
        {
            if (markers.TryGetValue((stringIndex, fret), out var position))
            {
                var label = position.IsMuted ? "X" : position.Label;
                var displayLabel = DisplayLabel(label);
                var paddedLabel = displayLabel.Length > 3 ? displayLabel[..3] : displayLabel.PadLeft(3).PadRight(4);
                builder.Append(Color(paddedLabel, ColorFor(label, highlighted, fret == 0)));
            }
            else
            {
                builder.Append(Color("----", fret == 0 ? NutColumn : highlighted ? HighlightEmpty : Dim));
            }

            builder.Append(Color("|", fret == 0 ? NutColumn : Dim));
        }

        return builder.ToString();
    }

    private static string Color(string value, string color) => $"{color}{value}{Reset}";

    private static string DisplayLabel(string label) => label switch
    {
        "b2" => "♭2",
        "b3" => "♭3",
        "#4" => "♯4",
        "b5" => "♭5",
        "b6" => "♭6",
        "b7" => "♭7",
        _ => label
    };

    private static string ColorFor(string label, bool highlighted = false, bool openFret = false)
    {
        if (openFret)
        {
            return label switch
            {
                "R" => "\e[1;38;5;46;48;5;238m",
                "3" => "\e[1;38;5;220;48;5;238m",
                "b3" => "\e[1;38;5;220;48;5;238m",
                "5" => "\e[1;38;5;39;48;5;238m",
                "X" => "\e[1;38;5;196;48;5;238m",
                _ => "\e[1;38;5;231;48;5;238m"
            };
        }

        if (highlighted)
        {
            return label switch
            {
                "R" => HighlightRoot,
                "3" => HighlightThird,
                "b3" => HighlightThird,
                "5" => HighlightFifth,
                _ => HighlightOther
            };
        }

        return label switch
    {
        "R" => Root,
        "2" => Pentatonic,
        "b2" => Pentatonic,
        "3" => Third,
        "b3" => Third,
        "4" => Pentatonic,
        "#4" => BlueNote,
        "b5" => BlueNote,
        "5" => Fifth,
        "6" => Pentatonic,
        "b6" => Pentatonic,
        "7" => Pentatonic,
        "b7" => Pentatonic,
        "X" => Muted,
        _ => Reset
    };
    }

    private static int VisibleLength(string value) => AnsiPattern.Replace(value, string.Empty).Length;

    private static string PadRightVisible(string value, int totalWidth)
    {
        var padding = totalWidth - VisibleLength(value);
        return padding <= 0 ? value : value + new string(' ', padding);
    }
}
