using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmationTokenExpires",
                table: "Users",
                newName: "EmailConfirmationCodeExpires");

            migrationBuilder.AddColumn<int>(
                name: "EmailConfirmationCode",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationCode",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmationCodeExpires",
                table: "Users",
                newName: "EmailConfirmationTokenExpires");

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
