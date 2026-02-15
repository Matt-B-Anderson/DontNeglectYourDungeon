using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontNeglectYourDungeon.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorAndCampaignToSessionsAndLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Sessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "LinkedCharacters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CreatedByUserId",
                table: "Sessions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedCharacters_CampaignId",
                table: "LinkedCharacters",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkedCharacters_Campaigns_CampaignId",
                table: "LinkedCharacters",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkedCharacters_Campaigns_CampaignId",
                table: "LinkedCharacters");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_CreatedByUserId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_LinkedCharacters_CampaignId",
                table: "LinkedCharacters");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "LinkedCharacters");
        }
    }
}
