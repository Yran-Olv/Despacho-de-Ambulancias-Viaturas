<table>
  <tr>
    <td width="160" valign="middle">
      <img src="https://auth.uniaraxa.edu.br/app/Content/img/nova-logo.png" alt="Centro Universitário do Planalto de Araxá" width="160" />
    </td>
    <td valign="middle">
      <strong>Centro Universitário do Planalto de Araxá</strong><br/>
      Curso: Sistemas de Informação<br/>
      Disciplina: Códigos de Alta Performance
    </td>
  </tr>
</table>

---

| | |
|---|---|
| **Trabalho** | API com Filas de Prioridade e Heaps — Despacho de Ambulâncias |
| **Professor** | Ricardo Fulgencio Alves |
| **Grupo 9** | Yran Augusto, Carlos Gabriel, Nicholas Gabriel |
| **Solução** | `ApiDespachoAmbulancias` |
| **Recurso** | `Ocorrencia` |
| **Endpoint base** | `/ocorrencias` |
| **Data** | 18/06/2026 |

---

# Api de Despacho de Ambulâncias

API REST em **C# (.NET 8)** para **despacho de ambulâncias/viaturas**, com fila de prioridade usando **Heap Máximo** (`HeapMaximo`) e persistência em **PostgreSQL**.

## Tema do grupo

Central de regulação de emergências (SAMU): ocorrências não são atendidas apenas por ordem de chegada, mas por **gravidade clínica**, **tipo de emergência**, **número de pacientes** e **tempo de espera**.

## Arquitetura

Organização em **Clean Architecture** (4 camadas em português):

| Camada | Pasta | Responsabilidade |
|--------|-------|------------------|
| **Domínio** | `ApiDespachoAmbulancias.Dominio` | Entidades, `HeapMaximo`, `CalculadorPrioridade` |
| **Aplicação** | `ApiDespachoAmbulancias.Aplicacao` | Serviços, DTOs, validadores |
| **Infraestrutura** | `ApiDespachoAmbulancias.Infraestrutura` | PostgreSQL, repositórios, seed |
| **Api** | `ApiDespachoAmbulancias.Api` | Controladores, Swagger, painel web |

### Princípios SOLID

| Princípio | Aplicação no projeto |
|-----------|----------------------|
| **SRP** | `CalculadorPrioridade`, `HeapMaximo` e `ServicoOcorrencia` — uma responsabilidade cada |
| **OCP** | Regra isolada em `ICalculadorPrioridade` |
| **LSP** | `HeapMaximo<T>` implementa `IHeapPrioridade<T>` |
| **ISP** | Contratos enxutos (`IRepositorioOcorrencia`, `IServicoFilaPrioridade`) |
| **DIP** | Controladores dependem de `IServicoOcorrencia`, não do banco |

## Por que Heap Máximo?

| Operação | Heap Máximo | Lista ordenada |
|----------|-------------|----------------|
| Inserir ocorrência | O(log n) | O(n) |
| Consultar próximo despacho | O(1) | O(1) |
| Reordenar após atualização | O(log n) | O(n) |

O Heap **não persiste no banco**. O `ServicoFilaPrioridade` monta o `HeapMaximo` **em memória** ao listar ocorrências ativas.

**Arquivo:** `src/ApiDespachoAmbulancias.Dominio/EstruturasDados/HeapMaximo.cs`

## Regra de prioridade

**Score = soma de 4 grupos de pontos** (maior = mais urgente):

| Grupo | Pontos |
|-------|--------|
| **Gravidade** | Baixa=20 · Moderada=40 · Alta=60 · Crítica=80 · Emergência máxima=100 |
| **Tipo** | Clínica=2 · Trauma=4 · Queimadura=6 · Obstétrica=8 · Cardíaca=10 · Múltiplas vítimas=12 |
| **Pacientes** | 3 pts cada (máx. 10) |
| **Espera** | 1 pt/min (máx. 30) |

**Exemplo — parada cardíaca:** `100 + 10 + 3 + 0 = 113 pontos`

**Desempate:** gravidade → tipo → pacientes → espera → FIFO.

**Arquivo:** `src/ApiDespachoAmbulancias.Dominio/Servicos/CalculadorPrioridade.cs`

## Ciclo de vida

### Ocorrência

| Status | Significado |
|--------|-------------|
| `Ativo` | Na fila aguardando despacho |
| `EmDeslocamento` | Ambulância a caminho |
| `Concluida` | Baixa no hospital |
| `Inativo` | Exclusão lógica (DELETE) |

