using GestionCursosAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;


namespace GestionCursosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarrerasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CarrerasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetCarreras()
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

        // Definimos un endpoint para obtener las carreras
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



        [HttpGet("carrera/{idCarrera}")]
        public IActionResult ObtenerEstudiantesPorCarrera(string idCarrera)
        {
            List<Estudiante> estudiantes = new List<Estudiante>();
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Estudiantes WHERE CarreraId = @CarreraId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CarreraId", idCarrera);
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
