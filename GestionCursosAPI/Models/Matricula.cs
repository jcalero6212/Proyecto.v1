namespace GestionCursosAPI.Models
{
    public class Matricula
    {
        public int Id { get; set; }
        public string EstudianteId { get; set; } // FK a Estudiantes (nvarchar(10))
        public int MatriculaCarreraId { get; set; } // FK a MatriculaCarrera (aka MateriasPorCarrera)
        public DateTime FechaMatricula { get; set; }
        public string Horario { get; set; }

        public Estudiante Estudiante { get; set; }
        public MatriculaCarrera MatriculaCarrera { get; set; }
    }
}
