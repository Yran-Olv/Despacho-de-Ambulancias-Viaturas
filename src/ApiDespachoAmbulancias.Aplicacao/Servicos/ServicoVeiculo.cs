using ApiDespachoAmbulancias.Aplicacao.Dtos;
using ApiDespachoAmbulancias.Aplicacao.Contratos;
using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;

namespace ApiDespachoAmbulancias.Aplicacao.Servicos;

public sealed class ServicoVeiculo : IServicoVeiculo
{
    private readonly IRepositorioVeiculo _repository;

    public ServicoVeiculo(IRepositorioVeiculo repository)
    {
        _repository = repository;
    }

    public async Task<VeiculoResponse> CriarAsync(
        CriarVeiculoRequest request,
        CancellationToken cancellationToken = default)
    {
        var identificacao = request.Identificacao.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(identificacao))
            throw new ArgumentException("Identificação do veículo é obrigatória.");

        if (await _repository.IdentificacaoExisteAsync(identificacao, cancellationToken))
            throw new ArgumentException($"Já existe um veículo com identificação '{identificacao}'.");

        var veiculo = new Veiculo
        {
            Identificacao = identificacao,
            Tipo = request.Tipo,
            Status = StatusVeiculo.Disponivel
        };

        await _repository.AdicionarAsync(veiculo, cancellationToken);
        return Mapear(veiculo);
    }

    public async Task<IReadOnlyList<VeiculoResponse>> ListarTodosAsync(CancellationToken cancellationToken = default)
    {
        var veiculos = await _repository.ListarTodosAsync(cancellationToken);
        return veiculos.Select(Mapear).ToList();
    }

    public async Task<IReadOnlyList<VeiculoResponse>> ListarDisponiveisAsync(CancellationToken cancellationToken = default)
    {
        var veiculos = await _repository.ListarDisponiveisAsync(cancellationToken);
        return veiculos.Select(Mapear).ToList();
    }

    public async Task<bool> ExcluirLogicamenteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var veiculo = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo is null || veiculo.Status == StatusVeiculo.Inativo)
            return false;

        if (veiculo.Status == StatusVeiculo.EmAtendimento)
            throw new ArgumentException("Não é possível excluir viatura em atendimento. Conclua a ocorrência primeiro.");

        veiculo.Status = StatusVeiculo.Inativo;
        veiculo.DataAtualizacao = DateTime.UtcNow;

        await _repository.AtualizarAsync(veiculo, cancellationToken);
        return true;
    }

    private static VeiculoResponse Mapear(Veiculo veiculo) =>
        new(
            veiculo.Id,
            veiculo.Identificacao,
            veiculo.Tipo,
            veiculo.Status,
            veiculo.DataCriacao,
            veiculo.DataAtualizacao);
}
