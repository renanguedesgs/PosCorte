using PosCorte.Domain.Validation;
using Xunit;

namespace PosCorte.Tests.Validation
{
    public class DocumentoBrasilTests
    {
        [Theory]
        [InlineData("529.982.247-25")]
        [InlineData("52998224725")]
        public void CpfValido_DevePassar(string cpf)
        {
            Assert.True(DocumentoBrasil.EhCpfValido(cpf));
        }

        [Theory]
        [InlineData("111.111.111-11")]
        [InlineData("123")]
        [InlineData("00000000000")]
        public void CpfInvalido_DeveFalhar(string cpf)
        {
            Assert.False(DocumentoBrasil.EhCpfValido(cpf));
        }

        [Theory]
        [InlineData("11.222.333/0001-81")]
        public void CnpjValido_DevePassar(string cnpj)
        {
            Assert.True(DocumentoBrasil.EhCnpjValido(cnpj));
        }

        [Theory]
        [InlineData("11.111.111/1111-11")]
        [InlineData("123")]
        public void CnpjInvalido_DeveFalhar(string cnpj)
        {
            Assert.False(DocumentoBrasil.EhCnpjValido(cnpj));
        }
    }

    public class SenhaPolicyTests
    {
        [Theory]
        [InlineData("SenhaForte1!")]
        [InlineData("Admin@PosCorte2026")]
        public void SenhaForte_DevePassar(string senha)
        {
            Assert.True(SenhaPolicy.EhSenhaForte(senha));
            Assert.Null(SenhaPolicy.ObterErro(senha));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("abcdefgh")]
        [InlineData("Abcdefgh")]
        [InlineData("Abcdefg1")]
        public void SenhaFraca_DeveFalhar(string senha)
        {
            Assert.False(SenhaPolicy.EhSenhaForte(senha));
            Assert.NotNull(SenhaPolicy.ObterErro(senha));
        }
    }
}
