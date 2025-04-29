using GestionCursosAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GestionCursosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatriculaCarreraController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MatriculaCarreraController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult GetAsignaciones()
        {
            List<MatriculaCarrera> asignaciones = new List<MatriculaCarrera>();
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, CarreraId, MateriaId, ProfesorId, Semestre FROM MateriasPorCarrera";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    asignaciones.Add(new MatriculaCarrera
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CarreraId = reader["CarreraId"].ToString(),
                        MateriaId = Convert.ToInt32(reader["MateriaId"]), // ✅ cambio aquí
                        ProfesorId = reader["ProfesorId"].ToString(),
                        Semestre = Convert.ToInt32(reader["Semestre"])
                    });
                }

                conn.Close();
            }

            return Ok(asignaciones);
        }

        [HttpPost]
        public IActionResult CrearMatriculaCarrera([FromBody] MatriculaCarrera nueva)
        {
            // Validación básica de entrada
            if (nueva == null || nueva.MateriaId <= 0 || string.IsNullOrEmpty(nueva.CarreraId) || string.IsNullOrEmpty(nueva.ProfesorId) || nueva.Semestre <= 0)
            {
                return BadRequest(new { message = "❌ Todos los campos son obligatorios y deben tener valores válidos." });
            }

            string connectionString = _configuration.GetConnectionString("Connection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                INSERT INTO MateriasPorCarrera (MateriaId, CarreraId, Semestre, ProfesorId)
                VALUES (@MateriaId, @CarreraId, @Semestre, @ProfesorId)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MateriaId", nueva.MateriaId);
                    cmd.Parameters.AddWithValue("@CarreraId", nueva.CarreraId);
                    cmd.Parameters.AddWithValue("@Semestre", nueva.Semestre);
                    cmd.Parameters.AddWithValue("@ProfesorId", nueva.ProfesorId);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (rows > 0)
                    {
                        return Created("", new { message = "✅ Asignación guardada correctamente." });
                    }
                    else
                    {
                        return StatusCode(500, new { message = "❌ No se pudo guardar la asignación." });
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, new { message = "❌ Error de base de datos.", error = sqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error interno del servidor.", error = ex.Message });
            }
        }


        // 🔽 DELETE: api/MatriculaCarrera/{id}
        [HttpDelete("{id}")]
        public IActionResult EliminarAsignacion(int id)
        {
            string connectionString = _configuration.GetConnectionString("Connection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM MateriasPorCarrera WHERE Id = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (filasAfectadas == 0)
                        return NotFound(new { message = "❌ Asignación no encontrada." });

                    return Ok(new { message = "✅ Asignación eliminada correctamente." });
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { message = "❌ Error de base de datos", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error interno", error = ex.Message });
            }
        }


    }
}
