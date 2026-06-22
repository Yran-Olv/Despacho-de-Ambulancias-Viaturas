using ApiDespachoAmbulancias.Dominio.Enumeradores;

namespace ApiDespachoAmbulancias.Aplicacao.Dtos;

public record CriarVeiculoRequest(
    string Identificacao,
    TipoVeiculo Tipo
);

public record VeiculoResponse(
    Guid Id,
    string Identificacao,
    TipoVeiculo Tipo,
    StatusVeiculo Status,
    DateTime DataCriacao,
    DateTime? DataAtualizacao
);

public record DespacharOcorrenciaRequest(Guid VeiculoId);
