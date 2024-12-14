using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Secuirty.Migrations
{
    /// <inheritdoc />
    public partial class inuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "35a870b2-1356-4471-aa72-e1dcdb3c735a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "df42d208-58e2-475d-a855-31372d9596be");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "68128976-36f7-4ab3-8b46-4154d343d580", "f69fb7cb-9691-4f39-b8b6-071f1d070dcf", "Admin", "ADMIN" },
                    { "bb1048ff-0b04-4cdc-b771-4521f34845f2", "4b5476e2-07ef-483b-84a6-bdec92b63ef3", "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "68128976-36f7-4ab3-8b46-4154d343d580");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bb1048ff-0b04-4cdc-b771-4521f34845f2");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "35a870b2-1356-4471-aa72-e1dcdb3c735a", "d706a6f0-5087-4bd3-b25e-4ca1dce4e7f8", "User", "USER" },
                    { "df42d208-58e2-475d-a855-31372d9596be", "fec59bf3-3635-41c2-bd1b-4c6de782d6ad", "Admin", "ADMIN" }
                });
        }
    }
}
