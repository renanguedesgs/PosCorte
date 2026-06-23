# Integração de Notificações (WhatsApp + E-mail)

Estado atual: **stub**. Toda notificação passa por um **ponto único** no código e hoje apenas registra em log. Quando você tiver as contas, basta implementar o envio nesse ponto — nenhum outro arquivo precisa mudar.

---

## Ponto único de integração

`src/PosCorte.API/Services/NotificacaoService.cs`

```csharp
public async Task NotificarEventoAsync(NotificacaoEvento evento, int projetoId, string mensagem)
{
    // STUB hoje. Implemente aqui o envio real (WhatsApp/e-mail).
    _logger.LogInformation("[NOTIFICACAO] {Evento} · Projeto {ProjetoId} · {Mensagem}", evento, projetoId, mensagem);
}
```

Eventos já disparados pelo sistema (enum `NotificacaoEvento`):

| Evento | Quando acontece |
|--------|-----------------|
| `PagamentoConfirmado` | PIX confirmado (webhook/stub) |
| `MontadorAlocado` | Provedor retorna montador |
| `MontagemConcluida` | Projeto concluído (aprovação ou liberação automática) |
| `DisputaAberta` | Arquiteto contesta a montagem |
| `ProjetoCriado` | (reservado) |

---

## O que VOCÊ precisa providenciar

### Opção WhatsApp

| Provedor | Observação |
|----------|------------|
| **Z-API** | Mais simples/barato no Brasil (~R$ 100/mês). Precisa de número dedicado. |
| **Twilio WhatsApp** | Robusto, global, exige aprovação de templates pela Meta. |

Você obtém: **instance ID / token** (Z-API) ou **Account SID + Auth Token + From** (Twilio).

### Opção E-mail

| Provedor | Observação |
|----------|------------|
| **Resend** | Moderno, fácil, bom free tier. |
| **SendGrid** | Tradicional, robusto. |

Você obtém: **API Key** + domínio remetente verificado.

---

## Variáveis sugeridas (quando for implementar)

```
Notificacoes__WhatsApp__Provedor   = ZApi | Twilio
Notificacoes__WhatsApp__Token      = ...
Notificacoes__WhatsApp__Instancia  = ...        (Z-API)
Notificacoes__WhatsApp__NumeroAdmin = 5511...   (seu número p/ receber cópia)

Notificacoes__Email__ApiKey        = ...
Notificacoes__Email__Remetente     = no-reply@poscorte.com.br
```

> **Nunca** comite essas chaves. Use variáveis de ambiente / user-secrets.

---

## Estratégia recomendada (primeiros 90 dias)

Conforme o `PLANO_EXECUCAO_COMPLETO.md` (Hack #5 — “WhatsApp como UI”): nos primeiros meses, **todo evento crítico também pinga o seu número**. Você tem controle total da operação antes de automatizar 100%.
