# 📋 GUIA DE IMPLEMENTAÇÃO COMPLETO - PósCorte
## Backend-First: Lógica de Negócio Sem Dependência de BD

---

## 🎯 Objetivo Final
Implementar uma plataforma SaaS de intermediação tecnológica para serviços de montagem de móveis planejados, com orquestração de equipes de campo via APIs parceiras, processamento de pagamentos e garantia financeira via Escrow.

**Abordagem:** Backend-first com lógica de negócio desacoplada, permitindo troca de persistência posteriormente.

---

## 📊 FASE 1: PLANEJAMENTO E ARQUITETURA

### 1.1 Estrutura de Pastas do Projeto

```
PosCorte/
│
├── src/
│   ├── PosCorte.API/                 # Projeto principal ASP.NET Core
│   │   ├── Controllers/
│   │   │   ├── UsuariosController.cs
│   │   │   ├── ProjetosController.cs
│   │   │   ├── OrdensServicoController.cs
│   │   │   └── WebhookPoscorteController.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── Usuario.cs
│   │   │   ├── Projeto.cs
│   │   │   ├── OrdemServico.cs
│   │   │   ├── OrcamentoResultado.cs
│   │   │   └── WebhookData.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── PrecificacaoService.cs
│   │   │   ├── ProvedorService.cs
│   │   │   ├── PagamentoService.cs
│   │   │   ├── NotificacaoService.cs
│   │   │   └── RepositorioService.cs      # Abstração de repositório
│   │   │
│   │   ├── Interfaces/
│   │   │   ├── IRepositorio.cs            # Interface genérica de dados
│   │   │   ├── IPrecificacaoService.cs
│   │   │   ├── IProvedorService.cs
│   │   │   ├── IPagamentoService.cs
│   │   │   └── INotificacaoService.cs
│   │   │
│   │   ├── Middleware/
│   │   │   ├── ErroMiddleware.cs
│   │   │   └── LoggingMiddleware.cs
│   │   │
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Program.cs
│   │   └── README.md
│   │
│   ├── PosCorte.Tests/               # Projeto de Testes Unitários
│   │   ├── Services/
│   │   │   ├── PrecificacaoServiceTests.cs
│   │   │   ├── ProvedorServiceTests.cs
│   │   │   └── PagamentoServiceTests.cs
│   │   │
│   │   ├── Controllers/
│   │   │   ├── UsuariosControllerTests.cs
│   │   │   ├── ProjetosControllerTests.cs
│   │   │   └── WebhookTests.cs
│   │   │
│   │   └── Fixtures/
│   │       └── MockData.cs
│   │
│   └── PosCorte.Domain/              # Camada de Domínio (Entities)
│       ├── Entities/
│       │   ├── Usuario.cs
│       │   ├── Projeto.cs
│       │   └── OrdemServico.cs
│       └── ValueObjects/
│           ├── Orcamento.cs
│           └── Endereco.cs
│
├── docs/
│   ├── ARQUITETURA.md
│   ├── API_SPECIFICATION.md
│   ├── WEBHOOK_GUIDE.md
│   └── REGRAS_NEGOCIO.md
│
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
│
└── PosCorte.sln
```

### 1.2 Stack Tecnológico

| Componente | Tecnologia | Versão |
|-----------|-----------|--------|
| **Backend** | ASP.NET Core | 8.0+ |
| **Linguagem** | C# | Latest |
| **Web Framework** | Minimal APIs / Controllers | 8.0+ |
| **DI Container** | Built-in | - |
| **Logging** | Serilog | Latest |
| **Testes** | xUnit + Moq | Latest |
| **HTTP Client** | Refit | Latest |
| **Validação** | FluentValidation | Latest |
| **Mapper** | AutoMapper | Latest |
| **Containerização** | Docker | Latest |

---

## 🛠️ FASE 2: CONFIGURAÇÃO INICIAL DO PROJETO

### 2.1 Criar Solução e Projetos

```bash
# Criar solução
dotnet new sln -n PosCorte

# Criar projeto API
dotnet new webapi -n PosCorte.API
dotnet sln add src/PosCorte.API/PosCorte.API.csproj

# Criar projeto Domain
dotnet new classlib -n PosCorte.Domain
dotnet sln add src/PosCorte.Domain/PosCorte.Domain.csproj

# Criar projeto Tests
dotnet new xunit -n PosCorte.Tests
dotnet sln add src/PosCorte.Tests/PosCorte.Tests.csproj

# Adicionar referências
cd src/PosCorte.API
dotnet add reference ../PosCorte.Domain/PosCorte.Domain.csproj

cd ../PosCorte.Tests
dotnet add reference ../PosCorte.API/PosCorte.API.csproj
```

### 2.2 Instalar NuGet Packages Essenciais

```bash
cd src/PosCorte.API

# Logging
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File

# Validação
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package FluentValidation.AspNetCore

# Autenticação
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# HTTP Client
dotnet add package Refit
dotnet add package Refit.HttpClientFactory

# Mapper
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Utilitários
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
```

---

## 📦 FASE 3: MODELS E ENTITIES (SEM BANCO DE DADOS)

### 3.1 Criar Models de Domínio

**Criar arquivo:** `src/PosCorte.Domain/Entities/Usuario.cs`

