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

## Rodando

```bash
dotnet run
```

Dev: `http://localhost:5176` — OpenAPI em `/openapi/v1.json`, requests de teste no
`Itajobs-API.http`.
