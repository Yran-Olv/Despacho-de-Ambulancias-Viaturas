namespace ApiDespachoAmbulancias.Dominio.Enumeradores;

/// <summary>
/// Nível de gravidade clínica da ocorrência (1 = menor, 5 = emergência máxima).
/// </summary>
public enum GravidadeEmergencia
{
    Baixa = 1,
    Moderada = 2,
    Alta = 3,
    Critica = 4,
    EmergenciaMaxima = 5
}
