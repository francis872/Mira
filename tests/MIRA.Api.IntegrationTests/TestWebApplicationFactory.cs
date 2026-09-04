using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MIRA.Api.Modelos;
using MIRA.Api.Repositorios;

namespace MIRA.Api.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ISedeRepositorio>();
            services.RemoveAll<ICatalogoRepositorio>();

            services.AddSingleton<ISedeRepositorio, SedeRepoMemoria>();
            services.AddSingleton<ICatalogoRepositorio, CatalogoRepoMemoria>();
        });
    }

    private sealed class SedeRepoMemoria : ISedeRepositorio
    {
        private readonly List<Sede> _items = [
            new Sede { Id = 1, Nombre = "Sede Medellin", Ciudad = "Medellin", Activa = true }
        ];

        public Task<IReadOnlyList<Sede>> ListarAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Sede>>(_items.OrderBy(x => x.Nombre).ToList());

        public Task<Sede?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
            => Task.FromResult<Sede?>(_items.FirstOrDefault(x => x.Id == id));

        public Task<Sede> CrearAsync(string nombre, string ciudad, CancellationToken cancellationToken)
        {
            var nextId = _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
            var sede = new Sede { Id = nextId, Nombre = nombre, Ciudad = ciudad, Activa = true };
            _items.Add(sede);
            return Task.FromResult(sede);
        }

        public Task<Sede?> ActualizarAsync(int id, string nombre, string ciudad, bool activa, CancellationToken cancellationToken)
        {
            var idx = _items.FindIndex(x => x.Id == id);
            if (idx < 0)
            {
                return Task.FromResult<Sede?>(null);
            }

            _items[idx] = new Sede { Id = id, Nombre = nombre, Ciudad = ciudad, Activa = activa };
            return Task.FromResult<Sede?>(_items[idx]);
        }

        public Task<bool> DesactivarAsync(int id, CancellationToken cancellationToken)
        {
            var idx = _items.FindIndex(x => x.Id == id);
            if (idx < 0)
            {
                return Task.FromResult(false);
            }

            var item = _items[idx];
            _items[idx] = new Sede { Id = item.Id, Nombre = item.Nombre, Ciudad = item.Ciudad, Activa = false };
            return Task.FromResult(true);
        }
    }

    private sealed class CatalogoRepoMemoria : ICatalogoRepositorio
    {
        private readonly Dictionary<string, List<CatalogoItem>> _store = new(StringComparer.OrdinalIgnoreCase)
        {
            ["areas-conocimiento"] = [new CatalogoItem { Id = 1, Nombre = "Ingenieria", Activo = true }],
            ["ods"] = [new CatalogoItem { Id = 1, Nombre = "ODS 4", Activo = true }],
            ["aplicaciones"] = [new CatalogoItem { Id = 1, Nombre = "Educacion", Activo = true }],
            ["palabras-clave"] = [new CatalogoItem { Id = 1, Nombre = "Innovacion", Activo = true }]
        };

        public Task<IReadOnlyList<CatalogoItem>> ListarAsync(string tipo, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<CatalogoItem>>(_store[tipo].OrderBy(x => x.Nombre).ToList());

        public Task<CatalogoItem> CrearAsync(string tipo, string nombre, CancellationToken cancellationToken)
        {
            var nextId = _store[tipo].Count == 0 ? 1 : _store[tipo].Max(x => x.Id) + 1;
            var item = new CatalogoItem { Id = nextId, Nombre = nombre, Activo = true };
            _store[tipo].Add(item);
            return Task.FromResult(item);
        }

        public Task<CatalogoItem?> ObtenerPorIdAsync(string tipo, int id, CancellationToken cancellationToken)
            => Task.FromResult<CatalogoItem?>(_store[tipo].FirstOrDefault(x => x.Id == id));

        public Task<CatalogoItem?> ActualizarAsync(string tipo, int id, string nombre, bool activo, CancellationToken cancellationToken)
        {
            var item = _store[tipo].FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                return Task.FromResult<CatalogoItem?>(null);
            }

            var updated = new CatalogoItem { Id = id, Nombre = nombre, Activo = activo };
            _store[tipo].Remove(item);
            _store[tipo].Add(updated);
            return Task.FromResult<CatalogoItem?>(updated);
        }

        public Task<bool> DesactivarAsync(string tipo, int id, CancellationToken cancellationToken)
        {
            var item = _store[tipo].FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                return Task.FromResult(false);
            }

            _store[tipo].Remove(item);
            _store[tipo].Add(new CatalogoItem { Id = item.Id, Nombre = item.Nombre, Activo = false });
            return Task.FromResult(true);
        }
    }
}
