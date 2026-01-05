using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DREAMHOMES.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewPropertyLotUnitArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LotAreaUnit",
                table: "SellerInformation",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LotAreaUnit",
                table: "SellerInformation");
        }
    }
}
