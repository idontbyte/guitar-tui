using GuitarResourcesTui.Triads;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private static readonly IReadOnlyList<TunerNote> StandardTuning =
    [
        new("6", "Low E", "E2", 40),
        new("5", "A", "A2", 45),
        new("4", "D", "D3", 50),
        new("3", "G", "G3", 55),
        new("2", "B", "B3", 59),
        new("1", "High E", "E4", 64)
    ];

    private void ShowTuner()
    {
        AudioPlayback? playback = null;

        try
        {
            while (true)
            {
                Console.Clear();
                WriteHeader("Guitar tuner");
                Console.WriteLine("Choose a string to play a reference note.");
                Console.WriteLine("Tune your guitar string until it matches the pitch you hear.");
                Console.WriteLine();
                foreach (var note in StandardTuning)
                {
                    Console.WriteLine($"{note.MenuKey}. {note.Name} string ({note.DisplayName})");
                }
                Console.WriteLine("N. Custom note");
                Console.WriteLine("S. Stop");
                Console.WriteLine("B. Back");
                Console.WriteLine();
                Console.Write("Choose a note > ");

                var input = Console.ReadLine()?.Trim() ?? string.Empty;
                if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (input.Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    playback?.Stop();
                    playback?.Dispose();
                    playback = null;
                    continue;
                }

                TunerNote? noteToPlay = null;
                if (input.Equals("N", StringComparison.OrdinalIgnoreCase))
                {
                    noteToPlay = ReadCustomTunerNote();
                    if (noteToPlay is null)
                    {
                        continue;
                    }
                }
                else
                {
                    noteToPlay = StandardTuning.FirstOrDefault(note => note.MenuKey == input);
                }

                if (noteToPlay is null)
                {
                    Console.WriteLine("Choose 1-6, N, S, or B.");
                    Console.ReadKey(intercept: true);
                    continue;
                }

                playback?.Stop();
                playback?.Dispose();
                playback = PlayTunerNote(noteToPlay);

                Console.WriteLine();
                Console.WriteLine($"Playing {noteToPlay.DisplayName} ({TunerFrequency(noteToPlay.MidiNote):0.00} Hz). Press any key to choose another note.");
                Console.ReadKey(intercept: true);
            }
        }
        finally
        {
            playback?.Stop();
            playback?.Dispose();
        }
    }

    private static TunerNote? ReadCustomTunerNote()
    {
        while (true)
        {
            Console.Write("Note, for example E2, A2, C#3, Bb3, or B > ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (input.Equals("B", StringComparison.OrdinalIgnoreCase) || input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (TryParseTunerNote(input, out var note))
            {
                return note;
            }

            Console.WriteLine("Use a note name plus octave from 0-8, for example E2, C#3, or Bb3.");
        }
    }

    private static bool TryParseTunerNote(string input, out TunerNote note)
    {
        note = new TunerNote(string.Empty, string.Empty, string.Empty, 0);
        var trimmed = input.Trim();
        if (trimmed.Length < 2)
        {
            return false;
        }

        var octaveStart = trimmed.TakeWhile(character => !char.IsDigit(character)).Count();
        if (octaveStart == 0 || octaveStart >= trimmed.Length)
        {
            return false;
        }

        var root = NormalizeRootInput(trimmed[..octaveStart]);
        if (!MusicTheory.ChromaticRoots.Contains(root) || !int.TryParse(trimmed[octaveStart..], out var octave) || octave is < 0 or > 8)
        {
            return false;
        }

        var midiNote = (octave + 1) * 12 + MusicTheory.PitchClassFor(root);
        note = new TunerNote(string.Empty, $"{root} custom", $"{root}{octave}", midiNote);
        return true;
    }
}
