using Microsoft.AspNetCore.SignalR.Protocol;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerSide.Data.Models;


public static class ConfigKeys
{
    public const string CLOUD_STORAGE_URL = "CLOUD_STORAGE_URL";
    public const string CLOUD_STORAGE_ACCESS_KEY = "CLOUD_STORAGE_ACCESS_KEY";
    public const string LAUNCHER_RESOURCE_PATH = "LAUNCHER_RESOURCE_PATH";
    public const string MINECRAFT_MOD_PATH = "MINECRAFT_MOD_PATH";
    public const string NEOFORGE_RESOURCE_PATH = "NEOFORGE_RESOURCE_PATH";
}


/// <summary>
/// 설정값 모음
/// </summary>
[Table("ll_m_config")]
public class Config
{
    [Key]
    [Column("key")]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
