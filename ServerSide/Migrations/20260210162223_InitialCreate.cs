using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ll_m_config",
                columns: table => new
                {
                    key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ll_m_config", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "ll_m_notice",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tag = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ll_m_notice", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ll_m_version_info",
                columns: table => new
                {
                    launcher_version = table.Column<string>(type: "text", nullable: false),
                    minecraft_version = table.Column<string>(type: "text", nullable: false),
                    neoforge_version = table.Column<string>(type: "text", nullable: false),
                    resource_version = table.Column<string>(type: "text", nullable: false),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ll_m_version_info", x => x.launcher_version);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ll_m_config");

            migrationBuilder.DropTable(
                name: "ll_m_notice");

            migrationBuilder.DropTable(
                name: "ll_m_version_info");
        }
    }
}
