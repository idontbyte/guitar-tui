# guitar-tui
A TUI for helping to learn guitar

## Run

```bash
dotnet run
```

## Test

```bash
dotnet run --project Tests/GuitarResourcesTui.Tests/GuitarResourcesTui.Tests.csproj
```

## Windows installer

```powershell
powershell -ExecutionPolicy Bypass -File Installer\Build-WindowsInstaller.ps1
```

The installer is written to `artifacts/GuitarTUI-Setup-win-x64.exe` and installs the app as `Guitar TUI`.

## Features

- Triad inversions across adjacent string groupings
- Major and minor pentatonic scale boxes
- Major and minor blues scale boxes
- Major scale, natural minor, major-scale modes, harmonic minor, and melodic minor shapes
- Interval function maps for learning note relationships across fretboard windows
- Colored interval labels and reusable fretboard rendering
