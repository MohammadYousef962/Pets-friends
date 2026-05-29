using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pets_friends.Migrations
{
    /// <inheritdoc />
    public partial class AddPickUpDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickUpDate",
                table: "BoardingRecords",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickUpDate",
                table: "BoardingRecords");
        }
    }
}
