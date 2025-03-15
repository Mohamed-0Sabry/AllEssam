using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlIssam.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddedPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Payment_Method",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "cash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payment_Method",
                table: "Orders");
        }
    }
}
