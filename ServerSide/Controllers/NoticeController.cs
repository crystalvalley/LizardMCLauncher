using Microsoft.AspNetCore.Mvc;
using ServerSide.Data.Models;
using ServerSide.Services;

namespace ServerSide.Controllers;



/// <summary>
/// 공지사항 컨트롤러
/// </summary>
/// <param name="_noticeService">공지사항 서비스</param>
[Route("api/[controller]")]
[ApiController]
public class NoticeController(NoticeService _noticeService) : ControllerBase
{

    /// <summary>
    /// 공지사항 목록 조회조회
    /// 최신부터 검색 하며 start는 뒤에서부터 몇 번째인지, count는 몇 개를 가져올지 지정
    /// </summary>
    /// <param name="start">시작지점</param>
    /// <param name="count">갯수</param>
    /// <returns>검색된 공지사항 리스트</returns>
    [HttpGet("list")]
    public ActionResult<List<Notice>> GetNotices([FromQuery] int start = 0, [FromQuery] int count = 10)
    {
        var notices = _noticeService.GetNotices(start, count);
        return Ok(notices);
    }
}
