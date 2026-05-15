# Guitar TUI

Guitar TUI is a terminal-based guitar practice tool for learning fretboard shapes, chord functions, song progressions, and timing. It combines reference diagrams with interactive music games, click/backing playback, printable sheets, and optional spoken chord prompts.

## Install

### Windows

Download the Windows `.exe` release and run it.

The Windows app is packaged as a self-contained executable, so you do not need to install the .NET SDK just to use the app.

### macOS

Download the macOS `.dmg`, open it, then drag `Guitar TUI` into `Applications`.

On first launch, macOS may ask you to confirm that you want to open an app downloaded from the internet. The macOS app launches the TUI in Terminal.

## Run From Source

Install the .NET SDK that supports the project target framework, then run:

```bash
dotnet run
```

The project currently targets `net10.0`.

## Main Menu

Guitar TUI opens with these areas:

- `Tuner`: play reference notes for the guitar strings or a custom note.
- `Cowboy chords`: learn open-position chords and practise changing between them in songs.
- `Triads`: study 3-note chord shapes and inversions across the fretboard.
- `Jazz chords`: learn guide tones, shell voicings, ii-V-I movement, and jazz standards.
- `Arpeggios`: learn chord tones one note at a time for soloing, rhythm, and song changes.
- `Scales`: explore scale shapes and practise using them over chord progressions.
- `Intervals`: see how notes relate to a root and practise targeting chord tones.

Most screens support `B` for back and `Q` for the main menu. Fretboard screens support `L` where shown to cycle note labels between interval names, fret numbers, and simple `X` markers.

The shared fretboard renderer shows six strings, keeps the nut / fret 0 column visually distinct, and colors roots, thirds, fifths, color tones, blue notes, and muted strings.

## Tuner

- Plays standard guitar-string reference tones.
- Supports custom note input such as `E2`, `C#3`, or `Bb3`.
- Shows the frequency of the note being played.
- Includes a stop command for the currently playing reference tone.

## Cowboy Chords

- `Chord reference`: shows common open-position chord diagrams with root, third, fifth, open-string, and muted-string labels.
- `Song mode`: practises open-chord changes in real progressions with tempo, click, and backing chords.

## Triads

- `Inversions`: reference all major and minor triad inversions by adjacent string group.
- `Music game`: practise connected close-voiced triad shapes through chord progressions.
- `Spread triads music game`: practise wider skipped-string triad shapes.
- Includes sharp and flat root choices through the chromatic root menu.
- Marks common lower-position triad shapes.
- Can open printable triad inversion sheets from the triad reference screen.

Useful triad-game controls:

- `Space`: pause or resume.
- `-` / `+`: decrease or increase tempo.
- `I`: cycle the triad inversion filter.
- `M`: mute or unmute the click.
- `S`: mute or unmute backing chords.
- `V`: mute or unmute voice prompts where supported.
- `T`: toggle between full phrase rows and the rolling current-plus-next-3 view.
- `L`: cycle note-label mode.
- `N`: jump to the next chord.

## Jazz Chords

- `Fundamentals path`: a guided path through seventh chord formulas, guide tones, shell voicings, major ii-V-I, minor ii-V-i, and standards practice.
- `Chord type list`: explains common jazz colors including `maj7`, `6`, `6/9`, `m7`, `m9`, `7`, `9`, `13`, `7b9`, `7#9`, `m7b5`, and `dim7`.
- `Voicing reference`: shows playable guide-tone, shell, and full voicing shapes.
- `ii-V-I trainer`: drills major or minor ii-V-I progressions in random or chosen keys.
- `Song library`: applies jazz chord vocabulary to standards-style progressions with concept tags.
- `Random arrangement game`: creates a fresh connected jazz progression to practise.

Jazz games show the current chord, the next movement, guide-tone summaries, and upcoming voicings.

Useful jazz controls:

- `Space`: pause or resume.
- `-` / `+`: decrease or increase tempo.
- `V`: cycle guide tones, shell voicings, and full voicings.
- `M`: mute or unmute the click.
- `S`: mute or unmute backing chords.
- `L`: cycle note-label mode.
- `N`: jump to the next chord.

## Arpeggios

- `What are arpeggios?`: explains chord tones, soloing targets, and broken-chord rhythm use.
- `Shape reference`: shows movable arpeggio maps for triads and 7th chords.
- `Find the chord tone`: plays a chord and asks you to find a target root, 3rd, 5th, or 7th.
- `Song changes arpeggio game`: follows the current chord and switches arpeggio shapes as the harmony moves.
- `Rhythm arpeggio patterns`: gives broken-chord picking patterns for rhythm parts.
- `Printable sheets`: opens printable arpeggio maps for practice away from the app.

Supported arpeggio qualities include major, minor, major 7, dominant 7, minor 7, minor 7 flat 5, and diminished 7.

