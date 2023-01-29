using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaThesis.Server.Migrations
{
    public partial class ActivityChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserGroupName",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserGroupName",
                table: "Activities");
        }
    }
}
