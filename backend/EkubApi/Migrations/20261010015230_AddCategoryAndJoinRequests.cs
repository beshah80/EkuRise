using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EkubApi.Migrations
{
    public partial class AddCategoryAndJoinRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Circles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Circles_CategoryId",
                table: "Circles",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Circles_EkubCategories_CategoryId",
                table: "Circles",
                column: "CategoryId",
                principalTable: "EkubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateTable(
                name: "CircleJoinRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CircleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AgreedToTerms = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CircleJoinRequests_Circles_CircleId",
                        column: x => x.CircleId,
                        principalTable: "Circles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CircleJoinRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CircleJoinRequests_CircleId",
                table: "CircleJoinRequests",
                column: "CircleId");

            migrationBuilder.CreateIndex(
                name: "IX_CircleJoinRequests_UserId",
                table: "CircleJoinRequests",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CircleJoinRequests");
            migrationBuilder.DropForeignKey(name: "FK_Circles_EkubCategories_CategoryId", table: "Circles");
            migrationBuilder.DropIndex(name: "IX_Circles_CategoryId", table: "Circles");
            migrationBuilder.DropColumn(name: "CategoryId", table: "Circles");
        }
    }
}
