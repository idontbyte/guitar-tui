namespace GuitarResourcesTui.Shredding;

public enum ShredDrillCategory
{
    FingerIndependence,
    Picking,
    Synchronization,
    ScaleSequencing,
    Legato,
    Burst
}

public enum PickStroke
{
    Down,
    Up,
    Hammer,
    Pull
}

public enum ShredAttemptResult
{
    Clean,
    Messy,
    Tense
}

public enum ShredStringFlow
{
    PerString,
    Cycle
}

public sealed record ShredDrill(
    string Id,
    string Title,
    ShredDrillCategory Category,
    string Pattern,
    IReadOnlyList<int> Fingers,
    IReadOnlyList<int> StringIndexes,
    IReadOnlyList<PickStroke> Picking,
    ShredStringFlow StringFlow,
    int StartBpm,
    int GoalBpm,
    string Goal,
    string ListenFor,
    string PracticeNote);

public sealed record ShredNote(
    int StringIndex,
    string StringName,
    int Fret,
    int Finger,
    PickStroke PickStroke);

public sealed record ShredExercise(
    ShredDrill Drill,
    int StartFret,
    IReadOnlyList<ShredNote> Notes);

public sealed record ShredTempoState(
    string DrillId,
    int CurrentBpm,
    int TopCleanBpm,
    int GoalBpm,
    int CleanStreak);

public sealed record ShredTempoUpdate(
    ShredTempoState State,
    string Message);

public sealed record ShredTempoRecord(
    string DrillId,
    string Title,
    int TopCleanBpm,
    int LastBpm,
    DateTimeOffset LastPracticed,
    int CleanAttempts,
    int MessyAttempts,
    int TenseAttempts);

public sealed class ShredPracticeLog
{
    public List<ShredTempoRecord> Records { get; init; } = [];
}

public static class ShredLibrary
{
    public const int MinimumBuilderBpm = 30;
    public const int MaximumBuilderBpm = 320;

    private static readonly IReadOnlyList<string> GuitarStrings = ["E", "B", "G", "D", "A", "E"];

