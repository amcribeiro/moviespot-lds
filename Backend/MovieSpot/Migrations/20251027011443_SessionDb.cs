using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MovieSpot.Migrations
{
    /// <inheritdoc />
    public partial class SessionDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CinemaHallId",
                table: "Session",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Session",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MovieId",
                table: "Session",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Session",
                type: "numeric(6,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Session",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "CinemaHall",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CinemaHall", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_CinemaHallId",
                table: "Session",
                column: "CinemaHallId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CreatedBy",
                table: "Session",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Session_MovieId",
                table: "Session",
                column: "MovieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_CinemaHall_CinemaHallId",
                table: "Session",
                column: "CinemaHallId",
                principalTable: "CinemaHall",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Movie_MovieId",
                table: "Session",
                column: "MovieId",
                principalTable: "Movie",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_CinemaHall_CinemaHallId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Movie_MovieId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_CreatedBy",
                table: "Session");

            migrationBuilder.DropTable(
                name: "CinemaHall");

            migrationBuilder.DropIndex(
                name: "IX_Session_CinemaHallId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_CreatedBy",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_MovieId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CinemaHallId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "MovieId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Session");
        }
    }
}
