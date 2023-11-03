using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Migrations
{
    /// <inheritdoc />
    public partial class V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Delay",
                table: "Ip",
                newName: "DelayReponse");

            migrationBuilder.AddColumn<double>(
                name: "DelayConnection",
                table: "Ip",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelayConnection",
                table: "Ip");

            migrationBuilder.RenameColumn(
                name: "DelayReponse",
                table: "Ip",
                newName: "Delay");
        }
    }
}
