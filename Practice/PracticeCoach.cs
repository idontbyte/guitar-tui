namespace GuitarResourcesTui.Practice;

public enum PracticeFocus
{
    Mixed,
    Beginner,
    Triads,
    Jazz,
    Soloing,
    Rhythm
}

public enum PracticeDifficulty
{
    Easy,
    Okay,
    Hard,
    Skipped
}

public enum PracticeBlockKind
{
    Tuner,
    CowboyChords,
    TriadMovement,
    SpreadTriads,
    JazzGuideTones,
    JazzShellVoicings,
    ArpeggioTargeting,
    ArpeggioSong,
    RhythmArpeggios,
    ScaleSong,
    IntervalTargets
}

public sealed record PracticeSessionRequest(
    int DurationMinutes,
    PracticeFocus Focus);

public sealed record PracticeSessionPlan(
    DateOnly Date,
    int DurationMinutes,
    PracticeFocus Focus,
    IReadOnlyList<PracticeSessionBlock> Blocks);

public sealed record PracticeSessionBlock(
    string Id,
    PracticeBlockKind Kind,
    string Area,
    string Title,
    int Minutes,
    string Goal,
    string Prompt,
    string ProgressionText,
    string KeyRoot,
    string ScaleKindName);

public sealed record PracticeBlockResult(
    string BlockId,
    PracticeBlockKind Kind,
    string Area,
    string Title,
    int Minutes,
    PracticeDifficulty Difficulty);

public sealed record PracticeSessionEntry(
    DateTimeOffset CompletedAt,
    int DurationMinutes,
    PracticeFocus Focus,
    IReadOnlyList<PracticeBlockResult> Blocks);

public sealed class PracticeSessionLog
{
    public List<PracticeSessionEntry> Entries { get; init; } = [];
}

public static class PracticeCoachPlanner
{
    private static readonly PracticeBlockTemplate Tuner = new(
        "tuner-ear",
        PracticeBlockKind.Tuner,
        "Tuner",
        "Tune by ear",
        "Settle your hands and ears before the fretboard work.",
        "Play a reference note, tune by ear, then check the open strings cleanly.",
        "E A D G",
        "C",
        "Major scale",
        FocusSet(PracticeFocus.Mixed, PracticeFocus.Beginner, PracticeFocus.Rhythm, PracticeFocus.Triads, PracticeFocus.Soloing, PracticeFocus.Jazz));

