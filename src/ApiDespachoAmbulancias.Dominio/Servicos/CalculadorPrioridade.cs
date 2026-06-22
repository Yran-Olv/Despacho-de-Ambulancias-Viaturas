using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using ApiDespachoAmbulancias.Dominio.Contratos;
using ApiDespachoAmbulancias.Dominio.ObjetosValor;

namespace ApiDespachoAmbulancias.Dominio.Servicos;

/// <summary>
/// Regra de prioridade em pontos simples (soma transparente).
///
/// Score = PontosGravidade + PontosTipo + PontosPacientes + PontosEspera
///
/// Tabela de gravidade:
///   Baixa=20 | Moderada=40 | Alta=60 | Critica=80 | EmergenciaMaxima=100
///
/// Tabela de tipo:
///   Clinica=2 | Trauma=4 | Queimadura=6 | Obstetrica=8 | Cardiaca=10 | MultiplasVitimas=12
///
/// Pacientes: 3 pontos por pessoa (máx. 10 pessoas = 30 pts)
/// Espera:    1 ponto por minuto (máx. 30 min contabilizados)
///
/// Desempate (score igual): gravidade → tipo → pacientes → espera → FIFO
/// </summary>
public sealed class CalculadorPrioridade : ICalculadorPrioridade
{
    private const int PontosPorPaciente = 3;
    private const int MaxPacientes = 10;
    private const int MaxMinutosEspera = 30;

    public ResultadoPrioridade Calcular(Ocorrencia ocorrencia)
    {
        ArgumentNullException.ThrowIfNull(ocorrencia);

        var ptsGravidade = PontosGravidade.Obter(ocorrencia.Gravidade);
        var ptsTipo = PontosTipoEmergencia.Obter(ocorrencia.TipoEmergencia);
        var ptsPacientes = Math.Clamp(ocorrencia.PacientesEnvolvidos, 1, MaxPacientes) * PontosPorPaciente;
        var ptsEspera = Math.Clamp(ocorrencia.TempoEsperaMinutos, 0, MaxMinutosEspera);

        var total = ptsGravidade + ptsTipo + ptsPacientes + ptsEspera;

        return new ResultadoPrioridade(total, ptsGravidade, ptsTipo, ptsPacientes, ptsEspera);
    }

    public decimal CalcularScore(Ocorrencia ocorrencia) => Calcular(ocorrencia).ScoreTotal;

    public int Comparar(Ocorrencia a, Ocorrencia b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var scoreA = CalcularScore(a);
        var scoreB = CalcularScore(b);

        if (scoreA != scoreB)
            return scoreA.CompareTo(scoreB);

        if (a.Gravidade != b.Gravidade)
            return ((int)a.Gravidade).CompareTo((int)b.Gravidade);

        if (a.TipoEmergencia != b.TipoEmergencia)
            return ((int)a.TipoEmergencia).CompareTo((int)b.TipoEmergencia);

        if (a.PacientesEnvolvidos != b.PacientesEnvolvidos)
            return a.PacientesEnvolvidos.CompareTo(b.PacientesEnvolvidos);

        if (a.TempoEsperaMinutos != b.TempoEsperaMinutos)
            return a.TempoEsperaMinutos.CompareTo(b.TempoEsperaMinutos);

        return b.DataCriacao.CompareTo(a.DataCriacao);
    }
}
