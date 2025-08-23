using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskLabels_Tasks_TasksId",
                table: "TaskLabels");

            migrationBuilder.RenameColumn(
                name: "TasksId",
                table: "TaskLabels",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskLabels_TasksId",
                table: "TaskLabels",
                newName: "IX_TaskLabels_TaskId");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "TaskLists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLabels_Tasks_TaskId",
                table: "TaskLabels",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskLabels_Tasks_TaskId",
                table: "TaskLabels");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "TaskLists");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskLabels",
                newName: "TasksId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskLabels_TaskId",
                table: "TaskLabels",
                newName: "IX_TaskLabels_TasksId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLabels_Tasks_TasksId",
                table: "TaskLabels",
                column: "TasksId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
