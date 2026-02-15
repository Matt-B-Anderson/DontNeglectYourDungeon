using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontNeglectYourDungeon.Migrations
{
    /// <inheritdoc />
    public partial class Dm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DndBeyondUrl",
                table: "Characters",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DndBeyondUrl",
                table: "Characters");
        }
    }
}
