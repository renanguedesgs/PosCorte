# PósCorte — Guia completo: o que falta, deploy gratuito e domínio real

> **Para quem já tem:** Supabase + Vercel  
> **Tempo do deploy inicial:** ~1 hora  
> **Custo mínimo:** R$ 0 (subdomínios gratuitos) · domínio `.com.br` ~R$ 40/ano (opcional)

---

## 1. Resumo em 30 segundos

| Peça | O que é | Você já tem? |
|------|---------|--------------|
| **Supabase** | Banco PostgreSQL | ✅ |
| **Vercel** | Landing estática (`public/`) | ✅ |
| **Render (ou Railway)** | API + Web ASP.NET (cadastro, dashboard, admin) | ❌ **obrigatório criar** |
| **Asaas** | PIX real | ❌ precisa CNPJ |
| **Domínio** | `poscorte.com.br` | ❌ opcional |

**A Vercel sozinha não roda o app .NET.** Sem o host da API + Web, o botão “Criar conta” na landing não abre o cadastro real (ou abre URL vazia).

---

## 2. O que já funciona no código (hoje)

### Arquiteto
- Cadastro `/Auth/Register` (login, senha, CPF, CEP)
- Dashboard, projetos, wizard 3 passos, orçamento ao vivo
- Perfil, logout
- Página de pagamento PIX (stub ou Asaas)
- Mapa do montador + trajeto (quando alocado)

### Montador
- Auto-cadastro `/Marceneiros/Seja` (sem login)
- Admin homologa em `/Admin/Marceneiros`

### Admin
- Painel, financeiro, projetos, **Operar** (alocar montador manualmente)
- Cadastro manual de arquiteto/montador

### Infra no código
- Migrations EF automáticas no startup da API
- Dockerfiles prontos (`docker/Dockerfile`, `docker/Dockerfile.web`)
- `render.yaml` (Blueprint opcional)
- Webhook Asaas (estrutura pronta, desligado)

---

## 3. O que ainda NÃO funciona (lista honesta)

### Infra / deploy (bloqueia ir ao ar)

| Item | Situação | Impacto |
|------|----------|---------|
| API em produção | Não deployada | Nada persiste / não há backend |
| Web em produção | Não deployada | Sem cadastro real na internet |
| `APP_WEB_URL` na Vercel | Pode estar vazio | Botões da landing não redirecionam |
| `ApiBaseUrl` no Web | Localhost em prod | Web não fala com API |
| CORS na API | Só URLs configuradas | Login quebra se URL errada |
| `JWT_SECRET` igual API ↔ login | Se diferentes, auth quebra | |
| Senha Supabase em env (não no git) | Credenciais no `appsettings` = risco | Reset + usar só variáveis |

### Produto (piloto manual OK, automático não)

| Item | Situação |
|------|----------|
| PIX real | Asaas `Enabled: false` — só simulação em Development |
| Alocação automática em 24h | `ProvedorApi:Enabled: false` — admin aloca manual |
| Portal do montador (login) | **Não existe** |
| WhatsApp / e-mail automático | `Notificacao:Enabled: false` |
| Captação Google Places | `Captacao:Enabled: false` |
| Split automático montador | Registrado no DB; transferência Asaas pendente |

### Jurídico / comercial (fora do código)

| Item | Necessário para |
|------|-----------------|
| CNPJ | Asaas produção, NF, conta PJ |
| Termos / Privacidade publicados | Confiança + Asaas |
| Processo operacional | Quem homologa montador e aloca em 24h |

---

## 4. Arquitetura (como tudo se conecta)

```
Usuário
   │
   ├─► https://pos-corte.vercel.app  (Vercel — landing HTML estática)
   │         │  APP_WEB_URL
   │         ▼
   ├─► https://poscorte-web-xxxx.onrender.com  (Web Razor — app completo)
   │         │  ApiBaseUrl
   │         ▼
   └─► https://poscorte-api-xxxx.onrender.com  (API REST + JWT)
                 │
                 ▼
            Supabase PostgreSQL
```

