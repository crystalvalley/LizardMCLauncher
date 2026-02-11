using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSide.Data.Models;

/// <summary>
/// 버전 정보 모델
/// </summary>
[Table("ll_m_version_info")]
public class VersionInfo
{
    [Key]
    [Column("launcher_version")]
    public string LauncherVersion { get; set; } = string.Empty;

    [Column("minecraft_version")]
    public string MinecraftVersion { get; set; } = string.Empty;

    [Column("neoforge_version")]
    public string NeoforgeVersion { get; set; } = string.Empty;

    [Column("resource_version")]
    public string ResourceVersion { get; set; } = string.Empty;

    [Column("create_at")]
    public DateTime CreatedAt { get; set; }
}
