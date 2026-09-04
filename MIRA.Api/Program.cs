using MIRA.Api.Configuracion;
using MIRA.Api.Excepciones;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMiraConfiguracion(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors(MiraServiceCollectionExtensions.CorsPolicyName);

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "mira-api" }));

app.Run();

public partial class Program
{
}
