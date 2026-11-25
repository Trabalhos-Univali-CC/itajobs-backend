using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Itajobs_API.Migrations
{
    /// <inheritdoc />
    public partial class PerfilEmpresaVagaAuthExterna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // E-mails antigos podem ter maiuscula ou espaco (de antes da API normalizar).
            // Ajusta antes do CHECK ck_contato_email_normalizado e do indice unico.
            migrationBuilder.Sql("UPDATE contato SET email = lower(btrim(email)) WHERE email <> lower(btrim(email));");

            migrationBuilder.AddColumn<string>(
                name: "modalidade",
                table: "vaga",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "salario_maximo",
                table: "vaga",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "salario_minimo",
                table: "vaga",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao_curta",
                table: "usuario_empresa",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "linkedin",
                table: "usuario_empresa",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ramo_atuacao",
                table: "usuario_empresa",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "razao_social",
                table: "usuario_empresa",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "site",
                table: "usuario_empresa",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sobre",
                table: "usuario_empresa",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "titulo_profissional",
                table: "curriculo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "empresa_arquivo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamanho = table.Column<int>(type: "integer", nullable: false),
                    conteudo = table.Column<byte[]>(type: "bytea", nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa_arquivo", x => x.id);
                    table.CheckConstraint("ck_empresa_arquivo_content_type", "content_type IN ('image/jpeg', 'image/png', 'image/webp')");
                    table.CheckConstraint("ck_empresa_arquivo_ordem", "ordem >= 0");
                    table.CheckConstraint("ck_empresa_arquivo_tamanho", "tamanho > 0");
                    table.CheckConstraint("ck_empresa_arquivo_tipo", "tipo IN ('Logo', 'Banner', 'Galeria')");
                    table.ForeignKey(
                        name: "FK_empresa_arquivo_usuario_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "usuario_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identidade_externa",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_candidato = table.Column<int>(type: "integer", nullable: true),
                    id_empresa = table.Column<int>(type: "integer", nullable: true),
                    provedor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    identificador_externo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identidade_externa", x => x.id);
                    table.CheckConstraint("ck_identidade_externa_identificador", "identificador_externo <> ''");
                    table.CheckConstraint("ck_identidade_externa_provedor", "provedor IN ('google')");
                    table.CheckConstraint("ck_identidade_externa_titular", "(id_candidato IS NULL) <> (id_empresa IS NULL)");
                    table.ForeignKey(
                        name: "FK_identidade_externa_usuario_candidato_id_candidato",
                        column: x => x.id_candidato,
                        principalTable: "usuario_candidato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_identidade_externa_usuario_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "usuario_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recuperacao_senha",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_candidato = table.Column<int>(type: "integer", nullable: true),
                    id_empresa = table.Column<int>(type: "integer", nullable: true),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    utilizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recuperacao_senha", x => x.id);
                    table.CheckConstraint("ck_recuperacao_senha_expira", "expira_em > criado_em");
                    table.CheckConstraint("ck_recuperacao_senha_titular", "(id_candidato IS NULL) <> (id_empresa IS NULL)");
                    table.ForeignKey(
                        name: "FK_recuperacao_senha_usuario_candidato_id_candidato",
                        column: x => x.id_candidato,
                        principalTable: "usuario_candidato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recuperacao_senha_usuario_empresa_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "usuario_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vaga_palavra_chave",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_vaga = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaga_palavra_chave", x => x.id);
                    table.CheckConstraint("ck_vaga_palavra_chave_descricao", "descricao = btrim(descricao) AND descricao <> ''");
                    table.ForeignKey(
                        name: "FK_vaga_palavra_chave_vaga_id_vaga",
                        column: x => x.id_vaga,
                        principalTable: "vaga",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_vaga_modalidade",
                table: "vaga",
                sql: "modalidade IS NULL OR modalidade IN ('Presencial', 'Hibrido', 'Remoto')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vaga_remuneracao",
                table: "vaga",
                sql: "(salario IS NULL AND salario_minimo IS NULL AND salario_maximo IS NULL) OR (salario > 0 AND salario_minimo IS NULL AND salario_maximo IS NULL) OR (salario IS NULL AND salario_minimo > 0 AND salario_maximo >= salario_minimo)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vaga_tipo_contrato",
                table: "vaga",
                sql: "tipo_contrato IN ('Clt', 'Pj', 'Estagio', 'Temporario', 'Aprendiz', 'Diaria')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_contato_email_normalizado",
                table: "contato",
                sql: "email = lower(btrim(email)) AND email <> ''");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_arquivo_galeria",
                table: "empresa_arquivo",
                columns: new[] { "id_empresa", "tipo", "ordem", "id" });

            migrationBuilder.CreateIndex(
                name: "ux_empresa_arquivo_logo_banner",
                table: "empresa_arquivo",
                columns: new[] { "id_empresa", "tipo" },
                unique: true,
                filter: "tipo IN ('Logo', 'Banner')");

            migrationBuilder.CreateIndex(
                name: "IX_identidade_externa_id_candidato",
                table: "identidade_externa",
                column: "id_candidato");

            migrationBuilder.CreateIndex(
                name: "IX_identidade_externa_id_empresa",
                table: "identidade_externa",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_identidade_externa_provedor_identificador_externo",
                table: "identidade_externa",
                columns: new[] { "provedor", "identificador_externo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recuperacao_senha_id_candidato",
                table: "recuperacao_senha",
                column: "id_candidato");

            migrationBuilder.CreateIndex(
                name: "IX_recuperacao_senha_id_empresa",
                table: "recuperacao_senha",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_recuperacao_senha_token_hash",
                table: "recuperacao_senha",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vaga_palavra_chave_id_vaga",
                table: "vaga_palavra_chave",
                column: "id_vaga");

            // Indices de expressao (o EF nao gera): e-mail unico entre todos os contatos,
            // de candidatos e empresas, e palavra-chave unica por vaga, os dois sem
            // diferenciar maiuscula nem espaco nas pontas.
            migrationBuilder.Sql("CREATE UNIQUE INDEX ux_contato_email ON contato (lower(btrim(email)));");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_vaga_palavra_chave_descricao ON vaga_palavra_chave (id_vaga, lower(btrim(descricao)));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_contato_email;");

            migrationBuilder.DropTable(
                name: "empresa_arquivo");

            migrationBuilder.DropTable(
                name: "identidade_externa");

            migrationBuilder.DropTable(
                name: "recuperacao_senha");

            migrationBuilder.DropTable(
                name: "vaga_palavra_chave");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vaga_modalidade",
                table: "vaga");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vaga_remuneracao",
                table: "vaga");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vaga_tipo_contrato",
                table: "vaga");

            migrationBuilder.DropCheckConstraint(
                name: "ck_contato_email_normalizado",
                table: "contato");

            migrationBuilder.DropColumn(
                name: "modalidade",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "salario_maximo",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "salario_minimo",
                table: "vaga");

            migrationBuilder.DropColumn(
                name: "descricao_curta",
                table: "usuario_empresa");

            migrationBuilder.DropColumn(
                name: "linkedin",
                table: "usuario_empresa");

            migrationBuilder.DropColumn(
                name: "ramo_atuacao",
                table: "usuario_empresa");

            migrationBuilder.DropColumn(
                name: "razao_social",
                table: "usuario_empresa");

            migrationBuilder.DropColumn(
                name: "site",
                table: "usuario_empresa");

            migrationBuilder.DropColumn(
                name: "sobre",
                table: "usuario_empresa");

            migrationBuilder.DropColumn(
                name: "titulo_profissional",
                table: "curriculo");
        }
    }
}
