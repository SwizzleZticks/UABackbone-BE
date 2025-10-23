using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UABackbone_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdsAndAddBlacklistedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing FK before modifying key types
            migrationBuilder.DropForeignKey(
                name: "Users_LocalUnions_FK",
                table: "Users");

            // Update Users.local_id
            migrationBuilder.AlterColumn<int>(
                name: "local_id",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            // Update Users.id
            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            // Update PendingUsers.Local
            migrationBuilder.AlterColumn<int>(
                name: "Local",
                table: "PendingUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            // Update PendingUsers.Id
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PendingUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            // Update LocalUnions.Local
            migrationBuilder.AlterColumn<int>(
                name: "Local",
                table: "LocalUnions",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            // Recreate FK after both columns are ints
            migrationBuilder.AddForeignKey(
                name: "Users_LocalUnions_FK",
                table: "Users",
                column: "local_id",
                principalTable: "LocalUnions",
                principalColumn: "Local",
                onDelete: ReferentialAction.Cascade);

            // Create BlacklistedUsers table
            migrationBuilder.CreateTable(
                name: "BlacklistedUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserAffectedId = table.Column<int>(type: "int", nullable: false),
                    ByAdminId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlacklistedUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlacklistedUsers_Users_ByAdminId",
                        column: x => x.ByAdminId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlacklistedUsers_Users_UserAffectedId",
                        column: x => x.UserAffectedId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedUsers_ByAdminId",
                table: "BlacklistedUsers",
                column: "ByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedUsers_UserAffectedId",
                table: "BlacklistedUsers",
                column: "UserAffectedId");
        }
    }
}
