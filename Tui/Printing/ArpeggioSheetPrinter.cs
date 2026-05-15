using GuitarResourcesTui.Arpeggios;
using GuitarResourcesTui.Fretboards;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace GuitarResourcesTui.Tui.Printing;

internal static class ArpeggioSheetPrinter
{
    public static string Print(string root, ArpeggioQuality quality, IReadOnlyList<ArpeggioShape> shapes)
    {
        var suffix = string.IsNullOrEmpty(quality.Suffix) ? "major" : quality.Suffix.Replace('/', '-');
        var filePath = Path.Combine(Path.GetTempPath(), $"guitar-tui-{root}-{suffix}-arpeggios.html");
        File.WriteAllText(filePath, BuildHtml(root, quality, shapes));
        Open(filePath);
        return filePath;
    }

    private static void Open(string filePath)
    {
        if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList = { filePath }
            });
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }

    private static string BuildHtml(string root, ArpeggioQuality quality, IReadOnlyList<ArpeggioShape> shapes)
    {
        var title = $"{root}{quality.Suffix} arpeggios";
        var builder = new StringBuilder();

        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("<meta charset=\"utf-8\">");
        builder.AppendLine($"<title>{Html(title)}</title>");
        builder.AppendLine("""
<style>
body { color: #111; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; margin: 24px; }
h1 { font-size: 24px; margin: 0 0 4px; }
.subtitle, .hint { color: #555; font-size: 12px; margin-bottom: 14px; }
.legend { display: flex; flex-wrap: wrap; gap: 12px; font-size: 12px; margin-bottom: 18px; }
.legend span { align-items: center; display: inline-flex; gap: 4px; }
.dot { border: 1px solid #111; border-radius: 999px; display: inline-block; height: 16px; line-height: 16px; text-align: center; width: 22px; }
.root { background: #c8f7c5; }
.third { background: #ffe58f; }
.fifth { background: #b7d9ff; }
.color { background: #e4c7ff; }
.shapes { display: grid; gap: 14px; grid-template-columns: repeat(2, minmax(0, 1fr)); }
.shape { break-inside: avoid; page-break-inside: avoid; }
.shape-title { font-size: 12px; font-weight: 700; margin-bottom: 4px; }
table { border-collapse: collapse; font-size: 11px; width: 100%; }
th { color: #555; font-weight: 500; height: 18px; }
td { border: 1px solid #777; height: 24px; min-width: 28px; text-align: center; }
td.string-name { border: 0; color: #555; font-weight: 700; min-width: 20px; width: 20px; }
td.note { border: 2px solid #111; border-radius: 999px; font-weight: 800; }
td.nut, th.nut { background: #f8f0d6; }
@media print {
  body { margin: 12mm; }
  button { display: none; }
  .shapes { grid-template-columns: repeat(2, 1fr); }
}
</style>
<script>
window.addEventListener("load", () => setTimeout(() => window.print(), 250));
</script>
""");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine($"<h1>{Html(title)}</h1>");
        builder.AppendLine($"<div class=\"subtitle\">Formula: {Html(string.Join(" ", quality.Intervals))} · Notes: {Html(ArpeggioLibrary.NotesFor(new ArpeggioChordSymbol(root, quality)))}</div>");
        builder.AppendLine("<div class=\"hint\">Arpeggios are chord tones played one at a time. Practise ascending, descending, then landing on a target tone when the chord changes.</div>");
        builder.AppendLine("<div class=\"legend\"><span><b class=\"dot root\">R</b> root</span><span><b class=\"dot third\">3</b> third</span><span><b class=\"dot fifth\">5</b> fifth</span><span><b class=\"dot color\">7</b> color tones</span><span>pale column = nut / fret 0</span></div>");
        builder.AppendLine("<div class=\"shapes\">");

        foreach (var shape in shapes)
        {
            builder.AppendLine("<div class=\"shape\">");
            builder.AppendLine($"<div class=\"shape-title\">Position {shape.Number} ({shape.StartFret}-{shape.StartFret + shape.Diagram.Length - 1})</div>");
            AppendDiagramHtml(builder, shape.Diagram);
            builder.AppendLine("</div>");
        }

        builder.AppendLine("</div>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
        return builder.ToString();
    }

    private static void AppendDiagramHtml(StringBuilder builder, FretboardDiagram diagram)
    {
        var markers = diagram.Positions
            .GroupBy(position => (position.StringIndex, position.Fret))
            .ToDictionary(group => group.Key, group => group.Last());

        builder.AppendLine("<table>");
        builder.AppendLine("<thead><tr><th></th>");
        for (var fret = diagram.StartFret; fret < diagram.StartFret + diagram.Length; fret++)
        {
            builder.AppendLine($"<th class=\"{(fret == 0 ? "nut" : string.Empty)}\">{fret}</th>");
        }
        builder.AppendLine("</tr></thead>");
        builder.AppendLine("<tbody>");

        for (var stringIndex = 0; stringIndex < diagram.Strings.Count; stringIndex++)
        {
            builder.AppendLine("<tr>");
            builder.AppendLine($"<td class=\"string-name\">{Html(diagram.Strings[stringIndex])}</td>");

            for (var fret = diagram.StartFret; fret < diagram.StartFret + diagram.Length; fret++)
            {
                if (markers.TryGetValue((stringIndex, fret), out var position))
                {
                    builder.AppendLine($"<td class=\"note {CssClassFor(position.Label)}{(fret == 0 ? " nut" : string.Empty)}\">{Html(position.Label)}</td>");
                }
                else
                {
                    builder.AppendLine($"<td class=\"{(fret == 0 ? "nut" : string.Empty)}\"></td>");
                }
            }

            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody>");
        builder.AppendLine("</table>");
    }

    private static string CssClassFor(string label) => label switch
    {
        "R" => "root",
        "3" or "b3" => "third",
        "5" or "b5" => "fifth",
        _ => "color"
    };

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
