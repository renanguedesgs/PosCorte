# PósCorte API

Plataforma SaaS de intermediação de serviços de montagem de móveis planejados.

## Stack

- **ASP.NET Core 9** — Web API
- **PostgreSQL (Supabase)** — Banco de dados
- **Entity Framework Core 9** — ORM
- **Serilog** — Logging
- **Refit** — Integração com API de provedor externo
- **xUnit + Moq** — Testes unitários
- **Docker** — Containerização
- **Railway** — Deploy (CI/CD via GitHub Actions)

---

## Estrutura

```
src/
??? PosCorte.API/        # Controllers, Services, Middleware, EF DbContext
??? PosCorte.Domain/     # Entities, ValueObjects
??? PosCorte.Tests/      # Testes unitários (26 testes)
docs/                    # Documentação de arquitetura, API, webhooks e regras
docker/                  # Dockerfile e docker-compose
.github/workflows/       # CI (build+test) e CD (deploy Railway)
```

---

## Rodar localmente

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Conta no [Supabase](https://supabase.com) com banco PostgreSQL

### 1. Clonar e configurar
```bash
git clone https://github.com/renanguedesgs/PosCorte.git
cd PosCorte
```

Criar o arquivo `src/PosCorte.API/appsettings.Development.json` com sua connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.SEU_HOST.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 2. Aplicar migrations
```bash
cd src/PosCorte.API
dotnet ef database update
```

### 3. Rodar a API
```bash
dotnet run --project src/PosCorte.API
```

Acesse o Swagger em: **http://localhost:5000**

### 4. Rodar testes
```bash
dotnet test src/PosCorte.Tests/PosCorte.Tests.csproj
```

---

## Rodar com Docker

```bash
cd docker
docker compose up --build
```

API disponível em: **http://localhost:8080**

---

## Deploy (Railway)

1. Crie uma conta em [railway.app](https://railway.app)
2. Crie um novo projeto ? **Deploy from GitHub repo**
3. Selecione o repositório `PosCorte`
4. Adicione a variável de ambiente:
   ```
   ConnectionStrings__DefaultConnection = Host=...;Port=5432;...
   ```
5. Adicione o secret `RAILWAY_TOKEN` no GitHub:
   - GitHub ? Settings ? Secrets ? `RAILWAY_TOKEN`
6. O deploy acontece automaticamente a cada push na branch `main`

---

## Endpoints principais

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/v1/usuarios` | Criar usuário |
| `GET` | `/api/v1/usuarios` | Listar usuários |
| `POST` | `/api/v1/projetos` | Criar projeto |
| `POST` | `/api/v1/projetos/{id}/calcular-orcamento` | Calcular orçamento (markup 20%) |
| `GET` | `/api/v1/ordens-servico` | Listar ordens |
| `POST` | `/api/v1/webhooks/pagamento-confirmado` | Webhook pagamento PIX |
| `POST` | `/api/v1/webhooks/atualizacao-montador` | Webhook montador |

Documentação completa em [`docs/API_SPECIFICATION.md`](docs/API_SPECIFICATION.md)

---

## Segurança

- `appsettings.Development.json` está no `.gitignore` — a senha **nunca vai ao GitHub**
- Em produção, use variáveis de ambiente para a `ConnectionStrings__DefaultConnection`
