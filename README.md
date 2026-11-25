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

| Camada     | Conhece                | NAO conhece    |
| ---------- | ---------------------- | -------------- |
| Controller | Service, DTO           | EF Core, Model |
| Service    | Repository, Model, DTO | HTTP, EF Core  |
| Repository | AppDbContext, Model    | HTTP, DTO      |

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

As rotas estao em [Endpoints > Autenticacao](#autenticacao--apiauth).

- **Limite de tentativas:** 5 logins por minuto por IP (`LOGIN_TENTATIVAS_POR_MINUTO`);
  passou disso, 429. Atras do proxy do Next vale o IP do `X-Forwarded-For`.
- **Toda rota exige login**, menos as marcadas com `[AllowAnonymous]`: cadastro de
  candidato e de empresa, login (senha e Google), recuperacao de senha,
  listagem/detalhe de vagas e imagens das empresas. Rota nova ja nasce protegida.
- Cada um so altera os proprios dados; empresas podem ver candidatos e curriculos.
  As regras de cada controller estao no `<remarks>` dele.
- A senha e guardada so como hash (PBKDF2, `SenhaHasher`). O **e-mail e unico entre
  todas as contas** (candidatos e empresas; indice unico `lower(btrim(email))` no
  banco), entao o login so precisa de e-mail e senha. O `tipo` continua aceito.
- **Esqueci a senha:** o link vai por e-mail (SMTP, `SMTP_*`) e vale 30 min, uma vez. O
  banco guarda so o SHA-256 do token. Sem SMTP, em desenvolvimento o e-mail sai no log
  da API.
- **Google:** so entra quem ja tem conta. No primeiro acesso, a conta e achada pelo
  e-mail verificado do Google e fica vinculada pelo `sub`; depois o e-mail do Google
  pode mudar. Precisa de `GOOGLE_CLIENT_ID`.
- O limite de 5 por minuto vale tambem para o login com Google e, separado, para
  esqueci/redefinir senha.
- Erros seguem o formato da API: 401 sem login, 403 sem permissao, ambos com
  `{ "mensagem": "..." }`.

Chaves no `itajobs.env`:

```env
JWT_SECRET=        # 32+ caracteres aleatorios; trocar invalida todos os logins
JWT_EXPIRA_HORAS=8
GOOGLE_CLIENT_ID=  # Client ID OAuth do Google (o mesmo do botao no front)
SMTP_HOST=         # + SMTP_PORTA, SMTP_SSL, SMTP_USUARIO, SMTP_SENHA, SMTP_REMETENTE
RECUPERACAO_SENHA_MINUTOS=30
```

## Endereco: so Itajai/SC

O CEP e obrigatorio e e consultado no ViaCEP. Se nao for de Itajai (codigo IBGE
4208203), o cadastro e recusado com 400. A cidade sempre vem do CEP; rua e bairro em
branco tambem. Se o ViaCEP nao responder, a API devolve 503.

## Endpoints

Base em dev: `http://localhost:8080`. Todas as rotas comecam com `/api`.

### Convencoes

- **JSON em camelCase**, entrada e saida.
- **Enums como texto** camelCase (`"clt"`, `"emAnalise"`). Numero e recusado com 400.
- **Datas:** `DateOnly` como `"2024-05-10"`; data e hora como `"2026-11-09T14:29:56"`,
  no horario de Itajai, sem fuso.
- **CPF, CNPJ, CEP e telefone** aceitam com ou sem mascara; a API devolve sem.
- **Autenticacao:** header `Authorization: Bearer {token}` (ver [Autenticacao](#autenticacao)).
- **Validacao e automatica:** `[ApiController]` devolve 400 sozinho quando o DTO fura
  os DataAnnotations. Nao ha validacao manual no controller.

Coluna **Acesso** das tabelas:

| Acesso          | Quem pode                                                   |
| --------------- | ----------------------------------------------------------- |
| Publico         | qualquer um, sem token                                      |
| Logado          | qualquer conta logada                                       |
| Empresa         | qualquer empresa logada                                     |
| Proprio         | so o dono do recurso (o candidato ou a empresa da rota)     |
| Proprio/empresa | o candidato dono ou qualquer empresa                        |
| Empresa dona    | so a empresa dona da vaga                                   |

### Erros

| Status | Quando                                                                      |
| ------ | --------------------------------------------------------------------------- |
| 400    | DTO invalido, ou regra de negocio (CPF invalido, CEP fora de Itajai...)     |
| 401    | sem token, token invalido/expirado, ou e-mail/senha errados no login        |
| 403    | logado, mas o recurso e de outra conta                                      |
| 404    | o recurso da rota nao existe                                                |
| 409    | conflito com o banco (CPF, CNPJ ou e-mail repetido, candidatura duplicada)  |
| 413    | upload acima de 6 MB                                                        |
| 429    | mais de 5 tentativas de login por minuto do mesmo IP                        |
| 503    | o ViaCEP nao respondeu                                                      |

Corpo dos erros de regra (400, 401, 403, 404, 409, 429, 503):

```json
{ "mensagem": "Ja existe um candidato com este CPF." }
```

Erro de validacao do DTO (400), com os campos que falharam:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Senha": ["A senha deve ter entre 8 e 100 caracteres."] }
}
```

### Valores dos enums

| Campo                      | Valores                                                        |
| -------------------------- | -------------------------------------------------------------- |
| `tipo` (login, opcional)   | `candidato`, `empresa`                                         |
| `tipoContrato` (vaga)      | `clt`, `pj`, `estagio`, `temporario`, `aprendiz`, `diaria`     |
| `jornada` (vaga)           | `integral`, `meioPeriodo`, `noturno`, `escala`                 |
| `modalidade` (vaga)        | `presencial`, `hibrido`, `remoto` (opcional)                   |
| `tipo` (imagem da empresa) | `logo`, `banner`, `galeria`                                    |
| `status` (vaga)            | `aberta`, `encerrada`                                          |
| `status` (candidatura)     | `recebida`, `emAnalise`, `aprovada`, `recusada`                |
| `nivel` (formacao)         | `fundamental`, `medio`, `tecnico`, `superior`, `posGraduacao`  |
| `nivel` (idioma)           | `basico`, `intermediario`, `avancado`, `fluente`, `nativo`     |
| `tipo` (arquivo)           | `pdf`, `foto`                                                  |

### Autenticacao — `/api/auth`

| Verbo | Rota              | Acesso  | Corpo            | Sucesso                | Erros                           |
| ----- | ----------------- | ------- | ---------------- | ---------------------- | ------------------------------- |
| POST  | `/api/auth/login` | Publico | `LoginDTO`       | 200 + `TokenDTO`       | 400, 401, 429                   |
| GET   | `/api/auth/me`    | Logado  | —                | 200 + usuario logado   | 401                             |
| PUT   | `/api/auth/senha` | Logado  | `TrocarSenhaDTO` | 204                    | 400 (senha atual errada, igual ou curta), 401 |
| POST  | `/api/auth/esqueci-senha`   | Publico | `{ email }`            | 202 (sempre)     | 400, 429                   |
| POST  | `/api/auth/redefinir-senha` | Publico | `{ token, novaSenha }` | 204              | 400 (link invalido, expirado ou usado), 429 |
| POST  | `/api/auth/google`          | Publico | `{ idToken }`          | 200 + `TokenDTO` | 401 (token do Google invalido), 404 (sem conta), 429, 503 (sem `GOOGLE_CLIENT_ID`) |
| POST  | `/api/auth/google/vincular` | Logado  | `{ idToken }`          | 204              | 401, 409 (Google de outra conta, ou conta ja com outro Google), 503 |
| DELETE| `/api/auth/google`          | Logado  | —                      | 204              | 401, 404 (sem vinculo)     |

- **Esqueci a senha:** responde 202 com a mesma mensagem exista a conta ou nao. Pedir
  de novo invalida o link anterior. O link e `{FRONTEND_URL}/redefinir-senha?token=...`
  (ou `RECUPERACAO_SENHA_URL`); a pagina do front manda o `token` para
  `redefinir-senha`.
- **Google:** o front usa o botao "Entrar com Google" (Google Identity Services) e manda
  o `credential` (ID token) como `idToken`. A API confere assinatura, emissor, validade
  e se o token e do nosso `GOOGLE_CLIENT_ID`. Sem conta com aquele Google nem com o
  e-mail dele: 404 (o cadastro continua pelo formulario, que pede CPF/CNPJ e CEP).

```jsonc
// POST /api/auth/login  (tipo opcional; se vier, a conta precisa ser desse tipo)
{ "email": "marina@teste.com", "senha": "senha1234" }

// 200
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiraEm": "2026-11-09T22:00:00Z",   // UTC
  "usuario": { "tipo": "candidato", "id": 3, "nome": "Marina" }
}

// PUT /api/auth/senha  (senha nova: 8 a 100 caracteres)
{ "senhaAtual": "senha1234", "novaSenha": "outraSenha9" }

// POST /api/auth/esqueci-senha        -> 202
{ "email": "marina@teste.com" }

// POST /api/auth/redefinir-senha      -> 204
{ "token": "o-token-do-link", "novaSenha": "outraSenha9" }

// POST /api/auth/google               -> 200 (mesmo TokenDTO do login)
{ "idToken": "eyJhbGciOiJSUzI1NiIs..." }
```

`GET /api/auth/me` devolve o mesmo objeto de `usuario`.

### Candidatos — `/api/candidato`

| Verbo  | Rota                  | Acesso          | Corpo                | Sucesso                    | Erros                    |
| ------ | --------------------- | --------------- | -------------------- | -------------------------- | ------------------------ |
| GET    | `/api/candidato`      | Empresa         | —                    | 200 + lista `CandidatoDTO` | 401, 403                 |
| GET    | `/api/candidato/{id}` | Proprio/empresa | —                    | 200 + `CandidatoDTO`       | 401, 403, 404            |
| POST   | `/api/candidato`      | Publico         | `CandidatoCreateDTO` | 201 + `CandidatoDTO`       | 400, 409, 503            |
| PUT    | `/api/candidato/{id}` | Proprio         | `CandidatoCreateDTO` | 200 + `CandidatoDTO`       | 400, 401, 403, 404, 409, 503 |
| DELETE | `/api/candidato/{id}` | Proprio         | —                    | 204                        | 401, 403, 404            |

- **POST:** CPF com digitos verificadores validos, CPF unico entre candidatos, e-mail
  unico entre todas as contas (409 se uma empresa ja usa), CEP de Itajai/SC (ver [Endereco](#endereco-so-itajaisc)), senha
  obrigatoria.
- **PUT:** mesmas regras; o campo `senha` e ignorado (troca so por `PUT /api/auth/senha`).
- **DELETE:** apaga junto endereco, contato, curriculo, candidaturas e avaliacoes.

```jsonc
// POST /api/candidato
{
  "nome": "Marina",                    // 2 a 100
  "sobrenome": "Deschamps",            // 2 a 100
  "cpf": "529.982.247-25",
  "senha": "senha1234",                // 8 a 100
  "endereco": {
    "cep": "88301-001",                // obrigatorio, Itajai/SC
    "numero": "10",                    // obrigatorio, ate 6
    "rua": null,                       // vazio = vem do CEP
    "bairro": null,                    // vazio = vem do CEP
    "complemento": "Apto 2"
  },
  "contato": { "email": "marina@teste.com", "telefone": "47999990000" }
}

// 201 (CandidatoDTO)
{
  "id": 3, "nome": "Marina", "sobrenome": "Deschamps", "cpf": "52998224725",
  "endereco": { "cep": "88301001", "cidade": "Itajaí", "bairro": "Centro",
                "rua": "Rua Hercílio Luz", "numero": "10", "complemento": "Apto 2" },
  "contato": { "email": "marina@teste.com", "telefone": "47999990000" },
  "idCurriculo": null                  // null ate criar o curriculo
}
```

### Empresas — `/api/empresa`

| Verbo  | Rota                | Acesso  | Corpo              | Sucesso                  | Erros                        |
| ------ | ------------------- | ------- | ------------------ | ------------------------ | ---------------------------- |
| GET    | `/api/empresa`      | Logado  | —                  | 200 + lista `EmpresaDTO` | 401                          |
| GET    | `/api/empresa/{id}` | Logado  | —                  | 200 + `EmpresaDTO`       | 401, 404                     |
| POST   | `/api/empresa`      | Publico | `EmpresaCreateDTO` | 201 + `EmpresaDTO`       | 400, 409, 503                |
| PUT    | `/api/empresa/{id}` | Proprio | `EmpresaCreateDTO` | 200 + `EmpresaDTO`       | 400, 401, 403, 404, 409, 503 |
| DELETE | `/api/empresa/{id}` | Proprio | —                  | 204                      | 401, 403, 404                |

- **POST/PUT:** CNPJ com digitos verificadores validos (aceita o CNPJ alfanumerico),
  CNPJ unico entre empresas, e-mail unico entre todas as contas, CEP de Itajai/SC. O
  `senha` segue a mesma regra do candidato.
- **Perfil publico** (tudo opcional; no PUT, vazio ou ausente apaga): `razaoSocial` ate
  200, `ramoAtuacao` ate 100, `descricaoCurta` ate 500, `sobre` ate 5000, `site` ate 255
  (link http/https), `linkedin` ate 255 (link do linkedin.com).
- **`nota`** nao se edita: e a media das [avaliacoes](#avaliacoes--apiempresaidempresaavaliacoes).
- **DELETE:** apaga junto endereco, contato, vagas, as candidaturas delas, as
  avaliacoes e as imagens.

```jsonc
// POST /api/empresa
{
  "nome": "Porto Pesca Ltda.",         // 2 a 100
  "cnpj": "11.222.333/0001-81",
  "senha": "senha1234",
  "endereco": { "cep": "88302-000", "numero": "20" },
  "contato": { "email": "rh@portopesca.com", "telefone": "4733330000" },
  "razaoSocial": "Porto Pesca Comercio de Pescados Ltda.",
  "ramoAtuacao": "Pesca e logistica portuaria",
  "descricaoCurta": "Distribuidora de pescados no porto de Itajai.",
  "sobre": "Texto longo da pagina da empresa...",
  "site": "https://portopesca.com.br",
  "linkedin": "https://www.linkedin.com/company/portopesca"
}

// 201 (EmpresaDTO)
{
  "id": 1, "nome": "Porto Pesca Ltda.", "cnpj": "11222333000181",
  "nota": null,                        // media 1..5; null sem avaliacao
  "totalAvaliacoes": 0,
  "razaoSocial": "...", "ramoAtuacao": "...", "descricaoCurta": "...",
  "sobre": "...", "site": "...", "linkedin": "...",
  "endereco": { ... }, "contato": { ... }
}
```

### Imagens da empresa — `/api/empresa/{idEmpresa}/imagens`

Uma logo, um banner e uma galeria de fotos. **Ver e baixar e publico** (a `url` serve
direto num `<img src>`); enviar, reordenar e apagar, so a propria empresa.

| Verbo  | Rota                                             | Acesso  | Corpo                              | Sucesso                    | Erros                   |
| ------ | ------------------------------------------------ | ------- | ---------------------------------- | -------------------------- | ----------------------- |
| GET    | `/api/empresa/{id}/imagens`                      | Publico | —                                  | 200 + lista `EmpresaImagemDTO` | 404                 |
| PUT    | `/api/empresa/{id}/imagens/{logo\|banner}`       | Proprio | multipart `arquivo`                | 200 + `EmpresaImagemDTO`   | 400, 401, 403, 404, 413 |
| GET    | `/api/empresa/{id}/imagens/{logo\|banner}`       | Publico | —                                  | 200 + a imagem (binario)   | 404                     |
| DELETE | `/api/empresa/{id}/imagens/{logo\|banner}`       | Proprio | —                                  | 204                        | 401, 403, 404           |
| POST   | `/api/empresa/{id}/imagens/galeria`              | Proprio | multipart `arquivo` + `ordem` opcional | 201 + `EmpresaImagemDTO` | 400, 401, 403, 404, 413 |
| GET    | `/api/empresa/{id}/imagens/galeria/{idImagem}`   | Publico | —                                  | 200 + a imagem (binario)   | 404                     |
| PATCH  | `/api/empresa/{id}/imagens/galeria/{idImagem}`   | Proprio | `{ ordem }` (0 a 1000)             | 204                        | 400, 401, 403, 404      |
| DELETE | `/api/empresa/{id}/imagens/galeria/{idImagem}`   | Proprio | —                                  | 204                        | 401, 403, 404           |

- **Formatos:** JPG, PNG ou WebP, conferido pelo conteudo do arquivo (nao pelo nome).
- **Tamanhos:** logo ate 2 MB, banner ate 5 MB, cada foto da galeria ate 5 MB; no
  maximo 10 fotos na galeria.
- **Logo e banner:** enviar de novo substitui (indice unico parcial no banco).
- **Galeria:** sem `ordem`, a foto vai para o fim. A lista vem: logo, banner, galeria
  pela `ordem`.
- O download sai com `Cache-Control: public, max-age=300`. A `url` da lista leva
  `?v=` com a data do envio, entao muda quando a imagem e trocada.

```jsonc
// GET /api/empresa/1/imagens
[
  { "id": 4, "tipo": "logo", "ordem": 0, "nome": "logo.png", "contentType": "image/png",
    "tamanho": 18211, "enviadoEm": "2026-11-20T17:30:02.123456Z",
    "url": "/api/empresa/1/imagens/logo?v=639000000000000000" },
  { "id": 7, "tipo": "galeria", "ordem": 0, "nome": "galpao.jpg", "contentType": "image/jpeg",
    "tamanho": 402113, "enviadoEm": "2026-11-20T17:31:40Z",
    "url": "/api/empresa/1/imagens/galeria/7?v=639000000000000000" }
]
```

```bash
curl -X PUT http://localhost:8080/api/empresa/1/imagens/logo \
  -H "Authorization: Bearer $TOKEN" -F "arquivo=@logo.png"
curl -X POST http://localhost:8080/api/empresa/1/imagens/galeria \
  -H "Authorization: Bearer $TOKEN" -F "arquivo=@galpao.jpg" -F "ordem=0"
```

### Vagas — `/api/vaga`

| Verbo  | Rota                    | Acesso       | Corpo           | Sucesso                     | Erros                   |
| ------ | ----------------------- | ------------ | --------------- | --------------------------- | ----------------------- |
| GET    | `/api/vaga`             | Publico      | query (filtros) | 200 + pagina de `VagaDTO`   | 400                     |
| GET    | `/api/vaga/{id}`        | Publico      | —               | 200 + `VagaDTO`             | 404                     |
| POST   | `/api/vaga`             | Empresa dona | `VagaCreateDTO` | 201 + `VagaDTO`             | 400, 401, 403, 404      |
| PUT    | `/api/vaga/{id}`        | Empresa dona | `VagaCreateDTO` | 200 + `VagaDTO`             | 400, 401, 403, 404      |
| PATCH  | `/api/vaga/{id}/status` | Empresa dona | `{ status }`    | 200 + `VagaDTO`             | 400, 401, 403, 404      |
| DELETE | `/api/vaga/{id}`        | Empresa dona | —               | 204                         | 401, 403, 404           |

- **POST/PUT:** o `idEmpresa` do corpo tem que ser a empresa logada, e a vaga nao troca
  de empresa no PUT. Sem `bairro`, vale o da empresa. A vaga nasce `aberta`; o status
  so muda pelo PATCH.
- **Remuneracao** (CHECK `ck_vaga_remuneracao` no banco), uma de tres formas:

  | Forma      | `salario` | `salarioMinimo` | `salarioMaximo`  |
  | ---------- | --------- | --------------- | ---------------- |
  | A combinar | null      | null            | null             |
  | Fixo       | > 0       | null            | null             |
  | Faixa      | null      | > 0             | >= minimo        |

  Valores ate 99.999.999,99. Qualquer outra combinacao: 400.
- **`modalidade`** opcional: `presencial`, `hibrido` ou `remoto`.
- **`palavrasChave`** opcional: ate 20, cada uma ate 100 caracteres. Espacos nas pontas
  saem, vazias sao ignoradas e repetidas (sem diferenciar maiuscula) entram uma vez.
  O PUT substitui a lista (ausente = sem palavras).
- **PATCH status:** `aberta` ou `encerrada`. Vaga encerrada continua visivel, mas nao
  aceita candidatura.
- **DELETE:** apaga junto as candidaturas.

Filtros do `GET /api/vaga` (todos opcionais):

| Parametro       | Filtra                                                           |
| --------------- | ---------------------------------------------------------------- |
| `cargo`         | busca parcial, sem diferenciar maiuscula                         |
| `bairro`        | bairro exato, sem diferenciar maiuscula                          |
| `salarioMinimo` | paga pelo menos isso: fixo `>=` valor, ou maximo da faixa `>=` valor ("a combinar" fica de fora) |
| `salarioMaximo` | fixo `<=` valor, ou minimo da faixa `<=` valor                   |
| `tipoContrato`  | um dos valores do enum                                           |
| `jornada`       | um dos valores do enum                                           |
| `modalidade`    | um dos valores do enum                                           |
| `palavraChave`  | palavra-chave exata da vaga, sem diferenciar maiuscula           |
| `status`        | sem informar: so `aberta`, menos quando filtra por `idEmpresa` (ai vem todas) |
| `idEmpresa`     | vagas de uma empresa (o painel da empresa)                       |
| `pagina`        | comeca em 1 (padrao 1)                                           |
| `tamanhoPagina` | 1 a 100 (padrao 20)                                              |

Ordem: das mais novas para as mais antigas.

```jsonc
// GET /api/vaga?cargo=solda&bairro=Centro&tipoContrato=clt&pagina=1&tamanhoPagina=20
{
  "itens": [ { /* VagaDTO */ } ],
  "pagina": 1, "tamanhoPagina": 20, "total": 37, "totalPaginas": 2
}

// POST /api/vaga
{
  "idEmpresa": 1,
  "cargo": "Soldador",                 // 2 a 45
  "descricao": "Solda MIG em estruturas navais.",
  "salario": null,                     // fixo; ou use a faixa abaixo
  "salarioMinimo": 2800.00,            // faixa: minimo e maximo juntos
  "salarioMaximo": 3600.00,
  "modalidade": "presencial",          // opcional
  "palavrasChave": ["solda", "MIG", "naval"],   // opcional, ate 20
  "tipoContrato": "clt",               // obrigatorio
  "jornada": "escala",                 // obrigatorio
  "bairro": null,                      // opcional; null = bairro da empresa
  "requisitos": "NR-34, 2 anos de experiencia"   // opcional, ate 4000
}

// VagaDTO
{
  "id": 5, "idEmpresa": 1, "nomeEmpresa": "Porto Pesca Ltda.",
  "cargo": "Soldador", "descricao": "...",
  "salario": null, "salarioMinimo": 2800.00, "salarioMaximo": 3600.00,
  "modalidade": "presencial", "palavrasChave": ["solda", "MIG", "naval"],
  "tipoContrato": "clt", "jornada": "escala", "bairro": "Fazenda",
  "requisitos": "...", "status": "aberta", "dataPublicacao": "2026-11-09T14:29:56"
}

// PATCH /api/vaga/5/status
{ "status": "encerrada" }
```

### Candidaturas

A candidatura nao tem id proprio: e o par (vaga, candidato).

| Verbo  | Rota                                                      | Acesso             | Corpo             | Sucesso                      | Erros                   |
| ------ | --------------------------------------------------------- | ------------------ | ----------------- | ---------------------------- | ----------------------- |
| GET    | `/api/vaga/{idVaga}/candidaturas`                         | Empresa dona       | query `status`    | 200 + lista `CandidaturaDTO` | 400, 401, 403, 404      |
| GET    | `/api/candidato/{idCandidato}/candidaturas`               | Proprio            | —                 | 200 + lista `CandidaturaDTO` | 401, 403, 404           |
| POST   | `/api/vaga/{idVaga}/candidaturas`                         | Proprio candidato  | `{ idCandidato }` | 201 + `CandidaturaDTO`       | 400, 401, 403, 404, 409 |
| PATCH  | `/api/vaga/{idVaga}/candidaturas/{idCandidato}/status`    | Empresa dona       | `{ status }`      | 200 + `CandidaturaDTO`       | 400, 401, 403, 404      |
| DELETE | `/api/vaga/{idVaga}/candidaturas/{idCandidato}`           | Candidato ou empresa dona | —          | 204                          | 401, 403, 404           |

- **POST:** o `idCandidato` do corpo tem que ser o candidato logado. Recusa (400) se o
  candidato nao tem curriculo ou se a vaga esta encerrada; 409 se ja se candidatou.
- **Status:** nasce `recebida`; a empresa dona muda para `emAnalise`, `aprovada` ou
  `recusada`. `dataStatus` guarda a ultima mudanca.
- **GET da vaga:** `?status=emAnalise` filtra; sem o parametro, vem todas.

```jsonc
// POST /api/vaga/5/candidaturas
{ "idCandidato": 3 }

// CandidaturaDTO
{
  "idVaga": 5, "cargo": "Soldador", "idEmpresa": 1, "nomeEmpresa": "Porto Pesca Ltda.",
  "idCandidato": 3, "nomeCandidato": "Marina Deschamps",
  "dataCandidatura": "2026-11-09T14:30:00",
  "status": "emAnalise",
  "dataStatus": "2026-11-10T09:12:00",  // null enquanto "recebida"
  "statusVaga": "aberta"
}

// PATCH /api/vaga/5/candidaturas/3/status
{ "status": "aprovada" }
```

### Avaliacoes — `/api/empresa/{idEmpresa}/avaliacoes`

Cada candidato avalia cada empresa uma vez; avaliar de novo substitui.

| Verbo  | Rota                                              | Acesso            | Corpo                | Sucesso                     | Erros              |
| ------ | ------------------------------------------------- | ----------------- | -------------------- | --------------------------- | ------------------ |
| GET    | `/api/empresa/{idEmpresa}/avaliacoes`             | Logado            | —                    | 200 + lista `AvaliacaoDTO`  | 401, 404           |
| PUT    | `/api/empresa/{idEmpresa}/avaliacoes/{idCandidato}` | Proprio candidato | `AvaliacaoCreateDTO` | 200 + `AvaliacaoDTO`      | 400, 401, 403, 404 |
| DELETE | `/api/empresa/{idEmpresa}/avaliacoes/{idCandidato}` | Proprio candidato | —                  | 204                         | 401, 403, 404      |

- **PUT:** so quem ja se candidatou a alguma vaga da empresa (senao 400). Nota de 1 a 5.
- A `nota` e o `totalAvaliacoes` da empresa sao recalculados a cada PUT e DELETE.
- A lista mostra so o primeiro nome do candidato, das mais novas para as mais antigas.

```jsonc
// PUT /api/empresa/1/avaliacoes/3
{ "nota": 4, "comentario": "Bom ambiente, pagamento em dia." }   // comentario ate 500

// AvaliacaoDTO
{ "idEmpresa": 1, "idCandidato": 3, "nomeCandidato": "Marina",
  "nota": 4, "comentario": "...", "data": "2026-11-09T15:00:00" }
```

### Curriculo — `/api/curriculo`

Um curriculo por candidato. Para os itens (experiencias, habilidades, formacoes, cursos,
projetos, idiomas) e arquivos, o acesso de escrita e sempre do **candidato dono**; ver e baixar,
**Proprio/empresa**.

| Verbo  | Rota                                         | Acesso          | Corpo                | Sucesso                  | Erros              |
| ------ | -------------------------------------------- | --------------- | -------------------- | ------------------------ | ------------------ |
| GET    | `/api/curriculo/{id}`                        | Proprio/empresa | —                    | 200 + `CurriculoDTO`     | 401, 403, 404      |
| GET    | `/api/candidato/{idCandidato}/curriculo`     | Proprio/empresa | —                    | 200 + `CurriculoDTO`     | 401, 403, 404 (sem curriculo) |
| POST   | `/api/curriculo`                             | Proprio         | `{ idCandidato }`    | 201 + `CurriculoDTO`     | 401, 403, 404, 409 (ja tem) |
| PUT    | `/api/curriculo/{id}`                        | Proprio         | `{ tituloProfissional, resumo, portfolio }` | 200 + `CurriculoDTO`  | 400, 401, 403, 404 |
| DELETE | `/api/curriculo/{id}`                        | Proprio         | —                    | 204                      | 401, 403, 404      |
| POST   | `/api/curriculo/{id}/experiencias`           | Proprio         | `ExperienciaCreateDTO` | 201 + `ExperienciaDTO` | 400, 401, 403, 404 |
| PUT    | `/api/curriculo/{id}/experiencias/{idExp}`   | Proprio         | `ExperienciaCreateDTO` | 200 + `ExperienciaDTO` | 400, 401, 403, 404 |
| DELETE | `/api/curriculo/{id}/experiencias/{idExp}`   | Proprio         | —                    | 204                      | 401, 403, 404      |
| POST   | `/api/curriculo/{id}/habilidades`            | Proprio         | `{ descricao }`      | 201 + `HabilidadeDTO`    | 400, 401, 403, 404, 409 (repetida) |
| DELETE | `/api/curriculo/{id}/habilidades/{idHab}`    | Proprio         | —                    | 204                      | 401, 403, 404      |
| POST   | `/api/curriculo/{id}/formacoes`              | Proprio         | `FormacaoCreateDTO`  | 201 + `FormacaoDTO`      | 400, 401, 403, 404 |
| PUT    | `/api/curriculo/{id}/formacoes/{idForm}`     | Proprio         | `FormacaoCreateDTO`  | 200 + `FormacaoDTO`      | 400, 401, 403, 404 |
| DELETE | `/api/curriculo/{id}/formacoes/{idForm}`     | Proprio         | —                    | 204                      | 401, 403, 404      |
| POST   | `/api/curriculo/{id}/cursos`                 | Proprio         | `CursoCreateDTO`     | 201 + `CursoDTO`         | 400, 401, 403, 404 |
| PUT    | `/api/curriculo/{id}/cursos/{idCurso}`       | Proprio         | `CursoCreateDTO`     | 200 + `CursoDTO`         | 400, 401, 403, 404 |
| DELETE | `/api/curriculo/{id}/cursos/{idCurso}`       | Proprio         | —                    | 204                      | 401, 403, 404      |
| POST   | `/api/curriculo/{id}/projetos`               | Proprio         | `ProjetoCreateDTO`   | 201 + `ProjetoDTO`       | 400, 401, 403, 404 |
| PUT    | `/api/curriculo/{id}/projetos/{idProjeto}`   | Proprio         | `ProjetoCreateDTO`   | 200 + `ProjetoDTO`       | 400, 401, 403, 404 |
| DELETE | `/api/curriculo/{id}/projetos/{idProjeto}`   | Proprio         | —                    | 204                      | 401, 403, 404      |
| POST   | `/api/curriculo/{id}/idiomas`                | Proprio         | `IdiomaCreateDTO`    | 201 + `IdiomaDTO`        | 400, 401, 403, 404, 409 (repetido) |
| PUT    | `/api/curriculo/{id}/idiomas/{idIdioma}`     | Proprio         | `IdiomaCreateDTO`    | 200 + `IdiomaDTO`        | 400, 401, 403, 404, 409 (repetido) |
| DELETE | `/api/curriculo/{id}/idiomas/{idIdioma}`     | Proprio         | —                    | 204                      | 401, 403, 404      |
| PUT    | `/api/curriculo/{id}/arquivos/{pdf\|foto}`   | Proprio         | multipart `arquivo`  | 200 + `ArquivoDTO`       | 400, 401, 403, 404, 413 |
| GET    | `/api/curriculo/{id}/arquivos/{pdf\|foto}`   | Proprio/empresa | —                    | 200 + o arquivo (binario) | 401, 403, 404     |
| DELETE | `/api/curriculo/{id}/arquivos/{pdf\|foto}`   | Proprio         | —                    | 204                      | 401, 403, 404      |

Regras dos itens:

- **Experiencia:** cargo 2 a 45, `nomeEmpresa` ate 45, `descricao` ate 1000. Inicio e
  fim nao ficam no futuro; `dataFim` vazia = emprego atual.
- **Habilidade:** `descricao` 1 a 45, nao se repete no curriculo (sem diferenciar maiuscula).
- **Formacao:** instituicao 2 a 100, `curso` ate 100. Inicio nao fica no futuro; fim
  pode (previsao de conclusao), mas nao antes do inicio. `dataFim` vazia = em andamento.
- **Curso:** nome 2 a 100, `cargaHoraria` 1 a 10000, conclusao nao fica no futuro.
- **Projeto:** `titulo` 2 a 100, `descricao` ate 1000.
- **Idioma:** `idioma` 2 a 45, `nivel` obrigatorio. Nao se repete no curriculo (sem
  diferenciar maiuscula); 409 no POST e no PUT.
- **Titulo, resumo e portfolio** (`PUT /api/curriculo/{id}`): `tituloProfissional` ate
  100 ("Soldador naval"), resumo ate 2000, portfolio ate 255 e precisa ser link
  `http://` ou `https://`. O PUT substitui os tres: o que vier vazio ou nao vier e
  apagado.
- **Arquivos:** um PDF (ate 5 MB) e uma foto JPG, PNG ou WebP (ate 2 MB); enviar de novo
  substitui. O tipo e conferido pelo conteudo, nao pelo nome. O download sai `inline`
  com o `Content-Type` real. Como exige token, a tela baixa com `fetch` e monta um
  blob; `<img src>` direto nao funciona. Os arquivos ficam no banco (`bytea`).

```jsonc
// POST /api/curriculo/2/experiencias
{ "cargo": "Soldador", "nomeEmpresa": "Estaleiro Sul",
  "dataInicio": "2021-03-01", "dataFim": null,
  "descricao": "Solda MIG em cascos de aco." }

// POST /api/curriculo/2/habilidades
{ "descricao": "Solda MIG" }

// POST /api/curriculo/2/formacoes
{ "instituicao": "SENAI Itajai", "curso": "Tecnico em Soldagem", "nivel": "tecnico",
  "dataInicio": "2025-02-01", "dataFim": "2027-12-01" }

// POST /api/curriculo/2/cursos
{ "nome": "NR-34 Trabalho a quente", "instituicao": "SENAI",
  "cargaHoraria": 8, "dataConclusao": "2024-05-10" }

// POST /api/curriculo/2/projetos
{ "titulo": "Reforma de rebocador", "descricao": "Soldagem do casco e da casa de maquinas." }

// POST /api/curriculo/2/idiomas
{ "idioma": "Ingles", "nivel": "intermediario" }

// PUT /api/curriculo/2
{ "tituloProfissional": "Soldador naval",
  "resumo": "Soldador com 5 anos de experiencia em estaleiro.",
  "portfolio": "https://github.com/marina" }

// CurriculoDTO
{
  "id": 2, "idCandidato": 3, "nomeCandidato": "Marina Deschamps",
  "tituloProfissional": "Soldador naval",
  "resumo": "...", "portfolio": "https://github.com/marina",
  "experiencias": [ { "id": 1, "cargo": "Soldador", "nomeEmpresa": "Estaleiro Sul",
                      "dataInicio": "2021-03-01", "dataFim": null, "descricao": "..." } ],
  "habilidades":  [ { "id": 1, "descricao": "Solda MIG" } ],
  "formacoes":    [ { "id": 1, "instituicao": "SENAI Itajai", "curso": "Tecnico em Soldagem",
                      "nivel": "tecnico", "dataInicio": "2025-02-01", "dataFim": "2027-12-01" } ],
  "cursos":       [ { "id": 1, "nome": "NR-34 Trabalho a quente", "instituicao": "SENAI",
                      "cargaHoraria": 8, "dataConclusao": "2024-05-10" } ],
  "projetos":     [ { "id": 1, "titulo": "Reforma de rebocador", "descricao": "..." } ],
  "idiomas":      [ { "id": 1, "idioma": "Ingles", "nivel": "intermediario" } ],
  "arquivos":     [ { "tipo": "pdf", "nome": "curriculo.pdf", "contentType": "application/pdf",
                      "tamanho": 48213, "enviadoEm": "2026-11-09T14:30:02" } ]
}
```

Upload pelo terminal:

```bash
curl -X PUT http://localhost:8080/api/curriculo/2/arquivos/pdf \
  -H "Authorization: Bearer $TOKEN" -F "arquivo=@curriculo.pdf"
```

## Rodando

```bash
dotnet run
```

Dev: `http://localhost:8080` — OpenAPI em `/openapi/v1.json`, requests de teste no
`Itajobs-API.http`.

### Em producao / com muitos usuarios

Rode em Release, nao com o `dotnet run` de desenvolvimento:

```bash
dotnet publish -c Release -o publish
ASPNETCORE_ENVIRONMENT=Production dotnet publish/Itajobs-API.dll --urls http://0.0.0.0:8080
```

O que ja esta configurado para aguentar carga:

- **Tudo assincrono:** toda rota que vai no banco ou no ViaCEP usa `async/await`; a thread
  fica livre enquanto espera.
- **Pool de conexoes** (`DB_POOL_MAX`, padrao 80) abaixo do `max_connections` do Postgres
  (100). Com o pool cheio, a request espera a vez em vez de falhar (`DB_POOL_ESPERA_SEGUNDOS`).
  Para subir o pool, suba antes o `max_connections` no `postgresql.conf`.
- **Pool de DbContext** (`AddDbContextPool`).
- **Indice em `contato.email`:** login e cadastro nao varrem a tabela inteira.
- **Cache de CEP** em memoria (24 h): o ViaCEP bloqueia quem consulta demais.
- **Compressao** Brotli/Gzip nas respostas.
- **Fila de conexoes** do Kestrel em 4096 (o padrao, 512, recusa conexoes em pico).
  No Windows 10/11 o proprio sistema limita essa fila; em Windows Server ou Linux o
  valor vale inteiro.
- **SQL fora do log** (`Microsoft.EntityFrameworkCore.Database.Command: Warning` no
  `appsettings.json`). Para ver as queries ao depurar, troque para `Information`.

Teste de carga (API e gerador de carga na mesma maquina, 16 nucleos, Postgres local):

| Cenario                                                         | Requests | Sucesso  | Mediana | p95    |
| --------------------------------------------------------------- | -------- | -------- | ------- | ------ |
| 10.000 usuarios logados, 10 acoes cada, pausa de 1 a 5 s        | 100.000  | 99,998 % | 6 ms    | 1,9 s* |
| 10.000 usuarios no mesmo milissegundo, sem pausa (pior caso)     | 50.000   | 99,98 %  | 2,8 s   | 5,0 s  |
| 500 usuarios sem pausa (vazao maxima)                            | 50.000   | 100 %    | 97 ms   | 158 ms |

\* o p95 vem do primeiro clique, quando os 10 mil chegam juntos. Vazao maxima medida:
~5.400 requests/s.
