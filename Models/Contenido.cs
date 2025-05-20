using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Agrega este using

namespace SGR_API.Models
{
    public class Contenido
    {
        public int Id { get; set; }

        [Required]
        public int ReferenciaId { get; set; }

        [Required]
        public int NumdePag { get; set; }

        [Required]
        public string Texto { get; set; }

        // Agrega [ValidateNever] para que este campo no se valide en la entrada
        [ValidateNever]
        public Referencia Referencia { get; set; }
    }
}
