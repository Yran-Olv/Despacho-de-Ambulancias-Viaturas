namespace ApiDespachoAmbulancias.Dominio.Contratos;

/// <summary>
/// Contrato para estrutura de fila baseada em Heap Máximo (Max-Heap).
/// </summary>
public interface IHeapPrioridade<T>
{
    int Quantidade { get; }

    void Inserir(T item);

    T? ExtrairMaximo();

    T? ConsultarMaximo();

    void Reconstruir(IEnumerable<T> itens);

    IReadOnlyList<T> ObterOrdenados();
}
