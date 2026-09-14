using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services;
using backend.Messages; 

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


// CONFIGURACIÓN INICIAL

var builder = WebApplication.CreateBuilder(args);


// BASE DE DATOS

// Registra Entity Framework Core y configura la conexión
// con SQL Server utilizando la cadena definida en configuración.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// SERVICIOS


// Permite utilizarlos  mediante inyección de dependencias.
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<CredencialService>();
builder.Services.AddScoped<QrService>();
builder.Services.AddScoped<NotificacionService>();

// ==========================================================
// AUTENTICACIÓN JWT
// ==========================================================

// Configura JWT como mecanismo de autenticación de la API.
builder.Services.AddAuthentication(
    
    JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Comprueba que el token haya sido generado
        // por el emisor esperado.
        ValidateIssuer = true,

        // Comprueba que el token esté destinado
        // a esta aplicación.
        ValidateAudience = true,

        // Comprueba que el token no haya expirado.
        ValidateLifetime = true,

        // Comprueba que el token haya sido firmado
        // con nuestra clave.
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Clave"]!
            )
        )
    };
});


// Permite utilizar .RequireAuthorization()
// en los endpoints que necesiten autenticación.
builder.Services.AddAuthorization();


// ==========================================================
// CORS
// ==========================================================

// Permite que el frontend Nuxt, ejecutándose en localhost:3000,
// pueda realizar solicitudes al backend.
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


// ==========================================================
// SWAGGER / OPENAPI
// ==========================================================

// Permite explorar y probar los endpoints de la API
// durante el desarrollo.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ==========================================================
// CREACIÓN DE LA APLICACIÓN
// ==========================================================

var app = builder.Build();


// ==========================================================
// MIDDLEWARE
// ==========================================================

// Permite las solicitudes provenientes del frontend.
app.UseCors("NuxtPolicy");

// Comprueba los tokens JWT antes de permitir
// el acceso a endpoints protegidos.
app.UseAuthentication();

// Aplica las reglas de autorización.
app.UseAuthorization();


// Swagger solamente se habilita durante el desarrollo.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// HTTPS se habilitará posteriormente como parte
// de la configuración de seguridad y despliegue.
// app.UseHttpsRedirection();

// ==========================================================
// ENDPOINT: REGISTRO DE USUARIOS
// ==========================================================

