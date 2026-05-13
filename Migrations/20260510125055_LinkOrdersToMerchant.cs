using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pets_friends.Migrations
{
    /// <inheritdoc />
    public partial class LinkOrdersToMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "MerchantProfileId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_MerchantProfileId",
                table: "Orders",
                column: "MerchantProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_MerchantProfiles_MerchantProfileId",
                table: "Orders",
                column: "MerchantProfileId",
                principalTable: "MerchantProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_MerchantProfiles_MerchantProfileId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_MerchantProfileId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MerchantProfileId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