```csharp
namespace PosCorte.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string CpfCnpj { get; set; }
        public string Telefone { get; set; }
        public DateTime DataCadastro { get; set; }

        public Usuario() { }

        public Usuario(string nome, string email, string cpfCnpj, string telefone)
        {
            Nome = nome;
            Email = email;
            CpfCnpj = cpfCnpj;
            Telefone = telefone;
            DataCadastro = DateTime.UtcNow;
        }
    }
}
```

**Criar arquivo:** `src/PosCorte.Domain/Entities/Projeto.cs`

```csharp
namespace PosCorte.Domain.Entities
{
    public class Projeto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; }
        public string UrlArquivoCorteCloud { get; set; }
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; }
        public string EnderecoCompleto { get; set; }
        public string StatusProjeto { get; set; }
        public DateTime DataCriacao { get; set; }

        public Projeto() { }

        public Projeto(int usuarioId, string nomeProjeto, string urlArquivo, 
                      int qtdPecas, int qtdGavetas, string cep, string endereco)
        {
            UsuarioId = usuarioId;
            NomeProjeto = nomeProjeto;
            UrlArquivoCorteCloud = urlArquivo;
            QtdPecas = qtdPecas;
            QtdGavetas = qtdGavetas;
            CepObra = cep;
            EnderecoCompleto = endereco;
            StatusProjeto = "Aguardando_Pagamento";
            DataCriacao = DateTime.UtcNow;
        }
    }
}
```

**Criar arquivo:** `src/PosCorte.Domain/Entities/OrdemServico.cs`

```csharp
namespace PosCorte.Domain.Entities
{
    public class OrdemServico
    {
        public int Id { get; set; }
        public int ProjetoId { get; set; }
        public string ExternalProviderId { get; set; }
        public string StatusProvedor { get; set; }
        public string MontadorNome { get; set; }
        public string MontadorTelefone { get; set; }
        public string MontadorFotoUrl { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime DataAtualizacao { get; set; }

        public OrdemServico() { }

        public OrdemServico(int projetoId, string externalProviderId)
        {
            ProjetoId = projetoId;
            ExternalProviderId = externalProviderId;
            StatusProvedor = "Pendente";
            DataAtualizacao = DateTime.UtcNow;
        }
    }
}
```

### 3.2 Criar DTOs (Data Transfer Objects)

**Criar arquivo:** `src/PosCorte.API/Models/DTOs/OrcamentoResultado.cs`

```csharp
namespace PosCorte.API.Models.DTOs
{
    public class OrcamentoResultado
    {
        public decimal ValorTotal { get; set; }
        public decimal CustoPrestador { get; set; }
        public decimal MargemLucro { get; set; }
        public decimal TaxaPlataforma { get; set; }
    }
}
```

**Criar arquivo:** `src/PosCorte.API/Models/DTOs/UsuarioDTO.cs`

```csharp
namespace PosCorte.API.Models.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
    }

    public class CreateUsuarioDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string CpfCnpj { get; set; }
        public string Telefone { get; set; }
    }
}
```

**Criar arquivo:** `src/PosCorte.API/Models/DTOs/ProjetoDTO.cs`

```csharp
namespace PosCorte.API.Models.DTOs
{
    public class ProjetoDTO
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; }
        public string UrlArquivoCorteCloud { get; set; }
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; }
        public string EnderecoCompleto { get; set; }
        public string StatusProjeto { get; set; }
    }

    public class CreateProjetoDTO
    {
        public int UsuarioId { get; set; }
        public string NomeProjeto { get; set; }
        public string UrlArquivoCorteCloud { get; set; }
        public int QtdPecas { get; set; }
        public int QtdGavetas { get; set; }
        public string CepObra { get; set; }
        public string EnderecoCompleto { get; set; }
    }
}
```

**Criar arquivo:** `src/PosCorte.API/Models/DTOs/WebhookData.cs`

```csharp
namespace PosCorte.API.Models.DTOs
{
    public class WebhookData
    {
        public string IdExternalProviderId { get; set; }
        public string Status { get; set; }
        public string NomeMontador { get; set; }
        public string TelefoneMontador { get; set; }
        public string FotoMontadorUrl { get; set; }
        public DateTime DataRetorno { get; set; }
    }

    public class WebhookPagamento
    {
        public int ProjetoId { get; set; }
        public string Status { get; set; }
        public string PixId { get; set; }
        public decimal Valor { get; set; }
    }
}
```

---

## 🎮 FASE 4: INTERFACES E ABSTRAÇÕES

### 4.1 Interface de Repositório (Genérica)

**Criar arquivo:** `src/PosCorte.API/Interfaces/IRepositorio.cs`

```csharp
namespace PosCorte.API.Interfaces
{
    public interface IRepositorio<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
```

### 4.2 Interfaces de Serviços

**Criar arquivo:** `src/PosCorte.API/Interfaces/IPrecificacaoService.cs`

```csharp
using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Interfaces
{
    public interface IPrecificacaoService
    {
        OrcamentoResultado ProcessarProjeto(int pecas, int gavetas);
    }
}
```

**Criar arquivo:** `src/PosCorte.API/Interfaces/IProvedorService.cs`

```csharp
using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Interfaces
{
    public interface IProvedorService
    {
        Task<ProvedorResponse> CriarOrdemServicoAsync(ProvedorRequest request);
        Task<ProvedorResponse> ObterStatusAsync(string externalId);
    }

    public class ProvedorRequest
    {
        public string EnderecoCompleto { get; set; }
        public string Cep { get; set; }
        public DateTime DataAgendamento { get; set; }
        public decimal ValorTotal { get; set; }
        public string UrlPlano { get; set; }
    }

    public class ProvedorResponse
    {
        public string ExternalProviderId { get; set; }
        public string Status { get; set; }
        public string MontadorNome { get; set; }
        public string MontadorTelefone { get; set; }
    }
}
```