app.MapPost("/api/usuarios", async (
    RegistroUsuarioDto datos,
    ApplicationDbContext db,
    PasswordService passwordService,
    CredencialService credencialService,
    NotificacionService notificacionService) =>
    {
    // Comprueba que ambas contraseñas coincidan.
    if (datos.Password != datos.ConfirmarPassword)
    {
        return Results.BadRequest(new
        {
            mensaje = "Las contraseñas no coinciden."
        });
    }


    if (!passwordService.EsPasswordValida(datos.Password))
    {
        return Results.BadRequest(new
        {
            mensaje =
                "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número."
        });
    }


    // Comprueba que el correo no esté registrado.
    bool correoExiste = await db.Usuarios
        .AnyAsync(u => u.Correo == datos.Correo);


    if (correoExiste)
    {
        return Results.BadRequest(new
        {
            mensaje = "El correo electrónico ya está registrado."
        });
    }


    // Comprueba que el nickname no esté registrado.
    bool nicknameExiste = await db.Usuarios
        .AnyAsync(u => u.Nickname == datos.Nickname);


    if (nicknameExiste)
    {
        return Results.BadRequest(new
        {
            mensaje = "El nickname ya está registrado."
        });
    }


    // Comprueba que la preferencia de notificación
    // seleccionada exista en la base de datos.
    var preferenciaExiste = await db.PreferenciasNotificacion
        .AnyAsync(p =>
            p.Id == datos.PreferenciaNotificacionId
        );


    if (!preferenciaExiste)
    {
        return Results.BadRequest(new
        {
            mensaje =
                "La preferencia de notificación no es válida."
        });
    }


    // Todos los usuarios nuevos reciben inicialmente
    // el rol ANALISTA.
    var rolAnalista = await db.Roles
        .FirstOrDefaultAsync(r =>
            r.Nombre == "ANALISTA"
        );


    if (rolAnalista == null)
    {
        return Results.Problem(
            "No se encontró el rol ANALISTA en la base de datos."
        );
    }


    // Crea la entidad Usuario utilizando los datos
    // previamente validados.
    var usuario = new Usuario
    {
        Correo = datos.Correo,
        Telefono = datos.Telefono,
        FechaNacimiento = datos.FechaNacimiento,
        Nickname = datos.Nickname,
        FotoOriginal = datos.FotoOriginal,
        FotoModificada = datos.FotoModificada,

        // La contraseña nunca se almacena directamente.
        PasswordHash =
            passwordService.HashPassword(datos.Password),

        PreferenciaNotificacionId =
            datos.PreferenciaNotificacionId,

        RolId = rolAnalista.Id,

        Activo = true,

        FechaRegistro = DateTime.Now,
        CodigoCredencial = Guid.NewGuid()
    };


    db.Usuarios.Add(usuario);

    await db.SaveChangesAsync();

    var datosBiometricos = new DatosBiometricos
    {
        UsuarioId = usuario.Id,
        RostroRecortado = datos.RostroRecortado
    };

    db.DatosBiometricos.Add(datosBiometricos);

    var datosCredencial = new CredencialDto
    {
        Id = usuario.Id,
        CodigoCredencial = usuario.CodigoCredencial!.Value,
        Nickname = usuario.Nickname,
        Correo = usuario.Correo,
        Telefono = usuario.Telefono,
        Rol = rolAnalista.Nombre,
        Foto = usuario.FotoModificada
    };

    var credencialPdf = credencialService.GenerarCredencial(datosCredencial);

    
    await notificacionService.EnviarNotificacionAsync(
        new NotificacionDto
        {
            UsuarioId = usuario.Id,
            Asunto = "Tu credencial - Analizador Léxico",
            Mensaje = MensajesCorreo.CredencialRegistro(usuario.Nickname),
            Archivo = credencialPdf,
            NombreArchivo =$"credencial-{usuario.Nickname}.pdf"
        }
    );


    await db.SaveChangesAsync();


    // Devuelve únicamente información necesaria del usuario.
    // El PasswordHash nunca se envía al frontend.
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

// ==========================================================
// ENDPOINT: LOGIN
// ==========================================================

app.MapPost("/api/login", async (
    LoginUsuarioDto datos,
    ApplicationDbContext db,
    PasswordService passwordService,
    HttpContext httpContext) =>
    {
    // Busca al usuario utilizando su correo.
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u =>
            u.Correo == datos.Correo);


    // No se revela si el correo existe o no.
    if (usuario == null)
    {
        return Results.BadRequest(new
        {
            mensaje = "Correo o contraseña incorrectos."
        });
    }


    // Comprueba la contraseña utilizando el hash almacenado.
    bool contraseñaCorrecta =
        passwordService.VerifyPassword(
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


    // Un usuario inactivo no puede iniciar sesión.
    if (!usuario.Activo)
    {
        return Results.BadRequest(new
        {
            mensaje = "El usuario está inactivo."
        });
    }


    // Obtiene la dirección IP desde la que se realizó el login.
    // Esta información se utiliza para la bitácora.
    var ip =
        httpContext.Connection.RemoteIpAddress?.ToString()
        ?? "IP desconocida";


    // Registra el inicio de sesión para mantener
    // la bitácora de accesos del sistema.
    var bitacora = new BitacoraLogin
    {
        UsuarioId = usuario.Id,
        FechaHora = DateTime.Now,
        Ip = ip
    };


    db.BitacoraLogin.Add(bitacora);

    await db.SaveChangesAsync();


    // Información que se incluirá dentro del JWT.
    var claims = new[]
    {
        new Claim(
            ClaimTypes.NameIdentifier,  
            usuario.Id.ToString()
        ),

        new Claim(
            ClaimTypes.Email,
            usuario.Correo
        ),

        new Claim(
            ClaimTypes.Name,
            usuario.Nickname
        ),

        new Claim(
            ClaimTypes.Role,
            usuario.RolId.ToString()
        )
    };


    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Clave"]!
        )
    );


    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256
    );


    // Genera un token válido durante dos horas.
    var token = new JwtSecurityToken(
        issuer: builder.Configuration["Jwt:Issuer"],
        audience: builder.Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: credentials
    );


    var tokenString =
        new JwtSecurityTokenHandler().WriteToken(token);


    // Devuelve el token y la información necesaria
    // para mantener la sesión en el frontend.
    return Results.Ok(new
    {
        mensaje = "Inicio de sesión correcto.",

        token = tokenString,

        usuario = new
        {
            id = usuario.Id,
            correo = usuario.Correo,
            telefono = usuario.Telefono,
            fechaNacimiento = usuario.FechaNacimiento,
            nickname = usuario.Nickname,
            preferenciaNotificacionId = usuario.PreferenciaNotificacionId,
            rolId = usuario.RolId,
            activo = usuario.Activo,
            fechaRegistro = usuario.FechaRegistro,
            debeCambiarPassword = usuario.DebeCambiarPassword
        }
    });
});


