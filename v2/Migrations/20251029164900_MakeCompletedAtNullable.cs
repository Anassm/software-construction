using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class MakeCompletedAtNullable : Migration
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

            migrationBuilder.AlterColumn<decimal>(
                name: "TransactionAmount",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "Payments",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reservations",
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

            migrationBuilder.AlterColumn<float>(
                name: "TransactionAmount",
                table: "Payments",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "Payments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Amount",
                table: "Payments",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

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
