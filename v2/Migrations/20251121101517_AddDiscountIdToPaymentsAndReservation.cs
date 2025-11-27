using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountIdToPaymentsAndReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DiscountID",
                table: "Reservations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountID",
                table: "Payments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DiscountID",
                table: "Payments");
        }
    }
}