// ==========================================================
// ENDPOINT: LOGIN MEDIANTE QR
// ==========================================================

app.MapPost("/api/login/qr", async (
    LoginQrDto datos,
    ApplicationDbContext db,
    HttpContext httpContext) =>
    {
    // Busca al usuario utilizando el código único
    // almacenado dentro de su credencial QR.
    if (!Guid.TryParse(datos.CodigoCredencial, out var codigoCredencial))
    {
        return Results.BadRequest(new
        {
            mensaje = "El código QR no es válido."
        });
    }


    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u =>
            u.CodigoCredencial == codigoCredencial);


    // No se permite iniciar sesión si el código
    // no pertenece a ningún usuario.
    if (usuario == null)
    {
        return Results.BadRequest(new
        {
            mensaje = "La credencial no es válida."
        });
    }


    // Un usuario inactivo tampoco puede iniciar sesión.
    if (!usuario.Activo)
    {
        return Results.BadRequest(new
        {
            mensaje = "El usuario está inactivo."
        });
    }


    // Obtiene la dirección IP desde la que se realizó
    // el inicio de sesión para registrarla en la bitácora.
    var ip =
        httpContext.Connection.RemoteIpAddress?.ToString()
        ?? "IP desconocida";


    // Registra el inicio de sesión mediante QR.
    var bitacora = new BitacoraLogin
    {
        UsuarioId = usuario.Id,
        FechaHora = DateTime.Now,
        Ip = ip
    };


    db.BitacoraLogin.Add(bitacora);

    await db.SaveChangesAsync();


    // Utiliza los mismos datos del usuario que
    // utiliza el login tradicional para generar el JWT.
    var claims = new[]
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            usuario.Id.ToString()
        ),

        new Claim(
            ClaimTypes.Email,
            usuario.Correo
        ),

        new Claim(
            ClaimTypes.Name,
            usuario.Nickname
        ),

        new Claim(
            ClaimTypes.Role,
            usuario.RolId.ToString()
        )
    };


    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Clave"]!
        )
    );


    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256
    );


    // Genera el mismo tipo de JWT utilizado
    // por el login mediante correo y contraseña.
    var token = new JwtSecurityToken(
        issuer: builder.Configuration["Jwt:Issuer"],
        audience: builder.Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: credentials
    );


    var tokenString =
        new JwtSecurityTokenHandler().WriteToken(token);


    // Devuelve el token y la información necesaria
    // para establecer la sesión en el frontend.
    return Results.Ok(new
    {
        mensaje = "Inicio de sesión mediante QR correcto.",

        token = tokenString,

        usuario = new
        {
            id = usuario.Id,
            correo = usuario.Correo,
            telefono = usuario.Telefono,
            fechaNacimiento = usuario.FechaNacimiento,
            nickname = usuario.Nickname,
            preferenciaNotificacionId = usuario.PreferenciaNotificacionId,
            rolId = usuario.RolId,
            activo = usuario.Activo,
            fechaRegistro = usuario.FechaRegistro,
            debeCambiarPassword = usuario.DebeCambiarPassword
        }
    });
});

