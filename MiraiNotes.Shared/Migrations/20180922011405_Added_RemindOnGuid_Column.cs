using Microsoft.EntityFrameworkCore.Migrations;

namespace MiraiNotes.Data.Migrations
{
    public partial class Added_RemindOnGuid_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RemindOnGUID",
                table: "Tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemindOnGUID",
                table: "Tasks");
        }
    }
}
