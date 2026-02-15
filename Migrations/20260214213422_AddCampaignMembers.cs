using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DontNeglectYourDungeon.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CampaignId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    JoinedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignMembers_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_JoinCode",
                table: "Campaigns",
                column: "JoinCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMembers_CampaignId",
                table: "CampaignMembers",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMembers_CampaignId_UserId",
                table: "CampaignMembers",
                columns: new[] { "CampaignId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignMembers_UserId",
                table: "CampaignMembers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignMembers");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_JoinCode",
                table: "Campaigns");
        }
    }
}
