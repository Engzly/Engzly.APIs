using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engzly.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIdentityVerified",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "IdentityVerifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NationalIdFrontPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    NationalIdBackPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SelfiePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityVerifications_AspNetUsers_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IdentityVerifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerificationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentKind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationAccessLogs_IdentityVerifications_VerificationId",
                        column: x => x.VerificationId,
                        principalTable: "IdentityVerifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerifications_ReviewedByAdminId",
                table: "IdentityVerifications",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerifications_Status",
                table: "IdentityVerifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerifications_UserId",
                table: "IdentityVerifications",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerifications_UserId_Status",
                table: "IdentityVerifications",
                columns: new[] { "UserId", "Status" },
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationAccessLogs_AccessedOn",
                table: "VerificationAccessLogs",
                column: "AccessedOn");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationAccessLogs_VerificationId",
                table: "VerificationAccessLogs",
                column: "VerificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerificationAccessLogs");

            migrationBuilder.DropTable(
                name: "IdentityVerifications");

            migrationBuilder.DropColumn(
                name: "IsIdentityVerified",
                table: "AspNetUsers");
        }
    }
}