**Criar arquivo:** `src/PosCorte.API/Interfaces/IPagamentoService.cs`

```csharp
namespace PosCorte.API.Interfaces
{
    public interface IPagamentoService
    {
        Task<bool> ValidarPagamentoPixAsync(string pixId, decimal valorEsperado);
        Task<bool> LiquidarFundosAsync(string pixId, decimal valor);
        Task<bool> ReservarFundosAsync(string pixId, decimal valor);
    }
}
```

**Criar arquivo:** `src/PosCorte.API/Interfaces/INotificacaoService.cs`

```csharp
namespace PosCorte.API.Interfaces
{
    public interface INotificacaoService
    {
        Task<bool> NotificarArquiteto(int usuarioId, string mensagem);
        Task<bool> NotificarMontador(string telefoneMontador, string mensagem);
        Task<bool> EnviarEmailConfirmacao(string email, string conteudo);
    }
}
```

---

## 🔧 FASE 5: LÓGICA DE NEGÓCIO (SERVICES)

### 5.1 Serviço de Precificação (Motor de Negócio)

**Criar arquivo:** `src/PosCorte.API/Services/PrecificacaoService.cs`

```csharp
using PosCorte.API.Interfaces;
using PosCorte.API.Models.DTOs;

namespace PosCorte.API.Services
{
    public class PrecificacaoService : IPrecificacaoService
    {
        private const decimal CUSTO_FIXO_PECA = 12.50m;
        private const decimal CUSTO_FIXO_GAVETA = 40.00m;
        private const decimal MARKUP_PLATAFORMA = 0.20m;

        private readonly ILogger<PrecificacaoService> _logger;

        public PrecificacaoService(ILogger<PrecificacaoService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processa orçamento utilizando fórmula de Markup Inverso
        /// Preço Final = Custo / (1 - Taxa)
        /// </summary>
        public OrcamentoResultado ProcessarProjeto(int pecas, int gavetas)
        {
            _logger.LogInformation($"Processando orçamento: {pecas} peças, {gavetas} gavetas");

            // Validar entrada
            if (pecas < 0 || gavetas < 0)
                throw new ArgumentException("Quantidade de peças e gavetas não podem ser negativas.");

            if (pecas == 0 && gavetas == 0)
                throw new ArgumentException("Projeto deve ter pelo menos uma peça ou gaveta.");

            // Calcular custo de mão de obra
            decimal custoMaoDeObra = (pecas * CUSTO_FIXO_PECA) + (gavetas * CUSTO_FIXO_GAVETA);

            // Aplicar Markup Inverso: Preço Final = Custo / (1 - Taxa)
            // Taxa de 20% significa que a plataforma retém 20% do valor final
            decimal precoFinal = custoMaoDeObra / (1 - MARKUP_PLATAFORMA);
            decimal margemLucro = precoFinal - custoMaoDeObra;

            var resultado = new OrcamentoResultado
            {
                ValorTotal = Math.Round(precoFinal, 2),
                CustoPrestador = Math.Round(custoMaoDeObra, 2),
                MargemLucro = Math.Round(margemLucro, 2),
                TaxaPlataforma = Math.Round(MARKUP_PLATAFORMA * 100, 2)
            };

            _logger.LogInformation($"Orçamento calculado: Total R${resultado.ValorTotal}, Margem R${resultado.MargemLucro}");

            return resultado;
        }
    }
}
```

### 5.2 Serviço de Provedor (Integração com API Externa)

**Criar arquivo:** `src/PosCorte.API/Services/ProvedorService.cs`

```csharp
using Refit;
using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    public interface IProvedorApi
    {
        [Post("/ordensservico")]
        Task<ProvedorResponse> CriarOrdemServico([Body] ProvedorRequest request);

        [Get("/ordensservico/{externalId}")]
        Task<ProvedorResponse> ObterStatusOrdem(string externalId);
    }

    public class ProvedorService : IProvedorService
    {
        private readonly IProvedorApi _provedorApi;
        private readonly ILogger<ProvedorService> _logger;

        public ProvedorService(IProvedorApi provedorApi, ILogger<ProvedorService> logger)
        {
            _provedorApi = provedorApi;
            _logger = logger;
        }

        public async Task<ProvedorResponse> CriarOrdemServicoAsync(ProvedorRequest request)
        {
            try
            {
                _logger.LogInformation($"Criando ordem de serviço para: {request.EnderecoCompleto}");

                var resposta = await _provedorApi.CriarOrdemServico(request);

                _logger.LogInformation($"Ordem criada com sucesso. ID Externo: {resposta.ExternalProviderId}");

                return resposta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar ordem no provedor");
                throw new InvalidOperationException("Falha na comunicação com provedor de montadores", ex);
            }
        }

        public async Task<ProvedorResponse> ObterStatusAsync(string externalId)
        {
            try
            {
                _logger.LogInformation($"Consultando status de ordem: {externalId}");

                return await _provedorApi.ObterStatusOrdem(externalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao obter status da ordem {externalId}");
                throw;
            }
        }
    }
}
```

