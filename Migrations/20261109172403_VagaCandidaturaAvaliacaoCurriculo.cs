using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Itajobs_API.Migrations
{
    /// <inheritdoc />
    public partial class VagaCandidaturaAvaliacaoCurriculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Linhas que ja existiam recebem valores validos dos enums (Aberta, Clt...).
            migrationBuilder.AddColumn<string>(
                name: "bairro",
                table: "vaga",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_publicacao",
                table: "vaga",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "jornada",
                table: "vaga",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Integral");

            migrationBuilder.AddColumn<string>(
                name: "requisitos",
                table: "vaga",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "vaga",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Aberta");

            migrationBuilder.AddColumn<string>(
                name: "tipo_contrato",
                table: "vaga",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Clt");

            migrationBuilder.AddColumn<string>(
                name: "resumo",
                table: "curriculo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_status",
                table: "candidatura",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "candidatura",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Recebida");

            migrationBuilder.CreateTable(
                name: "avaliacao_empresa",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    id_candidato = table.Column<int>(type: "integer", nullable: false),
                    nota = table.Column<int>(type: "integer", nullable: false),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacao_empresa", x => new { x.id_empresa, x.id_candidato });
                    table.CheckConstraint("ck_avaliacao_nota", "nota BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_avaliacao_empresa_usuario_candidato_id_candidato",
                        column: x => x.id_candidato,
                        principalTable: "usuario_candidato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_avaliacao_empresa_usuario_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "usuario_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "curriculo_arquivo",
                columns: table => new
                {
                    id_curriculo = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamanho = table.Column<int>(type: "integer", nullable: false),
                    conteudo = table.Column<byte[]>(type: "bytea", nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculo_arquivo", x => new { x.id_curriculo, x.tipo });
                    table.ForeignKey(
                        name: "FK_curriculo_arquivo_curriculo_id_curriculo",
                        column: x => x.id_curriculo,
                        principalTable: "curriculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logs_cursos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_curriculo = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    instituicao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    carga_horaria = table.Column<int>(type: "integer", nullable: true),
                    data_conclusao = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_cursos", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_cursos_curriculo_id_curriculo",
                        column: x => x.id_curriculo,
                        principalTable: "curriculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logs_formacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_curriculo = table.Column<int>(type: "integer", nullable: false),
                    instituicao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    curso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_formacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_formacoes_curriculo_id_curriculo",
                        column: x => x.id_curriculo,
                        principalTable: "curriculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vaga_status_data_publicacao",
                table: "vaga",
                columns: new[] { "status", "data_publicacao" });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacao_empresa_id_candidato",
                table: "avaliacao_empresa",
                column: "id_candidato");

            migrationBuilder.CreateIndex(
                name: "IX_logs_cursos_id_curriculo",
                table: "logs_cursos",
                column: "id_curriculo");

            migrationBuilder.CreateIndex(
                name: "IX_logs_formacoes_id_curriculo",
                table: "logs_formacoes",
                column: "id_curriculo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avaliacao_empresa");

            migrationBuilder.DropTable(
                name: "curriculo_arquivo");

            migrationBuilder.DropTable(
                name: "logs_cursos");

            migrationBuilder.DropTable(
                name: "logs_formacoes");

            migrationBuilder.DropIndex(
                name: "IX_vaga_status_data_publicacao",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "bairro",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "data_publicacao",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "jornada",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "requisitos",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "status",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "tipo_contrato",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "resumo",
                table: "curriculo");

            migrationBuilder.DropColumn(
                name: "data_status",
                table: "candidatura");

            migrationBuilder.DropColumn(
                name: "status",
                table: "candidatura");
        }
    }
}
