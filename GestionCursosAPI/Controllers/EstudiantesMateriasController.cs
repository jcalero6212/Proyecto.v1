using System.Data;
using GestionCursosAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace GestionCursosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstudiantesMateriasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EstudiantesMateriasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("carrera")]
        public IActionResult ObtenerCarreras()
        {
            List<Carrera> carreras = new List<Carrera>();

            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Nombre FROM Carreras";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    carreras.Add(new Carrera
                    {
                        Id = reader["Id"].ToString(),
                        Nombre = reader["Nombre"].ToString()
                    });
                }

                conn.Close();
            }

            return Ok(carreras);
        }





        [HttpGet("carrera/{carreraId}/semestre/{semestre}/materia/{materiaId}")]
        public async Task<IActionResult> ObtenerEstudiantesMatriculados(string carreraId, int semestre, int materiaId)
        {
            List<Estudiante> estudiantes = new List<Estudiante>();

            string query = @"
                SELECT 
                    E.Id, 
                    E.Nombre, 
                    E.Apellido, 
                    E.FechaNacimiento, 
                    E.Email, 
                    E.Direccion, 
                    E.Telefono, 
                    E.CarreraId
                FROM 
                    Estudiantes E
                INNER JOIN Matriculas M ON M.EstudianteId = E.Id
                INNER JOIN MateriasPorCarrera MPC ON M.MateriaPorCarreraId = MPC.Id
                WHERE 
                    MPC.CarreraId = @CarreraId AND
                    MPC.Semestre = @Semestre AND
                    MPC.MateriaId = @MateriaId";

            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CarreraId", carreraId);
                cmd.Parameters.AddWithValue("@Semestre", semestre);
                cmd.Parameters.AddWithValue("@MateriaId", materiaId);

                conn.Open();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    estudiantes.Add(new Estudiante
                    {
                        Id = reader["Id"].ToString(),
                        Nombre = CapitalizeFirstLetter(reader["Nombre"].ToString()),
                        Apellido = CapitalizeFirstLetter(reader["Apellido"].ToString()),
                        FechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"]),
                        Email = reader["Email"].ToString(),
                        Direccion = CapitalizeFirstLetter(reader["Direccion"].ToString()),
                        Telefono = reader["Telefono"].ToString(),
                        CarreraId = reader["CarreraId"].ToString()
                    });
                }

                conn.Close();
            }

            if (estudiantes.Count == 0)
                return NotFound(new { message = "No se encontraron estudiantes para los parámetros especificados." });

            return Ok(estudiantes);
        }





        // Método para capitalizar la primera letra de un string
        private static string CapitalizeFirstLetter(string input)
        {
            return string.IsNullOrEmpty(input) ? input : char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }



        [HttpGet("profesores/carrera/{carreraId}/semestre/{semestre}/materia/{materiaId}")]
        public IActionResult ObtenerProfesoresPorCursoCarreraYSemestre(string carreraId, int semestre, int materiaId)
        {
            List<string> profesorIds = new List<string>();
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT DISTINCT ProfesorId 
            FROM MateriasPorCarrera 
            WHERE CarreraId = @CarreraId AND Semestre = @Semestre AND MateriaId = @MateriaId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CarreraId", carreraId);
                cmd.Parameters.AddWithValue("@Semestre", semestre);
                cmd.Parameters.AddWithValue("@MateriaId", materiaId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    profesorIds.Add(reader["ProfesorId"].ToString());
                }
                conn.Close();
            }

            return Ok(profesorIds); // Puedes devolver directamente los IDs o mapearlos a nombres si lo prefieres
        }




    }
}
