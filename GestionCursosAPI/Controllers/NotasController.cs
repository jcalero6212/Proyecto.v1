using System.Data;
using GestionCursosAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
namespace GestionCursosAPI.Controllers

{
    [ApiController]
    [Route("api/[controller]")]

    public class NotasController : ControllerBase
    {
        private readonly string _connectionString;

        public NotasController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Connection");
        }

        //// GET: api/Notas/estudiante/{estudianteId}
        //[HttpGet("estudiante/{estudianteId}")]
        //public async Task<IActionResult> GetNotasByEstudiante(string estudianteId)
        //{
        //    List<Nota> notas = new List<Nota>();

        //    using (SqlConnection conn = new SqlConnection(_connectionString))
        //    {
        //        await conn.OpenAsync();
        //        SqlCommand cmd = new SqlCommand("SELECT * FROM Notas WHERE Estudiante_Id = @Estudiante_Id", conn);
        //        cmd.Parameters.AddWithValue("@Estudiante_Id", estudianteId);

        //        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                var nota = new Nota
        //                {
        //                    Id = (int)reader["Id"],
        //                    Estudiante_Id = reader["Estudiante_Id"].ToString(),
        //                    AsignaturaId = (int)reader["AsignaturaId"],
        //                    Nota1 = (decimal)reader["Nota1"],
        //                    Nota2 = (decimal)reader["Nota2"],
        //                    Promedio = (decimal)reader["Promedio"],
        //                    Estado = reader["Estado"].ToString(),
        //                    Comentarios = reader["Comentarios"].ToString()
        //                };
        //                notas.Add(nota);
        //            }
        //        }
        //    }

        //    if (notas.Count == 0)
        //    {
        //        return NotFound("No se encontraron notas para este estudiante.");
        //    }

        //    return Ok(notas);
        //}



        [HttpGet("estudiante/detalles/{estudianteId}/materia/{asignaturaId}")]
        public async Task<IActionResult> GetNotasYDetallesByEstudianteYMateria(string estudianteId, int asignaturaId)
        {
            var result = new EstudianteNotasDTO();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Obtener datos del estudiante
                SqlCommand cmdEst = new SqlCommand("SELECT * FROM Estudiantes WHERE Id = @Id", conn);
                cmdEst.Parameters.AddWithValue("@Id", estudianteId);

                using (SqlDataReader reader = await cmdEst.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        result.Estudiante = new Estudiante
                        {
                            Id = reader["Id"].ToString(),
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            FechaNacimiento = (DateTime)reader["FechaNacimiento"],
                            Email = reader["Email"].ToString(),
                            Direccion = reader["Direccion"].ToString(),
                            Telefono = reader["Telefono"].ToString(),
                            CarreraId = reader["CarreraId"].ToString()
                        };
                    }
                    else
                    {
                        return NotFound("Estudiante no encontrado.");
                    }
                }

                // Obtener las notas del estudiante para la materia seleccionada
                SqlCommand cmdNotas = new SqlCommand(@"
        SELECT n.Id, n.Estudiante_Id, n.AsignaturaId, n.Nota1, n.Nota2, n.Promedio, n.Estado, n.Comentarios, c.Nombre AS CursoNombre
        FROM Notas n
        JOIN Cursos c ON n.AsignaturaId = c.Id
        WHERE Estudiante_Id = @EstudianteId AND n.AsignaturaId = @AsignaturaId", conn);

                cmdNotas.Parameters.AddWithValue("@EstudianteId", estudianteId);
                cmdNotas.Parameters.AddWithValue("@AsignaturaId", asignaturaId);

                using (SqlDataReader readerNotas = await cmdNotas.ExecuteReaderAsync())
                {
                    while (await readerNotas.ReadAsync())
                    {
                        var nota = new Nota
                        {
                            Id = (int)readerNotas["Id"],
                            Estudiante_Id = readerNotas["Estudiante_Id"].ToString(),
                            AsignaturaId = (int)readerNotas["AsignaturaId"],
                            Nota1 = (decimal)readerNotas["Nota1"],
                            Nota2 = (decimal)readerNotas["Nota2"],
                            Promedio = (decimal)readerNotas["Promedio"],
                            Estado = readerNotas["Estado"].ToString(),
                            Comentarios = readerNotas["Comentarios"].ToString(),
                            Curso = new Curso
                            {
                                Id = (int)readerNotas["AsignaturaId"],
                                Nombre = readerNotas["CursoNombre"].ToString()
                            }
                        };
                        result.Notas.Add(nota);
                    }
                }
            }

