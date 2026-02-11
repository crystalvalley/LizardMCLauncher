using Launcher.Models;
using RestSharp;
using System.IO;
using System.Net.Http;

namespace Launcher.Services;

public class RestService(ApiSettings _apiSettings)
{
    private readonly RestClient _client = new(_apiSettings.ServerSideUrl);

    public async Task<string> GetNeoforgeDownloadUrl(string version)
    {
        // ServerSide API에서 리소스 다운로드 URL 조회
        var request = new RestRequest($"api/resource/neoforge?neoforgeVersion={version}", Method.Get);
        var info = await _client.ExecuteAsync<string>(request);
        return info.Data ?? string.Empty;
    }

    public async Task DownloadFile(string url, string outputPath)
    {
        using var httpClient = new HttpClient();
        var bytes = await httpClient.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(outputPath, bytes);
    }

    public async Task<List<Notice>> GetNoticeAsync(int start = 0, int count = 10)
    {
        var request = new RestRequest($"api/notice/list?start={start}&count={count}", Method.Get);
        var response = await _client.ExecuteAsync<List<Notice>>(request);
        return response.Data ?? [];
    }

    public async Task<VersionInfo> GetVersionInfoAsync()
    {
        var request = new RestRequest("api/versioninfo", Method.Get);
        var response = await _client.ExecuteAsync<VersionInfo>(request);
        return response.Data ?? new VersionInfo("", "", "", "");
    }

    public async Task<string> GetLauncherDownloadUrlAsync()
    {
        var request = new RestRequest("api/resource/launcher", Method.Get);
        var response = await _client.ExecuteAsync<string>(request);
        return response.Data ?? string.Empty;
    }

    public record VersionInfo(string LauncherVersion, string MinecraftVersion, string NeoforgeVersion, string ResourceVersion);
    public record Notice(string Tag, string Title, string Content, DateTime CreatedAt);
}
