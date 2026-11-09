# Itajobs-API

API REST em ASP.NET Core (net10.0) com EF Core e PostgreSQL. Mesma estrutura do
projeto `E-Commerce API`.

## Estrutura

```
Itajobs-API/
├── Itajobs-API.csproj
├── Program.cs                      # Entrypoint: registra servicos e pipeline
├── Itajobs-API.http                # Requests de teste
├── Migrations/                     # Gerado pelo EF (nao editar a mao)
├── Core/
│   └── Secrets/                    # .env gitignored
│       ├── itajobs.env             #   credenciais (VAZIAS)
│       └── itajobs.env.example
├── Properties/launchSettings.json
├── appsettings.json
└── src/
    ├── Controllers/                # HTTP: rota, status code
    │   └── ExemploController.cs
    ├── Services/                   # Regra de negocio, Model <-> DTO
    │   └── ExemploService.cs
    ├── Repositories/               # Acesso ao banco
    │   ├── IExemploRepository.cs
    │   └── ExemploRepository.cs
    ├── DTO/                        # O que entra e sai pela API
    │   ├── ExemploDTO.cs           #   saida
    │   └── ExemploCreateDTO.cs     #   entrada
    ├── Models/                     # As tabelas representadas em classe
    │   └── Exemplo.cs
    ├── Data/                       # Configuracao do banco
    │   └── AppDbContext.cs
    └── Utils/                      # Auxiliares sem dono
        └── EnvLoader.cs
```

## As camadas

Uma request desce e volta:

```
HTTP
  -> Controller     le a rota, escolhe o status code
  -> Service        regra de negocio, converte Model <-> DTO
  -> Repository     unica camada que toca o AppDbContext
  -> AppDbContext   EF Core
  -> PostgreSQL
```

Cada camada so conhece a de baixo:

| Camada | Conhece | NAO conhece |
|---|---|---|
| Controller | Service, DTO | EF Core, Model |
| Service | Repository, Model, DTO | HTTP, EF Core |
| Repository | AppDbContext, Model | HTTP, DTO |

Por que importa: com o Repository isolando o EF, trocar Postgres por SQL Server mexe
numa camada so. Com o Service sem HTTP, a regra de negocio da pra testar sem subir
servidor.

### Model x DTO

- **Model** (`src/Models/Exemplo.cs`) — a tabela. Uma classe = uma tabela, uma
  propriedade = uma coluna. Nunca sai da API.
- **DTO** (`src/DTO/`) — o contrato com o cliente. Existe para a tabela mudar sem
  quebrar quem consome, e para nao vazar coluna interna.

`ExemploCreateDTO` nao tem `Id` nem `CriadoEm` de proposito: quem decide esses e o
servidor. Se o Model fosse aceito direto no `[FromBody]`, o cliente poderia mandar
os dois.

## Criando uma entidade nova

Copie os arquivos de `Exemplo` trocando o nome — sao seis:

1. `src/Models/Vaga.cs` — a tabela
2. `src/Data/AppDbContext.cs` — adicione `public DbSet<Vaga> Vagas { get; set; }`
3. `src/DTO/VagaDTO.cs` e `VagaCreateDTO.cs`
4. `src/Repositories/IVagaRepository.cs` e `VagaRepository.cs`
5. `src/Services/VagaService.cs`
6. `src/Controllers/VagaController.cs`

Depois registre no `Program.cs`:

```csharp
builder.Services.AddScoped<IVagaRepository, VagaRepository>();
builder.Services.AddScoped<VagaService>();
```

E gere a migration:

```bash
dotnet ef migrations add AddVaga
```

## Rotas

`[Route("api/[controller]")]` — o nome do controller vira o caminho.

| Verbo | Rota | Retorno |
|---|---|---|
| GET | `/api/exemplo` | 200 + lista |
| GET | `/api/exemplo?apenasAtivos=true` | 200 + lista filtrada |
| GET | `/api/exemplo/{id}` | 200 ou 404 |
| POST | `/api/exemplo` | 201 + header Location |
| PUT | `/api/exemplo/{id}` | 200 ou 404 |
| DELETE | `/api/exemplo/{id}` | 204 ou 404 |

Validacao e automatica: `[ApiController]` devolve 400 sozinho quando o DTO fura os
DataAnnotations. Nao ha validacao manual no controller.

## Banco

As credenciais ficam em `Core/Secrets/itajobs.env`, **fora** do `appsettings.json`
— que vai versionado. O `EnvLoader` le o arquivo e monta a connection string; o
`Program.cs` passa para o `AddDbContext`.

```env
DB_HOST=
DB_PORT=5432
DB_DATABASE=
DB_USER=
DB_PASSWORD=
```

> Os valores estao **vazios**. Preencha antes de subir; sem eles as rotas dao 500.

