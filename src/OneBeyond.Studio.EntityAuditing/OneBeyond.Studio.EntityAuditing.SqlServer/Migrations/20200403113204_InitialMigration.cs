using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneBeyond.Studio.EntityAuditing.SqlServer.Migrations;

public partial class InitialMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "audit");

        migrationBuilder.CreateTable(
            name: "AuditEvents",
            schema: "audit",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(nullable: true),
                EventType = table.Column<string>(nullable: true),
                EntityId = table.Column<string>(nullable: true),
                EntityName = table.Column<string>(nullable: true),
                EntityDescription = table.Column<string>(nullable: true),
                InsertedDate = table.Column<DateTimeOffset>(nullable: false),
                ChangedData = table.Column<string>(nullable: true),
                Context = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditEvents", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuditEvents",
            schema: "audit");
    }
}
