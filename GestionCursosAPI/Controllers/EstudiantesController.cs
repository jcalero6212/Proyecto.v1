using GestionCursosAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GestionCursosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstudiantesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EstudiantesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetEstudiantes()
        {
            List<Estudiante> estudiantes = new List<Estudiante>();
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Nombre, Apellido, FechaNacimiento, Email, Direccion, Telefono,CarreraId FROM Estudiantes";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
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

            return Ok(estudiantes);
        }


        [HttpPost]
        public IActionResult InsertarEstudiante([FromBody] Estudiante nuevoEstudiante)
        {
            if (string.IsNullOrEmpty(nuevoEstudiante.Id) || nuevoEstudiante.Id.Length != 10)
            {
                return BadRequest("La cédula es obligatoria y debe tener 10 dígitos.");
            }

            if (!EsCedulaValida(nuevoEstudiante.Id))
            {
                return BadRequest("La cédula ingresada no es válida.");
            }

            nuevoEstudiante.Nombre = CapitalizeFirstLetter(nuevoEstudiante.Nombre);
            nuevoEstudiante.Apellido = CapitalizeFirstLetter(nuevoEstudiante.Apellido);
            nuevoEstudiante.Direccion = CapitalizeFirstLetter(nuevoEstudiante.Direccion);

            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Estudiantes (Id, Nombre, Apellido, FechaNacimiento, Email, Direccion, Telefono, CarreraId)
                         VALUES (@Id, @Nombre, @Apellido, @FechaNacimiento, @Email, @Direccion, @Telefono, @CarreraId)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", nuevoEstudiante.Id);
                cmd.Parameters.AddWithValue("@Nombre", nuevoEstudiante.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", nuevoEstudiante.Apellido);
                cmd.Parameters.AddWithValue("@FechaNacimiento", nuevoEstudiante.FechaNacimiento);
                cmd.Parameters.AddWithValue("@Email", nuevoEstudiante.Email);
                cmd.Parameters.AddWithValue("@Direccion", nuevoEstudiante.Direccion);
                cmd.Parameters.AddWithValue("@Telefono", nuevoEstudiante.Telefono);
                cmd.Parameters.AddWithValue("@CarreraId", (object)nuevoEstudiante.CarreraId ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return Ok(new { message = "Estudiante insertado correctamente ✅" });
        }


        private static bool EsCedulaValida(string cedula)
        {
            if (cedula.Length != 10 || !cedula.All(char.IsDigit))
                return false;

            int digitoRegion = int.Parse(cedula.Substring(0, 2));
            if (digitoRegion < 1 || digitoRegion > 24)
                return false;

            int ultimoDigito = int.Parse(cedula[9].ToString());

            int sumaPares = int.Parse(cedula[1].ToString()) +
                            int.Parse(cedula[3].ToString()) +
                            int.Parse(cedula[5].ToString()) +
                            int.Parse(cedula[7].ToString());

            int sumaImpares = 0;
            for (int i = 0; i < 9; i += 2)
            {
                int num = int.Parse(cedula[i].ToString()) * 2;
                if (num > 9) num -= 9;
                sumaImpares += num;
            }

            int sumaTotal = sumaPares + sumaImpares;
            int digitoValidador = 10 - (sumaTotal % 10);
            if (digitoValidador == 10) digitoValidador = 0;

            return digitoValidador == ultimoDigito;
        }


        [HttpPut("{id}")]
        public IActionResult ActualizarEstudiante(string id, [FromBody] Estudiante estudiante)
        {
            if (estudiante == null || estudiante.Id != id)
                return BadRequest(new { message = "❌ Datos inválidos o el ID no coincide." });

            // Capitalización de campos
            estudiante.Nombre = CapitalizeFirstLetter(estudiante.Nombre);
            estudiante.Apellido = CapitalizeFirstLetter(estudiante.Apellido);
            estudiante.Direccion = CapitalizeFirstLetter(estudiante.Direccion);

            string connectionString = _configuration.GetConnectionString("Connection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE Estudiantes 
                             SET Nombre = @Nombre, 
                                 Apellido = @Apellido, 
                                 FechaNacimiento = @FechaNacimiento, 
                                 Email = @Email,  
                                 Direccion = @Direccion, 
                                 Telefono = @Telefono,
                                 CarreraId = @CarreraId
                             WHERE Id = @Id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", id);  // El ID es varchar
                    cmd.Parameters.AddWithValue("@Nombre", estudiante.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", estudiante.Apellido);
                    cmd.Parameters.AddWithValue("@FechaNacimiento", estudiante.FechaNacimiento);
                    cmd.Parameters.AddWithValue("@Email", estudiante.Email);
                    cmd.Parameters.AddWithValue("@Direccion", estudiante.Direccion);
                    cmd.Parameters.AddWithValue("@Telefono", estudiante.Telefono);
                    cmd.Parameters.AddWithValue("@CarreraId", estudiante.CarreraId);  // El ID de la carrera, no el nombre

                    conn.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (filasAfectadas == 0)
                        return NotFound(new { message = "Estudiante no encontrado." });

                    return Ok(new { message = "✅ Estudiante actualizado correctamente." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error interno al actualizar el estudiante", error = ex.Message });
            }
        }



        [HttpDelete("id/{id}")]
        public IActionResult EliminarPorId(string id)  // Cambiar a string
        {
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Estudiantes WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);  // Asegúrate de que se pase como string

                conn.Open();
                int filas = cmd.ExecuteNonQuery();
                conn.Close();

                if (filas == 0)
                    return NotFound();  // No se encontró el estudiante

                return Ok(new { message = "Estudiante eliminado correctamente." });
            }
        }


        [HttpGet("carrera/{carreraId}")]
        public IActionResult GetEstudiantesPorCarrera(string carreraId)
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



        private static string CapitalizeFirstLetter(string input)
        {
            return string.IsNullOrEmpty(input) ? input : char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}
