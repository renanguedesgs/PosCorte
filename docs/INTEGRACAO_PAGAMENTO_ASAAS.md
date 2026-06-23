# Integração de pagamento PIX (Asaas) — PósCorte

Este documento descreve a **estrutura já implementada** no código e o que **você precisa preencher** quando tiver CNPJ e conta Asaas própria. Enquanto isso, o sistema opera em **modo Stub** (simulação) — **nenhum PIX real é gerado nem cobrado**.

---

## Resumo executivo

| Situação | Comportamento |
|----------|---------------|
| `Asaas:Enabled = false` ou `ApiKey` vazio | Modo **Stub**: PIX fake, botão "Simular pagamento" só em Development |
| `Asaas:Enabled = true` + `ApiKey` preenchida | Cobrança PIX real via API Asaas + webhook de confirmação |
| Conta de terceiros / sem CNPJ | **Não configure** — use Stub em dev até ter conta própria |

---

## Arquitetura

```
Arquiteto (Web)                    API PósCorte                         Asaas
     │                                  │                                  │
     │  POST /projetos/{id}/gerar-pix   │                                  │
     ├─────────────────────────────────►│  (Stub ou API Asaas)             │
     │◄─────────────────────────────────┤  QR + copia-e-cola               │
     │                                  │                                  │
     │  Página /Projetos/Pagar          │                                  │
     │  (polling status)                │                                  │
     │                                  │◄──── POST /webhooks/asaas ───────┤
     │                                  │      PAYMENT_RECEIVED            │
     │                                  │  → confirma → escrow → ordem     │
```

### Entidades

- **`Pagamento`** — cobrança PIX por projeto (`Modo`: `Stub` | `Asaas`)
- **`Liquidacao`** — repasse ao marceneiro após conclusão (escrow)

### Endpoints principais

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/v1/projetos/{id}/gerar-pix` | Gera cobrança (auth JWT) |
| `GET` | `/api/v1/projetos/{id}/pagamento` | Status do pagamento |
| `POST` | `/api/v1/webhooks/asaas` | Webhook oficial Asaas |
| `POST` | `/api/v1/pagamentos/{id}/simular-confirmacao` | **Só Development** — simula PIX pago |
| `GET` | `/api/v1/admin/gateway-pagamento` | Admin: status do gateway |

### Tela Web

- **`/Projetos/Pagar/{id}`** — QR, copia-e-cola, simulação em dev
- Botão **"Pagar com PIX"** em `/Projetos/Detalhes` quando status = `Aguardando_Pagamento`

---

## Modo Stub (atual — seguro para desenvolvimento)

Com a configuração padrão em `appsettings.json`:

```json
"Asaas": {
  "BaseUrl": "https://sandbox.asaas.com/api/v3",
  "ApiKey": "",
  "WebhookToken": "",
  "Enabled": false,
  "DiasVencimentoPix": 1
}
```

O sistema:

1. Cria registro `Pagamento` com `Modo = Stub`
2. Gera `PixCopiaECola` fake (`STUB-...`)
3. **Não chama a API Asaas**
4. Em **Development**, exibe botão **"Simular pagamento"** que confirma o fluxo sem dinheiro real

### Testar fluxo completo em dev

1. Subir API (`localhost:5047`) e Web (`localhost:5197`)
2. Login como arquiteto → criar projeto → calcular orçamento
3. Projeto fica `Aguardando_Pagamento` → **Pagar com PIX**
4. Clicar **Simular pagamento (dev)** → projeto avança → ordem de serviço criada

---

## O que você precisa quando tiver conta própria

### 1. Pré-requisitos legais e comerciais

- [ ] **CNPJ** ativo (MEI ou LTDA)
- [ ] Conta **Asaas** no nome da empresa ([asaas.com](https://www.asaas.com))
- [ ] Conta bancária vinculada para receber repasses
- [ ] Política de privacidade / termos de uso publicados (exigência de gateways)

### 2. Criar conta e obter credenciais

1. Cadastre-se no Asaas (comece pelo **Sandbox** para testes)
2. No painel: **Integrações → API** → copie a **API Key**
3. Defina um **Webhook Token** (string secreta que você inventa)

### 3. Variáveis de ambiente (nunca commitar no Git)

Configure no servidor / `appsettings.Production.json` / secrets do host:

| Variável / chave | Valor |
|------------------|-------|
| `Asaas__Enabled` | `true` |
| `Asaas__ApiKey` | Sua chave API do painel |
| `Asaas__WebhookToken` | Token secreto que você definiu |
| `Asaas__BaseUrl` | Sandbox: `https://sandbox.asaas.com/api/v3` |
| | Produção: `https://api.asaas.com/api/v3` |
| `Asaas__DiasVencimentoPix` | `1` (ou conforme regra comercial) |

