# Supabase → Railway (seu projeto)

> **Projeto:** `ftsdoekpuzjmbeprkivm`  
> **Não commite a senha no Git.** Configure só no painel do Railway.

---

## Dados do banco (já confirmados)

| Campo | Valor |
|-------|--------|
| Host | `db.ftsdoekpuzjmbeprkivm.supabase.co` |
| Port | `5432` |
| Database | `postgres` |
| User | `postgres` |
| Senha | A que você definiu no Supabase (não é `[YOUR-PASSWORD]`) |

URI (referência):
```
postgresql://postgres:SUA_SENHA_AQUI@db.ftsdoekpuzjmbeprkivm.supabase.co:5432/postgres
```

---

## Railway — serviço `poscorte-api`

**Settings → Variables → Raw Editor** — cole e troque `SUA_SENHA_AQUI`:

### Opção A — connection string (recomendada)

```
ConnectionStrings__DefaultConnection=Host=db.ftsdoekpuzjmbeprkivm.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=SUA_SENHA_AQUI;SSL Mode=Require;Trust Server Certificate=true
```

### Opção B — senha separada

```
DB_PASSWORD=SUA_SENHA_AQUI
JWT_SECRET=PosCorte-JWT-Producao-Altere-Esta-Chave-2026!!
Jwt__Issuer=PosCorteAPI
Jwt__Audience=PosCorteWeb
```

(O host já está no `appsettings.json` da API; só falta a senha.)

### Opção C — DATABASE_URL

```
DATABASE_URL=postgresql://postgres:SUA_SENHA_AQUI@db.ftsdoekpuzjmbeprkivm.supabase.co:5432/postgres
```

Se a senha tiver `@`, `#`, `$`, etc., use a **Opção A** (mais segura).

### CORS (landing Vercel)

```
Cors__AllowedOrigins__0=https://pos-corte.vercel.app
```

### Admin (primeiro acesso)

```
Admin__Email=seu@email.com
Admin__Password=SenhaForteQueVoceEscolher
```

---

## Railway — serviço `poscorte-web` (app com login)

Depois que a API estiver no ar:

```
ApiBaseUrl=https://SUA-API.up.railway.app
```

(Dockerfile: `docker/Dockerfile.web`)

---

## Vercel — landing

```
APP_WEB_URL=https://SEU-WEB.up.railway.app
CONTACT_WHATSAPP=5511XXXXXXXXX
```

---

## Testar

1. Redeploy da API no Railway (após salvar variáveis)
2. Abra no navegador:

```
https://SUA-API.up.railway.app/api/v1/health
```

**OK:** `{"status":"ok","database":"connected"}`  
**Erro 503:** senha errada ou projeto Supabase pausado (reative no dashboard)

3. Web: login em `https://SEU-WEB.up.railway.app/Auth/Login`  
4. Landing: botões em https://pos-corte.vercel.app passam a apontar pro Web

---

## Onde pegar a senha no Supabase

1. [supabase.com](https://supabase.com) → seu projeto  
2. **Project Settings** → **Database**  
3. **Database password** — se não lembrar, use **Reset database password**  
4. Cole no Railway (não no código)

---

## Local (dev)

O `appsettings.Development.json` (gitignored) já pode usar a mesma connection string com a senha real — só na sua máquina.
