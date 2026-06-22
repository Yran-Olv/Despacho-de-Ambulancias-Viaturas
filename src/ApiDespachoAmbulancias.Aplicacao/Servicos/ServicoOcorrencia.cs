using ApiDespachoAmbulancias.Aplicacao.Dtos;
using ApiDespachoAmbulancias.Aplicacao.Contratos;
using ApiDespachoAmbulancias.Aplicacao.Mapeamentos;
using ApiDespachoAmbulancias.Aplicacao.Validadores;
using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using ApiDespachoAmbulancias.Dominio.Contratos;

namespace ApiDespachoAmbulancias.Aplicacao.Servicos;

/// <summary>
/// Orquestra CRUD e fila de prioridade usando Heap Máximo (SRP + DIP).
/// </summary>
public sealed class ServicoOcorrencia : IServicoOcorrencia
{
    private readonly IRepositorioOcorrencia _repository;
    private readonly IRepositorioVeiculo _veiculoRepository;
    private readonly IServicoFilaPrioridade _servicoFilaPrioridade;
    private readonly ICalculadorPrioridade _calculadorPrioridade;

    public ServicoOcorrencia(
        IRepositorioOcorrencia repository,
        IRepositorioVeiculo veiculoRepository,
        IServicoFilaPrioridade servicoFilaPrioridade,
        ICalculadorPrioridade calculadorPrioridade)
    {
        _repository = repository;
        _veiculoRepository = veiculoRepository;
        _servicoFilaPrioridade = servicoFilaPrioridade;
        _calculadorPrioridade = calculadorPrioridade;
    }

    public async Task<OcorrenciaResponse> CriarAsync(
        CriarOcorrenciaRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidadorRequisicaoOcorrencia.Validar(request.Cpf, request.Descricao, request.PacientesEnvolvidos);

        var ocorrencia = MapeadorOcorrencia.CriarEntidade(
            request,
            ValidadorRequisicaoOcorrencia.NormalizarCpf(request.Cpf));

        _servicoFilaPrioridade.RecalcularScores([ocorrencia]);

        await _repository.AdicionarAsync(ocorrencia, cancellationToken);

        var fila = await MontarFilaOrdenadaAsync(cancellationToken);
        return Mapear(ocorrencia, _servicoFilaPrioridade.ObterPosicao(fila, ocorrencia.Id));
    }

    public async Task<OcorrenciaResponse?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ocorrencia = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (ocorrencia is null || ocorrencia.Status != StatusOcorrencia.Ativo)
            return null;

        _servicoFilaPrioridade.RecalcularScores([ocorrencia]);

