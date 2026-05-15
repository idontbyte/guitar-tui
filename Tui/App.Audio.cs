using GuitarResourcesTui.Triads;
using GuitarResourcesTui.JazzChords;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace GuitarResourcesTui.Tui;

public sealed partial class App
{
    private static readonly List<AudioPlayback> ClickPlaybacks = [];
    private static readonly object ClickPlaybackLock = new();

    private static void Click(bool accent)
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        try
        {
            lock (ClickPlaybackLock)
            {
                CleanupFinishedProcesses(ClickPlaybacks);
                var playback = PlayAudioFile(EnsureMetronomeClickFile(accent));
                if (playback is not null)
                {
                    ClickPlaybacks.Add(playback);
                    return;
                }
            }
        }
        catch
        {
            // Fall through to the simplest platform beep.
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                Console.Beep(accent ? 1568 : 1175, 18);
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

    private static AudioPlayback? PlayBackingChord(ChordSymbol chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        return PlayBackingChord(BackingChordFor(chord), beatsPerChord, bpm, timeSignature);
    }

    private static AudioPlayback? PlayBackingChord(JazzChordSymbol chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        return PlayBackingChord(BackingChordFor(chord), beatsPerChord, bpm, timeSignature);
    }

    private static AudioPlayback? PlayBackingChord(BackingChord chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        var path = EnsureBackingChordFile(chord, beatsPerChord, bpm, timeSignature);
        return PlayAudioFile(path);
    }

    private static AudioPlayback? PlayAudioFile(string path)
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "afplay",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ArgumentList = { path }
                });

                return process is null ? null : new MacAudioPlayback(process);
            }

            if (OperatingSystem.IsWindows())
            {
                return WindowsAudioPlayback.Play(path);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static AudioPlayback? PlayTunerNote(TunerNote note)
    {
        var path = EnsureTunerNoteFile(note);
        return PlayAudioFile(path);
    }

    private static string EnsureMetronomeClickFile(bool accent)
    {
        var directory = Path.Combine(Path.GetTempPath(), "guitar-tui-click");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, accent ? "accent-v2.wav" : "plain-v2.wav");
        if (!File.Exists(path))
        {
            WriteMetronomeClickWav(path, accent);
        }

        return path;
    }

    private static void WriteMetronomeClickWav(string path, bool accent)
    {
        const int sampleRate = 44100;
        const short channels = 1;
        const short bitsPerSample = 16;
        const double durationSeconds = 0.045d;
        var samples = (int)(sampleRate * durationSeconds);
        var dataSize = samples * channels * bitsPerSample / 8;
        var frequency = accent ? 1760d : 1175d;

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
            var attack = Math.Min(1d, time / 0.0015d);
            var envelope = attack * Math.Exp(-time * 95d);
            var tone = Math.Sin(2d * Math.PI * frequency * time)
                + 0.22d * Math.Sin(2d * Math.PI * frequency * 2.01d * time);
            var value = Math.Tanh(tone * 1.2d) * envelope * (accent ? 0.78d : 0.62d);
            writer.Write((short)(value * short.MaxValue));
        }
    }

    private static string EnsureTunerNoteFile(TunerNote note)
    {
        var directory = Path.Combine(Path.GetTempPath(), "guitar-tui-tuner");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"{note.DisplayName.Replace('#', 's')}-v1.wav");
        if (!File.Exists(path))
        {
            WriteTunerNoteWav(path, note);
        }

        return path;
    }

    private static void WriteTunerNoteWav(string path, TunerNote note)
    {
        const int sampleRate = 44100;
        const short channels = 1;
        const short bitsPerSample = 16;
        const double durationSeconds = 5d;
        var samples = (int)(sampleRate * durationSeconds);
        var dataSize = samples * channels * bitsPerSample / 8;
        var frequency = TunerFrequency(note.MidiNote);

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
            var value = TunerTone(frequency, time, durationSeconds);
            writer.Write((short)(value * short.MaxValue));
        }
    }

    private static void WarmBackingChords(IReadOnlyList<TriadPracticeItem> phrase, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < phrase.Count; index++)
        {
            EnsureBackingChordFile(BackingChordFor(phrase[index].Chord), chordLengths[index], bpm, timeSignature);
        }
    }

    private static void WarmBackingChords(IReadOnlyList<ChordSymbol> progression, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < progression.Count; index++)
        {
            EnsureBackingChordFile(BackingChordFor(progression[index]), chordLengths[index], bpm, timeSignature);
        }
    }

    private static void WarmIntervalBackingChords(IReadOnlyList<IntervalPrompt> prompts, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < prompts.Count; index++)
        {
            EnsureBackingChordFile(prompts[index].BackingChord, chordLengths[index], bpm, timeSignature);
        }
    }

    private static void WarmJazzBackingChords(IReadOnlyList<JazzChordSymbol> progression, IReadOnlyList<int> chordLengths, int bpm, TimeSignature timeSignature)
    {
        for (var index = 0; index < progression.Count; index++)
        {
            EnsureBackingChordFile(BackingChordFor(progression[index]), chordLengths[index], bpm, timeSignature);
        }
    }

    private static string EnsureBackingChordFile(BackingChord chord, int beatsPerChord, int bpm, TimeSignature timeSignature)
    {
        var durationSeconds = beatsPerChord * 60d / bpm;
        var overlapSeconds = Math.Clamp(durationSeconds * 0.38, 0.32, 0.8);
        var path = BackingChordFilePath(chord, durationSeconds, overlapSeconds, bpm, timeSignature);
        if (!File.Exists(path))
        {
            WriteBackingChordWav(path, chord, beatsPerChord, bpm, timeSignature, durationSeconds, overlapSeconds);
        }

        return path;
    }

    private static string BackingChordFilePath(BackingChord chord, double durationSeconds, double overlapSeconds, int bpm, TimeSignature timeSignature)
    {
        var directory = Path.Combine(Path.GetTempPath(), "guitar-tui-backing");
        Directory.CreateDirectory(directory);

        var durationKey = Math.Round(durationSeconds, 3).ToString("0.000").Replace('.', '-');
        var overlapKey = Math.Round(overlapSeconds, 3).ToString("0.000").Replace('.', '-');
        var intervalKey = string.Join("-", chord.Intervals.OrderBy(LabelSortOrderForBacking)).Replace("#", "s", StringComparison.Ordinal);
        return Path.Combine(directory, $"{chord.Root.Replace('#', 's')}-{chord.Quality}-{chord.Suffix}-{intervalKey}-{bpm}-{timeSignature.DisplayName.Replace('/', '-')}-{durationKey}-{overlapKey}-v8.wav");
    }

    private static void WriteBackingChordWav(
        string path,
        BackingChord chord,
        int beatsPerChord,
        int bpm,
        TimeSignature timeSignature,
        double durationSeconds,
        double overlapSeconds)
    {
        const int sampleRate = 44100;
        const short channels = 1;
        const short bitsPerSample = 16;
        var totalDurationSeconds = durationSeconds + overlapSeconds;
        var samples = Math.Max(1, (int)(sampleRate * totalDurationSeconds));
        var dataSize = samples * channels * bitsPerSample / 8;
        var guitarFrequencies = GuitarChordFrequencies(chord).ToArray();
        var bassFrequency = BassFrequency(chord);
        var secondsPerBeat = 60d / bpm;

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
            var beatPosition = time / secondsPerBeat;
            var barBeat = (int)Math.Floor(beatPosition) % timeSignature.BeatsPerBar;
            var value = 0d;

            for (var beat = 0; beat < beatsPerChord; beat++)
            {
                var beatStart = beat * secondsPerBeat;
                var beatOffset = time - beatStart;
                if (beatOffset < 0 || beatOffset > secondsPerBeat)
                {
                    continue;
                }

                var accent = beat % timeSignature.BeatsPerBar == 0 ? 1.15 : 0.82;
                value += DrumKit(beatOffset, barBeat, timeSignature.BeatsPerBar) * 0.32;
                value += BassNote(bassFrequency, beatOffset) * (beat % timeSignature.BeatsPerBar == 0 ? 0.34 : 0.2);

                foreach (var (frequency, stringIndex) in guitarFrequencies.Select((frequency, index) => (frequency, index)))
                {
                    var stringDelay = stringIndex * 0.012;
                    value += GuitarString(frequency, beatOffset - stringDelay) * accent * 0.18;
                }
            }

            value += ChordWash(guitarFrequencies, time, totalDurationSeconds, durationSeconds) * 0.18;
            value = Math.Tanh(value) * 0.55d;
            writer.Write((short)(value * short.MaxValue));
        }
    }

    private static BackingChord BackingChordFor(ChordSymbol chord)
    {
        var intervals = chord.Quality == ChordQuality.Minor
            ? new HashSet<string>(["R", "b3", "5"])
            : new HashSet<string>(["R", "3", "5"]);

        return new BackingChord(chord.Root, chord.Quality, intervals, chord.Quality == ChordQuality.Minor ? "m" : string.Empty);
    }

    private static BackingChord BackingChordFor(JazzChordSymbol chord)
    {
        var quality = chord.Quality.Intervals.Contains("b3") ? ChordQuality.Minor : ChordQuality.Major;
        return new BackingChord(chord.Root, quality, chord.Quality.Intervals.ToHashSet(), chord.Quality.Suffix);
    }

    private static IEnumerable<double> GuitarChordFrequencies(ChordSymbol chord)
    {
        return GuitarChordFrequencies(BackingChordFor(chord));
    }

    private static IEnumerable<double> GuitarChordFrequencies(BackingChord chord)
    {
        var rootPitch = MusicTheory.PitchClassFor(chord.Root);
        var midiRoot = 48 + rootPitch;

        while (midiRoot > 64)
        {
            midiRoot -= 12;
        }

        var intervals = chord.Intervals
            .Select(IntervalSemitones)
            .Distinct()
            .Order()
            .ToArray();

        return intervals
            .Concat(intervals.Select(interval => interval + 12))
            .Take(6)
            .Select(interval => MidiToFrequency(midiRoot + interval));
    }

    private static double BassFrequency(BackingChord chord)
    {
        var midiRoot = 36 + MusicTheory.PitchClassFor(chord.Root);
        while (midiRoot > 47)
        {
            midiRoot -= 12;
        }

        return MidiToFrequency(midiRoot);
    }

    private static int IntervalSemitones(string interval) => interval switch
    {
        "R" => 0,
        "b2" or "b9" => 1,
        "2" or "9" => 2,
        "b3" or "#9" => 3,
        "3" => 4,
        "4" or "11" => 5,
        "b5" or "#11" => 6,
        "5" => 7,
        "b6" or "b13" => 8,
        "6" or "13" or "bb7" => 9,
        "b7" => 10,
        "7" => 11,
        _ => 0
    };

    private static int LabelSortOrderForBacking(string interval) => IntervalSemitones(interval);

    private static double GuitarString(double frequency, double time)
    {
        if (time < 0)
        {
            return 0;
        }

        var envelope = Math.Exp(-time * 2.6) * Math.Min(1, time / 0.008);
        var shimmer = Math.Sin(2d * Math.PI * frequency * time)
            + 0.45d * Math.Sin(2d * Math.PI * frequency * 2d * time)
            + 0.18d * Math.Sin(2d * Math.PI * frequency * 3d * time);

        return Math.Tanh(shimmer * 0.9) * envelope;
    }

    private static double BassNote(double frequency, double time)
    {
        if (time < 0)
        {
            return 0;
        }

        var envelope = Math.Exp(-time * 2.4) * Math.Min(1, time / 0.014);
        return Math.Sin(2d * Math.PI * frequency * time) * envelope;
    }

    private static double DrumKit(double time, int barBeat, int beatsPerBar)
    {
        var kick = Kick(time) * (barBeat == 0 ? 1.2 : 0.55);
        var snare = (beatsPerBar == 3 ? barBeat == 2 : barBeat == 1 || barBeat == 3) ? Snare(time) : 0d;
        var hat = HiHat(time) * 0.45;
        return kick + snare + hat;
    }

    private static double Kick(double time)
    {
        if (time < 0 || time > 0.16)
        {
            return 0;
        }

        var frequency = 90d - 42d * (time / 0.16);
        return Math.Sin(2d * Math.PI * frequency * time) * Math.Exp(-time * 23d);
    }

    private static double Snare(double time)
    {
        if (time < 0 || time > 0.12)
        {
            return 0;
        }

        var noise = Math.Sin((time * 44100d + 17d) * 12.9898d) * 43758.5453d;
        noise -= Math.Floor(noise);
        return (noise * 2d - 1d) * Math.Exp(-time * 28d);
    }

    private static double HiHat(double time)
    {
        if (time < 0 || time > 0.055)
        {
            return 0;
        }

        var noise = Math.Sin((time * 44100d + 91d) * 78.233d) * 12345.6789d;
        noise -= Math.Floor(noise);
        return (noise * 2d - 1d) * Math.Exp(-time * 70d);
    }

    private static double ChordWash(IReadOnlyList<double> frequencies, double time, double totalDuration, double releaseStart)
    {
        var envelope = Envelope(time, totalDuration, releaseStart);
        var value = 0d;

        foreach (var frequency in frequencies)
        {
            value += Math.Sin(2d * Math.PI * frequency * time);
        }

        return value / frequencies.Count * envelope;
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

    private static double TunerFrequency(int midiNote) => MidiToFrequency(midiNote);

    private static double TunerTone(double frequency, double time, double durationSeconds)
    {
        var attack = Math.Min(1, time / 0.025);
        var releaseStart = durationSeconds - 0.3;
        var release = time > releaseStart
            ? Math.Max(0, (durationSeconds - time) / 0.3)
            : 1;
        var envelope = attack * release;
        var tone = Math.Sin(2d * Math.PI * frequency * time)
            + 0.18d * Math.Sin(2d * Math.PI * frequency * 2d * time)
            + 0.08d * Math.Sin(2d * Math.PI * frequency * 3d * time);

        return Math.Tanh(tone * 0.85d) * envelope * 0.7d;
    }

    private sealed class VoiceAnnouncer : IDisposable
    {
        private const int MacVoiceRate = 210;
        private const string MacDefaultVoiceVolumePrefix = "[[volm 0.7]] ";
        private const string MacLoudVoiceVolumePrefix = "[[volm 1.0]] ";

        private dynamic? _windowsVoice;
        private Process? _macVoiceProcess;

        public void SayChord(ChordSymbol chord)
        {
            var text = SpokenChordName(chord);

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    SayWindows(text);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    SayMac(text);
                }
            }
            catch
            {
                // Spoken hints are optional; the game should keep flowing if TTS is unavailable.
            }
        }

        public void Stop()
        {
            try
            {
                if (_windowsVoice is not null)
                {
                    _windowsVoice.Speak(string.Empty, 2);
                }
            }
            catch
            {
                // Best effort.
            }

            try
            {
                if (_macVoiceProcess is { HasExited: false })
                {
                    _macVoiceProcess.Kill();
                }
            }
            catch
            {
                // Best effort.
            }
        }

        public void Dispose()
        {
            Stop();

            if (_windowsVoice is null)
            {
                return;
            }

            try
            {
                Marshal.FinalReleaseComObject(_windowsVoice);
            }
            catch
            {
                // Best effort.
            }

            _windowsVoice = null;
        }

        [SupportedOSPlatform("windows")]
        private void SayWindows(string text)
        {
            _windowsVoice ??= CreateWindowsVoice();
            if (_windowsVoice is null)
            {
                return;
            }

            _windowsVoice.Volume = 70;
            _windowsVoice.Rate = 1;
            _windowsVoice.Speak(text, 3);
        }

        [SupportedOSPlatform("windows")]
        private static dynamic? CreateWindowsVoice()
        {
            var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
            return voiceType is null ? null : Activator.CreateInstance(voiceType);
        }

        private void SayMac(string text)
        {
            Stop();
            var volumePrefix = text == "a"
                ? MacLoudVoiceVolumePrefix
                : MacDefaultVoiceVolumePrefix;
            _macVoiceProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "say",
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList = { "-r", MacVoiceRate.ToString(), $"{volumePrefix}{text}" }
            });
        }

        private static string SpokenChordName(ChordSymbol chord)
        {
            var root = chord.Root.Replace("#", " sharp ", StringComparison.Ordinal).Trim();
            if (chord.Quality == ChordQuality.Minor)
            {
                return $"{root} minor";
            }

            return root == "A" ? "a" : root;
        }
    }

    private abstract class AudioPlayback : IDisposable
    {
        public abstract bool HasFinished { get; }

        public abstract void Stop();

        public abstract void Dispose();
    }

    private sealed class MacAudioPlayback(Process process) : AudioPlayback
    {
        public override bool HasFinished
        {
            get
            {
                try
                {
                    return process.HasExited;
                }
                catch
                {
                    return true;
                }
            }
        }

        public override void Stop()
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
                // Best effort: a finished audio process is harmless.
            }
        }

        public override void Dispose() => process.Dispose();
    }

    private sealed class WindowsAudioPlayback : AudioPlayback
    {
        private const uint WaveMapper = uint.MaxValue;
        private const uint WhdrDone = 0x00000001;
        private readonly byte[] _audioData;
        private readonly GCHandle _audioHandle;
        private readonly IntPtr _headerPointer;
        private IntPtr _waveOut;
        private bool _closed;

        private WindowsAudioPlayback(IntPtr waveOut, byte[] audioData, GCHandle audioHandle, IntPtr headerPointer)
        {
            _waveOut = waveOut;
            _audioData = audioData;
            _audioHandle = audioHandle;
            _headerPointer = headerPointer;
        }

        public static WindowsAudioPlayback? Play(string path)
        {
            const int wavHeaderBytes = 44;
            var fileBytes = File.ReadAllBytes(path);
            if (fileBytes.Length <= wavHeaderBytes)
            {
                return null;
            }

            var audioData = fileBytes[wavHeaderBytes..];
            var audioHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);
            var headerPointer = IntPtr.Zero;
            var waveOut = IntPtr.Zero;
            var started = false;

            try
            {
                var format = new WaveFormat
                {
                    FormatTag = 1,
                    Channels = 1,
                    SamplesPerSec = 44100,
                    AvgBytesPerSec = 44100 * 2,
                    BlockAlign = 2,
                    BitsPerSample = 16,
                    Size = 0
                };

                if (waveOutOpen(out waveOut, WaveMapper, ref format, IntPtr.Zero, IntPtr.Zero, 0) != 0)
                {
                    return null;
                }

                var header = new WaveHeader
                {
                    Data = audioHandle.AddrOfPinnedObject(),
                    BufferLength = (uint)audioData.Length
                };

                var headerSize = Marshal.SizeOf<WaveHeader>();
                headerPointer = Marshal.AllocHGlobal(headerSize);
                Marshal.StructureToPtr(header, headerPointer, false);

                if (waveOutPrepareHeader(waveOut, headerPointer, (uint)headerSize) != 0)
                {
                    return null;
                }

                if (waveOutWrite(waveOut, headerPointer, (uint)headerSize) != 0)
                {
                    return null;
                }

                started = true;
                return new WindowsAudioPlayback(waveOut, audioData, audioHandle, headerPointer);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (!started)
                {
                    if (headerPointer != IntPtr.Zero)
                    {
                        if (waveOut != IntPtr.Zero)
                        {
                            waveOutUnprepareHeader(waveOut, headerPointer, (uint)Marshal.SizeOf<WaveHeader>());
                        }

                        Marshal.FreeHGlobal(headerPointer);
                    }

                    if (waveOut != IntPtr.Zero)
                    {
                        waveOutClose(waveOut);
                    }

                    if (audioHandle.IsAllocated)
                    {
                        audioHandle.Free();
                    }
                }
            }
        }

        public override bool HasFinished
        {
            get
            {
                if (_closed)
                {
                    return true;
                }

                var header = Marshal.PtrToStructure<WaveHeader>(_headerPointer);
                return (header.Flags & WhdrDone) == WhdrDone;
            }
        }

        public override void Stop() => Dispose();

        public override void Dispose()
        {
            if (_closed)
            {
                return;
            }

            if (_waveOut != IntPtr.Zero)
            {
                waveOutReset(_waveOut);
                waveOutUnprepareHeader(_waveOut, _headerPointer, (uint)Marshal.SizeOf<WaveHeader>());
                waveOutClose(_waveOut);
                _waveOut = IntPtr.Zero;
            }

            Marshal.FreeHGlobal(_headerPointer);
            _audioHandle.Free();
            GC.KeepAlive(_audioData);
            _closed = true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WaveFormat
        {
            public ushort FormatTag;
            public ushort Channels;
            public uint SamplesPerSec;
            public uint AvgBytesPerSec;
            public ushort BlockAlign;
            public ushort BitsPerSample;
            public ushort Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WaveHeader
        {
            public IntPtr Data;
            public uint BufferLength;
            public uint BytesRecorded;
            public IntPtr User;
            public uint Flags;
            public uint Loops;
            public IntPtr Next;
            public IntPtr Reserved;
        }

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutOpen(out IntPtr waveOut, uint deviceId, ref WaveFormat format, IntPtr callback, IntPtr instance, uint flags);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutPrepareHeader(IntPtr waveOut, IntPtr waveHeader, uint waveHeaderSize);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutWrite(IntPtr waveOut, IntPtr waveHeader, uint waveHeaderSize);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutReset(IntPtr waveOut);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutUnprepareHeader(IntPtr waveOut, IntPtr waveHeader, uint waveHeaderSize);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern int waveOutClose(IntPtr waveOut);
    }

    private static void StopProcesses(List<AudioPlayback> processes)
    {
        foreach (var process in processes)
        {
            process.Stop();
            process.Dispose();
        }

        processes.Clear();
    }

    private static void CleanupFinishedProcesses(List<AudioPlayback> processes)
    {
        processes.RemoveAll(process =>
        {
            try
            {
                if (!process.HasFinished)
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
}
