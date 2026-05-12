using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

const string AppName = "Guitar TUI";
const string Publisher = "Guitar TUI";
const string PayloadResourceName = "GuitarTuiPayload";

var argsSet = args.ToHashSet(StringComparer.OrdinalIgnoreCase);
if (argsSet.Contains("/uninstall") || argsSet.Contains("--uninstall"))
{
    Uninstall();
    return;
}

Install();

static void Install()
{
    var installDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Programs",
        AppName);
    Directory.CreateDirectory(installDirectory);

    var appPath = Path.Combine(installDirectory, $"{AppName}.exe");
    var uninstallerPath = Path.Combine(installDirectory, "Uninstall.exe");

    using (var payload = Assembly.GetExecutingAssembly().GetManifestResourceStream(PayloadResourceName)
        ?? throw new InvalidOperationException("Installer payload is missing."))
    using (var output = File.Create(appPath))
    {
        payload.CopyTo(output);
    }

    File.Copy(Environment.ProcessPath!, uninstallerPath, overwrite: true);

    CreateShortcut(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", $"{AppName}.lnk"),
        appPath,
        installDirectory,
        "Launch Guitar TUI");
    CreateShortcut(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{AppName}.lnk"),
        appPath,
        installDirectory,
        "Launch Guitar TUI");

    RegisterUninstaller(uninstallerPath, installDirectory);
    Process.Start(new ProcessStartInfo(appPath) { UseShellExecute = true });
}

static void Uninstall()
{
    var installDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Programs",
        AppName);

    TryDelete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", $"{AppName}.lnk"));
    TryDelete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{AppName}.lnk"));

    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", writable: true))
    {
        key?.DeleteSubKeyTree(AppName, throwOnMissingSubKey: false);
    }

    var deleteScript = Path.Combine(Path.GetTempPath(), "guitar-tui-uninstall.cmd");
    File.WriteAllText(deleteScript, $"""
@echo off
timeout /t 2 /nobreak > nul
rmdir /s /q "{installDirectory}"
del "%~f0"
""");

    Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{deleteScript}\"")
    {
        CreateNoWindow = true,
        UseShellExecute = false
    });
}

static void RegisterUninstaller(string uninstallerPath, string installDirectory)
{
    using var key = Registry.CurrentUser.CreateSubKey(@$"Software\Microsoft\Windows\CurrentVersion\Uninstall\{AppName}");
    key.SetValue("DisplayName", AppName);
    key.SetValue("DisplayIcon", Path.Combine(installDirectory, $"{AppName}.exe"));
    key.SetValue("Publisher", Publisher);
    key.SetValue("InstallLocation", installDirectory);
    key.SetValue("UninstallString", $"\"{uninstallerPath}\" /uninstall");
    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
}

static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory, string description)
{
    Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);

    var shellType = Type.GetTypeFromProgID("WScript.Shell")
        ?? throw new InvalidOperationException("Windows Script Host is unavailable.");
    dynamic shell = Activator.CreateInstance(shellType)!;
    dynamic shortcut = shell.CreateShortcut(shortcutPath);
    shortcut.TargetPath = targetPath;
    shortcut.WorkingDirectory = workingDirectory;
    shortcut.Description = description;
    shortcut.IconLocation = targetPath;
    shortcut.Save();

    Marshal.FinalReleaseComObject(shortcut);
    Marshal.FinalReleaseComObject(shell);
}

static void TryDelete(string path)
{
    try
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    catch
    {
        // Best effort cleanup.
    }
}
