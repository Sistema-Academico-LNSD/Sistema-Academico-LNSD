$(document).ready(function () {
    cargarEstructura();
    cargarGradosActivos();

    $("#btnBuscar").click(function () { cargarEstructura(); });
    $("#btnLimpiar").click(function () {
        $("#txtBuscar").val("");
        $("#filtroNivel").val("");
        cargarEstructura();
    });

    $("#btnGuardarGrado").click(function () { crearGrado(); });
    $("#btnActualizarGrado").click(function () { actualizarGrado(); });
    $("#btnGuardarSeccion").click(function () { crearSeccion(); });
    $("#btnActualizarSeccion").click(function () { actualizarSeccion(); });

    $("#modalCrearSeccion").on("show.bs.modal", function () {
        cargarGradosActivos();
    });
});

function cargarEstructura() {
    $.ajax({
        url: "/Grado/GetEstructura",
        type: "GET",
        data: {
            texto: $("#txtBuscar").val(),
            nivel: $("#filtroNivel").val()
        },
        success: function (response) {
            let filas = "";

            if (!response.dato || response.dato.length === 0) {
                filas = `<tr><td colspan="8" class="text-center text-muted">No existen registros disponibles.</td></tr>`;
            } else {
                $.each(response.dato, function (_, item) {
                    const seccion = item.nombreSeccion ? escapeHtml(item.nombreSeccion) : "—";
                    const capacidad = item.capacidadMaxima ?? "—";
                    const cupos = item.cuposDisponibles ?? "—";
                    const estado = item.estado ? "Activo" : "Inactivo";
                    const nuevoEstado = !item.estado;

                    let acciones = `
                        <a class="btn btn-info btn-sm me-1" href="/Grado/Detalle/${item.idGrado}" title="Ver detalle">
                            <i class="bi bi-eye"></i>
                        </a>
                        <button class="btn btn-warning btn-sm me-1" onclick="obtenerGrado(${item.idGrado})">
                            <i class="bi bi-pencil-square"></i>
                        </button>
                        <button class="btn btn-secondary btn-sm me-1" onclick="cambiarEstadoGrado(${item.idGrado}, ${nuevoEstado})">
                            <i class="bi bi-toggle-${item.estadoGrado ? "on" : "off"}"></i>
                        </button>`;

                    if (item.idSeccion) {
                        acciones += `
                        <button class="btn btn-warning btn-sm me-1" onclick="obtenerSeccion(${item.idSeccion})">
                            Sección
                        </button>
                        <button class="btn btn-secondary btn-sm" onclick="cambiarEstadoSeccion(${item.idSeccion}, ${!item.estado})">
                            ${item.estado ? "Desactivar" : "Activar"}
                        </button>`;
                    }

                    filas += `<tr>
                        <td>${escapeHtml(item.codigoGrado)}</td>
                        <td>${escapeHtml(item.nombreGrado)}</td>
                        <td>${escapeHtml(item.nivel)}</td>
                        <td>${seccion}</td>
                        <td>${capacidad}</td>
                        <td>${cupos}</td>
                        <td>${estado}</td>
                        <td>${acciones}</td>
                    </tr>`;
                });
            }

            $("#tblEstructura tbody").html(filas);
        },
        error: function () {
            mostrarMensaje("Error al cargar la estructura académica.", "danger");
        }
    });
}

function cargarGradosActivos() {
    $.get("/Grado/GetGradosActivos", function (response) {
        let opciones = `<option value="">Seleccione un grado</option>`;

        if (response.dato) {
            $.each(response.dato, function (_, g) {
                opciones += `<option value="${g.idGrado}">${escapeHtml(g.nombre)} (${escapeHtml(g.codigo)})</option>`;
            });
        }

        $("#IdGrado, #editSeccionIdGrado").html(opciones);
    });
}

