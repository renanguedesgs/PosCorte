# Análise de Mercado e Posicionamento — PósCorte

> Pesquisa de mercado (junho/2026) para validar o diferencial, mapear concorrência e definir o layout/estrutura que mais converte.

---

## 1. Conclusão principal

**Não existe concorrente direto** atacando o mesmo micro-momento do PósCorte: **a montagem logo após o corte do MDF, para o arquiteto de planejados, com orçamento instantâneo por peça/gaveta + escrow + montador via API parceira.**

O mercado se divide em duas categorias que **não** resolvem essa dor:

| Categoria | Exemplos | O que fazem | Por que NÃO são você |
|-----------|----------|-------------|----------------------|
| **Software para marcenaria** | MDFPro, Calcme | Projeto 3D, plano de corte, orçamento, gestão da marcenaria | Servem o **marceneiro**, não o arquiteto no pós-corte. Não montam nada. |
| **Marketplace genérico de serviços** | GetNinjas, Parafuzo, Montajá, PreGo, Constru Match | Conectam consumidor final a montador (móvel de loja, IKEA, Tok&Stok) | Público **B2C** e genérico. Sem foco em planejados, sem preço por peça/gaveta, sem o gatilho “pós-corte”. |

**Tradução:** o "API marceneiro" — alocar montador homologado automaticamente via integração, com pagamento retido — **não tem equivalente nacional nem claramente mundial** nesse nicho. O horizonte sem rival que você intuiu se confirma na pesquisa.

---

## 2. O diferencial defensável (moat)

O código **não** é o moat (é copiável). O que protege o PósCorte:

1. **Posicionamento no micro-momento** — ser lembrado exatamente quando “o caminhão do MDF chegou”.
2. **Confiança via escrow** — o arquiteto compra “não ser caloteado”, não “montagem”.
3. **Velocidade de alocação** — montador via API parceira em até 24h, sem você cadastrar ninguém.
4. **Dados proprietários** — preço de montagem por peça/gaveta/CEP. Ninguém tem isso estruturado.
5. **Canal de distribuição** — balcão de MDF + comunidade Promob (ver plano §3 e §14).

---

## 3. Tendências de tela que mais convertem (B2B SaaS 2026)

Da pesquisa de benchmarks de conversão e design B2B 2026:

- **Conversão B2B:** mediana 1–3%; topo de mercado chega a 11–15%. A diferença vem de **página**, não de tráfego (clareza, prova social, velocidade, copy).
- **Flow-first, não tela-a-tela:** desenhar a jornada inteira (subir projeto → preço → pagar → vistoria). ✅ aplicado.
- **Revelação progressiva:** mostrar só o necessário em cada etapa. ✅ (estimador simples no hero, detalhes depois).
- **Copy orientada a resultado** (“Quem monta agora?”, “a taxa é seu seguro”) em vez de features genéricas. ✅ aplicado.
- **Prova social segmentada** (depoimentos de arquitetos) + sinais de confiança (escrow, homologado). ✅ aplicado.
- **Mobile-first real** e **LCP < 2,5s** (fonte e ícone otimizados). ✅ favicon/ícone reduzidos, fontes via Google Fonts com preconnect.
- **Um objetivo de conversão por página:** o CTA único é “subir/despachar projeto”. ✅.

---

## 4. O que foi aplicado na landing (com base na pesquisa)

| Seção | Função de conversão |
|-------|---------------------|
| Hero “Quem monta agora?” | Dor imediata + CTA |
| **Estimador interativo (sem login)** | Lead magnet: arquiteto vê o preço antes de cadastrar |
| Trust bar | Sinais de confiança rápidos |
| Problema (montador some / calote / caça) | Agitação da dor |
| Como funciona (4 passos) | Reduz incerteza |
| Preços por escopo | Transparência + ancoragem (“taxa = seguro”) |
| Depoimentos | Prova social (ilustrativa na validação) |
| FAQ | Remove objeções (escrow, prazo, disputa) |
| CTA final | Fechamento |

---

## 5. Limitações e riscos honestos

| Limitação | Impacto | Mitigação |
|-----------|---------|-----------|
| **Depende de parceiro de montagem** | Sem Helpie/Gaba, montador real não é alocado | Fechar parceria em paralelo ao PIX; piloto manual no início |
| **Sem CNPJ/Asaas = sem receita** | Demo eterna | Bloco 1 do `ACOES_NECESSARIAS.md` |
| **Concorrente pode copiar a ideia** | Corrida de preço | Moat = rede + marca + canal MDF, não código |
| **Depoimentos/dados ainda ilustrativos** | Credibilidade inicial baixa | Trocar por cases reais após 3 pilotos (plano §3.1) |
| **Concentração geográfica (SP)** | Mercado limitado no início | É proposital: dominar SP antes de expandir |
| **Take rate 20% pode assustar** | Objeção de preço | Reenquadrado como “seguro” na UI (feito) |

---

## 6. Recomendação de foco

1. **Provar o ciclo com dinheiro real** (Asaas + 1 parceiro de montagem) na Grande SP.
2. **Capturar dados de pricing** desde o primeiro projeto — é o ativo de longo prazo.
3. **Distribuição via balcão MDF** (1 vendedor = 20 arquitetos/semana) — maior alavanca de volume.
4. Só então pensar em expansão geográfica, white-label para lojas e produtos de dados.

> *Fontes da pesquisa: benchmarks de conversão de landing pages 2026, tendências de design B2B SaaS 2026, e levantamento de plataformas brasileiras de marcenaria e montagem (MDFPro, Calcme, GetNinjas, Parafuzo, Montajá, PreGo, Constru Match).*
