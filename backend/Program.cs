using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<PasswordService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NuxtPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//============= APIS=========================

var app = builder.Build();
app.UseCors("NuxtPolicy");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapPost("/api/usuarios", async (
    RegistroUsuarioDto datos,
    ApplicationDbContext db,
    PasswordService passwordService) =>
{


    
if (datos.Password != datos.ConfirmarPassword)
{
    return Results.BadRequest(new
    {
        mensaje = "Las contraseñas no coinciden."
    });
}

if (datos.Password.Length < 8)
{
    return Results.BadRequest(new
    {
        mensaje = "La contraseña debe tener al menos 8 caracteres."
    });
}

var contraseñaSegura = System.Text.RegularExpressions.Regex.IsMatch(
    datos.Password,
    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"
);

if (!contraseñaSegura)
{
    return Results.BadRequest(new
    {
        mensaje = "La contraseña debe tener al menos una mayúscula, una minúscula y un número."
    });
}

    bool correoExiste = await db.Usuarios
        .AnyAsync(u => u.Correo == datos.Correo);

    if (correoExiste)
    {
        return Results.BadRequest(new
        {
            mensaje = "El correo electrónico ya está registrado."
        });
    }

    bool nicknameExiste = await db.Usuarios
        .AnyAsync(u => u.Nickname == datos.Nickname);

    if (nicknameExiste)
    {
        return Results.BadRequest(new
        {
            mensaje = "El nickname ya está registrado."
        });
    }

    var preferenciaExiste = await db.PreferenciasNotificacion
        .AnyAsync(p => p.Id == datos.PreferenciaNotificacionId);

    if (!preferenciaExiste)
    {
        return Results.BadRequest(new
        {
            mensaje = "La preferencia de notificación no es válida."
        });
    }

    var rolAnalista = await db.Roles
        .FirstOrDefaultAsync(r => r.Nombre == "ANALISTA");

    if (rolAnalista == null)
    {
        return Results.Problem(
            "No se encontró el rol ANALISTA en la base de datos."
        );
    }

    var usuario = new Usuario
    {
        Correo = datos.Correo,
        Telefono = datos.Telefono,
        FechaNacimiento = datos.FechaNacimiento,
        Nickname = datos.Nickname,
        PasswordHash = passwordService.HashPassword(datos.Password),
        PreferenciaNotificacionId = datos.PreferenciaNotificacionId,
        RolId = rolAnalista.Id,
        Activo = true,
        FechaRegistro = DateTime.Now
    };

    db.Usuarios.Add(usuario);

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        mensaje = "Usuario registrado correctamente.",
        usuario = new
        {
            usuario.Id,
            usuario.Correo,
            usuario.Telefono,
            usuario.FechaNacimiento,
            usuario.Nickname,
            usuario.PreferenciaNotificacionId,
            usuario.RolId,
            usuario.Activo,
            usuario.FechaRegistro
        }
    });
});

app.MapPost("/api/login", async (
    LoginUsuarioDto datos,
    ApplicationDbContext db,
    PasswordService passwordService) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Correo == datos.Correo);

    if (usuario == null)
    {
        return Results.BadRequest(new
        {
            mensaje = "Correo o contraseña incorrectos."
        });
    }

    bool contraseñaCorrecta = passwordService.VerifyPassword(
        usuario.PasswordHash,
        datos.Password
    );

    if (!contraseñaCorrecta)
    {
        return Results.BadRequest(new
        {
            mensaje = "Correo o contraseña incorrectos."
        });
    }

    return Results.Ok(new
    {
        mensaje = "Inicio de sesión correcto.",
        usuario = new
        {
            usuario.Id,
            usuario.Correo,
            usuario.Nickname,
            usuario.RolId,
            usuario.Activo
        }
    });
});

app.MapGet("/api/test", () =>
{
    return "API funcionando correctamente";
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/api/db-test", async (ApplicationDbContext db) =>
{
    bool conectado = await db.ProbarConexionAsync();

    return conectado
        ? "Conexión con SQL Server funcionando correctamente"
        : "No se pudo conectar con SQL Server";
});

app.MapGet("/api/roles", async (ApplicationDbContext db) =>
{
    var roles = await db.Roles.ToListAsync();

    return roles;
});

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
