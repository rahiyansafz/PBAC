using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { "$2a$11$ysX3ykS8fYQfDmPFKPYj4eQCsJJXBgT3UfQoDQtfF.1c1HnCyjXwm", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAEAACcQAAAAEBfFg6ePJilcYVvdvbYyYsYUKhj2h9oFSW49vjAYr4mIkyPRm2eQO5HzYcspJwzB0A==");
        }
    }
}
