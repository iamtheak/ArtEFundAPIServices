using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtEFundAPIServices.Data.DbMigrations
{
    /// <inheritdoc />
    public partial class Add_IsDeleted_Membership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Memberships",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Memberships");
        }
    }
}
