namespace PosCorte.API.Configuration
{
    /// <summary>
    /// Robô de captação automática de montadores. Quando habilitado e com a chave
    /// do Google Places preenchida, o sistema busca profissionais de montagem por
    /// cidade, cadastra como leads (Verificado=false, Disponivel=false) e dispara
    /// um convite de auto-cadastro — sem você cadastrar ninguém na mão.
    /// </summary>
    public class CaptacaoOptions
    {
        public const string SectionName = "Captacao";

        public bool Enabled { get; set; }

        /// <summary>Chave da Google Places API (Places API - Text Search).</summary>
        public string GooglePlacesApiKey { get; set; } = string.Empty;

        /// <summary>Termos de busca usados no Places (ex.: "montador de móveis planejados").</summary>
        public string[] TermosBusca { get; set; } =
            { "montador de móveis planejados", "montagem de móveis planejados", "marceneiro montador" };

        /// <summary>Cidades-alvo (ex.: "São Paulo, SP"). A captação roda uma busca por cidade × termo.</summary>
        public string[] Cidades { get; set; } = { "São Paulo, SP" };

        /// <summary>Intervalo entre ciclos de captação, em horas.</summary>
        public int IntervaloHoras { get; set; } = 24;

        /// <summary>Máximo de novos leads inseridos por ciclo (evita estouro de cota/custo).</summary>
        public int MaxLeadsPorCiclo { get; set; } = 40;

        /// <summary>Se true, envia convite WhatsApp/e-mail ao lead recém-capturado.</summary>
        public bool EnviarConvite { get; set; } = true;

        public bool EstaConfigurado =>
            Enabled && !string.IsNullOrWhiteSpace(GooglePlacesApiKey)
            && Cidades.Length > 0 && TermosBusca.Length > 0;
    }
}
