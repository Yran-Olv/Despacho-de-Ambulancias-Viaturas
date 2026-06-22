using ApiDespachoAmbulancias.Dominio.Entidades;
using ApiDespachoAmbulancias.Dominio.Enumeradores;
using ApiDespachoAmbulancias.Dominio.Contratos;
using ApiDespachoAmbulancias.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiDespachoAmbulancias.Infraestrutura;

public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ContextoBancoDados>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ApiDespachoAmbulancias.Aplicacao.Contratos.IRepositorioOcorrencia, Repositorios.RepositorioOcorrencia>();
        services.AddScoped<ApiDespachoAmbulancias.Aplicacao.Contratos.IRepositorioVeiculo, Repositorios.RepositorioVeiculo>();

        return services;
    }

    public static async Task InicializarBancoDadosAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContextoBancoDados>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ContextoBancoDados>>();
        var calculator = scope.ServiceProvider.GetRequiredService<ICalculadorPrioridade>();

        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync("""
            ALTER TABLE ocorrencias ADD COLUMN IF NOT EXISTS data_despacho timestamp with time zone NULL;
            ALTER TABLE ocorrencias ADD COLUMN IF NOT EXISTS data_conclusao timestamp with time zone NULL;
            ALTER TABLE ocorrencias ADD COLUMN IF NOT EXISTS veiculo_id uuid NULL;
            CREATE TABLE IF NOT EXISTS veiculos (
                id uuid NOT NULL PRIMARY KEY,
                identificacao varchar(20) NOT NULL,
                tipo varchar(50) NOT NULL,
                status varchar(50) NOT NULL,
                data_criacao timestamp with time zone NOT NULL,
                data_atualizacao timestamp with time zone NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_veiculos_identificacao ON veiculos (identificacao);
            CREATE INDEX IF NOT EXISTS ix_veiculos_status ON veiculos (status);
            CREATE INDEX IF NOT EXISTS ix_ocorrencias_veiculo_id ON ocorrencias (veiculo_id);
            """);

        if (!await context.Veiculos.AnyAsync())
        {
            logger.LogInformation("Inserindo veículos iniciais...");
            await context.Veiculos.AddRangeAsync(CriarVeiculosSeed());
            await context.SaveChangesAsync();
        }

        if (await context.Ocorrencias.AnyAsync())
            return;

        logger.LogInformation("Inserindo massa de dados inicial...");

        var seed = CriarMassaDeDados();
        foreach (var o in seed)
        {
            o.AtualizarTempoEspera();
            o.ScorePrioridade = calculator.CalcularScore(o);
        }

        await context.Ocorrencias.AddRangeAsync(seed);
        await context.SaveChangesAsync();

        logger.LogInformation("{Count} ocorrências de teste inseridas.", seed.Count);
    }

    private static List<Veiculo> CriarVeiculosSeed() =>
    [
        new Veiculo
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Identificacao = "SAMU-01",
            Tipo = TipoVeiculo.Basica,
            Status = StatusVeiculo.Disponivel,
            DataCriacao = DateTime.UtcNow
        },
        new Veiculo
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            Identificacao = "SAMU-02",
            Tipo = TipoVeiculo.Avancada,
            Status = StatusVeiculo.Disponivel,
            DataCriacao = DateTime.UtcNow
        },
        new Veiculo
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Identificacao = "UTI-01",
            Tipo = TipoVeiculo.UTI,
            Status = StatusVeiculo.Disponivel,
            DataCriacao = DateTime.UtcNow
        }
    ];

    private static List<Ocorrencia> CriarMassaDeDados()
    {
        var baseTime = DateTime.UtcNow.AddMinutes(-45);

        return
        [
            new Ocorrencia
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Cpf = "12345678901",
                Descricao = "Queda de escada com fratura exposta",
                Endereco = "Rua das Flores, 120 - Centro",
                Gravidade = GravidadeEmergencia.Alta,
                TipoEmergencia = TipoEmergencia.Trauma,
                PacientesEnvolvidos = 1,
                DataCriacao = baseTime.AddMinutes(5),
                Status = StatusOcorrencia.Ativo
            },
            new Ocorrencia
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Cpf = "98765432100",
                Descricao = "Parada cardiorrespiratória em via pública",
                Endereco = "Av. Brasil, 4500 - Zona Norte",
                Gravidade = GravidadeEmergencia.EmergenciaMaxima,
                TipoEmergencia = TipoEmergencia.Cardiaca,
                PacientesEnvolvidos = 1,
                DataCriacao = baseTime.AddMinutes(2),
                Status = StatusOcorrencia.Ativo
            },
            new Ocorrencia
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Cpf = "45678912345",
                Descricao = "Febre alta e desidratação em idoso",
                Endereco = "Rua Ipê, 88 - Bairro Verde",
                Gravidade = GravidadeEmergencia.Moderada,
                TipoEmergencia = TipoEmergencia.Clinica,
                PacientesEnvolvidos = 1,
                DataCriacao = baseTime.AddMinutes(30),
                Status = StatusOcorrencia.Ativo
            },
            new Ocorrencia
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Cpf = "32165498700",
                Descricao = "Acidente com múltiplas vítimas na rodovia",
                Endereco = "BR-101, km 42",
                Gravidade = GravidadeEmergencia.Critica,
                TipoEmergencia = TipoEmergencia.MultiplasVitimas,
                PacientesEnvolvidos = 4,
                DataCriacao = baseTime.AddMinutes(10),
                Status = StatusOcorrencia.Ativo
            },
            new Ocorrencia
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Cpf = "78912345600",
                Descricao = "Parto iminente em domicílio",
                Endereco = "Travessa Sol, 15 - Vila Nova",
                Gravidade = GravidadeEmergencia.Alta,
                TipoEmergencia = TipoEmergencia.Obstetrica,
                PacientesEnvolvidos = 2,
                DataCriacao = baseTime.AddMinutes(20),
                Status = StatusOcorrencia.Ativo
            },
            new Ocorrencia
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Cpf = "15935748620",
                Descricao = "Queimadura de segundo grau em cozinha industrial",
                Endereco = "Rua Industrial, 900",
                Gravidade = GravidadeEmergencia.Alta,
                TipoEmergencia = TipoEmergencia.Queimadura,
                PacientesEnvolvidos = 1,
                DataCriacao = baseTime.AddMinutes(15),
                Status = StatusOcorrencia.Ativo
            },
            new Ocorrencia
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Cpf = "75395185264",
                Descricao = "Consulta cancelada - ocorrência encerrada",
                Endereco = "Rua Teste, 1",
                Gravidade = GravidadeEmergencia.Baixa,
                TipoEmergencia = TipoEmergencia.Clinica,
                PacientesEnvolvidos = 1,
                DataCriacao = baseTime.AddMinutes(40),
                Status = StatusOcorrencia.Inativo
            }
        ];
    }
}
