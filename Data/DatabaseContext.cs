using Microsoft.EntityFrameworkCore;
using TPV_Gastronomico.Models;

namespace TPV_Gastronomico.Data
{
    public class DatabaseContext : DbContext
    {
        // Tablas
        public DbSet<User> Users { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketDetalle> TicketDetalles { get; set; }

        // Cadena de conexión
        private readonly string _connectionString;

        public DatabaseContext()
        {
            // SQLite en la misma carpeta del proyecto
            _connectionString = "Data Source=tpv_gastronomico.db";
        }

        // Configuración de EF Core
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }

        // Relaciones y constraints opcionales
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Usuario: username único
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Ticket -> TicketDetalle (1 a muchos)
            modelBuilder.Entity<Ticket>()
                .HasMany(t => t.Detalles)
                .WithOne(d => d.Ticket)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Producto -> TicketDetalle (1 a muchos)
            modelBuilder.Entity<Producto>()
                .HasMany<TicketDetalle>()
                .WithOne(d => d.Producto)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva -> Mesa
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Mesa)
                .WithMany()
                .HasForeignKey(r => r.MesaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva -> Usuario
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ NUEVA CONFIGURACIÓN: Convertir el enum TipoComida a string en la BD
            modelBuilder.Entity<Reserva>()
                .Property(r => r.Tipo)
                .HasConversion<string>();
        }
    }
}