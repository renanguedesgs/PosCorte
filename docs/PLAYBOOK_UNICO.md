# PósCorte — Playbook Único de Execução

> **Para:** Renan  
> **Visão:** um documento só. Ordem de execução. Sem desculpa. Sem feature bonita antes de PIX.  
> **Estratégia travada:** manual (arquiteto + montador) + divulgação. Sem API Corte Cloud. Sem Helpie/Gaba até provar tração.  
> **Meta 90 dias:** R$ 6.000–18.000/mês de margem (10–30 projetos pagos). Depois você toma suco de maracujá.

---

## A verdade em uma frase

Você **não vende software**. Você vende **fim do pânico** no dia em que o caminhão do MDF chegou na obra — com dinheiro seguro e alguém que aparece.

O código é o **escudo de confiança**. O dinheiro vem da **sua boca** (divulgação) e do **seu WhatsApp** (montador na mão).

---

## O que o ser humano compra (psicologia que paga)

| O arquiteto pensa | O que você fala |
|-------------------|-----------------|
| "E se o montador sumir?" | Escrow: o dinheiro só sai depois do serviço |
| "Quanto custa?" | Orçamento em 30 segundos na tela |
| "Não tenho tempo de caçar ninguém" | Você aloca montador homologado em até 24h |
| "Já me deram golpe" | Pagamento na plataforma, não PIX direto pro cara |

**Regra de ouro:** nunca venda "marketplace" ou "20% de taxa". Venda **segurança + tempo**.

---

## O que NÃO fazer (armadilhas de dev inteligente)

- ❌ Integrar Corte Cloud antes do 10º PIX pago  
- ❌ App do montador antes de 50 projetos  
- ❌ Parser automático de arquivo antes de validar preço na mão  
- ❌ Ads antes de 3 depoimentos reais  
- ❌ Reduzir taxa para 10% "pra crescer"  
- ❌ Codar 3 dias seguidos sem falar com 1 arquiteto  
- ❌ Usar conta Asaas de terceiro (golpe futuro garantido)

---

## O que o sistema JÁ faz (junho/2026)

Use isso na conversa de vendas — **não é promessa, é demo**:

| Funciona hoje | Modo |
|---------------|------|
| Landing com estimador de orçamento (sem login) | ✅ |
| Cadastro/login arquiteto | ✅ |
| Criar projeto + preço automático (R$ 12,50/peça + R$ 40/gaveta, 20%) | ✅ |
| Pagar com PIX (tela QR + copia-e-cola) | Stub / Asaas quando ligar |
| Escrow (dinheiro retido após pagamento) | ✅ lógica |
| Vistoria: aprovar montagem / abrir disputa | ✅ |
| Liberação automática após 72h sem contestação | ✅ job em background |
| Painel admin (KPIs, projetos, cadastro manual arquiteto/montador, alocar) | ✅ |
| Termos de Uso + Privacidade (rodapé) | ✅ piloto |
| Testes automatizados | 35 passando |

**Credenciais locais (dev):**

| Item | Valor |
|------|-------|
| Web | http://localhost:5197 |
| API | http://localhost:5047 |
| Admin | `admin@poscorte.com` / `Admin@PosCorte2026` |

**Operação manual no admin (implementado):**

- [x] Admin: **cadastrar montador** (`/Admin/Marceneiros`)  
- [x] Admin: **cadastrar arquiteto** (`/Admin/Arquitetos`)  
- [x] Admin: **alocar montador** (`/Admin/Projetos` → Operar)  
- [x] Admin: **marcar montagem concluída** (libera vistoria 72h)  

**Seu trabalho nas Fases 1–3 (fora do código):** CNPJ, Asaas, divulgação, planilhas WhatsApp — ver seções abaixo.

---

# ORDEM DE EXECUÇÃO

Execute **de cima para baixo**. Não pule fase.

---

## FASE 0 — Decisão (hoje, 30 minutos)

Marque mentalmente:

- [x] Modelo **manual**: eu cadastro/aloco montador  
- [x] Aquisição **divulgação + DM**, não API  
- [x] Região **Grande SP** só  
- [x] Take rate **20% fixo** (piloto: taxa 0% nos 3 primeiros)  
- [x] Eu sou **vendas + ops** nas primeiras 8 semanas  

**Contrato consigo mesmo:** se em 25 dias não tiver 1 PIX de terceiro, paro de codar 48h e faço 50 abordagens.

---

## FASE 1 — Fundação que destrava dinheiro (dias 1–10)

Sem CNPJ não tem PIX comercial. Sem PIX não tem negócio.

