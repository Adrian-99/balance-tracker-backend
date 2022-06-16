using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class CategoryOrderOnListKeywordIconAndIconColor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_NameTranslationKey",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "NameTranslationKey",
                table: "Categories",
                newName: "Keyword");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IconColor",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrderOnList",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_OrderOnList_Keyword",
                table: "Categories",
                columns: new[] { "OrderOnList", "Keyword" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_OrderOnList_Keyword",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IconColor",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OrderOnList",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "Keyword",
                table: "Categories",
                newName: "NameTranslationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NameTranslationKey",
                table: "Categories",
                column: "NameTranslationKey",
                unique: true);
        }
    }
}
