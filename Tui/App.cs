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

    private readonly FretboardRenderer _renderer = new();
    private readonly TriadProgressionGameLibrary _triadGame = new(triads);

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Guitar Resources");
            Console.WriteLine("1. Triad inversions");
            Console.WriteLine("2. Scale shapes");
            Console.WriteLine("3. Interval function map");
            Console.WriteLine("4. Triad progression game");
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowTriadInversions();
                    break;
                case "2":
                    ShowPentatonicShapes();
                    break;
                case "3":
                    ShowIntervalFunctionMap();
                    break;
                case "4":
                    ShowTriadProgressionGame();
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

    private void ShowTriadProgressionGame()
    {
        Console.Clear();
        WriteHeader("Triad progression game");
        Console.WriteLine("Enter chords separated by commas or spaces. Use m for minor, for example Am, C, G, D.");
        Console.Write("Progression > ");
        var progressionInput = Console.ReadLine()?.Trim() ?? string.Empty;

        IReadOnlyList<ChordSymbol> progression;
        try
        {
            progression = _triadGame.ParseProgression(progressionInput);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
            Console.ReadKey(intercept: true);
            return;
        }

        var bpm = ReadInt("BPM", defaultValue: 80, min: 30, max: 240);
        var beatsPerChord = ReadInt("Clicks per chord", defaultValue: 2, min: 1, max: 8);
        var clickEnabled = true;
        var synthEnabled = true;

        var currentPhrase = _triadGame.BuildPhrase(progression);
        var nextPhrase = _triadGame.BuildPhrase(progression, currentPhrase[^1]);
        var beat = 0;
        var paused = false;
        var lastSynthChordIndex = -1;
        var synthProcesses = new List<Process>();

        try
        {
            while (true)
            {
                var chordIndex = beat / beatsPerChord;
                var beatWithinChord = beat % beatsPerChord;

                RenderTriadProgressionGame(currentPhrase, nextPhrase, chordIndex, beatWithinChord, beatsPerChord, bpm, clickEnabled, synthEnabled, paused);

                if (!paused && synthEnabled && chordIndex != lastSynthChordIndex)
                {
                    CleanupFinishedProcesses(synthProcesses);
                    var synthProcess = PlaySynthChord(currentPhrase[chordIndex].Chord, beatsPerChord, bpm);
                    if (synthProcess is not null)
                    {
                        synthProcesses.Add(synthProcess);
                    }
                    lastSynthChordIndex = chordIndex;
                }

                if (!paused && clickEnabled)
                {
                    Click(beatWithinChord == 0);
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
                                RenderTriadProgressionGame(currentPhrase, nextPhrase, chordIndex, beatWithinChord, beatsPerChord, bpm, clickEnabled, synthEnabled, paused);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderTriadProgressionGame(currentPhrase, nextPhrase, chordIndex, beatWithinChord, beatsPerChord, bpm, clickEnabled, synthEnabled, paused);
                                break;
                            case ConsoleKey.S:
                                synthEnabled = !synthEnabled;
                                if (!synthEnabled)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    lastSynthChordIndex = -1;
                                }
                                RenderTriadProgressionGame(currentPhrase, nextPhrase, chordIndex, beatWithinChord, beatsPerChord, bpm, clickEnabled, synthEnabled, paused);
                                break;
                            case ConsoleKey.N:
                            case ConsoleKey.RightArrow:
                                beat = (chordIndex + 1) * beatsPerChord;
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
                if (beat >= currentPhrase.Count * beatsPerChord)
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
        IReadOnlyList<TriadPracticeItem> currentPhrase,
        IReadOnlyList<TriadPracticeItem> nextPhrase,
        int chordIndex,
        int beatWithinChord,
        int beatsPerChord,
        int bpm,
        bool clickEnabled,
        bool synthEnabled,
        bool paused)
    {
        Console.Clear();
        WriteHeader("Triad progression game");
        Console.WriteLine($"Progression: {string.Join(" - ", currentPhrase.Select(item => item.Chord.DisplayName))}");
        Console.WriteLine($"BPM: {bpm}  Clicks/chord: {beatsPerChord}  Click: {(clickEnabled ? "on" : "muted")}  Synth: {(synthEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine("Space = pause, M = mute click, S = mute synth, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {currentPhrase[chordIndex].Chord.DisplayName}  click {beatWithinChord + 1}/{beatsPerChord}");
        var currentDiagrams = currentPhrase
            .Select((item, index) => ($"{(index == chordIndex ? "> " : "  ")}{item.Title}", item.Diagram))
            .ToArray();

        foreach (var line in _renderer.RenderMany(currentDiagrams, GetUsableConsoleWidth(), new HashSet<int> { chordIndex }))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        Console.WriteLine("Up next");
        var nextDiagrams = nextPhrase
            .Select(item => ($"  {item.Title}", item.Diagram))
            .ToArray();

        foreach (var line in _renderer.RenderMany(nextDiagrams, GetUsableConsoleWidth()))
        {
            Console.WriteLine(line);
        }
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

    private static Process? PlaySynthChord(ChordSymbol chord, int beatsPerChord, int bpm)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var durationSeconds = beatsPerChord * 60d / bpm;
        var overlapSeconds = Math.Clamp(durationSeconds * 0.22, 0.18, 0.45);
        var path = SynthChordFilePath(chord, durationSeconds, overlapSeconds);
        if (!File.Exists(path))
        {
            WriteSynthChordWav(path, chord, durationSeconds, overlapSeconds);
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

    private static string SynthChordFilePath(ChordSymbol chord, double durationSeconds, double overlapSeconds)
    {
        var directory = Path.Combine(Path.GetTempPath(), "guitar-tui-synth");
        Directory.CreateDirectory(directory);

        var durationKey = Math.Round(durationSeconds, 3).ToString("0.000").Replace('.', '-');
        var overlapKey = Math.Round(overlapSeconds, 3).ToString("0.000").Replace('.', '-');
        return Path.Combine(directory, $"{chord.Root.Replace('#', 's')}-{chord.Quality}-{durationKey}-{overlapKey}-v2.wav");
    }

    private static void WriteSynthChordWav(string path, ChordSymbol chord, double durationSeconds, double overlapSeconds)
    {
        const int sampleRate = 44100;
        const short channels = 1;
        const short bitsPerSample = 16;
        var totalDurationSeconds = durationSeconds + overlapSeconds;
        var samples = Math.Max(1, (int)(sampleRate * totalDurationSeconds));
        var dataSize = samples * channels * bitsPerSample / 8;
        var tones = SynthFrequencies(chord).ToArray();

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
            var envelope = Envelope(time, totalDurationSeconds, durationSeconds);
            var value = 0d;

            foreach (var frequency in tones)
            {
                value += Math.Sin(2d * Math.PI * frequency * time);
                value += 0.35d * Math.Sin(2d * Math.PI * frequency * 2d * time);
            }

            value = Math.Tanh(value / tones.Length) * envelope * 0.22d;
            writer.Write((short)(value * short.MaxValue));
        }
    }

    private static IEnumerable<double> SynthFrequencies(ChordSymbol chord)
    {
        var rootPitch = MusicTheory.PitchClassFor(chord.Root);
        var thirdInterval = chord.Quality == ChordQuality.Major ? 4 : 3;
        var midiRoot = 48 + rootPitch;

        while (midiRoot > 59)
        {
            midiRoot -= 12;
        }

        return
        [
            MidiToFrequency(midiRoot),
            MidiToFrequency(midiRoot + thirdInterval),
            MidiToFrequency(midiRoot + 7),
            MidiToFrequency(midiRoot + 12)
        ];
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
