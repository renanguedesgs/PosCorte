# PósCorte — Plano de Execução Completo

> **Para:** Renan (fundador)  
> **Atualizado:** junho/2026  
> **Objetivo deste documento:** lista **tudo** que falta — técnico, jurídico, comercial e operacional — para transformar o protótipo em um negócio que **pode** chegar a **R$ 1 milhão+/mês** de margem (ou valuation de dezenas de milhões).  
> **Leitura honesta:** software sozinho não faz milionário. **PIX real + operação + aquisição de clientes** fazem. Este doc é o mapa.

---

## 1. A conta do milhão (para você não se perder)

### 1.1 Meta do manual de arquitetura

| Cenário | Projetos/mês | Margem média (20%) | Receita líquida plataforma/mês |
|---------|--------------|--------------------|--------------------------------|
| Validação | 30 | R$ 600 | **R$ 18.000** |
| Tração | 100 | R$ 600 | **R$ 60.000** |
| Escala SP | 300 | R$ 600 | **R$ 180.000** |
| “Milionário operacional” | **~1.700** | R$ 600 | **~R$ 1.000.000** |

> **1.700 projetos/mês** na Grande SP é agressivo, mas não impossível com marketplace + parcerias MDF + marca forte. É o horizonte de **2–5 anos**, não de 2 meses.

### 1.2 Duas formas de “ficar milionário”

| Caminho | O que é | O que precisa |
|---------|---------|---------------|
| **A — Caixa (lucro)** | R$ 1M/mês entrando na sua conta PJ | Volume absurdo de projetos + operação redonda |
| **B — Patrimônio (equity)** | Empresa valendo R$ 30M–100M+ | Tração comprovada (ex.: R$ 60k–180k/mês), dados, retenção, narrativa B2B |

**Recomendação:** mirar primeiro **R$ 18k/mês** (30 projetos), depois **R$ 60k**, depois **R$ 180k**. Milhão é consequência de escala + time + capital — não de mais uma feature.

---

## 2. Onde o PósCorte está HOJE (junho/2026)

### ✅ Já construído (vantagem real)

- API .NET 9 + PostgreSQL (Supabase) + migrations automáticas
- Web completa: landing, auth arquiteto, dashboard, projetos, ordens
- Painel Admin: KPIs, financeiro explicativo, projetos, rede
- Motor de preço: R$ 12,50/peça + R$ 40/gaveta + markup 20%
- Fluxo de status do projeto (da criação até vistoria)
- JWT + roles (Arquiteto / Admin)
- Integração **preparada** com API externa de marceneiros (`ProvedorApi`)
- Logo e identidade visual no Web
- **30 testes** passando

### ❌ Ainda NÃO gera dinheiro (bloqueadores)

| # | Item | Situação atual | Impacto |
|---|------|----------------|---------|
| 1 | **PIX real** | Stub — sempre “aprovado”, zero cobrança | **Zero receita** |
| 2 | **Escrow + split 80/20** | Só na tela / lógica simulada | Ninguém recebe de verdade |
| 3 | **Provedor de marceneiros** | `ProvedorApi` desligado (`Enabled: false`) — precisa parceria Helpie ou Rede Gaba | Ordens ficam `Aguardando_Provedor` |
| 4 | **Botão “Pagar PIX”** | Não existe no Web | Arquiteto não consegue pagar sozinho |
| 5 | **Notificações** | Só log no console | Operação morre no WhatsApp manual |
| 6 | **Liquidação 72h** | TODO no código | Dinheiro nunca seria liberado |
| 7 | **Produção** | Só localhost | Ninguém de fora usa |
| 8 | **CNPJ + conta gateway** | Pendente (sua ação) | Sem PJ não tem PIX comercial |

**Verdade em uma frase:** você tem um **MVP de demonstração excelente**, mas **0 transações reais** até hoje.

---

## 3. Checklist mestre — o que falta executar

Marque conforme for fazendo. Ordem sugerida: **Fase 0 → 1 → 2 → 3**.

---

### FASE 0 — Fundação legal e financeira (VOCÊ, 1–2 semanas)

Sem isso, nada abaixo vale dinheiro.

- [ ] **Abrir CNPJ** (LTDA ou MEI se couber — consulte contador para marketplace)
- [ ] **Conta PJ** em banco digital (Inter, C6, etc.)
- [ ] **Abrir conta Asaas** (recomendado: PIX + split + API madura)  
      Alternativa: Iugu, Mercado Pago
