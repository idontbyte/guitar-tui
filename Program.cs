using GuitarResourcesTui.IntervalMaps;
using GuitarResourcesTui.Pentatonics;
using GuitarResourcesTui.Triads;
using GuitarResourcesTui.Tui;

var app = new App(new TriadInversionLibrary(), new PentatonicLibrary(), new IntervalFunctionMapLibrary());
app.Run();
