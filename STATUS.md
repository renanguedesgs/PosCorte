# PósCorte — Documentação Técnica

> **Runtime:** .NET 9 · **Banco:** PostgreSQL (Supabase) via EF Core · **Branch:** `main`
> **Arquitetura:** Domain + API REST (JWT) + Web (Razor Pages / Cookie Auth) + Testes (xUnit)

PósCorte é uma plataforma B2B que intermedia a **montagem de móveis planejados**, conectando
arquitetos/designers a montadores homologados. O arquiteto sobe o arquivo de corte, recebe um
orçamento automático (markup inverso de 20%), paga via PIX (retido em Escrow) e um montador é
alocado automaticamente via provedor externo.

---

## Status dos projetos

| Projeto            | Status      | Responsabilidade                                          |
|--------------------|-------------|-----------------------------------------------------------|
| `PosCorte.Domain`  | ✅ Concluído | Entidades de negócio e Value Objects                      |
| `PosCorte.API`     | ✅ Concluído | REST API com JWT, EF Core, Swagger, Serilog, `[Authorize]`|
| `PosCorte.Tests`   | ✅ Concluído | 26 testes unitários (xUnit + Moq) — todos passando        |
| `PosCorte.Web`     | ✅ Concluído | Frontend Razor Pages completo (landing, auth, dashboard, projetos, ordens) |

---

## Como rodar localmente

Pré-requisitos: **.NET 9 SDK** e acesso de rede ao banco Supabase (já configurado em
`appsettings.Development.json`).

Abra **dois terminais** na raiz do repositório:

```powershell
# Terminal 1 — API (http://localhost:5047, Swagger na raiz)
dotnet run --project src/PosCorte.API/PosCorte.API.csproj --launch-profile http

# Terminal 2 — Web (http://localhost:5197)
dotnet run --project src/PosCorte.Web/PosCorte.Web.csproj --launch-profile http
```

Acesse **http://localhost:5197**, crie uma conta e comece a cadastrar projetos.
A API precisa estar no ar para o Web funcionar (o Web consome a API via `ApiService`).

Rodar os testes:

```powershell
dotnet test PosCorte.slnx
```

---

## Fluxo principal de negócio

```
[Arquiteto]
   ?? POST /api/v1/auth/register .............. cria conta (BCrypt)
   ?? POST /api/v1/auth/login ................. recebe JWT Bearer (8h)
   ?? POST /api/v1/projetos ................... cria projeto com URL do arquivo de corte
   ?? POST /api/v1/projetos/{id}/calcular-orcamento
   ?        ?? PrecificacaoService ............ markup inverso 20%: Pre?o = Custo / (1 - 0.20)
   ?? [Paga via PIX]
          ?
[Webhook POST /api/v1/webhooks/pagamento-confirmado]   ? TRAVA DE SEGURAN?A (Escrow)
   ?? PagamentoService.ValidarPagamentoPixAsync()
   ?? PagamentoService.ReservarFundosAsync()
   ?? ProvedorService.CriarOrdemServicoAsync()  ? Refit ? API externa de montadores
   ?? Projeto.StatusProjeto = "Ordem_Criada"
          ?
[Webhook POST /api/v1/webhooks/atualizacao-montador]
   ?? "aceito"    ? StatusProjeto = "Prestador_Alocado"
   ?? "concluido" ? StatusProjeto = "Aguardando_Vistoria"
   ?? "cancelado" ? StatusProjeto = "Cancelado"
```

### Estados do projeto

| Status                  | Descri??o                                       |
|-------------------------|-------------------------------------------------|
| `Aguardando_Pagamento`  | Projeto criado, aguardando PIX                  |
| `Pagamento_Confirmado`  | PIX validado, fundos em Escrow                  |
| `Ordem_Criada`          | Ordem enviada ao provedor de montadores         |
| `Prestador_Alocado`     | Montador aceitou e foi alocado                  |
| `Aguardando_Vistoria`   | Servi?o conclu?do, aguardando vistoria (72h)    |
| `Cancelado`             | Servi?o cancelado                               |

---

## PosCorte.Web ? p?ginas

| P?gina                 | Rota                       | Fun??o                                                    |
|------------------------|----------------------------|-----------------------------------------------------------|
| Landing                | `/`                        | Marketing p?blico (hero, como funciona, pre?os, CTA)      |
| Login                  | `/Auth/Login`              | Form ? API; salva JWT na Session + cookie de autentica??o |
| Cadastro               | `/Auth/Register`           | Form ? `POST /auth/register`                              |
| Logout                 | `/Auth/Logout` (POST)      | `SignOut` + `Session.Clear`                              |
| Dashboard              | `/Dashboard`               | Resumo: totais, projetos recentes (somente do usu?rio)    |
| Projetos               | `/Projetos/Index`          | Lista de projetos do usu?rio                              |
| Novo projeto           | `/Projetos/Criar`          | Form + **preview de or?amento em tempo real** (JS)        |
| Detalhe do projeto     | `/Projetos/Detalhes?id=`   | Dados, or?amento (via API) e ordens de servi?o            |
| Ordens de servi?o      | `/Ordens/Index`            | Acompanhamento dos montadores alocados                    |

**Autentica??o Web:** o login decodifica o JWT (`Services/JwtHelper.cs`) para preencher o cookie
de identidade (nome, e-mail, id) e guarda o token bruto em `Session["jwt"]`. O `ApiService` injeta
`Authorization: Bearer` automaticamente em todas as chamadas ? API. P?ginas internas usam
`[Authorize]`; usu?rios n?o logados s?o redirecionados para o login.