        var fila = await MontarFilaOrdenadaAsync(cancellationToken);
        return Mapear(ocorrencia, _servicoFilaPrioridade.ObterPosicao(fila, ocorrencia.Id));
    }

    public async Task<PaginacaoResponse<OcorrenciaResponse>> ListarPaginadoAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanho = Math.Clamp(tamanho, 1, 100);

        var filaOrdenada = await MontarFilaOrdenadaAsync(cancellationToken);
        var total = filaOrdenada.Count;
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanho);

        var paginaItens = filaOrdenada
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select((o, index) => Mapear(o, ((pagina - 1) * tamanho) + index + 1))
            .ToList();

        return new PaginacaoResponse<OcorrenciaResponse>(
            paginaItens,
            pagina,
            tamanho,
            total,
            totalPaginas);
    }

    public async Task<IReadOnlyList<OcorrenciaResponse>> BuscarAsync(
        string? cpf,
        string? descricao,
        CancellationToken cancellationToken = default)
    {
        ValidadorRequisicaoOcorrencia.ValidarBusca(cpf, descricao);

        var resultados = await _repository.BuscarAsync(
            string.IsNullOrWhiteSpace(cpf) ? null : ValidadorRequisicaoOcorrencia.NormalizarCpf(cpf),
            descricao?.Trim(),
            cancellationToken);

        var fila = await MontarFilaOrdenadaAsync(cancellationToken);

        return resultados
            .Where(o => o.Status == StatusOcorrencia.Ativo)
            .Select(o =>
            {
                _servicoFilaPrioridade.RecalcularScores([o]);
                return Mapear(o, _servicoFilaPrioridade.ObterPosicao(fila, o.Id));
            })
            .OrderByDescending(o => o.ScorePrioridade)
            .ThenBy(o => o.DataCriacao)
            .ToList();
    }

    public async Task<OcorrenciaResponse?> AtualizarAsync(
        Guid id,
        AtualizarOcorrenciaRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidadorRequisicaoOcorrencia.Validar(request.Cpf, request.Descricao, request.PacientesEnvolvidos);

        var ocorrencia = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (ocorrencia is null || ocorrencia.Status != StatusOcorrencia.Ativo)
            return null;

        MapeadorOcorrencia.AplicarAtualizacao(
            ocorrencia,
            request,
            ValidadorRequisicaoOcorrencia.NormalizarCpf(request.Cpf));

        _servicoFilaPrioridade.RecalcularScores([ocorrencia]);

        await _repository.AtualizarAsync(ocorrencia, cancellationToken);

        var fila = await MontarFilaOrdenadaAsync(cancellationToken);
        return Mapear(ocorrencia, _servicoFilaPrioridade.ObterPosicao(fila, ocorrencia.Id));
    }

    public async Task<bool> ExcluirLogicamenteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ocorrencia = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (ocorrencia is null || ocorrencia.Status != StatusOcorrencia.Ativo)
            return false;

        ocorrencia.Status = StatusOcorrencia.Inativo;
        ocorrencia.DataAtualizacao = DateTime.UtcNow;

        await _repository.AtualizarAsync(ocorrencia, cancellationToken);
        return true;
    }

    public async Task<OcorrenciaResponse?> ObterProximaDespachoAsync(CancellationToken cancellationToken = default)
    {
        var fila = await MontarFilaOrdenadaAsync(cancellationToken);
        if (fila.Count == 0)
            return null;

        return Mapear(fila[0], 1);
    }

    public async Task<OcorrenciaResponse?> DespacharAmbulanciaAsync(
        Guid id,
        Guid veiculoId,
        CancellationToken cancellationToken = default)
    {
        var ocorrencia = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (ocorrencia is null || ocorrencia.Status != StatusOcorrencia.Ativo)
            return null;

        var veiculo = await _veiculoRepository.ObterPorIdAsync(veiculoId, cancellationToken);
        if (veiculo is null || veiculo.Status != StatusVeiculo.Disponivel)
            return null;

        ocorrencia.Status = StatusOcorrencia.EmDeslocamento;
        ocorrencia.VeiculoId = veiculoId;
        ocorrencia.Veiculo = veiculo;
        ocorrencia.DataDespacho = DateTime.UtcNow;
        ocorrencia.DataAtualizacao = DateTime.UtcNow;

        veiculo.Status = StatusVeiculo.EmAtendimento;
        veiculo.DataAtualizacao = DateTime.UtcNow;

        await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
        await _repository.AtualizarAsync(ocorrencia, cancellationToken);
        return Mapear(ocorrencia, 0);
    }

    public async Task<OcorrenciaResponse?> ConcluirAtendimentoAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var ocorrencia = await _repository.ObterPorIdAsync(id, cancellationToken);
        if (ocorrencia is null || ocorrencia.Status != StatusOcorrencia.EmDeslocamento)
            return null;

        ocorrencia.Status = StatusOcorrencia.Concluida;
        ocorrencia.DataConclusao = DateTime.UtcNow;
        ocorrencia.DataAtualizacao = DateTime.UtcNow;

        if (ocorrencia.VeiculoId is Guid veiculoId)
        {
            var veiculo = await _veiculoRepository.ObterPorIdAsync(veiculoId, cancellationToken);
            if (veiculo is not null)
            {
                veiculo.Status = StatusVeiculo.Disponivel;
                veiculo.DataAtualizacao = DateTime.UtcNow;
                await _veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
            }
        }

        await _repository.AtualizarAsync(ocorrencia, cancellationToken);
        return Mapear(ocorrencia, 0);
    }

    public async Task<IReadOnlyList<OcorrenciaResponse>> ListarACaminhoAsync(
        CancellationToken cancellationToken = default)
    {
        var emDeslocamento = await _repository.ListarEmDeslocamentoAsync(cancellationToken);

        return emDeslocamento
            .Select(o => Mapear(o, 0))
            .ToList();
    }

    public async Task<PaginacaoResponse<OcorrenciaResponse>> ListarConcluidasPaginadoAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanho = Math.Clamp(tamanho, 1, 100);

        var (itens, total) = await _repository.ListarConcluidasPaginadasAsync(pagina, tamanho, cancellationToken);
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanho);

        var respostas = itens
            .Select(o => Mapear(o, 0))
            .ToList();

        return new PaginacaoResponse<OcorrenciaResponse>(
            respostas,
            pagina,
            tamanho,
            total,
            totalPaginas);
    }

    private async Task<IReadOnlyList<Ocorrencia>> MontarFilaOrdenadaAsync(CancellationToken cancellationToken)
    {
        var ativas = await _repository.ListarAtivasAsync(cancellationToken);
        return _servicoFilaPrioridade.OrdenarPorPrioridade(ativas);
    }

    private OcorrenciaResponse Mapear(Ocorrencia ocorrencia, int posicaoFila)
    {
        var prioridade = _calculadorPrioridade.Calcular(ocorrencia);
        return MapeadorOcorrencia.ParaResponse(ocorrencia, posicaoFila, prioridade);
    }
}
