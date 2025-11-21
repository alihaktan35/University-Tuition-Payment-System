using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIGateway.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RateLimits",
                columns: table => new
                {
                    RateLimitId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentNo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CallCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastCall = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateLimits", x => x.RateLimitId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RateLimit_StudentNo_Endpoint_Date",
                table: "RateLimits",
                columns: new[] { "StudentNo", "Endpoint", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateLimits");
        }
    }
}
