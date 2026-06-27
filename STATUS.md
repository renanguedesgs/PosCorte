# PósCorte — Status técnico

> Atualizado: junho/2026 · Estratégia: **operação manual** + divulgação

---

## Projetos

| Projeto | Status |
|---------|--------|
| `PosCorte.Domain` | ✅ |
| `PosCorte.API` | ✅ REST + JWT + EF + operações admin |
| `PosCorte.Web` | ✅ Landing, arquiteto, admin manual |
| `PosCorte.Tests` | ✅ 34 testes |

---

## Rodar localmente

```powershell
# Terminal 1 — API http://localhost:5047
dotnet run --project src/PosCorte.API/PosCorte.API.csproj --launch-profile http

# Terminal 2 — Web http://localhost:5197
dotnet run --project src/PosCorte.Web/PosCorte.Web.csproj --launch-profile http

# Testes
dotnet test src/PosCorte.Tests/PosCorte.Tests.csproj
```

**Admin dev:** `admin@poscorte.com` / `Admin@PosCorte2026`

---

## Fluxo (operação manual)

```
Arquiteto cria projeto → Paga PIX (stub ou Asaas)
    → Admin aloca montador (/Admin/Projetos/Operar)
    → Montagem concluída (admin ou dev)
    → Arquiteto vistoria → Escrow liberado → Concluído
```

---

## Admin — telas

| Rota | Função |
|------|--------|
| `/Admin/Index` | KPIs + atalhos |
| `/Admin/Arquitetos` | Cadastro manual |
| `/Admin/Marceneiros` | Rede de montadores |
| `/Admin/Projetos` → Operar | Alocar + concluir obra |
| `/Admin/Conta` | Trocar senha |
| `/Admin/Financeiro` | Explicação escrow |

---

## Deploy

Ver [`docs/DEPLOY.md`](docs/DEPLOY.md) — Docker API + Web, CORS, env vars.

---

## O que falta (não é código)

- CNPJ + Asaas (PIX real)
- Domínio + hospedagem
- Divulgação e pilotos

Ver [`docs/ACOES_NECESSARIAS.md`](docs/ACOES_NECESSARIAS.md) e [`docs/PLAYBOOK_UNICO.md`](docs/PLAYBOOK_UNICO.md).

---

## Integrações (quando escalar)

| Serviço | Estado |
|---------|--------|
| Asaas PIX | Estrutura pronta, `Enabled=false` |
| Provedor API (Helpie/Gaba) | Desligado — manual por decisão |
| Notificações WhatsApp/e-mail | ✅ Real (Meta/Z-API + SMTP), config-gated → log |
| Auto-cadastro de montador | ✅ `/Marceneiros/Seja` + fila de aprovação no admin |
| Captação automática (Google Places) | ✅ Robô config-gated (`Captacao:Enabled`) |

> Como ligar a automação: [`docs/AUTOMACAO_CAPTACAO.md`](docs/AUTOMACAO_CAPTACAO.md)

---

*Documentação completa: `docs/` na raiz do repositório.*
