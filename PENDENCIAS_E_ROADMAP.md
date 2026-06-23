# PósCorte — Pendências e Roadmap para Escala

> **Documento vivo** · Atualizado em junho/2026  
> **Objetivo:** listar tudo que falta para sair do protótipo funcional e virar um SaaS + marketplace que fatura de verdade.  
> **Meta de referência (manual):** 300 projetos/mês na Grande SP → ~R$ 180.000/mês de margem líquida (20% take rate).

---

## 🆕 Atualização (polimento junho/2026)

Itens que **passaram de pendentes a prontos** nesta rodada de polimento (em modo simulação até ligar credenciais):

- ✅ **Estrutura de pagamento PIX** (Asaas + stub) — entidade `Pagamento`, gerar PIX, webhook, tela de pagamento.
- ✅ **Escrow** — fundos retidos ao confirmar pagamento.
- ✅ **Fluxo de vistoria completo** — arquiteto aprova montagem ou abre disputa.
- ✅ **Liquidação automática 72h** — `BackgroundService` libera o escrow por prazo.
- ✅ **Split 80/20 registrado** (`Liquidacao`) — execução real depende do Asaas.
- ✅ **Status `Concluido` / `Em_Disputa`** ligados ao fluxo real.
- ✅ **Tela "Pagar com PIX"** + breakdown de preço ancorado ("a taxa é seu seguro").
- ✅ **Notificações centralizadas** por evento (stub pronto para WhatsApp/e-mail).
- ✅ **Segurança** — cada arquiteto vê só os próprios projetos.
- ✅ **UI/branding** — ícone nuvem+serra, fontes modernas, landing focada em conversão + **estimador público**.

➡️ **O que ainda depende de você está consolidado em [`docs/ACOES_NECESSARIAS.md`](docs/ACOES_NECESSARIAS.md).**
Os itens P1/P2/P3 abaixo seguem válidos como roadmap de evolução.

---

## 1. Onde estamos hoje (baseline honesto)

### ✅ O que já funciona

| Área | Status | Detalhe |
|------|--------|---------|
| **API REST (.NET 9)** | Pronto | JWT, Swagger, EF Core, migrations automáticas, Serilog |
| **Banco PostgreSQL (Supabase)** | Pronto | Tabelas: usuarios, projetos, ordens_servico, marceneiros, avaliacoes |
| **Web Arquiteto (Razor)** | Pronto | Landing, login, cadastro, dashboard, projetos, ordens |
| **Web Admin (sua área)** | Pronto | Painel, financeiro (explicativo), projetos, rede de marceneiros |
| **Motor de precificação** | Pronto | Markup inverso 20%: R$ 12,50/peça + R$ 40/gaveta |
| **Fluxo de status** | Pronto | Aguardando_Pagamento → … → Aguardando_Vistoria |
| **Rede de marceneiros (demo)** | Parcial | Cadastro + avaliações + alocação automática |
| **Testes unitários** | Pronto | 27 testes passando |
| **Autenticação** | Pronto | BCrypt + JWT (API) + Cookie (Web) |
| **Admin seed** | Pronto | `admin@poscorte.com` / `Admin@PosCorte2026` |

### ⚠️ O que parece funcionar mas é SIMULAÇÃO

| Item | Realidade hoje | Risco |
|------|----------------|-------|
| **Pagamento PIX** | `PagamentoService` sempre retorna `true` — **não cobra nada** | Zero receita real |
| **Escrow (retenção)** | Simulado — nenhum dinheiro retido | Sem garantia ao arquiteto |
| **Split 80/20** | Não executado — só calculado na tela | Marceneiro não recebe, você não recebe |
| **Marceneiros importados** | API `randomuser.me` = **perfis fictícios** (nome/foto demo) | Não são profissionais reais |
| **Provedor externo (Helpie etc.)** | URL fake — falha silenciosa, usa rede interna | OK por enquanto (rede própria) |
| **Notificações** | Stub — só loga no console | Ninguém é avisado de nada |
| **Liquidação 72h** | TODO no código — não roda | Fundos ficariam retidos para sempre |

**Conclusão:** temos um **MVP de demonstração** com fluxo de negócio desenhado, mas **nenhuma transação financeira real** acontece ainda.

---

## 2. Modelo de negócio (como o dinheiro deve fluir)

```
Arquiteto paga R$ 3.000 (preço cheio, via PIX na plataforma)
        │
        ▼
   [ ESCROW — dinheiro retido ]
        │
        ├── 80% → Marceneiro (R$ 2.400) — só após serviço + vistoria OK
        └── 20% → PósCorte / você (R$ 600) — sua margem
```