// ==========================================================
// ENDPOINT: Recuperar Imagenes
// ==========================================================

app.MapGet("/api/usuarios/{id}/fotos", async (
    int id,
    ApplicationDbContext db) =>
    {
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Id == id);

    if (usuario == null)
    {
        return Results.NotFound(new
        {
            mensaje = "Usuario no encontrado."
        });
    }

    var datosBiometricos = await db.DatosBiometricos
        .FirstOrDefaultAsync(d => d.UsuarioId == id);

    return Results.Ok(new
    {
        fotoOriginal = usuario.FotoOriginal,
        rostroRecortado = datosBiometricos?.RostroRecortado,
        fotoModificada = usuario.FotoModificada
    });
});

// ==========================================================
// ENDPOINT: Actualizar imagenes
// ==========================================================

app.MapPut("/api/usuarios/{id}/fotos", async (
    int id,
    ActualizarFotosDto datos,
    ApplicationDbContext db) =>
    {
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Id == id);

    if (usuario == null)
    {
        return Results.NotFound(new
        {
            mensaje = "Usuario no encontrado."
        });
    }

    usuario.FotoOriginal = datos.FotoOriginal;
    usuario.FotoModificada = datos.FotoModificada;

    var datosBiometricos = await db.DatosBiometricos
        .FirstOrDefaultAsync(d => d.UsuarioId == id);

    if (datosBiometricos == null)
    {
        datosBiometricos = new DatosBiometricos
        {
            UsuarioId = id,
            RostroRecortado = datos.RostroRecortado
        };

        db.DatosBiometricos.Add(datosBiometricos);
    }
    else
    {
        datosBiometricos.RostroRecortado = datos.RostroRecortado;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        mensaje = "Fotografías actualizadas correctamente."
    });
});

// ==========================================================
// ENDPOINT: Datos para la credencial
// ==========================================================

app.MapGet("/api/usuarios/{id}/credencial", async (
    int id,
    ApplicationDbContext db,
    CredencialService credencialService) =>
    {
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Id == id);

    if (usuario == null)
    {
        return Results.NotFound(new
        {
            mensaje = "Usuario no encontrado."
        });
    }

    var rol = await db.Roles
        .FirstOrDefaultAsync(r => r.Id == usuario.RolId);

    if (rol == null)
    {
        return Results.NotFound(new
        {
            mensaje = "Rol del usuario no encontrado."
        });
    }

    var datos = new CredencialDto
    {
        Id = usuario.Id,
        CodigoCredencial = usuario.CodigoCredencial!.Value,
        Nickname = usuario.Nickname,
        Correo = usuario.Correo,
        Telefono = usuario.Telefono,
        Rol = rol.Nombre,
        Foto = usuario.FotoModificada
    };

    var pdf = credencialService.GenerarCredencial(datos);

    return Results.File(
        pdf,
        "application/pdf",
        $"credencial-{usuario.Nickname}.pdf"
    );
});



// ==========================================================
// ENDPOINT: RECUPERAR CONTRASEÑA OLVIDADADA
// ==========================================================

