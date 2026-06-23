# PósCorte — O que VOCÊ precisa fazer (handoff do fundador)

> **Para:** Renan (fundador)
> **Atualizado:** junho/2026
> **Resumo:** o sistema está **funcional de ponta a ponta em modo simulação** (sem dinheiro real).
> Este documento lista, em um só lugar, **tudo que depende de você** para ligar o negócio de verdade —
> em especial os **dados sensíveis** que não podem ser preenchidos nesta conta compartilhada.

---

## 0. Leitura rápida (1 minuto)

| Bloco | O que falta de você | Bloqueia o quê | Prioridade |
|-------|---------------------|----------------|------------|
| 🔴 Pagamento (Asaas) | CNPJ + conta Asaas + chaves | Receita real | P0 |
| 🔴 Marceneiro (API parceira) | Contrato + credenciais Helpie/Gaba | Montador real | P0 |
| 🟠 Notificações | Conta WhatsApp/e-mail + chaves | Operação automática | P1 |
| 🟠 Deploy + domínio | Hospedagem + `poscorte.com.br` | Cliente externo usar | P1 |
| 🟡 Jurídico/LGPD | CNPJ, termos, contador | Operar legalmente | P0 legal |

> **Importante:** o código **já está pronto** para todos esses blocos. Você só preenche credenciais e liga as chaves (`Enabled: true`). Nenhuma reprogramação é necessária para começar.

---

## 1. 🔴 Pagamento PIX real (Asaas)

**Estado atual:** modo **Stub** — gera PIX falso, nenhuma cobrança real. Em desenvolvimento dá para simular o pagamento inteiro.

**O que só você faz** (detalhes completos em [`INTEGRACAO_PAGAMENTO_ASAAS.md`](INTEGRACAO_PAGAMENTO_ASAAS.md)):

1. Abrir **CNPJ** (não use conta de terceiros).
2. Criar conta **Asaas** no nome da empresa.
3. Pegar a **API Key** no painel Asaas.
4. Definir um **Webhook Token** (segredo que você inventa).
5. Configurar via variáveis de ambiente / secrets (**nunca no Git**):

```
Asaas__Enabled       = true
Asaas__ApiKey        = (sua chave do painel)
Asaas__WebhookToken  = (seu segredo)
Asaas__BaseUrl       = https://api.asaas.com/api/v3   (produção)
```

6. No painel Asaas, cadastrar o webhook:
   `https://SEU_DOMINIO/api/v1/webhooks/asaas`

> Enquanto `Asaas__Enabled=false`, tudo roda em simulação — seguro para testar nesta conta.

---

## 2. 🔴 Marceneiro via API parceira

**Estado atual:** `ProvedorApi` desligado (`Enabled: false`). Sem ele, o projeto pago fica em **"Aguardando_Provedor"** (nenhum montador real é alocado). Você decidiu **não cadastrar marceneiros manualmente** — a alocação vem 100% de um parceiro.

**O que só você faz:**

