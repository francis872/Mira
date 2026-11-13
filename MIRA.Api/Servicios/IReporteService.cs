using System.Threading.Tasks;
using MIRA.Api.Modelos;

namespace MIRA.Api.Servicios;

public interface IReporteService
{
    Task<DashboardEjecutivoDto> ObtenerDashboardCompletoAsync();
    Task<object> EjecutarConsultaMultitablaAsync(int numero);
}
