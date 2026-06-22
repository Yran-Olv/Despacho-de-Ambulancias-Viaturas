using ApiDespachoAmbulancias.Aplicacao.Dtos;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiDespachoAmbulancias.Api.Swagger;

public sealed class ExemplosEsquemaSwagger : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CriarOcorrenciaRequest))
        {
            schema.Example = CriarExemploOcorrencia();
            schema.Description = """
                Cadastro de nova ocorrência na fila.
                Gravidade e Tipo aparecem como lista (dropdown) no Swagger.
                Formulário visual mais fácil: /
                """;
        }

        if (context.Type == typeof(AtualizarOcorrenciaRequest))
        {
            schema.Example = CriarExemploOcorrencia();
        }

        if (context.Type == typeof(OcorrenciaResponse))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("22222222-2222-2222-2222-222222222222"),
                ["cpf"] = new OpenApiString("98765432100"),
                ["descricao"] = new OpenApiString("Parada cardiorrespiratoria em via publica"),
                ["endereco"] = new OpenApiString("Av. Brasil, 4500 - Zona Norte"),
                ["gravidade"] = new OpenApiString("EmergenciaMaxima"),
                ["tipoEmergencia"] = new OpenApiString("Cardiaca"),
                ["pacientesEnvolvidos"] = new OpenApiInteger(1),
                ["tempoEsperaMinutos"] = new OpenApiInteger(5),
                ["scorePrioridade"] = new OpenApiDouble(118),
                ["pontosGravidade"] = new OpenApiInteger(100),
                ["pontosTipo"] = new OpenApiInteger(10),
                ["pontosPacientes"] = new OpenApiInteger(3),
                ["pontosEspera"] = new OpenApiInteger(5),
                ["resumoPrioridade"] = new OpenApiString("Gravidade 100 + Tipo 10 + Pacientes 3 + Espera 5 = 118"),
                ["posicaoFila"] = new OpenApiInteger(1),
                ["status"] = new OpenApiString("Ativo"),
                ["dataCriacao"] = new OpenApiString("2026-06-18T22:00:00Z"),
                ["dataAtualizacao"] = new OpenApiNull()
            };
        }
    }

    private static OpenApiObject CriarExemploOcorrencia() =>
        new()
        {
            ["cpf"] = new OpenApiString("12345678901"),
            ["descricao"] = new OpenApiString("Parada cardiorrespiratoria em via publica"),
            ["endereco"] = new OpenApiString("Av. Brasil, 4500 - Zona Norte"),
            ["gravidade"] = new OpenApiString("EmergenciaMaxima"),
            ["tipoEmergencia"] = new OpenApiString("Cardiaca"),
            ["pacientesEnvolvidos"] = new OpenApiInteger(1)
        };
}
