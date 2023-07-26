using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChildrenDaycare.Migrations
{
    /// <inheritdoc />
    public partial class addnewCollInUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserAddress",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserAge",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UserDOB",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserFullname",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserAddress",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserAge",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserDOB",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserFullname",
                table: "AspNetUsers");
        }
    }
}
