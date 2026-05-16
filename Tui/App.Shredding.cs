using System.Text.Json;
using GuitarResourcesTui.Shredding;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private void ShowShreddingMenu()
    {
        while (true)
        {
            Console.Clear();
            WriteHeader("Shredding");
            WriteDescriptionLine("Shredding practice is clean-speed training: relaxed hands, tiny motion, even timing, and honest tempo tracking.");
            Console.WriteLine();
            WriteMenuOption("1", "Technique path", "Work through the core skills in a sensible order.");
            WriteMenuOption("2", "Finger independence drills", "Classic fretting-hand patterns like 1-2-3-4 and spider walks.");
            WriteMenuOption("3", "Picking drills", "Alternate picking, string crossing, and picking-hand control.");
            WriteMenuOption("4", "Synchronization drills", "Lock the pick and fretting hand together.");
            WriteMenuOption("5", "Scale sequencing", "Turn scale shapes into fast musical cells.");
            WriteMenuOption("6", "Speed builder", "Track clean reps and gradually raise the metronome.");
            WriteMenuOption("7", "Daily shred workout", "Get a short routine biased toward neglected or messy drills.");
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose an option > ");

            switch (Console.ReadLine()?.Trim().ToUpperInvariant())
            {
                case "1":
                    ShowShredTechniquePath();
                    break;
                case "2":
                    ShowShredDrillBrowser(ShredDrillCategory.FingerIndependence);
                    break;
                case "3":
                    ShowShredDrillBrowser(ShredDrillCategory.Picking);
                    break;
                case "4":
                    ShowShredDrillBrowser(ShredDrillCategory.Synchronization);
                    break;
                case "5":
                    ShowShredDrillBrowser(ShredDrillCategory.ScaleSequencing);
                    break;
                case "6":
                    ShowShredSpeedBuilder();
                    break;
                case "7":
                    ShowDailyShredWorkout();
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

    private static void ShowShredTechniquePath()
    {
        Console.Clear();
        WriteHeader("Shred technique path");
        WriteDescriptionLine("Use this order when speed work feels scattered:");
        Console.WriteLine();
        WriteDescriptionLine("1. Tune and relax the hands.");
        WriteDescriptionLine("2. Chromatic 1-2-3-4 for basic finger independence.");
        WriteDescriptionLine("3. One-string alternate picking for pick motion.");
        WriteDescriptionLine("4. Outside and inside picking for string crossing.");
        WriteDescriptionLine("5. Synchronization 1-2-4 and 1-3-4 for hand locking.");
        WriteDescriptionLine("6. Three-note scale sequences for musical speed.");
        WriteDescriptionLine("7. Legato drills for hammer-on and pull-off evenness.");
        WriteDescriptionLine("8. Four-note bursts for brief high-speed relaxed motion.");
        Console.WriteLine();
        Console.WriteLine("Rules");
        WriteDescriptionLine("- Clean and relaxed beats fast and tense.");
        WriteDescriptionLine("- Raise tempo only after repeatable clean reps.");
        WriteDescriptionLine("- If the shoulder, forearm, or thumb tightens, slow down.");
        WriteDescriptionLine("- Muting matters: unused strings should stay quiet.");
        Console.WriteLine();
        WriteDescriptionLine("Press any key to go back.");
        Console.ReadKey(intercept: true);
    }

    private void ShowShredDrillBrowser(ShredDrillCategory category)
    {
        var drills = ShredLibrary.Drills
            .Where(drill => drill.Category == category)
            .ToArray();

        while (true)
        {
            Console.Clear();
            WriteHeader($"{ShredLibrary.DisplayNameFor(category)} drills");
            for (var index = 0; index < drills.Length; index++)
            {
                Console.WriteLine($"{index + 1}. {drills[index].Title}");
                Console.WriteLine($"   {drills[index].Goal}");
            }
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a drill > ");

            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (int.TryParse(input, out var drillNumber) && drillNumber >= 1 && drillNumber <= drills.Length)
            {
                ShowShredDrillReference(drills[drillNumber - 1]);
                continue;
            }

            Console.WriteLine("That choice is not on the menu.");
            Console.ReadKey(intercept: true);
        }
    }

    private void ShowShredDrillReference(ShredDrill drill)
    {
        var startFret = 5;

        while (true)
        {
            var exercise = ShredLibrary.BuildExercise(drill, startFret);
            Console.Clear();
            WriteHeader(drill.Title);
            Console.WriteLine($"{ShredLibrary.DisplayNameFor(drill.Category)}  |  start {drill.StartBpm} BPM  |  goal {drill.GoalBpm} BPM");
            Console.WriteLine();
            Console.WriteLine(drill.Goal);
            Console.WriteLine($"Listen for: {drill.ListenFor}");
            Console.WriteLine($"Practice note: {drill.PracticeNote}");
            Console.WriteLine();

            foreach (var line in ShredLibrary.RenderTab(exercise))
            {
                Console.WriteLine(line);
            }

            Console.WriteLine();
            foreach (var line in ShredLibrary.RenderFingerLine(exercise))
            {
                Console.WriteLine(line);
            }

            Console.WriteLine();
            Console.WriteLine("-/+ = move position, S = speed builder, B/Q = back");

            switch (Console.ReadKey(intercept: true).Key)
            {
                case ConsoleKey.OemMinus:
                case ConsoleKey.Subtract:
                    startFret = Math.Max(1, startFret - 1);
                    break;
                case ConsoleKey.OemPlus:
                case ConsoleKey.Add:
                    startFret = Math.Min(17, startFret + 1);
                    break;
                case ConsoleKey.S:
                    RunShredSpeedBuilder(drill);
                    break;
                case ConsoleKey.B:
                case ConsoleKey.Escape:
                    return;
                case ConsoleKey.Q:
                    return;
            }
        }
    }

    private void ShowShredSpeedBuilder()
    {
        var drill = ReadShredDrillChoice("Speed builder");
        if (drill is null)
        {
            return;
        }

        RunShredSpeedBuilder(drill);
    }

    private void ShowDailyShredWorkout()
    {
        var log = LoadShredPracticeLog();
        var drills = ShredLibrary.BuildDailyWorkout(log);

        while (true)
        {
            Console.Clear();
            WriteHeader("Daily shred workout");
            WriteDescriptionLine("A short routine biased toward new, messy, tense, or neglected drills.");
            Console.WriteLine();

            for (var index = 0; index < drills.Count; index++)
            {
                var record = log.Records.FirstOrDefault(item => item.DrillId == drills[index].Id);
                var top = record is null || record.TopCleanBpm == 0 ? "no clean tempo yet" : $"top clean {record.TopCleanBpm} BPM";
                Console.WriteLine($"{index + 1}. {drills[index].Title} ({top})");
                Console.WriteLine($"   {drills[index].Goal}");
            }

            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Choose a drill > ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (int.TryParse(input, out var drillNumber) && drillNumber >= 1 && drillNumber <= drills.Count)
            {
                RunShredSpeedBuilder(drills[drillNumber - 1]);
                log = LoadShredPracticeLog();
                drills = ShredLibrary.BuildDailyWorkout(log);
                continue;
            }

            Console.WriteLine("Choose a drill number or B.");
            Console.ReadKey(intercept: true);
        }
    }

    private ShredDrill? ReadShredDrillChoice(string title)
    {
        var drills = ShredLibrary.Drills;

        while (true)
        {
            Console.Clear();
            WriteHeader(title);
            for (var index = 0; index < drills.Count; index++)
            {
                Console.WriteLine($"{index + 1}. {drills[index].Title} ({ShredLibrary.DisplayNameFor(drills[index].Category)})");
            }
            Console.WriteLine("B. Back");
            Console.WriteLine();
            Console.Write("Drill > ");

            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(input, out var drillNumber) && drillNumber >= 1 && drillNumber <= drills.Count)
            {
                return drills[drillNumber - 1];
            }

            Console.WriteLine("Choose a drill number or B.");
            Console.ReadKey(intercept: true);
        }
    }

    private void RunShredSpeedBuilder(ShredDrill drill)
    {
        var log = LoadShredPracticeLog();
        var state = ShredLibrary.StartingTempoFor(drill, log);
        var startFret = 5;
        var clickEnabled = true;
        var message = "Play with the click. Mark only honest clean reps as clean.";
        var lastClick = DateTime.MinValue;
        var beat = 0;

        while (true)
        {
            var exercise = ShredLibrary.BuildExercise(drill, startFret);
            RenderShredSpeedBuilder(drill, exercise, state, clickEnabled, message);

            while (true)
            {
                if (clickEnabled)
                {
                    var interval = TimeSpan.FromMinutes(1d / state.CurrentBpm);
                    if (DateTime.UtcNow - lastClick >= interval)
                    {
                        Click(beat % 4 == 0);
                        beat++;
                        lastClick = DateTime.UtcNow;
                    }
                }

                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(5);
                    continue;
                }

                var key = Console.ReadKey(intercept: true).Key;
                switch (key)
                {
                    case ConsoleKey.C:
                        (state, message) = ApplyAndRecordShredAttempt(log, drill, state, ShredAttemptResult.Clean);
                        break;
                    case ConsoleKey.M:
                        (state, message) = ApplyAndRecordShredAttempt(log, drill, state, ShredAttemptResult.Messy);
                        break;
                    case ConsoleKey.T:
                        (state, message) = ApplyAndRecordShredAttempt(log, drill, state, ShredAttemptResult.Tense);
                        break;
                    case ConsoleKey.Spacebar:
                        clickEnabled = !clickEnabled;
                        message = clickEnabled ? "Click on." : "Click muted.";
                        break;
                    case ConsoleKey.OemMinus:
                    case ConsoleKey.Subtract:
                        state = state with { CurrentBpm = Math.Max(30, state.CurrentBpm - 5), CleanStreak = 0 };
                        message = $"Manual tempo change: {state.CurrentBpm} BPM.";
                        break;
                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Add:
                        state = state with { CurrentBpm = Math.Min(state.GoalBpm, state.CurrentBpm + 5), CleanStreak = 0 };
                        message = $"Manual tempo change: {state.CurrentBpm} BPM.";
                        break;
                    case ConsoleKey.LeftArrow:
                        startFret = Math.Max(1, startFret - 1);
                        message = $"Moved to fret {startFret}.";
                        break;
                    case ConsoleKey.RightArrow:
                        startFret = Math.Min(17, startFret + 1);
                        message = $"Moved to fret {startFret}.";
                        break;
                    case ConsoleKey.B:
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        return;
                    default:
                        continue;
                }

                break;
            }
        }
    }

    private static void RenderShredSpeedBuilder(
        ShredDrill drill,
        ShredExercise exercise,
        ShredTempoState state,
        bool clickEnabled,
        string message)
    {
        Console.Clear();
        WriteHeader("Speed builder");
        Console.WriteLine($"{drill.Title}  |  {ShredLibrary.DisplayNameFor(drill.Category)}");
        Console.WriteLine($"BPM: {state.CurrentBpm}  Top clean: {state.TopCleanBpm}  Goal: {state.GoalBpm}  Clean reps: {state.CleanStreak}/3  Click: {(clickEnabled ? "on" : "muted")}");
        Console.WriteLine("C = clean, M = messy, T = tense, Space = click, -/+ = tempo, arrows = position, B/Q = back");
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine();
        Console.WriteLine($"Goal: {drill.Goal}");
        Console.WriteLine($"Listen for: {drill.ListenFor}");
        Console.WriteLine();

        foreach (var line in ShredLibrary.RenderTab(exercise))
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
        foreach (var line in ShredLibrary.RenderFingerLine(exercise))
        {
            Console.WriteLine(line);
        }
    }

    private static (ShredTempoState State, string Message) ApplyAndRecordShredAttempt(
        ShredPracticeLog log,
        ShredDrill drill,
        ShredTempoState state,
        ShredAttemptResult result)
    {
        var update = ShredLibrary.ApplyAttempt(state, result);
        ShredLibrary.RecordAttempt(log, drill, update.State, result, DateTimeOffset.Now);
        SaveShredPracticeLog(log);
        return (update.State, update.Message);
    }

    private static ShredPracticeLog LoadShredPracticeLog()
    {
        var path = ShredPracticeLogPath();
        if (!File.Exists(path))
        {
            return new ShredPracticeLog();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ShredPracticeLog>(json, PracticeJsonOptions) ?? new ShredPracticeLog();
        }
        catch
        {
            return new ShredPracticeLog();
        }
    }

    private static void SaveShredPracticeLog(ShredPracticeLog log)
    {
        var path = ShredPracticeLogPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(log, PracticeJsonOptions));
    }

    private static string ShredPracticeLogPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(appData))
        {
            appData = Environment.CurrentDirectory;
        }

        return Path.Combine(appData, "GuitarTUI", "shred-log.json");
    }
}
