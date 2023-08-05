using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChildrenDaycare.Migrations
{
    /// <inheritdoc />
    public partial class addconfirmationstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isConfirmed",
                table: "AspNetUsers");
        }
    }
}