**Exemplo local (User Secrets):**

```bash
cd src/PosCorte.API
dotnet user-secrets set "Asaas:Enabled" "true"
dotnet user-secrets set "Asaas:ApiKey" "SUA_CHAVE_AQUI"
dotnet user-secrets set "Asaas:WebhookToken" "seu-token-secreto-webhook"
```

### 4. Configurar webhook no painel Asaas

| Campo | Valor |
|-------|-------|
| URL | `https://SEU_DOMINIO/api/v1/webhooks/asaas` |
| Eventos | `PAYMENT_RECEIVED`, `PAYMENT_CONFIRMED` (conforme disponível) |
| Token | Mesmo valor de `Asaas:WebhookToken` |

O controller valida o header `asaas-access-token`.

**Desenvolvimento local:** use [ngrok](https://ngrok.com) ou similar para expor `localhost:5047` e registrar a URL pública no Asaas Sandbox.

### 5. Aplicar migration no banco

Após clonar/atualizar o código:

```bash
cd src/PosCorte.API
dotnet ef database update
```

Isso cria as tabelas `Pagamentos` e `Liquidacoes`.

### 6. Checklist sandbox → produção

| Etapa | Sandbox | Produção |
|-------|---------|----------|
| BaseUrl | `sandbox.asaas.com/api/v3` | `api.asaas.com/api/v3` |
| ApiKey | Chave sandbox | Chave produção |
| Webhook URL | URL ngrok ou staging | `https://api.poscorte.com.br/...` |
| PIX de teste | Valores simulados no painel | Cobrança real |
| `Enabled` | `true` após testes | `true` |

---

## Fluxo de negócio (escrow)

1. **Orçamento** → projeto `Aguardando_Pagamento`
2. **Gerar PIX** → `Pagamento` status `Aguardando_Pix`
3. **Cliente paga** → webhook Asaas → `Confirmado` → projeto `Aguardando_Provedor`
4. **Montagem concluída** → `LiquidarFundosAsync` → repasse marceneiro (80%) + taxa plataforma (20%)

> **Pendência conhecida:** split automático via API Asaas em `LiquidarFundosAsync` está marcado como TODO — hoje registra `Liquidacao` no banco; integrar split quando conta tiver subcontas configuradas.

---

## Arquivos relevantes no repositório

```
src/PosCorte.Domain/Entities/Pagamento.cs
src/PosCorte.Domain/Entities/Liquidacao.cs
src/PosCorte.API/Configuration/AsaasOptions.cs
src/PosCorte.API/Services/PagamentoService.cs
src/PosCorte.API/Services/Pagamentos/PagamentoConfirmacaoService.cs
src/PosCorte.API/Services/Pagamentos/Asaas/AsaasClient.cs
src/PosCorte.API/Controllers/WebhookAsaasController.cs
src/PosCorte.API/Controllers/ProjetosController.cs      # gerar-pix, pagamento
src/PosCorte.API/Controllers/PagamentosController.cs      # simular (dev)
src/PosCorte.Web/Pages/Projetos/Pagar.cshtml
src/PosCorte.Web/Services/ApiService.cs                   # GerarPixAsync, etc.
```

---

## Segurança

- **Nunca** commite `ApiKey`, `WebhookToken` ou senhas de banco
- **Nunca** use conta Asaas de terceiros em produção
- Webhook manual `pagamento-confirmado` (legado) é **bloqueado** se Asaas estiver configurado em produção
- Endpoint `simular-confirmacao` existe **apenas** em `Development`

---

## Suporte e referências

- [Documentação API Asaas](https://docs.asaas.com/)
- [Webhooks Asaas](https://docs.asaas.com/docs/webhooks)
- Guia interno de webhook legado: `docs/WEBHOOK_GUIDE.md`

---

*Última atualização: junho/2026 — estrutura pronta; aguardando credenciais e CNPJ do titular do PósCorte.*
