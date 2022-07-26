using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class CategoryAndTagUniqueIndicesFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username_Email_EmailVerificationCode_ResetPasswordCode",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Categories_OrderOnList_Keyword",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailVerificationCode",
                table: "Users",
                column: "EmailVerificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ResetPasswordCode",
                table: "Users",
                column: "ResetPasswordCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Keyword",
                table: "Categories",
                column: "Keyword",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_OrderOnList",
                table: "Categories",
                column: "OrderOnList",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmailVerificationCode",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ResetPasswordCode",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId_Name",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Keyword",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_OrderOnList",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username_Email_EmailVerificationCode_ResetPasswordCode",
                table: "Users",
                columns: new[] { "Username", "Email", "EmailVerificationCode", "ResetPasswordCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId",
                table: "Tags",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_OrderOnList_Keyword",
                table: "Categories",
                columns: new[] { "OrderOnList", "Keyword" },
                unique: true);
        }
    }
}
