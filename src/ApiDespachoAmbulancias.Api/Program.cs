using ApiDespachoAmbulancias.Api.Middleware;
using ApiDespachoAmbulancias.Api.Swagger;
using ApiDespachoAmbulancias.Aplicacao;
using ApiDespachoAmbulancias.Infraestrutura;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

builder.Services.AdicionarAplicacao();
builder.Services.AdicionarInfraestrutura(connectionString);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Api de Despacho de Ambulâncias",
        Version = "v1",
        Description = """
            **Painel web:** [Abrir sistema de despacho](/)

            ---

            API REST de despacho de ambulâncias/viaturas com fila de prioridade baseada em Max-Heap.

            Regra de prioridade (soma de pontos):
            - Gravidade: 20 a 100 pts (Baixa → EmergenciaMaxima)
            - Tipo: 2 a 12 pts
            - Pacientes: 3 pts cada (máx. 10)
            - Espera: 1 pt/min (máx. 30)
            """
    });

    options.UseInlineDefinitionsForEnums();
    options.SchemaFilter<ExemplosEsquemaSwagger>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

await app.Services.InicializarBancoDadosAsync();

app.UseMiddleware<MiddlewareTratamentoExcecoes>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Despacho Ambulâncias v1");
    options.RoutePrefix = "swagger";
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
