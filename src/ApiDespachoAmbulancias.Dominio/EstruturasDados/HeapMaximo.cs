using ApiDespachoAmbulancias.Dominio.Contratos;

namespace ApiDespachoAmbulancias.Dominio.EstruturasDados;

/// <summary>
/// Heap Máximo (Max-Heap) genérico para fila de prioridade.
///
/// Justificativa da escolha do Heap:
/// - Inserção e remoção do elemento de maior prioridade em O(log n).
/// - Consulta do próximo despacho (topo) em O(1).
/// - Ideal para cenários de pronto-socorro onde novas ocorrências chegam
///   continuamente e a viatura deve atender sempre a mais crítica.
/// - Estrutura compacta em memória (array), sem overhead de nós como em árvores balanceadas.
/// </summary>
public sealed class HeapMaximo<T> : IHeapPrioridade<T>
{
    private readonly List<T> _heap = [];
    private readonly Comparison<T> _comparador;

    public HeapMaximo(Comparison<T> comparador)
    {
        _comparador = comparador ?? throw new ArgumentNullException(nameof(comparador));
    }

    public int Quantidade => _heap.Count;

    public void Inserir(T item)
    {
        _heap.Add(item);
        SubirNoHeap(_heap.Count - 1);
    }

    public T? ExtrairMaximo()
    {
        if (_heap.Count == 0)
            return default;

        var maximo = _heap[0];
        var ultimo = _heap[^1];
        _heap.RemoveAt(_heap.Count - 1);

        if (_heap.Count > 0)
        {
            _heap[0] = ultimo;
            DescerNoHeap(0);
        }

        return maximo;
    }

    public T? ConsultarMaximo() => _heap.Count == 0 ? default : _heap[0];

    public void Reconstruir(IEnumerable<T> itens)
    {
        _heap.Clear();
        _heap.AddRange(itens);
        for (var i = ObterIndicePai(_heap.Count - 1); i >= 0; i--)
            DescerNoHeap(i);
    }

    public IReadOnlyList<T> ObterOrdenados()
    {
        var copia = new HeapMaximo<T>(_comparador);
        copia.Reconstruir(_heap);

        var ordenados = new List<T>(_heap.Count);
        while (copia.Quantidade > 0)
        {
            var item = copia.ExtrairMaximo();
            if (item is not null)
                ordenados.Add(item);
        }

        return ordenados;
    }

    private void SubirNoHeap(int indice)
    {
        while (indice > 0)
        {
            var pai = ObterIndicePai(indice);
            if (_comparador(_heap[indice], _heap[pai]) <= 0)
                break;

            Trocar(indice, pai);
            indice = pai;
        }
    }

    private void DescerNoHeap(int indice)
    {
        while (true)
        {
            var maior = indice;
            var esquerdo = ObterFilhoEsquerdo(indice);
            var direito = ObterFilhoDireito(indice);

            if (esquerdo < _heap.Count && _comparador(_heap[esquerdo], _heap[maior]) > 0)
                maior = esquerdo;

            if (direito < _heap.Count && _comparador(_heap[direito], _heap[maior]) > 0)
                maior = direito;

            if (maior == indice)
                break;

            Trocar(indice, maior);
            indice = maior;
        }
    }

    private static int ObterIndicePai(int indice) => (indice - 1) / 2;
    private static int ObterFilhoEsquerdo(int indice) => (2 * indice) + 1;
    private static int ObterFilhoDireito(int indice) => (2 * indice) + 2;

    private void Trocar(int a, int b) => (_heap[a], _heap[b]) = (_heap[b], _heap[a]);
}