### 5.3 Serviço de Pagamento

**Criar arquivo:** `src/PosCorte.API/Services/PagamentoService.cs`

```csharp
using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    public class PagamentoService : IPagamentoService
    {
        private readonly ILogger<PagamentoService> _logger;

        public PagamentoService(ILogger<PagamentoService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Valida se o pagamento foi confirmado pela gateway (Asaas/Iugu)
        /// TRAVA DE SEGURANÇA CRITICAL: Ordem só é criada se isso retornar true
        /// </summary>
        public async Task<bool> ValidarPagamentoPixAsync(string pixId, decimal valorEsperado)
        {
            _logger.LogInformation($"Validando pagamento PIX: {pixId}, Valor: R${valorEsperado}");

            try
            {
                // TODO: Integrar com API real de pagamento (Asaas/Iugu)
                await Task.Delay(100); // Simular latência de rede

                _logger.LogInformation($"Pagamento validado: {pixId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar pagamento");
                return false;
            }
        }

        /// <summary>
        /// Liquida os fundos do Escrow após conclusão da vistoria (72h úteis)
        /// </summary>
        public async Task<bool> LiquidarFundosAsync(string pixId, decimal valor)
        {
            _logger.LogInformation($"Liquidando fundos de Escrow: {pixId}, Valor: R${valor}");

            try
            {
                // TODO: Integrar com API real de pagamento para split de fundos
                await Task.Delay(100);

                _logger.LogInformation($"Fundos liquidados: {pixId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao liquidar fundos");
                return false;
            }
        }

        /// <summary>
        /// Reserva fundos em Escrow até conclusão do serviço
        /// </summary>
        public async Task<bool> ReservarFundosAsync(string pixId, decimal valor)
        {
            _logger.LogInformation($"Reservando fundos em Escrow: {pixId}, Valor: R${valor}");

            try
            {
                // TODO: Integrar com API real de pagamento
                await Task.Delay(100);

                _logger.LogInformation($"Fundos reservados: {pixId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reservar fundos");
                return false;
            }
        }
    }
}
```

### 5.4 Serviço de Notificação

**Criar arquivo:** `src/PosCorte.API/Services/NotificacaoService.cs`

```csharp
using PosCorte.API.Interfaces;

namespace PosCorte.API.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly ILogger<NotificacaoService> _logger;

        public NotificacaoService(ILogger<NotificacaoService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> NotificarArquiteto(int usuarioId, string mensagem)
        {
            _logger.LogInformation($"Notificando arquiteto {usuarioId}: {mensagem}");

            try
            {
                // TODO: Integrar com serviço de notificação (Push, Email, SMS)
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar arquiteto");
                return false;
            }
        }

        public async Task<bool> NotificarMontador(string telefoneMontador, string mensagem)
        {
            _logger.LogInformation($"Notificando montador {telefoneMontador}: {mensagem}");

            try
            {
                // TODO: Integrar com serviço de SMS/WhatsApp
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao notificar montador");
                return false;
            }
        }

        public async Task<bool> EnviarEmailConfirmacao(string email, string conteudo)
        {
            _logger.LogInformation($"Enviando email para: {email}");

            try
            {
                // TODO: Integrar com serviço de email (SendGrid, Mailgun, etc)
                await Task.Delay(100);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email");
                return false;
            }
        }
    }
}
```

---

## 🌐 FASE 6: CONTROLLERS E ENDPOINTS

### 6.1 Controller de Usuários

**Criar arquivo:** `src/PosCorte.API/Controllers/UsuariosController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/usuarios")]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly IRepositorio<PosCorte.Domain.Entities.Usuario> _usuarioRepo;

        public UsuariosController(
            ILogger<UsuariosController> logger,
            IRepositorio<PosCorte.Domain.Entities.Usuario> usuarioRepo)
        {
            _logger = logger;
            _usuarioRepo = usuarioRepo;
        }

        /// <summary>
        /// Criar novo usuário (Arquiteto/Designer)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioDTO>> CriarUsuario([FromBody] CreateUsuarioDTO dto)
        {
            _logger.LogInformation($"Criando usuário: {dto.Email}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var usuario = new PosCorte.Domain.Entities.Usuario(
                    dto.Nome,
                    dto.Email,
                    dto.CpfCnpj,
                    dto.Telefone
                );

                var usuarioCriado = await _usuarioRepo.AddAsync(usuario);
                await _usuarioRepo.SaveChangesAsync();

                var usuarioRetorno = new UsuarioDTO
                {
                    Id = usuarioCriado.Id,
                    Nome = usuarioCriado.Nome,
                    Email = usuarioCriado.Email,
                    Telefone = usuarioCriado.Telefone
                };

                return CreatedAtAction(nameof(ObterUsuario), new { id = usuarioCriado.Id }, usuarioRetorno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário");
                return StatusCode(500, new { error = "Erro ao criar usuário" });
            }
        }

        /// <summary>
        /// Obter usuário por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDTO>> ObterUsuario(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);

            if (usuario == null)
                return NotFound(new { error = "Usuário não encontrado" });

            return Ok(new UsuarioDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Telefone = usuario.Telefone
            });
        }

        /// <summary>
        /// Listar todos os usuários
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> ListarUsuarios()
        {
            var usuarios = await _usuarioRepo.GetAllAsync();

            var dtos = usuarios.Select(u => new UsuarioDTO
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Telefone = u.Telefone
            });

            return Ok(dtos);
        }
    }
}
```

