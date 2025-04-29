namespace GestionCursosAPI.Models
{
    public class NotaRequest
    {
        public string Estudiante_Id { get; set; }
        public int AsignaturaId { get; set; }
        public decimal Nota1 { get; set; }
        public decimal Nota2 { get; set; }
        public DateTime Fecha { get; set; }
        public string Comentarios { get; set; }
    }
}
