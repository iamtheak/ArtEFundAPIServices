using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtEFundAPIServices.Data.DbMigrations
{
    /// <inheritdoc />
    public partial class Adddonationgoaltitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoalTitle",
                table: "GoalModel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoalTitle",
                table: "GoalModel");
        }
    }
}
