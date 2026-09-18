using MIRA.Api.Excepciones;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios;
using MIRA.Api.Servicios;

namespace MIRA.Api.UnitTests;

public class CatalogoServicioTests
{
    [Fact]
    public async Task CrearAsync_WithInvalidType_ThrowsNotFound()
    {
        var service = new CatalogoServicio(new CatalogoRepositorioFake());

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            service.CrearAsync("invalido", "x", CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task ListarAsync_ReturnsItems()
    {
        var service = new CatalogoServicio(new CatalogoRepositorioFake());

        var items = await service.ListarAsync("ods", CancellationToken.None);

        Assert.Single(items);
        Assert.Equal("ODS 4", items[0].Nombre);
    }

    private sealed class CatalogoRepositorioFake : ICatalogoRepositorio
    {
        public Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<CatalogoItem>>([
                new CatalogoItem { Id = 1, Nombre = "ODS 4", Activo = true }
            ]);

        public Task<CatalogoItem> CrearAsync(string tipo, string nombre, CancellationToken cancellationToken)
            => Task.FromResult(new CatalogoItem { Id = 2, Nombre = nombre, Activo = true });

        public Task<CatalogoItem?> ObtenerPorIdAsync(string tipo, int id, CancellationToken cancellationToken)
            => Task.FromResult<CatalogoItem?>(new CatalogoItem { Id = id, Nombre = "ODS 4", Activo = true });

        public Task<CatalogoItem?> ActualizarAsync(string tipo, int id, string nombre, bool activo, CancellationToken cancellationToken)
            => Task.FromResult<CatalogoItem?>(new CatalogoItem { Id = id, Nombre = nombre, Activo = activo });

        public Task<bool> DesactivarAsync(string tipo, int id, CancellationToken cancellationToken)
            => Task.FromResult(true);
    }
}
