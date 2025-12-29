using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieSpot.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Voucher_VoucherId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_CreatedByUserId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_CreatedByUserId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Review_BookingId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Payment_BookingId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Session");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CreatedBy",
                table: "Session",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Review_BookingId",
                table: "Review",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_BookingId",
                table: "Payment",
                column: "BookingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Voucher_VoucherId",
                table: "Payment",
                column: "VoucherId",
                principalTable: "Voucher",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_User_CreatedBy",
                table: "Session",
                column: "CreatedBy",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Voucher_VoucherId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_CreatedBy",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_CreatedBy",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Review_BookingId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Payment_BookingId",
                table: "Payment");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Session",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Session_CreatedByUserId",
                table: "Session",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_BookingId",
                table: "Review",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_BookingId",
                table: "Payment",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Voucher_VoucherId",
                table: "Payment",
                column: "VoucherId",
                principalTable: "Voucher",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_User_CreatedByUserId",
                table: "Session",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