### 6.2 Controller de Projetos

**Criar arquivo:** `src/PosCorte.API/Controllers/ProjetosController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/projetos")]
    [Produces("application/json")]
    public class ProjetosController : ControllerBase
    {
        private readonly ILogger<ProjetosController> _logger;
        private readonly IRepositorio<Projeto> _projetoRepo;
        private readonly IRepositorio<Usuario> _usuarioRepo;
        private readonly IPrecificacaoService _precificacaoService;

        public ProjetosController(
            ILogger<ProjetosController> logger,
            IRepositorio<Projeto> projetoRepo,
            IRepositorio<Usuario> usuarioRepo,
            IPrecificacaoService precificacaoService)
        {
            _logger = logger;
            _projetoRepo = projetoRepo;
            _usuarioRepo = usuarioRepo;
            _precificacaoService = precificacaoService;
        }

        /// <summary>
        /// Criar novo projeto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjetoDTO>> CriarProjeto([FromBody] CreateProjetoDTO dto)
        {
            _logger.LogInformation($"Criando projeto: {dto.NomeProjeto}");

            // Validar usuário
            var usuario = await _usuarioRepo.GetByIdAsync(dto.UsuarioId);
            if (usuario == null)
                return BadRequest(new { error = "Usuário não encontrado" });

            try
            {
                var projeto = new Projeto(
                    dto.UsuarioId,
                    dto.NomeProjeto,
                    dto.UrlArquivoCorteCloud,
                    dto.QtdPecas,
                    dto.QtdGavetas,
                    dto.CepObra,
                    dto.EnderecoCompleto
                );

                var projetoCriado = await _projetoRepo.AddAsync(projeto);
                await _projetoRepo.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterProjeto), new { id = projetoCriado.Id }, 
                    MapToDTO(projetoCriado));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar projeto");
                return StatusCode(500, new { error = "Erro ao criar projeto" });
            }
        }

        /// <summary>
        /// Calcular orçamento de montagem
        /// Usa fórmula de Markup Inverso com taxa de 20%
        /// </summary>
        [HttpPost("{id}/calcular-orcamento")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrcamentoResultado>> CalcularOrcamento(
            int id, 
            [FromBody] OrcamentoRequest request)
        {
            var projeto = await _projetoRepo.GetByIdAsync(id);
            if (projeto == null)
                return NotFound(new { error = "Projeto não encontrado" });

            try
            {
                var resultado = _precificacaoService.ProcessarProjeto(
                    request.QtdPecas ?? projeto.QtdPecas,
                    request.QtdGavetas ?? projeto.QtdGavetas
                );

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obter projeto por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjetoDTO>> ObterProjeto(int id)
        {
            var projeto = await _projetoRepo.GetByIdAsync(id);

            if (projeto == null)
                return NotFound(new { error = "Projeto não encontrado" });

            return Ok(MapToDTO(projeto));
        }

        /// <summary>
        /// Listar todos os projetos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProjetoDTO>>> ListarProjetos()
        {
            var projetos = await _projetoRepo.GetAllAsync();
            return Ok(projetos.Select(MapToDTO));
        }

        private static ProjetoDTO MapToDTO(Projeto projeto)
        {
            return new ProjetoDTO
            {
                Id = projeto.Id,
                UsuarioId = projeto.UsuarioId,
                NomeProjeto = projeto.NomeProjeto,
                UrlArquivoCorteCloud = projeto.UrlArquivoCorteCloud,
                QtdPecas = projeto.QtdPecas,
                QtdGavetas = projeto.QtdGavetas,
                CepObra = projeto.CepObra,
                EnderecoCompleto = projeto.EnderecoCompleto,
                StatusProjeto = projeto.StatusProjeto
            };
        }
    }

    public class OrcamentoRequest
    {
        public int? QtdPecas { get; set; }
        public int? QtdGavetas { get; set; }
    }
}
```

### 6.3 Controller de Webhooks (CRÍTICO)

