using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engzly.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOtpFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OtpAttempts",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OtpChannel",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAtUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtpHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpLastSentAtUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OtpPurpose",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpChannel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpLastSentAtUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OtpPurpose",
                table: "AspNetUsers");
        }
    }
}
