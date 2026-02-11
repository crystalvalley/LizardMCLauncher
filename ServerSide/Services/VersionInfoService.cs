using ServerSide.Data;
using ServerSide.Data.Models;

namespace ServerSide.Services;


/// <summary>
/// 버전 정보 서비스
/// </summary>
public class VersionInfoService(AppDbContext _dbContext)
{
    public VersionInfo GetLastVersionInfo()
    {
        var result = _dbContext.VersionInfos
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefault();
        return result ?? throw new Exception("버전 정보가 존재하지 않습니다.");
    }
}
