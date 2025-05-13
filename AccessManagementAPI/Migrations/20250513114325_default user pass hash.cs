﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class defaultuserpasshash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$vLWt0GGw4fmYbKjpXWyryOucFhPUOX/ylJzg6FFV0FdqAibOBVpOi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ysX3ykS8fYQfDmPFKPYj4eQCsJJXBgT3UfQoDQtfF.1c1HnCyjXwm");
        }
    }
}