    private static readonly IReadOnlyList<PracticeBlockTemplate> Templates =
    [
        new(
            "cowboy-changes",
            PracticeBlockKind.CowboyChords,
            "Cowboy chords",
            "Open chord changes",
            "Make common open grips feel automatic under time.",
            "Use small finger motions. Keep the click on until the change lands on beat 1.",
            "G D Em C",
            "G",
            "Major scale",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Beginner, PracticeFocus.Rhythm)),
        new(
            "triad-connected",
            PracticeBlockKind.TriadMovement,
            "Triads",
            "Connected triad movement",
            "Move through a progression using nearby triad inversions.",
            "Say the chord name, find the nearest shape, and listen for smooth movement.",
            "C G Am F",
            "C",
            "Major scale",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Beginner, PracticeFocus.Triads, PracticeFocus.Rhythm)),
        new(
            "spread-triads",
            PracticeBlockKind.SpreadTriads,
            "Triads",
            "Spread triad color",
            "Open up triads with skipped-string shapes.",
            "Let each note speak separately first, then bring the shape into time.",
            "D A Bm G",
            "D",
            "Major scale",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Triads, PracticeFocus.Soloing)),
        new(
            "jazz-guide-tones",
            PracticeBlockKind.JazzGuideTones,
            "Jazz chords",
            "Guide-tone ii-V-I",
            "Hear the 3rds and 7ths that steer jazz harmony.",
            "Track the smallest movement between guide tones before filling in bigger voicings.",
            "Dm7 G7 Cmaj7 Cmaj7",
            "C",
            "Major scale",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Jazz, PracticeFocus.Soloing)),
        new(
            "jazz-shells",
            PracticeBlockKind.JazzShellVoicings,
            "Jazz chords",
            "Shell voicing ii-V-I",
            "Turn guide tones into compact comping grips.",
            "Keep the root quiet but clear, then notice how the 3rd and 7th resolve.",
            "Dm7 G7 Cmaj7 Cmaj7",
            "C",
            "Major scale",
            FocusSet(PracticeFocus.Jazz, PracticeFocus.Rhythm)),
        new(
            "arpeggio-targeting",
            PracticeBlockKind.ArpeggioTargeting,
            "Arpeggios",
            "Find the chord tone",
            "Train your ear and fingers to land on strong notes.",
            "Hear the chord, name the target interval, then find the same note in more than one place.",
            "Dm7 G7 Cmaj7",
            "C",
            "Major scale",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Soloing, PracticeFocus.Jazz)),
        new(
            "arpeggio-song",
            PracticeBlockKind.ArpeggioSong,
            "Arpeggios",
            "Arpeggios through changes",
            "Connect arpeggio shapes to a moving progression.",
            "Play only chord tones at first. Add rhythm after the switches feel calm.",
            "Dm7 G7 Cmaj7 Am7",
            "C",
            "Major scale",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Soloing, PracticeFocus.Jazz)),
        new(
            "rhythm-arpeggios",
            PracticeBlockKind.RhythmArpeggios,
            "Arpeggios",
            "Rhythm arpeggio patterns",
            "Use broken chords as rhythm-guitar material.",
            "Keep the fretting hand relaxed while the picking hand stays even.",
            "C G Am F",
            "C",
            "Major scale",
            FocusSet(PracticeFocus.Rhythm, PracticeFocus.Beginner)),
        new(
            "scale-song",
            PracticeBlockKind.ScaleSong,
            "Scales",
            "Scale over song changes",
            "Use a scale shape while tracking the current chord tones.",
            "Aim for R, 3, or 5 when the chord changes instead of wandering through the box.",
            "C G Am F",
            "C",
            "Major pentatonic",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Beginner, PracticeFocus.Soloing)),
        new(
            "interval-targets",
            PracticeBlockKind.IntervalTargets,
            "Intervals",
            "Target intervals",
            "See every note as a function of the current chord.",
            "Call out the target before you play it. Treat the map as a set of landing zones.",
            "C G Am F",
            "C",
            "Major scale",
            FocusSet(PracticeFocus.Mixed, PracticeFocus.Triads, PracticeFocus.Soloing, PracticeFocus.Jazz))
    ];

    public static PracticeSessionPlan BuildPlan(PracticeSessionRequest request, PracticeSessionLog log, DateOnly date)
    {
        var duration = Math.Clamp(request.DurationMinutes, 5, 30);
        var blockCount = BlockCountFor(duration);
        var slots = AllocateMinutes(duration, blockCount);
        var selected = SelectTemplates(request.Focus, log, blockCount - 1, date);
        var blocks = new[] { Tuner }
            .Concat(selected)
            .Select((template, index) => template.ToBlock(slots[index]))
            .ToArray();

        return new PracticeSessionPlan(date, duration, request.Focus, blocks);
    }

    public static IReadOnlyList<string> HardAreas(PracticeSessionLog log, int maxCount = 3)
    {
        return log.Entries
            .SelectMany(entry => entry.Blocks)
            .Where(block => block.Difficulty == PracticeDifficulty.Hard)
            .GroupBy(block => block.Area)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Take(maxCount)
            .Select(group => group.Key)
            .ToArray();
    }

    private static int BlockCountFor(int duration) => duration switch
    {
        <= 5 => 3,
        <= 10 => 4,
        <= 20 => 5,
        _ => 6
    };

    private static IReadOnlySet<PracticeFocus> FocusSet(params PracticeFocus[] focuses)
    {
        return focuses.ToHashSet();
    }

    private static IReadOnlyList<int> AllocateMinutes(int duration, int blockCount)
    {
        var baseMinutes = Math.Max(1, duration / blockCount);
        var remaining = duration - (baseMinutes * blockCount);
        var result = Enumerable.Repeat(baseMinutes, blockCount).ToArray();

        for (var index = result.Length - 1; remaining > 0; index = Math.Max(0, index - 1), remaining--)
        {
            result[index]++;
        }

        return result;
    }

    private static IReadOnlyList<PracticeBlockTemplate> SelectTemplates(
        PracticeFocus focus,
        PracticeSessionLog log,
        int count,
        DateOnly date)
    {
        var candidates = Templates
            .Where(template => focus == PracticeFocus.Mixed || template.Focuses.Contains(focus))
            .OrderByDescending(template => PriorityFor(template, log, date))
            .ThenBy(template => template.Title)
            .ToList();

        if (candidates.Count < count)
        {
            candidates.AddRange(Templates.Where(template => !candidates.Contains(template)));
        }

        return candidates.Take(count).ToArray();
    }

    private static int PriorityFor(PracticeBlockTemplate template, PracticeSessionLog log, DateOnly date)
    {
        var results = log.Entries
            .SelectMany(entry => entry.Blocks.Select(block => (entry.CompletedAt, Block: block)))
            .Where(item => item.Block.BlockId == template.Id)
            .ToArray();

        if (results.Length == 0)
        {
            return 100;
        }

        var hardScore = results.Count(item => item.Block.Difficulty == PracticeDifficulty.Hard) * 100;
        var skippedScore = results.Count(item => item.Block.Difficulty == PracticeDifficulty.Skipped) * 8;
        var easyPenalty = results.TakeLast(3).Count(item => item.Block.Difficulty == PracticeDifficulty.Easy) * 5;
        var lastDate = DateOnly.FromDateTime(results.Max(item => item.CompletedAt).Date);
        var daysSince = Math.Min(30, Math.Max(0, date.DayNumber - lastDate.DayNumber));

        return 30 + hardScore + skippedScore + daysSince - easyPenalty;
    }

    private sealed record PracticeBlockTemplate(
        string Id,
        PracticeBlockKind Kind,
        string Area,
        string Title,
        string Goal,
        string Prompt,
        string ProgressionText,
        string KeyRoot,
        string ScaleKindName,
        IReadOnlySet<PracticeFocus> Focuses)
    {
        public PracticeSessionBlock ToBlock(int minutes)
        {
            return new PracticeSessionBlock(
                Id,
                Kind,
                Area,
                Title,
                minutes,
                Goal,
                Prompt,
                ProgressionText,
                KeyRoot,
                ScaleKindName);
        }
    }
}