- [ ] **Enviar documentos** e passar homologação sandbox do gateway
- [ ] **Definir conta de repasse** para marceneiros (via gateway ou provedor parceiro)
- [ ] **Contrato social / termos** com advogado (intermediação de serviços)
- [ ] **LGPD mínima:** Política de Privacidade + Termos de Uso publicados no site
- [ ] **Contador** alinhado: ISS, NF de serviço da plataforma, split com terceiros

**Custo estimado:** R$ 2.000–8.000 (abertura + contador + advogado básico)  
**Prazo:** 7–15 dias úteis

---

### FASE 1 — Primeiro real (dinheiro entrando) — 3–4 semanas dev

**Meta:** 1 arquiteto paga PIX de verdade → marceneiro real vai na obra → você fica com 20%.

#### 1.1 Pagamento PIX (P0 — crítico)

- [ ] Entidade `Pagamento` no banco (projetoId, externalId, valor, status, qrCode, expiraEm)
- [ ] `POST /api/v1/projetos/{id}/gerar-pix` → chama API Asaas
- [ ] Tela Web **“Pagar com PIX”** (QR Code + copia-e-cola + countdown)
- [ ] Webhook Asaas `PAYMENT_RECEIVED` → substituir stub do `PagamentoService`
- [ ] Validar assinatura do webhook (secret)
- [ ] Projeto só avança após webhook **real** (não simulado)
- [ ] Remover/desabilitar stub em produção

**Responsável:** dev  
**Prazo:** 5–7 dias

#### 1.2 Escrow + split 80/20 (P0)

- [ ] Ao confirmar PIX: persistir `valorTotal`, `valorMarceneiro` (80%), `valorPlataforma` (20%)
- [ ] Status `Fundos_Retidos` até vistoria
- [ ] Após vistoria OK (ou 72h sem disputa): `POST split` no Asaas
- [ ] Entidade `Liquidacao` (data, ids externos, valores)
- [ ] Admin: financeiro com **dados reais**, não estimativa

**Prazo:** 3–5 dias (após PIX funcionando)

#### 1.3 Marceneiros reais — API parceira (P0 comercial + 2 dias dev)

Você decidiu: **marceneiro NÃO usa o PósCorte** — ele usa a plataforma do parceiro.

