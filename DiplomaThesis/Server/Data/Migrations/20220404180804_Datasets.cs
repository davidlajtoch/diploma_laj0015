using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomaThesis.Server.Data.Migrations
{
    public partial class Datasets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Datasets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PowerBiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColumnNames = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnTypes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datasets", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Datasets");
        }
    }
}
