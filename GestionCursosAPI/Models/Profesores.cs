namespace GestionCursosAPI.Models
{
    public class Profesores
    {
        public string Id { get; set; } // 🧠 Importante que sea string si es como en Ecuador
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
    }
}
