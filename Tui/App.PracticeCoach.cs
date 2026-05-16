using System.Text.Json;
using GuitarResourcesTui.JazzChords;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Practice;
using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private static readonly JsonSerializerOptions PracticeJsonOptions = new()
    {
        WriteIndented = true
    };

    private void ShowPracticeCoach()
    {
        while (true)
        {
            var log = LoadPracticeLog();

            Console.Clear();
            WriteHeader("Daily practice");
            WriteDescriptionLine("Build a focused session from the app's drills, then mark each block easy, okay, or hard.");
            WriteDescriptionLine("Hard and skipped blocks are gently rotated back into future sessions.");
            Console.WriteLine();
            WritePracticeSummary(log);
            Console.WriteLine();
            WriteMenuOption("1", "Start guided session", "Pick a duration and focus. The coach builds the practice blocks.");
            WriteMenuOption("2", "Practice history", "Review recent sessions and hard areas.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim().ToUpperInvariant())
            {
                case "1":
                    StartPracticeSession(log);
                    break;
                case "2":
                    ShowPracticeHistory(log);
                    break;
                case "B":
                case "Q":
                    return;
                default:
                    Console.WriteLine("That choice is not on the menu.");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }

    private void StartPracticeSession(PracticeSessionLog log)
    {
        var duration = ReadPracticeDuration();
        if (duration is null)
        {
            return;
        }

        var focus = ReadPracticeFocus();
        if (focus is null)
        {
            return;
        }

        var plan = PracticeCoachPlanner.BuildPlan(
            new PracticeSessionRequest(duration.Value, focus.Value),
            log,
            DateOnly.FromDateTime(DateTime.Now));
        var results = new Dictionary<string, PracticeBlockResult>();

        while (true)
        {
            Console.Clear();
            WriteHeader("Practice session");
            Console.WriteLine($"{DisplayNameFor(plan.Focus)} focus  |  {plan.DurationMinutes} minutes  |  {results.Count}/{plan.Blocks.Count} blocks marked");
            Console.WriteLine();

            for (var index = 0; index < plan.Blocks.Count; index++)
            {
                var block = plan.Blocks[index];
                var status = results.TryGetValue(block.Id, out var result) ? DisplayNameFor(result.Difficulty) : "open";
                Console.WriteLine($"{index + 1}. [{status}] {block.Area}: {block.Title} ({block.Minutes} min)");
                Console.WriteLine($"   Goal: {block.Goal}");
            }

            Console.WriteLine();
            Console.WriteLine("Choose a block number to open its drill, M = mark without opening, F = finish, B = back");
            Console.Write("Session > ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                if (results.Count > 0 && Confirm("Save the marked blocks before leaving?"))
                {
                    SavePracticeSession(log, plan, results);
                }

                return;
            }

            if (input.Equals("F", StringComparison.OrdinalIgnoreCase))
            {
                SavePracticeSession(log, plan, results);
                ShowPracticeSessionComplete(plan, results);
                return;
            }

            if (input.Equals("M", StringComparison.OrdinalIgnoreCase))
            {
                MarkPracticeBlock(plan, results);
                continue;
            }

            if (int.TryParse(input, out var blockNumber) && blockNumber >= 1 && blockNumber <= plan.Blocks.Count)
            {
                var block = plan.Blocks[blockNumber - 1];
                ShowPracticeBlockIntro(block);
                LaunchPracticeBlock(block);
                results[block.Id] = BuildPracticeResult(block, ReadPracticeDifficulty());
                continue;
            }

            Console.WriteLine("Choose a block number, M, F, or B.");
            Console.ReadKey(intercept: true);
        }
    }

    private void ShowPracticeBlockIntro(PracticeSessionBlock block)
    {
        Console.Clear();
        WriteHeader(block.Title);
        Console.WriteLine($"{block.Area}  |  {block.Minutes} minutes");
        Console.WriteLine();
        Console.WriteLine(block.Goal);
        Console.WriteLine(block.Prompt);
        Console.WriteLine();
        Console.WriteLine("The matching drill opens next. Return with B or Q when you are ready to mark it.");
        Console.WriteLine("Press any key to start.");
        Console.ReadKey(intercept: true);
    }

    private void LaunchPracticeBlock(PracticeSessionBlock block)
    {
        var setup = new TriadProgressionSetup(
            $"Practice coach: {block.Title}",
            block.ProgressionText,
            Bpm: 80,
            TimeSignature: new TimeSignature(4, 4),
            ChordLengths: ChordLengthsFor(block.ProgressionText, 4));

        switch (block.Kind)
        {
            case PracticeBlockKind.Tuner:
                ShowTuner();
                break;
            case PracticeBlockKind.CowboyChords:
                ShowChordDiagramSongGame("Practice coach: cowboy chord changes", setup, BuildCowboySongDiagram);
                break;
            case PracticeBlockKind.TriadMovement:
                ShowTriadProgressionGame("Practice coach: triad movement", _triadGame, setup);
                break;
            case PracticeBlockKind.SpreadTriads:
                ShowTriadProgressionGame("Practice coach: spread triads", _spreadTriadGame, setup);
                break;
            case PracticeBlockKind.JazzGuideTones:
                ShowJazzChordGame("Practice coach: guide-tone ii-V-I", BuildMajorTwoFiveOneSetup(block.KeyRoot), JazzVoicingMode.GuideTones);
                break;
            case PracticeBlockKind.JazzShellVoicings:
                ShowJazzChordGame("Practice coach: shell voicings", BuildMajorTwoFiveOneSetup(block.KeyRoot), JazzVoicingMode.Shell);
                break;
            case PracticeBlockKind.ArpeggioTargeting:
                ShowArpeggioTargetingGame(setup);
                break;
            case PracticeBlockKind.ArpeggioSong:
                ShowArpeggioSongGame(setup);
                break;
            case PracticeBlockKind.RhythmArpeggios:
                ShowRhythmArpeggioPatterns();
                break;
            case PracticeBlockKind.ScaleSong:
                ShowScaleSongGame(block.KeyRoot, ScaleKindFor(block.ScaleKindName), setup);
                break;
            case PracticeBlockKind.IntervalTargets:
                ShowIntervalSongGame(setup, new HashSet<string>(["b3", "3", "5", "b7"]), block.KeyRoot);
                break;
            case PracticeBlockKind.ShredSpeed:
                ShowDailyShredWorkout();
                break;
        }
    }

    private void MarkPracticeBlock(PracticeSessionPlan plan, Dictionary<string, PracticeBlockResult> results)
    {
        Console.Write("Block number > ");
        var input = Console.ReadLine()?.Trim();
        if (!int.TryParse(input, out var blockNumber) || blockNumber < 1 || blockNumber > plan.Blocks.Count)
        {
            Console.WriteLine("That block is not in this session.");
            Console.ReadKey(intercept: true);
            return;
        }

        var block = plan.Blocks[blockNumber - 1];
        results[block.Id] = BuildPracticeResult(block, ReadPracticeDifficulty());
    }

    private static PracticeBlockResult BuildPracticeResult(PracticeSessionBlock block, PracticeDifficulty difficulty)
    {
        return new PracticeBlockResult(block.Id, block.Kind, block.Area, block.Title, block.Minutes, difficulty);
    }

    private static IReadOnlyList<int> ChordLengthsFor(string progressionText, int beatsPerChord)
    {
        var chordCount = progressionText
            .Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Length;

        return Enumerable.Repeat(beatsPerChord, Math.Max(1, chordCount)).ToArray();
    }

    private static PentatonicScaleKind ScaleKindFor(string name)
    {
        return PentatonicLibrary.ScaleKinds.FirstOrDefault(kind => PentatonicLibrary.NameFor(kind).Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static int? ReadPracticeDuration()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Session length");
            Console.WriteLine("5. Quick reset");
            Console.WriteLine("10. Focused short session");
            Console.WriteLine("20. Balanced practice");
            Console.WriteLine("30. Deeper practice");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Minutes > ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(input, out var value) && new[] { 5, 10, 20, 30 }.Contains(value))
            {
                return value;
            }

            Console.WriteLine("Choose 5, 10, 20, 30, or B.");
            Console.ReadKey(intercept: true);
        }
    }

    private static PracticeFocus? ReadPracticeFocus()
    {
        var options = new[]
        {
            PracticeFocus.Mixed,
            PracticeFocus.Beginner,
            PracticeFocus.Triads,
            PracticeFocus.Jazz,
            PracticeFocus.Soloing,
            PracticeFocus.Rhythm,
            PracticeFocus.Shredding
        };

        while (true)
        {
            Console.Clear();
            WriteHeader("Practice focus");
            for (var index = 0; index < options.Length; index++)
            {
                Console.WriteLine($"{index + 1}. {DisplayNameFor(options[index])}");
            }
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Focus > ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(input, out var indexChoice) && indexChoice >= 1 && indexChoice <= options.Length)
            {
                return options[indexChoice - 1];
            }

            Console.WriteLine("Choose a focus number or B.");
            Console.ReadKey(intercept: true);
        }
    }

    private static PracticeDifficulty ReadPracticeDifficulty()
    {
        while (true)
        {
            Console.WriteLine();
            Console.Write("How did it feel? E = easy, O = okay, H = hard, S = skip > ");
            switch ((Console.ReadLine()?.Trim() ?? string.Empty).ToUpperInvariant())
            {
                case "E":
                    return PracticeDifficulty.Easy;
                case "O":
                case "":
                    return PracticeDifficulty.Okay;
                case "H":
                    return PracticeDifficulty.Hard;
                case "S":
                    return PracticeDifficulty.Skipped;
                default:
                    Console.WriteLine("Choose E, O, H, or S.");
                    break;
            }
        }
    }

    private static void SavePracticeSession(
        PracticeSessionLog log,
        PracticeSessionPlan plan,
        IReadOnlyDictionary<string, PracticeBlockResult> results)
    {
        var completedBlocks = plan.Blocks
            .Select(block => results.TryGetValue(block.Id, out var result)
                ? result
                : BuildPracticeResult(block, PracticeDifficulty.Skipped))
            .ToArray();

        log.Entries.Add(new PracticeSessionEntry(DateTimeOffset.Now, plan.DurationMinutes, plan.Focus, completedBlocks));
        SavePracticeLog(log);
    }

    private static void ShowPracticeSessionComplete(
        PracticeSessionPlan plan,
        IReadOnlyDictionary<string, PracticeBlockResult> results)
    {
        Console.Clear();
        WriteHeader("Session saved");
        Console.WriteLine($"{DisplayNameFor(plan.Focus)} practice is logged.");
        Console.WriteLine();

        foreach (var block in plan.Blocks)
        {
            var difficulty = results.TryGetValue(block.Id, out var result)
                ? result.Difficulty
                : PracticeDifficulty.Skipped;
            Console.WriteLine($"{block.Title}: {DisplayNameFor(difficulty)}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue.");
        Console.ReadKey(intercept: true);
    }

    private static void ShowPracticeHistory(PracticeSessionLog log)
    {
        Console.Clear();
        WriteHeader("Practice history");

        if (log.Entries.Count == 0)
        {
            Console.WriteLine("No sessions logged yet.");
        }
        else
        {
            foreach (var entry in log.Entries.OrderByDescending(entry => entry.CompletedAt).Take(10))
            {
                Console.WriteLine($"{entry.CompletedAt.LocalDateTime:g}  {DisplayNameFor(entry.Focus)}  {entry.DurationMinutes} min");
                Console.WriteLine($"   {string.Join(" | ", entry.Blocks.Select(block => $"{block.Title}: {DisplayNameFor(block.Difficulty)}"))}");
            }

            var hardAreas = PracticeCoachPlanner.HardAreas(log);
            if (hardAreas.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"Review soon: {string.Join(", ", hardAreas)}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Log file: {PracticeLogPath()}");
        Console.WriteLine("Press any key to go back.");
        Console.ReadKey(intercept: true);
    }

    private static void WritePracticeSummary(PracticeSessionLog log)
    {
        if (log.Entries.Count == 0)
        {
            Console.WriteLine("No practice sessions logged yet.");
            return;
        }

        var last = log.Entries.OrderByDescending(entry => entry.CompletedAt).First();
        Console.WriteLine($"Last session: {last.CompletedAt.LocalDateTime:g}, {DisplayNameFor(last.Focus)}, {last.DurationMinutes} min");

        var hardAreas = PracticeCoachPlanner.HardAreas(log);
        if (hardAreas.Count > 0)
        {
            Console.WriteLine($"Review bias: {string.Join(", ", hardAreas)}");
        }
    }

    private static PracticeSessionLog LoadPracticeLog()
    {
        var path = PracticeLogPath();
        if (!File.Exists(path))
        {
            return new PracticeSessionLog();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PracticeSessionLog>(json, PracticeJsonOptions) ?? new PracticeSessionLog();
        }
        catch (Exception)
        {
            return new PracticeSessionLog();
        }
    }

    private static void SavePracticeLog(PracticeSessionLog log)
    {
        var path = PracticeLogPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(log, PracticeJsonOptions));
    }

    private static string PracticeLogPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(appData))
        {
            appData = Environment.CurrentDirectory;
        }

        return Path.Combine(appData, "GuitarTUI", "practice-log.json");
    }

    private static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} Y/N > ");
        var input = Console.ReadLine()?.Trim();
        return input is not null && input.Equals("Y", StringComparison.OrdinalIgnoreCase);
    }

    private static string DisplayNameFor(PracticeFocus focus) => focus switch
    {
        PracticeFocus.Mixed => "mixed",
        PracticeFocus.Beginner => "beginner",
        PracticeFocus.Triads => "triads",
        PracticeFocus.Jazz => "jazz",
        PracticeFocus.Soloing => "soloing",
        PracticeFocus.Rhythm => "rhythm",
        PracticeFocus.Shredding => "shredding",
        _ => focus.ToString()
    };

    private static string DisplayNameFor(PracticeDifficulty difficulty) => difficulty switch
    {
        PracticeDifficulty.Easy => "easy",
        PracticeDifficulty.Okay => "okay",
        PracticeDifficulty.Hard => "hard",
        PracticeDifficulty.Skipped => "skipped",
        _ => difficulty.ToString()
    };
}
