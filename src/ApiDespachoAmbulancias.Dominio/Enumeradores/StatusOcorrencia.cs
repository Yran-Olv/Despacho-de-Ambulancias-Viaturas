namespace ApiDespachoAmbulancias.Dominio.Enumeradores;

/// <summary>
/// Ciclo de vida da ocorrência no despacho SAMU.
/// </summary>
public enum StatusOcorrencia
{
    /// <summary>Aguardando na fila de prioridade.</summary>
    Ativo = 1,

    /// <summary>Ambulância despachada e a caminho.</summary>
    EmDeslocamento = 2,

    /// <summary>Atendimento concluído — paciente entregue no hospital (baixa).</summary>
    Concluida = 3,

    /// <summary>Cancelada / exclusão lógica.</summary>
    Inativo = 4
}
