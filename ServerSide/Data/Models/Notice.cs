using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSide.Data.Models;


/// <summary>
/// 공지사항 태그 상수값 모음
/// </summary>
public static class NoticeTags
{
    public const string UPDATE = "업데이트";
    public const string NOTICE = "공지사항";
}

/// <summary>
/// 공지사항 모델
/// </summary>
[Table("ll_m_notice")]
public class Notice
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("tag")]
    public string Tag { get; set; } = string.Empty;
    [Column("title")]
    public string Title { get; set; } = string.Empty;
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("create_at")]
    public DateTime CreatedAt { get; set; }
}