**Criar arquivo:** `src/PosCorte.API/Controllers/WebhookPoscorteController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.API.Controllers
{
    [ApiController]
    [Route("api/v1/webhooks")]
    [Produces("application/json")]
    public class WebhookPoscorteController : ControllerBase
    {
        private readonly ILogger<WebhookPoscorteController> _logger;
        private readonly IRepositorio<Projeto> _projetoRepo;
        private readonly IRepositorio<OrdemServico> _ordemRepo;
        private readonly IPagamentoService _pagamentoService;
        private readonly IProvedorService _provedorService;
        private readonly INotificacaoService _notificacaoService;

        public WebhookPoscorteController(
            ILogger<WebhookPoscorteController> logger,
            IRepositorio<Projeto> projetoRepo,
            IRepositorio<OrdemServico> ordemRepo,
            IPagamentoService pagamentoService,
            IProvedorService provedorService,
            INotificacaoService notificacaoService)
        {
            _logger = logger;
            _projetoRepo = projetoRepo;
            _ordemRepo = ordemRepo;
            _pagamentoService = pagamentoService;
            _provedorService = provedorService;
            _notificacaoService = notificacaoService;
        }

        /// <summary>
        /// Webhook: Pagamento confirmado via PIX
        /// TRAVA DE SEGURANÇA: Ordem só é criada após confirmação de pagamento
        /// </summary>
        [HttpPost("pagamento-confirmado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TratarPagamentoConfirmado([FromBody] WebhookPagamento dados)
        {
            _logger.LogInformation($"Webhook pagamento recebido: Projeto {dados.ProjetoId}, Status: {dados.Status}");

            try
            {
                var projeto = await _projetoRepo.GetByIdAsync(dados.ProjetoId);
                if (projeto == null)
                    return NotFound(new { error = "Projeto não encontrado" });

                if (dados.Status == "pago")
                {
                    // VALIDAR PAGAMENTO COM GATEWAY
                    bool pagamentoValido = await _pagamentoService.ValidarPagamentoPixAsync(
                        dados.PixId, 
                        dados.Valor
                    );

                    if (!pagamentoValido)
                        return BadRequest(new { error = "Pagamento não validado" });

                    // RESERVAR FUNDOS EM ESCROW
                    bool fundosReservados = await _pagamentoService.ReservarFundosAsync(
                        dados.PixId,
                        dados.Valor
                    );

                    if (!fundosReservados)
                        return BadRequest(new { error = "Falha ao reservar fundos em escrow" });

                    projeto.StatusProjeto = "Pagamento_Confirmado";

                    // CRIAR ORDEM DE SERVIÇO
                    var request = new ProvedorService.ProvedorRequest
                    {
                        EnderecoCompleto = projeto.EnderecoCompleto,
                        Cep = projeto.CepObra,
                        DataAgendamento = DateTime.UtcNow.AddDays(1),
                        ValorTotal = dados.Valor,
                        UrlPlano = projeto.UrlArquivoCorteCloud
                    };

                    var provedorResponse = await _provedorService.CriarOrdemServicoAsync(request);

                    var ordem = new OrdemServico(projeto.Id, provedorResponse.ExternalProviderId)
                    {
                        StatusProvedor = provedorResponse.Status,
                        MontadorNome = provedorResponse.MontadorNome,
                        MontadorTelefone = provedorResponse.MontadorTelefone
                    };

                    await _ordemRepo.AddAsync(ordem);
                    await _ordemRepo.SaveChangesAsync();

                    projeto.StatusProjeto = "Ordem_Criada";
                    await _projetoRepo.UpdateAsync(projeto);
                    await _projetoRepo.SaveChangesAsync();

                    _logger.LogInformation($"Ordem criada com sucesso para projeto {dados.ProjetoId}");
                }

                return Ok(new { message = "Webhook de pagamento processado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook de pagamento");
                return StatusCode(500, new { error = "Erro ao processar webhook", details = ex.Message });
            }
        }

        /// <summary>
        /// Webhook: Atualização de status do montador
        /// Estados: aceito, a_caminho, concluido, cancelado
        /// </summary>
        [HttpPost("atualizacao-montador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TratarAtualizacaoMontador([FromBody] WebhookData dados)
        {
            _logger.LogInformation($"Webhook montador recebido: ID {dados.IdExternalProviderId}, Status: {dados.Status}");

            try
            {
                var ordensServico = await _ordemRepo.GetAllAsync();
                var ordem = ordensServico.FirstOrDefault(o => o.ExternalProviderId == dados.IdExternalProviderId);

                if (ordem == null)
                    return NotFound(new { error = "Ordem não encontrada" });

                var projeto = await _projetoRepo.GetByIdAsync(ordem.ProjetoId);
                if (projeto == null)
                    return NotFound(new { error = "Projeto não encontrado" });

                // Processar diferentes estados do montador
                if (dados.Status == "aceito")
                {
                    ordem.StatusProvedor = "Prestador_Alocado";
                    ordem.MontadorNome = dados.NomeMontador;
                    ordem.MontadorTelefone = dados.TelefoneMontador;
                    ordem.MontadorFotoUrl = dados.FotoMontadorUrl;

                    projeto.StatusProjeto = "Prestador_Alocado";

                    _logger.LogInformation($"Montador alocado: {dados.NomeMontador}");
                }
                else if (dados.Status == "concluido")
                {
                    ordem.StatusProvedor = "Concluido";
                    projeto.StatusProjeto = "Aguardando_Vistoria";

                    // ENCERRAMENTO POR DECURSO DE PRAZO: Liquidar após 72h úteis
                    _logger.LogInformation($"Serviço concluído. Iniciando contagem de 72h para liquidação");
                    // TODO: Implementar agendador (Hangfire/Quartz)
                }
                else if (dados.Status == "cancelado")
                {
                    ordem.StatusProvedor = "Cancelado";
                    projeto.StatusProjeto = "Cancelado";

                    _logger.LogWarning($"Serviço cancelado para projeto {ordem.ProjetoId}");
                }

                ordem.DataAtualizacao = DateTime.UtcNow;
                await _ordemRepo.UpdateAsync(ordem);
                await _ordemRepo.SaveChangesAsync();

                await _projetoRepo.UpdateAsync(projeto);
                await _projetoRepo.SaveChangesAsync();

                return Ok(new { message = "Atualização de montador processada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook de montador");
                return StatusCode(500, new { error = "Erro ao processar webhook", details = ex.Message });
            }
        }
    }
}
```

---

## 🧪 FASE 7: TESTES UNITÁRIOS

### 7.1 Testes do Serviço de Precificação

**Criar arquivo:** `src/PosCorte.Tests/Services/PrecificacaoServiceTests.cs`

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using PosCorte.API.Services;
using PosCorte.API.Interfaces;

