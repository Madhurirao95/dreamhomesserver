using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DREAMHOMES.Migrations
{
    /// <inheritdoc />
    public partial class SellerInformationUserRelationshipUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SellerInformation",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SellerInformation_UserId",
                table: "SellerInformation",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SellerInformation_AspNetUsers_UserId",
                table: "SellerInformation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellerInformation_AspNetUsers_UserId",
                table: "SellerInformation");

            migrationBuilder.DropIndex(
                name: "IX_SellerInformation_UserId",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SellerInformation");
        }
    }
}
