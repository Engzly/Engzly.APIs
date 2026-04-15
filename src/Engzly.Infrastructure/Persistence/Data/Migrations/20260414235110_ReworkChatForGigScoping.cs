using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engzly.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReworkChatForGigScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_UserAId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_UserBId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserAId_UserBId_GigId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserBId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserAId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserBId",
                table: "Conversations");

            migrationBuilder.AlterColumn<bool>(
                name: "IsBot",
                table: "Conversations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Conversations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditedOn",
                table: "ChatMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaveReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_GigId",
                table: "Conversations",
                column: "GigId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_OwnerId",
                table: "Conversations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_ConversationId_UserId",
                table: "ConversationParticipants",
                columns: new[] { "ConversationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_UserId",
                table: "ConversationParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_GigId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_OwnerId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "EditedOn",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<bool>(
                name: "IsBot",
                table: "Conversations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserAId",
                table: "Conversations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserBId",
                table: "Conversations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserAId_UserBId_GigId",
                table: "Conversations",
                columns: new[] { "UserAId", "UserBId", "GigId" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserBId",
                table: "Conversations",
                column: "UserBId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AspNetUsers_UserAId",
                table: "Conversations",
                column: "UserAId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AspNetUsers_UserBId",
                table: "Conversations",
                column: "UserBId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
