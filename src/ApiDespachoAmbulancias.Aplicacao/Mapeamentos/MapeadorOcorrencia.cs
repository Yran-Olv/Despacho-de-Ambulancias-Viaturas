using ApiDespachoAmbulancias.Aplicacao.Dtos;
using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.ObjetosValor;

namespace ApiDespachoAmbulancias.Aplicacao.Mapeamentos;

public static class MapeadorOcorrencia
{
    public static OcorrenciaResponse ParaResponse(
        Ocorrencia ocorrencia,
        int posicaoFila,
        ResultadoPrioridade prioridade) =>
        new(
            ocorrencia.Id,
            ocorrencia.Cpf,
            ocorrencia.Descricao,
            ocorrencia.Endereco,
            ocorrencia.Gravidade,
            ocorrencia.TipoEmergencia,
            ocorrencia.PacientesEnvolvidos,
            ocorrencia.TempoEsperaMinutos,
            prioridade.ScoreTotal,
            prioridade.PontosGravidade,
            prioridade.PontosTipo,
            prioridade.PontosPacientes,
            prioridade.PontosEspera,
            prioridade.Resumo,
            posicaoFila,
            ocorrencia.Status,
            ocorrencia.DataCriacao,
            ocorrencia.DataAtualizacao,
            ocorrencia.DataDespacho,
            ocorrencia.DataConclusao,
            ocorrencia.VeiculoId,
            ocorrencia.Veiculo?.Identificacao);

    public static Ocorrencia CriarEntidade(CriarOcorrenciaRequest request, string cpfNormalizado) =>
        new()
        {
            Cpf = cpfNormalizado,
            Descricao = request.Descricao.Trim(),
            Endereco = request.Endereco.Trim(),
            Gravidade = request.Gravidade,
            TipoEmergencia = request.TipoEmergencia,
            PacientesEnvolvidos = request.PacientesEnvolvidos
        };

    public static void AplicarAtualizacao(Ocorrencia ocorrencia, AtualizarOcorrenciaRequest request, string cpfNormalizado)
    {
        ocorrencia.Cpf = cpfNormalizado;
        ocorrencia.Descricao = request.Descricao.Trim();
        ocorrencia.Endereco = request.Endereco.Trim();
        ocorrencia.Gravidade = request.Gravidade;
        ocorrencia.TipoEmergencia = request.TipoEmergencia;
        ocorrencia.PacientesEnvolvidos = request.PacientesEnvolvidos;
        ocorrencia.DataAtualizacao = DateTime.UtcNow;
    }
}
