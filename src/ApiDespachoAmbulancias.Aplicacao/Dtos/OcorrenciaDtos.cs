using ApiDespachoAmbulancias.Dominio.Enumeradores;

namespace ApiDespachoAmbulancias.Aplicacao.Dtos;

public record CriarOcorrenciaRequest(
    string Cpf,
    string Descricao,
    string Endereco,
    GravidadeEmergencia Gravidade,
    TipoEmergencia TipoEmergencia,
    int PacientesEnvolvidos = 1
);

public record AtualizarOcorrenciaRequest(
    string Cpf,
    string Descricao,
    string Endereco,
    GravidadeEmergencia Gravidade,
    TipoEmergencia TipoEmergencia,
    int PacientesEnvolvidos = 1
);

public record OcorrenciaResponse(
    Guid Id,
    string Cpf,
    string Descricao,
    string Endereco,
    GravidadeEmergencia Gravidade,
    TipoEmergencia TipoEmergencia,
    int PacientesEnvolvidos,
    int TempoEsperaMinutos,
    decimal ScorePrioridade,
    int PontosGravidade,
    int PontosTipo,
    int PontosPacientes,
    int PontosEspera,
    string ResumoPrioridade,
    int PosicaoFila,
    StatusOcorrencia Status,
    DateTime DataCriacao,
    DateTime? DataAtualizacao,
    DateTime? DataDespacho,
    DateTime? DataConclusao,
    Guid? VeiculoId,
    string? VeiculoIdentificacao
);

public record PaginacaoResponse<T>(
    IEnumerable<T> Itens,
    int Pagina,
    int Tamanho,
    int TotalItens,
    int TotalPaginas
);