    public static readonly IReadOnlyList<ShredDrill> Drills =
    [
        new(
            "chromatic-1234",
            "Chromatic 1-2-3-4",
            ShredDrillCategory.FingerIndependence,
            "1-2-3-4 across all strings",
            [1, 2, 3, 4],
            [5, 4, 3, 2, 1, 0],
            AlternatePicking(4),
            ShredStringFlow.PerString,
            60,
            140,
            "Build basic finger independence without extra finger lift.",
            "Every note should be the same volume and length.",
            "Keep fingers close to the strings. Speed is a side effect of small relaxed motion."),
        new(
            "permutation-1324",
            "Permutation 1-3-2-4",
            ShredDrillCategory.FingerIndependence,
            "1-3-2-4 across all strings",
            [1, 3, 2, 4],
            [5, 4, 3, 2, 1, 0],
            AlternatePicking(4),
            ShredStringFlow.PerString,
            55,
            130,
            "Break the habit of always moving fingers in order.",
            "The 3-to-2 move should stay clean and unhurried.",
            "Start painfully slow if the fingers want to fly away from the fretboard."),
        new(
            "spider-1423",
            "Spider walk 1-4-2-3",
            ShredDrillCategory.FingerIndependence,
            "1-4-2-3 across adjacent strings",
            [1, 4, 2, 3],
            [5, 4, 3, 2, 1, 0],
            AlternatePicking(4),
            ShredStringFlow.PerString,
            45,
            115,
            "Train awkward finger independence and string tracking.",
            "No note should pop louder when the hand changes strings.",
            "This is supposed to feel odd; keep it slow and relaxed."),
        new(
            "single-string-alt",
            "One-string alternate picking",
            ShredDrillCategory.Picking,
            "1-2-3-4 repeated on one string",
            [1, 2, 3, 4, 4, 3, 2, 1],
            [0],
            AlternatePicking(8),
            ShredStringFlow.PerString,
            70,
            180,
            "Make the pick move evenly down-up without accents sneaking in.",
            "The upstrokes should sound as strong as the downstrokes.",
            "Use tiny pick motion and stay loose through the forearm."),
        new(
            "outside-picking",
            "Outside picking",
            ShredDrillCategory.Picking,
            "two-string outside crossing",
            [1, 2, 3, 1, 2, 3],
            [1, 0],
            [PickStroke.Down, PickStroke.Up, PickStroke.Down, PickStroke.Up, PickStroke.Down, PickStroke.Up],
            ShredStringFlow.Cycle,
            60,
            150,
            "Train the pick to cross around the outside of two strings.",
            "The string change should not drag or snag.",
            "Accent the first note of each six-note group until the crossing feels stable."),
        new(
            "inside-picking",
            "Inside picking",
            ShredDrillCategory.Picking,
            "two-string inside crossing",
            [1, 2, 3, 1, 2, 3],
            [0, 1],
            [PickStroke.Down, PickStroke.Up, PickStroke.Down, PickStroke.Up, PickStroke.Down, PickStroke.Up],
            ShredStringFlow.Cycle,
            55,
            145,
            "Train the pick to cross between two strings.",
            "Listen for a clicky scrape at the crossing; reduce motion if you hear it.",
            "Stay relaxed and do not dig in harder to force speed."),
        new(
            "sync-124",
            "Synchronization 1-2-4",
            ShredDrillCategory.Synchronization,
            "1-2-4 across all strings",
            [1, 2, 4],
            [5, 4, 3, 2, 1, 0],
            AlternatePicking(3),
            ShredStringFlow.PerString,
            60,
            150,
            "Lock the fretting hand to the pick on a common three-note shape.",
            "No note should smear into the next one.",
            "Use this before three-note-per-string scales."),
        new(
            "sync-134",
            "Synchronization 1-3-4",
            ShredDrillCategory.Synchronization,
            "1-3-4 across all strings",
            [1, 3, 4],
            [5, 4, 3, 2, 1, 0],
            AlternatePicking(3),
            ShredStringFlow.PerString,
            55,
            140,
            "Strengthen the 3rd and 4th fingers without tension.",
            "The 3-4 movement should sound clean rather than squeezed.",
            "If the hand tightens, lower the tempo immediately."),
        new(
            "major-sequence-threes",
            "Major scale sequence in threes",
            ShredDrillCategory.ScaleSequencing,
            "1-2-3, 2-3-4, 3-4-5",
            [1, 2, 4, 1, 3, 4, 1, 3, 4],
            [5, 4, 3],
            AlternatePicking(9),
            ShredStringFlow.PerString,
            60,
            150,
            "Turn scale shapes into musical three-note groups.",
            "Each three-note cell should have a clear first-note accent.",
            "Think in groups, not a blur of notes."),
        new(
            "minor-sixes",
            "Minor scale sequence in sixes",
            ShredDrillCategory.ScaleSequencing,
            "six-note cells across three strings",
            [1, 3, 4, 1, 3, 4],
            [2, 1, 0],
            AlternatePicking(6),
            ShredStringFlow.PerString,
            55,
            140,
            "Practice a classic six-note shred cell.",
            "The first note of each six should land with the click.",
            "Move the shape through keys after it is clean in one position."),
        new(
            "legato-124",
            "Legato 1-2-4",
            ShredDrillCategory.Legato,
            "pick, hammer, hammer, pull, pull",
            [1, 2, 4, 2, 1],
            [0],
            [PickStroke.Down, PickStroke.Hammer, PickStroke.Hammer, PickStroke.Pull, PickStroke.Pull],
            ShredStringFlow.PerString,
            50,
            130,
            "Make hammer-ons and pull-offs even without picking every note.",
            "The unpicked notes should not disappear.",
            "Use less force than you think; clarity beats volume."),
        new(
            "four-note-bursts",
            "Four-note speed bursts",
            ShredDrillCategory.Burst,
            "short fast groups with space",
            [1, 2, 3, 4],
            [0],
            AlternatePicking(4),
            ShredStringFlow.PerString,
            80,
            190,
            "Taste higher speed in tiny relaxed bursts.",
            "The burst should stop cleanly without the hand clenching.",
            "Play one fast group, rest, breathe, repeat.")
    ];

