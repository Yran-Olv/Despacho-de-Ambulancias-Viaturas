using ApiDespachoAmbulancias.Dominio.EstruturasDados;
using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using ApiDespachoAmbulancias.Dominio.Servicos;

namespace ApiDespachoAmbulancias.Testes;

public class TestesHeapMaximo
{
    private readonly CalculadorPrioridade _calculador = new();

    [Fact]
    public void ExtrairMaximo_DeveRetornarItensEmOrdemDecrescenteDePrioridade()
    {
        var heap = new HeapMaximo<Ocorrencia>((a, b) => _calculador.Comparar(a, b));

        var baixa = Criar("11111111-1111-1111-1111-111111111111", GravidadeEmergencia.Baixa, 1);
        var alta = Criar("22222222-2222-2222-2222-222222222222", GravidadeEmergencia.EmergenciaMaxima, 1);
        var media = Criar("33333333-3333-3333-3333-333333333333", GravidadeEmergencia.Alta, 2);

        heap.Inserir(baixa);
        heap.Inserir(alta);
        heap.Inserir(media);

        var primeiro = heap.ExtrairMaximo();
        var segundo = heap.ExtrairMaximo();
        var terceiro = heap.ExtrairMaximo();

        Assert.Equal(GravidadeEmergencia.EmergenciaMaxima, primeiro!.Gravidade);
        Assert.Equal(GravidadeEmergencia.Alta, segundo!.Gravidade);
        Assert.Equal(GravidadeEmergencia.Baixa, terceiro!.Gravidade);
    }

    [Fact]
    public void ConsultarMaximo_NaoRemoveElemento()
    {
        var heap = new HeapMaximo<Ocorrencia>((a, b) => _calculador.Comparar(a, b));
        var item = Criar("44444444-4444-4444-4444-444444444444", GravidadeEmergencia.Critica, 3);
        heap.Inserir(item);

        var topo = heap.ConsultarMaximo();

        Assert.NotNull(topo);
        Assert.Equal(1, heap.Quantidade);
    }

    private static Ocorrencia Criar(string id, GravidadeEmergencia gravidade, int pacientes) =>
        new()
        {
            Id = Guid.Parse(id),
            Cpf = "12345678901",
            Descricao = "Teste heap",
            Endereco = "Rua A",
            Gravidade = gravidade,
            TipoEmergencia = TipoEmergencia.Trauma,
            PacientesEnvolvidos = pacientes,
            DataCriacao = DateTime.UtcNow
        };
}
