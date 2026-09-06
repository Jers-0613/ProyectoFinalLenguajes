using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles { get; set; }

    public DbSet<PreferenciaNotificacion> PreferenciasNotificacion { get; set; }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<BitacoraLogin> BitacoraLogin { get; set; }

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

    public async Task<bool> ProbarConexionAsync()
    {
        return await Database.CanConnectAsync();
    }
}