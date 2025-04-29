namespace GestionCursosAPI.Models
{
    public class MatriculaCarrera
    {
        public int Id { get; set; }  // Este no se usa al guardar, pero sí al editar/eliminar
        public int MateriaId { get; set; }
        public string CarreraId { get; set; }
        public int Semestre { get; set; }
        public string ProfesorId { get; set; }
    }
}
