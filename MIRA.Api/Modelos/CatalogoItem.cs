namespace MIRA.Api.Modelos;

public sealed class CatalogoItem
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public bool Activo { get; init; }
}
