using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using GestionCursosAPI.Models;


namespace GestionCursosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CursosController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CursosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetCursos()
        {
            List<Curso> cursos = new List<Curso>();
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Nombre, Creditos FROM Cursos";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cursos.Add(new Curso
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nombre = reader["Nombre"].ToString(),
                        Creditos = Convert.ToInt32(reader["Creditos"])
                    });
                }
                conn.Close();
            }

            return Ok(cursos);
        }

        [HttpPost]
        public IActionResult InsertarCurso([FromBody] Curso nuevoCurso)
        {
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Cursos (Nombre, Creditos) VALUES (@Nombre, @Creditos)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", nuevoCurso.Nombre);
                cmd.Parameters.AddWithValue("@Creditos", nuevoCurso.Creditos);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return Ok(new { message = "Curso insertado correctamente." });
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarCurso(int id)
        {
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Cursos WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                int filasAfectadas = cmd.ExecuteNonQuery();
                conn.Close();

                if (filasAfectadas == 0)
                    return NotFound();

                return Ok(new { message = "Curso eliminado correctamente." });
            }
        }


        [HttpPut("{id}")]
        public IActionResult ActualizarCurso(int id, [FromBody] Curso cursoActualizado)
        {
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Cursos SET Nombre = @Nombre, Creditos = @Creditos WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Nombre", cursoActualizado.Nombre);
                cmd.Parameters.AddWithValue("@Creditos", cursoActualizado.Creditos);

                conn.Open();
                int filas = cmd.ExecuteNonQuery();
                conn.Close();

                if (filas == 0)
                    return NotFound();

                return Ok(new { message = "Curso actualizado correctamente." });
            }
        }

        [HttpGet("carrera/{idCarrera}/semestre/{semestre}")]
        public IActionResult ObtenerCursosPorCarreraYSemestre(string idCarrera, int semestre)
        {
            List<Curso> cursos = new List<Curso>();
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT c.Id, c.Nombre, c.Creditos 
                             FROM Cursos c
                             INNER JOIN MateriasPorCarrera mc ON c.Id = mc.MateriaId
                             WHERE mc.CarreraId = @CarreraId AND mc.Semestre = @Semestre";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CarreraId", idCarrera);
                cmd.Parameters.AddWithValue("@Semestre", semestre);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cursos.Add(new Curso
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nombre = reader["Nombre"].ToString(),
                        Creditos = Convert.ToInt32(reader["Creditos"])
                    });
                }
                conn.Close();
            }

            return Ok(cursos);
        }

    }
}
