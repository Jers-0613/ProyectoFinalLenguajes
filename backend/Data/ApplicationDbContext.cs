using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

// Contexto principal de Entity Framework Core. 
// Representa la conexión entre el backend y la base de datos SQL Server.
public class ApplicationDbContext : DbContext
{

    // Recibe las opciones de configuración de Entity Framework
    // incluyendo la cadena de conexión a SQL Server.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Cada DbSet representa una tabla
    public DbSet<Rol> Roles { get; set; }

    public DbSet<PreferenciaNotificacion> PreferenciasNotificacion { get; set; }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<BitacoraLogin> BitacoraLogin { get; set; }

    // Configura cómo las clases de C# se relacionan 
    // con las tablas y columnas existentes en SQL Server.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasMaxLength(255);

            entity.Property(e => e.Activo)
                .HasColumnName("activo")
                .IsRequired();
        });

        modelBuilder.Entity<PreferenciaNotificacion>(entity =>
        {
            entity.ToTable("PreferenciasNotificacion");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(30)
                .IsRequired();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Correo)
                .HasColumnName("correo")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Telefono)
                .HasColumnName("telefono")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.FechaNacimiento)
                .HasColumnName("fechaNacimiento")
                .IsRequired();

            entity.Property(e => e.Nickname)
                .HasColumnName("nickname")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("passwordHash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.FotoOriginal)
                .HasColumnName("fotoOriginal")
                .HasMaxLength(500);

            entity.Property(e => e.FotoModificada)
                .HasColumnName("fotoModificada")
                .HasMaxLength(500);

            entity.Property(e => e.PreferenciaNotificacionId)
                .HasColumnName("preferenciaNotificacionId")
                .IsRequired();

            entity.Property(e => e.RolId)
                .HasColumnName("rolId")
                .IsRequired();

            entity.Property(e => e.Activo)
                .HasColumnName("activo")
                .IsRequired();

            entity.Property(e => e.DebeCambiarPassword)
                .HasColumnName("debeCambiarPassword")
                .IsRequired();

            entity.Property(e => e.FechaRegistro)
                .HasColumnName("fechaRegistro")
                .IsRequired();

            entity.HasOne<PreferenciaNotificacion>()
                .WithMany()
                .HasForeignKey(e => e.PreferenciaNotificacionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Rol>()
                .WithMany()
                .HasForeignKey(e => e.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BitacoraLogin>(entity =>
        {
            entity.ToTable("BitacoraLogin");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UsuarioId)
                .HasColumnName("usuarioId")
                .IsRequired();

            entity.Property(e => e.FechaHora)
                .HasColumnName("fechaHora")
                .IsRequired();

            entity.Property(e => e.Ip)
                .HasColumnName("ip")
                .HasMaxLength(45)
                .IsRequired();

            entity.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // Comprueba si el backend puede establecer conexión
    public async Task<bool> ProbarConexionAsync()
    {
        return await Database.CanConnectAsync();
    }
}