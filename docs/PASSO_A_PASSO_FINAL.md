# PósCorte — Passo a passo para finalizar (botão Cadastrar funcionando)

> **Tempo estimado:** 45–60 min  
> **O que você terá no fim:**  
> `pos-corte.vercel.app` → clica **Criar conta** → abre cadastro real → dados no **Supabase**

---

## Visão rápida (3 peças)

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  VERCEL         │     │  RENDER          │     │  SUPABASE       │
│  Landing        │────▶│  Web + API .NET  │────▶│  PostgreSQL     │
│  (já tem)       │     │  (falta criar)   │     │  (já tem)       │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

- **Supabase** = só banco (não abre tela de cadastro)
- **Render** = roda o app (cadastro, login, admin)
- **Vercel** = marketing; botão aponta pro Render via `APP_WEB_URL`

---

## PARTE 1 — Supabase (10 min)

### 1.1 Confirmar que o projeto está ativo

1. Acesse [supabase.com](https://supabase.com) → seu projeto  
2. Se estiver **Paused**, clique **Restore project**

### 1.2 Pegar ou resetar a senha do banco

1. **Project Settings** (engrenagem) → **Database**  
2. Em **Database password**:
   - Se não lembra → **Reset database password**  
   - **Copie e guarde** a senha nova (bloco de notas seguro)

### 1.3 Montar a connection string

Substitua `SUA_SENHA` pela senha real:

```
Host=db.ftsdoekpuzjmbeprkivm.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=SUA_SENHA;SSL Mode=Require;Trust Server Certificate=true
```

Guarde esse texto — você vai colar no Render no passo 2.

---

## PARTE 2 — Render: API (15 min)

### 2.1 Criar conta

1. [render.com](https://render.com) → Sign up (pode usar conta GitHub)  
2. Conecte o repositório **PosCorte** do GitHub

### 2.2 Criar serviço da API

1. **Dashboard** → **New +** → **Web Service**  
2. Selecione o repo **PosCorte**  
3. Preencha:

| Campo | Valor |
|-------|--------|
| Name | `poscorte-api` |
| Region | Oregon (ou mais perto de SP) |
| Branch | `main` |
| Runtime | **Docker** |
| Dockerfile Path | `docker/Dockerfile` |
| Instance type | **Free** |

4. **Advanced** → Health Check Path: `/api/v1/health`

### 2.3 Variáveis de ambiente da API

Na seção **Environment Variables**, adicione:

| Key | Value |
|-----|--------|
| `ConnectionStrings__DefaultConnection` | (cole a string da Parte 1.3 inteira) |
| `JWT_SECRET` | `PosCorte-JWT-Producao-Altere-Esta-Chave-2026!!` |
| `Jwt__Issuer` | `PosCorteAPI` |
| `Jwt__Audience` | `PosCorteWeb` |
| `Cors__AllowedOrigins__0` | `https://pos-corte.vercel.app` |
| `Admin__Email` | seu e-mail |
| `Admin__Password` | senha forte do admin |

5. **Create Web Service**  
6. Aguarde o deploy (5–10 min na 1ª vez)

### 2.4 Testar a API

Quando ficar **Live**, copie a URL (ex.: `https://poscorte-api-xxxx.onrender.com`).

Abra no navegador:

```
https://poscorte-api-xxxx.onrender.com/api/v1/health
```

| Resultado | Significado |
|-----------|-------------|
| `{"status":"ok","database":"connected"}` | Banco OK — siga em frente |
| `503` + password failed | Senha errada → refaça Parte 1.2 |
| Página em branco / erro | Veja **Logs** no Render |

**Anote a URL da API:** `https://poscorte-api-xxxx.onrender.com`

---

## PARTE 3 — Render: Web (10 min)

### 3.1 Criar segundo serviço

1. **New +** → **Web Service** → mesmo repo  
2. Preencha:

| Campo | Valor |
|-------|--------|
| Name | `poscorte-web` |
| Runtime | **Docker** |
| Dockerfile Path | `docker/Dockerfile.web` |
| Instance type | **Free** |

### 3.2 Variável do Web

| Key | Value |
|-----|--------|
| `ApiBaseUrl` | `https://poscorte-api-xxxx.onrender.com` (URL da API **sem** barra no final) |

3. **Create Web Service** → aguarde **Live**

### 3.3 Testar cadastro direto

Abra:

```
https://poscorte-web-xxxx.onrender.com/Auth/Register
```

Deve aparecer o **formulário de criar conta**.

Se der erro de API:
- Confira se `ApiBaseUrl` está certo  
- API está Live?  
- Free tier do Render “dorme” — primeira abertura pode demorar ~30s

**Anote a URL do Web:** `https://poscorte-web-xxxx.onrender.com`

---

## PARTE 4 — Vercel: ligar o botão (5 min)

### 4.1 Variável na Vercel

1. [vercel.com](https://vercel.com) → projeto **pos-corte**  
2. **Settings** → **Environment Variables**  
3. Adicione:

| Name | Value |
|------|--------|
| `APP_WEB_URL` | `https://poscorte-web-xxxx.onrender.com` (URL do Web, **sem** barra no final) |

Marque: Production, Preview, Development → **Save**

### 4.2 Redeploy obrigatório

1. **Deployments** → último deploy → **⋯** → **Redeploy**  
2. Aguarde terminar

### 4.3 Testar o fluxo completo

1. Abra **https://pos-corte.vercel.app**  
2. Clique **Criar conta** ou **Subir meu projeto**  
3. Deve abrir `...onrender.com/Auth/Register`  
4. Crie uma conta de teste  
5. Faça login → Dashboard → criar projeto

---

## PARTE 5 — Admin e operação manual (5 min)

### 5.1 Login admin

- URL: `https://poscorte-web-xxxx.onrender.com/Auth/Login`  
- Use o `Admin__Email` e `Admin__Password` que você definiu no Render (API)  
- Ou o seed padrão se não definiu admin: `admin@poscorte.com` / `Admin@PosCorte2026`

### 5.2 Trocar senha admin

`/Admin/Conta` → troque a senha padrão

### 5.3 Cadastrar piloto

| Tela | O que fazer |
|------|-------------|
| `/Admin/Arquitetos` | Cadastra arquiteto piloto |
| `/Admin/Marceneiros` | Cadastra montador da sua lista |
| `/Admin/Projetos` → Operar | Aloca montador após PIX |

---

## PARTE 6 — Opcional: WhatsApp enquanto Render “dorme”

No plano **Free**, o Render desliga após inatividade. A 1ª visita demora.

Na Vercel, adicione também:

```
CONTACT_WHATSAPP=5511999999888
```

Se `APP_WEB_URL` falhar, botões abrem WhatsApp.

Para produção séria: plano pago no Render ou outro host.

---

## Checklist final

- [ ] Supabase ativo + senha correta  
- [ ] `poscorte-api` Live + `/api/v1/health` OK  
- [ ] `poscorte-web` Live + `/Auth/Register` abre  
- [ ] Vercel `APP_WEB_URL` + redeploy  
- [ ] Botão na landing abre cadastro  
- [ ] Conta criada aparece no Supabase (Table Editor → `usuarios`)  
- [ ] Admin login OK  

---

## Problemas comuns

| Sintoma | Solução |
|---------|---------|
| Botão na Vercel não vai pra lugar nenhum | `APP_WEB_URL` vazio ou faltou **Redeploy** |
| `password authentication failed` | Reset senha Supabase + atualizar `ConnectionStrings__DefaultConnection` |
| Web abre mas cadastro falha | `ApiBaseUrl` errado no serviço Web |
| Tudo lento na 1ª vez | Render Free acordando — espere 30–60s |
| CORS error no login | Adicione URL do Web em `Cors__AllowedOrigins__1` na API |

---

## Atalho: Blueprint Render

Se o repo tiver `render.yaml` na raiz:

1. Render → **New +** → **Blueprint**  
2. Conecte o repo  
3. Preencha `ConnectionStrings__DefaultConnection` e `ApiBaseUrl` quando pedir  
4. Siga Parte 4 (Vercel) igual

---

## Depois disso (comercial — não é código)

1. Divulgar `https://pos-corte.vercel.app`  
2. Cadastrar arquitetos pilotos no admin  
3. Primeiro PIX (simulado em dev ou Asaas quando tiver CNPJ)

Ver também: [`PLAYBOOK_UNICO.md`](PLAYBOOK_UNICO.md)
