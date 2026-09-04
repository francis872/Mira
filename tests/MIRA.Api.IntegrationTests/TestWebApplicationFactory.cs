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
            services.RemoveAll<IUsuarioRepositorio>();
            services.RemoveAll<ISedeRepositorio>();
            services.RemoveAll<ICatalogoRepositorio>();

            services.AddSingleton<IUsuarioRepositorio, UsuarioRepoMemoria>();
            services.AddSingleton<ISedeRepositorio, SedeRepoMemoria>();
            services.AddSingleton<ICatalogoRepositorio, CatalogoRepoMemoria>();
        });
    }

    private sealed class UsuarioRepoMemoria : IUsuarioRepositorio
    {
        private readonly Usuario _admin = new()
        {
            Id = 1,
            Nombre = "Administrador MIRA",
            Correo = "admin@mira.local",
            PasswordHash = "Admin123!",
            Roles = ["Administrador", "Docente"]
        };

        public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken)
            => Task.FromResult<Usuario?>(_admin.Correo == correo ? _admin : null);

        public Task<IReadOnlyList<string>> ObtenerRolesAsync(int usuarioId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<string>>(_admin.Roles);

        public Task<bool> AsignarRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
        {
            if (_admin.Roles.Contains(rol, StringComparer.OrdinalIgnoreCase))
            {
                return Task.FromResult(false);
            }

            var list = _admin.Roles.ToList();
            list.Add(rol);
            return Task.FromResult(true);
        }

        public Task<bool> RemoverRolAsync(int usuarioId, string rol, CancellationToken cancellationToken)
            => Task.FromResult(_admin.Roles.Contains(rol, StringComparer.OrdinalIgnoreCase));
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
    }
}
