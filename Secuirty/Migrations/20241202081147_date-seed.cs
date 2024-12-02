using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Secuirty.Migrations
{
    /// <inheritdoc />
    public partial class dateseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "50007f84-68e2-4507-ad0d-bed4678d8fa5", "299b4ce0-0b41-432c-ae5a-50293a121412", "User", "USER" },
                    { "9ee8fb3f-a89f-4278-aaa1-8a4da8405328", "dd9fe806-a19e-4192-892f-190b697a2898", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "50007f84-68e2-4507-ad0d-bed4678d8fa5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ee8fb3f-a89f-4278-aaa1-8a4da8405328");
        }
    }
}
