using ApiDespachoAmbulancias.Dominio.Entidades;

namespace ApiDespachoAmbulancias.Aplicacao.Contratos;

/// <summary>
/// Orquestra o Heap Máximo para ordenar a fila de ocorrências ativas.
/// </summary>
public interface IServicoFilaPrioridade
{
    IReadOnlyList<Ocorrencia> OrdenarPorPrioridade(IReadOnlyList<Ocorrencia> ocorrencias);

    int ObterPosicao(IReadOnlyList<Ocorrencia> filaOrdenada, Guid id);

    void RecalcularScores(IEnumerable<Ocorrencia> ocorrencias);
}
