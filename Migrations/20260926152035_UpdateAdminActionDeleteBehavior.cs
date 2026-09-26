using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UABackbone_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminActionDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminActions_Users_UserAffectedId",
                table: "AdminActions");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminActions_Users_UserAffectedId",
                table: "AdminActions",
                column: "UserAffectedId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminActions_Users_UserAffectedId",
                table: "AdminActions");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminActions_Users_UserAffectedId",
                table: "AdminActions",
                column: "UserAffectedId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
