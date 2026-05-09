namespace GuitarResourcesTui.Fretboards;

public sealed record FretboardDiagram(
    IReadOnlyList<string> Strings,
    int StartFret,
    int Length,
    IReadOnlyList<FretPosition> Positions);

public sealed record FretPosition(
    int StringIndex,
    int Fret,
    string Label,
    bool IsMuted = false,
    int? SourceStringIndex = null);
