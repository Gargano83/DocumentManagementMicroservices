var builder = WebApplication.CreateBuilder(args);

// Aggiunge i default di Aspire (Telemetria, HealthChecks, ecc.)
builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
