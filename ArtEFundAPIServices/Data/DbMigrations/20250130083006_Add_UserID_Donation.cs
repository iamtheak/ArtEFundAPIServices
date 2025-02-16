using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtEFundAPIServices.Data.DbMigrations
{
    /// <inheritdoc />
    public partial class Add_UserID_Donation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Donations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Donations");
        }
    }
}
