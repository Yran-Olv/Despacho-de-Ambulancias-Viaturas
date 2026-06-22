using System.Net;
using System.Text.Json;

namespace ApiDespachoAmbulancias.Api.Middleware;

public sealed class MiddlewareTratamentoExcecoes
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MiddlewareTratamentoExcecoes> _logger;

    public MiddlewareTratamentoExcecoes(RequestDelegate next, ILogger<MiddlewareTratamentoExcecoes> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Requisição inválida.");
            await EscreverErroAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno.");
            await EscreverErroAsync(context, HttpStatusCode.InternalServerError, "Erro interno do servidor.");
        }
    }

    private static async Task EscreverErroAsync(HttpContext context, HttpStatusCode status, string mensagem)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { mensagem }));
    }
}
