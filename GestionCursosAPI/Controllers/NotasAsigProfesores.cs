using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace GestionCursosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotasAsigProfesores : Controller
    {
        private readonly IConfiguration _configuration;

        public NotasAsigProfesores(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet("{profesorId}")]
        public IActionResult GetCursosPorProfesor(string profesorId)
        {
            List<object> resultados = new List<object>();

            string connectionString = _configuration.GetConnectionString("Connection");
            string query = @"
                SELECT 
                    ca.Nombre AS Carrera, 
                    c.Nombre AS Asignaturas, 
                    cp.Semestre
                FROM Cursos c
                JOIN MateriasPorCarrera cp ON cp.Id = c.Id
                JOIN Carreras ca ON ca.Id = cp.CarreraId
                WHERE cp.ProfesorId = @ProfesorId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProfesorId", profesorId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        resultados.Add(new
                        {
                            Carrera = reader["Carrera"].ToString(),
                            Asignatura = reader["Asignaturas"].ToString(),
                            Semestre = reader["Semestre"].ToString()
                           
                        });
                    }
                }
            }

            return Ok(resultados);
        }


    }
}
