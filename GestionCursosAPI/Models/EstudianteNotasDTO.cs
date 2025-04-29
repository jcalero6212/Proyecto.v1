namespace GestionCursosAPI.Models
{
    public class EstudianteNotasDTO
    {
        public Estudiante Estudiante { get; set; }
        public List<Nota> Notas { get; set; } = new List<Nota>();
    }
}