Aplicar as migrations num banco novo:

```bash
dotnet ef database update
```

## Autenticacao

JWT. O login devolve um token que vai nas proximas requests no header
`Authorization: Bearer {token}`.

| Verbo | Rota | Corpo / retorno |
|---|---|---|
| POST | `/api/auth/login` | `{ email, senha, tipo: "candidato" \| "empresa" }` → `{ token, expiraEm, usuario }` |
| GET | `/api/auth/me` | quem esta logado |
| PUT | `/api/auth/senha` | `{ senhaAtual, novaSenha }` → 204. Unico jeito de trocar a senha |

- **Limite de tentativas:** 5 logins por minuto por IP (`LOGIN_TENTATIVAS_POR_MINUTO`);
  passou disso, 429. Atras do proxy do Next vale o IP do `X-Forwarded-For`.
- **Toda rota exige login**, menos as marcadas com `[AllowAnonymous]`: cadastro de
  candidato e de empresa, login e listagem/detalhe de vagas. Rota nova ja nasce
  protegida.
- Cada um so altera os proprios dados; empresas podem ver candidatos e curriculos.
  As regras de cada controller estao no `<remarks>` dele.
- A senha e guardada so como hash (PBKDF2, `SenhaHasher`). O e-mail e unico dentro de
  cada tipo de conta, por isso o login pede o `tipo`.
- Erros seguem o formato da API: 401 sem login, 403 sem permissao, ambos com
  `{ "mensagem": "..." }`.

Chaves no `itajobs.env`:

```env
JWT_SECRET=        # 32+ caracteres aleatorios; trocar invalida todos os logins
JWT_EXPIRA_HORAS=8
```

## Endereco: so Itajai/SC

O CEP e obrigatorio e e consultado no ViaCEP. Se nao for de Itajai (codigo IBGE
4208203), o cadastro e recusado com 400. A cidade sempre vem do CEP; rua e bairro em
branco tambem. Se o ViaCEP nao responder, a API devolve 503.

## Vagas, candidaturas, avaliacoes e curriculo

Enums trafegam como texto camelCase (`"clt"`, `"meioPeriodo"`, `"emAnalise"`); numero
e recusado. No banco ficam como texto (`"Clt"`, `"EmAnalise"`).

**Vagas**
- Campos: `tipoContrato` (clt, pj, estagio, temporario, aprendiz), `jornada` (integral,
  meioPeriodo, noturno, escala), `bairro` (vazio = bairro da empresa), `requisitos`,
  `status` e `dataPublicacao`.
- `GET /api/vaga` e paginado: devolve `{ itens, pagina, tamanhoPagina, total, totalPaginas }`.
  Filtros: `cargo`, `bairro`, `salarioMinimo`, `salarioMaximo`, `tipoContrato`, `jornada`,
  `status`, `idEmpresa`, `pagina`, `tamanhoPagina` (ate 100). Sem `status`, so abertas,
  menos quando filtra por `idEmpresa`.
- `PATCH /api/vaga/{id}/status` `{ status: "encerrada" | "aberta" }`. Vaga encerrada nao
  aceita candidatura.

**Candidaturas**
- Status: recebida → emAnalise → aprovada / recusada, so a empresa dona muda:
  `PATCH /api/vaga/{idVaga}/candidaturas/{idCandidato}/status`.
- `GET /api/vaga/{id}/candidaturas?status=emAnalise` filtra.

**Avaliacoes de empresa** (substituem o antigo `PATCH /api/empresa/{id}/nota`)
- `PUT /api/empresa/{idEmpresa}/avaliacoes/{idCandidato}` `{ nota: 1..5, comentario }`:
  so quem se candidatou a uma vaga da empresa; avaliar de novo substitui.
- `GET /api/empresa/{id}/avaliacoes`, `DELETE .../avaliacoes/{idCandidato}`.
- A `nota` da empresa e a media, com `totalAvaliacoes`.

**Curriculo**
- `PUT /api/curriculo/{id}` `{ resumo }`.
- Formacoes e cursos: `POST/PUT/DELETE /api/curriculo/{id}/formacoes[/{idFormacao}]` e
  `.../cursos[/{idCurso}]`.
- Arquivos: `PUT /api/curriculo/{id}/arquivos/{pdf|foto}` (multipart, campo `arquivo`),
  `GET` para baixar, `DELETE`. PDF ate 5 MB; foto JPG/PNG/WebP ate 2 MB. O tipo e
  conferido pelo conteudo. Ficam no banco (`bytea`), entao saem no backup e no cascade.

## Rodando

```bash
dotnet run
```

Dev: `http://localhost:5176` — OpenAPI em `/openapi/v1.json`, requests de teste no
`Itajobs-API.http`.
