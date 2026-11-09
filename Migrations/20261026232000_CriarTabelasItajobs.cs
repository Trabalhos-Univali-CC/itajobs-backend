using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Itajobs_API.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelasItajobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exemplo");

            migrationBuilder.CreateTable(
                name: "contato",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "endereco",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rua = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    numero = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_endereco", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario_candidato",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sobrenome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    id_endereco = table.Column<int>(type: "integer", nullable: false),
                    id_contato = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_candidato", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_candidato_contato_id_contato",
                        column: x => x.id_contato,
                        principalTable: "contato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_candidato_endereco_id_endereco",
                        column: x => x.id_endereco,
                        principalTable: "endereco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_empresa",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    id_endereco = table.Column<int>(type: "integer", nullable: false),
                    id_contato = table.Column<int>(type: "integer", nullable: false),
                    nota = table.Column<decimal>(type: "numeric(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_empresa", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_empresa_contato_id_contato",
                        column: x => x.id_contato,
                        principalTable: "contato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_empresa_endereco_id_endereco",
                        column: x => x.id_endereco,
                        principalTable: "endereco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "curriculo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_candidato = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculo", x => x.id);
                    table.ForeignKey(
                        name: "FK_curriculo_usuario_candidato_id_candidato",
                        column: x => x.id_candidato,
                        principalTable: "usuario_candidato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vaga",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    cargo = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    salario = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaga", x => x.id);
                    table.ForeignKey(
                        name: "FK_vaga_usuario_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "usuario_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logs_experiencias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_curriculo = table.Column<int>(type: "integer", nullable: false),
                    cargo = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    nome_empresa = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_experiencias", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_experiencias_curriculo_id_curriculo",
                        column: x => x.id_curriculo,
                        principalTable: "curriculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logs_habilidades",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_curriculo = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_habilidades", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_habilidades_curriculo_id_curriculo",
                        column: x => x.id_curriculo,
                        principalTable: "curriculo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidatura",
                columns: table => new
                {
                    id_vaga = table.Column<int>(type: "integer", nullable: false),
                    id_candidato = table.Column<int>(type: "integer", nullable: false),
                    data_candidatura = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidatura", x => new { x.id_vaga, x.id_candidato });
                    table.ForeignKey(
                        name: "FK_candidatura_usuario_candidato_id_candidato",
                        column: x => x.id_candidato,
                        principalTable: "usuario_candidato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_candidatura_vaga_id_vaga",
                        column: x => x.id_vaga,
                        principalTable: "vaga",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidatura_id_candidato",
                table: "candidatura",
                column: "id_candidato");

            migrationBuilder.CreateIndex(
                name: "IX_curriculo_id_candidato",
                table: "curriculo",
                column: "id_candidato",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logs_experiencias_id_curriculo",
                table: "logs_experiencias",
                column: "id_curriculo");

            migrationBuilder.CreateIndex(
                name: "IX_logs_habilidades_id_curriculo",
                table: "logs_habilidades",
                column: "id_curriculo");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_candidato_cpf",
                table: "usuario_candidato",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_candidato_id_contato",
                table: "usuario_candidato",
                column: "id_contato");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_candidato_id_endereco",
                table: "usuario_candidato",
                column: "id_endereco");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_empresa_cnpj",
                table: "usuario_empresa",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_empresa_id_contato",
                table: "usuario_empresa",
                column: "id_contato");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_empresa_id_endereco",
                table: "usuario_empresa",
                column: "id_endereco");

            migrationBuilder.CreateIndex(
                name: "IX_vaga_id_empresa",
                table: "vaga",
                column: "id_empresa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidatura");

            migrationBuilder.DropTable(
                name: "logs_experiencias");

            migrationBuilder.DropTable(
                name: "logs_habilidades");

            migrationBuilder.DropTable(
                name: "vaga");

            migrationBuilder.DropTable(
                name: "curriculo");

            migrationBuilder.DropTable(
                name: "usuario_empresa");

            migrationBuilder.DropTable(
                name: "usuario_candidato");

            migrationBuilder.DropTable(
                name: "contato");

            migrationBuilder.DropTable(
                name: "endereco");

            migrationBuilder.CreateTable(
                name: "exemplo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exemplo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exemplo_Nome",
                table: "exemplo",
                column: "Nome");
        }
    }
}
