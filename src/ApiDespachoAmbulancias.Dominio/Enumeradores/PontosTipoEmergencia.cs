namespace ApiDespachoAmbulancias.Dominio.Enumeradores;

public static class PontosTipoEmergencia
{
    public static int Obter(TipoEmergencia tipo) => tipo switch
    {
        TipoEmergencia.Clinica => 2,
        TipoEmergencia.Trauma => 4,
        TipoEmergencia.Queimadura => 6,
        TipoEmergencia.Obstetrica => 8,
        TipoEmergencia.Cardiaca => 10,
        TipoEmergencia.MultiplasVitimas => 12,
        _ => 0
    };
}
