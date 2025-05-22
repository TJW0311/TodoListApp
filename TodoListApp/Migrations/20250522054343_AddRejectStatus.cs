using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "status",
                columns: new[] { "StatusId", "Name" },
                values: new object[] { 6, "Rejected" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "StatusId",
                keyValue: 6);
        }
    }
}
