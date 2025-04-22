using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminEmailConfirmed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "EmailConfirmed",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "EmailConfirmed",
                value: false);
        }
    }
}
