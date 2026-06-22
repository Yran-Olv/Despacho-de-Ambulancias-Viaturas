using ApiDespachoAmbulancias.Dominio.Enumeradores;

namespace ApiDespachoAmbulancias.Dominio.Entidades;

/// <summary>
/// Representa uma ocorrência na fila de despacho de ambulâncias/viaturas.
/// </summary>
public class Ocorrencia
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Cpf { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public GravidadeEmergencia Gravidade { get; set; }

    public TipoEmergencia TipoEmergencia { get; set; }

    /// <summary>Quantidade de pacientes envolvidos na ocorrência.</summary>
    public int PacientesEnvolvidos { get; set; } = 1;

    /// <summary>Tempo de espera em minutos desde o registro (atualizado a cada consulta).</summary>
    public int TempoEsperaMinutos { get; set; }

    /// <summary>Score calculado pela regra de prioridade (maior = mais urgente).</summary>
    public decimal ScorePrioridade { get; set; }

    public StatusOcorrencia Status { get; set; } = StatusOcorrencia.Ativo;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }

    /// <summary>Momento em que a ambulância foi despachada (a caminho).</summary>
    public DateTime? DataDespacho { get; set; }

    /// <summary>Momento da baixa — chegada ao hospital.</summary>
    public DateTime? DataConclusao { get; set; }

    public Guid? VeiculoId { get; set; }

    public Veiculo? Veiculo { get; set; }

    public void AtualizarTempoEspera()
    {
        var criacaoUtc = NormalizarUtc(DataCriacao);
        var minutos = (DateTime.UtcNow - criacaoUtc).TotalMinutes;
        TempoEsperaMinutos = Math.Max(0, (int)Math.Floor(minutos));
    }

    private static DateTime NormalizarUtc(DateTime data) =>
        data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };
}
