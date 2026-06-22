namespace ApiDespachoAmbulancias.Dominio.ObjetosValor;

/// <summary>
/// Detalhamento transparente do score — cada critério em pontos separados.
/// </summary>
public sealed record ResultadoPrioridade(
    decimal ScoreTotal,
    int PontosGravidade,
    int PontosTipo,
    int PontosPacientes,
    int PontosEspera)
{
    public string Resumo =>
        $"Gravidade {PontosGravidade} + Tipo {PontosTipo} + Pacientes {PontosPacientes} + Espera {PontosEspera} = {ScoreTotal}";
}
