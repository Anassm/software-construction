using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class fixpendingchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationID",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "DiscountCodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationID",
                table: "Users",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountCodes_OrganizationId",
                table: "DiscountCodes",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountCodes_Organizations_OrganizationId",
                table: "DiscountCodes",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationID",
                table: "Users",
                column: "OrganizationID",
                principalTable: "Organizations",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscountCodes_Organizations_OrganizationId",
                table: "DiscountCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationID",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_DiscountCodes_OrganizationId",
                table: "DiscountCodes");

            migrationBuilder.DropColumn(
                name: "OrganizationID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "DiscountCodes");
        }
    }
}
