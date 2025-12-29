using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieSpot.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "MaxUsages",
                table: "Voucher",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Usages",
                table: "Voucher",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxUsages",
                table: "Voucher");

            migrationBuilder.DropColumn(
                name: "Usages",
                table: "Voucher");
        }
    }
}
