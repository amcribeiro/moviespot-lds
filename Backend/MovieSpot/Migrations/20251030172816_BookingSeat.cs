using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MovieSpot.Migrations
{
    /// <inheritdoc />
    public partial class BookingSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CinemaHall_Seat_CinemaId",
                table: "CinemaHall");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_CreatedBy",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_CreatedBy",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Seat");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "Seat",
                newName: "SeatNumber");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Session",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CinemaHallId",
                table: "Seat",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SeatType",
                table: "Seat",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BookingSeat",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    SeatId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SeatPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSeat", x => new { x.BookingId, x.SeatId });
                    table.ForeignKey(
                        name: "FK_BookingSeat_Booking_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingSeat_Seat_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cinema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinema", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_CreatedByUserId",
                table: "Session",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Seat_CinemaHallId",
                table: "Seat",
                column: "CinemaHallId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingSeat_SeatId",
                table: "BookingSeat",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_CinemaHall_Cinema_CinemaId",
                table: "CinemaHall",
                column: "CinemaId",
                principalTable: "Cinema",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seat_CinemaHall_CinemaHallId",
                table: "Seat",
                column: "CinemaHallId",
                principalTable: "CinemaHall",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_User_CreatedByUserId",
                table: "Session",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CinemaHall_Cinema_CinemaId",
                table: "CinemaHall");

            migrationBuilder.DropForeignKey(
                name: "FK_Seat_CinemaHall_CinemaHallId",
                table: "Seat");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_CreatedByUserId",
                table: "Session");

            migrationBuilder.DropTable(
                name: "BookingSeat");

            migrationBuilder.DropTable(
                name: "Cinema");

            migrationBuilder.DropIndex(
                name: "IX_Session_CreatedByUserId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Seat_CinemaHallId",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CinemaHallId",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "Seat");

            migrationBuilder.RenameColumn(
                name: "SeatNumber",
                table: "Seat",
                newName: "ZipCode");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Seat",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Seat",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Seat",
                type: "numeric(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Seat",
                type: "numeric(9,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Seat",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Seat",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Seat",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CreatedBy",
                table: "Session",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CinemaHall_Seat_CinemaId",
                table: "CinemaHall",
                column: "CinemaId",
                principalTable: "Seat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_User_CreatedBy",
                table: "Session",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
