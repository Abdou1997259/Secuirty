using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Secuirty.Migrations
{
    /// <inheritdoc />
    public partial class updateuserrefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "50007f84-68e2-4507-ad0d-bed4678d8fa5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ee8fb3f-a89f-4278-aaa1-8a4da8405328");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "35a870b2-1356-4471-aa72-e1dcdb3c735a", "d706a6f0-5087-4bd3-b25e-4ca1dce4e7f8", "User", "USER" },
                    { "df42d208-58e2-475d-a855-31372d9596be", "fec59bf3-3635-41c2-bd1b-4c6de782d6ad", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "35a870b2-1356-4471-aa72-e1dcdb3c735a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "df42d208-58e2-475d-a855-31372d9596be");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryDate",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "50007f84-68e2-4507-ad0d-bed4678d8fa5", "299b4ce0-0b41-432c-ae5a-50293a121412", "User", "USER" },
                    { "9ee8fb3f-a89f-4278-aaa1-8a4da8405328", "dd9fe806-a19e-4192-892f-190b697a2898", "Admin", "ADMIN" }
                });
        }
    }
}
