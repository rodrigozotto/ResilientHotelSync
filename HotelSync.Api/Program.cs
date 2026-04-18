using Microsoft.EntityFrameworkCore;
using HotelSync.Api.Data;
using HotelSync.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DB com Resiliência (Padrão Sênior)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null))
);

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Skill de Sênior: Garante que o banco e tabelas existam no boot (útil para Dev/Docker)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
