using GuitarResourcesTui.Arpeggios;
using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.Triads;
using GuitarResourcesTui.Tui.Printing;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private readonly ArpeggioLibrary _arpeggios = new();

    private void ShowArpeggiosMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Arpeggios");
            WriteDescriptionLine("An arpeggio is a chord played one note at a time.");
            WriteDescriptionLine("They are the chord tones soloists target and the broken-chord patterns rhythm players pick.");
            Console.WriteLine();
            WriteMenuOption("1", "What are arpeggios?", "Learn what arpeggios are, why they matter, and how they differ from scales.");
            WriteMenuOption("2", "Shape reference", "See movable arpeggio maps for triads and 7th chords.");
            WriteMenuOption("3", "Find the chord tone", "Hear a chord, then find a target root, 3rd, 5th, or 7th.");
            WriteMenuOption("4", "Song changes arpeggio game", "Follow the current chord and switch arpeggios as the harmony moves.");
            WriteMenuOption("5", "Rhythm arpeggio patterns", "Practise broken-chord picking patterns for rhythm parts.");
            WriteMenuOption("6", "Printable sheets", "Open printable arpeggio maps for practice away from the app.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowArpeggioIntroduction();
                    break;
                case "2":
                    ShowArpeggioShapeReference(printOnly: false);
                    break;
                case "3":
                    ShowArpeggioTargetingGame();
                    break;
                case "4":
                    ShowArpeggioSongGame();
                    break;
                case "5":
                    ShowRhythmArpeggioPatterns();
                    break;
                case "6":
                    ShowArpeggioShapeReference(printOnly: true);
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

    private static void ShowArpeggioIntroduction()
    {
        Console.Clear();
        WriteHeader("What are arpeggios?");
        WriteDescriptionLine("Chord: notes played together.");
        WriteDescriptionLine("Arpeggio: the same chord notes played separately.");
        WriteDescriptionLine("Scale: a wider note collection. Arpeggio: the strongest chord tones inside that collection.");
        Console.WriteLine();
        Console.WriteLine("Examples");
        WriteDescriptionLine("C major chord tones: C E G       Formula: R 3 5");
        WriteDescriptionLine("Cmaj7 chord tones:   C E G B     Formula: R 3 5 7");
        WriteDescriptionLine("Cm7 chord tones:     C Eb G Bb   Formula: R b3 5 b7");
        WriteDescriptionLine("G7 chord tones:      G B D F     Formula: R 3 5 b7");
        Console.WriteLine();
        Console.WriteLine("Why soloists use them");
        WriteDescriptionLine("- They outline the chord currently being played.");
        WriteDescriptionLine("- They make melodies sound connected to the harmony.");
        WriteDescriptionLine("- Landing on a 3rd or 7th during a chord change can sound more intentional than running a scale.");
        Console.WriteLine();
        Console.WriteLine("Why rhythm players use them");
        WriteDescriptionLine("- Fingerpicking and broken-chord parts are arpeggios.");
        WriteDescriptionLine("- Picking chord tones one at a time creates motion without changing harmony.");
        WriteDescriptionLine("- Jazz players often break shell voicings into small rhythmic figures.");
        Console.WriteLine();
        WriteDescriptionLine("Press any key to go back.");
        Console.ReadKey(intercept: true);
    }

    private void ShowArpeggioShapeReference(bool printOnly)
    {
        while (true)
        {
            Console.Clear();
            WriteHeader(printOnly ? "Printable arpeggio sheets" : "Arpeggio shape reference");
            WriteDescriptionLine("Choose a root and chord type. The app shows chord tones across movable fretboard windows.");
            Console.WriteLine();

            var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
            if (root is null)
            {
                return;
            }

            var quality = ReadArpeggioQuality();
            if (quality is null)
            {
                continue;
            }

            var shapes = _arpeggios.GetShapes(root, quality);
            if (printOnly)
            {
                PrintArpeggioSheet(root, quality, shapes);
                continue;
            }

            ShowArpeggioShapes(root, quality, shapes);
        }
    }

    private void ShowArpeggioShapes(string root, ArpeggioQuality quality, IReadOnlyList<ArpeggioShape> shapes)
    {
        while (true)
        {
            Console.Clear();
            WriteHeader($"{root}{quality.Suffix} arpeggio shapes");
            Console.WriteLine($"{quality.Name}: {string.Join(" ", quality.Intervals)} = {ArpeggioLibrary.NotesFor(new ArpeggioChordSymbol(root, quality))}");
            Console.WriteLine($"{quality.Use}");
            Console.WriteLine($"P = print sheet, {NoteLabelCommandText()}, B = back, Q = main menu");
            Console.WriteLine();

            var diagrams = shapes
                .Select(shape => ($"Position {shape.Number} ({shape.StartFret}-{shape.StartFret + shape.Diagram.Length - 1})", shape.Diagram))
                .ToArray();
            var cellWidth = diagrams.Max(_renderer.MeasureTitledDiagramWidth);

            foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth(), maxColumns: 2, cellWidth: cellWidth))
            {
                Console.WriteLine(line);
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
                case ConsoleKey.P:
                    PrintArpeggioSheet(root, quality, shapes);
                    break;
                case ConsoleKey.Q:
                    return;
            }
        }
    }

    private ArpeggioQuality? ReadArpeggioQuality()
    {
        var qualityName = ReadMenuChoice(
            "Choose an arpeggio type",
            ArpeggioLibrary.Qualities.Select(quality => $"{quality.Name} ({DisplayArpeggioSuffix(quality)})").ToArray(),
            allowBack: true);
        if (qualityName is null)
        {
            return null;
        }

        var index = ArpeggioLibrary.Qualities
            .Select((quality, itemIndex) => (quality, itemIndex))
            .Single(item => $"{item.quality.Name} ({DisplayArpeggioSuffix(item.quality)})" == qualityName)
            .itemIndex;
        return ArpeggioLibrary.Qualities[index];
    }

    private void ShowArpeggioTargetingGame()
    {
        Console.Clear();
        WriteHeader("Find the chord tone");
        WriteDescriptionLine("The app gives you a chord and a target chord tone. Hear the chord, then find the target note.");
        WriteDescriptionLine("This teaches soloing through changes: land on strong notes when the chord arrives.");
        Console.WriteLine();

        var setup = ReadArpeggioProgressionSetup();
        if (setup is null)
        {
            return;
        }

        ShowArpeggioTargetingGame(setup);
    }

    private void ShowArpeggioTargetingGame(TriadProgressionSetup setup)
    {
        IReadOnlyList<ArpeggioChordSymbol> progression;
        try
        {
            progression = _arpeggios.ParseProgression(setup.ProgressionText);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
            Console.ReadKey(intercept: true);
            return;
        }

        var selectedIntervals = ReadArpeggioIntervalSet("Target intervals, for example R 3 5 b7, or blank for all chord tones");
        var prompt = _arpeggios.BuildPrompt(progression, selectedIntervals);
        var reveal = false;
        AudioPlayback? playback = null;

        try
        {
            playback = PlayArpeggioPromptChord(prompt);

            while (true)
            {
                var diagram = reveal
                    ? _arpeggios.BuildMap(prompt.Chord.Root, prompt.Chord.Quality, startFret: 0, length: 16, new HashSet<string> { prompt.TargetInterval })
                    : _arpeggios.BuildMap(prompt.Chord.Root, prompt.Chord.Quality, startFret: 0, length: 16);

                Console.Clear();
                WriteHeader("Find the chord tone");
                Console.WriteLine("Goal: hear the chord, then find and play the target note on your guitar.");
                Console.WriteLine("The same target note appears in several places; any matching fret counts.");
                Console.WriteLine();
                Console.WriteLine($"Progression: {string.Join(" - ", progression.Select(chord => chord.DisplayName))}");
                Console.WriteLine($"Chord sounding: {prompt.Chord.DisplayName} ({ArpeggioLibrary.NotesFor(prompt.Chord)})");
                Console.WriteLine($"Find: {Third}{prompt.TargetInterval}{Reset} = {prompt.TargetNote}");
                Console.WriteLine(ArpeggioLibrary.TargetHintFor(prompt.TargetInterval));
                Console.WriteLine(reveal ? "Reveal is on: only the target notes are shown." : "Reveal is off: the full arpeggio is shown so you can work out the target.");
                Console.WriteLine($"C = hear chord, T = hear target note, R = reveal target, N = next prompt, {NoteLabelCommandText()}, B/Q = back");
                Console.WriteLine();

                foreach (var line in _renderer.Render(diagram))
                {
                    Console.WriteLine(line);
                }

                Console.WriteLine();
                Console.Write("Command > ");
                switch (Console.ReadKey(intercept: true).Key)
                {
                    case ConsoleKey.C:
                        playback = ReplacePlayback(playback, PlayArpeggioPromptChord(prompt));
                        break;
                    case ConsoleKey.T:
                        playback = ReplacePlayback(playback, PlayTunerNote(TargetTunerNote(prompt)));
                        break;
                    case ConsoleKey.R:
                        reveal = !reveal;
                        break;
                    case ConsoleKey.N:
                    case ConsoleKey.RightArrow:
                        prompt = _arpeggios.BuildPrompt(progression, selectedIntervals);
                        reveal = false;
                        playback = ReplacePlayback(playback, PlayArpeggioPromptChord(prompt));
                        break;
                    case ConsoleKey.L:
                        ToggleNoteLabelMode();
                        break;
                    case ConsoleKey.B:
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        return;
                }
            }
        }
        finally
        {
            playback?.Stop();
            playback?.Dispose();
        }
    }

    private void ShowArpeggioSongGame()
    {
        Console.Clear();
        WriteHeader("Song changes arpeggio game");
        WriteDescriptionLine("Follow the chord changes by switching to the matching arpeggio.");
        WriteDescriptionLine("This is the bridge between knowing shapes and soloing through a progression.");
        Console.WriteLine();

        var setup = ReadArpeggioProgressionSetup();
        if (setup is null)
        {
            return;
        }

        ShowArpeggioSongGame(setup);
    }

    private void ShowArpeggioSongGame(TriadProgressionSetup setup)
    {
        IReadOnlyList<ArpeggioChordSymbol> progression;
        try
        {
            progression = _arpeggios.ParseProgression(setup.ProgressionText);
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine(exception.Message);
            Console.ReadKey(intercept: true);
            return;
        }

        var bpm = setup.Bpm ?? ReadInt("BPM", defaultValue: 80, min: 30, max: 240);
        var timeSignature = setup.TimeSignature ?? ReadTimeSignature();
        var chordLengths = setup.ChordLengths ?? ReadArpeggioChordLengths(progression, timeSignature);
        var beat = 0;
        var paused = false;
        var clickEnabled = true;
        var backingEnabled = true;
        var targetMode = false;
        var lastSynthChordIndex = -1;
        var lastRenderedChordIndex = -1;
        var phraseLength = chordLengths.Sum();
        var synthProcesses = new List<AudioPlayback>();
        WarmArpeggioBackingChords(progression, chordLengths, bpm, timeSignature);

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
                    var synthProcess = PlayBackingChord(BackingChordFor(progression[chordIndex]), chordLengths[chordIndex], bpm, timeSignature);
                    if (synthProcess is not null)
                    {
                        synthProcesses.Add(synthProcess);
                    }

                    lastSynthChordIndex = chordIndex;
                }

                if (chordIndex != lastRenderedChordIndex)
                {
                    RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
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
                                RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
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
                                RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
                                break;
                            case ConsoleKey.T:
                                targetMode = !targetMode;
                                RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
                                break;
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmArpeggioBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmArpeggioBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderArpeggioSongGame(setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, targetMode);
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

    private static void ShowRhythmArpeggioPatterns()
    {
        Console.Clear();
        WriteHeader("Rhythm arpeggio patterns");
        WriteDescriptionLine("Rhythm arpeggios are broken chords: you hold a chord shape and pick its notes separately.");
        WriteDescriptionLine("Use them for fingerpicking, ballads, pop accompaniments, and quiet comping.");
        Console.WriteLine();
        Console.WriteLine("Practice these over any chord shape you know:");
        WriteDescriptionLine("1. Ascending: low string -> high string.");
        WriteDescriptionLine("2. Descending: high string -> low string.");
        WriteDescriptionLine("3. Bass + upper notes: bass, 3rd/4th string, 2nd string, 1st string.");
        WriteDescriptionLine("4. Outside-in: lowest, highest, middle-low, middle-high.");
        WriteDescriptionLine("5. 6/8 ballad: bass, middle, high, middle, high, middle.");
        WriteDescriptionLine("6. Jazz shell break-up: root, 7th, 3rd, 7th.");
        Console.WriteLine();
        Console.WriteLine("How to practise");
        WriteDescriptionLine("- Start with C - G - Am - F or Dm7 - G7 - Cmaj7.");
        WriteDescriptionLine("- Keep the chord ringing while each note speaks clearly.");
        WriteDescriptionLine("- Change chords on beat 1, then keep the picking hand moving.");
        WriteDescriptionLine("- Later, mute some notes for a tighter rhythmic comping sound.");
        Console.WriteLine();
        WriteDescriptionLine("Press any key to go back.");
        Console.ReadKey(intercept: true);
    }

    private void RenderArpeggioSongGame(
        string title,
        IReadOnlyList<ArpeggioChordSymbol> progression,
        IReadOnlyList<int> chordLengths,
        int chordIndex,
        int beatWithinChord,
        int beatWithinBar,
        TimeSignature timeSignature,
        int bpm,
        bool clickEnabled,
        bool backingEnabled,
        bool paused,
        bool targetMode)
    {
        var chord = progression[chordIndex];
        var selectedIntervals = targetMode
            ? new HashSet<string>(["3", "b3", "7", "b7", "bb7"])
            : chord.Quality.Intervals.ToHashSet();
        var diagram = _arpeggios.BuildMap(chord.Root, chord.Quality, startFret: 0, length: 16, selectedIntervals);

        Console.Clear();
        WriteHeader("Song changes arpeggio game");
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  View: {(targetMode ? "3rds/7ths" : "full arpeggio")}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine($"Space = pause, -/+ = tempo, T = target tones, M = mute click, S = mute backing, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
        Console.WriteLine();
        Console.WriteLine($"Now: {chord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        Console.WriteLine(ArpeggioLibrary.TeachingHintFor(chord));
        Console.WriteLine();

        foreach (var line in _renderer.Render(diagram))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedArpeggioProgression(progression, chordIndex);
    }

    private TriadProgressionSetup? ReadArpeggioProgressionSetup()
    {
        WriteDescriptionLine("Choose where the chord progression comes from.");
        Console.WriteLine();
        WriteMenuOption("1", "Simple mode", "The app creates a short I-vi-ii-V style arpeggio workout.");
        WriteMenuOption("C", "Custom mode", "Type chords such as Dm7 G7 Cmaj7 or C G Am F.");
        WriteMenuOption("S", "Song select", "Pick a built-in song-style progression using triad arpeggios.");
        Console.WriteLine("B. Back");
        Console.WriteLine();
        Console.Write("Choose a mode > ");

        while (true)
        {
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            switch (input.ToUpperInvariant())
            {
                case "1":
                    return BuildSimpleArpeggioProgressionSetup();
                case "C":
                    return ReadCustomArpeggioProgressionSetup();
                case "S":
                    return ReadSongArpeggioProgressionSetup();
                case "B":
                case "Q":
                    return null;
                default:
                    Console.Write("Choose 1, C, S, or B > ");
                    break;
            }
        }
    }

    private static TriadProgressionSetup BuildSimpleArpeggioProgressionSetup()
    {
        var keyRoot = MusicTheory.ChromaticRoots[Random.Shared.Next(MusicTheory.ChromaticRoots.Count)];
        var rootPitch = MusicTheory.PitchClassFor(keyRoot);
        var chords = new[]
        {
            new ArpeggioChordSymbol(keyRoot, ArpeggioLibrary.Quality("maj7")),
            new ArpeggioChordSymbol(MusicTheory.NameFor(rootPitch + 9), ArpeggioLibrary.Quality("m7")),
            new ArpeggioChordSymbol(MusicTheory.NameFor(rootPitch + 2), ArpeggioLibrary.Quality("m7")),
            new ArpeggioChordSymbol(MusicTheory.NameFor(rootPitch + 7), ArpeggioLibrary.Quality("7"))
        };
        var progression = string.Join(" ", chords.Select(chord => chord.DisplayName));
        return new TriadProgressionSetup($"Simple arpeggio workout in {keyRoot}: {progression}", progression, 80, new TimeSignature(4, 4), [4, 4, 4, 4]);
    }

    private static TriadProgressionSetup ReadCustomArpeggioProgressionSetup()
    {
        Console.WriteLine();
        Console.WriteLine("Enter chords separated by commas or spaces. Try Dm7 G7 Cmaj7 or C G Am F.");
        Console.Write("Progression > ");
        return new TriadProgressionSetup("Custom arpeggio progression", Console.ReadLine()?.Trim() ?? string.Empty);
    }

    private TriadProgressionSetup? ReadSongArpeggioProgressionSetup()
    {
        var setup = ReadSongTriadProgressionSetup();
        if (setup is null)
        {
            return null;
        }

        var progression = _triadGame.ParseProgression(setup.ProgressionText)
            .Select(ArpeggioLibrary.FromTriadChord)
            .Select(chord => chord.DisplayName);

        return setup with { ProgressionText = string.Join(" ", progression), Title = $"{setup.Title} (triad arpeggios)" };
    }

    private IReadOnlyList<int> ReadArpeggioChordLengths(IReadOnlyList<ArpeggioChordSymbol> progression, TimeSignature timeSignature)
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
                return _arpeggios.ParseChordLengths(lengthInput, progression.Count, defaultBeatsPerChord);
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }

    private static HashSet<string> ReadArpeggioIntervalSet(string prompt)
    {
        var supported = new[] { "R", "b3", "3", "5", "b5", "bb7", "b7", "7" };
        while (true)
        {
            Console.Write($"{prompt} > ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                return supported.ToHashSet();
            }

            var labels = input
                .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var normalized = labels
                .Select(label => supported.SingleOrDefault(interval => interval.Equals(label, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (normalized.All(label => label is not null))
            {
                return normalized.Cast<string>().ToHashSet();
            }

            Console.WriteLine($"Use interval labels: {string.Join(" ", supported)}");
        }
    }

    private static void PrintArpeggioSheet(string root, ArpeggioQuality quality, IReadOnlyList<ArpeggioShape> shapes)
    {
        try
        {
            var filePath = ArpeggioSheetPrinter.Print(root, quality, shapes);
            Console.WriteLine();
            Console.WriteLine($"Opened printable sheet: {filePath}");
            Console.WriteLine("Use your browser's print dialog if it does not open automatically.");
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine($"Could not open the printable sheet automatically: {exception.Message}");
        }

        Console.WriteLine("Press any key to continue.");
        Console.ReadKey(intercept: true);
    }

    private static void WarmArpeggioBackingChords(IReadOnlyList<ArpeggioChordSymbol> progression, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < progression.Count; index++)
        {
            EnsureBackingChordFile(BackingChordFor(progression[index]), chordLengths[index], bpm, timeSignature);
        }
    }

    private static BackingChord BackingChordFor(ArpeggioChordSymbol chord)
    {
        var quality = chord.Quality.Intervals.Contains("b3") ? ChordQuality.Minor : ChordQuality.Major;
        return new BackingChord(chord.Root, quality, chord.Quality.Intervals.ToHashSet(), chord.Quality.Suffix);
    }

    private static AudioPlayback? PlayArpeggioPromptChord(ArpeggioPrompt prompt)
    {
        return PlayBackingChord(BackingChordFor(prompt.Chord), 4, 80, new TimeSignature(4, 4));
    }

    private static TunerNote TargetTunerNote(ArpeggioPrompt prompt)
    {
        var midiNote = 60 + MusicTheory.Normalize(MusicTheory.PitchClassFor(prompt.Chord.Root) + ArpeggioLibrary.SemitonesFor(prompt.TargetInterval));
        return new TunerNote(string.Empty, $"{prompt.TargetNote} target note", $"{prompt.TargetNote}4", midiNote);
    }

    private static AudioPlayback? ReplacePlayback(AudioPlayback? existing, AudioPlayback? replacement)
    {
        existing?.Stop();
        existing?.Dispose();
        return replacement;
    }

    private static void WriteCenteredHighlightedArpeggioProgression(IReadOnlyList<ArpeggioChordSymbol> progression, int chordIndex)
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

    private static string DisplayArpeggioSuffix(ArpeggioQuality quality)
    {
        return string.IsNullOrEmpty(quality.Suffix) ? "major" : quality.Suffix;
    }
}
