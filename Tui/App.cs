using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.IntervalMaps;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Triads;
using System.Diagnostics;
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

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Guitar Resources");
            Console.WriteLine("1. Triads");
            Console.WriteLine("2. Scale shapes");
            Console.WriteLine("3. Interval function map");
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowTriadsMenu();
                    break;
                case "2":
                    ShowPentatonicShapes();
                    break;
                case "3":
                    ShowIntervalFunctionMap();
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
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowTriadInversions();
                    break;
                case "2":
                    ShowTriadProgressionGame();
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

    private void ShowTriadProgressionGame()
    {
        Console.Clear();
        WriteHeader("Triad music game");
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

        var bpm = setup.Bpm ?? ReadInt("BPM", defaultValue: 80, min: 30, max: 240);
        var timeSignature = setup.TimeSignature ?? ReadTimeSignature();
        var chordLengths = setup.ChordLengths ?? ReadChordLengths(progression, timeSignature);

        var clickEnabled = true;
        var backingEnabled = true;

        var currentPhrase = _triadGame.BuildPhrase(progression);
        var nextPhrase = _triadGame.BuildPhrase(progression, currentPhrase[^1]);
        var beat = 0;
        var paused = false;
        var lastSynthChordIndex = -1;
        var phraseLength = chordLengths.Sum();
        var synthProcesses = new List<Process>();

        try
        {
            while (true)
            {
                var chordIndex = ChordIndexAtBeat(chordLengths, beat);
                var beatWithinChord = beat - StartBeatForChord(chordLengths, chordIndex);
                var beatWithinBar = beat % timeSignature.BeatsPerBar;

                RenderTriadProgressionGame(setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);

                if (!paused && backingEnabled && chordIndex != lastSynthChordIndex)
                {
                    CleanupFinishedProcesses(synthProcesses);
                    var synthProcess = PlayBackingChord(currentPhrase[chordIndex].Chord, chordLengths[chordIndex], bpm, timeSignature);
                    if (synthProcess is not null)
                    {
                        synthProcesses.Add(synthProcess);
                    }
                    lastSynthChordIndex = chordIndex;
                }

                if (!paused && clickEnabled)
                {
                    Click(beatWithinBar == 0);
                }

                var interval = TimeSpan.FromMinutes(1d / bpm);
                var deadline = DateTime.UtcNow + interval;

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
                                RenderTriadProgressionGame(setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderTriadProgressionGame(setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
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
                                RenderTriadProgressionGame(setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.N:
                            case ConsoleKey.RightArrow:
                                beat = StartBeatForChord(chordLengths, chordIndex + 1);
                                goto BeatAdvancedManually;
                        }
                    }

                    Thread.Sleep(25);
                }

                if (!paused)
                {
                    beat++;
                }

            BeatAdvancedManually:
                if (beat >= phraseLength)
                {
                    currentPhrase = nextPhrase;
                    nextPhrase = _triadGame.BuildPhrase(progression, currentPhrase[^1]);
                    beat = 0;
                    lastSynthChordIndex = -1;
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

    private void ShowIntervalFunctionMap()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Interval function map");

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

                while (true)
                {
                    var diagram = intervalMaps.BuildMap(root, anchorFret);

                    Console.Clear();
                    WriteHeader($"{root} interval function map ({diagram.StartFret}-{diagram.StartFret + diagram.Length - 1})");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals are relative to {root}");
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
        bool paused)
    {
        Console.Clear();
        WriteHeader("Triad music game");
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"Progression: {string.Join(" - ", currentPhrase.Select(item => item.Chord.DisplayName))}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine("Space = pause, M = mute click, S = mute backing, N = next chord, B/Q = main menu");
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
        Console.WriteLine("Up next");
        var nextDiagrams = nextPhrase
            .Select((item, index) => ($"  {item.Title} [{chordLengths[index]} beat{Pluralize(chordLengths[index])}]", item.Diagram))
            .ToArray();

        foreach (var line in _renderer.RenderMany(nextDiagrams, GetUsableConsoleWidth()))
        {
            Console.WriteLine(line);
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

    private static Process? PlayBackingChord(ChordSymbol chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var durationSeconds = beatsPerChord * 60d / bpm;
        var overlapSeconds = Math.Clamp(durationSeconds * 0.22, 0.18, 0.45);
        var path = BackingChordFilePath(chord, durationSeconds, overlapSeconds, bpm, timeSignature);
        if (!File.Exists(path))
        {
            WriteBackingChordWav(path, chord, beatsPerChord, bpm, timeSignature, durationSeconds, overlapSeconds);
        }

        try
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "afplay",
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList = { path }
            });
        }
        catch
        {
            return null;
        }
    }

    private static string BackingChordFilePath(ChordSymbol chord, double durationSeconds, double overlapSeconds, int bpm, TimeSignature timeSignature)
    {
        var directory = Path.Combine(Path.GetTempPath(), "guitar-tui-backing");
        Directory.CreateDirectory(directory);

        var durationKey = Math.Round(durationSeconds, 3).ToString("0.000").Replace('.', '-');
        var overlapKey = Math.Round(overlapSeconds, 3).ToString("0.000").Replace('.', '-');
        return Path.Combine(directory, $"{chord.Root.Replace('#', 's')}-{chord.Quality}-{bpm}-{timeSignature.DisplayName.Replace('/', '-')}-{durationKey}-{overlapKey}-v4.wav");
    }

    private static void WriteBackingChordWav(
        string path,
        ChordSymbol chord,
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

            value += ChordWash(guitarFrequencies, time, totalDurationSeconds, durationSeconds) * 0.08;
            value = Math.Tanh(value) * 0.75d;
            writer.Write((short)(value * short.MaxValue));
        }
    }

    private static IEnumerable<double> GuitarChordFrequencies(ChordSymbol chord)
    {
        var rootPitch = MusicTheory.PitchClassFor(chord.Root);
        var thirdInterval = chord.Quality == ChordQuality.Major ? 4 : 3;
        var midiRoot = 52 + rootPitch;

        while (midiRoot > 64)
        {
            midiRoot -= 12;
        }

        return
        [
            MidiToFrequency(midiRoot),
            MidiToFrequency(midiRoot + thirdInterval),
            MidiToFrequency(midiRoot + 7),
            MidiToFrequency(midiRoot + 12),
            MidiToFrequency(midiRoot + thirdInterval + 12),
            MidiToFrequency(midiRoot + 19)
        ];
    }

    private static double BassFrequency(ChordSymbol chord)
    {
        var midiRoot = 36 + MusicTheory.PitchClassFor(chord.Root);
        while (midiRoot > 47)
        {
            midiRoot -= 12;
        }

        return MidiToFrequency(midiRoot);
    }

    private static double GuitarString(double frequency, double time)
    {
        if (time < 0)
        {
            return 0;
        }

        var envelope = Math.Exp(-time * 5.2) * Math.Min(1, time / 0.01);
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

        var envelope = Math.Exp(-time * 3.5) * Math.Min(1, time / 0.018);
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

    private static void StopProcess(Process? process)
    {
        try
        {
            if (process is { HasExited: false })
            {
                process.Kill();
            }
        }
        catch
        {
            // Best effort: a finished audio process is harmless.
        }
    }

    private static void StopProcesses(List<Process> processes)
    {
        foreach (var process in processes)
        {
            StopProcess(process);
        }

        processes.Clear();
    }

    private static void CleanupFinishedProcesses(List<Process> processes)
    {
        processes.RemoveAll(process =>
        {
            try
            {
                if (!process.HasExited)
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
