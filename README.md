# P�sCorte

Plataforma de intermedia��o de **montagem de m�veis planejados** ? or�amento instant�neo, PIX em escrow e opera��o manual de montadores (modelo fundador).

## Stack

- **ASP.NET Core 9** ? API REST + Razor Pages (Web)
- **PostgreSQL (Supabase)** ? EF Core 9
- **JWT + Cookie Auth** ? API + Web
- **xUnit** ? 34 testes
- **Docker** ? API + Web (`docker/`)
- **GitHub Actions** ? CI build/test + CD Railway (API)

---

## Estrutura

```
src/
??? PosCorte.API/     # API, servi�os, webhooks Asaas
??? PosCorte.Domain/  # Entidades
??? PosCorte.Web/     # Landing, arquiteto, admin
??? PosCorte.Tests/
docs/
??? PLAYBOOK_UNICO.md      # Ordem de execu��o (comercial + produto)
??? ACOES_NECESSARIAS.md   # O que s� voc� faz
??? DEPLOY.md              # Produ��o
??? templates/             # Planilhas MONTADORES, ARQUITETOS
docker/                    # Dockerfile + compose
```

---

## Rodar localmente

Pr�-requisito: **.NET 9 SDK** + `appsettings.Development.json` na API com connection string Supabase.

```powershell
dotnet run --project src/PosCorte.API/PosCorte.API.csproj --launch-profile http   # :5047
dotnet run --project src/PosCorte.Web/PosCorte.Web.csproj --launch-profile http   # :5197
dotnet test src/PosCorte.Tests/PosCorte.Tests.csproj
```

**Admin:** `admin@poscorte.com` / `Admin@PosCorte2026`

---

## Docker

```bash
cd docker
docker compose up --build
# API :8080 � Web :8081
```

---

## Documenta��o

| Doc | Conte�do |
|-----|----------|
| [`docs/PLAYBOOK_UNICO.md`](docs/PLAYBOOK_UNICO.md) | Fases 0?8, scripts, m�tricas |
| [`docs/ACOES_NECESSARIAS.md`](docs/ACOES_NECESSARIAS.md) | CNPJ, Asaas, deploy, comercial |
| [`docs/DEPLOY.md`](docs/DEPLOY.md) | Checklist produ��o |
| [`STATUS.md`](STATUS.md) | Snapshot t�cnico |

---

## Estado atual

- ? Produto completo para **opera��o manual** (cadastro arquiteto/montador, aloca��o, vistoria, escrow)
- ? PIX real quando **Asaas + CNPJ**
- ? Tra��o = **divulga��o** (planilhas em `docs/templates/`)
