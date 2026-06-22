using ApiDespachoAmbulancias.Aplicacao.Dtos;
using ApiDespachoAmbulancias.Aplicacao.Contratos;
using Microsoft.AspNetCore.Mvc;

namespace ApiDespachoAmbulancias.Api.Controladores;

[ApiController]
[Route("veiculos")]
[Produces("application/json")]
public class ControladorVeiculos : ControllerBase
{
    private readonly IServicoVeiculo _service;

    public ControladorVeiculos(IServicoVeiculo service)
    {
        _service = service;
    }

    /// <summary>Cadastra uma nova viatura/ambulância.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VeiculoResponse>> Criar(
        [FromBody] CriarVeiculoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id = result.Id }, result);
    }

    /// <summary>Lista veículos ativos cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VeiculoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VeiculoResponse>>> Listar(CancellationToken cancellationToken)
    {
        var result = await _service.ListarTodosAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Lista veículos disponíveis para despacho.</summary>
    [HttpGet("disponiveis")]
    [ProducesResponseType(typeof(IEnumerable<VeiculoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VeiculoResponse>>> ListarDisponiveis(CancellationToken cancellationToken)
    {
        var result = await _service.ListarDisponiveisAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Exclusão lógica — status passa para Inativo.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var removido = await _service.ExcluirLogicamenteAsync(id, cancellationToken);
            return removido
                ? NoContent()
                : NotFound(new { mensagem = "Viatura não encontrada ou já inativa." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
