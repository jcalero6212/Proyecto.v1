function verificarSesion() {
    const usuario = JSON.parse(localStorage.getItem("usuario"));

    if (!usuario) {
        window.location.href = "login.html";
        return;
    }

    document.getElementById("user-name").textContent = `👤 ${usuario.username}`;
    document.getElementById("user-role").textContent = `Rol: ${usuario.rol}`;

    // Validar si esta página es exclusiva para estudiantes
    if (usuario.rol !== "profesor") {
        alert("Acceso denegado. Solo profesores pueden acceder a esta página.");
        window.location.href = "login.html";
    }
}


verificarSesion(); // Ejecutamos la función al cargar


function logout() {
    localStorage.removeItem("usuario");
    window.location.href = "login.html";
}

document.getElementById("ver-cursos").addEventListener("click", function (e) {
    e.preventDefault(); // Evita redirección
    cargarCursosProfesores();

    document.getElementById("notas-container").style.display = "none";
    document.getElementById("carrera-selector").style.display = "none";
});




function cargarCursosProfesores() {

    const usuario = JSON.parse(localStorage.getItem("usuario"));
    const profesorId = usuario.profesorId;

    fetch(`/api/NotasAsigProfesores/${profesorId}`)
        .then(res => res.json())
        .then(data => {
            const contenedor = document.getElementById("cursos-container");
            const tbody = document.getElementById("cursos-body");

            contenedor.style.display = "block";
            tbody.innerHTML = "";

            if (data.length === 0) {
                tbody.innerHTML = `<tr><td colspan="3">No hay cursos asignados.</td></tr>`;
            } else {
                data.forEach(curso => {
                    const row = document.createElement("tr");
                    row.innerHTML = `
            <td>${curso.asignatura || 'N/A'}</td>
            <td>${curso.semestre || 'N/A'}</td>
            <td>${curso.carrera || 'N/A'}</td>
          `;
                    tbody.appendChild(row);
                });
            }
        })
        .catch(error => {
            console.error("Error al cargar cursos:", error);
            alert("No se pudieron cargar los cursos.");
        });
}