### Dia 1–2: Jurídico

- [ ] WhatsApp/ligação **contador**: abrir LTDA "intermediação de serviços" / marketplace  
- [ ] Abrir **conta PJ** (Inter, C6, etc.)  
- [ ] Comprar domínio **poscorte.com.br** (ou equivalente)

### Dia 3–5: Pagamento

- [ ] Criar conta **Asaas** no CNPJ da empresa (não de amigo)  
- [ ] Enviar documentos, passar homologação  
- [ ] Quando aprovado, configurar (ver `docs/INTEGRACAO_PAGAMENTO_ASAAS.md`):

```
Asaas__Enabled       = true
Asaas__ApiKey        = (sua chave)
Asaas__WebhookToken  = (segredo que você inventa)
```

- [ ] Webhook: `https://SEU_DOMINIO/api/v1/webhooks/asaas`

### Dia 6–10: Jurídico mínimo + deploy

- [ ] Advogado/contador: revisar minuta **Termos de Uso** + **Política de Privacidade** (rascunho já no site: `/Legal/Termos`, `/Legal/Privacidade`)  
- [x] Publicar termos no rodapé do site (versão piloto)  
- [x] Trocar senha admin (`/Admin/Conta`) — faça no 1º login produção  
- [ ] Deploy API + Web — ver [`docs/DEPLOY.md`](DEPLOY.md)  

**Critério de sucesso Fase 1:** CNPJ em andamento + domínio comprado + Asaas sandbox ou produção OK.

---

## FASE 2 — Montadores na mão (dias 1–14, em paralelo à Fase 1)

Meta: **10 montadores** no WhatsApp que respondem em menos de 4h.

### Onde achar (tarde de quinta = horário morto em marcenarias)

| Fonte | Ação |
|-------|------|
| Grupos WhatsApp | "marceneiros SP", "montagem planejados", "corte CNC" |
| Instagram | `#montadordemoveis` `#marceneirosp` |
| Marcenarias locais | Visitar 3, pedir indicação de montador autônomo |
| GetNinjas / OLX | Só para **pegar telefone**, não depender da plataforma |

### Planilha `MONTADORES` (Google Sheets)

Importe o modelo: [`docs/templates/MONTADORES.csv`](templates/MONTADORES.csv)

```
Nome | WhatsApp | Cidade/Bairro | Especialidade | Responde rápido? (S/N) | Fez obra? (S/N) | Nota 1-5
```

Depois de validar no WhatsApp, cadastre no admin em **Montadores**.

### Script para montador (WhatsApp)

```
Oi [Nome], sou Renan do PósCorte.

Montamos móveis planejados pós-corte para arquitetos em SP.
O arquiteto paga na plataforma (você não corre atrás de cobrança).
Você recebe ~80% após aprovar a montagem.

Posso te mandar 1–2 obras por mês no começo. Topa entrar na lista?
Me manda: cidade que atende + se faz cozinha / dormitório.
```

**Critério de sucesso Fase 2:** 10 montadores responderam "sim" + 3 disseram que aceitam obra esta semana.

---

## FASE 3 — Arquitetos e primeiros pilotos (dias 7–30)

Meta: **3 projetos pagos** (pode ser conhecido no 1º; estranho no 2º ou 3º).

### Planilha `ARQUITETOS`

Importe o modelo: [`docs/templates/ARQUITETOS.csv`](templates/ARQUITETOS.csv)

```
Nome | Instagram/LinkedIn | WhatsApp | Origem | Status | Projeto criado? | PIX pago? | Valor
```

Status: `frio` → `morno` → `piloto` → `pago` → `indicou`

### Onde achar arquitetos (50 nomes em 3 dias)

- Instagram: `#arquiteturaSP` `#moveisplanejados` `#promob`  
- LinkedIn: "arquiteto móveis planejados São Paulo"  
- Grupos Facebook/Telegram: Promob, SketchUp, CNC  
- Indicação de marceneiro: "quem é seu arquiteto que mais compra MDF?"

### Oferta piloto (use literal)

> Primeiro projeto: **taxa da plataforma zero** (você paga só o montador + custo operacional embutido no orçamento).
> Em troca: 2 min de depoimento em vídeo se der certo.

### Script DM arquiteto

```
Oi [Nome], vi [projeto específico no IG] — muito bom.

Trabalho com o PósCorte: depois do corte do MDF, orçamento de montagem
em 30 seg + pagamento em escrow (dinheiro só libera quando o serviço
termina). A gente aloca montador homologado.

Posso te mandar o link pra simular o preço sem compromisso? São 2 min.
```

