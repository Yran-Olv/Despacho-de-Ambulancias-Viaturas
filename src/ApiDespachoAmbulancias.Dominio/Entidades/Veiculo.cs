using ApiDespachoAmbulancias.Dominio.Enumeradores;

namespace ApiDespachoAmbulancias.Dominio.Entidades;

/// <summary>
/// Viatura/ambulância disponível para despacho.
/// </summary>
public class Veiculo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Identificacao { get; set; } = string.Empty;

    public TipoVeiculo Tipo { get; set; }

    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }
}
