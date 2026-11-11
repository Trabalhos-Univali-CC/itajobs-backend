using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Controllers;
using Itajobs_API.src.Data;
using Itajobs_API.src.Repositories;
using Itajobs_API.src.Services;
using Itajobs_API.src.Utils;

var builder = WebApplication.CreateBuilder(args);
var env = EnvLoader.Load("itajobs.env");

// Fila de conexoes TCP aguardando aceite. O padrao (512) estoura quando milhares de
// usuarios conectam no mesmo instante; ai o sistema recusa a conexao.
builder.WebHost.UseSockets(options => options.Backlog = 4096);

// Controllers e OpenAPI. O filtro transforma RegraNegocioException em 400/401/403/404/409/503.
// Enums saem e entram como texto camelCase ("emAnalise"); numero nao e aceito.
builder.Services.AddControllers(options => options.Filters.Add<RegraNegocioFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));
builder.Services.AddOpenApi();

// IP real do cliente quando a request chega pelo proxy do Next (rewrites do
// next.config.ts), que manda X-Forwarded-For. So proxies locais sao confiaveis.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor);

// Limite de tentativas de login por IP, contra senha no chute. Passou, 429.
var tentativasLogin = int.Parse(EnvLoader.Get(env, "LOGIN_TENTATIVAS_POR_MINUTO", "5"));
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(AuthController.LimiteLogin, context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = tentativasLogin,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)espera.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new { mensagem = "Muitas tentativas de login. Espere um minuto e tente de novo." }, token);
    };
});

// CORS: o frontend (Next.js) roda em outra porta. A origem vem de FRONTEND_URL no
// itajobs.env; sem ela, vale o padrao do `npm run dev` (porta 8080).
var frontendUrl = EnvLoader.Get(env, "FRONTEND_URL", "http://localhost:8080");
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(frontendUrl.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Banco: a connection string vem de Core/Secrets/itajobs.env, nao do
// appsettings.json - senha nao entra em arquivo versionado.
// Pool de DbContext: reaproveita as instancias em vez de criar uma por request.
// O tamanho do pool de conexoes fica na connection string (DB_POOL_MAX).
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(EnvLoader.BuildConnectionString()));

// Cache em memoria: guarda as consultas de CEP (ver ViaCepService).
builder.Services.AddMemoryCache();

// Compressao das respostas JSON (listas de vagas, curriculos). Menos banda por request.
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

// JWT: o segredo vem de JWT_SECRET no itajobs.env. Sem ele a API nem sobe.
var jwt = JwtOptions.Criar(
    EnvLoader.Get(env, "JWT_SECRET"),
    TimeSpan.FromHours(int.Parse(EnvLoader.Get(env, "JWT_EXPIRA_HORAS", "8"))));
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mantem "sub"/"role"/"name" como vieram, sem renomear para as claims do .NET.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = jwt.ParametrosDeValidacao();

        // 401 e 403 no mesmo formato dos outros erros da API: { "mensagem": "..." }.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(new { mensagem = "Faca login para continuar." });
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(new { mensagem = "Voce nao tem permissao para acessar este recurso." });
            }
        };
    });

// Toda rota exige login, menos as marcadas com [AllowAnonymous] (cadastro, login,
// listagem de vagas). Rota nova ja nasce protegida.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// ViaCEP para conferir se o endereco e de Itajai.
builder.Services.AddHttpClient<ICepService, ViaCepService>(http =>
{
    http.BaseAddress = new Uri("https://viacep.com.br/");
    http.Timeout = TimeSpan.FromSeconds(5);
});

// Uma linha por camada, para cada entidade nova.
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<EmpresaService>();

builder.Services.AddScoped<ICandidatoRepository, CandidatoRepository>();
builder.Services.AddScoped<CandidatoService>();

builder.Services.AddScoped<IVagaRepository, VagaRepository>();
builder.Services.AddScoped<VagaService>();

builder.Services.AddScoped<ICurriculoRepository, CurriculoRepository>();
builder.Services.AddScoped<CurriculoService>();

builder.Services.AddScoped<ICandidaturaRepository, CandidaturaRepository>();
builder.Services.AddScoped<CandidaturaService>();

builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
builder.Services.AddScoped<AvaliacaoService>();

var app = builder.Build();

// Primeiro de tudo: o resto do pipeline (rate limiting inclusive) ja ve o IP real.
app.UseForwardedHeaders();

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

// Antes do redirect de HTTPS: o preflight (OPTIONS) do navegador nao segue redirect.
app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
