# PósCorte — Deploy em produção

> Checklist técnico para colocar API + Web no ar. **Estratégia:** operação manual (você cadastra arquitetos/montadores). Sem Helpie/Gaba por enquanto.

---

## Pré-requisitos (você)

- [ ] CNPJ + conta PJ (para Asaas depois)
- [ ] Domínio (ex.: `poscorte.com.br`)
- [ ] Banco Supabase com senha forte
- [ ] Conta Railway / Render / Azure

---

## 1. Variáveis de ambiente (API)

| Variável | Obrigatório | Exemplo |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Sim | Connection string PostgreSQL (Supabase) |
| `Jwt__Key` | Sim | String aleatória 32+ caracteres |
| `Jwt__Issuer` | Sim | `PosCorteAPI` |
| `Jwt__Audience` | Sim | `PosCorteWeb` |
| `Cors__AllowedOrigins__0` | Sim (prod) | `https://app.poscorte.com.br` |
| `Admin__Email` | Recomendado | Seu e-mail admin |
| `Admin__Password` | Recomendado | Senha forte (só na 1ª criação do admin) |
| `Asaas__Enabled` | Quando tiver CNPJ | `true` |
| `Asaas__ApiKey` | Quando tiver Asaas | Chave do painel |
| `Asaas__WebhookToken` | Quando tiver Asaas | Segredo que você inventa |

`ProvedorApi__Enabled` pode ficar **false** — alocação é manual no admin.

---

## 2. Variáveis de ambiente (Web)

| Variável | Obrigatório | Exemplo |
|----------|-------------|---------|
| `ApiBaseUrl` | Sim | `https://api.poscorte.com.br` |

---

## 3. Deploy Railway (sugestão)

1. Crie **dois serviços** no mesmo projeto:
   - `poscorte-api` — Dockerfile `docker/Dockerfile`, porta 8080
   - `poscorte-web` — Dockerfile `docker/Dockerfile.web`, porta 8080
2. Configure as env vars acima em cada serviço.
3. Domínio customizado:
   - API → `api.poscorte.com.br`
   - Web → `app.poscorte.com.br` (ou raiz)
4. Webhook Asaas: `https://api.poscorte.com.br/api/v1/webhooks/asaas`

O workflow `.github/workflows/deploy.yml` já faz deploy da API no push em `main` (precisa do secret `RAILWAY_TOKEN`).

---

## 4. Migrations

As migrations rodam **automaticamente** na subida da API (`db.Database.Migrate()`).

Manual (se precisar):

```powershell
cd src/PosCorte.API
dotnet ef database update
```

---

## 5. Pós-deploy (obrigatório)

- [ ] Login admin → **Admin → Minha conta** → trocar senha padrão
- [ ] Testar: criar arquiteto, montador, projeto, PIX (stub ou real)
- [ ] Confirmar que botão **Simular pagamento** não aparece em produção
- [ ] Revisar Termos/Privacidade com contador (`/Legal/Termos`, `/Legal/Privacidade`)

---

## 6. Docker local

```bash
cd docker
# Defina DB_CONNECTION e JWT_SECRET no ambiente ou .env
docker compose up --build
```

- API: http://localhost:8080  
- Web: http://localhost:8081  

---

## 7. Segurança

- Swagger só em `Development`
- Simulação de PIX bloqueada fora de `Development`
- CORS restrito via `Cors:AllowedOrigins` em produção
- Nunca commitar `appsettings.Development.json` com senhas reais

---

*Ver também: [`ACOES_NECESSARIAS.md`](ACOES_NECESSARIAS.md), [`INTEGRACAO_PAGAMENTO_ASAAS.md`](INTEGRACAO_PAGAMENTO_ASAAS.md), [`PLAYBOOK_UNICO.md`](PLAYBOOK_UNICO.md)*
