using ApiDespachoAmbulancias.Aplicacao.Contratos;
using ApiDespachoAmbulancias.Aplicacao.Servicos;
using ApiDespachoAmbulancias.Dominio.Contratos;
using ApiDespachoAmbulancias.Dominio.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace ApiDespachoAmbulancias.Aplicacao;

public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        services.AddScoped<ICalculadorPrioridade, CalculadorPrioridade>();
        services.AddScoped<IServicoFilaPrioridade, ServicoFilaPrioridade>();
        services.AddScoped<IServicoOcorrencia, ServicoOcorrencia>();
        services.AddScoped<IServicoVeiculo, ServicoVeiculo>();
        return services;
    }
}
