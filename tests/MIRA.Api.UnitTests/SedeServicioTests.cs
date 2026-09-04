using MIRA.Api.Excepciones;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;

namespace MIRA.Api.UnitTests;

public class SedeServicioTests
{
    [Fact]
    public async Task ActualizarAsync_WhenSedeDoesNotExist_ThrowsNotFound()
    {
        var service = new SedeServicio(new SedeRepositorioFake(returnNullOnUpdate: true));

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            service.ActualizarAsync(999, "Sede", "Ciudad", true, CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task CrearAsync_WithValidData_ReturnsSede()
    {
        var service = new SedeServicio(new SedeRepositorioFake());

        var sede = await service.CrearAsync("Principal", "Medellin", CancellationToken.None);

        Assert.Equal("Principal", sede.Nombre);
    }

    private sealed class SedeRepositorioFake(bool returnNullOnUpdate = false) : ISedeRepositorio
    {
        public Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Sede>>([
                new Sede { Id = 1, Nombre = "Principal", Ciudad = "Medellin", Activa = true }
            ]);

        public Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken)
            => Task.FromResult(new Sede { Id = 1, Nombre = nombre, Ciudad = ciudad, Activa = true });

        public Task<Sede?> ActualizarAsync(int id, string nombre, string ciudad, bool activa, CancellationToken cancellationToken)
            => Task.FromResult(returnNullOnUpdate
                ? null
                : new Sede { Id = id, Nombre = nombre, Ciudad = ciudad, Activa = activa });

        public Task<bool> DesactivarAsync(int id, CancellationToken cancellationToken)
            => Task.FromResult(id == 1);
    }
}
