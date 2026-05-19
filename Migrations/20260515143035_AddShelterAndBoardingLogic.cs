using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pets_friends.Migrations
{
    /// <inheritdoc />
    public partial class AddShelterAndBoardingLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePicture",
                table: "Pets",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdopted",
                table: "Pets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ShelterProfileId",
                table: "Pets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdoptionApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    ClientProfileId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdoptionApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdoptionApplications_ClientProfiles_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdoptionApplications_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BoardingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShelterProfileId = table.Column<int>(type: "int", nullable: false),
                    PetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PetBreed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecialNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardingRecords_ShelterProfiles_ShelterProfileId",
                        column: x => x.ShelterProfileId,
                        principalTable: "ShelterProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pets_ShelterProfileId",
                table: "Pets",
                column: "ShelterProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplications_ClientProfileId",
                table: "AdoptionApplications",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplications_PetId",
                table: "AdoptionApplications",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardingRecords_ShelterProfileId",
                table: "BoardingRecords",
                column: "ShelterProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_ShelterProfiles_ShelterProfileId",
                table: "Pets",
                column: "ShelterProfileId",
                principalTable: "ShelterProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_ShelterProfiles_ShelterProfileId",
                table: "Pets");

            migrationBuilder.DropTable(
                name: "AdoptionApplications");

            migrationBuilder.DropTable(
                name: "BoardingRecords");

            migrationBuilder.DropIndex(
                name: "IX_Pets_ShelterProfileId",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsAdopted",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "ShelterProfileId",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Pets",
                newName: "ProfilePicture");
        }
    }
}
