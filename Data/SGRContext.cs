using Microsoft.EntityFrameworkCore;
using SGR_API.Models;

namespace SGR_API.Data
{
    public class SGRContext : DbContext
    {
        public SGRContext(DbContextOptions<SGRContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Autores> Autores { get; set; }
        public DbSet<Referencia> Referencias { get; set; }
        public DbSet<Contenido> Contenidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de la relación uno a muchos: Una Referencia tiene varios Contenidos
            modelBuilder.Entity<Referencia>()
                .HasMany(r => r.Contenidos)
                .WithOne(c => c.Referencia)
                .HasForeignKey(c => c.ReferenciaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Aquí puedes agregar configuraciones adicionales para otras relaciones,
            // por ejemplo, relacionar Referencia con Usuario o Autores según sea necesario.

            base.OnModelCreating(modelBuilder);
        }
    }
}
