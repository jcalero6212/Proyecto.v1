namespace GestionCursosAPI.Models
{
    public class MatriculaDto
    {
        public int Id { get; set; }
        public string EstudianteId { get; set; } // nvarchar(10)
        public int MatriculaCarreraId { get; set; } // Esto es el ID de la tabla MateriasPorCarrera
        public DateTime FechaMatricula { get; set; }
        public string Horario { get; set; }
    }
}
