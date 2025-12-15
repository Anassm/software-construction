using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class organisationsUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOrganizationAdmin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationID",
                table: "Sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationID",
                table: "Reservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_OrganizationID",
                table: "Sessions",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_OrganizationID",
                table: "Reservations",
                column: "OrganizationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Organizations_OrganizationID",
                table: "Reservations",
                column: "OrganizationID",
                principalTable: "Organizations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Organizations_OrganizationID",
                table: "Sessions",
                column: "OrganizationID",
                principalTable: "Organizations",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Organizations_OrganizationID",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Organizations_OrganizationID",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_OrganizationID",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_OrganizationID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsOrganizationAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationID",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "OrganizationID",
                table: "Reservations");
        }
    }
}
