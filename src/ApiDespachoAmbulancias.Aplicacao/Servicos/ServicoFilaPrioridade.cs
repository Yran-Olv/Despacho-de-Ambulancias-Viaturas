using ApiDespachoAmbulancias.Aplicacao.Contratos;
using ApiDespachoAmbulancias.Dominio.EstruturasDados;
using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Contratos;

namespace ApiDespachoAmbulancias.Aplicacao.Servicos;

/// <summary>
/// Monta a fila de despacho usando Heap Máximo em memória.
/// </summary>
public sealed class ServicoFilaPrioridade : IServicoFilaPrioridade
{
    private readonly ICalculadorPrioridade _calculadorPrioridade;

    public ServicoFilaPrioridade(ICalculadorPrioridade calculadorPrioridade)
    {
        _calculadorPrioridade = calculadorPrioridade;
    }

    public IReadOnlyList<Ocorrencia> OrdenarPorPrioridade(IReadOnlyList<Ocorrencia> ocorrencias)
    {
        RecalcularScores(ocorrencias);

        var heap = new HeapMaximo<Ocorrencia>((a, b) => _calculadorPrioridade.Comparar(a, b));
        heap.Reconstruir(ocorrencias);

        return heap.ObterOrdenados();
    }

    public int ObterPosicao(IReadOnlyList<Ocorrencia> filaOrdenada, Guid id)
    {
        for (var i = 0; i < filaOrdenada.Count; i++)
        {
            if (filaOrdenada[i].Id == id)
                return i + 1;
        }

        return 0;
    }

    public void RecalcularScores(IEnumerable<Ocorrencia> ocorrencias)
    {
        foreach (var ocorrencia in ocorrencias)
        {
            ocorrencia.AtualizarTempoEspera();
            ocorrencia.ScorePrioridade = _calculadorPrioridade.CalcularScore(ocorrencia);
        }
    }
}
