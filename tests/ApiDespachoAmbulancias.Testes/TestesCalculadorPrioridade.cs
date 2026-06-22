using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using ApiDespachoAmbulancias.Dominio.Servicos;

namespace ApiDespachoAmbulancias.Testes;

public class TestesCalculadorPrioridade
{
    private readonly CalculadorPrioridade _calculador = new();

    [Fact]
    public void CalcularScore_EmergenciaMaxima_DeveTerMaiorScoreQueBaixa()
    {
        var critica = CriarOcorrencia(GravidadeEmergencia.EmergenciaMaxima, TipoEmergencia.Cardiaca, 1, 0);
        var baixa = CriarOcorrencia(GravidadeEmergencia.Baixa, TipoEmergencia.Clinica, 1, 0);

        Assert.True(_calculador.CalcularScore(critica) > _calculador.CalcularScore(baixa));
    }

    [Fact]
    public void Calcular_ParadaCardiaca_DeveSomar113Pontos()
    {
        var o = CriarOcorrencia(GravidadeEmergencia.EmergenciaMaxima, TipoEmergencia.Cardiaca, 1, 0);
        var resultado = _calculador.Calcular(o);

        Assert.Equal(100, resultado.PontosGravidade);
        Assert.Equal(10, resultado.PontosTipo);
        Assert.Equal(3, resultado.PontosPacientes);
        Assert.Equal(0, resultado.PontosEspera);
        Assert.Equal(113, resultado.ScoreTotal);
    }

    [Fact]
    public void Comparar_MultiplasVitimas_DeveTerPrioridadeSobreTraumaSimples()
    {
        var multiplas = CriarOcorrencia(GravidadeEmergencia.Critica, TipoEmergencia.MultiplasVitimas, 4, 10);
        var trauma = CriarOcorrencia(GravidadeEmergencia.Alta, TipoEmergencia.Trauma, 1, 10);

        Assert.True(_calculador.Comparar(multiplas, trauma) > 0);
    }

    [Fact]
    public void Comparar_EmpateDeScore_UsaFifoPorDataCriacao()
    {
        var maisAntiga = CriarOcorrencia(GravidadeEmergencia.Alta, TipoEmergencia.Trauma, 1, 5);
        maisAntiga.DataCriacao = DateTime.UtcNow.AddMinutes(-30);

        var maisRecente = CriarOcorrencia(GravidadeEmergencia.Alta, TipoEmergencia.Trauma, 1, 5);
        maisRecente.DataCriacao = DateTime.UtcNow.AddMinutes(-10);

        Assert.Equal(_calculador.CalcularScore(maisAntiga), _calculador.CalcularScore(maisRecente));
        Assert.True(_calculador.Comparar(maisAntiga, maisRecente) > 0);
    }

    [Fact]
    public void CalcularScore_TempoEspera_AumentaPrioridade()
    {
        var recente = CriarOcorrencia(GravidadeEmergencia.Moderada, TipoEmergencia.Clinica, 1, 5);
        var esperando = CriarOcorrencia(GravidadeEmergencia.Moderada, TipoEmergencia.Clinica, 1, 15);

        Assert.True(_calculador.CalcularScore(esperando) > _calculador.CalcularScore(recente));
    }

    [Fact]
    public void AtualizarTempoEspera_DataCriacaoUnspecified_CalculaMinutosCorretamente()
    {
        var o = CriarOcorrencia(GravidadeEmergencia.Alta, TipoEmergencia.Trauma, 1, 0);
        o.DataCriacao = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(-5), DateTimeKind.Unspecified);

        o.AtualizarTempoEspera();

        Assert.Equal(5, o.TempoEsperaMinutos);
    }

    private static Ocorrencia CriarOcorrencia(
        GravidadeEmergencia gravidade,
        TipoEmergencia tipo,
        int pacientes,
        int tempoEspera)
    {
        return new Ocorrencia
        {
            Cpf = "12345678901",
            Descricao = "Teste",
            Endereco = "Endereço teste",
            Gravidade = gravidade,
            TipoEmergencia = tipo,
            PacientesEnvolvidos = pacientes,
            TempoEsperaMinutos = tempoEspera,
            DataCriacao = DateTime.UtcNow
        };
    }
}