**URLs recomendadas no piloto gratuito:**

| Serviço | URL exemplo |
|---------|-------------|
| Landing | `https://pos-corte.vercel.app` |
| App (arquiteto) | `https://poscorte-web-xxxx.onrender.com` |
| API | `https://poscorte-api-xxxx.onrender.com` |
| Health | `.../api/v1/health` |

**Alternativa mais simples:** divulgar direto a URL do **Web no Render** (app completo com landing Razor) e usar Vercel só depois.

---

## 5. Stack 100% gratuita e limitações

| Serviço | Plano free | Limite importante |
|---------|------------|-------------------|
| **Supabase** | Free | 500 MB, projeto pausa após 7 dias sem uso → **Restore** no dashboard |
| **Vercel** | Hobby | Landing estática, domínio custom grátis |
| **Render** | Free | **Dorme** após 15 min sem uso; 1ª visita ~30–60 s; 750 h/mês |
| **Railway** | Trial/créditos | Alternativa ao Render (ver `docs/RAILWAY_ENV.md`) |
| **Domínio .com.br** | — | **Não é grátis** (~R$ 40/ano Registro.br) |

Para piloto com 5–15 arquitetos, o free tier basta. Para produção séria, planeje Render **Starter** (~US$ 7/serviço) para não dormir.

---

## 6. Passo a passo — deploy sem erro

Siga **na ordem**. Não pule etapas.

### FASE A — Supabase (10 min)

