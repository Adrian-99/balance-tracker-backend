using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class EntryDescriptionImprovedEncryption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Entries",
                newName: "DescriptionContent");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionKey",
                table: "Entries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionIV",
                table: "Entries",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionKey",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "DescriptionIV",
                table: "Entries");

            migrationBuilder.RenameColumn(
                name: "DescriptionContent",
                table: "Entries",
                newName: "Description");
        }
    }
}