    public static ShredExercise BuildExercise(ShredDrill drill, int startFret)
    {
        var clampedStartFret = Math.Clamp(startFret, 1, 17);
        var notes = new List<ShredNote>();

        if (drill.StringFlow == ShredStringFlow.Cycle)
        {
            for (var index = 0; index < drill.Fingers.Count; index++)
            {
                var stringIndex = drill.StringIndexes[index % drill.StringIndexes.Count];
                var finger = drill.Fingers[index];
                var fret = clampedStartFret + finger - 1;
                var pickStroke = drill.Picking[index % drill.Picking.Count];
                notes.Add(new ShredNote(stringIndex, GuitarStrings[stringIndex], fret, finger, pickStroke));
            }

            return new ShredExercise(drill, clampedStartFret, notes);
        }

        foreach (var stringIndex in drill.StringIndexes)
        {
            for (var index = 0; index < drill.Fingers.Count; index++)
            {
                var finger = drill.Fingers[index];
                var fret = clampedStartFret + finger - 1;
                var pickStroke = drill.Picking[index % drill.Picking.Count];
                notes.Add(new ShredNote(stringIndex, GuitarStrings[stringIndex], fret, finger, pickStroke));
            }
        }

        return new ShredExercise(drill, clampedStartFret, notes);
    }

    public static IReadOnlyList<string> RenderTab(ShredExercise exercise)
    {
        var cellsByString = Enumerable.Range(0, GuitarStrings.Count)
            .ToDictionary(index => index, _ => new List<string>());

        foreach (var note in exercise.Notes)
        {
            for (var stringIndex = 0; stringIndex < GuitarStrings.Count; stringIndex++)
            {
                cellsByString[stringIndex].Add(stringIndex == note.StringIndex ? note.Fret.ToString().PadLeft(2, '-') : "--");
            }
        }

        return Enumerable.Range(0, GuitarStrings.Count)
            .Select(index => $"{GuitarStrings[index],2}|{string.Join("-", cellsByString[index])}-|")
            .ToArray();
    }

    public static IReadOnlyList<string> RenderFingerLine(ShredExercise exercise)
    {
        return
        [
            $"Fingers: {string.Join(" ", exercise.Notes.Select(note => note.Finger))}",
            $"Picking: {string.Join(" ", exercise.Notes.Select(note => DisplayNameFor(note.PickStroke)))}"
        ];
    }

    public static ShredTempoState StartingTempoFor(ShredDrill drill, ShredPracticeLog log)
    {
        var record = log.Records.FirstOrDefault(item => item.DrillId == drill.Id);
        var startBpm = record is null
            ? drill.StartBpm
            : Math.Clamp(record.TopCleanBpm - 10, drill.StartBpm, MaximumBuilderBpm);

        return new ShredTempoState(drill.Id, startBpm, record?.TopCleanBpm ?? 0, drill.GoalBpm, 0);
    }

    public static ShredTempoUpdate ApplyAttempt(
        ShredTempoState state,
        ShredAttemptResult result,
        int cleanRepsRequired = 3,
        int increment = 5)
    {
        return result switch
        {
            ShredAttemptResult.Clean => ApplyCleanAttempt(state, cleanRepsRequired, increment),
            ShredAttemptResult.Messy => new ShredTempoUpdate(
                state with
                {
                    CurrentBpm = Math.Max(MinimumBuilderBpm, state.CurrentBpm - increment),
                    CleanStreak = 0
                },
                "Messy rep: drop the tempo and make the notes even again."),
            ShredAttemptResult.Tense => new ShredTempoUpdate(
                state with
                {
                    CurrentBpm = Math.Max(MinimumBuilderBpm, state.CurrentBpm - increment * 2),
                    CleanStreak = 0
                },
                "Tension rep: back off more. Relaxation is part of the drill."),
            _ => new ShredTempoUpdate(state, "No change.")
        };
    }

