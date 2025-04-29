using System.Data;
using GestionCursosAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace GestionCursosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MatriculaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public MatriculaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult MatricularEstudiante([FromBody] MatriculaDto dto)
        {
            string connStr = _configuration.GetConnectionString("Connection");

            // Validación de fecha
            if (dto.FechaMatricula < new DateTime(1753, 1, 1) || dto.FechaMatricula > new DateTime(9999, 12, 31))
            {
                return BadRequest(new { message = "La fecha de matrícula debe estar entre el 1 de enero de 1753 y el 31 de diciembre de 9999." });
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Validar si el MateriaPorCarreraId existe en la tabla MateriasPorCarrera
                    string validarMateriaQuery = @"
                        SELECT COUNT(*) 
                        FROM MateriasPorCarrera 
                        WHERE Id = @MateriaPorCarreraId";

                    SqlCommand validarMateriaCmd = new SqlCommand(validarMateriaQuery, conn);
                    validarMateriaCmd.Parameters.Add("@MateriaPorCarreraId", SqlDbType.Int).Value = dto.MatriculaCarreraId;

                    int existeMateria = (int)validarMateriaCmd.ExecuteScalar();

                    if (existeMateria == 0)
                    {
                        return BadRequest(new { message = "⚠️ El Id de Materia seleccionado no existe en el sistema." });
                    }

                    // Validar si ya está matriculado en la misma materia
                    string validarQuery = @"
                        SELECT COUNT(*) 
                        FROM Matriculas 
                        WHERE EstudianteId = @EstudianteId 
                        AND MateriaPorCarreraId = @MateriaPorCarreraId";

                    SqlCommand validarCmd = new SqlCommand(validarQuery, conn);
                    validarCmd.Parameters.Add("@EstudianteId", SqlDbType.Int).Value = dto.EstudianteId;
                    validarCmd.Parameters.Add("@MateriaPorCarreraId", SqlDbType.Int).Value = dto.MatriculaCarreraId;

                    int existe = (int)validarCmd.ExecuteScalar();

                    if (existe > 0)
                    {
                        return BadRequest(new { message = "⚠️ El estudiante ya está matriculado en esta materia." });
                    }
                    dto.Horario = string.IsNullOrWhiteSpace(dto.Horario) ? "No Asignado" : dto.Horario;

                    // Insertar la nueva matrícula
                    string insertQuery = @"
                        INSERT INTO Matriculas (EstudianteId, MateriaPorCarreraId, FechaMatricula, Horario)
                        VALUES (@EstudianteId, @MateriaPorCarreraId, @FechaMatricula, @Horario)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.Add("@EstudianteId", SqlDbType.VarChar, 50).Value = dto.EstudianteId;
                    insertCmd.Parameters.Add("@MateriaPorCarreraId", SqlDbType.Int).Value = dto.MatriculaCarreraId;
                    insertCmd.Parameters.Add("@FechaMatricula", SqlDbType.Date).Value = dto.FechaMatricula;
                    insertCmd.Parameters.Add("@Horario", SqlDbType.NVarChar, 50).Value = dto.Horario;

                    int rowsAffected = insertCmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "✅ Estudiante matriculado correctamente." });
                    }
                    else
                    {
                        return StatusCode(500, new { message = "⚠️ No se pudo matricular al estudiante. Intenta nuevamente." });
                    }
                }
                catch (Exception ex)
                {
                    // Detallar el error
                    return StatusCode(500, new { message = "⚠️ Error al guardar la matrícula", detalle = ex.Message });
                }
                finally
                {
                    conn.Close();
                }
            }
        }






        [HttpGet("carrera/{carreraId}/semestre/{semestre}")]
        public IActionResult GetMateriasPorCarreraSemestre(string carreraId, int semestre)
        {
            string connStr = _configuration.GetConnectionString("Connection");
            List<MatriculaCarrera> materias = new List<MatriculaCarrera>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT Id, MateriaId, CarreraId, Semestre, ProfesorId 
                             FROM MateriasPorCarrera 
                             WHERE CarreraId = @CarreraId AND Semestre = @Semestre";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CarreraId", carreraId);
                cmd.Parameters.AddWithValue("@Semestre", semestre);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    materias.Add(new MatriculaCarrera
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        MateriaId = Convert.ToInt32(reader["MateriaId"]),
                        CarreraId = reader["CarreraId"].ToString(),
                        Semestre = Convert.ToInt32(reader["Semestre"]),
                        ProfesorId = reader["ProfesorId"].ToString()
                    });
                }

                conn.Close();
            }

            return Ok(materias);
        }

        [HttpGet("estudiantes/carrera/{carreraId}")]
        public IActionResult GetEstudiantesPorCarrera(string carreraId)
        {
            List<Estudiante> estudiantes = new List<Estudiante>();
            string connStr = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Id, Nombre, Apellido, FechaNacimiento, Email, Direccion, Telefono, CarreraId FROM Estudiantes WHERE CarreraId = @CarreraId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CarreraId", carreraId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    estudiantes.Add(new Estudiante
                    {
                        Id = reader["Id"].ToString(),
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString(),
                        FechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"]),
                        Email = reader["Email"].ToString(),
                        Direccion = reader["Direccion"].ToString(),
                        Telefono = reader["Telefono"].ToString(),
                        CarreraId = reader["CarreraId"].ToString()
                    });
                }

                conn.Close();
            }

            return Ok(estudiantes);
        }

        [HttpGet("profesores/carrera/{carreraId}/semestre/{semestre}/materia/{materiaId}")]
        public IActionResult GetProfesoresPorCarreraSemestreMateria(string carreraId, int semestre, int materiaId)
        {
            string connStr = _configuration.GetConnectionString("Connection");
            List<object> profesoresConMaterias = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Consulta ajustada para traer el nombre y apellido del profesor directamente de la base de datos
                string query = @"
                    SELECT 
                        p.Id AS ProfesorId, 
                        p.Nombre AS ProfesorNombre, 
                        p.Apellido AS ProfesorApellido,
                        m.Id AS MateriaId, 
                        m.Nombre AS MateriaNombre
                        FROM MateriasPorCarrera mc
                        INNER JOIN Profesores p ON mc.ProfesorId = p.Id
                        INNER JOIN Cursos m ON mc.MateriaId = m.Id
                        WHERE mc.CarreraId = @CarreraId 
                        AND mc.Semestre = @Semestre 
                        AND mc.MateriaId = @MateriaId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@CarreraId", SqlDbType.NVarChar).Value = carreraId;
                cmd.Parameters.Add("@Semestre", SqlDbType.Int).Value = semestre;
                cmd.Parameters.Add("@MateriaId", SqlDbType.Int).Value = materiaId;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var profesor = new
                    {
                        ProfesorId = reader["ProfesorId"].ToString(),
                        ProfesorNombre = reader["ProfesorNombre"].ToString(),
                        ProfesorApellido = reader["ProfesorApellido"].ToString(),
                        MateriaId = reader["MateriaId"].ToString(),
                        MateriaNombre = reader["MateriaNombre"].ToString()
                    };

                    Console.WriteLine($"Profesor: {profesor.ProfesorNombre} {profesor.ProfesorApellido}"); // Aquí vemos si está devolviendo el nombre

                    profesoresConMaterias.Add(profesor);
                }


                conn.Close();
            }

            return Ok(profesoresConMaterias);
        }









        // En MatriculaController
        [HttpGet("carreraMatricula")]
        public IActionResult ObtenerCarrerasParaMatricula()
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

        [HttpGet("materiasporcarrera/buscar")]
        public IActionResult BuscarMateriaPorCarrera(
        [FromQuery] int materiaId,
        [FromQuery] string carreraId,
        [FromQuery] int semestre,
        [FromQuery] string profesorId)
        {
            string connStr = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT TOP 1 Id 
            FROM MateriasPorCarrera 
            WHERE MateriaId = @MateriaId 
              AND CarreraId = @CarreraId 
              AND Semestre = @Semestre 
              AND ProfesorId = @ProfesorId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MateriaId", materiaId);
                cmd.Parameters.AddWithValue("@CarreraId", carreraId);
                cmd.Parameters.AddWithValue("@Semestre", semestre);
                cmd.Parameters.AddWithValue("@ProfesorId", profesorId);

                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();

                if (result != null)
                {
                    return Ok(new { id = Convert.ToInt32(result) });
                }
                else
                {
                    return NotFound(new { message = "❌ No se encontró una asignación válida de materia." });
                }
            }
        }




        [HttpGet("carreraMat/{carreraId}")]
        public IActionResult GetEstudiantesPorCarreraMatricula(string carreraId)
        {
            List<Estudiante> estudiantes = new List<Estudiante>();
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Estudiantes WHERE CarreraId = @CarreraId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CarreraId", carreraId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    estudiantes.Add(new Estudiante
                    {
                        Id = reader["Id"].ToString(),
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString(),
                        FechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"]),
                        Email = reader["Email"].ToString(),
                        Direccion = reader["Direccion"].ToString(),
                        Telefono = reader["Telefono"].ToString(),
                        CarreraId = reader["CarreraId"].ToString()
                    });
                }

                conn.Close();
            }

            return Ok(estudiantes);
        }
    }
}
