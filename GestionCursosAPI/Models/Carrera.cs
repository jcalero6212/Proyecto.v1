using System.ComponentModel.DataAnnotations;

namespace GestionCursosAPI.Models
{
    public class Carrera
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } // Ej: TI, SW, DS

        [Required]
        public string Nombre { get; set; }

        // Relación con Estudiantes
        public ICollection<Estudiante> Estudiantes { get; set; }
        
    }
}
