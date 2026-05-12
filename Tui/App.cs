using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.IntervalMaps;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Triads;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace GuitarResourcesTui.Tui;

public sealed class App(TriadInversionLibrary triads, PentatonicLibrary pentatonics, IntervalFunctionMapLibrary intervalMaps)
{
    private const string Reset = "\e[0m";
    private const string Root = "\e[1;38;5;46m";
    private const string Third = "\e[1;38;5;220m";
    private const string Fifth = "\e[1;38;5;39m";
    private const string Pentatonic = "\e[1;38;5;213m";
    private const string BlueNote = "\e[1;38;5;51m";

    private static readonly IReadOnlyList<string> SimpleTriadProgressions =
    [
        "C G Am F",
        "G D Em C",
        "D A Bm G",
        "A E F#m D",
        "E B C#m A",
        "Am F C G",
        "Em C G D",
        "C Am F G",
        "G C D G",
        "D G A D"
    ];

    private readonly FretboardRenderer _renderer = new();
    private readonly TriadProgressionGameLibrary _triadGame = new(triads);
    private readonly TriadProgressionGameLibrary _spreadTriadGame = new(triads, voicingKind: TriadVoicingKind.Spread);

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Guitar Resources");
            Console.WriteLine("1. Triads");
            Console.WriteLine("2. Scales");
            Console.WriteLine("3. Intervals");
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowTriadsMenu();
                    break;
                case "2":
                    ShowScalesMenu();
                    break;
                case "3":
                    ShowIntervalsMenu();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("That choice is not on the menu.");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }

    private void ShowTriadsMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Triads");
            Console.WriteLine("1. Inversions");
            Console.WriteLine("2. Music game");
            Console.WriteLine("3. Spread triads music game");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowTriadInversions();
                    break;
                case "2":
                    ShowTriadProgressionGame("Triad music game", _triadGame);
                    break;
                case "3":
                    ShowTriadProgressionGame("Spread triads music game", _spreadTriadGame);
                    break;
                case "B":
                case "b":
                    return;
                default:
                    Console.WriteLine("That choice is not on the menu.");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }

    private void ShowTriadProgressionGame(string gameTitle, TriadProgressionGameLibrary game)
    {
        Console.Clear();
        WriteHeader(gameTitle);
        var setup = ReadTriadProgressionSetup();
        if (setup is null)
        {
            return;
        }

        IReadOnlyList<ChordSymbol> progression;
        try
        {
            progression = game.ParseProgression(setup.ProgressionText);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
            Console.ReadKey(intercept: true);
            return;
        }

        var bpm = setup.Bpm ?? ReadInt("BPM", defaultValue: 80, min: 30, max: 240);
        var timeSignature = setup.TimeSignature ?? ReadTimeSignature();
        var chordLengths = setup.ChordLengths ?? ReadChordLengths(progression, timeSignature);

        var clickEnabled = true;
        var backingEnabled = true;
        var voiceEnabled = true;

        var currentPhrase = game.BuildPhrase(progression);
        var nextPhrase = game.BuildPhrase(progression, currentPhrase[^1]);
        var beat = 0;
        var paused = false;
        var lastSynthChordIndex = -1;
        var lastRenderedChordIndex = -1;
        var phraseLength = chordLengths.Sum();
        var synthProcesses = new List<AudioPlayback>();
        using var voiceAnnouncer = new VoiceAnnouncer();
        WarmBackingChords(currentPhrase, chordLengths, bpm, timeSignature);
        WarmBackingChords(nextPhrase, chordLengths, bpm, timeSignature);

        try
        {
            while (true)
            {
                var beatStartedAt = DateTime.UtcNow;
                var chordIndex = ChordIndexAtBeat(chordLengths, beat);
                var beatWithinChord = beat - StartBeatForChord(chordLengths, chordIndex);
                var beatWithinBar = beat % timeSignature.BeatsPerBar;
                var chordChanged = chordIndex != lastRenderedChordIndex;
                var currentChord = currentPhrase[chordIndex].Chord;

                if (!paused && backingEnabled && chordIndex != lastSynthChordIndex)
                {
                    CleanupFinishedProcesses(synthProcesses);
                    var synthProcess = PlayBackingChord(currentChord, chordLengths[chordIndex], bpm, timeSignature);
                    if (synthProcess is not null)
                    {
                        synthProcesses.Add(synthProcess);
                    }
                    lastSynthChordIndex = chordIndex;
                }

                if (!paused && voiceEnabled && chordChanged)
                {
                    voiceAnnouncer.SayChord(currentChord);
                }

                if (chordChanged)
                {
                    RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused);
                    lastRenderedChordIndex = chordIndex;
                }

                if (!paused && clickEnabled)
                {
                    Click(beatWithinBar == 0);
                }

                var interval = TimeSpan.FromMinutes(1d / bpm);
                var deadline = beatStartedAt + interval;

                while (DateTime.UtcNow < deadline)
                {
                    if (Console.KeyAvailable)
                    {
                        switch (Console.ReadKey(intercept: true).Key)
                        {
                            case ConsoleKey.Q:
                            case ConsoleKey.B:
                            case ConsoleKey.Escape:
                                return;
                            case ConsoleKey.Spacebar:
                                paused = !paused;
                                if (paused)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    lastSynthChordIndex = -1;
                                }
                                if (paused)
                                {
                                    voiceAnnouncer.Stop();
                                }
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused);
                                break;
                            case ConsoleKey.S:
                                backingEnabled = !backingEnabled;
                                if (!backingEnabled)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    lastSynthChordIndex = -1;
                                }
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused);
                                break;
                            case ConsoleKey.V:
                                voiceEnabled = !voiceEnabled;
                                if (!voiceEnabled)
                                {
                                    voiceAnnouncer.Stop();
                                }
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused);
                                break;
                            case ConsoleKey.N:
                            case ConsoleKey.RightArrow:
                                beat = StartBeatForChord(chordLengths, chordIndex + 1);
                                goto BeatAdvancedManually;
                        }
                    }

                    Thread.Sleep(5);
                }

                if (!paused)
                {
                    beat++;
                }

            BeatAdvancedManually:
                if (beat >= phraseLength)
                {
                    currentPhrase = nextPhrase;
                    nextPhrase = game.BuildPhrase(progression, currentPhrase[^1]);
                    WarmBackingChords(nextPhrase, chordLengths, bpm, timeSignature);
                    beat = 0;
                    lastSynthChordIndex = -1;
                    lastRenderedChordIndex = -1;
                }
            }
        }
        finally
        {
            StopProcesses(synthProcesses);
        }
    }

    private TriadProgressionSetup? ReadTriadProgressionSetup()
    {
        Console.WriteLine("1. Simple mode");
        Console.WriteLine("C. Custom mode");
        Console.WriteLine("S. Song select");
        Console.WriteLine("B. Back");
        Console.WriteLine();
        Console.Write("Choose a mode > ");

        while (true)
        {
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            switch (input.ToUpperInvariant())
            {
                case "1":
                    return BuildSimpleTriadProgressionSetup();
                case "C":
                    return ReadCustomTriadProgressionSetup();
                case "S":
                    return ReadSongTriadProgressionSetup();
                case "B":
                case "Q":
                    return null;
                default:
                    Console.Write("Choose 1, C, S, or B > ");
                    break;
            }
        }
    }

    private static TriadProgressionSetup BuildSimpleTriadProgressionSetup()
    {
        var progression = SimpleTriadProgressions[Random.Shared.Next(SimpleTriadProgressions.Count)];
        return new TriadProgressionSetup($"Simple mode: {progression}", progression, 80, new TimeSignature(4, 4), [4, 4, 4, 4]);
    }

    private static TriadProgressionSetup BuildSimpleScaleProgressionSetup(string keyRoot, PentatonicScaleKind scaleKind)
    {
        var minor = IsMinorScale(scaleKind);
        var formulas = minor
            ? new[]
            {
                new[] { (0, ChordQuality.Minor), (8, ChordQuality.Major), (3, ChordQuality.Major), (10, ChordQuality.Major) },
                new[] { (0, ChordQuality.Minor), (5, ChordQuality.Minor), (10, ChordQuality.Major), (3, ChordQuality.Major) },
                new[] { (0, ChordQuality.Minor), (10, ChordQuality.Major), (8, ChordQuality.Major), (10, ChordQuality.Major) }
            }
            : new[]
            {
                new[] { (0, ChordQuality.Major), (7, ChordQuality.Major), (9, ChordQuality.Minor), (5, ChordQuality.Major) },
                new[] { (0, ChordQuality.Major), (5, ChordQuality.Major), (7, ChordQuality.Major), (0, ChordQuality.Major) },
                new[] { (0, ChordQuality.Major), (9, ChordQuality.Minor), (5, ChordQuality.Major), (7, ChordQuality.Major) }
            };
        var selected = formulas[Random.Shared.Next(formulas.Length)];
        var rootPitch = MusicTheory.PitchClassFor(keyRoot);
        var chords = selected
            .Select(chord => new ChordSymbol(MusicTheory.NameFor(rootPitch + chord.Item1), chord.Item2))
            .ToArray();
        var progression = string.Join(" ", chords.Select(chord => chord.DisplayName));

        return new TriadProgressionSetup($"{keyRoot} {PentatonicLibrary.NameFor(scaleKind)} simple: {progression}", progression, 80, new TimeSignature(4, 4), [4, 4, 4, 4]);
    }

    private TriadProgressionSetup? ReadScaleSongSetup(string keyRoot, PentatonicScaleKind scaleKind)
    {
        Console.WriteLine();
        Console.WriteLine("1. Simple mode");
        Console.WriteLine("C. Custom mode");
        Console.WriteLine("S. Song select");
        Console.WriteLine("B. Back");
        Console.WriteLine();
        Console.Write("Choose a mode > ");

        while (true)
        {
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            switch (input.ToUpperInvariant())
            {
                case "1":
                    return BuildSimpleScaleProgressionSetup(keyRoot, scaleKind);
                case "C":
                    return ReadCustomTriadProgressionSetup();
                case "S":
                    return ReadSongTriadProgressionSetup();
                case "B":
                case "Q":
                    return null;
                default:
                    Console.Write("Choose 1, C, S, or B > ");
                    break;
            }
        }
    }

    private static TriadProgressionSetup ReadCustomTriadProgressionSetup()
    {
        Console.WriteLine();
        Console.WriteLine("Enter chords separated by commas or spaces. Use m for minor, for example Am, C, G, D.");
        Console.Write("Progression > ");
        return new TriadProgressionSetup("Custom progression", Console.ReadLine()?.Trim() ?? string.Empty);
    }

    private TriadProgressionSetup? ReadSongTriadProgressionSetup()
    {
        Console.Clear();
        WriteHeader("Song select");
        foreach (var preset in TriadProgressionGameLibrary.PresetProgressions)
        {
            Console.WriteLine(preset.MenuText);
        }
        Console.WriteLine();
        Console.Write("Song number or B > ");

        while (true)
        {
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(input, out var presetNumber))
            {
                var preset = _triadGame.GetPresetProgression(presetNumber);
                if (preset is not null)
                {
                    return new TriadProgressionSetup(preset.Name, preset.ProgressionText, preset.Bpm, preset.TimeSignature, preset.ChordLengths);
                }
            }

            Console.Write("Choose 1-100 or B > ");
        }
    }

    private IReadOnlyList<int> ReadChordLengths(IReadOnlyList<ChordSymbol> progression, TimeSignature timeSignature)
    {
        var defaultBeatsPerChord = ReadInt("Default chord length in beats", defaultValue: timeSignature.BeatsPerBar, min: 1, max: 32);
        Console.WriteLine("Enter one length per chord, or leave blank to use the default for every chord.");
        Console.WriteLine($"Chords: {string.Join(" ", progression.Select(chord => chord.DisplayName))}");

        while (true)
        {
            Console.Write("Chord lengths > ");
            var lengthInput = Console.ReadLine()?.Trim() ?? string.Empty;
            try
            {
                return _triadGame.ParseChordLengths(lengthInput, progression.Count, defaultBeatsPerChord);
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }

    private static TimeSignature ReadTimeSignature()
    {
        var beatsPerBar = ReadInt("Time signature beats per bar", defaultValue: 4, min: 1, max: 12);
        var beatUnitChoice = ReadMenuChoice("Time signature beat unit", ["4", "8"], allowBack: false);
        return new TimeSignature(beatsPerBar, int.Parse(beatUnitChoice!));
    }

    private void ShowIntervalsMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Intervals");
            Console.WriteLine("1. Lookup");
            Console.WriteLine("2. Song game");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowIntervalLookup();
                    break;
                case "2":
                    ShowIntervalSongGame();
                    break;
                case "B":
                case "b":
                    return;
                default:
                    Console.WriteLine("That choice is not on the menu.");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }

    private void ShowIntervalLookup()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Interval lookup");

            var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            while (true)
            {
                var anchorFretChoice = ReadMenuChoice("Choose an anchor fret", ["0", "3", "5", "7", "9", "12"], allowBack: true);
                if (anchorFretChoice is null)
                {
                    break;
                }

                var anchorFret = int.Parse(anchorFretChoice);
                var selectedIntervals = ReadIntervalSet("Intervals to show, for example 3 5 b7, or blank for all");

                while (true)
                {
                    var diagram = selectedIntervals.Count == IntervalFunctionMapLibrary.IntervalLabels.Count
                        ? intervalMaps.BuildMap(root, anchorFret)
                        : intervalMaps.BuildLookup(root, anchorFret, selectedIntervals);

                    Console.Clear();
                    WriteHeader($"{root} interval lookup ({diagram.StartFret}-{diagram.StartFret + diagram.Length - 1})");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals are relative to {root}");
                    Console.WriteLine($"Showing: {string.Join(" ", selectedIntervals)}");
                    Console.WriteLine($"Anchor fret: {anchorFret}");
                    Console.WriteLine("N/P = move anchor fret, B = back, Q = main menu");
                    Console.WriteLine();

                    foreach (var line in _renderer.Render(diagram))
                    {
                        Console.WriteLine(line);
                    }

                    Console.WriteLine();
                    Console.Write("Command > ");

                    switch (Console.ReadKey(intercept: true).Key)
                    {
                        case ConsoleKey.N:
                        case ConsoleKey.RightArrow:
                            anchorFret = Math.Min(IntervalFunctionMapLibrary.MaxAnchorFret, anchorFret + 1);
                            break;
                        case ConsoleKey.P:
                        case ConsoleKey.LeftArrow:
                            anchorFret = Math.Max(0, anchorFret - 1);
                            break;
                        case ConsoleKey.B:
                        case ConsoleKey.Escape:
                            goto ChooseAnchor;
                        case ConsoleKey.Q:
                            return;
                    }
                }

            ChooseAnchor:
                continue;
            }
        }
    }

    private void ShowIntervalSongGame()
    {
        Console.Clear();
        WriteHeader("Interval song game");
        var setup = ReadTriadProgressionSetup();
        if (setup is null)
        {
            return;
        }

        IReadOnlyList<ChordSymbol> progression;
        try
        {
            progression = _triadGame.ParseProgression(setup.ProgressionText);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
            Console.ReadKey(intercept: true);
            return;
        }

        var targetIntervals = ReadIntervalSet("Intervals to practice, for example b3 3 5 b7, or blank for common");
        if (targetIntervals.Count == IntervalFunctionMapLibrary.IntervalLabels.Count)
        {
            targetIntervals = new HashSet<string>(["b3", "3", "4", "5", "6", "b7"]);
        }

        var keyRoot = ReadKeyCenter(progression[0].Root);

        var bpm = setup.Bpm ?? ReadInt("BPM", defaultValue: 80, min: 30, max: 240);
        var timeSignature = setup.TimeSignature ?? ReadTimeSignature();
        var chordLengths = setup.ChordLengths ?? ReadChordLengths(progression, timeSignature);
        var phraseAnchors = _triadGame.BuildPhrase(progression);
        var prompts = BuildIntervalPrompts(progression, targetIntervals);
        var beat = 0;
        var paused = false;
        var clickEnabled = true;
        var backingEnabled = true;
        var lastSynthChordIndex = -1;
        var lastRenderedChordIndex = -1;
        var phraseLength = chordLengths.Sum();
        var synthProcesses = new List<AudioPlayback>();
        WarmIntervalBackingChords(prompts, chordLengths, bpm, timeSignature);

        try
        {
            while (true)
            {
                var beatStartedAt = DateTime.UtcNow;
                var chordIndex = ChordIndexAtBeat(chordLengths, beat);
                var beatWithinChord = beat - StartBeatForChord(chordLengths, chordIndex);
                var beatWithinBar = beat % timeSignature.BeatsPerBar;

                if (!paused && backingEnabled && chordIndex != lastSynthChordIndex)
                {
                    CleanupFinishedProcesses(synthProcesses);
                    var synthProcess = PlayBackingChord(prompts[chordIndex].BackingChord, chordLengths[chordIndex], bpm, timeSignature);
                    if (synthProcess is not null)
                    {
                        synthProcesses.Add(synthProcess);
                    }

                    lastSynthChordIndex = chordIndex;
                }

                if (chordIndex != lastRenderedChordIndex)
                {
                    RenderIntervalSongGame(setup.Title, progression, prompts, phraseAnchors, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, keyRoot);
                    lastRenderedChordIndex = chordIndex;
                }

                if (!paused && clickEnabled)
                {
                    Click(beatWithinBar == 0);
                }

                var deadline = beatStartedAt + TimeSpan.FromMinutes(1d / bpm);
                while (DateTime.UtcNow < deadline)
                {
                    if (Console.KeyAvailable)
                    {
                        switch (Console.ReadKey(intercept: true).Key)
                        {
                            case ConsoleKey.Q:
                            case ConsoleKey.B:
                            case ConsoleKey.Escape:
                                return;
                            case ConsoleKey.Spacebar:
                                paused = !paused;
                                if (paused)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    lastSynthChordIndex = -1;
                                }
                                RenderIntervalSongGame(setup.Title, progression, prompts, phraseAnchors, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, keyRoot);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderIntervalSongGame(setup.Title, progression, prompts, phraseAnchors, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, keyRoot);
                                break;
                            case ConsoleKey.S:
                                backingEnabled = !backingEnabled;
                                if (!backingEnabled)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    lastSynthChordIndex = -1;
                                }
                                RenderIntervalSongGame(setup.Title, progression, prompts, phraseAnchors, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, keyRoot);
                                break;
                            case ConsoleKey.N:
                            case ConsoleKey.RightArrow:
                                beat = StartBeatForChord(chordLengths, chordIndex + 1);
                                goto BeatAdvancedManually;
                        }
                    }

                    Thread.Sleep(5);
                }

                if (!paused)
                {
                    beat++;
                }

            BeatAdvancedManually:
                if (beat >= phraseLength)
                {
                    prompts = BuildIntervalPrompts(progression, targetIntervals);
                    phraseAnchors = _triadGame.BuildPhrase(progression, phraseAnchors[^1]);
                    WarmIntervalBackingChords(prompts, chordLengths, bpm, timeSignature);
                    beat = 0;
                    lastSynthChordIndex = -1;
                    lastRenderedChordIndex = -1;
                }
            }
        }
        finally
        {
            StopProcesses(synthProcesses);
        }
    }

    private void ShowTriadInversions()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Triad inversions");

            var root = ReadMenuChoice("Choose a root", MusicTheory.NaturalRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            while (true)
            {
                var qualityChoice = ReadMenuChoice("Choose a quality", ["Major", "Minor"], allowBack: true);
                if (qualityChoice is null)
                {
                    break;
                }

                var quality = Enum.Parse<ChordQuality>(qualityChoice);

                while (true)
                {
                    Console.Clear();
                    WriteHeader($"{root} {quality} triad inversions");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, {Third}3{Reset} = third, {Fifth}5{Reset} = fifth, [common] = lower-position shape");
                    Console.WriteLine("B = back, Q = main menu");
                    Console.WriteLine();

                    foreach (var grouping in triads.GetTriadInversions(root, quality))
                    {
                        Console.WriteLine(grouping.Name);
                        Console.WriteLine(new string('-', grouping.Name.Length));

                        var diagrams = grouping.Shapes
                            .Select(shape => ($"{shape.InversionName} ({shape.MinFret}-{shape.MaxFret}){CommonTriadSuffix(shape)}", shape.Diagram))
                            .ToArray();

                        foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth()))
                        {
                            Console.WriteLine(line);
                        }

                        Console.WriteLine();
                    }

                    Console.Write("Command > ");
                    switch (Console.ReadKey(intercept: true).Key)
                    {
                        case ConsoleKey.B:
                        case ConsoleKey.Escape:
                            goto ChooseQuality;
                        case ConsoleKey.Q:
                            return;
                    }
                }

            ChooseQuality:
                continue;
            }
        }
    }

    private void ShowScalesMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Scales");
            Console.WriteLine("1. Shapes");
            Console.WriteLine("2. Song game");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowPentatonicShapes();
                    break;
                case "2":
                    ShowScaleSongGame();
                    break;
                case "B":
                case "b":
                    return;
                default:
                    Console.WriteLine("That choice is not on the menu.");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }

    private void ShowPentatonicShapes()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Scale shapes");

            var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            while (true)
            {
                var scaleName = ReadMenuChoice("Choose a scale", PentatonicLibrary.ScaleKinds.Select(PentatonicLibrary.NameFor).ToArray(), allowBack: true);
                if (scaleName is null)
                {
                    break;
                }

                var scaleKind = PentatonicLibrary.ScaleKinds.Single(kind => PentatonicLibrary.NameFor(kind) == scaleName);

                while (true)
                {
                    var shapes = pentatonics.GetShapes(root, scaleKind);

                    Console.Clear();
                    WriteHeader($"{root} {PentatonicLibrary.NameFor(scaleKind)} shapes");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals show scale degrees ({Pentatonic}2/4/6/7/flats{Reset}, {Third}3/b3{Reset}, {BlueNote}#4/b5{Reset}, {Fifth}5{Reset})");
                    Console.WriteLine("T = toggle major/minor, B = back, Q = main menu");
                    Console.WriteLine();

                    var diagrams = shapes
                        .Select(shape => ($"Shape {shape.Number} ({shape.MinFret}-{shape.MaxFret})", shape.Diagram))
                        .ToArray();

                    foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth()))
                    {
                        Console.WriteLine(line);
                    }

                    Console.WriteLine();
                    Console.Write("Command > ");

                    switch (Console.ReadKey(intercept: true).Key)
                    {
                        case ConsoleKey.T:
                            scaleKind = PentatonicLibrary.ToggleMajorMinor(scaleKind);
                            break;
                        case ConsoleKey.B:
                        case ConsoleKey.Escape:
                            goto ChooseScale;
                        case ConsoleKey.Q:
                            return;
                    }
                }

            ChooseScale:
                continue;
            }
        }
    }

    private static string? ReadMenuChoice(string prompt, IReadOnlyList<string> options, bool allowBack = false)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            for (var index = 0; index < options.Count; index++)
            {
                Console.WriteLine($"{index + 1}. {options[index]}");
            }
            if (allowBack)
            {
                Console.WriteLine("B. Back");
            }
            Console.Write("> ");

            var input = Console.ReadLine()?.Trim();
            if (allowBack && input is not null && input.Equals("B", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(input, out var optionNumber) && optionNumber >= 1 && optionNumber <= options.Count)
            {
                return options[optionNumber - 1];
            }

            if (input is not null && options.Any(option => option.Equals(input, StringComparison.OrdinalIgnoreCase)))
            {
                return options.Single(option => option.Equals(input, StringComparison.OrdinalIgnoreCase));
            }

            Console.WriteLine("That choice is not on the menu.");
            Console.WriteLine();
        }
    }

    private static int ReadInt(string prompt, int defaultValue, int min, int max)
    {
        while (true)
        {
            Console.Write($"{prompt} [{defaultValue}] > ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }

            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Enter a number from {min} to {max}.");
        }
    }

    private void RenderTriadProgressionGame(
        string gameTitle,
        string title,
        IReadOnlyList<TriadPracticeItem> currentPhrase,
        IReadOnlyList<TriadPracticeItem> nextPhrase,
        IReadOnlyList<int> chordLengths,
        int chordIndex,
        int beatWithinChord,
        int beatWithinBar,
        TimeSignature timeSignature,
        int bpm,
        bool clickEnabled,
        bool backingEnabled,
        bool voiceEnabled,
        bool paused)
    {
        Console.Clear();
        WriteHeader(gameTitle);
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  Voice: {(voiceEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine("Space = pause, M = mute click, S = mute backing, V = mute voice, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {currentPhrase[chordIndex].Chord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        var currentDiagrams = currentPhrase
            .Select((item, index) => ($"{(index == chordIndex ? "> " : "  ")}{item.Title} [{chordLengths[index]} beat{Pluralize(chordLengths[index])}]", item.Diagram))
            .ToArray();

        foreach (var line in _renderer.RenderMany(currentDiagrams, GetUsableConsoleWidth(), new HashSet<int> { chordIndex }))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedProgression(currentPhrase, chordIndex);
        Console.WriteLine();
        Console.WriteLine("Up next");
        var nextDiagrams = nextPhrase
            .Select((item, index) => ($"  {item.Title} [{chordLengths[index]} beat{Pluralize(chordLengths[index])}]", item.Diagram))
            .ToArray();

        foreach (var line in _renderer.RenderMany(nextDiagrams, GetUsableConsoleWidth()))
        {
            Console.WriteLine(line);
        }
    }

    private static void WriteCenteredHighlightedProgression(IReadOnlyList<TriadPracticeItem> phrase, int chordIndex)
    {
        const string label = "Progression: ";
        var progressionText = string.Join(" - ", phrase.Select(item => item.Chord.DisplayName));
        var textLength = label.Length + progressionText.Length;
        var padding = Math.Max(0, (GetUsableConsoleWidth() - textLength) / 2);

        Console.Write(new string(' ', padding));
        Console.Write(label);

        for (var index = 0; index < phrase.Count; index++)
        {
            if (index > 0)
            {
                Console.Write(" - ");
            }

            if (index == chordIndex)
            {
                Console.Write(Third);
                Console.Write(phrase[index].Chord.DisplayName);
                Console.Write(Reset);
            }
            else
            {
                Console.Write(phrase[index].Chord.DisplayName);
            }
        }
    }

    private void ShowScaleSongGame()
    {
        Console.Clear();
        WriteHeader("Scale song game");
        var keyRoot = ReadMenuChoice("Choose a key root", MusicTheory.ChromaticRoots, allowBack: true);
        if (keyRoot is null)
        {
            return;
        }

        var scaleName = ReadMenuChoice("Choose a scale", PentatonicLibrary.ScaleKinds.Select(PentatonicLibrary.NameFor).ToArray(), allowBack: true);
        if (scaleName is null)
        {
            return;
        }

        var scaleKind = PentatonicLibrary.ScaleKinds.Single(kind => PentatonicLibrary.NameFor(kind) == scaleName);
        var setup = ReadScaleSongSetup(keyRoot, scaleKind);
        if (setup is null)
        {
            return;
        }

        IReadOnlyList<ChordSymbol> progression;
        try
        {
            progression = _triadGame.ParseProgression(setup.ProgressionText);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
            Console.ReadKey(intercept: true);
            return;
        }

        var bpm = setup.Bpm ?? ReadInt("BPM", defaultValue: 80, min: 30, max: 240);
        var timeSignature = setup.TimeSignature ?? ReadTimeSignature();
        var chordLengths = setup.ChordLengths ?? ReadChordLengths(progression, timeSignature);
        var beat = 0;
        var paused = false;
        var clickEnabled = true;
        var backingEnabled = true;
        var lastSynthChordIndex = -1;
        var lastRenderedChordIndex = -1;
        var phraseLength = chordLengths.Sum();
        var synthProcesses = new List<AudioPlayback>();
        WarmBackingChords(progression, chordLengths, bpm, timeSignature);

        try
        {
            while (true)
            {
                var beatStartedAt = DateTime.UtcNow;
                var chordIndex = ChordIndexAtBeat(chordLengths, beat);
                var beatWithinChord = beat - StartBeatForChord(chordLengths, chordIndex);
                var beatWithinBar = beat % timeSignature.BeatsPerBar;

                if (!paused && backingEnabled && chordIndex != lastSynthChordIndex)
                {
                    CleanupFinishedProcesses(synthProcesses);
                    var synthProcess = PlayBackingChord(progression[chordIndex], chordLengths[chordIndex], bpm, timeSignature);
                    if (synthProcess is not null)
                    {
                        synthProcesses.Add(synthProcess);
                    }

                    lastSynthChordIndex = chordIndex;
                }

                if (chordIndex != lastRenderedChordIndex)
                {
                    RenderScaleSongGame(setup.Title, keyRoot, scaleKind, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                    lastRenderedChordIndex = chordIndex;
                }

                if (!paused && clickEnabled)
                {
                    Click(beatWithinBar == 0);
                }

                var deadline = beatStartedAt + TimeSpan.FromMinutes(1d / bpm);
                while (DateTime.UtcNow < deadline)
                {
                    if (Console.KeyAvailable)
                    {
                        switch (Console.ReadKey(intercept: true).Key)
                        {
                            case ConsoleKey.Q:
                            case ConsoleKey.B:
                            case ConsoleKey.Escape:
                                return;
                            case ConsoleKey.Spacebar:
                                paused = !paused;
                                if (paused)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    lastSynthChordIndex = -1;
                                }
                                RenderScaleSongGame(setup.Title, keyRoot, scaleKind, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderScaleSongGame(setup.Title, keyRoot, scaleKind, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.S:
                                backingEnabled = !backingEnabled;
                                if (!backingEnabled)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    lastSynthChordIndex = -1;
                                }
                                RenderScaleSongGame(setup.Title, keyRoot, scaleKind, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.N:
                            case ConsoleKey.RightArrow:
                                beat = StartBeatForChord(chordLengths, chordIndex + 1);
                                goto BeatAdvancedManually;
                        }
                    }

                    Thread.Sleep(5);
                }

                if (!paused)
                {
                    beat++;
                }

            BeatAdvancedManually:
                if (beat >= phraseLength)
                {
                    beat = 0;
                    lastSynthChordIndex = -1;
                    lastRenderedChordIndex = -1;
                }
            }
        }
        finally
        {
            StopProcesses(synthProcesses);
        }
    }

    private void RenderIntervalSongGame(
        string title,
        IReadOnlyList<ChordSymbol> progression,
        IReadOnlyList<IntervalPrompt> prompts,
        IReadOnlyList<TriadPracticeItem> phraseAnchors,
        IReadOnlyList<int> chordLengths,
        int chordIndex,
        int beatWithinChord,
        int beatWithinBar,
        TimeSignature timeSignature,
        int bpm,
        bool clickEnabled,
        bool backingEnabled,
        bool paused,
        string keyRoot)
    {
        var prompt = prompts[chordIndex];
        var diagram = intervalMaps.BuildLookup(prompt.Chord.Root, anchorFret: 6, new HashSet<string> { prompt.Interval }, length: 13);
        var romanNumerals = progression.Select(chord => RomanNumeral(chord, keyRoot)).ToArray();

        Console.Clear();
        WriteHeader("Interval song game");
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"Key: {keyRoot} major  Current function: {Third}{romanNumerals[chordIndex]}{Reset}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine("Space = pause, M = mute click, S = mute backing, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {prompt.Chord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        Console.WriteLine($"Target: {Third}{prompt.Interval}{Reset} ({IntervalFunctionMapLibrary.NameForInterval(prompt.Interval)}) from {prompt.Chord.Root}");
        Console.WriteLine($"Backing: {prompt.BackingChord.DisplayName}");
        Console.WriteLine();

        foreach (var line in _renderer.Render(diagram))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedProgression(progression, chordIndex);
        Console.WriteLine();
        WriteCenteredHighlightedRomanProgression(romanNumerals, chordIndex);
    }

    private void RenderScaleSongGame(
        string title,
        string keyRoot,
        PentatonicScaleKind scaleKind,
        IReadOnlyList<ChordSymbol> progression,
        IReadOnlyList<int> chordLengths,
        int chordIndex,
        int beatWithinChord,
        int beatWithinBar,
        TimeSignature timeSignature,
        int bpm,
        bool clickEnabled,
        bool backingEnabled,
        bool paused)
    {
        var currentChord = progression[chordIndex];
        var diagram = pentatonics.BuildScaleWindow(keyRoot, scaleKind, startFret: 0, length: 25, currentChord);
        var romanNumerals = progression.Select(chord => RomanNumeral(chord, keyRoot)).ToArray();

        Console.Clear();
        WriteHeader("Scale song game");
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"Key: {keyRoot}  Scale: {PentatonicLibrary.NameFor(scaleKind)}  Current function: {Third}{romanNumerals[chordIndex]}{Reset}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine("Space = pause, M = mute click, S = mute backing, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {currentChord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        Console.WriteLine($"Use the scale notes. Land on current chord tones: {Root}R{Reset}, {Third}3/b3{Reset}, {Fifth}5{Reset}");
        Console.WriteLine();

        foreach (var line in _renderer.Render(diagram))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedProgression(progression, chordIndex);
        Console.WriteLine();
        WriteCenteredHighlightedRomanProgression(romanNumerals, chordIndex);
    }

    private static void WriteCenteredHighlightedProgression(IReadOnlyList<ChordSymbol> progression, int chordIndex)
    {
        const string label = "Progression: ";
        var progressionText = string.Join(" - ", progression.Select(chord => chord.DisplayName));
        var textLength = label.Length + progressionText.Length;
        var padding = Math.Max(0, (GetUsableConsoleWidth() - textLength) / 2);

        Console.Write(new string(' ', padding));
        Console.Write(label);

        for (var index = 0; index < progression.Count; index++)
        {
            if (index > 0)
            {
                Console.Write(" - ");
            }

            if (index == chordIndex)
            {
                Console.Write(Third);
                Console.Write(progression[index].DisplayName);
                Console.Write(Reset);
            }
            else
            {
                Console.Write(progression[index].DisplayName);
            }
        }
    }

    private static void WriteCenteredHighlightedRomanProgression(IReadOnlyList<string> romanNumerals, int chordIndex)
    {
        const string label = "Functions: ";
        var progressionText = string.Join(" - ", romanNumerals);
        var textLength = label.Length + progressionText.Length;
        var padding = Math.Max(0, (GetUsableConsoleWidth() - textLength) / 2);

        Console.Write(new string(' ', padding));
        Console.Write(label);

        for (var index = 0; index < romanNumerals.Count; index++)
        {
            if (index > 0)
            {
                Console.Write(" - ");
            }

            if (index == chordIndex)
            {
                Console.Write(Third);
                Console.Write(romanNumerals[index]);
                Console.Write(Reset);
            }
            else
            {
                Console.Write(romanNumerals[index]);
            }
        }
    }

    private static string RomanNumeral(ChordSymbol chord, string keyRoot)
    {
        var interval = MusicTheory.Normalize(MusicTheory.PitchClassFor(chord.Root) - MusicTheory.PitchClassFor(keyRoot));
        var numeral = interval switch
        {
            0 => "I",
            1 => "bII",
            2 => "II",
            3 => "bIII",
            4 => "III",
            5 => "IV",
            6 => "bV",
            7 => "V",
            8 => "bVI",
            9 => "VI",
            10 => "bVII",
            11 => "VII",
            _ => "?"
        };

        return chord.Quality == ChordQuality.Minor ? numeral.ToLowerInvariant() : numeral;
    }

    private static string ReadKeyCenter(string defaultRoot)
    {
        while (true)
        {
            Console.Write($"Key center [{defaultRoot}] > ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultRoot;
            }

            var normalized = NormalizeRootInput(input);
            if (MusicTheory.ChromaticRoots.Contains(normalized))
            {
                return normalized;
            }

            Console.WriteLine($"Use a root: {string.Join(" ", MusicTheory.ChromaticRoots)}");
        }
    }

    private static string NormalizeRootInput(string input)
    {
        var trimmed = input.Trim();
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

    private static bool IsMinorScale(PentatonicScaleKind scaleKind) => scaleKind is
        PentatonicScaleKind.MinorPentatonic or
        PentatonicScaleKind.MinorBlues or
        PentatonicScaleKind.NaturalMinor or
        PentatonicScaleKind.Dorian or
        PentatonicScaleKind.Phrygian or
        PentatonicScaleKind.Locrian or
        PentatonicScaleKind.HarmonicMinor or
        PentatonicScaleKind.MelodicMinor;

    private static IReadOnlyList<IntervalPrompt> BuildIntervalPrompts(IReadOnlyList<ChordSymbol> progression, IReadOnlySet<string> targetIntervals)
    {
        var intervals = targetIntervals.Where(interval => interval != "R").ToArray();
        if (intervals.Length == 0)
        {
            intervals = ["3", "5", "b7"];
        }

        return progression
            .Select(chord =>
            {
                var interval = intervals[Random.Shared.Next(intervals.Length)];
                return new IntervalPrompt(chord, interval, BuildIntervalBackingChord(chord, interval));
            })
            .ToArray();
    }

    private static BackingChord BuildIntervalBackingChord(ChordSymbol chord, string interval)
    {
        var quality = interval switch
        {
            "b3" => ChordQuality.Minor,
            "3" => ChordQuality.Major,
            _ => chord.Quality
        };

        var chordIntervals = quality == ChordQuality.Minor
            ? new HashSet<string>(["R", "b3", "5"])
            : new HashSet<string>(["R", "3", "5"]);

        chordIntervals.Add(interval);

        var suffix = interval switch
        {
            "b2" => "b9",
            "2" => "add9",
            "4" => "sus4",
            "b5" => "b5",
            "b6" => "b13",
            "6" => "6",
            "b7" => "7",
            "7" => "maj7",
            _ => quality == ChordQuality.Minor ? "m" : string.Empty
        };

        return new BackingChord(chord.Root, quality, chordIntervals, suffix);
    }

    private static HashSet<string> ReadIntervalSet(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} > ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                return IntervalFunctionMapLibrary.IntervalLabels.ToHashSet();
            }

            var labels = input
                .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var normalized = labels
                .Select(label => IntervalFunctionMapLibrary.IntervalLabels.SingleOrDefault(interval => interval.Equals(label, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (normalized.All(label => label is not null))
            {
                return normalized.Cast<string>().ToHashSet();
            }

            Console.WriteLine($"Use interval labels: {string.Join(" ", IntervalFunctionMapLibrary.IntervalLabels)}");
        }
    }

    private static int ChordIndexAtBeat(IReadOnlyList<int> chordLengths, int beat)
    {
        var start = 0;
        for (var index = 0; index < chordLengths.Count; index++)
        {
            start += chordLengths[index];
            if (beat < start)
            {
                return index;
            }
        }

        return chordLengths.Count - 1;
    }

    private static int StartBeatForChord(IReadOnlyList<int> chordLengths, int chordIndex)
    {
        var clampedIndex = Math.Clamp(chordIndex, 0, chordLengths.Count);
        var start = 0;
        for (var index = 0; index < clampedIndex; index++)
        {
            start += chordLengths[index];
        }

        return start;
    }

    private static string Pluralize(int count) => count == 1 ? string.Empty : "s";

    private sealed record TriadProgressionSetup(
        string Title,
        string ProgressionText,
        int? Bpm = null,
        TimeSignature? TimeSignature = null,
        IReadOnlyList<int>? ChordLengths = null);

    private sealed record IntervalPrompt(ChordSymbol Chord, string Interval, BackingChord BackingChord);

    private sealed record BackingChord(
        string Root,
        ChordQuality Quality,
        IReadOnlySet<string> Intervals,
        string Suffix)
    {
        public string DisplayName => $"{Root}{Suffix}";
    }

    private static void Click(bool accent)
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                Console.Beep(accent ? 1200 : 900, 25);
            }
            else if (OperatingSystem.IsMacOS())
            {
                PlayMacClick(accent);
            }
            else
            {
                Console.Write('\a');
            }
        }
        catch
        {
            Console.Write('\a');
        }
    }

    private static void PlayMacClick(bool accent)
    {
        var sound = accent
            ? "/System/Library/Sounds/Pop.aiff"
            : "/System/Library/Sounds/Tink.aiff";

        Process.Start(new ProcessStartInfo
        {
            FileName = "afplay",
            UseShellExecute = false,
            CreateNoWindow = true,
            ArgumentList = { sound }
        });
    }

    private static AudioPlayback? PlayBackingChord(ChordSymbol chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        return PlayBackingChord(BackingChordFor(chord), beatsPerChord, bpm, timeSignature);
    }

    private static AudioPlayback? PlayBackingChord(BackingChord chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        var path = EnsureBackingChordFile(chord, beatsPerChord, bpm, timeSignature);

        try
        {
            if (OperatingSystem.IsMacOS())
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "afplay",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ArgumentList = { path }
                });

                return process is null ? null : new MacAudioPlayback(process);
            }

            if (OperatingSystem.IsWindows())
            {
                return WindowsAudioPlayback.Play(path);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static void WarmBackingChords(IReadOnlyList<TriadPracticeItem> phrase, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < phrase.Count; index++)
        {
            EnsureBackingChordFile(BackingChordFor(phrase[index].Chord), chordLengths[index], bpm, timeSignature);
        }
    }

    private static void WarmBackingChords(IReadOnlyList<ChordSymbol> progression, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < progression.Count; index++)
        {
            EnsureBackingChordFile(BackingChordFor(progression[index]), chordLengths[index], bpm, timeSignature);
        }
    }

    private static void WarmIntervalBackingChords(IReadOnlyList<IntervalPrompt> prompts, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < prompts.Count; index++)
        {
            EnsureBackingChordFile(prompts[index].BackingChord, chordLengths[index], bpm, timeSignature);
        }
    }

    private static string EnsureBackingChordFile(BackingChord chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        var durationSeconds = beatsPerChord * 60d / bpm;
        var overlapSeconds = Math.Clamp(durationSeconds * 0.38, 0.32, 0.8);
        var path = BackingChordFilePath(chord, durationSeconds, overlapSeconds, bpm, timeSignature);
        if (!File.Exists(path))
        {
            WriteBackingChordWav(path, chord, beatsPerChord, bpm, timeSignature, durationSeconds, overlapSeconds);
        }

        return path;
    }

    private static string BackingChordFilePath(BackingChord chord, double durationSeconds, double overlapSeconds, int bpm, TimeSignature timeSignature)
    {
        var directory = Path.Combine(Path.GetTempPath(), "guitar-tui-backing");
        Directory.CreateDirectory(directory);

        var durationKey = Math.Round(durationSeconds, 3).ToString("0.000").Replace('.', '-');
        var overlapKey = Math.Round(overlapSeconds, 3).ToString("0.000").Replace('.', '-');
        var intervalKey = string.Join("-", chord.Intervals.OrderBy(LabelSortOrderForBacking)).Replace("#", "s", StringComparison.Ordinal);
        return Path.Combine(directory, $"{chord.Root.Replace('#', 's')}-{chord.Quality}-{chord.Suffix}-{intervalKey}-{bpm}-{timeSignature.DisplayName.Replace('/', '-')}-{durationKey}-{overlapKey}-v7.wav");
    }

    private static void WriteBackingChordWav(
        string path,
        BackingChord chord,
        int beatsPerChord,
        int bpm,
        TimeSignature timeSignature,
        double durationSeconds,
        double overlapSeconds)
    {
        const int sampleRate = 44100;
        const short channels = 1;
        const short bitsPerSample = 16;
        var totalDurationSeconds = durationSeconds + overlapSeconds;
        var samples = Math.Max(1, (int)(sampleRate * totalDurationSeconds));
        var dataSize = samples * channels * bitsPerSample / 8;
        var guitarFrequencies = GuitarChordFrequencies(chord).ToArray();
        var bassFrequency = BassFrequency(chord);
        var secondsPerBeat = 60d / bpm;

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.ASCII);

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);

        for (var sample = 0; sample < samples; sample++)
        {
            var time = sample / (double)sampleRate;
            var beatPosition = time / secondsPerBeat;
            var barBeat = (int)Math.Floor(beatPosition) % timeSignature.BeatsPerBar;
            var value = 0d;

            for (var beat = 0; beat < beatsPerChord; beat++)
            {
                var beatStart = beat * secondsPerBeat;
                var beatOffset = time - beatStart;
                if (beatOffset < 0 || beatOffset > secondsPerBeat)
                {
                    continue;
                }

                var accent = beat % timeSignature.BeatsPerBar == 0 ? 1.15 : 0.82;
                value += DrumKit(beatOffset, barBeat, timeSignature.BeatsPerBar) * 0.32;
                value += BassNote(bassFrequency, beatOffset) * (beat % timeSignature.BeatsPerBar == 0 ? 0.34 : 0.2);

                foreach (var (frequency, stringIndex) in guitarFrequencies.Select((frequency, index) => (frequency, index)))
                {
                    var stringDelay = stringIndex * 0.012;
                    value += GuitarString(frequency, beatOffset - stringDelay) * accent * 0.18;
                }
            }

            value += ChordWash(guitarFrequencies, time, totalDurationSeconds, durationSeconds) * 0.18;
            value = Math.Tanh(value) * 0.75d;
            writer.Write((short)(value * short.MaxValue));
        }
    }

    private static BackingChord BackingChordFor(ChordSymbol chord)
    {
        var intervals = chord.Quality == ChordQuality.Minor
            ? new HashSet<string>(["R", "b3", "5"])
            : new HashSet<string>(["R", "3", "5"]);

        return new BackingChord(chord.Root, chord.Quality, intervals, chord.Quality == ChordQuality.Minor ? "m" : string.Empty);
    }

    private static IEnumerable<double> GuitarChordFrequencies(ChordSymbol chord)
    {
        return GuitarChordFrequencies(BackingChordFor(chord));
    }

    private static IEnumerable<double> GuitarChordFrequencies(BackingChord chord)
    {
        var rootPitch = MusicTheory.PitchClassFor(chord.Root);
        var midiRoot = 48 + rootPitch;

        while (midiRoot > 64)
        {
            midiRoot -= 12;
        }

        var intervals = chord.Intervals
            .Select(IntervalSemitones)
            .Distinct()
            .Order()
            .ToArray();

        return intervals
            .Concat(intervals.Select(interval => interval + 12))
            .Take(6)
            .Select(interval => MidiToFrequency(midiRoot + interval));
    }

    private static double BassFrequency(BackingChord chord)
    {
        var midiRoot = 36 + MusicTheory.PitchClassFor(chord.Root);
        while (midiRoot > 47)
        {
            midiRoot -= 12;
        }

        return MidiToFrequency(midiRoot);
    }

    private static int IntervalSemitones(string interval) => interval switch
    {
        "R" => 0,
        "b2" => 1,
        "2" => 2,
        "b3" => 3,
        "3" => 4,
        "4" => 5,
        "b5" => 6,
        "5" => 7,
        "b6" => 8,
        "6" => 9,
        "b7" => 10,
        "7" => 11,
        _ => 0
    };

    private static int LabelSortOrderForBacking(string interval) => IntervalSemitones(interval);

    private static double GuitarString(double frequency, double time)
    {
        if (time < 0)
        {
            return 0;
        }

        var envelope = Math.Exp(-time * 2.6) * Math.Min(1, time / 0.008);
        var shimmer = Math.Sin(2d * Math.PI * frequency * time)
            + 0.45d * Math.Sin(2d * Math.PI * frequency * 2d * time)
            + 0.18d * Math.Sin(2d * Math.PI * frequency * 3d * time);

        return Math.Tanh(shimmer * 0.9) * envelope;
    }

    private static double BassNote(double frequency, double time)
    {
        if (time < 0)
        {
            return 0;
        }

        var envelope = Math.Exp(-time * 2.4) * Math.Min(1, time / 0.014);
        return Math.Sin(2d * Math.PI * frequency * time) * envelope;
    }

    private static double DrumKit(double time, int barBeat, int beatsPerBar)
    {
        var kick = Kick(time) * (barBeat == 0 ? 1.2 : 0.55);
        var snare = (beatsPerBar == 3 ? barBeat == 2 : barBeat == 1 || barBeat == 3) ? Snare(time) : 0d;
        var hat = HiHat(time) * 0.45;
        return kick + snare + hat;
    }

    private static double Kick(double time)
    {
        if (time < 0 || time > 0.16)
        {
            return 0;
        }

        var frequency = 90d - 42d * (time / 0.16);
        return Math.Sin(2d * Math.PI * frequency * time) * Math.Exp(-time * 23d);
    }

    private static double Snare(double time)
    {
        if (time < 0 || time > 0.12)
        {
            return 0;
        }

        var noise = Math.Sin((time * 44100d + 17d) * 12.9898d) * 43758.5453d;
        noise -= Math.Floor(noise);
        return (noise * 2d - 1d) * Math.Exp(-time * 28d);
    }

    private static double HiHat(double time)
    {
        if (time < 0 || time > 0.055)
        {
            return 0;
        }

        var noise = Math.Sin((time * 44100d + 91d) * 78.233d) * 12345.6789d;
        noise -= Math.Floor(noise);
        return (noise * 2d - 1d) * Math.Exp(-time * 70d);
    }

    private static double ChordWash(IReadOnlyList<double> frequencies, double time, double totalDuration, double releaseStart)
    {
        var envelope = Envelope(time, totalDuration, releaseStart);
        var value = 0d;

        foreach (var frequency in frequencies)
        {
            value += Math.Sin(2d * Math.PI * frequency * time);
        }

        return value / frequencies.Count * envelope;
    }

    private static double Envelope(double time, double totalDuration, double releaseStart)
    {
        var attack = Math.Min(0.035, totalDuration * 0.08);
        var release = Math.Max(0.05, totalDuration - releaseStart);

        if (time < attack)
        {
            return time / attack;
        }

        if (time > releaseStart)
        {
            return Math.Max(0, (totalDuration - time) / release);
        }

        return 1;
    }

    private static double MidiToFrequency(int midiNote) => 440d * Math.Pow(2d, (midiNote - 69) / 12d);

    private sealed class VoiceAnnouncer : IDisposable
    {
        private dynamic? _windowsVoice;
        private Process? _macVoiceProcess;

        public void SayChord(ChordSymbol chord)
        {
            var text = SpokenChordName(chord);

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    SayWindows(text);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    SayMac(text);
                }
            }
            catch
            {
                // Spoken hints are optional; the game should keep flowing if TTS is unavailable.
            }
        }

        public void Stop()
        {
            try
            {
                if (_windowsVoice is not null)
                {
                    _windowsVoice.Speak(string.Empty, 2);
                }
            }
            catch
            {
                // Best effort.
            }

            try
            {
                if (_macVoiceProcess is { HasExited: false })
                {
                    _macVoiceProcess.Kill();
                }
            }
            catch
            {
                // Best effort.
            }
        }

        public void Dispose()
        {
            Stop();

            if (_windowsVoice is null)
            {
                return;
            }

            try
            {
                Marshal.FinalReleaseComObject(_windowsVoice);
            }
            catch
            {
                // Best effort.
            }

            _windowsVoice = null;
        }

        [SupportedOSPlatform("windows")]
        private void SayWindows(string text)
        {
            _windowsVoice ??= CreateWindowsVoice();
            if (_windowsVoice is null)
            {
                return;
            }

            _windowsVoice.Volume = 70;
            _windowsVoice.Rate = 1;
            _windowsVoice.Speak(text, 3);
        }

        [SupportedOSPlatform("windows")]
        private static dynamic? CreateWindowsVoice()
        {
            var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
            return voiceType is null ? null : Activator.CreateInstance(voiceType);
        }

        private void SayMac(string text)
        {
            Stop();
            _macVoiceProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "say",
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList = { "-r", "210", $"[[volm 0.7]] {text}" }
            });
        }

        private static string SpokenChordName(ChordSymbol chord)
        {
            var root = chord.Root.Replace("#", " sharp ", StringComparison.Ordinal).Trim();
            return chord.Quality == ChordQuality.Minor ? $"{root} minor" : root;
        }
    }

    private abstract class AudioPlayback : IDisposable
    {
        public abstract bool HasFinished { get; }

        public abstract void Stop();

        public abstract void Dispose();
    }

    private sealed class MacAudioPlayback(Process process) : AudioPlayback
    {
        public override bool HasFinished
        {
            get
            {
                try
                {
                    return process.HasExited;
                }
                catch
                {
                    return true;
                }
            }
        }

        public override void Stop()
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
                // Best effort: a finished audio process is harmless.
            }
        }

        public override void Dispose() => process.Dispose();
    }

    private sealed class WindowsAudioPlayback : AudioPlayback
    {
        private const uint WaveMapper = uint.MaxValue;
        private const uint WhdrDone = 0x00000001;
        private readonly byte[] _audioData;
        private readonly GCHandle _audioHandle;
        private readonly IntPtr _headerPointer;
        private IntPtr _waveOut;
        private bool _closed;

        private WindowsAudioPlayback(IntPtr waveOut, byte[] audioData, GCHandle audioHandle, IntPtr headerPointer)
        {
            _waveOut = waveOut;
            _audioData = audioData;
            _audioHandle = audioHandle;
            _headerPointer = headerPointer;
        }

        public static WindowsAudioPlayback? Play(string path)
        {
            const int wavHeaderBytes = 44;
            var fileBytes = File.ReadAllBytes(path);
            if (fileBytes.Length <= wavHeaderBytes)
            {
                return null;
            }

            var audioData = fileBytes[wavHeaderBytes..];
            var audioHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);
            var headerPointer = IntPtr.Zero;
            var waveOut = IntPtr.Zero;
            var started = false;

            try
            {
                var format = new WaveFormat
                {
                    FormatTag = 1,
                    Channels = 1,
                    SamplesPerSec = 44100,
                    AvgBytesPerSec = 44100 * 2,
                    BlockAlign = 2,
                    BitsPerSample = 16,
                    Size = 0
                };

                if (waveOutOpen(out waveOut, WaveMapper, ref format, IntPtr.Zero, IntPtr.Zero, 0) != 0)
                {
                    return null;
                }

                var header = new WaveHeader
                {
                    Data = audioHandle.AddrOfPinnedObject(),
                    BufferLength = (uint)audioData.Length
                };

                var headerSize = Marshal.SizeOf<WaveHeader>();
                headerPointer = Marshal.AllocHGlobal(headerSize);
                Marshal.StructureToPtr(header, headerPointer, false);

                if (waveOutPrepareHeader(waveOut, headerPointer, (uint)headerSize) != 0)
                {
                    return null;
                }

                if (waveOutWrite(waveOut, headerPointer, (uint)headerSize) != 0)
                {
                    return null;
                }

                started = true;
                return new WindowsAudioPlayback(waveOut, audioData, audioHandle, headerPointer);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (!started)
                {
                    if (headerPointer != IntPtr.Zero)
                    {
                        if (waveOut != IntPtr.Zero)
                        {
                            waveOutUnprepareHeader(waveOut, headerPointer, (uint)Marshal.SizeOf<WaveHeader>());
                        }

                        Marshal.FreeHGlobal(headerPointer);
                    }

                    if (waveOut != IntPtr.Zero)
                    {
                        waveOutClose(waveOut);
                    }

                    if (audioHandle.IsAllocated)
                    {
                        audioHandle.Free();
                    }
                }
            }
        }

        public override bool HasFinished
        {
            get
            {
                if (_closed)
                {
                    return true;
                }

                var header = Marshal.PtrToStructure<WaveHeader>(_headerPointer);
                return (header.Flags & WhdrDone) == WhdrDone;
            }
        }

        public override void Stop() => Dispose();

        public override void Dispose()
        {
            if (_closed)
            {
                return;
            }

            if (_waveOut != IntPtr.Zero)
            {
                waveOutReset(_waveOut);
                waveOutUnprepareHeader(_waveOut, _headerPointer, (uint)Marshal.SizeOf<WaveHeader>());
                waveOutClose(_waveOut);
                _waveOut = IntPtr.Zero;
            }

            Marshal.FreeHGlobal(_headerPointer);
            _audioHandle.Free();
            GC.KeepAlive(_audioData);
            _closed = true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WaveFormat
        {
            public ushort FormatTag;
            public ushort Channels;
            public uint SamplesPerSec;
            public uint AvgBytesPerSec;
            public ushort BlockAlign;
            public ushort BitsPerSample;
            public ushort Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WaveHeader
        {
            public IntPtr Data;
            public uint BufferLength;
            public uint BytesRecorded;
            public IntPtr User;
            public uint Flags;
            public uint Loops;
            public IntPtr Next;
            public IntPtr Reserved;
        }

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutOpen(out IntPtr waveOut, uint deviceId, ref WaveFormat format, IntPtr callback, IntPtr instance, uint flags);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutPrepareHeader(IntPtr waveOut, IntPtr waveHeader, uint waveHeaderSize);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutWrite(IntPtr waveOut, IntPtr waveHeader, uint waveHeaderSize);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutReset(IntPtr waveOut);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutUnprepareHeader(IntPtr waveOut, IntPtr waveHeader, uint waveHeaderSize);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutClose(IntPtr waveOut);
    }

    private static void StopProcesses(List<AudioPlayback> processes)
    {
        foreach (var process in processes)
        {
            process.Stop();
            process.Dispose();
        }

        processes.Clear();
    }

    private static void CleanupFinishedProcesses(List<AudioPlayback> processes)
    {
        processes.RemoveAll(process =>
        {
            try
            {
                if (!process.HasFinished)
                {
                    return false;
                }

                process.Dispose();
                return true;
            }
            catch
            {
                return true;
            }
        });
    }

    private static string CommonTriadSuffix(TriadShape shape)
    {
        return shape.MinFret > 0 && shape.MaxFret <= 8 ? " [common]" : string.Empty;
    }

    private static void WriteHeader(string title)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
        Console.WriteLine();
    }

    private static int GetUsableConsoleWidth()
    {
        if (Console.IsOutputRedirected)
        {
            return 120;
        }

        return Math.Max(40, Console.WindowWidth - 1);
    }
}