| Escopo (manual) | Preço ao arquiteto | Repasse marceneiro (80%) | Sua margem (20%) |
|-----------------|-------------------|--------------------------|------------------|
| Dormitório / Home Office | R$ 1.500 | R$ 1.200 | R$ 300 |
| Cozinha média | R$ 3.000 | R$ 2.400 | R$ 600 |
| Apto compacto integral | R$ 6.000 | R$ 4.800 | R$ 1.200 |

**Escala alvo:** 300 projetos/mês × margem média ~R$ 600 = **~R$ 180.000/mês líquido**.

---

## 3. Pendências — priorizadas por impacto no faturamento

### 🔴 P0 — Bloqueia receita (fazer primeiro)

Estes itens impedem qualquer real de entrar. Sem eles, o software é demo eterna.

#### 3.1 Gateway de pagamento PIX real (Asaas ou Iugu)

- [ ] Abrir conta PJ no gateway escolhido (Asaas recomendado: PIX + split nativo)
- [ ] Criar entidade `Pagamento` no banco (projetoId, pixId, valor, status, qrCode, dataExpiracao)
- [ ] Endpoint `POST /projetos/{id}/gerar-pix` — gera cobrança PIX com valor do orçamento
- [ ] Tela no Web: botão **"Pagar com PIX"** + QR Code + copia-e-cola
- [ ] Webhook real do gateway → substituir stub do `PagamentoService`
- [ ] Validar assinatura do webhook (secret) — segurança
- [ ] Status do projeto: `Aguardando_Pagamento` → `Pagamento_Confirmado` só após webhook real

**Estimativa:** 3–5 dias de dev + 1–2 dias homologação sandbox.

#### 3.2 Escrow + Split automático (80/20)

- [ ] Após PIX confirmado: marcar fundos como `Retidos`
- [ ] Calcular e persistir: `valorTotal`, `valorMarceneiro` (80%), `valorPlataforma` (20%)
- [ ] Após vistoria OK (ou 72h sem disputa): executar split via API do gateway
- [ ] Registrar `Liquidacao` no banco (data, valores, ids externos)
- [ ] Admin: painel financeiro com transações reais (não estimadas)

**Estimativa:** 2–3 dias (depende do gateway escolhido).

#### 3.3 Fluxo de pagamento no Web do arquiteto

- [ ] Página `Projetos/Pagar` — exibe orçamento + QR PIX
- [ ] Countdown de expiração do PIX
- [ ] Feedback visual: "Pagamento confirmado" em tempo real (polling ou SignalR)
- [ ] Bloquear criação de ordem/montador antes do PIX cair

**Estimativa:** 1–2 dias.

---

### 🟠 P1 — Produto mínimo vendável (segunda onda)

Sem isso, até cobrando PIX, a operação quebra na prática.

#### 3.4 Portal / App do Marceneiro (sistema separado)

O marceneiro **não usa** o sistema do arquiteto. Precisa de área própria:

- [ ] Cadastro de marceneiro real (CPF, documentos, especialidades, região)
- [ ] Login com role `Marceneiro`
- [ ] Lista de **oportunidades** com valor **dele** (80%), endereço, data, peças/gavetas
- [ ] Botões **Aceitar** / **Recusar**
- [ ] Ver plano de corte (PDF/link) após aceitar
- [ ] Marcar serviço como **Concluído**
- [ ] Histórico de serviços + total recebido
- [ ] (Futuro) App mobile React Native ou PWA

**Estimativa:** 1–2 semanas (Web Razor primeiro; app depois).

#### 3.5 Substituir marceneiros demo por rede real

- [ ] Remover dependência de `randomuser.me` em produção
- [ ] Processo de **homologação** (Admin aprova/rejeita marceneiro)
- [ ] Upload de documentos (RG, comprovante, fotos de trabalhos)
- [ ] Badge "Verificado" só após análise manual ou automática
- [ ] Onboarding: marceneiro preenche perfil → Admin aprova → entra na fila de alocação

**Estimativa:** 1 semana.

#### 3.6 Alocação inteligente (melhorar o algoritmo atual)

Hoje: escolhe melhor nota + mesma cidade. Falta:

- [ ] Matching por **CEP/região** (não só nome da cidade)
- [ ] Matching por **especialidade** (cozinha, dormitório…)
- [ ] Respeitar **disponibilidade** e carga (marceneiro ocupado = skip)
- [ ] Timeout: se marceneiro não aceitar em X horas → realocar para o próximo
- [ ] Fila de convites (push) em vez de alocação direta

**Estimativa:** 3–5 dias.

#### 3.7 Vistoria e disputa (72h úteis)

