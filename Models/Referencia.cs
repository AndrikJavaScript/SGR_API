using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGR_API.Models
{
    public class Referencia
    {
        public Referencia()
        {
            Contenidos = new List<Contenido>();
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; }

        [Required]
        public int Anio { get; set; }

        [MaxLength(255)]
        public string? Lugar { get; set; }

        [MaxLength(255)]
        public string Fuente { get; set; }

        [Required]
        [MaxLength(50)]
        public string Formato { get; set; } // "APA" o "Chicago"

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int AutoresId { get; set; }

        // Propiedad de navegación
        public ICollection<Contenido> Contenidos { get; set; }
    }
}
