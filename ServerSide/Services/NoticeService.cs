using ServerSide.Data;
using ServerSide.Data.Models;

namespace ServerSide.Services;

/// <summary>
/// 공지사항 서비스
/// </summary>
public class NoticeService(AppDbContext _dbContext)
{
    /// <summary>
    /// 공지사항 목록 조회
    /// 최신부터 검색 하며 start는 뒤에서부터 몇 번째인지, count는 몇 개를 가져올지 지정
    /// </summary>
    /// <param name="start">시작지점</param>
    /// <param name="count">갯수</param>
    /// <returns></returns>
    public List<Notice> GetNotices(int start, int count)
    {
        var result= _dbContext.Notices
            .OrderByDescending(n => n.CreatedAt)
            .Skip(start)
            .Take(count)
            .ToList();
        return result;
    }
}