- [ ] Tela arquiteto: **"Aprovar montagem"** ou **"Abrir disputa"**
- [ ] Job agendado (Hangfire ou Quartz): após 72h úteis sem ação → liquida split
- [ ] Fluxo de disputa: congelar split, Admin media
- [ ] Estados: `Aguardando_Vistoria` → `Concluido` ou `Em_Disputa`

**Estimativa:** 1 semana.

---

### 🟡 P2 — Confiança, escala e operação

#### 3.8 Notificações reais

- [ ] **WhatsApp** (Twilio ou Z-API): marceneiro recebe oportunidade, arquiteto recebe confirmação
- [ ] **E-mail** (SendGrid/Resend): recibos, status de projeto
- [ ] **SMS** backup para marceneiro
- [ ] Templates por evento (pagamento OK, montador alocado, serviço concluído)

**Estimativa:** 3–5 dias.

#### 3.9 Upload de arquivo de corte

Hoje: arquiteto cola URL manual. Ideal:

- [ ] Upload direto para **Supabase Storage** ou Azure Blob
- [ ] Validação de tipo (PDF, ZIP Corte Cloud)
- [ ] Injetar link automaticamente no perfil do marceneiro alocado

**Estimativa:** 2–3 dias.

#### 3.10 Avaliações anti-fraude

- [ ] Só arquiteto com ordem **Concluida** pode avaliar
- [ ] Uma avaliação por projeto (unique constraint)
- [ ] Admin pode moderar/remover avaliações abusivas

**Estimativa:** 1–2 dias.

#### 3.11 Admin completo

- [ ] Gestão de usuários (ativar/desativar arquitetos)
- [ ] Aprovar/reprovar marceneiros
- [ ] Ver disputas e intervir
- [ ] Relatório financeiro real (não estimado)
- [ ] Export CSV/Excel para contabilidade
- [ ] Dashboard com gráficos (projetos/mês, receita/mês)

**Estimativa:** 1 semana.

#### 3.12 Segurança e compliance

- [ ] CORS restrito ao domínio de produção
- [ ] Rate limiting nos endpoints públicos (auth, webhooks)
- [ ] Secrets em variáveis de ambiente (nunca no código)
- [ ] HTTPS obrigatório em produção
- [ ] LGPD: termos de uso, política de privacidade, consentimento
- [ ] Logs de auditoria (quem fez o quê)

**Estimativa:** 3–5 dias.

---

### 🟢 P3 — Diferenciais competitivos (escala milionária)

#### 3.13 Integração Corte Cloud

- [ ] Parser do arquivo de paginação → extrair `qtdPecas` e `qtdGavetas` automaticamente
- [ ] Orçamento instantâneo sem digitar manualmente
- [ ] Parceria comercial com Corte Cloud (canal de aquisição)

**Estimativa:** 2–4 semanas (depende de API/acesso ao formato).

#### 3.14 Parceria Helpie (opcional)

- [ ] Contato comercial: parceria@helpie.com.br
- [ ] Se fechar: integrar API OAuth2 deles como **reforço** da rede própria
- [ ] Não depender exclusivamente — rede própria é o ativo

**Estimativa:** comercial (2–3 meses de negociação).

#### 3.15 App mobile

- [ ] PWA ou React Native para marceneiro (campo)
- [ ] Push notification nativo
- [ ] Geolocalização para check-in/check-out na obra

**Estimativa:** 3–6 semanas.

#### 3.16 Inteligência de precificação

- [ ] Precificação dinâmica por região (SP capital vs interior)
- [ ] Sazonalidade / demanda
- [ ] Ajuste de take rate por volume (desconto para parceiros)

**Estimativa:** contínuo.

---

## 4. Go-to-market (do manual — ainda não implementado)

Estas frentes são **comerciais**, não código, mas são essenciais para os 300 projetos/mês:

| Frente | Ação | Status |
|--------|------|--------|
| **Parceria balcão MDF** | Bonificar vendedores de distribuidoras por indicação (Pix por lead) | ❌ Não iniciado |
| **Marketing de dor** | Criativos: "Suba seu projeto, pague seguro, montador homologado" | ❌ Landing existe, ads não |
| **Infiltração orgânica** | Links em grupos Promob, SketchUp, fóruns de corte | ❌ Não iniciado |
| **Programa de indicação** | Arquiteto indica arquiteto → crédito/desconto | ❌ Não no sistema |
| **SEO local** | "Montagem móveis planejados São Paulo" | ❌ Não otimizado |

### Pendências de produto para GTM

- [ ] Landing page com CTA de conversão + pixel Meta/Google
- [ ] Formulário "Indique um arquiteto" com tracking
- [ ] Cupom de desconto na primeira montagem
- [ ] Página pública de depoimentos / cases
- [ ] WhatsApp Business integrado no site

