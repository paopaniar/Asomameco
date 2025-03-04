using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using Asomameco.Application.Services.Implementations;
using Asomameco.Application.DTOs;
using Asomameco.Infraestructure.Repository.Interfaces;
using AutoMapper;

namespace Asomameco.Tests
{
    public class CrearAsambleaTest
    {
        private readonly Mock<IRepositoryAsamblea> _asambleaRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ServiceAsamblea _asambleaService;

        public CrearAsambleaTest()
        {
            // Arrange - Inicializar Mocks
            _asambleaRepositoryMock = new Mock<IRepositoryAsamblea>();
            _mapperMock = new Mock<IMapper>();

            // Inyectar Mocks en el servicio
            _asambleaService = new ServiceAsamblea(_asambleaRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CrearAsamblea_DeberiaRetornarIdAsamblea_CuandoEsExitosa()
        {
            // Arrange
            var asambleaDto = new AsambleaDTO
            {
                Id = 1,
                Fecha = DateTime.Now,
                Estado = 1 // "Registrado"
            };

            var asambleaEntidad = new Asomameco.Infraestructure.Models.Asamblea
            {
                Id = 1,
                Fecha = asambleaDto.Fecha,
                Estado = asambleaDto.Estado
            };

            // Configurar el mock de AutoMapper
            _mapperMock.Setup(mapper => mapper.Map<Asomameco.Infraestructure.Models.Asamblea>(asambleaDto))
                       .Returns(asambleaEntidad);

            // Configurar el mock del repositorio para que retorne el ID de la asamblea creada
            _asambleaRepositoryMock.Setup(repo => repo.AddAsync(asambleaEntidad))
                                   .ReturnsAsync(asambleaEntidad.Id);

            // Act
            var resultado = await _asambleaService.AddAsync(asambleaDto);

            // Assert
            Assert.Equal(1, resultado);
            _asambleaRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Asomameco.Infraestructure.Models.Asamblea>()), Times.Once);
        }
    }
}
