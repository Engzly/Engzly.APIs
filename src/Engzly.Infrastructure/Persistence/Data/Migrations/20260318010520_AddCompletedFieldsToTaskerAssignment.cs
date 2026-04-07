using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engzly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedFieldsToTaskerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedByTaskerOn",
                table: "GigAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompletedByTasker",
                table: "GigAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedByTaskerOn",
                table: "GigAssignments");

            migrationBuilder.DropColumn(
                name: "IsCompletedByTasker",
                table: "GigAssignments");
        }
    }
}
