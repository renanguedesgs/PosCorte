using PosCorte.Domain.Entities;

namespace PosCorte.Tests.Fixtures
{
    public static class MockData
    {
        public static Usuario CriarUsuarioMock(int id = 1) => new Usuario(
            "Arquiteto Teste",
            "arquiteto@teste.com",
            "12345678000195",
            "11999990000"
        ) { Id = id };

        public static Projeto CriarProjetoMock(int id = 1, int usuarioId = 1) => new Projeto(
            usuarioId,
            "Projeto Cozinha",
            "https://storage.cloud/projeto-123.pdf",
            20,
            4,
            "01310-100",
            "Avenida Paulista, 1000 - São Paulo/SP"
        ) { Id = id };

        public static OrdemServico CriarOrdemMock(int id = 1, int projetoId = 1) => new OrdemServico(
            projetoId,
            "EXT-12345"
        ) { Id = id };
    }
}
