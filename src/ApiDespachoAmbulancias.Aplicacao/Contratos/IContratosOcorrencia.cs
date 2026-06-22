using ApiDespachoAmbulancias.Aplicacao.Dtos;
using ApiDespachoAmbulancias.Dominio.Entidades;

namespace ApiDespachoAmbulancias.Aplicacao.Contratos;

public interface IRepositorioOcorrencia
{
    Task<Ocorrencia?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ocorrencia>> ListarAtivasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ocorrencia>> ListarEmDeslocamentoAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Ocorrencia> Itens, int Total)> ListarConcluidasPaginadasAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Ocorrencia> Itens, int Total)> ListarAtivasPaginadasAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ocorrencia>> BuscarAsync(
        string? cpf,
        string? descricao,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default);

    Task AtualizarAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IServicoOcorrencia
{
    Task<OcorrenciaResponse> CriarAsync(CriarOcorrenciaRequest request, CancellationToken cancellationToken = default);

    Task<OcorrenciaResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaginacaoResponse<OcorrenciaResponse>> ListarPaginadoAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OcorrenciaResponse>> BuscarAsync(
        string? cpf,
        string? descricao,
        CancellationToken cancellationToken = default);

    Task<OcorrenciaResponse?> AtualizarAsync(
        Guid id,
        AtualizarOcorrenciaRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirLogicamenteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OcorrenciaResponse?> ObterProximaDespachoAsync(CancellationToken cancellationToken = default);

    Task<OcorrenciaResponse?> DespacharAmbulanciaAsync(
        Guid id,
        Guid veiculoId,
        CancellationToken cancellationToken = default);

    Task<OcorrenciaResponse?> ConcluirAtendimentoAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OcorrenciaResponse>> ListarACaminhoAsync(CancellationToken cancellationToken = default);

    Task<PaginacaoResponse<OcorrenciaResponse>> ListarConcluidasPaginadoAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default);
}