### Viatura

| Status | Significado |
|--------|-------------|
| `Disponivel` | Pode ser despachada |
| `EmAtendimento` | Vinculada a uma ocorrência |
| `Inativo` | Exclusão lógica |

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) e Docker Compose

## Como executar

### 1. Subir o banco

```bash
docker compose up -d
```

PostgreSQL na porta **5435**.

### 2. Executar a API

```bash
dotnet restore ApiDespachoAmbulancias.sln
dotnet run --project src/ApiDespachoAmbulancias.Api
```

### 3. Acessar

| Interface | URL |
|-----------|-----|
| **Painel web** | http://localhost:5080 |
| **Swagger** | http://localhost:5080/swagger |

### 4. Testes

```bash
dotnet test ApiDespachoAmbulancias.sln
```

8 testes em `tests/ApiDespachoAmbulancias.Testes/`.

## Endpoints — Ocorrências (obrigatórios do edital)

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/ocorrencias` | Cadastrar na fila |
| `GET` | `/ocorrencias/{id}` | Buscar por ID |
| `GET` | `/ocorrencias?page=1&size=10` | Listar ativas paginadas (Heap) |
| `GET` | `/ocorrencias/buscar?cpf=&descricao=` | Buscar por CPF e/ou descrição |
| `PUT` | `/ocorrencias/{id}` | Atualizar e recalcular prioridade |
| `DELETE` | `/ocorrencias/{id}` | Exclusão lógica → `Inativo` |

## Endpoints — Extras

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/ocorrencias/proxima` | Topo do Heap |
| `GET` | `/ocorrencias/a-caminho` | Em deslocamento |
| `GET` | `/ocorrencias/concluidas` | Histórico |
| `POST` | `/ocorrencias/{id}/despachar` | Despacha viatura |
| `POST` | `/ocorrencias/{id}/concluir` | Baixa no hospital |
| `GET/POST/DELETE` | `/veiculos` | Gestão de viaturas |

## Exemplo — POST /ocorrencias

```json
{
  "cpf": "12345678901",
  "descricao": "Parada cardiorrespiratória em via pública",
  "endereco": "Av. Brasil, 4500 - Zona Norte",
  "gravidade": "EmergenciaMaxima",
  "tipoEmergencia": "Cardiaca",
  "pacientesEnvolvidos": 1
}
```

## Massa de dados

Na primeira execução: **6 ocorrências ativas**, **1 inativa**, **3 viaturas** (SAMU-01, SAMU-02, UTI-01).

**Arquivo:** `src/ApiDespachoAmbulancias.Infraestrutura/InjecaoDependencia.cs`

## Estrutura do projeto

```
ApiDespachoAmbulancias/
├── docker-compose.yml
├── ApiDespachoAmbulancias.sln
├── src/
│   ├── ApiDespachoAmbulancias.Dominio/
│   │   ├── EstruturasDados/HeapMaximo.cs
│   │   ├── Servicos/CalculadorPrioridade.cs
│   │   └── Entidades/Ocorrencia.cs, Veiculo.cs
│   ├── ApiDespachoAmbulancias.Aplicacao/
│   │   └── Servicos/ServicoOcorrencia.cs, ServicoFilaPrioridade.cs
│   ├── ApiDespachoAmbulancias.Infraestrutura/
│   │   └── Repositorios/RepositorioOcorrencia.cs
│   └── ApiDespachoAmbulancias.Api/
│       ├── Controladores/ControladorOcorrencias.cs
│       └── wwwroot/
└── tests/ApiDespachoAmbulancias.Testes/
```

## Arquivos-chave para apresentação

| Conceito | Arquivo |
|----------|---------|
| Heap Máximo | `Dominio/EstruturasDados/HeapMaximo.cs` |
| Prioridade | `Dominio/Servicos/CalculadorPrioridade.cs` |
| Fila (heap) | `Aplicacao/Servicos/ServicoFilaPrioridade.cs` |
| Exclusão lógica | `Aplicacao/Servicos/ServicoOcorrencia.cs` → `ExcluirLogicamenteAsync` |
| Endpoints | `Api/Controladores/ControladorOcorrencias.cs` |
| Banco Docker | `docker-compose.yml` |

## Conexão com o banco

```
Host=localhost;Port=5435;Database=emergency_dispatch;Username=dispatch;Password=dispatch123
```

---

*Centro Universitário do Planalto de Araxá — Sistemas de Informação — Códigos de Alta Performance*
