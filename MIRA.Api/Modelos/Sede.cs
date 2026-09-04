namespace MIRA.Api.Modelos;

public sealed class Sede
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Ciudad { get; init; } = string.Empty;
    public bool Activa { get; init; }
}