Useful arpeggio controls:

- `Space`: pause or resume in song mode.
- `-` / `+`: decrease or increase tempo in song mode.
- `T`: toggle full/target-tone view in the song game, or play the target note in the targeting game.
- `C`: replay the current chord in the targeting game.
- `R`: reveal or hide the target tone in the targeting game.
- `M`: mute or unmute the click.
- `S`: mute or unmute backing chords.
- `L`: cycle note-label mode.
- `N`: jump to the next chord or prompt.

## Scales

- `Shapes`: shows movable fretboard positions for a chosen root and scale.
- `Song game`: practises a chosen scale while chords move underneath.
- `Song library scale suggester`: picks a best-fit key and scale for a selected song and shows alternate suggestions.
- Scale song modes mark the current chord tones as `R`, `3` / `b3`, and `5`; `1` marks the root of the selected or suggested scale.

Scale references include major pentatonic, minor pentatonic, major blues, minor blues, major scale, natural minor, major-scale modes, harmonic minor, and melodic minor shapes.

Useful scale controls:

- `Space`: pause or resume in song modes.
- `-` / `+`: decrease or increase tempo in song modes.
- `T`: toggle major/minor on compatible shape screens.
- `M`: mute or unmute the click.
- `S`: mute or unmute backing chords.
- `L`: cycle note-label mode.
- `N`: jump to the next chord.

## Intervals

- `Lookup`: pick a root and see interval names across a fretboard window.
- `Song game`: practise finding target intervals while chords change.
- The interval lookup can show all intervals or a selected subset.
- The anchor fret can be moved with `N` / `P` or the arrow keys.
- Interval games display the target interval, its function name, the current chord, and the backing chord.

## Progressions And Songs

Progression entry accepts chords separated by spaces or commas:

```text
Am, C, G, D
```

For triad-oriented modes, use `m` for minor chords. Flat roots are accepted and normalized internally, so `Bb` is understood.

Jazz and arpeggio modes also accept 7th-chord-style symbols such as:

```text
Dm7 G7 Cmaj7
```

Many games let you choose:

- `Simple mode`: the app creates a short progression.
- `Custom mode`: type your own progression.
- `Song select`: choose from the built-in song-style progression library.

Song selection accepts the preset number, a copied menu line, or a song title. Chord-length prompts accept one length per chord, or a blank value to use the default length for every chord.

## Audio

Music games can play:

- a click/metronome,
- a simple synthesized backing chord that follows the current progression,
- optional spoken chord prompts in supported modes.

Use `M`, `S`, and `V` in-game to mute or unmute those layers when available. The tuner, arpeggio targeting game, and music games share the same local audio helpers.

## Printable Sheets

Printable sheets are generated as local HTML files and opened in the browser when the platform allows it.

- Triad reference screens can print inversion sheets.
- Arpeggio screens can print arpeggio maps for a chosen root and chord type.

Use the browser print dialog to print or save the sheets as PDF.

## Development

### Test

Run the full test harness with:

```bash
dotnet run --project Tests/GuitarResourcesTui.Tests/GuitarResourcesTui.Tests.csproj
```

### Source Layout

- `Program.cs`: app entry point.
- `Tui/App.cs`: main menu, triads, scales, intervals, progression setup, and shared TUI helpers.
- `Tui/App.Audio.cs`: click, backing synth, tuner tone playback, and voice prompts.
- `Tui/App.CowboyChords.cs`: cowboy chord reference and song mode.
- `Tui/App.Tuner.cs`: tuner screens and note parsing.
- `Tui/App.JazzChords.cs`: jazz chord menus, voicing reference, ii-V-I trainer, standards library, and jazz games.
- `Tui/App.Arpeggios.cs`: arpeggio lessons, shape references, targeting game, song game, rhythm patterns, and printable sheets.
- `Tui/App.Models.cs`: app-local records and enums.
- `Tui/Printing/`: printable HTML sheet generation.
- `Fretboards/`: fretboard diagrams, rendering, and note-label modes.
- `Triads/`: music-theory helpers, triad inversions, and progression game phrase generation.
- `Pentatonics/`: scale and mode shape libraries.
- `IntervalMaps/`: interval-function fretboard maps.
- `JazzChords/`: jazz chord qualities, voicings, progression presets, and teaching helpers.
- `Arpeggios/`: arpeggio qualities, shapes, targeting prompts, and teaching helpers.
- `Tests/GuitarResourcesTui.Tests/`: lightweight console test harness.

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

### Build The macOS App

From macOS:

```bash
./packaging/macos/package-macos.sh
```

The script defaults to `osx-arm64`. Pass another runtime if needed:

```bash
./packaging/macos/package-macos.sh osx-x64
```

The generated `.app` and `.dmg` are written under `artifacts/macos/`.
