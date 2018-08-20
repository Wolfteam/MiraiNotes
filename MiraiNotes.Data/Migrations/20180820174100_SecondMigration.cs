using Microsoft.EntityFrameworkCore.Migrations;

namespace MiraiNotes.Data.Migrations
{
    public partial class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleUserID",
                table: "Users",
                column: "GoogleUserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_GoogleTaskID",
                table: "Tasks",
                column: "GoogleTaskID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_GoogleTaskListID",
                table: "TaskLists",
                column: "GoogleTaskListID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_GoogleUserID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_GoogleTaskID",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskLists_GoogleTaskListID",
                table: "TaskLists");
        }
    }
}
