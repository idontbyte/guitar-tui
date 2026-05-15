using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.IntervalMaps;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Triads;
using GuitarResourcesTui.Tui.Printing;

namespace GuitarResourcesTui.Tui;

public sealed partial class App(TriadInversionLibrary triads, PentatonicLibrary pentatonics, IntervalFunctionMapLibrary intervalMaps)
{
    private const string Reset = "\e[0m";
    private const string Root = "\e[1;38;5;46m";
    private const string Third = "\e[1;38;5;220m";
    private const string Fifth = "\e[1;38;5;39m";
    private const string Pentatonic = "\e[1;38;5;213m";
    private const string BlueNote = "\e[1;38;5;51m";

    private readonly FretboardRenderer _renderer = new();
    private readonly TriadProgressionGameLibrary _triadGame = new(triads);
    private readonly TriadProgressionGameLibrary _spreadTriadGame = new(triads, voicingKind: TriadVoicingKind.Spread);
    private NoteLabelMode _noteLabelMode = NoteLabelMode.IntervalNames;

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Guitar Resources");
            WriteMenuOption("1", "Tuner", "Play reference notes so you can tune by ear.");
            WriteMenuOption("2", "Cowboy chords", "Learn open-position chords and practise changing between them in songs.");
            WriteMenuOption("3", "Triads", "Study 3-note chord shapes and inversions across the fretboard.");
            WriteMenuOption("4", "Jazz chords", "Learn guide tones, shell voicings, ii-V-I movement, and jazz standards.");
            WriteMenuOption("5", "Arpeggios", "Learn chord tones one note at a time for soloing, rhythm, and song changes.");
            WriteMenuOption("6", "Scales", "Explore scale shapes and practise using them over chord progressions.");
            WriteMenuOption("7", "Intervals", "See how notes relate to a root and practise targeting chord tones.");
            Console.WriteLine("0. Exit");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowTuner();
                    break;
                case "2":
                    ShowCowboyChordsMenu();
                    break;
                case "3":
                    ShowTriadsMenu();
                    break;
                case "4":
                    ShowJazzChordsMenu();
                    break;
                case "5":
                    ShowArpeggiosMenu();
                    break;
                case "6":
                    ShowScalesMenu();
                    break;
                case "7":
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
            Console.WriteLine("A triad is a 3-note chord: root, third, and fifth.");
            Console.WriteLine("Inversions are the same notes rearranged so a different chord tone is lowest.");
            Console.WriteLine();
            WriteMenuOption("1", "Inversions", "Reference all major/minor triad shapes by string group.");
            WriteMenuOption("2", "Music game", "Practise connected triad shapes through chord progressions.");
            WriteMenuOption("3", "Spread triads music game", "Practise wider triad shapes with skipped strings for a more open sound.");
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

    private void ToggleNoteLabelMode()
    {
        _noteLabelMode = _noteLabelMode switch
        {
            NoteLabelMode.IntervalNames => NoteLabelMode.FretNumbers,
            NoteLabelMode.FretNumbers => NoteLabelMode.Markers,
            _ => NoteLabelMode.IntervalNames
        };
        _renderer.NoteLabelMode = _noteLabelMode;
    }

    private string NoteLabelCommandText() => $"L = note labels ({DisplayNameFor(_noteLabelMode)})";

    private static string DisplayNameFor(NoteLabelMode mode) => mode switch
    {
        NoteLabelMode.IntervalNames => "intervals",
        NoteLabelMode.FretNumbers => "frets",
        NoteLabelMode.Markers => "X marks",
        _ => mode.ToString()
    };

