using Microsoft.EntityFrameworkCore;
using Itajobs_API.src.Data;
using Itajobs_API.src.Repositories;
using Itajobs_API.src.Services;
using Itajobs_API.src.Utils;

var builder = WebApplication.CreateBuilder(args);

// Controllers e OpenAPI.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Banco: a connection string vem de Core/Secrets/itajobs.env, nao do
// appsettings.json - senha nao entra em arquivo versionado.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(EnvLoader.BuildConnectionString()));

// Uma linha por camada, para cada entidade nova.
builder.Services.AddScoped<IExemploRepository, ExemploRepository>();
builder.Services.AddScoped<ExemploService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
