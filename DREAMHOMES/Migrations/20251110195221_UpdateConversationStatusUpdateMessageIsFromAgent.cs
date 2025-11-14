using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DREAMHOMES.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversationStatusUpdateMessageIsFromAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Conversations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromAgent",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsFromAgent",
                table: "ChatMessages");
        }
    }
}