### Script ligação (30 segundos)

```
"[Nome]? Renan, PósCorte, rápido — você trabalha com planejados?
Depois do corte, como resolve montagem hoje?
[ouve]
A gente faz orçamento na hora e segura o pagamento até terminar.
Posso mandar o link no WhatsApp?"
```

### Fluxo quando arquiteto aceita

1. Manda link: `https://poscorte.com.br` (ou localhost em dev)  
2. **Cadastra o arquiteto** em Admin → Arquitetos (ou ele se registra) — manda login/senha no WhatsApp  
3. Projeto criado → mostra orçamento na tela  
4. PIX pago (ou simulado em dev) → Admin → Projetos → **Operar** → aloca montador  
5. Combina data no WhatsApp → montador vai na obra  
6. Após obra → você marca **montagem concluída** no admin OU arquiteto **aprova** na tela  
7. Pedir depoimento em vídeo (Loom, 60s)

**Critério de sucesso Fase 3:** 3 PIX confirmados + 1 vídeo depoimento gravado.

---

## FASE 4 — Divulgação que enche o funil (dia 10 em diante, contínuo)

### Ritmo mínimo (não negociável, seg–sex)

| Horário | Ação | Meta numérica |
|---------|------|---------------|
| 07:00 | Olhar admin: projetos, quem não pagou | 5 min |
| 07:15 | Follow-up WhatsApp (criou projeto, não pagou) | 3 msgs |
| 08:00–12:00 | **VENDAS** — DM, ligação, visita | 10 contatos/dia |
| 12:00 | Responder leads | — |
| 13:00–17:00 | Ops (alocar montador) OU 1 ação comercial | 1 entrega |
| 17:00 | 1 post em grupo ou story (dor pós-corte) | 1/dia |
| 17:30 | Atualizar planilha métricas | 10 min |
| 18:00 | **Desligar.** Sem "só mais uma feature" | — |

### Conteúdo que converte (roteiros)

**Post grupo Promob / CNC:**

```
Quem mais já perdeu entrega porque o montador sumiu depois do corte?

Montei uma ferramenta que dá orçamento de montagem na hora
e segura o pagamento até o serviço (escrow).

Quem quiser testar sem compromisso: [link]
Primeiros 3 projetos sem taxa da plataforma.
```

**Story/Reels (15s):**

Tela gravada: landing → arrastar peças no estimador → preço aparece → "link na bio".

### Alavanca mês 2: balcão MDF

Pitch pro gerente:

```
"Quando o arquiteto atrasa na montagem, quem ele culpa?
A gente resolve com montador homologado e pagamento seguro.
Você indica com QR no balcão — R$ 80 por montagem fechada.
Posso deixar 10 flyers e volto semana que vem?"
```

---

## FASE 5 — Ligar o dinheiro real (assim que Asaas aprovar)

Checklist técnico (dev ou você com o doc):

- [ ] `Asaas__Enabled=true` em produção  
- [ ] Webhook Asaas apontando pro domínio  
- [ ] Testar 1 PIX de R$ 1,00 seu → estorno  
- [ ] Testar 1 PIX real de piloto  
- [x] Desabilitar botão "simular pagamento" fora de Development  

Detalhes: `docs/INTEGRACAO_PAGAMENTO_ASAAS.md`

**Fluxo do dinheiro:**

```
Arquiteto paga R$ 3.000 (exemplo cozinha)
    → Escrow retém
    → Montagem OK + vistoria
    → R$ 2.400 montador / R$ 600 você (20%)
```

---

## FASE 6 — Operação manual dia a dia (seu "call center" até escalar)

### Quando PIX confirma (template WhatsApp montador)

```
🔨 Nova montagem PósCorte

Projeto: [nome]
Endereço: [endereço] — CEP [cep]
Peças: [X] | Gavetas: [Y]
Repasse seu: R$ [80% do custo mão de obra]
Plano: [link arquivo]

Data sugerida: [dia]. Aceita? Responde SIM e te confirmo o arquiteto.
```

### Quando montador aceita (template arquiteto)

```
✅ Montador alocado: [Nome] — [WhatsApp]
Data combinada: [dia]
Qualquer problema na obra, me chama ou abre disputa na plataforma.
```

### Quando obra termina

Lembrete ao arquiteto:

```
A montagem foi concluída? Entra no PósCorte → Projeto → Aprovar montagem.
Se não fizer nada em 72h, o pagamento libera automaticamente pro montador.
```

---

## FASE 7 — Métricas (olhe só isso)

