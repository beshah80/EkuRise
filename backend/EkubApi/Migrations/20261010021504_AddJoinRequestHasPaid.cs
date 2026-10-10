using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EkubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddJoinRequestHasPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPaid",
                table: "CircleJoinRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPaid",
                table: "CircleJoinRequests");
        }
    }
}
