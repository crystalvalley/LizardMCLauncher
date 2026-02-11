using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using Launcher.Models;
using System.IO;
using System.IO.Compression;

namespace Launcher.Services;

public interface IJavaLocatorService
{
    string? FindJava21();
}

public class GameLaunchService(RestService _restService, IJavaLocatorService _javaLocator)
{
    public event EventHandler<(int ProgressedTasks, int TotalTasks)>? FileProgressChanged;

    public async Task CheckMinecraftInstalled(string minecraftVersion,string neoforgeVersion,string resourceVersion)
    {
        var path = new MinecraftPath();
        var launcher = new MinecraftLauncher(path);

        launcher.FileProgressChanged += (_, args) =>
        {
            FileProgressChanged?.Invoke(this, (args.ProgressedTasks, args.TotalTasks));
        };

        // 마인크래프트 버전 설치, 이미 설치되어 있다면 파일 체크만 들어감
        await launcher.InstallAsync(minecraftVersion);
    }

    public bool CheckNeoforgeInstalled(string neoforgeVersion)
    {
        var path = new MinecraftPath();
        var versionDir = $@"{path.BasePath}/versions/{neoforgeVersion}";
        return Directory.Exists(versionDir);
    }

    public async Task InstallNeoforgeAsync(string neoforgeVersion)
    {
        var path = new MinecraftPath();
        var launcher = new MinecraftLauncher(path);

        // 기존 버전 폴더 삭제
        var versionDir = $@"{path.BasePath}/versions/{neoforgeVersion}";
        if (Directory.Exists(versionDir)) Directory.Delete(versionDir, true);

        // 다운로드용 temp 폴더 생성
        var tempPath = Path.Combine(path.BasePath, "temp");
        if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        Directory.CreateDirectory(tempPath);

        // 포지 다운로드
        var url = await _restService.GetNeoforgeDownloadUrl(neoforgeVersion);
        await _restService.DownloadFile(url, Path.Combine(tempPath, $"{neoforgeVersion}.zip"));

        // 압축해제할 디렉토리 정리
        var extractPath = Path.Combine(path.BasePath, $@"temp/{neoforgeVersion}");
        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
        Directory.CreateDirectory(extractPath);

        ZipFile.ExtractToDirectory($@"{tempPath}/{neoforgeVersion}.zip", extractPath);

        MergeDirectory($@"{extractPath}/versions/{neoforgeVersion}", $@"{path.BasePath}/versions/{neoforgeVersion}");
        MergeDirectory($@"{extractPath}/libraries", $@"{path.BasePath}/libraries");

        // temp 폴더 삭제
        Directory.Delete(tempPath, true);
    }

    public async Task LaunchAsync(string version, MSession session, LauncherSettings settings)
    {
        var path = new MinecraftPath();
        var launcher = new MinecraftLauncher(path);

        var launchOption = new MLaunchOption
        {
            Session = session,
            MaximumRamMb = settings.MaximumRamMb,
            JavaPath = settings.JavaPath ?? _javaLocator.FindJava21() ?? "javaw",
            ScreenWidth = settings.ScreenWidth,
            ScreenHeight = settings.ScreenHeight,
            FullScreen = settings.FullScreen,
            ServerIp = settings.AutoConnectEnabled ? settings.ServerAddress : null,
            ServerPort = settings.AutoConnectEnabled && settings.ServerPort.HasValue ? settings.ServerPort.Value : 0,
        };

        var process = await launcher.BuildProcessAsync(version, launchOption);
        var processWrapper = new ProcessWrapper(process);
        processWrapper.StartWithEvents();
        await processWrapper.WaitForExitTaskAsync();
    }

    private void MergeDirectory(string source, string target, bool overwrite = true)
    {
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
        if (!Directory.Exists(target)) Directory.CreateDirectory(target);

        foreach(var file in Directory.GetFiles(source))
        {
            var filename = Path.GetFileName(file);
            var destFile = Path.Combine(target, filename);

            File.Move(file, destFile, overwrite);
        }

        // 하위 디렉토리 재귀 병합
        foreach(var dir in Directory.GetDirectories(source))
        {
            var dirName = Path.GetFileName(dir);
            var destSubDir = Path.Combine(target, dirName);

            MergeDirectory(dir, destSubDir, overwrite);
        }
    }
}

