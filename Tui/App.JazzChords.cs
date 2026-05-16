using GuitarResourcesTui.Fretboards;
using GuitarResourcesTui.JazzChords;
using GuitarResourcesTui.Triads;
using GuitarResourcesTui.Tui.Printing;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private readonly JazzChordLibrary _jazzChords = new();

    private void ShowJazzMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz");
            WriteDescriptionLine("Jazz guitar is rhythm, harmony, melody, time feel, and language working together.");
            WriteDescriptionLine("Start with the roadmap and terms if any of the names feel mysterious.");
            Console.WriteLine();
            WriteMenuOption("1", "Jazz roadmap", "See the learning path from basic chord tones to comping, standards, and solos.");
            WriteMenuOption("2", "Jazz terms", "Plain-English meanings for comping, changes, heads, choruses, guide tones, shells, and more.");
            WriteMenuOption("3", "Chords and voicings", "Learn chord symbols, guide tones, shell voicings, inversions, and fretboard shapes.");
            WriteMenuOption("4", "Comping and rhythm", "Understand swing feel, time, space, four-to-the-bar, Charleston, and la pompe.");
            WriteMenuOption("5", "ii-V-I trainer", "Drill jazz's most common progression in major and minor keys.");
            WriteMenuOption("6", "Guide-tone soloing", "Use the 3rds and 7ths as melodic targets through changing chords.");
            WriteMenuOption("7", "Standards library", "Apply jazz vocabulary to standards-style progressions with concept tags.");
            WriteMenuOption("8", "Django / gypsy jazz path", "A route toward Reinhardt-style rhythm, arpeggios, chromaticism, and swing phrasing.");
            WriteMenuOption("9", "Random jazz workout", "Get a fresh connected jazz progression to practise with audio.");
            WriteMenuOption("10", "Printable jazz sheets", "Open printable voicing, ii-V-I, and standards study worksheets.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzRoadmap();
                    break;
                case "2":
                    ShowJazzTerms();
                    break;
                case "3":
                    ShowJazzChordsAndVoicingsMenu();
                    break;
                case "4":
                    ShowJazzCompingAndRhythm();
                    break;
                case "5":
                    ShowTwoFiveOneTrainer();
                    break;
                case "6":
                    ShowGuideToneSoloing();
                    break;
                case "7":
                    ShowJazzStandardsLibrary();
                    break;
                case "8":
                    ShowDjangoJazzPath();
                    break;
                case "9":
                    ShowRandomJazzChordGame();
                    break;
                case "10":
                    ShowPrintableJazzSheets();
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

    private void ShowJazzChordsAndVoicingsMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz chords and voicings");
            WriteDescriptionLine("Jazz chords still use triads, but the sound usually comes from adding 6ths, 7ths, 9ths, 11ths, and 13ths.");
            WriteDescriptionLine("Jazz guitarists use inversions too; in practice you will usually hear the word voicing for a chosen arrangement of the notes.");
            Console.WriteLine();
            WriteMenuOption("1", "Fundamentals path", "A guided order: formulas, guide tones, shells, ii-V-I, then standards.");
            WriteMenuOption("2", "Chord type list", "See what symbols like maj7, m7b5, 9, and 13 mean.");
            WriteMenuOption("3", "Voicing reference", "Find playable guitar shapes for guide tones, shells, or full chords.");
            WriteMenuOption("4", "Random chord workout", "Get a fresh connected jazz progression to practise with audio.");
            WriteMenuOption("5", "Printable voicing sheet", "Print one root/type/mode as a focused practice page.");
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
                    ShowRandomJazzChordGame();
                    break;
                case "5":
                    PrintJazzVoicingSheet();
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

    private void ShowJazzRoadmap()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz roadmap");
            WriteDescriptionLine("The end goal is simple to say and deep to play: keep time, support the tune, hear the changes, and make melodies from chord tones.");
            Console.WriteLine();
            WriteJazzStudyStage("1. Map the harmony", "Learn maj7, 6, m7, dominant 7, m7b5, dim7, and altered dominants. These are the raw materials.");
            WriteJazzStudyStage("2. Hear guide tones", "The 3rd and 7th tell you whether a chord is major, minor, or dominant, and they move smoothly between chords.");
            WriteJazzStudyStage("3. Build shell voicings", "Add the root to the guide tones so you can comp with small, clear shapes that do not fight a bass player or singer.");
            WriteJazzStudyStage("4. Internalize ii-V-I", "Most standards contain ii-V-I movement. In C major that means Dm7 -> G7 -> Cmaj7.");
            WriteJazzStudyStage("5. Learn comping rhythms", "Play fewer notes with better time: swing quarter notes, Charleston figures, anticipations, and tasteful gaps.");
            WriteJazzStudyStage("6. Learn standards", "Memorize form, melody, and changes. A standard is where isolated theory becomes real music.");
            WriteJazzStudyStage("7. Solo through changes", "Aim arpeggios and guide tones at strong beats, then add approach notes, enclosures, and rhythmic phrasing.");
            WriteJazzStudyStage("8. Add style language", "For a Reinhardt-flavored route, study la pompe, minor 6 sounds, diminished passing chords, rest-stroke picking, and chromatic arpeggios.");
            Console.WriteLine();
            WriteMenuOption("1", "Open chord fundamentals", "Start the harmony lane.");
            WriteMenuOption("2", "Open comping and rhythm", "Start the rhythm lane.");
            WriteMenuOption("3", "Open guide-tone soloing", "Start the soloing lane.");
            WriteMenuOption("4", "Open standards library", "Apply the roadmap to tune-style progressions.");
            WriteMenuOption("5", "Print jazz roadmap", "Create a printable overview of the whole learning route.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a lane > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzChordsAndVoicingsMenu();
                    break;
                case "2":
                    ShowJazzCompingAndRhythm();
                    break;
                case "3":
                    ShowGuideToneSoloing();
                    break;
                case "4":
                    ShowJazzStandardsLibrary();
                    break;
                case "5":
                    PrintJazzRoadmapSheet();
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

    private static void ShowJazzTerms()
    {
        Console.Clear();
        WriteHeader("Jazz terms");
        WriteDescriptionLine("A small vocabulary unlocks a lot of jazz conversations. These are the terms the app uses most.");
        Console.WriteLine();
        WriteJazzTerm("Changes", "The chord progression of a tune. To solo over changes means your line follows the current chord.");
        WriteJazzTerm("Standard", "A tune that lots of jazz players know, so it becomes shared repertoire for jams and study.");
        WriteJazzTerm("Head", "The composed melody of the tune, usually played before and after improvised solos.");
        WriteJazzTerm("Chorus", "One full pass through the form. If the tune is 32 bars, one solo chorus is 32 bars.");
        WriteJazzTerm("Form", "The map of the tune, such as AABA, ABAC, or 12-bar blues.");
        WriteJazzTerm("Swing", "A feel where eighth notes are uneven and the pulse has lift. The exact amount changes with tempo and style.");
        WriteJazzTerm("Comping", "Accompanying with chords and rhythm. The job is to support the melody or soloist, not fill every gap.");
        WriteJazzTerm("ii-V-I", "A progression from scale degree 2 to 5 to 1. In C major: Dm7 -> G7 -> Cmaj7.");
        WriteJazzTerm("Guide tones", "The chord tones that best reveal the harmony, usually the 3rd and 7th of a seventh chord.");
        WriteJazzTerm("Shell voicing", "A small chord shape containing the root plus essential tones, usually the 3rd and 7th.");
        WriteJazzTerm("Voicing", "The exact arrangement of chord notes on the instrument. Inversions are one kind of voicing.");
        WriteJazzTerm("Rootless voicing", "A chord shape that leaves out the root, common when bass or piano already covers it.");
        WriteJazzTerm("Drop 2", "A common jazz-guitar voicing family made by dropping the second-highest note of a close chord down an octave.");
        WriteJazzTerm("Voice leading", "Moving from chord to chord with small note movements instead of jumping around the neck.");
        WriteJazzTerm("Turnaround", "A short progression that leads back to the top of the form, often I-vi-ii-V or a variation.");
        WriteJazzTerm("Tritone substitution", "Replacing a dominant chord with another dominant chord a tritone away, such as Db7 for G7.");
        WriteJazzTerm("Altered dominant", "A dominant chord with tense colors like b9, #9, b5, or #5 that wants to resolve.");
        WriteJazzTerm("Approach note", "A note just above or below a target note, used to make a line lead into the chord tone.");
        WriteJazzTerm("Enclosure", "Surrounding a target note from above and below before landing on it.");
        WriteJazzTerm("La pompe", "The driving gypsy-jazz rhythm-guitar style: a percussive bass/chord pulse that makes the band swing.");
        Console.WriteLine();
        WriteDescriptionLine("Press any key to go back.");
        Console.ReadKey(intercept: true);
    }

    private void ShowJazzCompingAndRhythm()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Comping and rhythm");
            WriteDescriptionLine("Comping means accompanying: you create a rhythmic chord part that supports the tune, soloist, and time feel.");
            WriteDescriptionLine("Good comping is not about playing every possible chord. It is about time, taste, voice leading, and leaving room.");
            Console.WriteLine();
            WriteJazzStudyStage("Swing quarter notes", "Count 1 2 3 4 with a relaxed pulse. In a walking-bass setting, guitar often uses short, light four-to-the-bar chords.");
            WriteJazzStudyStage("Charleston", "A classic two-hit rhythm: beat 1, then the and of 2. Move it around the bar once it feels natural.");
            WriteJazzStudyStage("Anticipation", "Place a chord on the and of 4 to push into the next bar. This is common before a chord change.");
            WriteJazzStudyStage("Space", "One strong chord can say more than four nervous ones. Leave room for melody, bass, drums, and silence.");
            WriteJazzStudyStage("Voice-led comping", "Keep shared notes where possible and move guide tones by half-step or whole-step between chords.");
            WriteJazzStudyStage("La pompe", "For gypsy jazz, think percussive quarter-note drive: bassy downstroke, quick chord release, then a snappy chord stroke.");
            WriteJazzStudyStage("Rhythm practice loop", "Mute the backing, put the click on beats 2 and 4 in your head, and play the same two shell voicings until the groove feels easy.");
            Console.WriteLine();
            WriteMenuOption("1", "Comping rhythm trainer", "Follow a rhythm grid over shell voicings with click and backing audio.");
            WriteMenuOption("2", "Shell voicing ii-V-I", "Practise small comping grips through jazz's main cadence.");
            WriteMenuOption("3", "Standards library", "Apply comping to tune-style forms.");
            WriteMenuOption("4", "Random jazz workout", "Practise reacting to a fresh progression.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a practice route > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzCompingRhythmTrainer();
                    break;
                case "2":
                    ShowJazzChordGame("Shell voicing comping ii-V-I", BuildMajorTwoFiveOneSetup("C"), JazzVoicingMode.Shell);
                    break;
                case "3":
                    ShowJazzStandardsLibrary();
                    break;
                case "4":
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

    private void ShowGuideToneSoloing()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Guide-tone soloing");
            WriteDescriptionLine("Guide-tone soloing means building melodies around the notes that reveal the chord: usually the 3rd and 7th.");
            WriteDescriptionLine("If a line lands on the current chord's 3rd or 7th on a strong beat, it starts to sound like it knows the changes.");
            Console.WriteLine();
            WriteJazzStudyStage("Target first", "For each chord, find the 3rd and 7th. Sing them, play them, then connect them smoothly.");
            WriteJazzStudyStage("Add rhythm", "A plain guide-tone line becomes music when the rhythm swings and breathes.");
            WriteJazzStudyStage("Add arpeggios", "Use root, 3rd, 5th, and 7th to outline the chord more fully.");
            WriteJazzStudyStage("Add approaches", "Step into a target note from one fret above or below.");
            WriteJazzStudyStage("Add enclosures", "Play above-target-below-target, or below-target-above-target, then resolve.");
            WriteJazzStudyStage("Resolve tension", "Altered dominant notes such as b9 or #9 sound best when they clearly resolve into the next chord.");
            Console.WriteLine();
            WriteMenuOption("1", "Guide-tone line builder", "Get target-note prompts with direct, approach, and enclosure exercises.");
            WriteMenuOption("2", "Major ii-V-I guide tones", "See the essential 3rds and 7ths through Dm7 -> G7 -> Cmaj7.");
            WriteMenuOption("3", "Minor ii-V-i guide tones", "Hear the darker m7b5 -> 7b9 -> minor tonic path.");
            WriteMenuOption("4", "Arpeggio section", "Use arpeggio drills as the next layer after guide tones.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a practice route > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzLineBuilder();
                    break;
                case "2":
                    ShowJazzChordGame("Guide-tone soloing: major ii-V-I", BuildMajorTwoFiveOneSetup("C"), JazzVoicingMode.GuideTones);
                    break;
                case "3":
                    ShowJazzChordGame("Guide-tone soloing: minor ii-V-i", BuildMinorTwoFiveOneSetup("C"), JazzVoicingMode.GuideTones);
                    break;
                case "4":
                    ShowArpeggiosMenu();
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

    private void ShowDjangoJazzPath()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Django / gypsy jazz path");
            WriteDescriptionLine("Django Reinhardt's style is built from fierce swing time, chord-tone melody, arpeggios, chromatic approaches, and a strong acoustic rhythm engine.");
            WriteDescriptionLine("This path uses the app's existing jazz and arpeggio drills as stepping stones toward that sound.");
            Console.WriteLine();
            WriteJazzStudyStage("1. La pompe rhythm", "Make the rhythm feel like a drum kit: short, percussive, buoyant quarter notes with a strong 2 and 4.");
            WriteJazzStudyStage("2. Minor 6 and dominant colors", "Gypsy jazz leans heavily on m6, 6/9, 9, 13, dim7, and altered dominant sounds.");
            WriteJazzStudyStage("3. Arpeggio-first soloing", "Outline the chord before adding scale notes. Django-style lines often sound like moving chord shapes.");
            WriteJazzStudyStage("4. Chromatic approaches", "Slide or step into chord tones from above or below to create that slippery jazz pull.");
            WriteJazzStudyStage("5. Diminished passing sounds", "Use dim7 shapes to connect dominant chords and add tension.");
            WriteJazzStudyStage("6. Rest-stroke picking mindset", "For the authentic acoustic attack, study rest-stroke picking slowly and keep the hand relaxed.");
            WriteJazzStudyStage("7. Repertoire", "Learn standard forms such as minor-key swings, rhythm changes, and ballads so the language has somewhere to live.");
            Console.WriteLine();
            WriteMenuOption("1", "La pompe rhythm trainer", "Comp a minor-swing progression with a gypsy-jazz quarter-note drive.");
            WriteMenuOption("2", "Minor ii-V-i trainer", "Practise the darker harmony that appears often in this style.");
            WriteMenuOption("3", "Chromatic line builder", "Target chord tones with approaches and enclosures.");
            WriteMenuOption("4", "Diminished passing drill", "Practise dim7 and 7b9 colors resolving into minor tonic sounds.");
            WriteMenuOption("5", "Standards library", "Pick a tune-style progression and comp it with shell voicings.");
            WriteMenuOption("6", "Arpeggio section", "Drill the chord-tone shapes used in solo lines.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a practice route > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzCompingRhythmTrainer(JazzRhythmPatterns.Single(pattern => pattern.Name == "La pompe"), BuildDjangoMinorSwingSetup());
                    break;
                case "2":
                    ShowJazzChordGame("Django path: minor ii-V-i shells", BuildMinorTwoFiveOneSetup("A"), JazzVoicingMode.Shell);
                    break;
                case "3":
                    ShowJazzLineBuilder(JazzSoloExerciseKind.Enclosure, BuildDjangoMinorSwingSetup());
                    break;
                case "4":
                    ShowJazzChordGame("Django path: diminished passing colors", BuildDjangoDiminishedSetup(), JazzVoicingMode.Full);
                    break;
                case "5":
                    ShowJazzStandardsLibrary();
                    break;
                case "6":
                    ShowArpeggiosMenu();
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

    private static void WriteJazzStudyStage(string title, string description)
    {
        Console.WriteLine(title);
        WriteDescriptionLine($"  {description}");
    }

    private static void WriteJazzTerm(string term, string description)
    {
        Console.Write($"{term.PadRight(20)} ");
        WriteDescriptionLine(description);
    }

    private static readonly IReadOnlyList<JazzRhythmPattern> JazzRhythmPatterns =
    [
        new(
            "Four to the bar",
            "Short quarter-note chords on every beat. Think Freddie Green: light, steady, and never heavy.",
            ["1", "&", "2", "&", "3", "&", "4", "&"],
            new HashSet<int> { 0, 2, 4, 6 },
            new HashSet<int>(),
            "Play short chords on 1, 2, 3, and 4. Release each chord quickly so the rhythm breathes."),
        new(
            "Charleston",
            "A classic two-hit comping cell: beat 1, then the and of 2.",
            ["1", "&", "2", "&", "3", "&", "4", "&"],
            new HashSet<int> { 0, 3 },
            new HashSet<int> { 3 },
            "Play beat 1, rest through beat 2, then pop the and of 2. Keep counting through the silence."),
        new(
            "Anticipation",
            "A push into the next bar: beat 1, beat 3, then the and of 4.",
            ["1", "&", "2", "&", "3", "&", "4", "&"],
            new HashSet<int> { 0, 4, 7 },
            new HashSet<int> { 7 },
            "The and of 4 should feel like it leans into the next chord without rushing."),
        new(
            "Sparse answers",
            "Small answers around the soloist: one hit on 2, one hit on the and of 4.",
            ["1", "&", "2", "&", "3", "&", "4", "&"],
            new HashSet<int> { 2, 7 },
            new HashSet<int> { 2 },
            "Leave the empty beats empty. This drill is about patience and time feel."),
        new(
            "La pompe",
            "Gypsy-jazz rhythm is a quarter-note engine, but the feel is in the attack: lighter bassy strokes on 1 and 3, snapped chord accents on 2 and 4.",
            ["1", "&", "2", "&", "3", "&", "4", "&"],
            new HashSet<int> { 0, 2, 4, 6 },
            new HashSet<int> { 2, 6 },
            "Do not let the chords ring. Think boom-chuck-boom-chuck: touch 1 and 3 slightly lower/bassier, then release a sharper chord on 2 and 4.")
    ];

    private void ShowJazzCompingRhythmTrainer(JazzRhythmPattern? selectedPattern = null, TriadProgressionSetup? selectedSetup = null)
    {
        var pattern = selectedPattern ?? ReadJazzRhythmPattern();
        if (pattern is null)
        {
            return;
        }

        var setup = selectedSetup ?? ReadJazzPracticeSetup("Choose changes for the comping trainer.");
        if (setup is null)
        {
            return;
        }

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

        var bpm = setup.Bpm ?? ReadInt("BPM", defaultValue: 100, min: 30, max: 240);
        var timeSignature = setup.TimeSignature ?? new TimeSignature(4, 4);
        var chordLengths = setup.ChordLengths ?? ReadJazzChordLengths(progression, timeSignature);
        var phrase = _jazzChords.BuildPhrase(progression, voicingMode: JazzVoicingMode.Shell);
        var subdivision = 0;
        var paused = false;
        var clickEnabled = true;
        var backingEnabled = true;
        var exampleEnabled = false;
        var lastSynthChordIndex = -1;
        var lastExampleChordIndex = -1;
        var lastRenderedSubdivision = -1;
        var phraseSubdivisions = chordLengths.Sum() * 2;
        var synthProcesses = new List<AudioPlayback>();
        WarmJazzBackingChords(progression, chordLengths, bpm, timeSignature);
        WarmJazzCompExamples(progression, chordLengths, pattern, bpm, timeSignature);

        try
        {
            while (true)
            {
                var subdivisionStartedAt = DateTime.UtcNow;
                var beat = subdivision / 2;
                var chordIndex = ChordIndexAtBeat(chordLengths, beat);
                var beatWithinChord = beat - StartBeatForChord(chordLengths, chordIndex);
                var beatWithinBar = beat % timeSignature.BeatsPerBar;
                var subdivisionWithinBar = beatWithinBar * 2 + subdivision % 2;

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

                if (!paused && exampleEnabled && chordIndex != lastExampleChordIndex)
                {
                    CleanupFinishedProcesses(synthProcesses);
                    var exampleProcess = PlayJazzCompExample(progression[chordIndex], chordLengths[chordIndex], pattern, bpm, timeSignature);
                    if (exampleProcess is not null)
                    {
                        synthProcesses.Add(exampleProcess);
                        Thread.Sleep(35);
                        subdivisionStartedAt = DateTime.UtcNow;
                    }

                    lastExampleChordIndex = chordIndex;
                }

                if (subdivision != lastRenderedSubdivision)
                {
                    RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                    lastRenderedSubdivision = subdivision;
                }

                if (!paused && clickEnabled && subdivision % 2 == 0)
                {
                    Click(beatWithinBar == 0);
                }

                var deadline = subdivisionStartedAt + TimeSpan.FromMinutes(0.5d / bpm);
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
                                    lastExampleChordIndex = -1;
                                }
                                RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                                break;
                            case ConsoleKey.M:
                                clickEnabled = !clickEnabled;
                                RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                                break;
                            case ConsoleKey.S:
                                backingEnabled = !backingEnabled;
                                if (!backingEnabled)
                                {
                                    StopProcesses(synthProcesses);
                                }
                                else
                                {
                                    exampleEnabled = false;
                                    lastSynthChordIndex = -1;
                                }
                                RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                                break;
                            case ConsoleKey.E:
                                exampleEnabled = !exampleEnabled;
                                if (!exampleEnabled)
                                {
                                    StopProcesses(synthProcesses);
                                    lastSynthChordIndex = -1;
                                }
                                else
                                {
                                    backingEnabled = false;
                                    StopProcesses(synthProcesses);
                                    lastExampleChordIndex = -1;
                                }
                                RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                                break;
                            case ConsoleKey.L:
                                ToggleNoteLabelMode();
                                RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                                break;
                            case ConsoleKey.OemMinus:
                            case ConsoleKey.Subtract:
                                bpm = AdjustBpm(bpm, -5);
                                StopProcesses(synthProcesses);
                                WarmJazzBackingChords(progression, chordLengths, bpm, timeSignature);
                                WarmJazzCompExamples(progression, chordLengths, pattern, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                lastExampleChordIndex = -1;
                                RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                                break;
                            case ConsoleKey.OemPlus:
                            case ConsoleKey.Add:
                                bpm = AdjustBpm(bpm, 5);
                                StopProcesses(synthProcesses);
                                WarmJazzBackingChords(progression, chordLengths, bpm, timeSignature);
                                WarmJazzCompExamples(progression, chordLengths, pattern, bpm, timeSignature);
                                lastSynthChordIndex = -1;
                                lastExampleChordIndex = -1;
                                RenderJazzCompingRhythmTrainer(setup.Title, pattern, phrase, chordLengths, chordIndex, beatWithinChord, subdivisionWithinBar, timeSignature, bpm, clickEnabled, backingEnabled, exampleEnabled, paused);
                                break;
                            case ConsoleKey.N:
                            case ConsoleKey.RightArrow:
                                subdivision = StartBeatForChord(chordLengths, chordIndex + 1) * 2;
                                goto SubdivisionAdvancedManually;
                        }
                    }

                    Thread.Sleep(5);
                }

                if (!paused)
                {
                    subdivision++;
                }

            SubdivisionAdvancedManually:
                if (subdivision >= phraseSubdivisions)
                {
                    subdivision = 0;
                    lastSynthChordIndex = -1;
                    lastExampleChordIndex = -1;
                    lastRenderedSubdivision = -1;
                }
            }
        }
        finally
        {
            StopProcesses(synthProcesses);
        }
    }

    private void RenderJazzCompingRhythmTrainer(
        string title,
        JazzRhythmPattern pattern,
        IReadOnlyList<JazzPracticeItem> phrase,
        IReadOnlyList<int> chordLengths,
        int chordIndex,
        int beatWithinChord,
        int subdivisionWithinBar,
        TimeSignature timeSignature,
        int bpm,
        bool clickEnabled,
        bool backingEnabled,
        bool exampleEnabled,
        bool paused)
    {
        var current = phrase[chordIndex];
        var currentStep = subdivisionWithinBar % pattern.Counts.Count;

        Console.Clear();
        WriteHeader("Comping rhythm trainer");
        Console.WriteLine($"Study: {title}");
        Console.WriteLine($"Pattern: {pattern.Name}  BPM: {bpm}  Time: {timeSignature.DisplayName}  Click: {(clickEnabled ? "on" : "muted")}  Backing: {(backingEnabled ? "on" : "muted")}  Example: {(exampleEnabled ? "on" : "muted")}  {(paused ? "Paused" : "Playing")}");
        Console.WriteLine($"Space = pause, -/+ = tempo, M = mute click, S = mute backing, E = example guitar, {NoteLabelCommandText()}, N = next chord, B/Q = main menu");
        Console.WriteLine();
        WriteDescriptionLine(pattern.Description);
        WriteDescriptionLine(pattern.Instruction);
        if (pattern.Name == "La pompe")
        {
            WriteDescriptionLine("So yes, it is counted 1 2 3 4. The jazz part is the percussive short-long-feel: boom on 1, CHUCK on 2, boom on 3, CHUCK on 4.");
        }
        if (exampleEnabled)
        {
            WriteDescriptionLine("Example mode solos the rhythm guitar and mutes the backing so you can hear the articulation clearly.");
        }
        Console.WriteLine();
        Console.WriteLine($"Now: {current.Chord.DisplayName}  chord beat {beatWithinChord + 1}/{chordLengths[chordIndex]}");
        Console.WriteLine(JazzChordLibrary.TeachingHintFor(current.Chord, JazzVoicingMode.Shell));
        Console.WriteLine();
        WriteJazzRhythmGrid(pattern, currentStep);
        Console.WriteLine();

        foreach (var line in _renderer.Render(current.Diagram))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedJazzProgression(phrase.Select(item => item.Chord).ToArray(), chordIndex);
    }

    private static void WriteJazzRhythmGrid(JazzRhythmPattern pattern, int currentStep)
    {
        Console.Write("Count: ");
        for (var index = 0; index < pattern.Counts.Count; index++)
        {
            var value = pattern.Counts[index].PadLeft(2).PadRight(4);
            Console.Write(index == currentStep ? $"{Third}{value}{Reset}" : value);
        }

        Console.WriteLine();
        Console.Write("Play:  ");
        for (var index = 0; index < pattern.Counts.Count; index++)
        {
            var hit = pattern.HitIndexes.Contains(index)
                ? pattern.AccentIndexes.Contains(index) ? "X!" : "x"
                : ".";
            var value = hit.PadLeft(2).PadRight(4);
            Console.Write(index == currentStep ? $"{Third}{value}{Reset}" : value);
        }

        Console.WriteLine();
        WriteDescriptionLine("x = short chord, X! = accented/snapped chord, . = leave space. Count every subdivision even when you rest.");
    }

    private static JazzRhythmPattern? ReadJazzRhythmPattern()
    {
        var choice = ReadMenuChoice(
            "Choose a comping rhythm",
            JazzRhythmPatterns.Select(pattern => pattern.Name).ToArray(),
            allowBack: true);
        return choice is null ? null : JazzRhythmPatterns.Single(pattern => pattern.Name == choice);
    }

    private void ShowJazzLineBuilder(JazzSoloExerciseKind? selectedKind = null, TriadProgressionSetup? selectedSetup = null)
    {
        var exerciseKind = selectedKind ?? ReadJazzSoloExerciseKind();
        if (exerciseKind is null)
        {
            return;
        }

        var setup = selectedSetup ?? ReadJazzPracticeSetup("Choose changes for the line builder.");
        if (setup is null)
        {
            return;
        }

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

        var promptIndex = 0;
        var prompt = BuildJazzSoloPrompt(progression[promptIndex], exerciseKind.Value);
        AudioPlayback? playback = null;

        try
        {
            playback = PlayBackingChord(prompt.Chord, 4, setup.Bpm ?? 90, setup.TimeSignature ?? new TimeSignature(4, 4));

            while (true)
            {
                var voicingMode = exerciseKind is JazzSoloExerciseKind.ArpeggioOutline ? JazzVoicingMode.Full : JazzVoicingMode.GuideTones;
                var item = _jazzChords.BuildPhrase([prompt.Chord], voicingMode: voicingMode)[0];

                Console.Clear();
                WriteHeader("Guide-tone line builder");
                Console.WriteLine($"Study: {setup.Title}");
                Console.WriteLine($"Exercise: {DisplayNameFor(exerciseKind.Value)}");
                Console.WriteLine($"C = hear chord, T = hear target, N = next chord, {NoteLabelCommandText()}, B/Q = back");
                Console.WriteLine();
                Console.WriteLine($"Chord: {prompt.Chord.DisplayName} ({prompt.Chord.Quality.Name})");
                Console.WriteLine($"Target: {Third}{prompt.TargetInterval}{Reset} = {prompt.TargetNote}");
                WriteDescriptionLine(prompt.Instruction);
                WriteDescriptionLine(prompt.PracticeText);
                Console.WriteLine();

                foreach (var line in _renderer.Render(item.Diagram))
                {
                    Console.WriteLine(line);
                }

                Console.WriteLine();
                WriteCenteredHighlightedJazzProgression(progression, promptIndex);
                Console.WriteLine();
                Console.Write("Command > ");

                switch (Console.ReadKey(intercept: true).Key)
                {
                    case ConsoleKey.C:
                        playback = ReplacePlayback(playback, PlayBackingChord(prompt.Chord, 4, setup.Bpm ?? 90, setup.TimeSignature ?? new TimeSignature(4, 4)));
                        break;
                    case ConsoleKey.T:
                        playback = ReplacePlayback(playback, PlayTunerNote(TargetTunerNote(prompt.Chord, prompt.TargetInterval)));
                        break;
                    case ConsoleKey.N:
                    case ConsoleKey.RightArrow:
                        promptIndex = (promptIndex + 1) % progression.Count;
                        prompt = BuildJazzSoloPrompt(progression[promptIndex], exerciseKind.Value);
                        playback = ReplacePlayback(playback, PlayBackingChord(prompt.Chord, 4, setup.Bpm ?? 90, setup.TimeSignature ?? new TimeSignature(4, 4)));
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

    private static JazzSoloExerciseKind? ReadJazzSoloExerciseKind()
    {
        Console.WriteLine();
        WriteMenuOption("1", "Direct guide tones", "Land plainly on the 3rd or 7th so you can hear the changes.");
        WriteMenuOption("2", "Approach notes", "Step into a target from one fret above or below.");
        WriteMenuOption("3", "Enclosures", "Surround a target from above and below before resolving.");
        WriteMenuOption("4", "Arpeggio outline", "Use any chord tone as a strong melodic landing point.");
        Console.WriteLine("B. Back");
        Console.Write("Choose a soloing drill > ");

        while (true)
        {
            switch ((Console.ReadLine()?.Trim() ?? string.Empty).ToUpperInvariant())
            {
                case "1":
                    return JazzSoloExerciseKind.Direct;
                case "2":
                    return JazzSoloExerciseKind.Approach;
                case "3":
                    return JazzSoloExerciseKind.Enclosure;
                case "4":
                    return JazzSoloExerciseKind.ArpeggioOutline;
                case "B":
                case "Q":
                    return null;
                default:
                    Console.Write("Choose 1, 2, 3, 4, or B > ");
                    break;
            }
        }
    }

    private static JazzSoloPrompt BuildJazzSoloPrompt(JazzChordSymbol chord, JazzSoloExerciseKind exerciseKind)
    {
        var targetIntervals = exerciseKind is JazzSoloExerciseKind.ArpeggioOutline
            ? chord.Quality.Intervals
            : JazzChordLibrary.IntervalsFor(chord.Quality, JazzVoicingMode.GuideTones);
        var targetInterval = targetIntervals[Random.Shared.Next(targetIntervals.Count)];
        var targetNote = MusicTheory.NameForInterval(chord.Root, targetInterval);
        var instruction = exerciseKind switch
        {
            JazzSoloExerciseKind.Direct => "Play the target note cleanly on beat 1, then make a tiny phrase that returns to it.",
            JazzSoloExerciseKind.Approach => "Approach the target from one fret above or below, then resolve into it on a strong beat.",
            JazzSoloExerciseKind.Enclosure => "Play one note above the target, one note below the target, then land on the target.",
            _ => "Outline the chord with two or three chord tones, then land on the target."
        };
        var practiceText = exerciseKind switch
        {
            JazzSoloExerciseKind.Direct => "Say the chord name out loud, sing the target, then play it in two places.",
            JazzSoloExerciseKind.Approach => "Keep the approach quiet and make the target sound like the destination.",
            JazzSoloExerciseKind.Enclosure => "Do not rush the enclosure. The resolution is the point.",
            _ => "Start with only chord tones. Add passing notes after the harmony sounds clear."
        };

        return new JazzSoloPrompt(chord, targetInterval, targetNote, instruction, practiceText);
    }

    private static string DisplayNameFor(JazzSoloExerciseKind kind) => kind switch
    {
        JazzSoloExerciseKind.Direct => "direct guide tones",
        JazzSoloExerciseKind.Approach => "approach notes",
        JazzSoloExerciseKind.Enclosure => "enclosures",
        JazzSoloExerciseKind.ArpeggioOutline => "arpeggio outline",
        _ => kind.ToString()
    };

    private static TunerNote TargetTunerNote(JazzChordSymbol chord, string interval)
    {
        var targetNote = MusicTheory.NameForInterval(chord.Root, interval);
        var midiNote = 60 + MusicTheory.Normalize(MusicTheory.PitchClassFor(chord.Root) + JazzChordLibrary.SemitonesFor(interval));
        return new TunerNote(string.Empty, $"{targetNote} target note", $"{targetNote}4", midiNote);
    }

    private TriadProgressionSetup? ReadJazzPracticeSetup(string prompt)
    {
        WriteDescriptionLine(prompt);
        Console.WriteLine();
        WriteMenuOption("1", "Major ii-V-I", "Use Dm7 G9 Cmaj7 in C.");
        WriteMenuOption("2", "Minor ii-V-i", "Use Dm7b5 G7b9 Cm6 in C minor.");
        WriteMenuOption("3", "Standards library", "Choose a tune-style study progression.");
        WriteMenuOption("4", "Random jazz workout", "Let the app create connected changes.");
        WriteMenuOption("C", "Custom changes", "Type your own jazz chords, such as Dm7 G7 Cmaj7.");
        Console.WriteLine("B. Back");
        Console.Write("Choose changes > ");

        while (true)
        {
            switch ((Console.ReadLine()?.Trim() ?? string.Empty).ToUpperInvariant())
            {
                case "1":
                    return BuildMajorTwoFiveOneSetup("C");
                case "2":
                    return BuildMinorTwoFiveOneSetup("C");
                case "3":
                    return ReadJazzSongSetup();
                case "4":
                    return BuildRandomJazzProgressionSetup();
                case "C":
                    Console.WriteLine("Enter chords separated by commas or spaces. Try Dm7 G7 Cmaj7 or Am7 D7 Gmaj7.");
                    Console.Write("Progression > ");
                    return new TriadProgressionSetup("Custom jazz changes", Console.ReadLine()?.Trim() ?? string.Empty);
                case "B":
                case "Q":
                    return null;
                default:
                    Console.Write("Choose 1, 2, 3, 4, C, or B > ");
                    break;
            }
        }
    }

    private TriadProgressionSetup BuildRandomJazzProgressionSetup()
    {
        var progression = _jazzChords.BuildRandomProgression();
        var progressionText = string.Join(" ", progression.Select(chord => chord.DisplayName));
        return new TriadProgressionSetup($"Random jazz arrangement: {progressionText}", progressionText, 90, new TimeSignature(4, 4), [4, 4, 4, 4]);
    }

    private static TriadProgressionSetup BuildDjangoMinorSwingSetup()
    {
        return new TriadProgressionSetup(
            "Django-style minor swing: Am6 Dm6 E7b9 Am6",
            "Am6 Dm6 E7b9 Am6",
            160,
            new TimeSignature(4, 4),
            [8, 8, 8, 8]);
    }

    private static TriadProgressionSetup BuildDjangoDiminishedSetup()
    {
        return new TriadProgressionSetup(
            "Diminished passing colors: Am6 C#dim7 Dm6 E7b9 Am6",
            "Am6 C#dim7 Dm6 E7b9 Am6",
            120,
            new TimeSignature(4, 4),
            [4, 4, 4, 4, 8]);
    }

    private void ShowJazzStandardsLibrary()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz standards library");
            WriteDescriptionLine("Use standards as complete studies: form, comping, guide tones, solo targets, and performance loops.");
            Console.WriteLine();
            WriteMenuOption("1", "Study a standard step by step", "Pick one progression and move through form, comping, soloing, and performance.");
            WriteMenuOption("2", "Play standards chord game", "Choose a standard-style progression and practise the voicings directly.");
            WriteMenuOption("3", "Print standards study sheet", "Create a worksheet for one tune-style progression.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    {
                        var preset = ReadJazzPreset();
                        if (preset is not null)
                        {
                            ShowJazzStandardStudyPath(preset);
                        }
                        break;
                    }
                case "2":
                    ShowJazzChordSongLibrary();
                    break;
                case "3":
                    PrintJazzStandardStudySheet();
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

    private void ShowJazzStandardStudyPath(PresetJazzProgression preset)
    {
        var setup = SetupFor(preset);
        while (true)
        {
            Console.Clear();
            WriteHeader($"Standard study: {preset.Title}");
            Console.WriteLine($"{preset.Artist}  BPM {preset.Bpm}  {preset.TimeSignature.DisplayName}");
            WriteDescriptionLine($"Concepts: {preset.Concepts}");
            WriteDescriptionLine($"Changes: {preset.ProgressionText}");
            Console.WriteLine();
            WriteJazzStudyStage("1. Form and map", "Speak the progression in chunks. Mark every ii-V, tonic, turnaround, and minor cadence.");
            WriteJazzStudyStage("2. Comping", "Play shell voicings with excellent time before adding bigger grips.");
            WriteJazzStudyStage("3. Guide-tone melody", "Connect the 3rds and 7ths until the harmony is audible without full chords.");
            WriteJazzStudyStage("4. Solo targets", "Add approaches, enclosures, and arpeggio outlines around the guide tones.");
            WriteJazzStudyStage("5. Performance loop", "Play one chorus comping, one chorus guide-tone soloing, and one chorus freer.");
            Console.WriteLine();
            WriteMenuOption("1", "Comp shells", "Play the standard with shell voicings.");
            WriteMenuOption("2", "Comp rhythm trainer", "Apply a rhythm pattern to the standard.");
            WriteMenuOption("3", "Guide-tone line builder", "Target 3rds and 7ths through the form.");
            WriteMenuOption("4", "Performance loop", "Run the standard as a chord game with your chosen voicing mode.");
            WriteMenuOption("5", "Print study sheet", "Open a printable version of this study plan.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a stage > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    ShowJazzChordGame($"{preset.Title}: shell comping", setup, JazzVoicingMode.Shell);
                    break;
                case "2":
                    ShowJazzCompingRhythmTrainer(selectedSetup: setup);
                    break;
                case "3":
                    ShowJazzLineBuilder(JazzSoloExerciseKind.Direct, setup);
                    break;
                case "4":
                    {
                        var voicingMode = ReadJazzVoicingMode() ?? JazzVoicingMode.Shell;
                        ShowJazzChordGame($"{preset.Title}: performance loop", setup, voicingMode);
                        break;
                    }
                case "5":
                    PrintJazzStandardStudySheet(preset);
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

    private void ShowPrintableJazzSheets()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Printable jazz sheets");
            WriteDescriptionLine("Create practice pages you can keep beside the guitar.");
            Console.WriteLine();
            WriteMenuOption("1", "Voicing sheet", "Print guide-tone, shell, or full voicings for one chord.");
            WriteMenuOption("2", "ii-V-I worksheet", "Print major and minor ii-V-I formulas, guide tones, and practice steps.");
            WriteMenuOption("3", "Standards study sheet", "Print a tune-style progression with staged practice prompts.");
            WriteMenuOption("4", "Jazz roadmap sheet", "Print the full fundamentals roadmap.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a sheet > ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    PrintJazzVoicingSheet();
                    break;
                case "2":
                    PrintJazzTwoFiveOneSheet();
                    break;
                case "3":
                    PrintJazzStandardStudySheet();
                    break;
                case "4":
                    PrintJazzRoadmapSheet();
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

    private void PrintJazzVoicingSheet()
    {
        var root = ReadMenuChoice("Choose a root", MusicTheory.ChromaticRoots, allowBack: true);
        if (root is null)
        {
            return;
        }

        var qualityChoice = ReadMenuChoice("Choose a chord type", JazzChordLibrary.Qualities.Select(quality => quality.Suffix).ToArray(), allowBack: true);
        if (qualityChoice is null)
        {
            return;
        }

        var voicingMode = ReadJazzVoicingMode();
        if (voicingMode is null)
        {
            return;
        }

        var quality = JazzChordLibrary.Quality(qualityChoice);
        var groups = _jazzChords.GetVoicings(root, quality, voicingMode.Value);
        PrintJazzSheet(() => JazzSheetPrinter.PrintVoicingSheet(root, quality, voicingMode.Value, groups));
    }

    private static void PrintJazzTwoFiveOneSheet()
    {
        var keyRoot = ReadMenuChoice("Choose a key", MusicTheory.ChromaticRoots, allowBack: true);
        if (keyRoot is null)
        {
            return;
        }

        PrintJazzSheet(() => JazzSheetPrinter.PrintTwoFiveOneSheet(keyRoot));
    }

    private static void PrintJazzRoadmapSheet()
    {
        PrintJazzSheet(JazzSheetPrinter.PrintRoadmapSheet);
    }

    private static void PrintJazzStandardStudySheet(PresetJazzProgression? selectedPreset = null)
    {
        var preset = selectedPreset ?? ReadJazzPreset();
        if (preset is null)
        {
            return;
        }

        PrintJazzSheet(() => JazzSheetPrinter.PrintStandardStudySheet(preset));
    }

    private static void PrintJazzSheet(Func<string> print)
    {
        try
        {
            var filePath = print();
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

    private static void ShowJazzChordTypeList()
    {
        Console.Clear();
        WriteHeader("Jazz chord type list");
        WriteDescriptionLine("Jazz harmony mostly extends triads into 6th, 7th, 9th, 11th, and 13th chords.");
        WriteDescriptionLine("On guitar, players use inversions too, but usually call the practical shapes voicings: shell, drop 2, rootless, and altered voicings.");
        WriteDescriptionLine("The interval sets below are compact guitar-friendly voicing tones; extended chords may omit the 5th or other less-essential tones.");
        Console.WriteLine();

        foreach (var quality in JazzChordLibrary.Qualities)
        {
            Console.WriteLine($"{quality.Suffix.PadRight(6)} {quality.Name.PadRight(18)} {string.Join(" ", quality.Intervals).PadRight(14)} {quality.Use}");
        }

        Console.WriteLine();
        WriteDescriptionLine("Press any key to go back.");
        Console.ReadKey(intercept: true);
    }

    private void ShowJazzFundamentalsPath()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Jazz fundamentals path");
            WriteDescriptionLine("Work from the smallest useful idea to real jazz tunes.");
            Console.WriteLine();
            WriteMenuOption("1", "Seventh chord formulas", "Learn the ingredients behind jazz chord symbols.");
            WriteMenuOption("2", "Guide-tone ii-V-I", "Practise the 3rds and 7ths that steer seventh-chord harmony.");
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
            WriteDescriptionLine("A voicing is the way a chord's notes are arranged on the guitar.");
            WriteDescriptionLine("Start with guide tones, then shells, then full voicings.");
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
            Console.WriteLine($"{quality.Name}: compact chord tones {string.Join(" ", quality.Intervals)}. Showing {string.Join(" ", JazzChordLibrary.IntervalsFor(quality, voicingMode))}.");
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
        ShowJazzChordGame("Jazz standards library", setup, voicingMode);
    }

    private void ShowRandomJazzChordGame()
    {
        ShowJazzChordGame("Random jazz chord arrangement", BuildRandomJazzProgressionSetup(), JazzVoicingMode.Shell);
    }

    private static TriadProgressionSetup? ReadJazzSongSetup()
    {
        var preset = ReadJazzPreset();
        return preset is null ? null : SetupFor(preset);
    }

    private static PresetJazzProgression? ReadJazzPreset()
    {
        Console.Clear();
        WriteHeader("Jazz standards library");
        WriteDescriptionLine("Each preset lists a compact study progression plus the jazz ideas it demonstrates.");
        WriteDescriptionLine("Try shell voicings first, then switch to full voicings once the movement feels familiar.");
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
                return preset;
            }

            Console.Write("Choose a listed number, song title, or B > ");
        }
    }

    private static TriadProgressionSetup SetupFor(PresetJazzProgression preset)
    {
        return new TriadProgressionSetup(preset.Name, preset.ProgressionText, preset.Bpm, preset.TimeSignature, preset.ChordLengths);
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
            WriteDescriptionLine("A ii-V-I means chords built from scale degrees 2, 5, and 1.");
            WriteDescriptionLine("In C major that is Dm7 -> G7 -> Cmaj7.");
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
        WriteMenuOption("1", "Guide tones", "Usually the 3rd and 7th; for 6/dim chords, the nearest essential color tones.");
        WriteMenuOption("2", "Shell voicings", "Root plus guide/essential tones: small, practical jazz rhythm-guitar grips.");
        WriteMenuOption("3", "Full voicings", "Compact four-note guitar voicings with colors like 5, 9, 13, or altered tensions.");
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

        var measuredItems = EnumerateJazzItemsOnce(currentPhrase, nextPhrase, chordLengths, chordIndex)
            .ToArray();
        var gridCellWidth = measuredItems
            .Select((item, index) => ($"{(index == 0 ? "> " : "  ")}{item.PracticeItem.Title} [{item.Beats} beat{Pluralize(item.Beats)}]", item.PracticeItem.Diagram))
            .Max(_renderer.MeasureTitledDiagramWidth);
        var maxColumns = JazzGameColumnsFor(gridCellWidth);
        var visibleItems = EnumerateRollingJazzItems(currentPhrase, nextPhrase, chordLengths, chordIndex)
            .Take(maxColumns * 2)
            .Select((item, index) => ($"{(index == 0 ? "> " : "  ")}{item.PracticeItem.Title} [{item.Beats} beat{Pluralize(item.Beats)}]", item.PracticeItem.Diagram))
            .ToArray();

        foreach (var line in _renderer.RenderMany(visibleItems, GetUsableConsoleWidth(), new HashSet<int> { 0 }, maxColumns: maxColumns, cellWidth: gridCellWidth))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        WriteCenteredHighlightedJazzProgression(currentPhrase.Select(item => item.Chord).ToArray(), chordIndex);
    }

    private static int JazzGameColumnsFor(int cellWidth)
    {
        const int maxColumns = 4;
        const int diagramGap = 3;
        var usableWidth = GetUsableConsoleWidth();
        return Math.Clamp((usableWidth + diagramGap) / Math.Max(1, cellWidth + diagramGap), 1, maxColumns);
    }

    private static IEnumerable<(JazzPracticeItem PracticeItem, int Beats)> EnumerateJazzItemsOnce(
        IReadOnlyList<JazzPracticeItem> currentPhrase,
        IReadOnlyList<JazzPracticeItem> nextPhrase,
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
    }

    private static IEnumerable<(JazzPracticeItem PracticeItem, int Beats)> EnumerateRollingJazzItems(
        IReadOnlyList<JazzPracticeItem> currentPhrase,
        IReadOnlyList<JazzPracticeItem> nextPhrase,
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