    private void ShowChordDiagramSongGame(
        string gameTitle,
        TriadProgressionSetup setup,
        Func<ChordSymbol, FretboardDiagram> buildDiagram)
    {
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
                    RenderChordDiagramSongGame(gameTitle, setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, buildDiagram);
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
                                RenderChordDiagramSongGame(gameTitle, setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, buildDiagram);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderChordDiagramSongGame(gameTitle, setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, buildDiagram);
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
                                RenderChordDiagramSongGame(gameTitle, setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, buildDiagram);
                                break;
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderChordDiagramSongGame(gameTitle, setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, buildDiagram);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderChordDiagramSongGame(gameTitle, setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, buildDiagram);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderChordDiagramSongGame(gameTitle, setup.Title, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, buildDiagram);
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
        var layout = TriadGameLayout.PhraseRows;
        var inversionFilter = TriadInversionFilter.All;

        var currentPhrase = game.BuildPhrase(progression, inversionFilter: inversionFilter);
        var nextPhrase = game.BuildPhrase(progression, currentPhrase[^1], inversionFilter);
        var beat = 0;
        var paused = false;
        var lastSynthChordIndex = -1;
        var lastRenderedChordIndex = -1;
        var phraseLength = chordLengths.Sum();
        var phraseNumber = 0;
        (int Phrase, int ChordIndex)? lastAnnouncedChord = null;
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

                if (!paused && voiceEnabled && chordChanged && lastAnnouncedChord != (phraseNumber, chordIndex))
                {
                    voiceAnnouncer.SayChord(currentChord);
                    lastAnnouncedChord = (phraseNumber, chordIndex);
                }

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

                if (chordChanged)
                {
                    RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                    lastRenderedChordIndex = chordIndex;
                }

                if (!paused && clickEnabled)
                {
                    Click(beatWithinBar == 0);
                }

                var interval = TimeSpan.FromMinutes(1d / bpm);
                var deadline = beatStartedAt + interval;
                var voiceLeadTime = TimeSpan.FromMilliseconds(Math.Min(450, interval.TotalMilliseconds * 0.7));

                while (DateTime.UtcNow < deadline)
                {
                    if (!paused && voiceEnabled && DateTime.UtcNow >= deadline - voiceLeadTime)
                    {
                        var nextBeat = beat + 1;
                        var nextPhraseNumber = phraseNumber;
                        var nextPhraseForVoice = currentPhrase;

                        if (nextBeat >= phraseLength)
                        {
                            nextBeat = 0;
                            nextPhraseNumber++;
                            nextPhraseForVoice = nextPhrase;
                        }

                        var nextChordIndex = ChordIndexAtBeat(chordLengths, nextBeat);
                        if (nextBeat == StartBeatForChord(chordLengths, nextChordIndex) && lastAnnouncedChord != (nextPhraseNumber, nextChordIndex))
                        {
                            voiceAnnouncer.SayChord(nextPhraseForVoice[nextChordIndex].Chord);
                            lastAnnouncedChord = (nextPhraseNumber, nextChordIndex);
                        }
                    }

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
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
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
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                                break;
                            case ConsoleKey.V:
                                voiceEnabled = !voiceEnabled;
                                if (!voiceEnabled)
                                {
                                    voiceAnnouncer.Stop();
                                }
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                                break;
                            case ConsoleKey.T:
                                layout = layout == TriadGameLayout.PhraseRows ? TriadGameLayout.RollingNextThree : TriadGameLayout.PhraseRows;
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                                break;
                            case ConsoleKey.I:
                                inversionFilter = inversionFilter.Next();
                                currentPhrase = game.BuildPhrase(progression, inversionFilter: inversionFilter);
                                nextPhrase = game.BuildPhrase(progression, currentPhrase[^1], inversionFilter);
                                WarmBackingChords(currentPhrase, chordLengths, bpm, timeSignature);
                                WarmBackingChords(nextPhrase, chordLengths, bpm, timeSignature);
                                lastRenderedChordIndex = -1;
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                                break;
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(currentPhrase, chordLengths, bpm, timeSignature);
                                WarmBackingChords(nextPhrase, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(currentPhrase, chordLengths, bpm, timeSignature);
                                WarmBackingChords(nextPhrase, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderTriadProgressionGame(gameTitle, setup.Title, currentPhrase, nextPhrase, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, voiceEnabled, paused, inversionFilter, layout);
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
                    nextPhrase = game.BuildPhrase(progression, currentPhrase[^1], inversionFilter);
                    WarmBackingChords(nextPhrase, chordLengths, bpm, timeSignature);
                    beat = 0;
                    phraseNumber++;
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
        Console.WriteLine("Choose where the chord progression comes from.");
        Console.WriteLine();
        WriteMenuOption("1", "Simple mode", "The app creates a short random progression so you can start practising immediately.");
        WriteMenuOption("C", "Custom mode", "Type your own chords, such as Am C G D.");
        WriteMenuOption("S", "Song select", "Pick a built-in song-style progression.");
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
        var keyRoot = MusicTheory.ChromaticRoots[Random.Shared.Next(MusicTheory.ChromaticRoots.Count)];
        var minor = Random.Shared.Next(2) == 0;
        var formulas = minor
            ? new[] { (0, ChordQuality.Minor), (3, ChordQuality.Major), (5, ChordQuality.Minor), (7, ChordQuality.Minor), (8, ChordQuality.Major), (10, ChordQuality.Major) }
            : new[] { (0, ChordQuality.Major), (2, ChordQuality.Minor), (4, ChordQuality.Minor), (5, ChordQuality.Major), (7, ChordQuality.Major), (9, ChordQuality.Minor) };
        var rootPitch = MusicTheory.PitchClassFor(keyRoot);
        var selected = new List<(int Interval, ChordQuality Quality)> { formulas[0] };

        while (selected.Count < 4)
        {
            var candidate = formulas[Random.Shared.Next(formulas.Length)];
            if (candidate == selected[^1])
            {
                continue;
            }

            selected.Add(candidate);
        }

        var chords = selected
            .Select(chord => new ChordSymbol(MusicTheory.NameFor(rootPitch + chord.Interval), chord.Quality))
            .ToArray();
        var progression = string.Join(" ", chords.Select(chord => chord.DisplayName));
        var modeName = minor ? "minor" : "major";

        return new TriadProgressionSetup($"Simple random {keyRoot} {modeName}: {progression}", progression, 80, new TimeSignature(4, 4), [4, 4, 4, 4]);
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
        Console.WriteLine("Choose a progression to practise this scale over.");
        Console.WriteLine();
        WriteMenuOption("1", "Simple mode", "The app creates chords that fit the selected scale.");
        WriteMenuOption("C", "Custom mode", "Type your own chords and hear how the scale sits against them.");
        WriteMenuOption("S", "Song select", "Pick a built-in song-style progression.");
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
        return ReadPresetProgressionSetup(
            "Song select",
            TriadProgressionGameLibrary.PresetProgressions,
            "Choose 1-100, a song title, or B > ",
            allowListPosition: false);
    }

    private static TriadProgressionSetup? ReadPresetProgressionSetup(
        string title,
        IReadOnlyList<PresetChordProgression> presets,
        string retryPrompt,
        bool allowListPosition)
    {
        Console.Clear();
        WriteHeader(title);
        foreach (var preset in presets)
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

            if (TryFindPreset(presets, input, allowListPosition, out var preset))
            {
                return new TriadProgressionSetup(preset.Name, preset.ProgressionText, preset.Bpm, preset.TimeSignature, preset.ChordLengths);
            }

            Console.Write(retryPrompt);
        }
    }

    private static bool TryFindPreset(
        IReadOnlyList<PresetChordProgression> presets,
        string input,
        bool allowListPosition,
        out PresetChordProgression preset)
    {
        preset = null!;
        var trimmed = input.Trim();
        var digitPrefix = new string(trimmed.TakeWhile(char.IsDigit).ToArray());

        if (digitPrefix.Length > 0 && int.TryParse(digitPrefix, out var presetNumber))
        {
            var numberMatch = presets.FirstOrDefault(candidate => candidate.Number == presetNumber);
            if (numberMatch is not null)
            {
                preset = numberMatch;
                return true;
            }

            if (allowListPosition && presetNumber >= 1 && presetNumber <= presets.Count)
            {
                preset = presets[presetNumber - 1];
                return true;
            }
        }

        var textMatch = presets.FirstOrDefault(candidate =>
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
            Console.WriteLine("Intervals name the distance from a root note, such as 3, 5, b7, or 9.");
            Console.WriteLine("They help you understand chord tones and target notes anywhere on the neck.");
            Console.WriteLine();
            WriteMenuOption("1", "Lookup", "Pick a root and see interval names across a fretboard window.");
            WriteMenuOption("2", "Song game", "Practise finding target intervals while chords change.");
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
            Console.WriteLine("Choose a root note first. Every label will be measured from that root.");
            Console.WriteLine();

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
                    Console.WriteLine("Use this as a map: R is home, 3/b3 defines major/minor color, 5 is stable, and b7 adds dominant/minor-7 color.");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals are relative to {root}");
                    Console.WriteLine($"Showing: {string.Join(" ", selectedIntervals)}");
                    Console.WriteLine($"Anchor fret: {anchorFret}");
                    Console.WriteLine($"N/P = move anchor fret, {NoteLabelCommandText()}, B = back, Q = main menu");
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
                        case ConsoleKey.L:
                            ToggleNoteLabelMode();
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
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderIntervalSongGame(setup.Title, progression, prompts, phraseAnchors, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, keyRoot);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmIntervalBackingChords(prompts, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderIntervalSongGame(setup.Title, progression, prompts, phraseAnchors, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused, keyRoot);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmIntervalBackingChords(prompts, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
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

            var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
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
                    Console.WriteLine($"P = print sheet, {NoteLabelCommandText()}, B = back, Q = main menu");
                    Console.WriteLine();

                    var groupings = triads.GetTriadInversions(root, quality);
                    foreach (var grouping in groupings)
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
                        case ConsoleKey.L:
                            ToggleNoteLabelMode();
                            break;
                        case ConsoleKey.P:
                            PrintTriadSheet(root, quality, groupings);
                            break;
                        case ConsoleKey.Q:
                            return;
                    }
                }

            ChooseQuality:
                continue;
            }
        }
    }

    private static void PrintTriadSheet(string root, ChordQuality quality, IReadOnlyList<TriadGroupingResult> groupings)
    {
        try
        {
            var filePath = TriadSheetPrinter.Print(root, quality, groupings);
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

    private void ShowScalesMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Scales");
            Console.WriteLine("Scales are note collections for melodies, riffs, and improvising over chords.");
            Console.WriteLine();
            WriteMenuOption("1", "Shapes", "See movable fretboard patterns for a chosen root and scale.");
            WriteMenuOption("2", "Song game", "Practise using a chosen scale while chords move underneath.");
            WriteMenuOption("3", "Song library scale suggester", "Pick a song and let the app suggest scales that fit.");
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
                case "3":
                    ShowScaleLibrarySongGame();
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
            Console.WriteLine("Pick a root note, then a scale type. The app will show common positions across the neck.");
            Console.WriteLine();

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
                    Console.WriteLine("Scale degrees show how each note functions relative to the scale root.");
                    Console.WriteLine($"Labels: {Root}R{Reset} = root, intervals show scale degrees ({Pentatonic}2/4/6/7/flats{Reset}, {Third}3/b3{Reset}, {BlueNote}#4/b5{Reset}, {Fifth}5{Reset})");
                    Console.WriteLine($"T = toggle major/minor, {NoteLabelCommandText()}, B = back, Q = main menu");
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
                        case ConsoleKey.L:
                            ToggleNoteLabelMode();
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
        bool paused,
        TriadInversionFilter inversionFilter,
        TriadGameLayout layout)
    {
        Console.Clear();
        WriteHeader(gameTitle);
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Inversions: {inversionFilter.DisplayName()}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  Voice: {(voiceEnabled ? "on" : "muted")}  View: {DisplayNameFor(layout)}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine($"Space = pause, -/+ = tempo, I = inversion filter, M = mute click, S = mute backing, V = mute voice, T = toggle view, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {currentPhrase[chordIndex].Chord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        if (layout == TriadGameLayout.RollingNextThree)
        {
            RenderRollingTriadProgression(currentPhrase, nextPhrase, chordLengths, chordIndex);
            return;
        }

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
        WriteCenteredHighlightedProgression(currentPhrase, chordIndex);
        Console.WriteLine();
        Console.WriteLine("Up next");

        foreach (var line in _renderer.RenderMany(nextDiagrams, GetUsableConsoleWidth(), maxColumns: 4, cellWidth: gridCellWidth))
        {
            Console.WriteLine(line);
        }
    }

    private void RenderRollingTriadProgression(
        IReadOnlyList<TriadPracticeItem> currentPhrase,
        IReadOnlyList<TriadPracticeItem> nextPhrase,
        IReadOnlyList<int> chordLengths,
        int chordIndex)
    {
        var visibleItems = EnumerateRollingTriadItems(currentPhrase, nextPhrase, chordLengths, chordIndex)
            .Take(4)
            .Select((item, index) => ($"{(index == 0 ? "> " : "  ")}{item.PracticeItem.Title} [{item.Beats} beat{Pluralize(item.Beats)}]", item.PracticeItem.Diagram))
            .ToArray();
        var gridCellWidth = visibleItems.Max(_renderer.MeasureTitledDiagramWidth);

        foreach (var line in _renderer.RenderMany(visibleItems, GetUsableConsoleWidth(), new HashSet<int> { 0 }, maxColumns: 4, cellWidth: gridCellWidth))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedProgression(currentPhrase, chordIndex);
    }

    private static IEnumerable<(TriadPracticeItem PracticeItem, int Beats)> EnumerateRollingTriadItems(
        IReadOnlyList<TriadPracticeItem> currentPhrase,
        IReadOnlyList<TriadPracticeItem> nextPhrase,
        IReadOnlyList<int> chordLengths,
        int chordIndex)
    {
        for (var index = chordIndex; index < currentPhrase.Count; index++)
        {
            yield return (currentPhrase[index], chordLengths[index]);
        }

        for (var index = 0; index < nextPhrase.Count; index++)
        {
            yield return (nextPhrase[index], chordLengths[index]);
        }

        while (nextPhrase.Count > 0)
        {
            for (var index = 0; index < nextPhrase.Count; index++)
            {
                yield return (nextPhrase[index], chordLengths[index]);
            }
        }
    }

    private void RenderChordDiagramSongGame(
        string gameTitle,
        string title,
        IReadOnlyList<ChordSymbol> progression,
        IReadOnlyList<int> chordLengths,
        int chordIndex,
        int beatWithinChord,
        int beatWithinBar,
        TimeSignature timeSignature,
        int bpm,
        bool clickEnabled,
        bool backingEnabled,
        bool paused,
        Func<ChordSymbol, FretboardDiagram> buildDiagram)
    {
        var currentChord = progression[chordIndex];
        var uniqueChords = progression
            .Distinct()
            .ToArray();
        var currentDiagramIndex = Array.IndexOf(uniqueChords, currentChord);
        var diagrams = uniqueChords
            .Select(chord => (chord.DisplayName, buildDiagram(chord)))
            .ToArray();
        var cellWidth = diagrams.Max(_renderer.MeasureTitledDiagramWidth);

        Console.Clear();
        WriteHeader(gameTitle);
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine($"Space = pause, -/+ = tempo, M = mute click, S = mute backing, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
        Console.WriteLine();
        Console.WriteLine($"Now: {currentChord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        Console.WriteLine($"Labels: {Root}R{Reset} = root, {Third}3/b3{Reset} = third, {Fifth}5{Reset} = fifth, X = muted string");
        Console.WriteLine();

        foreach (var line in _renderer.RenderMany(diagrams, GetUsableConsoleWidth(), new HashSet<int> { currentDiagramIndex }, maxColumns: 4, cellWidth: cellWidth))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedProgression(progression, chordIndex);
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
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderScaleSongGame(setup.Title, keyRoot, scaleKind, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderScaleSongGame(setup.Title, keyRoot, scaleKind, progression, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
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

    private void ShowScaleLibrarySongGame()
    {
        var setup = ReadSongTriadProgressionSetup();
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

        var suggestions = SuggestScalesForProgression(progression);
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
                    RenderScaleLibrarySongGame(setup.Title, progression, suggestions, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
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
                                RenderScaleLibrarySongGame(setup.Title, progression, suggestions, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderScaleLibrarySongGame(setup.Title, progression, suggestions, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
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
                                RenderScaleLibrarySongGame(setup.Title, progression, suggestions, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderScaleLibrarySongGame(setup.Title, progression, suggestions, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderScaleLibrarySongGame(setup.Title, progression, suggestions, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmBackingChords(progression, chordLengths, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                RenderScaleLibrarySongGame(setup.Title, progression, suggestions, chordLengths, chordIndex, beatWithinChord, beatWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, paused);
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
        Console.WriteLine($"Space = pause, -/+ = tempo, M = mute click, S = mute backing, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
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
        Console.WriteLine($"Space = pause, -/+ = tempo, M = mute click, S = mute backing, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {currentChord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        Console.WriteLine($"Use the scale notes. Current chord tones are {Root}R{Reset}, {Third}3/b3{Reset}, {Fifth}5{Reset}; {Pentatonic}1{Reset} is the scale root.");
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

    private void RenderScaleLibrarySongGame(
        string title,
        IReadOnlyList<ChordSymbol> progression,
        IReadOnlyList<ScaleSuggestion> suggestions,
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
        var primary = suggestions[0];
        var romanNumerals = progression.Select(chord => RomanNumeral(chord, primary.KeyRoot)).ToArray();
        var scaleDiagrams = suggestions
            .Take(2)
            .Select(suggestion => ($"{suggestion.KeyRoot} {PentatonicLibrary.NameFor(suggestion.ScaleKind)}", pentatonics.BuildScaleWindow(suggestion.KeyRoot, suggestion.ScaleKind, startFret: 0, length: 25, currentChord)))
            .ToArray();

        Console.Clear();
        WriteHeader("Scale song suggester");
        Console.WriteLine($"Song: {title}");
        Console.WriteLine($"Best fit: {primary.KeyRoot} {PentatonicLibrary.NameFor(primary.ScaleKind)}  Current function: {Third}{romanNumerals[chordIndex]}{Reset}");
        Console.WriteLine($"BPM: {bpm}  Time: {timeSignature.DisplayName}  Lengths: {string.Join("-", chordLengths)}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine($"Space = pause, -/+ = tempo, M = mute click, S = mute backing, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
        Console.WriteLine();

        Console.WriteLine($"Now: {currentChord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}  bar beat {beatWithinBar + 1}/{timeSignature.BeatsPerBar}");
        Console.WriteLine($"Current chord tones are {Root}R{Reset}, {Third}3/b3{Reset}, {Fifth}5{Reset}; {Pentatonic}1{Reset} is the root of each suggested scale.");
        Console.WriteLine($"Also try: {string.Join("  |  ", suggestions.Skip(1).Take(3).Select(FormatScaleSuggestion))}");
        Console.WriteLine();

        foreach (var line in _renderer.RenderMany(scaleDiagrams, GetUsableConsoleWidth(), maxColumns: 1))
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

    private static IReadOnlyList<ScaleSuggestion> SuggestScalesForProgression(IReadOnlyList<ChordSymbol> progression)
    {
        var scaleKindOrder = PentatonicLibrary.ScaleKinds.ToArray();
        var keyCandidates = MusicTheory.ChromaticRoots
            .SelectMany(root => new[]
            {
                ScoreKeyCandidate(progression, root, minor: false),
                ScoreKeyCandidate(progression, root, minor: true)
            })
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.ChordMismatches)
            .ThenBy(candidate => candidate.Root == progression[0].Root ? 0 : 1)
            .Take(3)
            .ToArray();

        var suggestions = new List<ScaleSuggestion>();
        foreach (var candidate in keyCandidates)
        {
            var primaryKind = candidate.Minor ? PentatonicScaleKind.NaturalMinor : PentatonicScaleKind.MajorScale;
            var pentatonicKind = candidate.Minor ? PentatonicScaleKind.MinorPentatonic : PentatonicScaleKind.MajorPentatonic;
            var bluesKind = candidate.Minor ? PentatonicScaleKind.MinorBlues : PentatonicScaleKind.MajorBlues;

            suggestions.Add(new ScaleSuggestion(candidate.Root, primaryKind, candidate.Score, candidate.Reason));
            suggestions.Add(new ScaleSuggestion(candidate.Root, pentatonicKind, candidate.Score - 1, "Simpler box pattern for the same key"));
            suggestions.Add(new ScaleSuggestion(candidate.Root, bluesKind, candidate.Score - 2, "Adds blues color over the same key"));

            if (candidate.Minor)
            {
                suggestions.Add(new ScaleSuggestion(candidate.Root, PentatonicScaleKind.Dorian, candidate.Score - 3, "Brighter minor option when the progression wants a natural 6"));
            }
            else if (ProgressionContainsFlatSevenMajor(progression, candidate.Root))
            {
                suggestions.Add(new ScaleSuggestion(candidate.Root, PentatonicScaleKind.Mixolydian, candidate.Score - 3, "Good for major progressions with a bVII sound"));
            }
        }

        return suggestions
            .GroupBy(suggestion => (suggestion.KeyRoot, suggestion.ScaleKind))
            .Select(group => group.OrderByDescending(suggestion => suggestion.Score).First())
            .OrderByDescending(suggestion => suggestion.Score)
            .ThenBy(suggestion => Array.IndexOf(scaleKindOrder, suggestion.ScaleKind))
            .Take(6)
            .ToArray();
    }

    private static KeyCandidate ScoreKeyCandidate(IReadOnlyList<ChordSymbol> progression, string root, bool minor)
    {
        var score = 0;
        var mismatches = 0;

        for (var index = 0; index < progression.Count; index++)
        {
            var chord = progression[index];
            var interval = MusicTheory.Normalize(MusicTheory.PitchClassFor(chord.Root) - MusicTheory.PitchClassFor(root));
            var expectedQuality = QualityForDiatonicTriad(interval, minor);

            if (expectedQuality == chord.Quality)
            {
                score += index == 0 ? 5 : 3;
            }
            else if (expectedQuality is not null)
            {
                score += 1;
                mismatches++;
            }
            else
            {
                score -= 2;
                mismatches++;
            }
        }

        if (progression[0].Root == root && progression[0].Quality == (minor ? ChordQuality.Minor : ChordQuality.Major))
        {
            score += 4;
        }

        var modeName = minor ? "minor" : "major";
        var reason = mismatches == 0
            ? $"All chords fit {root} {modeName}"
            : $"{progression.Count - mismatches}/{progression.Count} chords fit {root} {modeName}";

        return new KeyCandidate(root, minor, score, mismatches, reason);
    }

    private static ChordQuality? QualityForDiatonicTriad(int interval, bool minor)
    {
        return minor
            ? interval switch
            {
                0 => ChordQuality.Minor,
                3 => ChordQuality.Major,
                5 => ChordQuality.Minor,
                7 => ChordQuality.Minor,
                8 => ChordQuality.Major,
                10 => ChordQuality.Major,
                _ => null
            }
            : interval switch
            {
                0 => ChordQuality.Major,
                2 => ChordQuality.Minor,
                4 => ChordQuality.Minor,
                5 => ChordQuality.Major,
                7 => ChordQuality.Major,
                9 => ChordQuality.Minor,
                _ => null
            };
    }

    private static bool ProgressionContainsFlatSevenMajor(IReadOnlyList<ChordSymbol> progression, string keyRoot)
    {
        var keyPitch = MusicTheory.PitchClassFor(keyRoot);
        return progression.Any(chord =>
            chord.Quality == ChordQuality.Major &&
            MusicTheory.Normalize(MusicTheory.PitchClassFor(chord.Root) - keyPitch) == 10);
    }

    private static string FormatScaleSuggestion(ScaleSuggestion suggestion)
    {
        return $"{suggestion.KeyRoot} {PentatonicLibrary.NameFor(suggestion.ScaleKind)}";
    }

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

    private static int AdjustBpm(int bpm, int delta) => Math.Clamp(bpm + delta, 30, 240);

    private static string Pluralize(int count) => count == 1 ? string.Empty : "s";

    private static string DisplayNameFor(TriadGameLayout layout) => layout switch
    {
        TriadGameLayout.PhraseRows => "phrase rows",
        TriadGameLayout.RollingNextThree => "next 3",
        _ => layout.ToString()
    };

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

    private static void WriteMenuOption(string key, string title, string description)
    {
        Console.WriteLine($"{key}. {title}");
        Console.WriteLine($"   {description}");
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
