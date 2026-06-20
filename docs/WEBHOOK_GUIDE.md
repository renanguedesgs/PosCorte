# ?? Guia de Webhooks

## Pagamento Confirmado

**Endpoint:** `POST /api/v1/webhooks/pagamento-confirmado`

Disparado pela gateway de pagamento (Asaas/Iugu) quando o PIX é confirmado.

```json
{
  "projetoId": 1,
  "status": "pago",
  "pixId": "pix-abc123",
  "valor": 312.50
}
```

**Fluxo:**
1. Valida o pagamento na gateway
2. Reserva fundos em Escrow
3. Cria Ordem de Serviço no provedor externo
4. Atualiza status do projeto para `Ordem_Criada`

## Atualização do Montador

**Endpoint:** `POST /api/v1/webhooks/atualizacao-montador`

Disparado pelo provedor de montadores quando há mudança de status.

```json
{
  "idExternalProviderId": "EXT-99999",
  "status": "aceito",
  "nomeMontador": "Carlos Silva",
  "telefoneMontador": "11988887777",
  "fotoMontadorUrl": "https://cdn.provedor.com/foto.jpg",
  "dataRetorno": "2024-01-15T10:00:00Z"
}
```

**Estados possíveis:**

| Status | Ação |
|--------|------|
| `aceito` | Aloca montador ao projeto |
| `a_caminho` | Atualiza status |
| `concluido` | Inicia contagem de 72h para liquidação do Escrow |
| `cancelado` | Cancela ordem e projeto |
