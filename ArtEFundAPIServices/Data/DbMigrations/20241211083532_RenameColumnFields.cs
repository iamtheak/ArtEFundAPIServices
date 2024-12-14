using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtEFundAPIServices.Data.DbMigrations
{
    /// <inheritdoc />
    public partial class RenameColumnFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefrehTokenId",
                table: "RefreshTokens",
                newName: "RefreshTokenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenId",
                table: "RefreshTokens",
                newName: "RefrehTokenId");
        }
    }
}
