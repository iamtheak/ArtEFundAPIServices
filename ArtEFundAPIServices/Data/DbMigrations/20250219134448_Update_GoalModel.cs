using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtEFundAPIServices.Data.DbMigrations
{
    /// <inheritdoc />
    public partial class Update_GoalModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembershipModel_Memberships_MembershipId",
                table: "EnrolledMembershipModel");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembershipModel_Users_UserId",
                table: "EnrolledMembershipModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Goals",
                table: "Goals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnrolledMembershipModel",
                table: "EnrolledMembershipModel");

            migrationBuilder.RenameTable(
                name: "Goals",
                newName: "GoalModel");

            migrationBuilder.RenameTable(
                name: "EnrolledMembershipModel",
                newName: "EnrolledMembershipModels");

            migrationBuilder.RenameColumn(
                name: "GoalReached",
                table: "GoalModel",
                newName: "IsGoalReached");

            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "EnrolledMembershipModels",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_EnrolledMembershipModel_MembershipId",
                table: "EnrolledMembershipModels",
                newName: "IX_EnrolledMembershipModels_MembershipId");

            migrationBuilder.AlterColumn<decimal>(
                name: "GoalAmount",
                table: "GoalModel",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<bool>(
                name: "IsGoalActve",
                table: "GoalModel",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EnrolledMembershipId",
                table: "EnrolledMembershipModels",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "EnrolledMembershipModels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoalModel",
                table: "GoalModel",
                column: "GoalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnrolledMembershipModels",
                table: "EnrolledMembershipModels",
                column: "EnrolledMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalModel_CreatorId",
                table: "GoalModel",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrolledMembershipModels_UserId",
                table: "EnrolledMembershipModels",
                column: "UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_GoalModel_Creators_CreatorId",
                table: "GoalModel",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "CreatorId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembershipModels_Memberships_MembershipId",
                table: "EnrolledMembershipModels");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrolledMembershipModels_Users_UserId",
                table: "EnrolledMembershipModels");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalModel_Creators_CreatorId",
                table: "GoalModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoalModel",
                table: "GoalModel");

            migrationBuilder.DropIndex(
                name: "IX_GoalModel_CreatorId",
                table: "GoalModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnrolledMembershipModels",
                table: "EnrolledMembershipModels");

            migrationBuilder.DropIndex(
                name: "IX_EnrolledMembershipModels_UserId",
                table: "EnrolledMembershipModels");

            migrationBuilder.DropColumn(
                name: "IsGoalActve",
                table: "GoalModel");

            migrationBuilder.DropColumn(
                name: "EnrolledMembershipId",
                table: "EnrolledMembershipModels");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "EnrolledMembershipModels");

            migrationBuilder.RenameTable(
                name: "GoalModel",
                newName: "Goals");

            migrationBuilder.RenameTable(
                name: "EnrolledMembershipModels",
                newName: "EnrolledMembershipModel");

            migrationBuilder.RenameColumn(
                name: "IsGoalReached",
                table: "Goals",
                newName: "GoalReached");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "EnrolledMembershipModel",
                newName: "isActive");

            migrationBuilder.RenameIndex(
                name: "IX_EnrolledMembershipModels_MembershipId",
                table: "EnrolledMembershipModel",
                newName: "IX_EnrolledMembershipModel_MembershipId");

            migrationBuilder.AlterColumn<float>(
                name: "GoalAmount",
                table: "Goals",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Goals",
                table: "Goals",
                column: "GoalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnrolledMembershipModel",
                table: "EnrolledMembershipModel",
                columns: new[] { "UserId", "MembershipId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EnrolledMembershipModel_Memberships_MembershipId",
                table: "EnrolledMembershipModel",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "MembershipId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EnrolledMembershipModel_Users_UserId",
                table: "EnrolledMembershipModel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
