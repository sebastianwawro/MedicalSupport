using Microsoft.EntityFrameworkCore.Migrations;

namespace MedicalSupport.Migrations
{
    public partial class SickProtection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_accepted",
                table: "sick_warden",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_proposed_by_warden",
                table: "sick_warden",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_accepted",
                table: "sick_warden");

            migrationBuilder.DropColumn(
                name: "is_proposed_by_warden",
                table: "sick_warden");
        }
    }
}
