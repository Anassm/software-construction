using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class removedsecondkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Vehicles_OldID",
                table: "Vehicles");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_OldID",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Sessions_OldID",
                table: "Sessions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Reservations_OldID",
                table: "Reservations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Payments_OldID",
                table: "Payments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParkingLots_OldID",
                table: "ParkingLots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Vehicles_OldID",
                table: "Vehicles",
                column: "OldID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_OldID",
                table: "Users",
                column: "OldID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Sessions_OldID",
                table: "Sessions",
                column: "OldID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Reservations_OldID",
                table: "Reservations",
                column: "OldID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Payments_OldID",
                table: "Payments",
                column: "OldID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParkingLots_OldID",
                table: "ParkingLots",
                column: "OldID");
        }
    }
}