namespace PosCorte.Tests.Services
{
    public class PrecificacaoServiceTests
    {
        private readonly Mock<ILogger<PrecificacaoService>> _loggerMock;
        private readonly IPrecificacaoService _service;

        public PrecificacaoServiceTests()
        {
            _loggerMock = new Mock<ILogger<PrecificacaoService>>();
            _service = new PrecificacaoService(_loggerMock.Object);
        }

        [Fact]
        public void ProcessarProjeto_ComValoresValidos_DeveRetornarOrcamentoCorreto()
        {
            // Arrange
            int pecas = 10;
            int gavetas = 5;

            // Act
            var resultado = _service.ProcessarProjeto(pecas, gavetas);

            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.ValorTotal > resultado.CustoPrestador);
            Assert.True(resultado.MargemLucro > 0);
            Assert.Equal(20, resultado.TaxaPlataforma);
        }

        [Theory]
        [InlineData(1, 0, 15.63)]   // 12.50 / 0.80
        [InlineData(0, 1, 50.00)]   // 40.00 / 0.80
        [InlineData(5, 5, 387.50)]  // (62.50 + 200) / 0.80
        [InlineData(10, 10, 775.00)]
        public void ProcessarProjeto_VariosValores_DeveCalcularCorretamente(
            int pecas, int gavetas, double esperado)
        {
            // Act
            var resultado = _service.ProcessarProjeto(pecas, gavetas);

            // Assert
            Assert.Equal(Math.Round((decimal)esperado, 2), resultado.ValorTotal);
        }

        [Fact]
        public void ProcessarProjeto_ComValoresNegativos_DeveLancarExcecao()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.ProcessarProjeto(-5, 5));
            Assert.Throws<ArgumentException>(() => _service.ProcessarProjeto(5, -5));
        }

        [Fact]
        public void ProcessarProjeto_ComZeroPecasEZeroGavetas_DeveLancarExcecao()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.ProcessarProjeto(0, 0));
        }

        [Fact]
        public void ProcessarProjeto_MargemLucroEhCorreta()
        {
            // Arrange
            int pecas = 10;
            int gavetas = 5;
            decimal custoEsperado = (pecas * 12.50m) + (gavetas * 40.00m);
            decimal precoFinalEsperado = custoEsperado / (1 - 0.20m);
            decimal margemEsperada = precoFinalEsperado - custoEsperado;

            // Act
            var resultado = _service.ProcessarProjeto(pecas, gavetas);

            // Assert
            Assert.Equal(Math.Round(margemEsperada, 2), resultado.MargemLucro);
        }
    }
}
```

### 7.2 Testes dos Controllers

**Criar arquivo:** `src/PosCorte.Tests/Controllers/UsuariosControllerTests.cs`

```csharp
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using PosCorte.API.Controllers;
using PosCorte.API.Models.DTOs;
using PosCorte.API.Interfaces;
using PosCorte.Domain.Entities;

namespace PosCorte.Tests.Controllers
{
    public class UsuariosControllerTests
    {
        private readonly Mock<ILogger<UsuariosController>> _loggerMock;
        private readonly Mock<IRepositorio<Usuario>> _usuarioRepoMock;
        private readonly UsuariosController _controller;

        public UsuariosControllerTests()
        {
            _loggerMock = new Mock<ILogger<UsuariosController>>();
            _usuarioRepoMock = new Mock<IRepositorio<Usuario>>();
            _controller = new UsuariosController(_loggerMock.Object, _usuarioRepoMock.Object);
        }

        [Fact]
        public async Task CriarUsuario_ComDadosValidos_DeveRetornar201()
        {
            // Arrange
            var createDto = new CreateUsuarioDTO
            {
                Nome = "João Silva",
                Email = "joao@email.com",
                CpfCnpj = "12345678901234",
                Telefone = "11999999999"
            };

            var usuarioCriado = new Usuario(
                createDto.Nome,
                createDto.Email,
                createDto.CpfCnpj,
                createDto.Telefone
            ) { Id = 1 };

            _usuarioRepoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .ReturnsAsync(usuarioCriado);

            _usuarioRepoMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CriarUsuario(createDto);

            // Assert
            Assert.NotNull(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(UsuariosController.ObterUsuario), createdResult.ActionName);
            Assert.Equal(1, ((UsuarioDTO)createdResult.Value).Id);
        }

        [Fact]
        public async Task ObterUsuario_ComIdValido_DeveRetornarUsuario()
        {
            // Arrange
            var usuario = new Usuario("João", "joao@email.com", "12345678901234", "11999999999") { Id = 1 };
            _usuarioRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(usuario);

            // Act
            var result = await _controller.ObterUsuario(1);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDto = Assert.IsType<UsuarioDTO>(okResult.Value);
            Assert.Equal("João", returnedDto.Nome);
        }

        [Fact]
        public async Task ObterUsuario_ComIdInvalido_DeveRetornar404()
        {
            // Arrange
            _usuarioRepoMock.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Usuario)null);

