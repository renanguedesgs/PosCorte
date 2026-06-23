namespace PosCorte.Web.Services
{
    public static class UiHelper
    {
        public static (string css, string label) StatusProjeto(string status) => status switch
        {
            "Aguardando_Pagamento" => ("st-aguardando", "Aguardando pagamento"),
            "Pagamento_Confirmado" => ("st-confirmado", "Pagamento confirmado"),
            "Ordem_Criada" => ("st-ordem", "Ordem criada"),
            "Aguardando_Provedor" => ("st-confirmado", "Alocando montador"),
            "Prestador_Alocado" => ("st-alocado", "Montador alocado"),
            "Aguardando_Vistoria" => ("st-vistoria", "Aguardando vistoria"),
            "Em_Disputa" => ("st-cancelado", "Em disputa"),
            "Concluido" => ("st-alocado", "Concluído"),
            "Cancelado" => ("st-cancelado", "Cancelado"),
            _ => ("st-default", status)
        };

        public record StepInfo(string Label, string State); // State: done | current | pending | cancel

        /// <summary>Retorna os passos do fluxo do projeto com seu estado, para o stepper visual.</summary>
        public static List<StepInfo> ProgressoProjeto(string status)
        {
            var ordem = new[] { "Aguardando_Pagamento", "Aguardando_Provedor", "Prestador_Alocado", "Aguardando_Vistoria", "Concluido" };
            var labels = new[] { "Pagamento", "Alocação", "Montador", "Vistoria", "Concluído" };

            // normaliza estados intermediários
            var atual = status switch
            {
                "Pagamento_Confirmado" => "Aguardando_Provedor",
                "Ordem_Criada" => "Aguardando_Provedor",
                "Em_Disputa" => "Aguardando_Vistoria",
                _ => status
            };

            var idx = Array.IndexOf(ordem, atual);
            var cancelado = status is "Cancelado";
            var emDisputa = status is "Em_Disputa";

            var steps = new List<StepInfo>();
            for (var i = 0; i < ordem.Length; i++)
            {
                string state;
                if (cancelado) state = i == 0 ? "cancel" : "pending";
                else if (idx < 0) state = "pending";
                else if (i < idx) state = "done";
                else if (i == idx) state = (emDisputa ? "cancel" : "current");
                else state = "pending";
                steps.Add(new StepInfo(labels[i], state));
            }
            return steps;
        }

        public static (string css, string label) StatusProvedor(string status) => status switch
        {
            "Pendente" => ("st-aguardando", "Pendente"),
            "Prestador_Alocado" or "Aceito" => ("st-alocado", "Aceito"),
            "Concluido" => ("st-vistoria", "Concluído"),
            "Cancelado" => ("st-cancelado", "Cancelado"),
            _ => ("st-default", string.IsNullOrEmpty(status) ? "—" : status)
        };

        /// <summary>Gera o HTML de estrelas (cheias/meia/vazias) para uma nota de 0 a 5.</summary>
        public static string Estrelas(decimal nota)
        {
            var cheias = (int)Math.Floor(nota);
            var meia = (nota - cheias) >= 0.5m;
            var html = new System.Text.StringBuilder();
            for (var i = 0; i < cheias && i < 5; i++)
                html.Append("<i class=\"bi bi-star-fill\"></i>");
            if (meia && cheias < 5)
            {
                html.Append("<i class=\"bi bi-star-half\"></i>");
                cheias++;
            }
            for (var i = cheias; i < 5; i++)
                html.Append("<i class=\"bi bi-star\"></i>");
            return html.ToString();
        }
    }
}
