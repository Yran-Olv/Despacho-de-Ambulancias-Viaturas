namespace ApiDespachoAmbulancias.Aplicacao.Validadores;

public static class ValidadorRequisicaoOcorrencia
{
    public static void Validar(string cpf, string descricao, int pacientesEnvolvidos)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF é obrigatório.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.");

        if (pacientesEnvolvidos < 1)
            throw new ArgumentException("Pacientes envolvidos deve ser no mínimo 1.");
    }

    public static void ValidarBusca(string? cpf, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(cpf) && string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Informe CPF ou descrição para busca.");
    }

    public static string NormalizarCpf(string cpf) =>
        new(cpf.Where(char.IsDigit).ToArray());
}
