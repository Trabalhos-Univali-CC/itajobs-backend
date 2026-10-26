using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Controllers;
using Itajobs_API.src.Data;
using Itajobs_API.src.Repositories;
using Itajobs_API.src.Services;
using Itajobs_API.src.Utils;

var builder = WebApplication.CreateBuilder(args);

// Controllers e OpenAPI. O filtro transforma RegraNegocioException em 400/404/409.
builder.Services.AddControllers(options => options.Filters.Add<RegraNegocioFilter>());
builder.Services.AddOpenApi();

// Banco: a connection string vem de Core/Secrets/itajobs.env, nao do
// appsettings.json - senha nao entra em arquivo versionado.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(EnvLoader.BuildConnectionString()));

// Uma linha por camada, para cada entidade nova.
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
