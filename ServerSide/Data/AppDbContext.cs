using Microsoft.EntityFrameworkCore;
using ServerSide.Data.Models;

namespace ServerSide.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<VersionInfo> VersionInfos { get; set; }
    public DbSet<Config> Configs { get; set; }
    public DbSet<Notice> Notices { get; set; }
}
