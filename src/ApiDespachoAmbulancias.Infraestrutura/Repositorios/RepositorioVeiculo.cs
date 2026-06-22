using ApiDespachoAmbulancias.Aplicacao.Contratos;
using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using ApiDespachoAmbulancias.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace ApiDespachoAmbulancias.Infraestrutura.Repositorios;

public sealed class RepositorioVeiculo : IRepositorioVeiculo
{
    private readonly ContextoBancoDados _context;

    public RepositorioVeiculo(ContextoBancoDados context)
    {
        _context = context;
    }

    public async Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Veiculos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Veiculo>> ListarTodosAsync(CancellationToken cancellationToken = default) =>
        await _context.Veiculos
            .Where(v => v.Status != StatusVeiculo.Inativo)
            .OrderBy(v => v.Identificacao)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Veiculo>> ListarDisponiveisAsync(CancellationToken cancellationToken = default) =>
        await _context.Veiculos
            .Where(v => v.Status == StatusVeiculo.Disponivel)
            .OrderBy(v => v.Identificacao)
            .ToListAsync(cancellationToken);

    public async Task<bool> IdentificacaoExisteAsync(string identificacao, CancellationToken cancellationToken = default) =>
        await _context.Veiculos.AnyAsync(
            v => v.Status != StatusVeiculo.Inativo &&
                 v.Identificacao.ToUpper() == identificacao.ToUpper(),
            cancellationToken);

    public async Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        await _context.Veiculos.AddAsync(veiculo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        _context.Veiculos.Update(veiculo);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
