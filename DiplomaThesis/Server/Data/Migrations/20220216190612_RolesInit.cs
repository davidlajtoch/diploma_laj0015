using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaThesis.Server.Data.Migrations
{
    public partial class RolesInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "0ec7c133-c8f9-4887-b7a8-05a32466a584", "e7189548-e780-49bb-9919-0a46280e014c", "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "3159c51b-0c11-4f57-8547-9bc235283ef4", "614a6174-b683-4122-a4a7-2bec6cc73143", "Architect", "ARCHITECT" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0ec7c133-c8f9-4887-b7a8-05a32466a584");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3159c51b-0c11-4f57-8547-9bc235283ef4");
        }
    }
}