- [ ] **Escolher parceiro** (uma opção):
  - **Rede Gaba** — [redegaba.com.br](https://redegaba.com.br) — API-first, montagem planejada
  - **Helpie B2B** — `parceria@helpie.com.br` — REST + OAuth 2.0
- [ ] Solicitar **credenciais sandbox + documentação API**
- [ ] Configurar em produção:
  ```json
  "ProvedorApi": {
    "BaseUrl": "https://api.DO_PARCEIRO",
    "ApiKey": "SUA_CHAVE",
    "Enabled": true
  }
  ```
- [ ] Ajustar `IProvedorApi` aos endpoints reais do parceiro (hoje é genérico)
- [ ] Webhook do parceiro → `POST /api/v1/webhooks/atualizacao-montador`
- [ ] Testar: pagamento → ordem criada no parceiro → montador alocado → status atualiza

**Prazo comercial:** 2–8 semanas (negociação)  
**Prazo técnico após credencial:** 2–4 dias

#### 1.4 Vistoria + liquidação 72h (P1)

- [ ] Tela arquiteto: **“Aprovar montagem”** / **“Abrir disputa”**
- [ ] Hangfire ou Quartz: job diário — projetos em `Aguardando_Vistoria` há 72h úteis → liquida
- [ ] Disputa: congela split, Admin resolve manualmente

**Prazo:** 5–7 dias

#### 1.5 Deploy produção (P0 para vender)

- [ ] Registrar **poscorte.com.br** (ou domínio escolhido)
- [ ] Hospedar API + Web (Azure, Railway, Render — qualquer um com .NET 9)
- [ ] SSL automático (HTTPS)
- [ ] Variáveis de ambiente: `DB_PASSWORD`, `JWT_SECRET`, `ASAAS_API_KEY`, `ProvedorApi__*`
- [ ] CORS só para domínio de produção
- [ ] Swagger desligado ou protegido em produção
- [ ] Trocar senha admin padrão no primeiro login
- [ ] Monitoramento: Sentry (erros) + UptimeRobot (queda)

**Prazo:** 2–3 dias

---

### FASE 2 — Produto vendável (retenção e escala) — 4–6 semanas

#### 2.1 Notificações (sem isso a operação é manual)

- [ ] **WhatsApp** (Z-API ou Twilio): marceneiro/oportunidade, arquiteto/confirmação
- [ ] **E-mail** (Resend/SendGrid): recibo, status, boas-vindas
- [ ] Templates por evento (pagamento OK, montador alocado, concluído, disputa)

**Prazo:** 3–5 dias

#### 2.2 Upload de arquivo de corte

- [ ] Upload para Supabase Storage (hoje é URL manual)
- [ ] Validar PDF/ZIP
- [ ] Link automático na ordem para o montador (via provedor)

**Prazo:** 2–3 dias

#### 2.3 Melhorias de produto

- [ ] Parser Corte Cloud → peças/gavetas automáticas (diferencial enorme)
- [ ] Avaliação só após serviço concluído (anti-fraude)
- [ ] Admin: aprovar disputas, export financeiro CSV
- [ ] Polling ou SignalR: “PIX confirmado” em tempo real na tela

**Prazo:** 2–4 semanas (parser é o mais longo)

---

### FASE 3 — Go-to-market (onde mora o milhão) — contínuo, começar na semana 6

Software sem clientes = R$ 0. Estas ações têm **mais impacto no milhão** do que mais código depois da Fase 1.

#### 3.1 Primeiros 10 clientes (manual, intensivo)

- [ ] Lista de **50 arquitetos** em SP (Instagram, LinkedIn, grupos Promob)
- [ ] Abordagem 1:1: “Pague seguro, montador homologado, 20% de taxa, você não corre atrás de marceneiro”
- [ ] **Fazer 3 projetos piloto** com desconto ou taxa zero (só para case + depoimento)
- [ ] Gravar **vídeo depoimento** de 1 arquiteto satisfeito
- [ ] Colocar na landing

**Meta 30 dias:** 10 projetos criados, 3 PIX pagos

#### 3.2 Parceria balcão MDF (alavanca de volume)

Distribuidoras (MDF, ferragens) têm contato com **centenas de arquitetos**.

- [ ] Mapear 10 distribuidoras na Grande SP
- [ ] Proposta: “Seu cliente compra MDF → você indica PósCorte para montagem → você ganha R$ X por lead fechado”
- [ ] Material PDF para o vendedor de balcão (1 página, dor + QR code)
- [ ] Rastrear indicações (cupom `BALCAO-NOME`)

**Meta 90 dias:** 2 parcerias ativas

#### 3.3 Marketing pago (depois de 3 cases reais)

- [ ] Pixel Meta + Google Ads na landing
- [ ] Campanha: “Acabou o pânico depois do corte do MDF”
- [ ] Público: arquitetos, designers, marcenarias de SP
- [ ] Orçamento inicial: R$ 3.000–5.000/mês — **só depois** de conversão comprovada orgânica

#### 3.4 Orgânico e comunidade

- [ ] Posts em grupos: Promob, SketchUp, corte CNC, arquitetura SP
- [ ] Conteúdo: “Quanto custa montar uma cozinha planejada em SP?” (SEO)
- [ ] Programa indicação: arquiteto indica arquiteto → R$ 100 crédito

#### 3.5 Expansão geográfica (só após SP funcionar)

- [ ] Replicar playbook: RJ, Curitiba, BH
- [ ] Marceneiros/provedor por região

---

## 4. Cronograma executivo (12 semanas)

```
SEMANA 1–2   │ CNPJ + Asaas + contratos
             │ Dev: PIX + tela pagar + webhook
SEMANA 3     │ Dev: Escrow + split + deploy staging
             │ Comercial: e-mail Helpie + Rede Gaba (parceria API)
SEMANA 4     │ Dev: integração ProvedorApi (quando tiver chave)
             │ Dev: vistoria 72h + Hangfire
             │ GTM: lista 50 arquitetos + 10 abordagens
SEMANA 5     │ Deploy produção + domínio
             │ 3 projetos piloto com desconto
SEMANA 6     │ Notificações WhatsApp + upload arquivo
             │ Primeiro PIX REAL de cliente não-fundador 🎯
SEMANA 7–8   │ Iterar feedback + corrigir atrito
             │ 10 projetos / 5 PIX pagos
SEMANA 9–10  │ Parceria balcão MDF (1ª fechada)
             │ Ads com case real
SEMANA 11–12 │ Meta: 30 projetos/mês run-rate
             │ R$ ~18.000 margem/mês
```

**Marco “negócio real”:** Semana 6 — PIX de terceiro + montador no local + split executado.  
**Marco “empresa”:** Semana 12 — 30+ projetos/mês repetível.

---

## 5. Decisões que SÓ VOCÊ toma (hoje)

| # | Decisão | Opções | Recomendação |
|---|---------|--------|--------------|
| 1 | Gateway | Asaas / Iugu / MP | **Asaas** |
| 2 | Marceneiros | Rede Gaba / Helpie / rede própria | **Gaba ou Helpie** (sem cadastrar marceneiro) |
| 3 | Região | SP capital / Grande SP / BR | **Grande SP** primeiro |
| 4 | Take rate | 20% fixo / variável | **20% fixo** (simples) |
| 5 | Piloto | Taxa zero / 10% / preço cheio | **Taxa zero nos 3 primeiros** (troca por case) |
| 6 | Foco seu | Dev / vendas / parcerias | **50% vendas + parcerias** após Fase 1 dev |

---

## 6. Métricas — painel que você deve olhar toda semana

| Métrica | Semana 6 | Mês 3 | Mês 12 |
|---------|----------|-------|--------|
| Projetos criados | 5 | 40 | 300 |
| Taxa projeto → PIX pago | 20% | 40% | 55% |
| PIX pagos/mês | 1 | 15 | 165 |
| Margem plataforma/mês | R$ 600 | R$ 9.000 | R$ 99.000 |
| Tempo médio alocação montador | < 24h | < 4h | < 2h |
| NPS arquitetos | — | > 7 | > 8 |
| CAC (custo aquisição) | R$ 0 (manual) | < R$ 150 | < R$ 80 |

---

## 7. Riscos que derrubam o plano (e como evitar)

| Risco | Consequência | Mitigação |
|-------|--------------|-----------|
| Ficar só codando | Demo eterna, R$ 0 | Trava: **não codar Fase 2 antes de 1 PIX real** |
| Sem CNPJ/Asaas | Não cobra | **Semana 1 = jurídico/financeiro** |
| Parceria marceneiro demora | Pagou mas ninguém monta | Fechar **Gaba ou Helpie** em paralelo ao PIX; piloto manual se precisar |
| Arquiteto não confia | Não paga | Escrow visível + cases + “pagamento só após alocar montador homologado” |
| Margem comida por suporte | Lucro some | WhatsApp automatizado + FAQ + Admin enxuto |
| Concorrente copia | Corrida de preço | Rede de marceneiros + marca + parcerias MDF (moat operacional) |

---

## 8. O que NÃO fazer agora (armadilhas)

- ❌ App mobile do marceneiro (parceiro resolve)
- ❌ Cadastrar marceneiros manualmente (você já disse que não quer)
- ❌ Microserviços, Kubernetes, IA no pricing — **prematuro**
- ❌ Expandir para todo Brasil antes de SP funcionar
- ❌ Reduzir take rate para 10% “para crescer” (mata margem sem volume)
- ❌ Mais features na landing sem cliente pagante

---

## 9. Stack de execução diária (ritual do fundador)

### Segunda
- Olhar dashboard Admin: projetos, status, erros
- 5 abordagens a arquitetos (DM, WhatsApp, ligação)

### Quarta
- Follow-up de quem criou projeto mas não pagou
- 1 ação comercial (distribuidora, parceiro, conteúdo)

### Sexta
- Revisar métricas da semana (tabela §6)
- Decidir **uma** prioridade técnica para a semana seguinte

---

## 10. Próximas 3 ações (comece amanhã)

1. **Abrir CNPJ + conta Asaas** (se ainda não tiver)  
2. **E-mail para Rede Gaba e Helpie** pedindo API de parceiro marketplace B2B  
3. **Dev:** integrar Asaas PIX (é o que transforma demo em negócio)

---

## 11. Contatos úteis

| Quem | Para quê | Contato |
|------|----------|---------|
| **Helpie** (marceneiros API) | Rede de montadores + pagamento deles | parceria@helpie.com.br |
| **Rede Gaba** (marceneiros API) | Montagem planejada B2B | [redegaba.com.br](https://redegaba.com.br) |
| **Asaas** | PIX + split + escrow | [asaas.com](https://www.asaas.com) |

---

## 12. Credenciais locais (desenvolvimento)

| Item | Valor |
|------|-------|
| Web | http://localhost:5197 |
| API | http://localhost:5047 |
| Admin | `admin@poscorte.com` / `Admin@PosCorte2026` |

**Lembrete:** API precisa estar rodando antes do Web.

---

## 13. Resumo final

Você **não está longe** — a base técnica é sólida. O que separa você de R$ 0 e de R$ 180k/mês é:

1. **CNPJ + Asaas** (legal/financeiro)  
2. **PIX real no Web** (1 semana de dev)  
3. **Parceria Gaba ou Helpie** (marceneiro real sem você cadastrar ninguém)  
4. **Deploy + domínio** (2 dias)  
5. **10 arquitetos pagantes em SP** (vendas suas, não do código)

O caminho para **milhão** é: provar **R$ 18k/mês** → **R$ 60k** → **R$ 180k** → parcerias MDF em escala → mais cidades. Software é **30%**; distribuição e confiança são **70%**.

> **Regra de ouro:** não adicione feature nova até o primeiro PIX de um cliente que você não conhece pessoalmente.

---

## 14. Visão absurda (e executável): ficar rico o mais rápido possível

> Esta seção não é fantasia de pitch deck. É um **modo de guerra** — combinação de posicionamento agressivo, hacks de distribuição e execução diária no nível do detalhe.  
> **Tese:** você não vende “software de montagem”. Você vende **fim do pânico logístico** no único momento em que o arquiteto está mais vulnerável e mais disposto a pagar: **depois que o MDF já foi cortado**.

### 14.1 A aposta central (por que isso PODE explodir)

O mercado de móveis planejados em SP move **bilhões/ano**. O gargalo não é projeto — é **montagem confiável depois do corte**. Hoje o arquiteto:

1. Fecha com o cliente  
2. Compra MDF  
3. Corta  
4. **Entra em pânico** — “quem monta?”, “e se sumir?”, “e se estragar?”  
5. Corre no WhatsApp, indicação, sorte  

**PósCorte no momento certo = Uber do pós-corte.** Quem dominar esse micro-momento com escrow + montador homologado + preço na hora, **come o mercado**.

Seu moat não é o código. É:

- **Confiança no pagamento** (escrow)  
- **Velocidade de alocação** (API parceiro)  
- **Canal de aquisição** (balcão MDF + comunidade Promob)  
- **Dados** (preço por peça/gaveta/CEP — ninguém tem isso estruturado)

---

### 14.2 Meta “rico rápido” — 3 horizontes

| Horizonte | Prazo | Meta caixa (sua margem 20%) | O que significa na prática |
|-----------|-------|------------------------------|----------------------------|
| **Sobrevivência** | 60 dias | R$ 6.000–12.000/mês | 10–20 projetos/mês pagos — prova que não é brinquedo |
| **Liberdade** | 6 meses | R$ 60.000/mês | ~100 projetos/mês — dá para contratar 2 pessoas e focar em escala |
| **Patrimônio** | 12–18 meses | R$ 180.000/mês + valuation | 300 projetos/mês — investidor ou comprador aparece sozinho |

**Atalho mental:** R$ 60k/mês com margem de 70% operacional líquida depois de custos = **~R$ 500k/ano no bolso**. Em 3–4 anos de reinvestimento + equity, **patrimônio de milhões** é cenário realista — não em 30 dias.

---

### 14.3 O plano de 90 dias — “Operação Relâmpago”

#### DIAS 1–7: Desbloqueio total (nada de feature bonita)

| Dia | Manhã (você) | Tarde (dev) | Noite (você) |
|-----|--------------|-------------|--------------|
| **1** | Ligar contador: abrir CNPJ LTDA “intermediação de serviços” | — | Listar 30 arquitetos SP no Notion (nome, IG, WhatsApp) |
| **2** | Abrir conta PJ + iniciar Asaas | Branch `feature/asaas-pix` | Enviar e-mail Rede Gaba + Helpie (template §14.8) |
| **3** | Asaas: enviar docs | Entidade `Pagamento` + migration | Gravar vídeo 60s tela do produto (Loom) |
| **4** | Follow-up Asaas | `POST gerar-pix` sandbox | Postar vídeo em 3 grupos Promob (sem spam — ver §14.6) |
| **5** | Advogado: minuta Termos + Privacidade | Tela `Projetos/Pagar` mock com QR sandbox | 10 DMs personalizadas a arquitetos |
| **6** | Comprar domínio `poscorte.com.br` | Webhook Asaas real | Script de ligação — 5 ligações (§14.7) |
| **7** | Revisão semana: 0 PIX ainda OK | Deploy staging | Definir 3 piloto clients (conhecidos ou semi-conhecidos) |

**Critério de sucesso semana 1:** CNPJ em andamento + Asaas sandbox OK + domínio comprado + 30 arquitetos mapeados.

---

#### DIAS 8–30: Primeiro sangue (1 PIX real)

| Semana | Dev (prioridade única) | Você (vendas) | Meta |
|--------|------------------------|---------------|------|
| **2** | PIX produção + split 80/20 | 3 pilotos com **taxa 0%** (só case) | 3 projetos criados |
| **3** | ProvedorApi ligado OU piloto manual montador | Fechar 1 parceria “soft” com vendedor MDF | 1 PIX pago |
| **4** | WhatsApp Z-API + upload arquivo | 20 abordagens + 1 depoimento em vídeo | 3 PIX pagos no mês |

**Regra absurda:** se dia 25 não tiver PIX, você **para de codar** e faz 50 abordagens em 48h.

---

#### DIAS 31–90: Máquina de aquisição

| Mês | Foco | Meta margem |
|-----|------|-------------|
| **2** | 1ª parceria balcão MDF + ads R$ 3k | R$ 9.000 |
| **3** | 2ª parceria + indicação arquiteto→arquiteto | R$ 18.000 |

---

### 14.4 Hacks inovadores (distribuição > produto)

#### Hack #1 — “Cavalo de Troia Corte Cloud”

**Ideia:** o arquiteto já vive no Corte Cloud / Promob. Você não compete — **encaixa depois**.

- [ ] Criar PDF de 1 página: “Checklist pós-corte: 5 passos antes da montagem”  
- [ ] Último passo: QR → `poscorte.com.br`  
- [ ] Distribuir em grupos de CNC, Facebook, Telegram  
- [ ] (Fase 2) Plugin ou bookmarklet que lê nome do projeto e pré-preenche formulário PósCorte  

**Execução detalhada:**

1. Canva: PDF profissional com sua logo  
2. Post: “Perdi R$ 8k com montador que sumiu — fiz esse checklist” (storytelling)  
3. Link rastreado: `poscorte.com.br/?utm_source=promob&utm_campaign=checklist`  
4. Medir: quantos cadastros vieram do PDF  

---

#### Hack #2 — “Comissão de balcão” (o multiplicador)

Um vendedor de MDF atende **20 arquitetos/semana**. Você precisa de **10 vendedores**, não de 10.000 arquitetos.

**Proposta exata para o gerente da distribuidora:**

> “Seu cliente já comprou o MDF. A dor de montagem é sua também — reclamação volta no balcão.  
> Indique PósCorte: arquiteto paga com escrow, montador homologado.  
> **Você ganha R$ 80 por montagem fechada** (cupom `DISTRI-NOME`).  
> Zero custo, zero estoque, zero risco.”

**Checklist de execução:**

- [ ] Mapear 20 distribuidoras: Guarulhos, SBC, Osasco, Lapa, Mooca  
- [ ] LinkedIn: buscar “vendedor MDF planejados São Paulo”  
- [ ] Imprimir 50 flyers A5 com QR (custo ~R$ 40)  
- [ ] Visitar **presencialmente** 5 balcões (quinta de manhã — horário morto)  
- [ ] Planilha: `distribuidora | contato | cupom | leads | fechados | comissão paga`  
- [ ] Pagar comissão em **24h** via Pix (confiança = vendedor indica de novo)  

---

#### Hack #3 — “Escrow como arma de vendas”

Arquiteto não compra montagem. Compra **não ser caloteado**.

**Landing — acima do fold (copiar literal):**

> **Seu MDF já foi cortado. E agora?**  
> Pague a montagem em ambiente seguro. O dinheiro só sai depois do serviço.  
> Montador homologado em até 24h.  
> **[Subir projeto — orçamento em 30 segundos]**

- [ ] Adicionar selo fake-it-till-you-make-it: “Fundos garantidos em parceria com gateway regulado” (só depois de Asaas real)  
- [ ] Vídeo 15s: tela gravada criando projeto → orçamento → QR PIX  

---

#### Hack #4 — “Preço ancorado” (psicologia)

Mostrar na tela de orçamento:

```
Custo montador (estimado):     R$ 2.400
Taxa PósCorte (segurança):     R$   600
─────────────────────────────────────
Total ao arquiteto:            R$ 3.000

✓ Escrow  ✓ Montador verificado  ✓ Suporte disputa
```

Arquiteto vê **R$ 600 como seguro**, não como “taxa de app”.

- [ ] Ajustar UI `Projetos/Detalhes` com breakdown (dev: 2h)  

---

#### Hack #5 — “WhatsApp como UI” (MVP de notificação em 1 dia)

Antes de app marceneiro: **todo evento crítico pinga WhatsApp seu + arquiteto**.

| Evento | Mensagem automática |
|--------|---------------------|
| Projeto criado | “Renan, novo projeto R$ X — CEP Y” |
| PIX pago | “✅ Pagamento confirmado. Alocando montador.” |
| Montador alocado | “Montador João — (11) 9xxxx. Data: DD/MM” |
| Concluído | “Aprove a montagem em 72h: [link]” |

- [ ] Z-API: R$ ~100/mês, integrar em `NotificacaoService`  
- [ ] Seu número recebe cópia de tudo nos primeiros 90 dias (controle total)  

---

#### Hack #6 — “White label para lojas de planejados” (mês 4+)

Lojas pequenas não têm operação de montagem. Você vira **backend invisível**.

- [ ] Pacote: “Sua loja oferece montagem com escrow — powered by PósCorte”  
- [ ] Take rate: 25% (loja fica com 5% de indicação, você 20%)  
- [ ] 5 lojas × 10 projetos/mês = 50 projetos sem ads  

---

### 14.5 Produto — só o que multiplica dinheiro (ordem cruel de prioridade)

| # | Feature | Por que | Prazo |
|---|---------|---------|-------|
| 1 | PIX + QR na tela | Sem isso = R$ 0 | 5 dias |
| 2 | Webhook → ordem provedor | Sem montador = churn | +3 dias |
| 3 | WhatsApp notificação | Sem isso você vira call center | 1 dia |
| 4 | Upload arquivo corte | Reduz atrito cadastro | 2 dias |
| 5 | Breakdown preço na UI | Aumenta conversão | 2h |
| 6 | Parser Corte Cloud | **10× conversão** — mas só após 10 PIX | 2–4 sem |
| 7 | SignalR “PIX confirmado” | UX premium | 1 dia |
| 8 | Tudo o resto | Ignorar até R$ 18k/mês | — |

---

### 14.6 Scripts prontos (copiar e colar)

#### DM Instagram / LinkedIn — arquiteto

```
Oi [Nome], vi seus projetos de planejados — muito bom o [projeto específico].

Trabalho com uma plataforma que resolve a parte que todo arquiteto odeia:
depois do corte do MDF, montador homologado + pagamento em escrow
(o dinheiro só libera depois do serviço).

Orçamento automático em 30 seg — subo o arquivo e informo peças/gavetas/CEP.

Posso te mandar um link pra testar sem compromisso? São 2 min.
```

#### E-mail Rede Gaba / Helpie

```
Assunto: Parceria B2B — marketplace de montagem pós-corte (São Paulo)

Olá, time de parcerias,

Sou fundador do PósCorte — plataforma que conecta arquitetos de móveis
planejados a montagem homologada no momento pós-corte, com pagamento
PIX em escrow e take rate de 20%.

Já temos MVP em produção (.NET, API REST) com integração preparada
para provedor externo (Refit). Buscamos:

1. Credenciais sandbox da API de criação de ordens de montagem
2. Modelo comercial de revenda / white label
3. Webhook de status do montador

Volume projetado: 30–300 ordens/mês (Grande SP, 12 meses).

Podemos agendar 20 min esta semana?

Renan
[seu WhatsApp]
poscorte.com.br
```

#### Ligação fria — arquiteto (30 segundos)

```
"[Nome]? Oi, Renan do PósCorte, rápido — você trabalha com planejados, certo?
Depois do corte, como você resolve montagem hoje?
[ouve]
A gente faz orçamento na hora e segura o pagamento até o montador terminar.
Posso te mandar o link no WhatsApp? Leva 2 minutos pra ver o preço."
```

#### Pitch gerente distribuidora MDF

```
"[Nome], uma pergunta: quando o arquiteto compra MDF aí e depois se atrasa
na montagem, quem ele culpa?
[pausa]
A gente resolve isso com montador homologado e pagamento seguro.
Você indica com um QR no balcão — ganha R$ 80 por montagem fechada.
Posso deixar 10 flyers e voltar semana que vem pra ver se gerou lead?"
```

---

### 14.7 Rotina diária “modo animal” (primeiros 90 dias)

| Horário | Ação | Meta numérica |
|---------|------|---------------|
| **07:00** | Dashboard Admin: projetos, erros, ordens travadas | 5 min |
| **07:15** | 3 follow-ups WhatsApp (quem criou projeto e não pagou) | 3 msgs |
| **08:00–12:00** | **VENDAS** — DMs, ligações, visitas balcão | 10 contatos/dia |
| **12:00** | Almoço + responder leads | — |
| **13:00–17:00** | Dev OU parcerias (alternar dias) | 1 entrega/dia |
| **17:00** | Post conteúdo (1/dia útil) — dor do pós-corte | 1 post |
| **17:30** | Planilha métricas: leads, projetos, PIX, R$ | 10 min |
| **18:00** | Desligar. Sem feature extra “só mais uma” | — |

**KPI diário não negociável:**

- Mínimo **10 abordagens comerciais/dia** (seg–sex)  
- Mínimo **1 post/dia** em grupo ou rede  
- Máximo **4h de dev/dia** até 10 PIX pagos (resto é venda)  

---

### 14.8 Planilha de controle (criar no Google Sheets hoje)

Abas:

1. **Pipeline arquitetos** — Nome | WhatsApp | Origem | Status (frio/morno/piloto/pago) | Valor | Data  
2. **Pipeline MDF** — Distribuidora | Contato | Cupom | Visitado? | Leads | Fechados  
3. **Financeiro** — Mês | PIX recebidos | GMV | Margem 20% | Custos | Líquido  
4. **Operação** — Projeto ID | Status | Montador | Tempo alocação | Problema?  

**Fórmula norte:** `Margem líquida = (PIX pagos × ticket médio × 0,20) - custos fixos`

---

### 14.9 Alavancas “absurdas” de longo prazo (mês 6+)

Só depois de R$ 18k/mês. Mas planeje agora:

| Alavanca | O que é | Potencial |
|----------|---------|-----------|
| **Seguro de montagem** | Parceria seguradora — R$ 49/projeto | Receita extra + confiança |
| **Financiamento ao arquiteto** | PIX parcelado via Asaas | Ticket 2× |
| **Dados de pricing** | Vender relatório “preço montagem SP” para fábricas | B2B data |
| **API pública** | Lojas integram orçamento | Network effect |
| **M&A** | Comprar lista de marceneiros / pequeno concorrente | Velocidade |

---

### 14.10 O que “ficar rico rápido” NÃO é

- ❌ Esperar produto perfeito  
- ❌ Construir app mobile antes de 100 PIX  
- ❌ Reduzir taxa para 5% “pra pegar mercado”  
- ❌ Atender Brasil inteiro  
- ❌ Fundar com investidor antes de tração (dilui antes da hora)  
- ❌ Gastar R$ 20k em ads sem case real  

---

### 14.11 Contrato consigo mesmo (assine mentalmente)

Eu, Renan, fundador do PósCorte, comprometo-me a:

1. **Não passar 3 dias seguidos só codando** sem falar com 1 arquiteto  
2. **Lançar PIX real em até 21 dias** a partir de hoje  
3. **Fazer 200 abordagens comerciais** nos primeiros 60 dias  
4. **Pagar comissão de balcão em 24h** quando fechar parceria MDF  
5. **Celebrar o primeiro PIX de estranho** mais do que qualquer feature  

---

### 14.12 Próximas 24 horas (lista cirúrgica)

- [ ] **08h** — WhatsApp contador: “Quero abrir LTDA esta semana, intermediação de serviços”  
- [ ] **09h** — Criar planilha §14.8 (4 abas)  
- [ ] **10h** — Listar 30 arquitetos (IG + WhatsApp)  
- [ ] **11h** — Enviar e-mail Helpie + formulário Rede Gaba  
- [ ] **14h** — Comprar domínio `poscorte.com.br`  
- [ ] **15h** — Gravar Loom 60s do produto  
- [ ] **16h** — 5 DMs com script §14.6  
- [ ] **17h** — Abrir branch `feature/asaas-pix` (ou pedir ao dev)  
- [ ] **18h** — Post em 1 grupo Promob com checklist PDF  

**Se fizer só isso em 24h, você estará à frente de 95% dos fundadores que só codam.**

---

### 14.13 Resumo da visão

| Camada | Estratégia |
|--------|------------|
| **Posicionamento** | Único no **pós-corte** — não compete com Promob |
| **Produto** | Escrow + preço instantâneo + 1 clique PIX |
| **Operação** | Marceneiro 100% via API parceiro — você não cadastra ninguém |
| **Distribuição** | Balcão MDF (10×) + comunidade Promob + indicação |
| **Velocidade** | 90 dias para R$ 18k/mês — 12 meses para R$ 180k/mês |
| **Patrimônio** | Equity em marketplace B2B com dados = múltiplo alto |

> **A versão absurda e inovadora não é reinventar a roda.**  
> É ser **o único** que o arquiteto lembra **no dia em que o caminhão do MDF chegou na obra** — e cobrar 20% por resolver o pânico em 30 segundos.

---

*Documento gerado com base no estado real do repositório PósCorte — junho/2026.*  
*Complemento técnico detalhado: `PENDENCIAS_E_ROADMAP.md`*
