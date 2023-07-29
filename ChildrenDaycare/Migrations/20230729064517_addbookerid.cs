using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChildrenDaycare.Migrations
{
    /// <inheritdoc />
    public partial class addbookerid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SlotTable",
                columns: table => new
                {
                    SlotID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TakecareGiverID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isBooked = table.Column<bool>(type: "bit", nullable: false),
                    ChildFullname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildAge = table.Column<int>(type: "int", nullable: true),
                    ChildDOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SlotPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookerID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotTable", x => x.SlotID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlotTable");
        }
    }
}
