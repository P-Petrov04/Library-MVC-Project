using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Data.Migrations
{
    public partial class AddAdminUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "Name", "Password", "RoleId" },
                values: new object[] { 1, "stu2301321002@uni-plovdiv.bg", "pPetrov04", "2301321002", 1 });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "Name", "Password", "RoleId" },
                values: new object[] { 2, "stu2301321069@uni-plovdiv.bg", "bMinkov05", "2301321069", 1 });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "Name", "Password", "RoleId" },
                values: new object[] { 3, "stu2301321059@uni-plovdiv.bg", "aKuznecov", "2301321059", 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
