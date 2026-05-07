# 💰 CashFlow — Controle de Fluxo de Caixa - VERX CONSULTORIA ( Carrefour )

Solução para o desafio de Arquiteto de Software: sistema de controle de lançamentos diários (débitos/créditos) com consolidado diário, construído sobre **.NET 8**, **CQRS**, **Clean Architecture** e **mensageria assíncrona com RabbitMQ**.

---

## 📐 Arquitetura

```
┌───────────────────────────────────────────────────────────────────┐
│                         CLIENT / FRONTEND                         │
└───────────┬───────────────────────────────┬───────────────────────┘
            │ REST                          │ REST
            ▼                              ▼
┌───────────────────────┐      ┌────────────────────────┐
│  Transactions API     │      │  Consolidation API     │
│  :5001                │      │  :5002                 │
│                       │      │                        │
│  POST /api/trans.     │      │  GET /api/consol/{dt}  │
│  GET  /api/trans.     │      │  GET /api/consol/range │
└───────────┬───────────┘      └────────────┬───────────┘
            │ Publish event                  │ Consume event
            ▼                               ▼
     ┌──────────────────────────────────────────────┐
     │              RabbitMQ (Fanout Exchange)       │
     │              cashflow.events                  │
     └──────────────────────────────────────────────┘
            │                               │
            ▼                               ▼
   ┌─────────────────┐           ┌─────────────────────┐
   │ transactions.db │           │ consolidation.db     │
   │ (SQLite)        │           │ (SQLite)             │
   └─────────────────┘           └─────────────────────┘
```



---

## 🏗️ Estrutura de Projetos

```
cashflow/
├── src/
│   ├── CashFlow.Shared/                        # DTOs e eventos compartilhados
│   ├── CashFlow.Transactions.Domain/           # Entidade Transaction, regras de negócio
│   ├── CashFlow.Transactions.Application/      # CQRS: Commands, Queries, Interfaces
│   ├── CashFlow.Transactions.Infrastructure/   # EF Core, RabbitMQ publisher, Repositório
│   ├── CashFlow.Transactions.API/              # ASP.NET Core Web API (porta 5001)
│   ├── CashFlow.Consolidation.Domain/          # Entidade DailyBalance
│   ├── CashFlow.Consolidation.Application/     # Queries, Event Handler
│   ├── CashFlow.Consolidation.Infrastructure/  # EF Core, RabbitMQ consumer, Repositório
│   └── CashFlow.Consolidation.API/             # ASP.NET Core Web API (porta 5002)
└── tests/
    ├── CashFlow.Transactions.Tests/            # xUnit + NSubstitute + FluentAssertions
    └── CashFlow.Consolidation.Tests/
```

### Padrões e práticas aplicadas

| Padrão | Onde |
|---|---|
| **Clean Architecture** | Separação Domain → Application → Infrastructure → API |
| **CQRS** com MediatR | Commands e Queries segregados por responsabilidade |
| **Domain-Driven Design (DDD)** | Agregados com fábricas estáticas e invariantes protegidas |
| **Repository Pattern** | Abstração de persistência desacoplada de ORM |
| **Event-Driven Architecture** | `TransactionCreatedEvent` publicado no RabbitMQ |
| **SOLID** | SRP, OCP, LSP, ISP, DIP aplicados em toda a solução |
| **Middleware de exceções** | Tratamento centralizado com respostas padronizadas |

---

## 🚀 Como rodar localmente

### Pré-requisitos
- [Docker](https://docs.docker.com/get-docker/) + Docker Compose
- **OU** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) + RabbitMQ local

### Opção 1 — Docker Compose (recomendado)

```bash
# Clone o repositório
git clone https://github.com/SEU_USUARIO/cashflow.git
cd cashflow

# Sobe RabbitMQ + ambos os serviços
docker compose up --build
```

Serviços disponíveis:
| Serviço | URL |
|---|---|
| Transactions API | http://localhost:5001/swagger |
| Consolidation API | http://localhost:5002/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

### Opção 2 — .NET SDK

```bash
# Terminal 1 — Transactions API
cd src/CashFlow.Transactions.API
dotnet run

# Terminal 2 — Consolidation API
cd src/CashFlow.Consolidation.API
dotnet run
```

> ⚠️ Certifique-se de ter o RabbitMQ rodando localmente na porta 5672.

### Rodar os testes

```bash
dotnet test
```

---

## 📡 Endpoints

### Transactions API (`:5001`)

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/transactions` | Registra um lançamento (crédito ou débito) |
| `GET` | `/api/transactions` | Lista lançamentos paginados |
| `GET` | `/api/transactions/by-date/{date}` | Lançamentos de um dia específico |
| `GET` | `/api/transactions/{id}` | Lançamento por ID |
| `GET` | `/health` | Health check |

**Exemplo de request:**
```json
POST /api/transactions
{
  "amount": 1500.00,
  "type": "Credit",
  "description": "Recebimento cliente XYZ",
  "occurredOn": "2024-06-01T10:00:00Z"
}
```

### Consolidation API (`:5002`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/consolidation/{date}` | Saldo consolidado de um dia |
| `GET` | `/api/consolidation/range?from=&to=` | Consolidado por período |
| `GET` | `/health` | Health check |

**Exemplo de response:**
```json
{
  "date": "2024-06-01T00:00:00Z",
  "totalCredits": 3000.00,
  "totalDebits": 800.00,
  "balance": 2200.00,
  "transactionCount": 5,
  "lastUpdatedAt": "2024-06-01T17:42:00Z"
}
```

---

## 📋 Requisitos Não-Funcionais atendidos

| Requisito | Solução |
|---|---|
| Transactions disponível mesmo se Consolidation cair | Mensageria assíncrona (RabbitMQ). O lançamento persiste mesmo sem o consumidor ativo. |
| 50 req/s no Consolidation com < 5% de perda | Fila durável + `BasicQos(prefetch=10)` + processamento assíncrono; horizontalmente escalável adicionando réplicas do consumidor. |
| Alta disponibilidade | Health checks expostos; serviços stateless e prontos para scale-out. |

---

## 🔭 Evoluções futuras

- **Outbox Pattern** — garante entrega exactly-once mesmo se o broker cair *durante* a transação
- **Cache com Redis** — cache do consolidado diário para absorver picos de leitura
- **Rate Limiting** — proteção da API contra sobrecarga
- **Authentication / Authorization** — JWT Bearer via Identity Server
- **Migrations via EF Core** — já há suporte no código, falta adicionar migration inicial
- **Observabilidade** — OpenTelemetry + Prometheus + Grafana
- **PostgreSQL em produção** — substituir SQLite (já desacoplado via repositório)
- **Dead Letter Queue** — reprocessamento de mensagens com falha
- **Integration Tests** — `WebApplicationFactory` com SQLite in-memory + TestContainers para RabbitMQ

---

## 🛠️ Stack

- **.NET 8** / C# 12
- **ASP.NET Core** Web API
- **MediatR 12** (CQRS)
- **Entity Framework Core 8** + SQLite
- **RabbitMQ.Client 7**
- **xUnit** + **NSubstitute** + **FluentAssertions**
- **Docker** + **Docker Compose**
