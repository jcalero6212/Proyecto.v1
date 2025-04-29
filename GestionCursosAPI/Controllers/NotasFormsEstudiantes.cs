using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GestionCursosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class NotasFormsEstudiantes : Controller
    {

        private readonly string _connectionString;

        public NotasFormsEstudiantes(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Connection");
        }


        [HttpGet("{estudianteId}")]
        public IActionResult GetNotasPorEstudiante(string estudianteId)
        {
            

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        c.Nombre AS Asignatura,
                        mpc.Semestre,
                        ISNULL(n.Nota1, 0) AS Nota1,
                        ISNULL(n.Nota2, 0) AS Nota2,
                        ISNULL(n.Promedio, 0) AS Promedio,
                        ISNULL(n.Estado, 'Sin calificar') AS Estado
                    FROM Matriculas mat
                    JOIN MateriasPorCarrera mpc ON mat.MateriaPorCarreraId = mpc.Id
                    JOIN Cursos c ON mpc.MateriaId = c.Id
                    LEFT JOIN Notas n ON n.Estudiante_Id = mat.EstudianteId AND n.AsignaturaId = c.Id
                    WHERE mat.EstudianteId = @EstudianteId";


                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EstudianteId", estudianteId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                var notas = new List<object>();

                while (reader.Read())
                {
                    notas.Add(new
                    {
                        Asignatura = reader["Asignatura"].ToString(),
                        Semestre = reader["Semestre"].ToString(),
                        Nota1 = reader["Nota1"],
                        Nota2 = reader["Nota2"],
                        Promedio = reader["Promedio"],
                        Estado = reader["Estado"].ToString()
                    });
                }

                return Ok(notas);
            }
        }

    }
}
