using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DREAMHOMES.Migrations
{
    /// <inheritdoc />
    public partial class AddedFewMoreProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SellerInformation",
                newName: "SoldPrice");

            migrationBuilder.AddColumn<double>(
                name: "AmountPerSqFt",
                table: "SellerInformation",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BathRooms",
                table: "SellerInformation",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BedRooms",
                table: "SellerInformation",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BuildingType",
                table: "SellerInformation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "HOA",
                table: "SellerInformation",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "HasFirePlace",
                table: "SellerInformation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasGarage",
                table: "SellerInformation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPool",
                table: "SellerInformation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ListingPrice",
                table: "SellerInformation",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LotArea",
                table: "SellerInformation",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFirePlace",
                table: "SellerInformation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfGarageSpace",
                table: "SellerInformation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SellerInformation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YearBuilt",
                table: "SellerInformation",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPerSqFt",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "BathRooms",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "BedRooms",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "BuildingType",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "HOA",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "HasFirePlace",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "HasGarage",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "HasPool",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "ListingPrice",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "LotArea",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "NumberOfFirePlace",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "NumberOfGarageSpace",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SellerInformation");

            migrationBuilder.DropColumn(
                name: "YearBuilt",
                table: "SellerInformation");

            migrationBuilder.RenameColumn(
                name: "SoldPrice",
                table: "SellerInformation",
                newName: "Price");
        }
    }
}
