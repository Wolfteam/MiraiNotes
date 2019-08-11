using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MiraiNotes.Data.Migrations
{
    public partial class Added_RemindOn_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RemindOn",
                table: "Tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemindOn",
                table: "Tasks");
        }
    }
}
