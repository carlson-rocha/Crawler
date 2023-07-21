using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using ExtratoClubeAPI.Models;
using ExtratoClubeAPI.Services;
using ExtratoClubeAPI.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ExtratoClubeAPI.Tests
{
    [TestClass]
    public class ExtratoClubeTests
    {
        [TestMethod]
        public async Task TestarServicoBuscarBeneficios()
        {
            // Arrange
            var credenciais = new Credencial { Cpf = "12345678900", Usuario = "testekonsi", Senha = "testekonsi" };
            var beneficiosEsperados = new List<string> { "123456", "12345", "67890" };
            var extratoClubeService = new CrawlerService();

            // Act
            var beneficios = extratoClubeService.ConsultarBeneficios(credenciais);

            // Assert
            CollectionAssert.AreEqual(beneficiosEsperados, beneficios);
        }

        [TestMethod]
        public async Task TestarControllerBuscarBeneficios()
        {
            // Arrange
            var credenciais = new Credencial { Cpf = "12345678900", Usuario = "testekonsi", Senha = "testekonsi" };
            var beneficiosEsperados = new List<string> { "12345", "67890" };

            var extratoClubeServiceMock = new Mock<ICrawlerService>();
            extratoClubeServiceMock.Setup(x => x.ConsultarBeneficios(credenciais)).Returns(beneficiosEsperados);

            var controller = new ExtratoClubeController(extratoClubeServiceMock.Object);

            // Act
            var result = controller.ConsultarBeneficios(credenciais) as OkObjectResult;
            var beneficios = result.Value as List<string>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(beneficios);
            CollectionAssert.AreEqual(beneficiosEsperados, beneficios);
        }
    }
}
