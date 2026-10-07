using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlayViAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "Profiles",
                newName: "AvatarId");

            migrationBuilder.UpdateData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 1,
                column: "VideoUrl",
                value: "https://www.w3schools.com/html/mov_bbb.mp4");

            migrationBuilder.UpdateData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "ReleaseYear", "VideoUrl" },
                values: new object[] { "Vida Maria", 2017, "https://www.w3schools.com/html/mov_bbb.mp4" });

            migrationBuilder.UpdateData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VideoUrl" },
                values: new object[] { "UNO Vs Lamborghini", "https://www.w3schools.com/html/mov_bbb.mp4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvatarId",
                table: "Profiles",
                newName: "AvatarUrl");

            migrationBuilder.UpdateData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 1,
                column: "VideoUrl",
                value: "https://exemplo.com/video1.mp4");

            migrationBuilder.UpdateData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "ReleaseYear", "VideoUrl" },
                values: new object[] { "Comédia de Teste (nova)", 2023, "https://exemplo.com/video2.mp4" });

            migrationBuilder.UpdateData(
                table: "Titles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "VideoUrl" },
                values: new object[] { "Drama de Teste", "https://exemplo.com/video3.mp4" });
        }
    }
}