            // Act
            var result = await _controller.ObterUsuario(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
```

---

## ⚙️ FASE 8: CONFIGURAÇÃO DO PROGRAMA

**Criar arquivo:** `src/PosCorte.API/Program.cs`

```csharp
using Serilog;
using PosCorte.API.Services;
using PosCorte.API.Interfaces;

var builder = WebApplicationBuilder.CreateBuilder(args);

// ===== SERILOG CONFIGURATION =====
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/poscorte-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "PosCorte.API")
    .CreateLogger();

builder.Host.UseSerilog();

// ===== SERVICES REGISTRATION =====

// Business Services
builder.Services.AddScoped<IPrecificacaoService, PrecificacaoService>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

// Refit HTTP Client para integração com Provedor
builder.Services.AddRefitClient<PosCorte.API.Services.IProvedorApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["ProvedorApi:BaseUrl"] ?? "https://api.provider.com"));

builder.Services.AddScoped<IProvedorService, ProvedorService>();

// Repositórios (será implementado na Fase final com BD)
// TODO: Implementar repositórios reais
// builder.Services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioBD<>));

// ===== CONTROLLERS & SWAGGER =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PósCorte API",
        Version = "v1",
        Description = "API de intermediação de serviços de montagem de móveis",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PósCorte",
            Url = new Uri("https://poscorte.com")
        }
    });
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PósCorte API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Criar arquivo:** `src/PosCorte.API/appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/poscorte-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "ProvedorApi": {
    "BaseUrl": "https://api.provider.com"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Criar arquivo:** `src/PosCorte.API/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug"
    }
  },
  "ProvedorApi": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

---

## 🐛 FASE 9: MIDDLEWARE DE ERRO E LOGGING

### 9.1 Middleware de Tratamento de Erros

**Criar arquivo:** `src/PosCorte.API/Middleware/ErroMiddleware.cs`

```csharp
using System.Net;
using System.Text.Json;

namespace PosCorte.API.Middleware
{
    public class ErroMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErroMiddleware> _logger;

        public ErroMiddleware(RequestDelegate next, ILogger<ErroMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ErroMiddleware>>();
            logger.LogError(exception, "Erro não tratado na requisição");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                error = "Erro interno do servidor",
                details = exception.Message,
                timestamp = DateTime.UtcNow
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

**Registrar middleware no Program.cs:**

```csharp
app.UseMiddleware<ErroMiddleware>();
```

---

## 🚀 FASE 10: BUILD E VALIDAÇÃO

### 10.1 Compilar Solução

```bash
dotnet clean
dotnet build
```

### 10.2 Executar Testes

```bash
dotnet test src/PosCorte.Tests/PosCorte.Tests.csproj -v normal
```

### 10.3 Rodar Aplicação

```bash
cd src/PosCorte.API
dotnet run
```

### 10.4 Acessar Swagger

```
https://localhost:5001
```

---

## 📦 FASE 11: DOCKER E DEPLOYMENT

### 11.1 Dockerfile

**Criar arquivo:** `docker/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/PosCorte.API/PosCorte.API.csproj", "PosCorte.API/"]
COPY ["src/PosCorte.Domain/PosCorte.Domain.csproj", "PosCorte.Domain/"]
RUN dotnet restore "PosCorte.API/PosCorte.API.csproj"

COPY . .
RUN dotnet build "src/PosCorte.API/PosCorte.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PosCorte.API/PosCorte.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "PosCorte.API.dll"]
```

### 11.2 Docker Compose

**Criar arquivo:** `docker-compose.yml`

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: docker/Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ProvedorApi__BaseUrl=https://api.provider.com
    volumes:
      - ./logs:/app/logs
    networks:
      - poscorte-network

networks:
  poscorte-network:
    driver: bridge
```

---

## 📋 FASE 12: BANCO DE DADOS (ÚLTIMO PASSO - A FAZER DEPOIS)

**NOTA:** Esta fase será implementada posteriormente com:

- [ ] Entity Framework Core + DbContext
- [ ] Migrations automáticas
- [ ] SQL Server / PostgreSQL
- [ ] Repository Pattern
- [ ] Unit of Work Pattern
- [ ] Connection strings e configurações
- [ ] Seed data inicial

---

## ✅ CHECKLIST DE CONCLUSÃO (FASES 1-11)

- [ ] Fase 1: Arquitetura definida
- [ ] Fase 2: Solução e projetos criados
- [ ] Fase 3: Models e DTOs implementados
- [ ] Fase 4: Interfaces de serviços criadas
- [ ] Fase 5: Services de negócio implementados
- [ ] Fase 6: Controllers com endpoints funcionando
- [ ] Fase 7: Testes unitários com cobertura >= 80%
- [ ] Fase 8: Program.cs e appsettings configurados
- [ ] Fase 9: Middleware de erro implementado
- [ ] Fase 10: Solução compilando e testes passando
- [ ] Fase 11: Docker pronto para deploy
- [ ] [ ] **Fase 12 (FUTURO): Banco de dados integrado**

---

## 📊 ARQUITETURA FINAL (Sem BD)

```
┌─────────────────────────────────────┐
│      HTTP Requests (Swagger)         │
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│      Controllers                     │
│  - UsuariosController               │
│  - ProjetosController               │
│  - WebhookPoscorteController        │
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│      Services (Lógica)              │
│  - PrecificacaoService              │
│  - ProvedorService                  │
│  - PagamentoService                 │
│  - NotificacaoService               │
└────────────┬────────────────────────┘
             │
      ┌──────┴──────┐
      │             │
   External API   Logging (Serilog)
   (Provedor)
```

---

**Versão:** 1.0  
**Status:** Backend-First (Sem BD)  
**Próximo Passo:** Fase 12 - Integração com Banco de Dados

