using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EkubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionKycAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "EkubSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "EkubSubscriptions",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdFan",
                table: "EkubSubscriptions",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProofUrl",
                table: "EkubSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "EkubSubscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EkubSubscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "EkubSubscriptions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "EkubSubscriptions");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "EkubSubscriptions");

            migrationBuilder.DropColumn(
                name: "NationalIdFan",
                table: "EkubSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentProofUrl",
                table: "EkubSubscriptions");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "EkubSubscriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EkubSubscriptions");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "EkubSubscriptions");
        }
    }
}
