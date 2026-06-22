using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.ObjetosValor;

namespace ApiDespachoAmbulancias.Dominio.Contratos;

/// <summary>
/// Contrato da regra de prioridade e desempate entre ocorrências.
/// </summary>
public interface ICalculadorPrioridade
{
    ResultadoPrioridade Calcular(Ocorrencia ocorrencia);

    decimal CalcularScore(Ocorrencia ocorrencia);

    int Comparar(Ocorrencia a, Ocorrencia b);
}
