using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace v2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingLots",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Reserved = table.Column<int>(type: "INTEGER", nullable: false),
                    Tariff = table.Column<float>(type: "REAL", nullable: false),
                    DayTariff = table.Column<float>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    latitude = table.Column<float>(type: "REAL", nullable: false),
                    longitude = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingLots", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateOnly>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    duration = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<float>(type: "REAL", nullable: false, defaultValue: 0f),
                    PaymentStatus = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParkingLotID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaymentID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Sessions_ParkingLots_ParkingLotID",
                        column: x => x.ParkingLotID,
                        principalTable: "ParkingLots",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Make = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateOnly>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID1 = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_UserID1",
                        column: x => x.UserID1,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<float>(type: "REAL", nullable: false),
                    Initiator = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TransactionAmount = table.Column<float>(type: "REAL", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TransactionMethod = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TransactionIssuer = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TransactionBank = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SessionID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Payments_Sessions_SessionID",
                        column: x => x.SessionID,
                        principalTable: "Sessions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TotalPrice = table.Column<float>(type: "REAL", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: true),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParkingLotID = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Reservation_ParkingLots_ParkingLotID",
                        column: x => x.ParkingLotID,
                        principalTable: "ParkingLots",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservation_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservation_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionID",
                table: "Payments",
                column: "SessionID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ParkingLotID",
                table: "Reservation",
                column: "ParkingLotID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_UserID",
                table: "Reservation",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_VehicleID",
                table: "Reservation",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ParkingLotID",
                table: "Sessions",
                column: "ParkingLotID");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserID",
                table: "Sessions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserID",
                table: "Vehicles",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_UserID1",
                table: "Vehicles",
                column: "UserID1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "ParkingLots");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