1. [supabase.com](https://supabase.com) → projeto **ativo** (se Paused → **Restore project**).
2. **Project Settings** → **Database** → **Reset database password** se não lembra.
3. Copie a senha e guarde (gerenciador de senhas).
4. Monte a connection string (uma linha, sem espaços):

```text
Host=db.SEU_PROJECT_REF.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=SUA_SENHA_AQUI;SSL Mode=Require;Trust Server Certificate=true
```

5. **Project Settings** → **Database** → confirme o host (`db.xxxx.supabase.co`).

> ⚠️ **Nunca** commite a senha no GitHub. Só em variáveis de ambiente do Render.

---

### FASE B — Render: API (15–20 min)

1. [render.com](https://render.com) → login com GitHub.
2. **New +** → **Web Service** → repositório **PosCorte**.
3. Configuração:

| Campo | Valor |
|-------|--------|
| Name | `poscorte-api` |
| Region | Oregon (ou mais perto) |
| Branch | `main` |
| Runtime | **Docker** |
| Dockerfile Path | `docker/Dockerfile` |
| Docker Context | `.` (raiz do repo) |
| Plan | **Free** |
| Health Check Path | `/api/v1/health` |

4. **Environment Variables** (copie exatamente os nomes):

| Key | Value |
|-----|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | (string completa da Fase A) |
| `JWT_SECRET` | string aleatória **32+ caracteres** (ex.: gere em [random.org](https://www.random.org/strings/)) |
| `Jwt__Issuer` | `PosCorteAPI` |
| `Jwt__Audience` | `PosCorteWeb` |
| `Cors__AllowedOrigins__0` | `https://pos-corte.vercel.app` |
| `Cors__AllowedOrigins__1` | `https://poscorte-web-XXXX.onrender.com` *(atualize depois com URL real do Web)* |
| `Admin__Email` | seu e-mail admin |
| `Admin__Password` | senha forte (só sua, não a padrão) |

5. **Create Web Service** → aguarde **Live** (5–15 min na 1ª vez).

6. Teste no navegador:

```text
https://poscorte-api-XXXX.onrender.com/api/v1/health
```

**Esperado:**

```json
{"status":"ok","database":"connected",...}
```

| Erro | Solução |
|------|---------|
| `password authentication failed` | Senha Supabase errada → reset + atualizar env |
| 503 / timeout | Render acordando → espere 60 s e tente de novo |
| Build failed | Ver **Logs** no Render |

**Anote:** `API_URL=https://poscorte-api-XXXX.onrender.com` (sem barra no final).

---

### FASE C — Render: Web (10–15 min)

1. **New +** → **Web Service** → mesmo repo.
2. Configuração:

| Campo | Valor |
|-------|--------|
| Name | `poscorte-web` |
| Runtime | **Docker** |
| Dockerfile Path | `docker/Dockerfile.web` |
| Plan | **Free** |

3. **Environment Variables:**

| Key | Value |
|-----|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ApiBaseUrl` | `https://poscorte-api-XXXX.onrender.com` *(API_URL, sem barra final)* |
| `Site__PublicUrl` | `https://poscorte-web-XXXX.onrender.com` |
| `Site__WhatsAppNumber` | `5511999999999` *(opcional, só dígitos)* |

4. **Create** → aguarde **Live**.

5. Teste:

```text
https://poscorte-web-XXXX.onrender.com/Auth/Register
```

Deve abrir o formulário completo de cadastro.

6. **Volte na API (Render)** e adicione/atualize:

```text
Cors__AllowedOrigins__1=https://poscorte-web-XXXX.onrender.com
```

Salve → Render redeploya a API automaticamente.

**Anote:** `WEB_URL=https://poscorte-web-XXXX.onrender.com`

---

### FASE D — Vercel: ligar landing ao app (5 min)

1. [vercel.com](https://vercel.com) → projeto **pos-corte**.
2. **Settings** → **Environment Variables**:

| Name | Value |
|------|--------|
| `APP_WEB_URL` | `https://poscorte-web-XXXX.onrender.com` |
| `CONTACT_WHATSAPP` | `5511999999999` *(fallback se app dormindo)* |

Marque **Production**, **Preview**, **Development** → Save.

3. **Deployments** → último → **⋯** → **Redeploy** (obrigatório após mudar env).

4. Teste:

- Abra `https://pos-corte.vercel.app`
- Clique **Criar conta** → deve ir para `...onrender.com/Auth/Register`
- Crie conta teste
- Supabase → **Table Editor** → tabela `usuarios` → linha nova

---

### FASE E — Admin e operação (5 min)

1. Login: `WEB_URL/Auth/Login`
2. Admin: e-mail/senha definidos em `Admin__Email` / `Admin__Password` na API  
   *(se não definiu: seed `admin@poscorte.com` / `Admin@PosCorte2026` — **troque imediatamente**)*
3. `/Admin/Conta` → trocar senha admin.
4. Fluxo piloto manual:

| Passo | Quem | Onde |
|-------|------|------|
| Arquiteto cria conta | Cliente | `/Auth/Register` |
| Cria projeto + “paga” | Cliente | Em **Development** simula PIX; em **Production** sem Asaas não cobra |
| Homologar montador | Você | `/Admin/Marceneiros` |
| Alocar montador | Você | `/Admin/Projetos` → Operar |
| Montador na fila | Montador | `/Marceneiros/Seja` |

---

### FASE F — Domínio real (opcional, ~R$ 40/ano)

**Opção 1 — Só app (recomendado no piloto):** use `poscorte-web-XXXX.onrender.com` e divulgue esse link. Grátis.

**Opção 2 — Domínio profissional `poscorte.com.br`:**

1. Compre em [registro.br](https://registro.br) (ou Cloudflare Registrar).
2. **App principal** no Render Web:
   - Render → `poscorte-web` → **Settings** → **Custom Domains** → `app.poscorte.com.br` (ou `www`)
   - Render mostra registro CNAME → configure no Registro.br
3. **API** (subdomínio):
   - `api.poscorte.com.br` → CNAME no Render `poscorte-api`
   - Atualize env:
     - Web: `ApiBaseUrl=https://api.poscorte.com.br`
     - API: `Cors__AllowedOrigins__2=https://app.poscorte.com.br`
     - Vercel: `APP_WEB_URL=https://app.poscorte.com.br`
4. **Landing na Vercel:**
   - Vercel → Domains → `poscorte.com.br` e `www.poscorte.com.br`
   - DNS: CNAME `www` → `cname.vercel-dns.com`
   - Apex `@` → A records da Vercel (instruções na Vercel)
5. Redeploy **API, Web e Vercel** após mudar URLs.

**Opção 3 — Tudo no Render (sem Vercel):** um domínio `poscorte.com.br` → só Web Render; landing Razor já está no Web (`/Index`).

---

## 7. Variáveis de ambiente — referência completa

### API (Render `poscorte-api`)

| Variável | Obrigatório | Exemplo / nota |
|----------|-------------|----------------|
| `ConnectionStrings__DefaultConnection` | ✅ | String Supabase |
| `JWT_SECRET` | ✅ | 32+ chars |
| `Jwt__Issuer` | ✅ | `PosCorteAPI` |
| `Jwt__Audience` | ✅ | `PosCorteWeb` |
| `Cors__AllowedOrigins__0` | ✅ | URL Vercel |
| `Cors__AllowedOrigins__1` | ✅ | URL Web Render |
| `Admin__Email` | ✅ | seu e-mail |
| `Admin__Password` | ✅ | senha forte |
| `Asaas__Enabled` | Depois CNPJ | `true` |
| `Asaas__ApiKey` | Depois CNPJ | painel Asaas |
| `Asaas__WebhookToken` | Depois CNPJ | segredo inventado |
| `Asaas__BaseUrl` | Sandbox | `https://sandbox.asaas.com/api/v3` |
| `Notificacao__WhatsApp__Enabled` | Opcional | `true` + tokens Meta/Z-API |
| `Captacao__Enabled` | Opcional | `true` + Google Places key |

### Web (Render `poscorte-web`)

| Variável | Obrigatório | Exemplo |
|----------|-------------|---------|
| `ApiBaseUrl` | ✅ | `https://poscorte-api-XXXX.onrender.com` |
| `Site__PublicUrl` | Recomendado | URL pública do Web |
| `Site__WhatsAppNumber` | Opcional | `5511...` |

### Vercel

| Variável | Obrigatório | Exemplo |
|----------|-------------|---------|
| `APP_WEB_URL` | ✅ | URL do Web Render |
| `CONTACT_WHATSAPP` | Opcional | fallback WhatsApp |

---

## 8. Depois do deploy — ligar o dinheiro (Asaas)

**Pré-requisito:** CNPJ + conta Asaas (sandbox primeiro).

1. Criar conta [asaas.com](https://www.asaas.com) no CNPJ da empresa.
2. Sandbox: copiar API Key.
3. Na API (Render), adicionar:

```text
Asaas__Enabled=true
Asaas__ApiKey=$aact_...
Asaas__WebhookToken=segredo-que-voce-inventa
Asaas__BaseUrl=https://sandbox.asaas.com/api/v3
```

4. Painel Asaas → Webhooks → URL:

```text
https://poscorte-api-XXXX.onrender.com/api/v1/webhooks/asaas
```

5. Evento: `PAYMENT_RECEIVED` (ou equivalente PIX confirmado).
6. Teste: arquiteto gera PIX em `/Projetos/Pagar` → paga no sandbox → projeto vai para `Aguardando_Provedor`.

Detalhes: [`INTEGRACAO_PAGAMENTO_ASAAS.md`](INTEGRACAO_PAGAMENTO_ASAAS.md)

---

## 9. Roadmap produto (após infra OK)

Ordem sugerida:

| Prioridade | Entrega | Libera |
|------------|---------|--------|
| P0 | Deploy Render + Supabase + Vercel | Piloto com cadastro real |
| P0 | Operação manual (admin aloca) | Primeiras montagens |
| P1 | Asaas produção | PIX real |
| P1 | Alocação automática rede interna | Menos trabalho manual |
| P2 | Portal montador (login + aceitar obra) | Disparar para marceneiros |
| P2 | WhatsApp (Z-API ou Meta) | Notificar montador/arquiteto |
| P3 | Domínio + SSL custom | Marca profissional |
| P3 | Captação Google Places | Leads montadores |

---

## 10. Checklist final (marque antes de divulgar)

### Infra
- [ ] Supabase ativo, senha só em env do Render
- [ ] API Live + `/api/v1/health` → database connected
- [ ] Web Live + `/Auth/Register` abre
- [ ] Cadastro teste aparece em Supabase `usuarios`
- [ ] Vercel `APP_WEB_URL` + redeploy
- [ ] Botão landing abre cadastro no Render
- [ ] CORS inclui URL do Web
- [ ] Senha admin padrão **alterada**
- [ ] `appsettings.json` **sem senhas reais** no GitHub (rotacionar se já commitou)

### Operação
- [ ] 2–3 montadores homologados no admin
- [ ] Script WhatsApp para arquitetos pilotos
- [ ] Quem responde disputa / alocação em 24h definido

### Comercial (quando cobrar)
- [ ] CNPJ
- [ ] Asaas configurado
- [ ] Termos e Privacidade revisados

---

## 11. Problemas comuns

| Sintoma | Causa | Fix |
|---------|-------|-----|
| Botão Vercel não redireciona | `APP_WEB_URL` vazio | Env + redeploy Vercel |
| Cadastro “erro genérico” | API down ou CORS | Health + CORS origins |
| Login ok no Web mas API 401 | `JWT_SECRET` mudou | Mesmo secret; usuário reloga |
| Página lenta 1ª vez | Render Free dormindo | Normal; upgrade ou UptimeRobot ping |
| PIX não gera | Asaas off | Esperado até Fase 8 |
| Montador não recebe no app | Portal não existe | WhatsApp manual + admin |
| Landing diferente do local | Vercel usa `public/index.html` estático | Use URL Render ou sincronize `public/` |

---

## 12. Atalho Blueprint Render

Se preferir criar API + Web de uma vez:

1. Render → **New +** → **Blueprint**
2. Conecte o repo (arquivo `render.yaml` na raiz)
3. Preencha `ConnectionStrings__DefaultConnection` e `ApiBaseUrl` quando solicitado
4. Siga Fase D (Vercel) e Fase E (admin)

---

## 13. Documentos relacionados

| Doc | Conteúdo |
|-----|----------|
| [`PASSO_A_PASSO_FINAL.md`](PASSO_A_PASSO_FINAL.md) | Versão curta deste guia |
| [`SUPABASE_RAILWAY.md`](SUPABASE_RAILWAY.md) | Supabase + Railway (alternativa ao Render) |
| [`RAILWAY_ENV.md`](RAILWAY_ENV.md) | Variáveis Railway |
| [`INTEGRACAO_PAGAMENTO_ASAAS.md`](INTEGRACAO_PAGAMENTO_ASAAS.md) | PIX real |
| [`PLAYBOOK_UNICO.md`](PLAYBOOK_UNICO.md) | Comercial e pilotos |
| [`ACOES_NECESSARIAS.md`](ACOES_NECESSARIAS.md) | Lista P0 negócio |

---

## 14. Ordem do que fazer esta semana

1. **Hoje:** Fases A → E (Supabase + Render + Vercel) — app no ar grátis  
2. **Amanhã:** 3 montadores em `/Marceneiros/Seja` + homologar no admin  
3. **Esta semana:** 5 arquitetos piloto na URL do Web  
4. **Quando tiver CNPJ:** Asaas (Fase 8)  
5. **Próximo sprint dev:** portal montador + alocação automática na rede  

**URL para divulgar no piloto gratuito:**

```text
https://poscorte-web-XXXX.onrender.com
```

ou, com Vercel ligada:

```text
https://pos-corte.vercel.app
```
