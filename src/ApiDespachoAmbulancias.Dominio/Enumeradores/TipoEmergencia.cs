namespace ApiDespachoAmbulancias.Dominio.Enumeradores;

/// <summary>
/// Tipo de emergência com peso diferenciado para cálculo de prioridade.
/// </summary>
public enum TipoEmergencia
{
    Clinica = 1,
    Trauma = 2,
    Queimadura = 3,
    Obstetrica = 4,
    Cardiaca = 5,
    MultiplasVitimas = 6
}
