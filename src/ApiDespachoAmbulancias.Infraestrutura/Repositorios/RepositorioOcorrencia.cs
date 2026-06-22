using ApiDespachoAmbulancias.Aplicacao.Contratos;
using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using ApiDespachoAmbulancias.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ApiDespachoAmbulancias.Infraestrutura.Repositorios;

public sealed class RepositorioOcorrencia : IRepositorioOcorrencia
{
    private readonly ContextoBancoDados _context;

    public RepositorioOcorrencia(ContextoBancoDados context)
    {
        _context = context;
    }

    public async Task<Ocorrencia?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Ocorrencias
            .Include(o => o.Veiculo)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Ocorrencia>> ListarAtivasAsync(CancellationToken cancellationToken = default) =>
        await _context.Ocorrencias
            .Where(o => o.Status == StatusOcorrencia.Ativo)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ocorrencia>> ListarEmDeslocamentoAsync(CancellationToken cancellationToken = default) =>
        await _context.Ocorrencias
            .Include(o => o.Veiculo)
            .Where(o => o.Status == StatusOcorrencia.EmDeslocamento)
            .OrderByDescending(o => o.DataDespacho)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Ocorrencia> Itens, int Total)> ListarConcluidasPaginadasAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Ocorrencias
            .Include(o => o.Veiculo)
            .Where(o => o.Status == StatusOcorrencia.Concluida);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(o => o.DataConclusao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public async Task<(IReadOnlyList<Ocorrencia> Itens, int Total)> ListarAtivasPaginadasAsync(
        int pagina,
        int tamanho,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Ocorrencias.Where(o => o.Status == StatusOcorrencia.Ativo);
        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(o => o.ScorePrioridade)
            .ThenBy(o => o.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public async Task<IReadOnlyList<Ocorrencia>> BuscarAsync(
        string? cpf,
        string? descricao,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Ocorrencias.AsQueryable();

        if (!string.IsNullOrWhiteSpace(cpf))
            query = query.Where(o => o.Cpf.Contains(cpf));

        if (!string.IsNullOrWhiteSpace(descricao))
            query = query.Where(o => EF.Functions.ILike(o.Descricao, $"%{descricao}%"));

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default)
    {
        await _context.Ocorrencias.AddAsync(ocorrencia, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Ocorrencia ocorrencia, CancellationToken cancellationToken = default)
    {
        _context.Ocorrencias.Update(ocorrencia);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Ocorrencias.AnyAsync(o => o.Id == id, cancellationToken);
}