1. Fechar parceria com **um** provedor de montagem B2B:
   - **Helpie** — `parceria@helpie.com.br`
   - **Rede Gaba** — [redegaba.com.br](https://redegaba.com.br)
2. Pedir **credenciais sandbox + documentação da API** (criar ordem + webhook de status).
3. Configurar:

```
ProvedorApi__Enabled  = true
ProvedorApi__BaseUrl  = https://api.DO_PARCEIRO
ProvedorApi__ApiKey   = (chave do parceiro)
```

4. Ajustar o contrato da API ao parceiro real (hoje é genérico — endpoints `POST /ordensservico` e `GET /ordensservico/{id}`). Se o parceiro usar outro formato, é um ajuste pequeno de dev no `ProvedorService`.
5. Configurar o webhook do parceiro para chamar:
   `https://SEU_DOMINIO/api/v1/webhooks/atualizacao-montador`

> **E-mail pronto para enviar** ao parceiro está no `PLANO_EXECUCAO_COMPLETO.md`, seção 14.6.

---

## 3. 🟠 Notificações (WhatsApp + e-mail)

**Estado atual:** centralizado e pronto, mas em **stub** (só registra em log). Cada evento de negócio já dispara uma notificação no código (pagamento confirmado, montador alocado, montagem concluída, disputa).

**O que só você faz** (detalhes em [`INTEGRACAO_NOTIFICACOES.md`](INTEGRACAO_NOTIFICACOES.md)):

1. Criar conta **Z-API** ou **Twilio** (WhatsApp) e/ou **Resend/SendGrid** (e-mail).
2. Pegar as chaves.
3. Avisar o dev para implementar o envio dentro de `NotificacaoService.NotificarEventoAsync` (ponto único — tudo já chama esse método).

> Sem isso, o sistema funciona; você só não recebe avisos automáticos.

---

## 4. 🟠 Deploy em produção + domínio

**Estado atual:** roda em `localhost`. Banco já é Supabase (nuvem).

**O que só você faz:**

1. Registrar **poscorte.com.br** (ou domínio escolhido).
2. Hospedar **API + Web** (Railway, Render ou Azure — qualquer um com .NET 9).
3. Configurar variáveis de ambiente em produção:

```
ConnectionStrings__DefaultConnection = (Supabase com senha real)
Jwt__Key                             = (segredo forte, 32+ caracteres)
DB_PASSWORD / JWT_SECRET             = conforme host
Asaas__*                             = ver bloco 1
ProvedorApi__*                       = ver bloco 2
```

4. Rodar a migration no banco de produção:

```
cd src/PosCorte.API
dotnet ef database update
```

5. Trocar a senha do admin padrão no primeiro acesso.
6. (Recomendado) Sentry para erros + UptimeRobot para queda.

> **Segurança a revisar antes de produção:** CORS está em `AllowAll` e Swagger só liga em Development (ok). Restrinja o CORS ao domínio final.

---

## 5. 🟡 Jurídico, LGPD e contábil

Sem isso você até cobra, mas opera em risco.

- [ ] **CNPJ** (LTDA — intermediação de serviços) com contador.
- [ ] **Conta PJ** em banco digital.
- [ ] **Termos de Uso + Política de Privacidade** publicados (exigência de gateway e LGPD).
- [ ] **Contador** alinhado: ISS, NF de serviço da plataforma, tratamento do split com terceiros.

---

## 6. O que JÁ ESTÁ PRONTO (não precisa mexer)

Para você ter clareza do que o sistema entrega hoje, em simulação:

- ✅ Cadastro/login de arquiteto (JWT) e painel admin
- ✅ Criar projeto + orçamento instantâneo (R$ 12,50/peça + R$ 40/gaveta, markup 20%)
- ✅ **Estimador público de orçamento** na landing (sem login — capta lead)
- ✅ Geração de PIX (stub/Asaas) com QR + copia-e-cola + tela de pagamento
- ✅ Escrow: retenção dos fundos ao confirmar pagamento
- ✅ Criação automática de ordem de serviço (provedor ou local)
- ✅ **Fluxo de vistoria completo**: arquiteto aprova montagem → libera escrow → projeto concluído
- ✅ **Abrir disputa** → congela o valor retido
- ✅ **Liquidação automática em 72h** (job em segundo plano) se o arquiteto não se manifestar
- ✅ Split 80/20 registrado (marceneiro/plataforma) — *execução real do split depende do Asaas (bloco 1)*
- ✅ Stepper visual de progresso do projeto
- ✅ Breakdown de preço “a taxa é seu seguro” (psicologia de conversão)
- ✅ Notificações centralizadas por evento (stub pronto p/ ligar)
- ✅ Identidade visual nova (ícone nuvem + serra), fontes modernas, landing focada em conversão
- ✅ Segurança: cada arquiteto vê só os próprios projetos
- ✅ 26 testes automatizados passando

---

## 7. Como testar o ciclo completo hoje (sem dinheiro real)

1. Suba a API (`http://localhost:5047`) e o Web (`http://localhost:5197`).
2. Login admin: `admin@poscorte.com` / `Admin@PosCorte2026`.
3. Crie uma conta de arquiteto e um projeto.
4. **Pagar com PIX** → **Simular pagamento (dev)** → ordem criada.
5. Na tela do projeto (dev): **simular montagem concluída** → entra em vistoria.
6. **Aprovar montagem** → escrow liberado, projeto **Concluído**.
   - Ou **Abrir disputa** → fica retido.
   - Ou não fazer nada → o job libera sozinho após 72h.

---

## 8. Ordem recomendada de execução

```
1. CNPJ + conta PJ + Asaas        (bloco 1 e 5)  ← destrava receita
2. Parceria marceneiro            (bloco 2)      ← destrava operação
3. Deploy + domínio               (bloco 4)      ← cliente externo usa
4. Notificações                   (bloco 3)      ← reduz trabalho manual
5. Vender (PLANO_EXECUCAO §14)    ← onde mora o dinheiro
```

> Regra de ouro do plano: **não adicione feature nova até o primeiro PIX de um cliente que você não conhece.**

---

*Documentos relacionados:*
- *[`INTEGRACAO_PAGAMENTO_ASAAS.md`](INTEGRACAO_PAGAMENTO_ASAAS.md) — passo a passo do gateway*
- *[`INTEGRACAO_NOTIFICACOES.md`](INTEGRACAO_NOTIFICACOES.md) — WhatsApp/e-mail*
- *[`ANALISE_MERCADO_POSICIONAMENTO.md`](ANALISE_MERCADO_POSICIONAMENTO.md) — concorrência e diferencial*
- *`../PLANO_EXECUCAO_COMPLETO.md` — plano de negócio e go-to-market*