| Métrica | Semana 4 | Mês 3 | Mês 6 |
|---------|----------|-------|-------|
| Abordagens/semana | 50 | 50 | 30 (mais inbound) |
| Projetos criados | 5 | 40 | 100 |
| Taxa projeto → PIX | 20% | 40% | 50% |
| PIX pagos/mês | 1 | 15 | 50 |
| Margem 20%/mês | R$ 600 | R$ 9.000 | R$ 30.000 |
| Tempo alocar montador | < 24h | < 4h | < 2h |

**Fórmula:** `Margem = PIX pagos × ticket médio × 0,20 − custos fixos`

Ticket médio referência: **R$ 3.000** (cozinha) → margem **R$ 600**/projeto.

---

## FASE 8 — Só depois de R$ 6.000/mês (gaveta)

Não abra antes. Suco de maracujá vem antes de escala.

- [ ] Contratar 1 pessoa de ops (alocação + WhatsApp)  
- [ ] Parceria balcão MDF (2+ ativas)  
- [ ] Ads Meta (R$ 3k/mês) com case real  
- [ ] API Helpie/Gaba (automatizar montador)  
- [ ] Corte Cloud checkout (canal de demanda)  
- [ ] Parser automático de peças/gavetas  

---

## Scripts e links úteis

| Recurso | Onde |
|---------|------|
| **Deploy produção** | `docs/DEPLOY.md` |
| Pagamento Asaas | `docs/INTEGRACAO_PAGAMENTO_ASAAS.md` |
| O que falta de credencial | `docs/ACOES_NECESSARIAS.md` |
| Planilhas MONTADORES / ARQUITETOS | `docs/templates/*.csv` |
| Mercado e concorrência | `docs/ANALISE_MERCADO_POSICIONAMENTO.md` |
| Plano longo (milhão) | `PLANO_EXECUCAO_COMPLETO.md` na raiz |
| Regras de negócio | `docs/REGRAS_NEGOCIO.md` |

---

## Checklist "primeiras 24 horas" (comece amanhã)

- [ ] 08h — Contador: abrir LTDA  
- [ ] 09h — Importar planilhas de `docs/templates/` (abas MONTADORES + ARQUITETOS + FINANCEIRO)  
- [ ] 10h — Listar 30 arquitetos (nome + WhatsApp)  
- [ ] 11h — Listar 10 montadores (nome + WhatsApp)  
- [ ] 14h — Comprar domínio  
- [ ] 15h — Gravar Loom 60s do produto (estimador + criar projeto)  
- [ ] 16h — 5 DMs com script de arquiteto  
- [ ] 17h — 3 WhatsApps para montadores com script  
- [ ] 18h — Post em 1 grupo Promob  

---

## Resumo executivo (cola na parede)

```
1. CNPJ + Asaas        → destrava cobrança
2. 10 montadores       → destrava operação
3. 10 abordagens/dia   → destrava clientes
4. 3 pilotos taxa 0%   → destrava prova social
5. Depoimento vídeo    → destrava escala
6. Código              → só depois do 1º PIX estranho
```

---

## Estado final desejado

Você acorda. Olha o admin: 12 projetos no mês, 8 concluídos, R$ 4.800 na conta PJ.

O telefone pinga: arquiteto novo veio da indicação.

Você manda um WhatsApp pro montador, aprova uma disputa em 2 minutos, toma um **suco de maracujá**.

O sistema cobrou, reteve, liberou. Você vendeu. O montador montou. O arquiteto dormiu em paz.

**Isso não é fantasia. É 90 dias de execução disciplinada em cima do que já está construído.**

---

*Playbook único PósCorte · junho/2026 · Estratégia manual + divulgação*

---

## Snapshot: código ✅ vs você ⏳

| Código (feito) | Você (fazer) |
|----------------|--------------|
| Admin cadastra arquiteto/montador | 10 montadores no WhatsApp |
| Alocar montador pós-PIX | 30 arquitetos na planilha |
| Vistoria + escrow + 72h | CNPJ + conta PJ |
| PIX stub + estrutura Asaas | Conta Asaas homologada |
| Termos/Privacidade piloto | Revisão jurídica |
| Deploy documentado (`DEPLOY.md`) | Domínio + Railway/Render |
| Planilhas CSV em `docs/templates/` | Importar no Sheets e preencher |
| 35 testes passando | 3 pilotos pagos + 1 vídeo |

**Próximo passo seu:** abrir as planilhas, mandar 3 WhatsApps para montadores e 5 DMs para arquitetos hoje.

