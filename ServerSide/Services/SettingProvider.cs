using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ServerSide.Data;
using System.Collections.Concurrent;

namespace ServerSide.Services;

/// <summary>
/// 설정값 제공자
/// 어플리케이션 시작시 DB에서 설정값을 불러와 메모리에 저장하고 제공하는 역할
/// </summary>
/// <param name="_scopeFactory">스코프 팩토리</param>
public class SettingProvider(IServiceScopeFactory _scopeFactory) : IDisposable
{
    private readonly ConcurrentDictionary<string, string> _settings = new();
    private bool _initialized = false;
    private Timer? _refreshTimer;

    /// <summary>
    /// 설정값 초기 로딩 및 주기적 갱신 시작
    /// </summary>
    public async Task LoadAsync()
    {
        await RefreshAsync();
        _initialized = true;

        // 5분마다 DB에서 설정값 갱신
        _refreshTimer = new Timer(async _ => await RefreshAsync(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    private async Task RefreshAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var configs = await dbContext.Configs.AsNoTracking().ToListAsync();
        _settings.Clear();
        foreach (var item in configs)
        {
            _settings[item.Key] = item.Value;
        }
    }

    public string Get(string key)
    {
        if (!_initialized) LoadAsync().GetAwaiter().GetResult();
        _settings.TryGetValue(key, out var value);
        return value ?? throw new Exception("설정값이 존재하지 않습니다.");
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}
