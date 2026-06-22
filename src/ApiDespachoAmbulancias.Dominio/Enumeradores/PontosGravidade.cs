namespace ApiDespachoAmbulancias.Dominio.Enumeradores;

/// <summary>
/// Pontos fixos por gravidade — fácil de entender e somar.
/// </summary>
public static class PontosGravidade
{
    public static int Obter(GravidadeEmergencia gravidade) => gravidade switch
    {
        GravidadeEmergencia.Baixa => 20,
        GravidadeEmergencia.Moderada => 40,
        GravidadeEmergencia.Alta => 60,
        GravidadeEmergencia.Critica => 80,
        GravidadeEmergencia.EmergenciaMaxima => 100,
        _ => 0
    };
}
