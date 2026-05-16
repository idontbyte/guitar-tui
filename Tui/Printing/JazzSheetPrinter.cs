using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.JazzChords;
using GuitarResourcesTui.Triads;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace GuitarResourcesTui.Tui.Printing;

internal static class JazzSheetPrinter
{
    public static string PrintVoicingSheet(
        string root,
        JazzChordQuality quality,
        JazzVoicingMode voicingMode,
        IReadOnlyList<JazzChordVoicingGroup> groups)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"guitar-tui-jazz-{SafeFileName(root + quality.Suffix)}-{voicingMode}.html");
        File.WriteAllText(filePath, BuildVoicingHtml(root, quality, voicingMode, groups));
        Open(filePath);
        return filePath;
    }

    public static string PrintTwoFiveOneSheet(string keyRoot)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"guitar-tui-jazz-{SafeFileName(keyRoot)}-ii-v-i.html");
        File.WriteAllText(filePath, BuildTwoFiveOneHtml(keyRoot));
        Open(filePath);
        return filePath;
    }

    public static string PrintRoadmapSheet()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "guitar-tui-jazz-roadmap.html");
        File.WriteAllText(filePath, BuildRoadmapHtml());
        Open(filePath);
        return filePath;
    }

    public static string PrintStandardStudySheet(PresetJazzProgression preset)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"guitar-tui-jazz-standard-{SafeFileName(preset.Title)}.html");
        File.WriteAllText(filePath, BuildStandardHtml(preset));
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

    private static string BuildVoicingHtml(
        string root,
        JazzChordQuality quality,
        JazzVoicingMode voicingMode,
        IReadOnlyList<JazzChordVoicingGroup> groups)
    {
        var title = $"{root}{quality.Suffix} {voicingMode.DisplayName()}";
        var builder = StartHtml(title);
        builder.AppendLine($"<h1>{Html(title)}</h1>");
        builder.AppendLine($"<div class=\"subtitle\">{Html(quality.Name)} - formula: {Html(string.Join(" ", quality.Intervals))} - showing: {Html(string.Join(" ", JazzChordLibrary.IntervalsFor(quality, voicingMode)))}</div>");
        builder.AppendLine($"<p class=\"hint\">{Html(quality.Use)}. Practise each shape as a short chord, then move to the nearest shape for the next chord.</p>");
        AppendLegend(builder);

        foreach (var group in groups)
        {
            builder.AppendLine("<section>");
            builder.AppendLine($"<h2>{Html(group.Name)}</h2>");
            builder.AppendLine("<div class=\"shapes\">");

            foreach (var voicing in group.Voicings)
            {
                builder.AppendLine("<div class=\"shape\">");
                builder.AppendLine($"<div class=\"shape-title\">{Html(voicing.LowToHigh)} ({voicing.MinFret}-{voicing.MaxFret})</div>");
                AppendDiagramHtml(builder, voicing.Diagram);
                builder.AppendLine("</div>");
            }

            builder.AppendLine("</div>");
            builder.AppendLine("</section>");
        }

        EndHtml(builder);
        return builder.ToString();
    }

    private static string BuildTwoFiveOneHtml(string keyRoot)
    {
        var rootPitch = MusicTheory.PitchClassFor(keyRoot);
        var major = new[]
        {
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 2), JazzChordLibrary.Quality("m7")),
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 7), JazzChordLibrary.Quality("9")),
            new JazzChordSymbol(keyRoot, JazzChordLibrary.Quality("maj7"))
        };
        var minor = new[]
        {
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 2), JazzChordLibrary.Quality("m7b5")),
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 7), JazzChordLibrary.Quality("7b9")),
            new JazzChordSymbol(keyRoot, JazzChordLibrary.Quality("m6"))
        };

        var builder = StartHtml($"{keyRoot} ii-V-I worksheet");
        builder.AppendLine($"<h1>{Html(keyRoot)} ii-V-I worksheet</h1>");
        builder.AppendLine("<p class=\"hint\">A ii-V-I is built from scale degrees 2, 5, and 1. Learn it as harmony, comping, and solo targets.</p>");
        AppendProgressionSection(builder, "Major ii-V-I", major);
        AppendProgressionSection(builder, "Minor ii-V-i", minor);
        builder.AppendLine("<h2>Practice order</h2>");
        builder.AppendLine("<ol>");
        builder.AppendLine("<li>Say each chord name and function out loud.</li>");
        builder.AppendLine("<li>Play only guide tones and listen for small movements.</li>");
        builder.AppendLine("<li>Add roots for shell voicings and comp a steady rhythm.</li>");
        builder.AppendLine("<li>Solo using only 3rds and 7ths, then add arpeggio tones.</li>");
        builder.AppendLine("<li>Add approach notes and enclosures after the targets feel clear.</li>");
        builder.AppendLine("</ol>");
        EndHtml(builder);
        return builder.ToString();
    }

    private static string BuildRoadmapHtml()
    {
        var builder = StartHtml("Jazz roadmap");
        builder.AppendLine("<h1>Jazz roadmap</h1>");
        builder.AppendLine("<p class=\"hint\">The goal is to keep time, support the tune, hear the changes, and make melodies from chord tones.</p>");
        builder.AppendLine("<ol>");
        AppendRoadmapItem(builder, "Map the harmony", "Learn maj7, 6, m7, dominant 7, m7b5, dim7, and altered dominants.");
        AppendRoadmapItem(builder, "Hear guide tones", "Find the 3rd and 7th of every seventh chord and connect them smoothly.");
        AppendRoadmapItem(builder, "Build shell voicings", "Add roots to guide tones for small, practical comping grips.");
        AppendRoadmapItem(builder, "Internalize ii-V-I", "Drill major and minor ii-V-I in many keys.");
        AppendRoadmapItem(builder, "Learn comping rhythms", "Practise four-to-the-bar, Charleston, anticipations, space, and la pompe.");
        AppendRoadmapItem(builder, "Learn standards", "Memorize form, melody, and changes.");
        AppendRoadmapItem(builder, "Solo through changes", "Target chord tones, then add approaches, enclosures, and rhythmic phrasing.");
        AppendRoadmapItem(builder, "Add style language", "Study gypsy-jazz rhythm, minor 6 sounds, diminished passing chords, and chromatic arpeggios.");
        builder.AppendLine("</ol>");
        EndHtml(builder);
        return builder.ToString();
    }

    private static string BuildStandardHtml(PresetJazzProgression preset)
    {
        var chords = new JazzChordLibrary().ParseProgression(preset.ProgressionText);
        var builder = StartHtml($"{preset.Title} study sheet");
        builder.AppendLine($"<h1>{Html(preset.Title)}</h1>");
        builder.AppendLine($"<div class=\"subtitle\">{Html(preset.Artist)} - BPM {preset.Bpm} - {Html(preset.TimeSignature.DisplayName)}</div>");
        builder.AppendLine($"<p class=\"hint\">Concepts: {Html(preset.Concepts)}</p>");
        builder.AppendLine($"<p><b>Changes:</b> {Html(preset.ProgressionText)}</p>");
        AppendProgressionSection(builder, "Guide-tone map", chords);
        builder.AppendLine("<h2>Study stages</h2>");
        builder.AppendLine("<ol>");
        builder.AppendLine("<li>Map the form: group the changes into phrases and mark ii-V, tonic, turnaround, and minor cadence points.</li>");
        builder.AppendLine("<li>Comp shells: play root, 3rd, and 7th with steady time.</li>");
        builder.AppendLine("<li>Guide-tone melody: connect the 3rds and 7ths without full chords.</li>");
        builder.AppendLine("<li>Solo targets: add approaches, enclosures, and arpeggio outlines.</li>");
        builder.AppendLine("<li>Performance loop: one chorus comping, one chorus target-note soloing, one chorus freer.</li>");
        builder.AppendLine("</ol>");
        EndHtml(builder);
        return builder.ToString();
    }

    private static StringBuilder StartHtml(string title)
    {
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
h2 { border-bottom: 1px solid #999; font-size: 15px; margin: 18px 0 10px; padding-bottom: 4px; }
.subtitle, .hint { color: #555; font-size: 12px; margin-bottom: 14px; }
.legend { display: flex; flex-wrap: wrap; gap: 12px; font-size: 12px; margin-bottom: 18px; }
.legend span { align-items: center; display: inline-flex; gap: 4px; }
.dot { border: 1px solid #111; border-radius: 999px; display: inline-block; height: 16px; line-height: 16px; text-align: center; width: 22px; }
.root { background: #c8f7c5; }
.third { background: #ffe58f; }
.fifth { background: #b7d9ff; }
.color { background: #e4c7ff; }
.shapes { display: grid; gap: 14px; grid-template-columns: repeat(2, minmax(0, 1fr)); }
.shape, section { break-inside: avoid; page-break-inside: avoid; }
.shape-title { font-size: 12px; font-weight: 700; margin-bottom: 4px; }
table { border-collapse: collapse; font-size: 11px; margin-bottom: 12px; width: 100%; }
th { color: #555; font-weight: 500; height: 18px; }
td { border: 1px solid #777; height: 24px; min-width: 28px; text-align: center; }
td.string-name { border: 0; color: #555; font-weight: 700; min-width: 20px; width: 20px; }
td.note { border: 2px solid #111; border-radius: 999px; font-weight: 800; }
td.nut, th.nut { background: #f8f0d6; }
.progression { width: auto; }
.progression td, .progression th { padding: 5px 8px; width: auto; }
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
        return builder;
    }

    private static void EndHtml(StringBuilder builder)
    {
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
    }

    private static void AppendLegend(StringBuilder builder)
    {
        builder.AppendLine("<div class=\"legend\"><span><b class=\"dot root\">R</b> root</span><span><b class=\"dot third\">3</b> third</span><span><b class=\"dot fifth\">5</b> fifth</span><span><b class=\"dot color\">7</b> color tones</span><span>pale column = nut / fret 0</span></div>");
    }

    private static void AppendProgressionSection(StringBuilder builder, string title, IReadOnlyList<JazzChordSymbol> chords)
    {
        builder.AppendLine($"<h2>{Html(title)}</h2>");
        builder.AppendLine("<table class=\"progression\"><thead><tr><th>Chord</th><th>Function notes</th><th>Guide tones</th><th>Practice prompt</th></tr></thead><tbody>");
        foreach (var chord in chords)
        {
            builder.AppendLine("<tr>");
            builder.AppendLine($"<td><b>{Html(chord.DisplayName)}</b></td>");
            builder.AppendLine($"<td>{Html(chord.Quality.Use)}</td>");
            builder.AppendLine($"<td>{Html(JazzChordLibrary.GuideToneSummary(chord))}</td>");
            builder.AppendLine("<td>Comp shell, sing target notes, then make a two-bar phrase.</td>");
            builder.AppendLine("</tr>");
        }
        builder.AppendLine("</tbody></table>");
    }

    private static void AppendRoadmapItem(StringBuilder builder, string title, string text)
    {
        builder.AppendLine($"<li><b>{Html(title)}</b>: {Html(text)}</li>");
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

    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var safe = new string(value.Select(character => invalid.Contains(character) || character is '/' or '\\' or ' ' ? '-' : character).ToArray());
        return safe.Replace('#', 's');
    }

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
