using Microsoft.EntityFrameworkCore.Migrations;

namespace MiraiNotes.Shared.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GoogleUserID = table.Column<string>(nullable: false),
                    Fullname = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    PictureUrl = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false),
                    UpdatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TaskLists",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GoogleTaskListID = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false),
                    UpdatedAt = table.Column<long>(nullable: false),
                    LocalStatus = table.Column<int>(nullable: false),
                    ToBeSynced = table.Column<bool>(nullable: false),
                    UserID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLists", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TaskLists_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GoogleTaskID = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false),
                    UpdatedAt = table.Column<long>(nullable: false),
                    ParentTask = table.Column<string>(nullable: true),
                    Position = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: false),
                    ToBeCompletedOn = table.Column<long>(nullable: true),
                    CompletedOn = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsHidden = table.Column<bool>(nullable: false),
                    LocalStatus = table.Column<int>(nullable: false),
                    ToBeSynced = table.Column<bool>(nullable: false),
                    RemindOn = table.Column<long>(nullable: true),
                    RemindOnGUID = table.Column<string>(nullable: true),
                    TaskListID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tasks_TaskLists_TaskListID",
                        column: x => x.TaskListID,
                        principalTable: "TaskLists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_GoogleTaskListID",
                table: "TaskLists",
                column: "GoogleTaskListID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_UserID",
                table: "TaskLists",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_GoogleTaskID",
                table: "Tasks",
                column: "GoogleTaskID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TaskListID",
                table: "Tasks",
                column: "TaskListID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleUserID",
                table: "Users",
                column: "GoogleUserID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "TaskLists");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
