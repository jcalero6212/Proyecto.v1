namespace GestionCursosAPI.Models
{
    public class Nota
    {
        public int Id { get; set; }
        public string Estudiante_Id { get; set; }  
        public int AsignaturaId { get; set; }      
        public decimal Nota1 { get; set; }         
        public decimal Nota2 { get; set; }         
        public decimal Promedio { get; set; }      
        public string Estado { get; set; }         
        public string Comentarios { get; set; }

        public Curso Curso { get; set; }
    }
}