app.MapPost("/api/recuperar-password", async (
    RecuperarPasswordDto dto,
    ApplicationDbContext db,
    PasswordService passwordService,
    NotificacionService notificacionService) =>
    {
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

    // No revelamos si el correo existe o no.
    if (usuario == null)
    {
        return Results.Ok(new
        {
            mensaje = "Si el correo está registrado, recibirás instrucciones para recuperar tu contraseña."
        });
    }

    var passwordTemporal = passwordService.GenerarPasswordTemporal();

    usuario.PasswordHash = passwordService.HashPassword(passwordTemporal);
    usuario.DebeCambiarPassword = true;

    await db.SaveChangesAsync();

    
    await notificacionService.EnviarNotificacionAsync(
        new NotificacionDto
        {
            UsuarioId = usuario.Id,
            Asunto = "Recuperación de contraseña - Analizador Léxico",
            Mensaje = MensajesCorreo.RecuperacionPassword( usuario.Nickname,passwordTemporal )
        }
    );


    return Results.Ok(new
    {
        mensaje = "Si el correo está registrado, recibirás instrucciones para recuperar tu contraseña."
    });
});


// ==========================================================
// ENDPOINT: CAMBIO DE CONTRASEÑA
// ==========================================================

app.MapPut("/api/usuario/password", async (
    CambioPasswordDto dto,
    HttpContext httpContext,
    ApplicationDbContext db,
    PasswordService passwordService) =>
    {
    var usuarioId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (usuarioId == null)
    {
        return Results.Unauthorized();
    }

    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Id == int.Parse(usuarioId));

    if (usuario == null)
    {
        return Results.NotFound(new 
        {
            mensaje = "Usuario no encontrado."
        });
    }

    if (dto.NuevaPassword != dto.ConfirmarNuevaPassword)
    {
        return Results.BadRequest(new
        {
            mensaje = "Las nuevas contraseñas no coinciden."
        });
    }

    if (!passwordService.VerifyPassword(usuario.PasswordHash, dto.PasswordActual))
    {
        return Results.BadRequest(new
        {
            mensaje = "La contraseña actual es incorrecta."
        });
    }

   if (!passwordService.EsPasswordValida(dto.NuevaPassword))
    {
        return Results.BadRequest(new
        {
            mensaje = "La nueva contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número."
        });
    }

    usuario.PasswordHash = passwordService.HashPassword(dto.NuevaPassword);
    usuario.DebeCambiarPassword = false;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        mensaje = "Contraseña actualizada correctamente."
    });
}).RequireAuthorization();


// ==========================================================
// ENDPOINT: USUARIO AUTENTICADO
// ==========================================================

// Devuelve la información del usuario correspondiente
// al JWT utilizado en la solicitud.
app.MapGet("/api/usuario", async (
    HttpContext httpContext,
    ApplicationDbContext db) =>
    {
    var usuarioId =
        httpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;


    if (usuarioId == null)
    {
        return Results.Unauthorized();
    }


    // Busca en la base de datos la información actual
    // del usuario autenticado.
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u =>
            u.Id == int.Parse(usuarioId));


    if (usuario == null)
    {
        return Results.NotFound(new
        {
            mensaje = "Usuario no encontrado."
        });
    }


    // No se devuelve información sensible como PasswordHash.
    return Results.Ok(new
    {
        id = usuario.Id,
        correo = usuario.Correo,
        telefono = usuario.Telefono,
        fechaNacimiento = usuario.FechaNacimiento,
        nickname = usuario.Nickname,
        preferenciaNotificacionId = usuario.PreferenciaNotificacionId,
        rolId = usuario.RolId,
        activo = usuario.Activo,
        fechaRegistro = usuario.FechaRegistro
    });

}).RequireAuthorization();

// ==========================================================
// ENDPOINT: ROLES
// ==========================================================

// Devuelve los roles registrados en la base de datos.
// Actualmente se utiliza para comprobar que la tabla
// Roles está correctamente conectada con el backend.
app.MapGet("/api/roles", async (
    ApplicationDbContext db) =>
    {
    var roles =
        await db.Roles.ToListAsync();

    return roles;
});


// ==========================================================
// INICIO DEL BACKEND
// ==========================================================

app.Run();

