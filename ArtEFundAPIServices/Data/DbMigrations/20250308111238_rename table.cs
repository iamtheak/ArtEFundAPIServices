using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtEFundAPIServices.Data.DbMigrations
{
    /// <inheritdoc />
    public partial class renametable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembershipModels_Memberships_MembershipId",
                table: "EnrolledMembershipModels");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembershipModels_Users_UserId",
                table: "EnrolledMembershipModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnrolledMembershipModels",
                table: "EnrolledMembershipModels");

            migrationBuilder.RenameTable(
                name: "EnrolledMembershipModels",
                newName: "EnrolledMembership");

            migrationBuilder.RenameIndex(
                name: "IX_EnrolledMembershipModels_UserId",
                table: "EnrolledMembership",
                newName: "IX_EnrolledMembership_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EnrolledMembershipModels_MembershipId",
                table: "EnrolledMembership",
                newName: "IX_EnrolledMembership_MembershipId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnrolledMembership",
                table: "EnrolledMembership",
                column: "EnrolledMembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrolledMembership_Memberships_MembershipId",
                table: "EnrolledMembership",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "MembershipId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EnrolledMembership_Users_UserId",
                table: "EnrolledMembership",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembership_Memberships_MembershipId",
                table: "EnrolledMembership");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembership_Users_UserId",
                table: "EnrolledMembership");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnrolledMembership",
                table: "EnrolledMembership");

            migrationBuilder.RenameTable(
                name: "EnrolledMembership",
                newName: "EnrolledMembershipModels");

            migrationBuilder.RenameIndex(
                name: "IX_EnrolledMembership_UserId",
                table: "EnrolledMembershipModels",
                newName: "IX_EnrolledMembershipModels_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EnrolledMembership_MembershipId",
                table: "EnrolledMembershipModels",
                newName: "IX_EnrolledMembershipModels_MembershipId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnrolledMembershipModels",
                table: "EnrolledMembershipModels",
                column: "EnrolledMembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrolledMembershipModels_Memberships_MembershipId",
                table: "EnrolledMembershipModels",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "MembershipId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EnrolledMembershipModels_Users_UserId",
                table: "EnrolledMembershipModels",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
