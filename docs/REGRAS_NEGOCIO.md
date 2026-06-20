# ?? Regras de Negócio

## Precificação (Markup Inverso)

**Fórmula:** `Preço Final = Custo / (1 - Taxa)`

| Item | Valor |
|------|-------|
| Custo fixo por peça | R$ 12,50 |
| Custo fixo por gaveta | R$ 40,00 |
| Taxa da plataforma | 20% |

**Exemplo:** 10 peças + 5 gavetas
- Custo = (10 × 12,50) + (5 × 40) = R$ 325,00
- Preço Final = 325 / 0,80 = **R$ 406,25**
- Margem plataforma = R$ 81,25

## Escrow (Garantia Financeira)

1. Pagamento PIX confirmado ? Fundos **reservados** no Escrow
2. Serviço concluído ? Inicia contagem de **72h úteis**
3. Sem contestação em 72h ? Fundos **liquidados** ao prestador
4. Contestação ? Fundos **bloqueados** até resolução

## Status do Projeto

```
Aguardando_Pagamento
       ?
Pagamento_Confirmado
       ?
Ordem_Criada
       ?
Prestador_Alocado
       ?
Aguardando_Vistoria
       ?
Concluido / Cancelado
```

## Regras de Segurança

- Ordem de serviço só é criada **após** confirmação de pagamento validada na gateway
- Fundos só são liberados **após** encerramento por decurso de prazo (72h úteis)
- Cancelamento não é permitido em ordens com status `Concluido`
