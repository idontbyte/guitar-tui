using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.JazzChords;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private readonly JazzChordLibrary _jazzChords = new();

    private void ShowJazzChordsMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz chords");
            Console.WriteLine("Jazz harmony is mostly about chord function, guide tones, and smooth voice movement.");
            Console.WriteLine();
            WriteMenuOption("1", "Fundamentals path", "A guided order: formulas, guide tones, shells, ii-V-I, then standards.");
            WriteMenuOption("2", "Chord type list", "See what symbols like maj7, m7b5, 9, and 13 mean.");
            WriteMenuOption("3", "Voicing reference", "Find playable guitar shapes for guide tones, shells, or full chords.");
            WriteMenuOption("4", "ii-V-I trainer", "Drill jazz's most common progression in major and minor keys.");
            WriteMenuOption("5", "Song library", "Apply jazz chords to standards-style progressions with concept tags.");
            WriteMenuOption("6", "Random arrangement game", "Get a fresh connected jazz progression to practise.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzFundamentalsPath();
                    break;
                case "2":
                    ShowJazzChordTypeList();
                    break;
                case "3":
                    ShowJazzChordVoicingReference();
                    break;
                case "4":
                    ShowTwoFiveOneTrainer();
                    break;
                case "5":
                    ShowJazzChordSongLibrary();
                    break;
                case "6":
                    ShowRandomJazzChordGame();
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

    private static void ShowJazzChordTypeList()
    {
        Console.Clear();
        WriteHeader("Jazz chord type list");
        Console.WriteLine("Jazz harmony mostly extends triads into 6th, 7th, 9th, 11th, and 13th chords.");
        Console.WriteLine("On guitar, players use inversions too, but usually call the practical shapes voicings: shell, drop 2, rootless, and altered voicings.");
        Console.WriteLine();

        foreach (var quality in JazzChordLibrary.Qualities)
        {
            Console.WriteLine($"{quality.Suffix.PadRight(6)} {quality.Name.PadRight(18)} {string.Join(" ", quality.Intervals).PadRight(14)} {quality.Use}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to go back.");
        Console.ReadKey(intercept: true);
    }

    private void ShowJazzFundamentalsPath()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz fundamentals path");
            Console.WriteLine("Work from the smallest useful idea to real jazz tunes.");
            Console.WriteLine();
            WriteMenuOption("1", "Seventh chord formulas", "Learn the ingredients behind jazz chord symbols.");
            WriteMenuOption("2", "Guide-tone ii-V-I", "Practise only the 3rds and 7ths that steer the harmony.");
            WriteMenuOption("3", "Shell voicing ii-V-I", "Add the root to guide tones for practical comping grips.");
            WriteMenuOption("4", "Minor ii-V-i", "Learn the darker minor-key version: m7b5 to altered dominant to minor tonic.");
            WriteMenuOption("5", "Apply to a standard", "Use shell voicings on a tune so the theory becomes music.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a step > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzChordTypeList();
                    break;
                case "2":
                    ShowJazzChordGame("Guide-tone ii-V-I", BuildMajorTwoFiveOneSetup("C"), JazzVoicingMode.GuideTones);
                    break;
                case "3":
                    ShowJazzChordGame("Shell voicing ii-V-I", BuildMajorTwoFiveOneSetup("C"), JazzVoicingMode.Shell);
                    break;
                case "4":
                    ShowJazzChordGame("Minor ii-V-i", BuildMinorTwoFiveOneSetup("C"), JazzVoicingMode.Shell);
                    break;
                case "5":
                    {
                        var setup = ReadJazzSongSetup();
                        if (setup is not null)
                        {
                            ShowJazzChordGame("Jazz standard fundamentals", setup, JazzVoicingMode.Shell);
                        }
                        break;
                    }
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

    private void ShowJazzChordVoicingReference()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz chord voicing reference");
            Console.WriteLine("A voicing is the way a chord's notes are arranged on the guitar.");
            Console.WriteLine("Start with guide tones, then shells, then full voicings.");
            Console.WriteLine();

            var root = ReadMenuChoice("Choose a root", GuitarResourcesTui.Triads.MusicTheory.ChromaticRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            while (true)
            {
                var qualityChoice = ReadMenuChoice("Choose a chord type", JazzChordLibrary.Qualities.Select(quality => quality.Suffix).ToArray(), allowBack: true);
                if (qualityChoice is null)
                {
                    break;
                }

                var quality = JazzChordLibrary.Quality(qualityChoice);
                var voicingMode = ReadJazzVoicingMode();
                if (voicingMode is null)
                {
                    break;
                }

                ShowJazzChordVoicings(root, quality, voicingMode.Value);
            }
        }
    }

    private void ShowJazzChordVoicings(string root, JazzChordQuality quality, JazzVoicingMode voicingMode)
    {
        while (true)
        {
            Console.Clear();
            WriteHeader($"{root}{quality.Suffix} {voicingMode.DisplayName()}");
            Console.WriteLine($"Labels: {Root}R{Reset} = root, {Third}3/b3{Reset} = third/tension, {Fifth}5{Reset} = fifth, {Pentatonic}7/b7/9/13{Reset} = color tones");
            Console.WriteLine($"V = voicing mode ({voicingMode.DisplayName()}), {NoteLabelCommandText()}, B = back, Q = main menu");
            Console.WriteLine();
            Console.WriteLine($"{quality.Name}: full formula {string.Join(" ", quality.Intervals)}. Showing {string.Join(" ", JazzChordLibrary.IntervalsFor(quality, voicingMode))}.");
            Console.WriteLine();

            var groups = _jazzChords.GetVoicings(root, quality, voicingMode);
            foreach (var group in groups)
            {
                if (group.Voicings.Count == 0)
                {
                    continue;
                }

                Console.WriteLine(group.Name);
                Console.WriteLine(new string('-', group.Name.Length));

                var diagrams = group.Voicings
                    .Select(voicing => ($"{voicing.LowToHigh} ({voicing.MinFret}-{voicing.MaxFret})", voicing.Diagram))
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
                    return;
                case ConsoleKey.L:
                    ToggleNoteLabelMode();
                    break;
                case ConsoleKey.V:
                    voicingMode = voicingMode.Next();
                    break;
                case ConsoleKey.Q:
                    return;
            }
        }
    }

    private void ShowJazzChordSongLibrary()
    {
        var setup = ReadJazzSongSetup();
        if (setup is null)
        {
            return;
        }

        var voicingMode = ReadJazzVoicingMode() ?? JazzVoicingMode.Shell;
        ShowJazzChordGame("Jazz chord song library", setup, voicingMode);
    }

    private void ShowRandomJazzChordGame()
    {
        var progression = _jazzChords.BuildRandomProgression();
        var progressionText = string.Join(" ", progression.Select(chord => chord.DisplayName));
        var setup = new TriadProgressionSetup($"Random jazz arrangement: {progressionText}", progressionText, 90, new TimeSignature(4, 4), [4, 4, 4, 4]);
        ShowJazzChordGame("Random jazz chord arrangement", setup, JazzVoicingMode.Shell);
    }

    private static TriadProgressionSetup? ReadJazzSongSetup()
    {
        Console.Clear();
        WriteHeader("Jazz song library");
        Console.WriteLine("Each preset lists a compact study progression plus the jazz ideas it demonstrates.");
        Console.WriteLine("Try shell voicings first, then switch to full voicings once the movement feels familiar.");
        Console.WriteLine();
        foreach (var preset in JazzChordLibrary.PresetProgressions)
        {
            Console.WriteLine(preset.MenuText);
        }
        Console.WriteLine();
        Console.Write("Song number, title, or B > ");

        while (true)
        {
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (TryFindJazzPreset(input, out var preset))
            {
                return new TriadProgressionSetup(preset.Name, preset.ProgressionText, preset.Bpm, preset.TimeSignature, preset.ChordLengths);
            }

            Console.Write("Choose a listed number, song title, or B > ");
        }
    }

    private static bool TryFindJazzPreset(string input, out PresetJazzProgression preset)
    {
        preset = null!;
        var trimmed = input.Trim();
        var digitPrefix = new string(trimmed.TakeWhile(char.IsDigit).ToArray());

        if (digitPrefix.Length > 0 && int.TryParse(digitPrefix, out var presetNumber))
        {
            var numberMatch = JazzChordLibrary.GetPresetProgression(presetNumber);
            if (numberMatch is not null)
            {
                preset = numberMatch;
                return true;
            }
        }

        var textMatch = JazzChordLibrary.PresetProgressions.FirstOrDefault(candidate =>
            candidate.Title.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            candidate.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            candidate.MenuText.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        if (textMatch is not null)
        {
            preset = textMatch;
            return true;
        }

        return false;
    }

    private void ShowTwoFiveOneTrainer()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("ii-V-I trainer");
            Console.WriteLine("A ii-V-I means chords built from scale degrees 2, 5, and 1.");
            Console.WriteLine("In C major that is Dm7 -> G7 -> Cmaj7.");
            Console.WriteLine();
            WriteMenuOption("1", "Major ii-V-I in a random key", "Let the app choose the key so you learn the pattern everywhere.");
            WriteMenuOption("2", "Minor ii-V-i in a random key", "Practise m7b5 -> 7b9 -> minor tonic movement.");
            WriteMenuOption("3", "Major ii-V-I in a chosen key", "Pick a key and focus on one area.");
            WriteMenuOption("4", "Minor ii-V-i in a chosen key", "Pick a minor key and drill its darker resolution.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a drill > ");

            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var minor = input is "2" or "4";
            var chosen = input is "3" or "4";
            if (input is not ("1" or "2" or "3" or "4"))
            {
                Console.WriteLine("That choice is not on the menu.");
                Console.ReadKey(intercept: true);
                continue;
            }

            var keyRoot = chosen
                ? ReadMenuChoice("Choose a key", MusicTheory.ChromaticRoots, allowBack: true)
                : MusicTheory.ChromaticRoots[Random.Shared.Next(MusicTheory.ChromaticRoots.Count)];
            if (keyRoot is null)
            {
                continue;
            }

            var voicingMode = ReadJazzVoicingMode() ?? JazzVoicingMode.GuideTones;
            var setup = minor ? BuildMinorTwoFiveOneSetup(keyRoot) : BuildMajorTwoFiveOneSetup(keyRoot);
            ShowJazzChordGame(minor ? "Minor ii-V-i trainer" : "Major ii-V-I trainer", setup, voicingMode);
        }
    }

    private static TriadProgressionSetup BuildMajorTwoFiveOneSetup(string keyRoot)
    {
        var rootPitch = MusicTheory.PitchClassFor(keyRoot);
        var progression = string.Join(" ", new[]
        {
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 2), JazzChordLibrary.Quality("m7")),
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 7), JazzChordLibrary.Quality("9")),
            new JazzChordSymbol(keyRoot, JazzChordLibrary.Quality("maj7"))
        }.Select(chord => chord.DisplayName));

        return new TriadProgressionSetup($"{keyRoot} major ii-V-I: {progression}", progression, 90, new TimeSignature(4, 4), [4, 4, 8]);
    }

    private static TriadProgressionSetup BuildMinorTwoFiveOneSetup(string keyRoot)
    {
        var rootPitch = MusicTheory.PitchClassFor(keyRoot);
        var progression = string.Join(" ", new[]
        {
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 2), JazzChordLibrary.Quality("m7b5")),
            new JazzChordSymbol(MusicTheory.NameFor(rootPitch + 7), JazzChordLibrary.Quality("7b9")),
            new JazzChordSymbol(keyRoot, JazzChordLibrary.Quality("m6"))
        }.Select(chord => chord.DisplayName));

        return new TriadProgressionSetup($"{keyRoot} minor ii-V-i: {progression}", progression, 84, new TimeSignature(4, 4), [4, 4, 8]);
    }

    private static JazzVoicingMode? ReadJazzVoicingMode()
    {
        Console.WriteLine();
        Console.WriteLine("Voicing mode");
        WriteMenuOption("1", "Guide tones", "Only the 3rd and 7th: the tiny notes that define the chord and lead the ear.");
        WriteMenuOption("2", "Shell voicings", "Root plus guide tones: small, practical jazz rhythm-guitar grips.");
        WriteMenuOption("3", "Full voicings", "Four-note shapes with extra colors like 5, 9, 13, or altered tensions.");
        Console.WriteLine("B. Back");
        Console.Write("> ");

        while (true)
        {
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            switch (input.ToUpperInvariant())
            {
                case "1":
                case "GUIDE":
                case "GUIDE TONES":
                    return JazzVoicingMode.GuideTones;
                case "2":
                case "SHELL":
                case "SHELL VOICINGS":
                    return JazzVoicingMode.Shell;
                case "3":
                case "FULL":
                case "FULL VOICINGS":
                    return JazzVoicingMode.Full;
                case "B":
                case "Q":
                    return null;
                default:
                    Console.Write("Choose 1, 2, 3, or B > ");
                    break;
            }
        }
    }

    private void ShowJazzChordGame(string gameTitle, TriadProgressionSetup setup, JazzVoicingMode voicingMode)
    {
        IReadOnlyList<JazzChordSymbol> progression;
        try
        {
            progression = _jazzChords.ParseProgression(setup.ProgressionText);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
            Console.ReadKey(intercept: true);
            return;
        }

        var bpm = setup.Bpm ?? ReadInt("BPM", defaultValue: 90, min: 30, max: 240);
        var timeSignature = setup.TimeSignature ?? ReadTimeSignature();
        var chordLengths = setup.ChordLengths ?? ReadJazzChordLengths(progression, timeSignature);
        var currentPhrase = _jazzChords.BuildPhrase(progression, voicingMode: voicingMode);
        var nextPhrase = _jazzChords.BuildPhrase(progression, currentPhrase[^1], voicingMode);
        var beat = 0;
        var paused = false;
        var clickEnabled = true;
        var backingEnabled = true;
        var lastSynthChordIndex = -1;
        var lastRenderedChordIndex = -1;
        var phraseLength = chordLengths.Sum();
        var synthProcesses = new List<AudioPlayback>();
        WarmJazzBackingChords(progression, chordLengths, bpm, timeSignature);

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
                    RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
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
                                RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
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
                                RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
                                break;
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
                                break;
                            case ConsoleKey.V:
                                voicingMode = voicingMode.Next();
                                currentPhrase = _jazzChords.BuildPhrase(progression, voicingMode: voicingMode);
                                nextPhrase = _jazzChords.BuildPhrase(progression, currentPhrase[^1], voicingMode);
                                lastRenderedChordIndex = -1;
                                RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmJazzBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmJazzBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderJazzChordGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, voicingMode);
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
                    nextPhrase = _jazzChords.BuildPhrase(progression, currentPhrase[^1], voicingMode);
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

    private IReadOnlyList<int> ReadJazzChordLengths(IReadOnlyList<JazzChordSymbol> progression, TimeSignature timeSignature)
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
                return _jazzChords.ParseChordLengths(lengthInput, progression.Count, defaultBeatsPerChord);
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }

    private void RenderJazzChordGame(
        string gameTitle,
        string title,
        IReadOnlyList<JazzPracticeItem> currentPhrase,
        IReadOnlyList<JazzPracticeItem> nextPhrase,
        IReadOnlyList<int> chordLengths,
        int chordIndex,
        int beatWithinChord,
        int beatWithinBar,
        TimeSignature timeSignature,
        int bpm,
        bool clickEnabled,
        bool backingEnabled,
        bool paused,
        JazzVoicingMode voicingMode)
    {
        Console.Clear();
        WriteHeader(gameTitle);
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Voicing: {voicingMode.DisplayName()}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine($"Space = pause, -/+ = tempo, V = voicing mode, M = mute click, S = mute backing, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {currentPhrase[chordIndex].Chord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        Console.WriteLine($"Labels: {Root}R{Reset} = root, {Third}3/b3{Reset} = third, {Fifth}5/b5{Reset} = fifth, {Pentatonic}7/b7/9/13{Reset} = color tones");
        Console.WriteLine(JazzChordLibrary.TeachingHintFor(currentPhrase[chordIndex].Chord, voicingMode));
        var nextChord = chordIndex + 1 < currentPhrase.Count ? currentPhrase[chordIndex + 1].Chord : nextPhrase[0].Chord;
        Console.WriteLine($"Guide tones: {JazzChordLibrary.GuideToneSummary(currentPhrase[chordIndex].Chord)}");
        Console.WriteLine($"Next movement: {JazzChordLibrary.GuideToneMovement(currentPhrase[chordIndex].Chord, nextChord)}");
        Console.WriteLine();

        var currentDiagrams = currentPhrase
            .Select((item, index) => ($"{(index == chordIndex ? "> " : "  ")}{item.Title} [{chordLengths[index]} beat{Pluralize(chordLengths[index])}]", item.Diagram))
            .ToArray();
        var nextDiagrams = nextPhrase
            .Select((item, index) => ($"  {item.Title} [{chordLengths[index]} beat{Pluralize(chordLengths[index])}]", item.Diagram))
            .ToArray();
        var gridCellWidth = currentDiagrams
            .Concat(nextDiagrams)
            .Max(_renderer.MeasureTitledDiagramWidth);

        foreach (var line in _renderer.RenderMany(currentDiagrams, GetUsableConsoleWidth(), new HashSet<int> { chordIndex }, maxColumns: 4, cellWidth: gridCellWidth))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedJazzProgression(currentPhrase.Select(item => item.Chord).ToArray(), chordIndex);
        Console.WriteLine();
        Console.WriteLine("Up next");

        foreach (var line in _renderer.RenderMany(nextDiagrams, GetUsableConsoleWidth(), maxColumns: 4, cellWidth: gridCellWidth))
        {
            Console.WriteLine(line);
        }
    }

    private static void WriteCenteredHighlightedJazzProgression(IReadOnlyList<JazzChordSymbol> progression, int chordIndex)
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

    private static FretboardDiagram BuildJazzChordDiagram(JazzChordSymbol chord)
    {
        return new JazzChordLibrary()
            .GetVoicings(chord.Root, chord.Quality)
            .SelectMany(group => group.Voicings)
            .First()
            .Diagram;
    }
}
