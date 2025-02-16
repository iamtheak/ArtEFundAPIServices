using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtEFundAPIServices.Data.DbMigrations
{
    /// <inheritdoc />
    public partial class Add_creator_content_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrolledMembership");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Creators",
                newName: "ContentTypeId");

            migrationBuilder.CreateTable(
                name: "EnrolledMembershipModel",
                columns: table => new
                {
                    MembershipId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrolledMembershipModel", x => new { x.UserId, x.MembershipId });
                    table.ForeignKey(
                        name: "FK_EnrolledMembershipModel_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "MembershipId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrolledMembershipModel_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Creators_ContentTypeId",
                table: "Creators",
                column: "ContentTypeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnrolledMembershipModel_MembershipId",
                table: "EnrolledMembershipModel",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_ContentTypes_ContentTypeId",
                table: "Creators",
                column: "ContentTypeId",
                principalTable: "ContentTypes",
                principalColumn: "ContentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creators_ContentTypes_ContentTypeId",
                table: "Creators");

            migrationBuilder.DropTable(
                name: "EnrolledMembershipModel");

            migrationBuilder.DropIndex(
                name: "IX_Creators_ContentTypeId",
                table: "Creators");

            migrationBuilder.RenameColumn(
                name: "ContentTypeId",
                table: "Creators",
                newName: "ContentType");

            migrationBuilder.CreateTable(
                name: "EnrolledMembership",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MembershipId = table.Column<int>(type: "int", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrolledMembership", x => new { x.UserId, x.MembershipId });
                    table.ForeignKey(
                        name: "FK_EnrolledMembership_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "MembershipId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrolledMembership_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnrolledMembership_MembershipId",
                table: "EnrolledMembership",
                column: "MembershipId");
        }
    }
}
