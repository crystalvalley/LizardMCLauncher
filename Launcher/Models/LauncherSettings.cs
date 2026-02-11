using System.IO;
using System.Text.Json;

namespace Launcher.Models;

public class LauncherSettings
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LizardMCLauncher");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    public int MaximumRamMb { get; set; } = 4096;
    public string GameDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".minecraft");
    public string? JavaPath { get; set; }
    public string JvmArguments { get; set; } = "-XX:+UseG1GC";
    public int ScreenWidth { get; set; } = 854;
    public int ScreenHeight { get; set; } = 480;
    public bool FullScreen { get; set; } = false;
    public bool AutoConnectEnabled { get; set; } = false;
    public string ServerAddress { get; set; } = "";
    public int? ServerPort { get; set; } = 25565;

    public static LauncherSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new LauncherSettings();

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<LauncherSettings>(json) ?? new LauncherSettings();
        }
        catch
        {
            return new LauncherSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}
