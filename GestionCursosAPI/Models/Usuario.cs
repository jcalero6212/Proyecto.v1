namespace GestionCursosAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } 
        public string Rol { get; set; }
        public  string EstudianteId { get; set; }
        public  string ProfesorId { get; set; }
    }
}
