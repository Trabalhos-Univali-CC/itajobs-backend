using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itajobs_API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSenhaECep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "senha_hash",
                table: "usuario_empresa",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "senha_hash",
                table: "usuario_candidato",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cep",
                table: "endereco",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "senha_hash",
                table: "usuario_empresa");

            migrationBuilder.DropColumn(
                name: "senha_hash",
                table: "usuario_candidato");

            migrationBuilder.DropColumn(
                name: "cep",
                table: "endereco");
        }
    }
}