---

## Rede de Marceneiros (oferta propria + avaliacoes)

A rede propria de profissionais e o principal ativo do marketplace. Em vez de depender de uma
API externa de terceiros (a Helpie, por exemplo, so libera API mediante parceria), o PosCorte
mantem sua propria base de marceneiros, com reputacao e alocacao inteligente.

### Entidades

- `Marceneiro` - perfil (nome, foto, telefone/WhatsApp, cidade/UF, especialidades, bio,
  `NotaMedia`, `TotalAvaliacoes`, `TotalServicos`, `Disponivel`, `Verificado`, `OrigemExterna`).
- `Avaliacao` - `MarceneiroId`, `Nota` (1-5), `Comentario`, `AutorNome`, `ProjetoId?`. A nota
  media do marceneiro e recalculada de forma incremental a cada nova avaliacao.

### Endpoints (`api/v1/marceneiros`, todos com `[Authorize]`)

| Metodo | Rota                | Funcao                                                          |
|--------|---------------------|-----------------------------------------------------------------|
| `GET`  | `/`                 | Lista com filtros `cidade`, `especialidade`, `notaMin`, `disponivel` |
| `GET`  | `/{id}`             | Detalhe do marceneiro + avaliacoes                              |
| `GET`  | `/{id}/avaliacoes`  | Lista de avaliacoes                                            |
| `POST` | `/{id}/avaliacoes`  | Cria avaliacao (1-5) e recalcula a nota media                  |
| `POST` | `/seed`             | Popula a rede consumindo a API publica randomuser.me (locale BR) |

### Seed via API publica

`MarceneiroSeedService` consome `https://randomuser.me/api/` (gratuita, sem chave) para importar
nomes, fotos e localidades brasileiras reais, enriquecendo cada profissional com especialidades,
reputacao inicial e flags de verificacao/disponibilidade. Evita duplicatas via `OrigemExterna`
(`randomuser:{uuid}`). Configuravel em `RandomUserApi:BaseUrl`.

### Alocacao automatica

No webhook `pagamento-confirmado`, apos a confirmacao do PIX (Escrow), o sistema chama
`IMarceneiroService.AlocarParaProjetoAsync(cidade, especialidade)`, que escolhe o melhor
marceneiro disponivel priorizando: mesma cidade -> especialidade compativel -> maior nota ->
mais servicos. O profissional e vinculado a `OrdemServico` e o projeto passa a `Prestador_Alocado`.
O provedor externo legado virou best-effort: se indisponivel, um ID local (`PC-{guid}`) e gerado e
o fluxo continua com a rede propria.

### UI Web

- `Marceneiros/Index` - vitrine com cards (foto, estrelas, especialidades, disponibilidade),
  filtros e botao "Importar da rede publica".
- `Marceneiros/Detalhes` - perfil completo, estrelas, contato por WhatsApp, lista de avaliacoes
  e formulario de avaliacao por estrelas.

---

## Configuração e ambiente

`PosCorte.API/appsettings.Development.json` (uso local) contém a connection string do Supabase e a
chave JWT de desenvolvimento. Em produção, use **variáveis de ambiente** (`appsettings.json` usa
placeholders `${DB_PASSWORD}` e `${JWT_SECRET}`):

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key` (use uma chave forte e secreta)
- `ProvedorApi__BaseUrl`

`PosCorte.Web/appsettings.json` define `ApiBaseUrl` (URL pública da API em produção).

---

## Deploy

1. **Banco:** o Supabase (PostgreSQL) já está provisionado. As migrations rodam automaticamente na
   inicialização da API (`db.Database.Migrate()`).
2. **API:** publicar `PosCorte.API` (ex.: Azure App Service, Render, Railway, container). Configurar
   as variáveis de ambiente acima. HTTPS recomendado.
3. **Web:** publicar `PosCorte.Web` e apontar `ApiBaseUrl` para a URL pública da API.
4. **CORS:** a API hoje usa `AllowAll` — restringir à origem do Web em produção.

### Trocar para SQL Server (opcional)

O EF Core abstrai o provedor. Para migrar de PostgreSQL para SQL Server / Azure SQL:

1. Trocar o pacote `Npgsql.EntityFrameworkCore.PostgreSQL` por
   `Microsoft.EntityFrameworkCore.SqlServer` no `PosCorte.API.csproj`.
2. Em `Program.cs`, trocar `options.UseNpgsql(...)` por `options.UseSqlServer(...)`.
3. No `PosCorteDbContext`, ajustar especificidades: `UseIdentityAlwaysColumn()` →
   `UseIdentityColumn()` e `HasDefaultValueSql("NOW()")` → `HasDefaultValueSql("GETUTCDATE()")`.
4. Regerar as migrations (`dotnet ef migrations add InitialSqlServer`) e apontar a connection string.

---

## Próximos passos (integrações reais)

Os serviços abaixo estão como **stub** e prontos para integração:

- [ ] `PagamentoService` → gateway PIX real (Asaas ou Iugu) com validação de webhook.
- [ ] `ProvedorService` → API real do provedor de montadores (Helpie B2B / Orkestra).
- [ ] `NotificacaoService` → e-mail (SendGrid) + SMS/WhatsApp (Twilio).
- [ ] Agendador (Hangfire/Quartz) para liquidar o Escrow após 72h úteis de vistoria.
- [ ] Restringir CORS e configurar secret de validação de origem dos webhooks.
