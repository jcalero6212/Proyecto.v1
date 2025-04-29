using GestionCursosAPI.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace GestionCursosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        [HttpPost]
        public IActionResult Login([FromBody] LoginRequestt request)
        {
            string connectionString = _configuration.GetConnectionString("Connection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, Username, Password, Rol, EstudianteId, ProfesorId FROM Usuarios WHERE Username = @username AND Password = @password";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", request.Username);
                cmd.Parameters.AddWithValue("@password", request.Password);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var usuario = new Usuario
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        Rol = reader["Rol"].ToString().ToLower(), // esto será sobreescrito abajo
                        EstudianteId = reader["EstudianteId"]?.ToString(),
                        ProfesorId = reader["ProfesorId"]?.ToString()
                    };

                    // ✅ Ajuste del rol basado en el ID real
                    if (!string.IsNullOrEmpty(usuario.ProfesorId))
                    {
                        usuario.Rol = "profesor";
                    }
                    else if (!string.IsNullOrEmpty(usuario.EstudianteId))
                    {
                        usuario.Rol = "estudiante";
                    }

                    return Ok(usuario);
                }

                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }
        }






    }
}
