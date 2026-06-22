using ApiDespachoAmbulancias.Aplicacao.Dtos;
using ApiDespachoAmbulancias.Aplicacao.Contratos;
using Microsoft.AspNetCore.Mvc;

namespace ApiDespachoAmbulancias.Api.Controladores;

[ApiController]
[Route("ocorrencias")]
[Produces("application/json")]
public class ControladorOcorrencias : ControllerBase
{
    private readonly IServicoOcorrencia _service;

    public ControladorOcorrencias(IServicoOcorrencia service)
    {
        _service = service;
    }

    /// <summary>Cadastra uma nova ocorrência na fila de despacho.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OcorrenciaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OcorrenciaResponse>> Criar(
        [FromBody] CriarOcorrenciaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    /// <summary>Busca uma ocorrência específica pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OcorrenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OcorrenciaResponse>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.ObterPorIdAsync(id, cancellationToken);
        return result is null ? NotFound(new { mensagem = "Ocorrência não encontrada ou inativa." }) : Ok(result);
    }

    /// <summary>Lista ocorrências ativas paginadas, ordenadas por prioridade (Heap).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<OcorrenciaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginacaoResponse<OcorrenciaResponse>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListarPaginadoAsync(page, size, cancellationToken);
        return Ok(result);
    }

    /// <summary>Busca ocorrências por CPF e/ou descrição.</summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IEnumerable<OcorrenciaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<OcorrenciaResponse>>> Buscar(
        [FromQuery] string? cpf,
        [FromQuery] string? descricao,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.BuscarAsync(cpf, descricao, cancellationToken);
        return Ok(result);
    }

    /// <summary>Atualiza dados da ocorrência e recalcula prioridade na fila.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OcorrenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OcorrenciaResponse>> Atualizar(
        Guid id,
        [FromBody] AtualizarOcorrenciaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.AtualizarAsync(id, request, cancellationToken);
        return result is null
            ? NotFound(new { mensagem = "Ocorrência não encontrada ou inativa." })
            : Ok(result);
    }

    /// <summary>Exclusão lógica: altera status para Inativo (não remove do banco).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var removido = await _service.ExcluirLogicamenteAsync(id, cancellationToken);
        return removido ? NoContent() : NotFound(new { mensagem = "Ocorrência não encontrada ou já inativa." });
    }

    /// <summary>Retorna a próxima ocorrência a ser despachada (topo do Heap).</summary>
    [HttpGet("proxima")]
    [ProducesResponseType(typeof(OcorrenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OcorrenciaResponse>> Proxima(CancellationToken cancellationToken)
    {
        var result = await _service.ObterProximaDespachoAsync(cancellationToken);
        return result is null ? NotFound(new { mensagem = "Nenhuma ocorrência ativa na fila." }) : Ok(result);
    }

    /// <summary>Lista ocorrências com ambulância a caminho.</summary>
    [HttpGet("a-caminho")]
    [ProducesResponseType(typeof(IEnumerable<OcorrenciaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OcorrenciaResponse>>> ListarACaminho(CancellationToken cancellationToken)
    {
        var result = await _service.ListarACaminhoAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Lista ocorrências concluídas (histórico de baixas).</summary>
    [HttpGet("concluidas")]
    [ProducesResponseType(typeof(PaginacaoResponse<OcorrenciaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginacaoResponse<OcorrenciaResponse>>> ListarConcluidas(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListarConcluidasPaginadoAsync(page, size, cancellationToken);
        return Ok(result);
    }

    /// <summary>Despacha ambulância — vincula veículo e status passa para EmDeslocamento.</summary>
    [HttpPost("{id:guid}/despachar")]
    [ProducesResponseType(typeof(OcorrenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OcorrenciaResponse>> Despachar(
        Guid id,
        [FromBody] DespacharOcorrenciaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.DespacharAmbulanciaAsync(id, request.VeiculoId, cancellationToken);
        return result is null
            ? NotFound(new { mensagem = "Ocorrência não está na fila ou veículo indisponível." })
            : Ok(result);
    }

    /// <summary>Baixa no hospital — status passa para Concluida.</summary>
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType(typeof(OcorrenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OcorrenciaResponse>> Concluir(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.ConcluirAtendimentoAsync(id, cancellationToken);
        return result is null
            ? NotFound(new { mensagem = "Ocorrência não encontrada ou ambulância não está a caminho." })
            : Ok(result);
    }
}
