# PósCorte — O que VOCÊ precisa fazer (handoff do fundador)

> **Para:** Renan  
> **Atualizado:** junho/2026  
> **Estratégia travada:** operação **manual** (você cadastra arquiteto + montador e aloca) + **divulgação**. Sem API Corte Cloud. Sem Helpie/Gaba até tração.

---

## 0. Leitura rápida (1 minuto)

| Bloco | O que falta de você | Bloqueia o quê | Prioridade |
|-------|---------------------|----------------|------------|
| 🔴 CNPJ + Asaas | Abrir empresa + conta gateway no **seu** CNPJ | PIX real / receita | P0 |
| 🟠 Deploy + domínio | Hospedar API + Web | Cliente externo usar | P1 |
| 🟡 Jurídico | Revisar Termos/Privacidade com contador | Risco legal | P1 |
| 🟢 Comercial | Planilhas, DMs, montadores WhatsApp | Tração | P0 humano |

**O código do produto para Fases 1–3 (parte técnica) está pronto.** O que falta é execução comercial e credenciais reais.

---

## 1. O que o sistema JÁ faz (use na demo)

| Funcionalidade | Status |
|----------------|--------|
| Landing + estimador público | ✅ |
| Cadastro/login arquiteto | ✅ |
| Admin: cadastrar arquiteto | ✅ `/Admin/Arquitetos` |
| Admin: cadastrar montador | ✅ `/Admin/Marceneiros` |
| Admin: alocar montador + marcar obra concluída | ✅ `/Admin/Projetos` → Operar |
| PIX (stub dev / Asaas quando ligar) | ✅ |
| Escrow + vistoria + liquidação 72h | ✅ |
| Termos + Privacidade no rodapé | ✅ piloto |
| Trocar senha admin | ✅ `/Admin/Conta` |
| CORS configurável para produção | ✅ `Cors:AllowedOrigins` |
| Docker API + Web | ✅ `docker/` |
| Testes automatizados | ✅ 34 passando |

**Dev:** Web `http://localhost:5197` · API `http://localhost:5047` · Admin `admin@poscorte.com` / `Admin@PosCorte2026`

---

## 2. 🔴 Pagamento PIX real (Asaas)

**Hoje:** modo Stub — simule em Development. Produção sem Asaas não cobra.

**Você faz:**

1. CNPJ + conta **Asaas** no nome da empresa (não de terceiros).
2. Variáveis de ambiente (nunca no Git):

```
Asaas__Enabled       = true
Asaas__ApiKey        = (sua chave)
Asaas__WebhookToken  = (segredo)
```

3. Webhook: `https://api.SEU_DOMINIO/api/v1/webhooks/asaas`

Detalhes: [`INTEGRACAO_PAGAMENTO_ASAAS.md`](INTEGRACAO_PAGAMENTO_ASAAS.md)

---

## 3. 🟠 Deploy em produção

**Guia completo:** [`DEPLOY.md`](DEPLOY.md)

Checklist mínimo:

- [ ] Domínio (ex. `poscorte.com.br`)
- [ ] API + Web no Railway/Render/Azure
- [ ] `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `ApiBaseUrl`
- [ ] `Cors__AllowedOrigins__0` = URL do Web
- [ ] Trocar senha admin em `/Admin/Conta`

---

## 4. 🟢 Operação manual (seu dia a dia)

Não precisa de API de parceiro. Fluxo:

1. Divulga → arquiteto interessado  
2. **Admin → Arquitetos** → cadastra e manda login no WhatsApp  
3. Arquiteto cria projeto e paga (ou simula em dev)  
4. **Admin → Projetos → Operar** → aloca montador da sua lista  
5. WhatsApp com montador e arquiteto  
6. Marca montagem concluída no admin → arquiteto vistoria  

Planilhas para importar no Google Sheets: [`templates/MONTADORES.csv`](templates/MONTADORES.csv), [`templates/ARQUITETOS.csv`](templates/ARQUITETOS.csv), [`templates/FINANCEIRO.csv`](templates/FINANCEIRO.csv)

---

## 5. 🟡 Jurídico

- [ ] LTDA + conta PJ com contador  
- [ ] Revisar páginas `/Legal/Termos` e `/Legal/Privacidade` (rascunho já publicado)  
- [x] Links no rodapé do site  

---

## 6. 🟠 Notificações (opcional P1)

Stub em log hoje. Para WhatsApp/e-mail automático: [`INTEGRACAO_NOTIFICACOES.md`](INTEGRACAO_NOTIFICACOES.md)

---

## 7. O que NÃO fazer agora

- ❌ Integrar Helpie/Gaba (decisão: manual até tração)  
- ❌ Corte Cloud checkout  
- ❌ App do montador  
- ❌ Conta Asaas de amigo  

---

## 8. Ordem de execução

```
1. Planilhas + 10 montadores + 30 arquitetos (WhatsApp)     ← hoje
2. CNPJ + Asaas                                              ← destrava PIX
3. Deploy + domínio                                          ← cliente usa link público
4. 3 pilotos taxa 0% + depoimento em vídeo                   ← prova social
5. Notificações automáticas                                  ← escala ops
```

Playbook completo: [`PLAYBOOK_UNICO.md`](PLAYBOOK_UNICO.md)

---

## 9. Testar ciclo completo (dev, sem dinheiro real)

1. Suba API + Web  
2. Login admin → cadastre 1 arquiteto e 1 montador  
3. Login como arquiteto → crie projeto → **Pagar** → **Simular pagamento**  
4. Admin → Operar → aloque montador  
5. Marque montagem concluída → login arquiteto → **Aprovar montagem**  

---

*Relacionados: [`PLAYBOOK_UNICO.md`](PLAYBOOK_UNICO.md) · [`DEPLOY.md`](DEPLOY.md) · [`ANALISE_MERCADO_POSICIONAMENTO.md`](ANALISE_MERCADO_POSICIONAMENTO.md)*