---

## 5. Infraestrutura e deploy (produção)

| Item | Status | Pendência |
|------|--------|-----------|
| Hospedagem API | ❌ | Azure App Service, Railway ou Render |
| Hospedagem Web | ❌ | Mesmo provider, domínio poscorte.com.br |
| Banco Supabase | ✅ | Já funciona — plano pago se escalar |
| CI/CD GitHub Actions | ⚠️ | Pipeline existe — validar deploy automático |
| Domínio + SSL | ❌ | Registrar poscorte.com.br |
| Monitoramento | ❌ | Sentry (erros) + UptimeRobot |
| Backup automático | ⚠️ | Supabase faz — documentar restore |
| Ambiente staging | ❌ | Branch `staging` + banco separado |

**Checklist deploy:**

- [ ] Comprar domínio
- [ ] Configurar `DB_PASSWORD`, `JWT_SECRET`, chaves Asaas em env vars
- [ ] Restringir CORS para domínio de produção
- [ ] Desligar Swagger em produção (ou proteger com auth)
- [ ] Remover senha admin default — forçar troca no primeiro login

---

## 6. Roadmap sugerido (12 semanas)

```
Semana 1–2   │ P0: PIX Asaas + tela pagar + webhook real
Semana 3     │ P0: Escrow + split 80/20 + entidade Pagamento
Semana 4–5   │ P1: Portal Marceneiro (aceitar/recusar/concluir)
Semana 6     │ P1: Homologação marceneiro real + Admin aprovar
Semana 7     │ P1: Vistoria 72h + Hangfire liquidação
Semana 8     │ P2: WhatsApp + e-mail notificações
Semana 9     │ P2: Upload arquivo corte + segurança produção
Semana 10    │ Deploy produção + domínio + SSL
Semana 11–12 │ GTM: parcerias MDF + ads + primeiros 10 clientes reais
```

**Marco "primeiro real":** arquiteto paga PIX de verdade → marceneiro real aceita → serviço concluído → split executado.

**Marco "escala":** 30 projetos/mês (R$ ~18.000 margem) → iterar → 300/mês.

---

## 7. Decisões que só você (fundador) pode tomar

| # | Decisão | Opções | Recomendação |
|---|---------|--------|--------------|
| 1 | Gateway de pagamento | Asaas vs Iugu vs Mercado Pago | **Asaas** (split nativo, API madura) |
| 2 | Banco em produção | Manter Supabase vs SQL Server Azure | **Supabase** (já funciona, escala bem) |
| 3 | Marceneiros | Rede própria vs Helpie vs híbrido | **Rede própria** (ativo da empresa) |
| 4 | App marceneiro | PWA vs React Native vs WhatsApp-only MVP | **WhatsApp MVP** primeiro, app depois |
| 5 | Região de lançamento | SP capital vs Grande SP vs BR | **Grande SP** (concentração de arquitetos) |
| 6 | Take rate | 20% fixo vs variável | **20% fixo** (simplicidade) |
| 7 | CNPJ / conta PJ | Precisa para Asaas e NF | **Urgente** — sem PJ não recebe PIX |

---

## 8. Métricas para acompanhar (quando estiver em produção)

| Métrica | Meta mês 1 | Meta mês 6 | Meta escala |
|---------|------------|------------|-------------|
| Projetos criados | 10 | 100 | 300/mês |
| Taxa conversão (projeto → PIX pago) | 30% | 50% | 60% |
| Tempo médio alocação marceneiro | < 4h | < 2h | < 1h |
| NPS arquitetos | > 7 | > 8 | > 9 |
| Margem líquida/mês | R$ 3.000 | R$ 60.000 | R$ 180.000 |
| Marceneiros ativos na rede | 20 | 100 | 500 |
| Avaliação média marceneiros | > 4.5 | > 4.7 | > 4.8 |

---

## 9. Resumo executivo (1 parágrafo)

O PósCorte tem **arquitetura sólida**, **fluxo de negócio desenhado** e **interfaces funcionais** para arquiteto e admin. Porém, **pagamento, escrow, split, marceneiros reais e notificações ainda são simulação**. O caminho para faturar é: **(1)** integrar PIX real, **(2)** portal do marceneiro, **(3)** deploy em produção, **(4)** GTM em SP. Estimativa: **8–10 semanas de dev** + **CNPJ/conta Asaas** para o primeiro real.

---

## 10. Próxima ação recomendada

> **Integrar Asaas (PIX + webhook + split)** — é o único item que transforma demo em negócio.

Depois disso: **portal do marceneiro** para fechar o ciclo operacional.

---

*Arquivo gerado com base no estado real do repositório PosCorte em junho/2026.*
