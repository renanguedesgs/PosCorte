# Variáveis de ambiente — Railway / Render (API)

Copie no painel do serviço **poscorte-api**:

## Opção A — recomendada (connection string completa)

```
ConnectionStrings__DefaultConnection=Host=db.SEU_PROJECT.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=SUA_SENHA_SUPABASE;SSL Mode=Require;Trust Server Certificate=true
```

Pegue em: Supabase → Project Settings → Database → Connection string → URI (modo **Session** ou **Direct**).

## Opção B — senha separada

```
DB_PASSWORD=SUA_SENHA_SUPABASE
JWT_SECRET=uma-chave-aleatoria-com-pelo-menos-32-caracteres
```

O `appsettings.json` já tem o host do Supabase; só falta a senha.

## JWT (obrigatório)

```
JWT_SECRET=PosCorte-Producao-Altere-Esta-Chave-2026!!
Jwt__Issuer=PosCorteAPI
Jwt__Audience=PosCorteWeb
```

## CORS (após ter URL do Web)

```
Cors__AllowedOrigins__0=https://SEU-WEB.up.railway.app
Cors__AllowedOrigins__1=https://pos-corte.vercel.app
```

## Admin (primeiro deploy)

```
Admin__Email=seu@email.com
Admin__Password=SenhaForteAqui123!
```

## Testar

Após deploy, abra:

```
https://SUA-API.up.railway.app/api/v1/health
```

Deve retornar: `{"status":"ok","database":"connected"}`

Se der 503, a senha ou host está errado — veja os **Logs** do Railway.

---

## Web (segundo serviço)

```
ApiBaseUrl=https://SUA-API.up.railway.app
```

Na **Vercel** (landing):

```
APP_WEB_URL=https://SEU-WEB.up.railway.app
```
