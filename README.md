# Guitar TUI

Guitar TUI is a terminal-based guitar practice tool for learning fretboard shapes, chord functions, song progressions, and timing. It combines reference diagrams with interactive music games, click/backing playback, printable sheets, and optional spoken chord prompts.

## Install

### Windows

Download the Windows `.exe` release and run it.

The Windows app is packaged as a self-contained executable, so you do not need to install the .NET SDK just to use the app.

### macOS

Download the macOS `.dmg`, open it, then drag `Guitar TUI` into `Applications`.

On first launch, macOS may ask you to confirm that you want to open an app downloaded from the internet.

## Run From Source

Install the .NET SDK that supports the project target framework, then run:

```bash
dotnet run
```

The project currently targets `net10.0`.

## Main Menu

Guitar TUI opens with these areas:

- `Tuner`: play reference notes for the guitar strings or a custom note.
- `Cowboy chords`: view open-position chord diagrams and song-mode practice.
- `Triads`: study triad inversions or run triad music games.
- `Scales`: view scale shapes, practise songs against scale positions, or ask the app for song-library scale suggestions.
- `Intervals`: look up interval functions on the fretboard or practise interval targeting over songs.

Most screens support `B` for back and `Q` for the main menu. Music games also show their active controls in the header while they run.

The shared fretboard renderer always shows all six strings and keeps the nut / fret 0 column visually distinct.

## Features

### Tuner

- Plays reference tones for standard guitar strings.
- Supports custom note input such as `E2`, `C#3`, or `Bb3`.
- Uses platform audio playback where available.

### Cowboy Chords

- Shows open-position chord references.
- Includes song mode for practising chord changes in context.
- Displays root, third, fifth, and muted-string labels with color coding.

### Triads

- Shows major and minor triad inversions across adjacent string groupings.
- Supports spread triad practice.
- Labels root, third, and fifth positions directly on the fretboard.
- Marks common lower-position shapes.
- Includes sharp and flat root choices through the chromatic root menu.
- Can open a printable triad inversion sheet from the triad reference screen.

### Triad Music Games

- Build practice phrases from simple mode, custom progressions, or the song library.
- Highlight the current triad while showing upcoming shapes.
- Control tempo and playback while practising with a built-in metronome.
- Supports click, backing chords, and optional spoken chord prompts.
- Includes both close-voiced triad and spread-triad game modes.
- Chooses nearby inversions so progressions can be practised as connected fretboard movement.
- Can filter the game to root position only, first inversion only, or second inversion only.
- Offers a full phrase view and a rolling current-plus-next view.

Useful in-game controls include:

- `Space`: pause or resume.
- `-` / `+`: decrease or increase tempo.
- `I`: cycle the triad inversion filter.
- `M`: mute or unmute the click.
- `S`: mute or unmute backing chords.
- `V`: mute or unmute voice prompts where supported.
- `T`: toggle between the full phrase rows and the rolling current-plus-next-3 view.
- `N`: jump to the next chord.
- `B` / `Q`: return to the main menu.

### Scales

- Shows shapes for major pentatonic, minor pentatonic, major blues, and minor blues.
- Also includes major scale, natural minor, major-scale modes, harmonic minor, and melodic minor shapes.
- Supports song games where the current chord function is shown while you practise scale-based improvisation.
- Includes a song-library scale suggester that picks a best-fit key and scale for a selected song.
- In scale song modes, `R`, `3` / `b3`, and `5` mark the current chord tones; `1` marks the root of the suggested scale.

### Intervals

- Shows interval-function maps across fretboard windows.
- Lets you choose the root and anchor fret.
- Can show all intervals or a selected subset.
- Includes song-game practice for targeting intervals and landing on chord tones.

## Progressions And Songs

Progression entry accepts chords separated by spaces or commas:

```text
Am, C, G, D
```

Use `m` for minor chords. Flat roots are accepted and normalized internally, so `Bb` is understood.

The app also includes a built-in song library with numbered presets for quick practice. Song selection accepts the preset number, a copied menu line, or a song title.

## Audio

Music games can play:

- a click/metronome,
- a simple synthesized backing chord that follows the current progression,
- optional spoken chord prompts in supported modes.

Use `M`, `S`, and `V` in-game to mute or unmute those layers when available.

## Development

### Test

Run the full test harness with:

```bash
dotnet run --project Tests/GuitarResourcesTui.Tests/GuitarResourcesTui.Tests.csproj
```

### Source Layout

- `Tui/App.cs`: main menu flow and shared TUI helpers.
- `Tui/App.Audio.cs`: click, backing synth, tuner tone playback, and voice prompts.
- `Tui/App.CowboyChords.cs`: cowboy chord reference and song mode.
- `Tui/App.Tuner.cs`: tuner screens and note parsing.
- `Tui/App.Models.cs`: app-local records and enums.
- `Tui/Printing/`: printable HTML sheet generation.
- `Triads/`, `Pentatonics/`, `IntervalMaps/`, and `Fretboards/`: core music and rendering libraries.

### Build The Windows Installer

From PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File Installer\Build-WindowsInstaller.ps1
```

The installer is written to:

```text
artifacts/GuitarTUI-Setup-win-x64.exe
```

The build script publishes the app as a self-contained single-file Windows executable, embeds it into the installer payload, and generates the installer icon.
