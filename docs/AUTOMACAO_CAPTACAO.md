# Automação de captação de montadores + notificações

> Implementado: rede de montadores cresce sem cadastro manual e sem você caçar gente.
> Tudo é **config-gated**: enquanto as chaves não forem preenchidas, o sistema funciona em modo log/manual (sem quebrar nada).

---

## 1. Os 3 canais de entrada de montadores

| Canal | Como funciona | Onde |
|-------|---------------|------|
| **Auto-cadastro** | O montador se cadastra sozinho pelo link público | Página `/Marceneiros/Seja` (anônima) |
| **Captação (robô)** | Job busca montadores no Google Places por cidade × termo, cria leads e convida por WhatsApp | `CaptacaoMarceneirosBackgroundService` |
| **API parceira** | Fallback pago (Helpie/Gaba) já existente | `ProvedorService` |

Todos entram como **pendentes** (`Verificado=false`, `Disponivel=false`).
No admin (`/Admin/Marceneiros`) há a **fila de aprovação**: 1 clique em *Homologar* e o montador entra na alocação automática (`AlocarParaProjetoAsync`).

Deduplicação: campo `Marceneiro.OrigemExterna` (`autocadastro`, `places:{place_id}`, `manual:{guid}`).

---

## 2. Notificações reais (WhatsApp + e-mail)

Seção `Notificacao` no appsettings / variáveis de ambiente. Sem isso, cai em log (stub).

### WhatsApp — Meta Cloud API (oficial, recomendado)
```
Notificacao__WhatsApp__Enabled            = true
Notificacao__WhatsApp__Provider           = meta
Notificacao__WhatsApp__MetaPhoneNumberId  = (do painel Meta)
Notificacao__WhatsApp__MetaToken          = (token permanente)
```

### WhatsApp — Z-API (alternativa nacional)
```
Notificacao__WhatsApp__Enabled            = true
Notificacao__WhatsApp__Provider           = zapi
Notificacao__WhatsApp__ZapiInstanceId     = ...
Notificacao__WhatsApp__ZapiInstanceToken  = ...
Notificacao__WhatsApp__ZapiClientToken    = ...
```

### E-mail (SMTP — Resend/SendGrid/Brevo/Gmail App Password)
```
Notificacao__Email__Enabled   = true
Notificacao__Email__SmtpHost  = smtp.seuprovedor.com
Notificacao__Email__SmtpPort  = 587
Notificacao__Email__Username  = ...
Notificacao__Email__Password  = ...
Notificacao__Email__From      = nao-responda@seudominio.com.br
```

`Notificacao__AppBaseUrl` deve apontar para a URL pública do Web (usada nos convites).

---

## 3. Robô de captação (Google Places)

Seção `Captacao`. Precisa de uma chave da **Google Places API** (Text Search + Place Details).

```
Captacao__Enabled              = true
Captacao__GooglePlacesApiKey   = (sua chave)
Captacao__Cidades__0           = São Paulo, SP
Captacao__Cidades__1           = Campinas, SP
Captacao__TermosBusca__0       = montador de móveis planejados
Captacao__IntervaloHoras       = 24
Captacao__MaxLeadsPorCiclo     = 40
Captacao__EnviarConvite        = true
```

Fluxo por ciclo: busca → dedup por `places:{id}` → cria lead pendente → (opcional) convite WhatsApp com link `/Marceneiros/Seja`.

> LGPD: o convite é opt-in/educado e a pessoa só entra de fato na rede ao se cadastrar e ser homologada.

---

## 4. Alcance do arquiteto (Corte Cloud)

- Cadastro de arquiteto **público** já existe (`/Auth/Register`).
- Landing com estimador sem login (lead magnet).
- **Indique e ganhe** no Dashboard do arquiteto: compartilhamento por WhatsApp + copiar link.

---

## 5. Resumo dos endpoints novos

| Método | Rota | Auth |
|--------|------|------|
| POST | `/api/v1/marceneiros/auto-cadastro` | anônimo |
| GET  | `/api/v1/admin/marceneiros?verificado=` | Admin |
| POST | `/api/v1/admin/marceneiros/{id}/verificar` | Admin |
| POST | `/api/v1/admin/marceneiros/{id}/disponibilidade` | Admin |
