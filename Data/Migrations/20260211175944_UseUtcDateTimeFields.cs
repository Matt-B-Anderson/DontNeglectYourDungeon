using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontNeglectYourDungeon.Migrations
{
    /// <inheritdoc />
    public partial class UseUtcDateTimeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Sessions",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Sessions",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Campaigns",
                newName: "UpdatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Campaigns",
                newName: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "Sessions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Sessions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAtUtc",
                table: "Campaigns",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Campaigns",
                newName: "CreatedAt");
        }
    }
}