            return Ok(result);
        }








        //// GET: api/Notas/{id}
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetNota(int id)
        //{
        //    Nota nota = null;

        //    using (SqlConnection conn = new SqlConnection(_connectionString))
        //    {
        //        await conn.OpenAsync();
        //        SqlCommand cmd = new SqlCommand("SELECT * FROM Notas WHERE Id = @Id", conn);
        //        cmd.Parameters.AddWithValue("@Id", id);

        //        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //        {
        //            if (await reader.ReadAsync())
        //            {
        //                nota = new Nota
        //                {
        //                    Id = (int)reader["Id"],
        //                    Estudiante_Id = reader["Estudiante_Id"].ToString(),
        //                    AsignaturaId = (int)reader["AsignaturaId"],
        //                    Nota1 = (decimal)reader["Nota1"],
        //                    Nota2 = (decimal)reader["Nota2"],
        //                    Promedio = (decimal)reader["Promedio"],
        //                    Estado = reader["Estado"].ToString(),
        //                    Comentarios = reader["Comentarios"].ToString()
        //                };
        //            }
        //        }
        //    }

        //    if (nota == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(nota);
        //}


        [HttpPost]
        public async Task<IActionResult> CrearNota([FromBody] Nota nota)
        {
            if (nota == null)
                return BadRequest("Los datos de la nota son inválidos.");

            if (nota.Nota1 < 0 || nota.Nota2 < 0)
                return BadRequest("Las notas deben ser mayores o iguales a 0.");

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string insertQuery = @"INSERT INTO Notas (Estudiante_Id, AsignaturaId, Nota1, Nota2, Comentarios)
                                   VALUES (@Estudiante_Id, @AsignaturaId, @Nota1, @Nota2, @Comentarios);";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        // Estudiante_Id es varchar(10)
                        cmd.Parameters.Add("@Estudiante_Id", SqlDbType.VarChar, 10).Value = nota.Estudiante_Id;

                        // AsignaturaId como entero
                        cmd.Parameters.Add("@AsignaturaId", SqlDbType.Int).Value = nota.AsignaturaId;

                        // Nota1 y Nota2 como DECIMAL(5,2)
                        cmd.Parameters.Add("@Nota1", SqlDbType.Decimal).Value = nota.Nota1;
                        cmd.Parameters.Add("@Nota2", SqlDbType.Decimal).Value = nota.Nota2;

                        // Comentarios como NVarChar
                        cmd.Parameters.Add("@Comentarios", SqlDbType.NVarChar).Value =
                            string.IsNullOrEmpty(nota.Comentarios) ? (object)DBNull.Value : nota.Comentarios;

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            // Al ser un cálculo automático, el promedio y estado se actualizan en la BD
                            return Ok("Nota creada correctamente.");
                        }
                        else
                        {
                            return BadRequest("Error al guardar la nota. No se afectaron filas.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return BadRequest($"Error SQL al guardar la nota: {ex.Message}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error inesperado: {ex.Message}");
            }
        }




        [HttpGet("estudiante/{estudianteId}/materia/{asignaturaId}")]
        public async Task<IActionResult> ObtenerNotas(int estudianteId, int asignaturaId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = "SELECT * FROM Notas WHERE Estudiante_Id = @EstudianteId AND AsignaturaId = @AsignaturaId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EstudianteId", estudianteId);
                cmd.Parameters.AddWithValue("@AsignaturaId", asignaturaId);

                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                List<Nota> notas = new List<Nota>();

                while (await reader.ReadAsync())
                {
                    notas.Add(new Nota
                    {
                        Id = reader.GetInt32(0),
                        Estudiante_Id = reader.GetString(10),
                        AsignaturaId = reader.GetInt32(2),
                        Nota1 = reader.GetDecimal(3),
                        Nota2 = reader.GetDecimal(4),
                        Comentarios = reader.GetString(5),
                    });
                }

                return Ok(notas);
            }
        }



        // PUT: api/Notas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarNota(int id, [FromBody] Nota nota)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE Notas SET Nota1 = @Nota1, Nota2 = @Nota2, Fecha = @Fecha, Comentarios = @Comentarios " +
                               "WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Nota1", nota.Nota1);
                cmd.Parameters.AddWithValue("@Nota2", nota.Nota2);
                cmd.Parameters.AddWithValue("@Comentarios", nota.Comentarios);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
        }

        // DELETE: api/Notas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarNota(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand("DELETE FROM Notas WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
        }
    }
}
