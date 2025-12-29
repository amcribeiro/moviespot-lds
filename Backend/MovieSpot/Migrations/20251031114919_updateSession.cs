using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MovieSpot.Migrations
{
    /// <inheritdoc />
    public partial class updateSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)");

            migrationBuilder.AddColumn<short>(
                name: "PromotionValue",
                table: "Session",
                type: "smallint",
                nullable: true);
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
                name: "PromotionValue",
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Session",
                type: "datetime2(0)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Session",
                type: "datetime2(0)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Session",
                type: "datetime2(0)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Session",
                type: "datetime2(0)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

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
