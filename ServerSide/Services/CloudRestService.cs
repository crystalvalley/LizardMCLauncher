using RestSharp;
using ServerSide.Data;
using ServerSide.Data.Models;
using System.Threading.Tasks;

namespace ServerSide.Services;


/// <summary>
/// 클라우드 REST 서비스
/// </summary>
public class CloudRestService(SettingProvider _settingProvider)
{
    private readonly RestClient _client = new(_settingProvider.Get(ConfigKeys.CLOUD_STORAGE_URL));
    private readonly string _key = _settingProvider.Get(ConfigKeys.CLOUD_STORAGE_ACCESS_KEY);  

    // 런처 다운로드 URL
    public async Task<string> GetLauncherDownloadUrl()
    {
        string path = _settingProvider.Get(ConfigKeys.LAUNCHER_RESOURCE_PATH);
        var request = new RestRequest($"api/v2.1/via-repo-token/download-link/?path={path}");
        request.AddHeader("Authorization", $"Token {_key}");

        var response = await _client.ExecuteAsync<string>(request);
        return response.Data ?? string.Empty;
    }

    // 네오포지 리소스 다운로드 URL
    public async Task<string> GetNeoforgeDownloadUrl(string neoforgeVersion)
    {
        string path = _settingProvider.Get(ConfigKeys.NEOFORGE_RESOURCE_PATH).Replace("{version}", neoforgeVersion);
        var request = new RestRequest($"api/v2.1/via-repo-token/download-link/?path={path}");
        request.AddHeader("Authorization", $"Token {_key}");

        var response = await _client.ExecuteAsync<string>(request);
        return response.Data ?? string.Empty;
    }
}
