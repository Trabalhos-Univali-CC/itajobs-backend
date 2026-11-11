using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itajobs_API.Migrations
{
    /// <inheritdoc />
    public partial class IndiceEmailContato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_contato_email",
                table: "contato",
                column: "email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_contato_email",
                table: "contato");
        }
    }
}
