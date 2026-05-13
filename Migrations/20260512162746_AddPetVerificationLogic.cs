using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pets_friends.Migrations
{
    /// <inheritdoc />
    public partial class AddPetVerificationLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Pets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByVetId",
                table: "Pets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_VerifiedByVetId",
                table: "Pets",
                column: "VerifiedByVetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_VetProfiles_VerifiedByVetId",
                table: "Pets",
                column: "VerifiedByVetId",
                principalTable: "VetProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_VetProfiles_VerifiedByVetId",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_Pets_VerifiedByVetId",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "VerifiedByVetId",
                table: "Pets");
        }
    }
}
