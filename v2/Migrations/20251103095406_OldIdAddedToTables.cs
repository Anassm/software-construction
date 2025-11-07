using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class OldIdAddedToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_ParkingLots_ParkingLotID",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Users_UserID",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Vehicles_VehicleID",
                table: "Reservation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reservation",
                table: "Reservation");

            migrationBuilder.RenameTable(
                name: "Reservation",
                newName: "Reservations");

            migrationBuilder.RenameIndex(
                name: "IX_Reservation_VehicleID",
                table: "Reservations",
                newName: "IX_Reservations_VehicleID");

            migrationBuilder.RenameIndex(
                name: "IX_Reservation_UserID",
                table: "Reservations",
                newName: "IX_Reservations_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Reservation_ParkingLotID",
                table: "Reservations",
                newName: "IX_Reservations_ParkingLotID");

            migrationBuilder.AddColumn<string>(
                name: "OldID",
                table: "Vehicles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldID",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldID",
                table: "Sessions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldID",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldID",
                table: "ParkingLots",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldID",
                table: "Reservations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

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
                name: "AK_Payments_OldID",
                table: "Payments",
                column: "OldID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ParkingLots_OldID",
                table: "ParkingLots",
                column: "OldID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Reservations_OldID",
                table: "Reservations",
                column: "OldID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotID",
                table: "Reservations",
                column: "ParkingLotID",
                principalTable: "ParkingLots",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_UserID",
                table: "Reservations",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Vehicles_VehicleID",
                table: "Reservations",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_ParkingLots_ParkingLotID",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_UserID",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Vehicles_VehicleID",
                table: "Reservations");

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
                name: "AK_Payments_OldID",
                table: "Payments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ParkingLots_OldID",
                table: "ParkingLots");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Reservations_OldID",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "OldID",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "OldID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OldID",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "OldID",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OldID",
                table: "ParkingLots");

            migrationBuilder.DropColumn(
                name: "OldID",
                table: "Reservations");

            migrationBuilder.RenameTable(
                name: "Reservations",
                newName: "Reservation");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_VehicleID",
                table: "Reservation",
                newName: "IX_Reservation_VehicleID");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_UserID",
                table: "Reservation",
                newName: "IX_Reservation_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ParkingLotID",
                table: "Reservation",
                newName: "IX_Reservation_ParkingLotID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reservation",
                table: "Reservation",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_ParkingLots_ParkingLotID",
                table: "Reservation",
                column: "ParkingLotID",
                principalTable: "ParkingLots",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Users_UserID",
                table: "Reservation",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Vehicles_VehicleID",
                table: "Reservation",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
