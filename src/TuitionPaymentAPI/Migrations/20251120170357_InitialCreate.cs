using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TuitionPaymentAPI.Migrations
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

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentNo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Tuitions",
                columns: table => new
                {
                    TuitionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Term = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tuitions", x => x.TuitionId);
                    table.ForeignKey(
                        name: "FK_Tuitions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TuitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TransactionReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Tuitions_TuitionId",
                        column: x => x.TuitionId,
                        principalTable: "Tuitions",
                        principalColumn: "TuitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "Email", "Name", "StudentNo" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(2690), "ahmet.yilmaz@university.edu", "Ahmet Yılmaz", "20210001" },
                    { 2, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(2780), "ayse.demir@university.edu", "Ayşe Demir", "20210002" },
                    { 3, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(2780), "mehmet.kaya@university.edu", "Mehmet Kaya", "20210003" },
                    { 4, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(2780), "fatma.oz@university.edu", "Fatma Öz", "20210004" },
                    { 5, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(2780), "ali.sahin@university.edu", "Ali Şahin", "20210005" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 20, 17, 3, 57, 269, DateTimeKind.Utc).AddTicks(5700), "$2a$11$Y1qZCLkc6XPN3kKbXaAu4ueM9EadGNjCr9jflQzVkCHID/jAGrTTK", "Admin", "admin" },
                    { 2, new DateTime(2025, 11, 20, 17, 3, 57, 371, DateTimeKind.Utc).AddTicks(5230), "$2a$11$TsvldFLTlImNfuwTNctioubOVaRsEhuywtHm6OHmNg8dGenHnkXhK", "BankingSystem", "bankapi" }
                });

            migrationBuilder.InsertData(
                table: "Tuitions",
                columns: new[] { "TuitionId", "Balance", "CreatedAt", "PaidAmount", "Status", "StudentId", "Term", "TotalAmount", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 50000m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5840), 0m, "UNPAID", 1, "2024-Fall", 50000m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5910) },
                    { 2, 25000m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5990), 25000m, "PARTIAL", 2, "2024-Fall", 50000m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5990) },
                    { 3, 0m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5990), 50000m, "PAID", 3, "2024-Fall", 50000m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5990) },
                    { 4, 50000m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5990), 0m, "UNPAID", 4, "2024-Fall", 50000m, new DateTime(2025, 11, 20, 17, 3, 57, 94, DateTimeKind.Utc).AddTicks(5990) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TuitionId",
                table: "Payments",
                column: "TuitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RateLimits_StudentNo_Endpoint_Date",
                table: "RateLimits",
                columns: new[] { "StudentNo", "Endpoint", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNo",
                table: "Students",
                column: "StudentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tuitions_StudentId_Term",
                table: "Tuitions",
                columns: new[] { "StudentId", "Term" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "RateLimits");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tuitions");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
