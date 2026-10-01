using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PlayViAPI.Migrations
{
    /// <inheritdoc />
    public partial class Subscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Titles",
                columns: new[] { "Id", "Description", "Name", "PosterUrl", "ReleaseYear", "Type", "VideoUrl" },
                values: new object[,]
                {
                    { 1, "Título de teste.", "Comédia de Teste (antiga)", null, 1985, "Movie", "https://exemplo.com/video1.mp4" },
                    { 2, "Título de teste.", "Comédia de Teste (nova)", null, 2023, "Movie", "https://exemplo.com/video2.mp4" },
                    { 3, "Título de teste.", "Drama de Teste", null, 2021, "Series", "https://exemplo.com/video3.mp4" }
                });

            migrationBuilder.InsertData(
                table: "TitleGenres",
                columns: new[] { "GenreId", "TitleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_Status",
                table: "Subscriptions",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DeleteData(
                table: "TitleGenres",
                keyColumns: new[] { "GenreId", "TitleId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "TitleGenres",
                keyColumns: new[] { "GenreId", "TitleId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "TitleGenres",
                keyColumns: new[] { "GenreId", "TitleId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
