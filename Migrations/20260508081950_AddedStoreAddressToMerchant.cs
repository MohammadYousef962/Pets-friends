using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pets_friends.Migrations
{
    /// <inheritdoc />
    public partial class AddedStoreAddressToMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreAddress",
                table: "MerchantProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreAddress",
                table: "MerchantProfiles");
        }
    }
}