    public static void RecordAttempt(ShredPracticeLog log, ShredDrill drill, ShredTempoState state, ShredAttemptResult result, DateTimeOffset now)
    {
        var existing = log.Records.FirstOrDefault(record => record.DrillId == drill.Id);
        var record = existing ?? new ShredTempoRecord(drill.Id, drill.Title, 0, drill.StartBpm, now, 0, 0, 0);

        var updated = record with
        {
            Title = drill.Title,
            TopCleanBpm = Math.Max(record.TopCleanBpm, state.TopCleanBpm),
            LastBpm = state.CurrentBpm,
            LastPracticed = now,
            CleanAttempts = record.CleanAttempts + (result == ShredAttemptResult.Clean ? 1 : 0),
            MessyAttempts = record.MessyAttempts + (result == ShredAttemptResult.Messy ? 1 : 0),
            TenseAttempts = record.TenseAttempts + (result == ShredAttemptResult.Tense ? 1 : 0)
        };

        if (existing is null)
        {
            log.Records.Add(updated);
        }
        else
        {
            var index = log.Records.IndexOf(existing);
            log.Records[index] = updated;
        }
    }

    public static IReadOnlyList<ShredDrill> BuildDailyWorkout(ShredPracticeLog log)
    {
        var due = Drills
            .OrderByDescending(drill => PriorityFor(drill, log))
            .ThenBy(drill => drill.Title)
            .Take(5)
            .ToArray();

        return due.Length == 0 ? Drills.Take(5).ToArray() : due;
    }

    public static string DisplayNameFor(ShredDrillCategory category) => category switch
    {
        ShredDrillCategory.FingerIndependence => "finger independence",
        ShredDrillCategory.Picking => "picking",
        ShredDrillCategory.Synchronization => "synchronization",
        ShredDrillCategory.ScaleSequencing => "scale sequencing",
        ShredDrillCategory.Legato => "legato",
        ShredDrillCategory.Burst => "speed bursts",
        _ => category.ToString()
    };

    public static string DisplayNameFor(PickStroke stroke) => stroke switch
    {
        PickStroke.Down => "D",
        PickStroke.Up => "U",
        PickStroke.Hammer => "H",
        PickStroke.Pull => "P",
        _ => stroke.ToString()
    };

    private static ShredTempoUpdate ApplyCleanAttempt(ShredTempoState state, int cleanRepsRequired, int increment)
    {
        var cleanStreak = state.CleanStreak + 1;
        var topClean = Math.Max(state.TopCleanBpm, state.CurrentBpm);

        if (cleanStreak < cleanRepsRequired)
        {
            return new ShredTempoUpdate(
                state with { TopCleanBpm = topClean, CleanStreak = cleanStreak },
                $"Clean rep {cleanStreak}/{cleanRepsRequired}. Stay relaxed and repeat.");
        }

        var nextBpm = Math.Min(MaximumBuilderBpm, state.CurrentBpm + increment);
        return new ShredTempoUpdate(
            state with
            {
                CurrentBpm = nextBpm,
                TopCleanBpm = topClean,
                CleanStreak = 0
            },
            nextBpm == state.CurrentBpm
                ? "Speed-builder ceiling reached. Keep it clean rather than forcing more speed."
                : nextBpm > state.GoalBpm
                    ? $"Goal tempo passed. Move up to {nextBpm} BPM only if it stays relaxed."
                : $"Three clean reps. Move up to {nextBpm} BPM.");
    }

    private static int PriorityFor(ShredDrill drill, ShredPracticeLog log)
    {
        var record = log.Records.FirstOrDefault(item => item.DrillId == drill.Id);
        if (record is null)
        {
            return 1000;
        }

        var daysSince = Math.Min(30, Math.Max(0, (DateTimeOffset.Now - record.LastPracticed).Days));
        var weakness = record.MessyAttempts + record.TenseAttempts * 2 - record.CleanAttempts;
        var belowGoal = Math.Max(0, drill.GoalBpm - record.TopCleanBpm) / 5;
        return daysSince + weakness + belowGoal;
    }

    private static IReadOnlyList<PickStroke> AlternatePicking(int count)
    {
        return Enumerable.Range(0, count)
            .Select(index => index % 2 == 0 ? PickStroke.Down : PickStroke.Up)
            .ToArray();
    }
}
