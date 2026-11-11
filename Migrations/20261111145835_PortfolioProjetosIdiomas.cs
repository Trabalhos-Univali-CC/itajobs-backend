using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Itajobs_API.Migrations
{
    /// <inheritdoc />
    public partial class PortfolioProjetosIdiomas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "logs_experiencias",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "portfolio",
                table: "curriculo",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "logs_idiomas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_curriculo = table.Column<int>(type: "integer", nullable: false),
                    idioma = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_idiomas", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_idiomas_curriculo_id_curriculo",
                        column: x => x.id_curriculo,
                        principalTable: "curriculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logs_projetos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_curriculo = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_projetos", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_projetos_curriculo_id_curriculo",
                        column: x => x.id_curriculo,
                        principalTable: "curriculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_logs_idiomas_id_curriculo",
                table: "logs_idiomas",
                column: "id_curriculo");

            migrationBuilder.CreateIndex(
                name: "IX_logs_projetos_id_curriculo",
                table: "logs_projetos",
                column: "id_curriculo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "logs_idiomas");

            migrationBuilder.DropTable(
                name: "logs_projetos");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "logs_experiencias");

            migrationBuilder.DropColumn(
                name: "portfolio",
                table: "curriculo");
        }
    }
}
