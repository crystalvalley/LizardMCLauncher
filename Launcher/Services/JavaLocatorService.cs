using System.IO;

namespace Launcher.Services;

public class JavaLocatorService : IJavaLocatorService
{
    public string? FindJava21()
    {
        var minecraftRuntime = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @".minecraft/runtime/windows-x64/java-runtime-delta/bin/javaw.exe");

        var candidates = new[]
        {
            minecraftRuntime,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Java/jdk-21/bin/javaw.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Eclipse Adoptium/jdk-21/bin/javaw.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft/jdk-21/bin/javaw.exe"),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var javaw = Path.Combine(javaHome, @"bin/javaw.exe");
            if (File.Exists(javaw))
                return javaw;
        }

        return null;
    }
}
