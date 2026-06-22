using ApiDespachoAmbulancias.Aplicacao.Dtos;
using ApiDespachoAmbulancias.Dominio.Entidades;

namespace ApiDespachoAmbulancias.Aplicacao.Contratos;

public interface IRepositorioVeiculo
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Veiculo>> ListarTodosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Veiculo>> ListarDisponiveisAsync(CancellationToken cancellationToken = default);

    Task<bool> IdentificacaoExisteAsync(string identificacao, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);

    Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
}

public interface IServicoVeiculo
{
    Task<VeiculoResponse> CriarAsync(CriarVeiculoRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VeiculoResponse>> ListarTodosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VeiculoResponse>> ListarDisponiveisAsync(CancellationToken cancellationToken = default);

    Task<bool> ExcluirLogicamenteAsync(Guid id, CancellationToken cancellationToken = default);
}