function crearGrado() {
    $.post("/Grado/CreateGrado", {
        codigo: $("#Codigo").val(),
        nombre: $("#Nombre").val(),
        nivel: $("#Nivel").val()
    }, function (response) {
        if (response.esCorrecto) {
            bootstrap.Modal.getInstance(document.getElementById("modalCrearGrado")).hide();
            $("#formCrearGrado")[0].reset();
            cargarEstructura();
            cargarGradosActivos();
            mostrarMensaje(response.mensaje, "success");
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    }).fail(function () {
        mostrarMensaje("Error al registrar el grado.", "danger");
    });
}

function obtenerGrado(id) {
    $.get("/Grado/GetGradoById", { id: id }, function (response) {
        if (response.esCorrecto && response.dato) {
            $("#editGradoId").val(response.dato.idGrado);
            $("#editCodigo").val(response.dato.codigo);
            $("#editNombreGrado").val(response.dato.nombre);
            $("#editNivel").val(response.dato.nivel);

            new bootstrap.Modal(
                document.getElementById("modalEditarGrado")
            ).show();
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    });
}

function actualizarGrado() {
    $.post("/Grado/UpdateGrado", {
        idGrado: $("#editGradoId").val(),
        codigo: $("#editCodigo").val(),
        nombre: $("#editNombreGrado").val(),
        nivel: $("#editNivel").val()
    }, function (response) {
        if (response.esCorrecto) {
            bootstrap.Modal.getInstance(
                document.getElementById("modalEditarGrado")
            ).hide();

            cargarEstructura();
            mostrarMensaje(response.mensaje, "success");
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    });
}

async function cambiarEstadoGrado(id, estado) {
    const ok = await confirmar(
        estado ? "¿Activar este grado?" : "¿Desactivar este grado?",
        estado ? "Activar" : "Desactivar",
        "warning"
    );
    if (!ok) return;

    $.post("/Grado/CambiarEstadoGrado", { id: id, estado: estado }, function (response) {
        if (response.esCorrecto) {
            cargarEstructura();
            mostrarMensaje(response.mensaje, "success");
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    });
}

function crearSeccion() {
    $.post("/Grado/CreateSeccion", {
        idGrado: $("#IdGrado").val(),
        nombre: $("#NombreSeccion").val(),
        capacidadMaxima: $("#CapacidadMaxima").val()
    }, function (response) {
        if (response.esCorrecto) {
            bootstrap.Modal.getInstance(document.getElementById("modalCrearSeccion")).hide();
            $("#formCrearSeccion")[0].reset();
            cargarEstructura();
            mostrarMensaje(response.mensaje, "success");
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    });
}

function obtenerSeccion(id) {
    $.get("/Grado/GetSeccionById", { id: id }, function (response) {
        if (response.esCorrecto && response.dato) {
            $("#editIdSeccion").val(response.dato.idSeccion);
            $("#editSeccionIdGrado").val(response.dato.idGrado);
            $("#editNombreSeccion").val(response.dato.nombre);
            $("#editCapacidadMaxima").val(response.dato.capacidadMaxima);

            new bootstrap.Modal(
                document.getElementById("modalEditarSeccion")
            ).show();
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    });
}

function actualizarSeccion() {
    $.post("/Grado/UpdateSeccion", {
        idSeccion: $("#editIdSeccion").val(),
        idGrado: $("#editSeccionIdGrado").val(),
        nombre: $("#editNombreSeccion").val(),
        capacidadMaxima: $("#editCapacidadMaxima").val()
    }, function (response) {
        if (response.esCorrecto) {
            bootstrap.Modal.getInstance(
                document.getElementById("modalEditarSeccion")
            ).hide();

            cargarEstructura();
            mostrarMensaje(response.mensaje, "success");
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    });
}

async function cambiarEstadoSeccion(id, estado) {
    const ok = await confirmar(
        estado
            ? "¿Activar esta sección?"
            : "¿Desactivar esta sección? Si más adelante hay matrículas activas, deberá revisarse antes de desactivar.",
        estado ? "Activar" : "Desactivar",
        "warning"
    );
    if (!ok) return;

    $.post("/Grado/CambiarEstadoSeccion", { id: id, estado: estado }, function (response) {
        if (response.esCorrecto) {
            cargarEstructura();
            mostrarMensaje(response.mensaje, "success");
        } else {
            mostrarMensaje(response.mensaje, "danger");
        }
    });
}