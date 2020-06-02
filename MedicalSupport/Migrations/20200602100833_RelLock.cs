using Microsoft.EntityFrameworkCore.Migrations;

namespace MedicalSupport.Migrations
{
    public partial class RelLock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_sick_warden_onerel",
                table: "sick_warden",
                columns: new[] { "sick_id", "warden_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sick_warden_onerel",
                table: "sick_warden");
        }
    }
}
