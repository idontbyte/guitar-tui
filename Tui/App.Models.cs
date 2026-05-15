using GuitarResourcesTui.Triads;
using GuitarResourcesTui.Pentatonics;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private sealed record TriadProgressionSetup(
        string Title,
        string ProgressionText,
        int? Bpm = null,
        TimeSignature? TimeSignature = null,
        IReadOnlyList<int>? ChordLengths = null);

    private sealed record ScaleSuggestion(
        string KeyRoot,
        PentatonicScaleKind ScaleKind,
        int Score,
        string Reason);

    private sealed record KeyCandidate(
        string Root,
        bool Minor,
        int Score,
        int ChordMismatches,
        string Reason);

    private sealed record TunerNote(
        string MenuKey,
        string Name,
        string DisplayName,
        int MidiNote);

    private enum TriadGameLayout
    {
        PhraseRows,
        RollingNextThree
    }

    private sealed record IntervalPrompt(ChordSymbol Chord, string Interval, BackingChord BackingChord);

    private sealed record BackingChord(
        string Root,
        ChordQuality Quality,
        IReadOnlySet<string> Intervals,
        string Suffix)
    {
        public string DisplayName => $"{Root}{Suffix}";
    }
}
