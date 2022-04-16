using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class UserCreationDateOfCodesAndUniqueCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationCodeCreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordCodeCreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailVerificationCode_ResetPasswordCode",
                table: "Users",
                columns: new[] { "EmailVerificationCode", "ResetPasswordCode" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmailVerificationCode_ResetPasswordCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeCreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetPasswordCodeCreatedAt",
                table: "Users");
        }
    }
}
