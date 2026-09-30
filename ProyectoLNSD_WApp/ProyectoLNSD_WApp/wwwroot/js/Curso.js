$(document).ready(function () {

    cargarCatalogos();
    cargarCursos();

    $("#btnBuscarCursos").click(function () {
        cargarCursos();
    });

    $("#btnLimpiarCursos").click(function () {

        $("#txtBuscarCurso").val("");
        $("#filtroArea").val("");
        $("#filtroGrado").val("");
        $("#filtroEstado").val("");

        cargarCursos();
    });

    $("#btnGuardarCurso").click(function () {
        crearCurso();
    });

    $("#btnActualizarCurso").click(function () {
        actualizarCurso();
    });

    $("#modalCrearCurso").on("show.bs.modal", function () {
        cargarCatalogos();
    });

    $("#modalEditarCurso").on("show.bs.modal", function () {
        cargarCatalogos();
    });
});


function cargarCatalogos() {

    cargarAreas();
    cargarGrados();
}


function cargarAreas() {

    $.get("/Curso/ObtenerAreas", function (response) {

        let opciones = `
            <option value="">
                Seleccione un área
            </option>`;

        if (response.dato) {

            $.each(response.dato, function (_, area) {

                opciones += `
                    <option value="${area.idArea}">
                        ${escapeHtml(area.nombre)}
                    </option>`;
            });
        }

        $("#filtroArea").html(
            `<option value="">Todas las áreas</option>` +
            opciones.replace(
                '<option value="">Seleccione un área</option>',
                '')
        );

        $("#crearArea").html(opciones);
        $("#editAreaCurso").html(opciones);
    });
}


function cargarGrados() {

    $.get("/Curso/ObtenerGrados", function (response) {

        let opcionesFiltro =
            `<option value="">Todos los grados</option>`;

        let opciones =
            "";

        if (response.dato) {

            $.each(response.dato, function (_, grado) {

                opcionesFiltro += `
                    <option value="${grado.idGrado}">
                        ${escapeHtml(grado.nombre)}
                    </option>`;

                opciones += `
                    <option value="${grado.idGrado}">
                        ${escapeHtml(grado.nombre)}
                        (${escapeHtml(grado.codigo)})
                    </option>`;
            });
        }

        $("#filtroGrado").html(opcionesFiltro);

        $("#crearGrados").html(opciones);
        $("#editGradosCurso").html(opciones);
    });
}


function cargarCursos() {

    $.ajax({

        url: "/Curso/Listar",

        type: "GET",

        data: {

            texto: $("#txtBuscarCurso").val(),

            idGrado:
                $("#filtroGrado").val() || null,

            idArea:
                $("#filtroArea").val() || null,

            estado:
                $("#filtroEstado").val() === ""
                    ? null
                    : $("#filtroEstado").val()

        },

        success: function (response) {

            let filas = "";

            if (!response.dato ||
                response.dato.length === 0) {

                filas = `
                    <tr>
                        <td colspan="6"
                            class="text-center text-muted py-4">
                            No existen cursos que coincidan con los filtros.
                        </td>
                    </tr>`;

            } else {

                $.each(response.dato, function (_, curso) {

                    let estado = curso.estado
                        ? `<span class="badge bg-success">Activo</span>`
                        : `<span class="badge bg-secondary">Inactivo</span>`;

                    let textoEstado =
                        curso.estado
                            ? "Desactivar"
                            : "Activar";

                    let iconoEstado =
                        curso.estado
                            ? "bi-toggle-on"
                            : "bi-toggle-off";

                    filas += `
                        <tr>

                            <td>
                                ${escapeHtml(curso.codigo)}
                            </td>

                            <td>
                                <strong>
                                    ${escapeHtml(curso.nombre)}
                                </strong>
                            </td>

                            <td>
                                ${escapeHtml(curso.nombreArea)}
                            </td>

                            <td>
                                ${escapeHtml(curso.grados || "Sin grados")}
                            </td>

                            <td>
                                ${estado}
                            </td>

                            <td class="text-center">

                                <a href="/Curso/Detalle?id=${curso.idCurso}"
                                   class="btn btn-info btn-sm me-1"
                                   title="Ver detalle">
                                    <i class="bi bi-eye"></i>
                                </a>

                                <button type="button"
                                        class="btn btn-warning btn-sm me-1"
                                        onclick="obtenerCurso(${curso.idCurso})"
                                        title="Editar">
                                    <i class="bi bi-pencil-square"></i>
                                </button>

                                <button type="button"
                                        class="btn btn-secondary btn-sm me-1"
                                        onclick="cambiarEstadoCurso(${curso.idCurso}, ${!curso.estado})"
                                        title="${textoEstado}">
                                    <i class="bi ${iconoEstado}"></i>
                                </button>

                                <button type="button"
                                        class="btn btn-danger btn-sm"
                                        onclick="eliminarCurso(${curso.idCurso})"
                                        title="Eliminar">
                                    <i class="bi bi-trash"></i>
                                </button>

                            </td>

                        </tr>`;
                });
            }

            $("#tblCursos tbody").html(filas);
        },

        error: function () {

            mostrarMensaje(
                "Error al cargar los cursos.",
                "danger"
            );
        }
    });
}


function crearCurso() {

    let grados = $("#crearGrados").val() || [];

    if (grados.length === 0) {

        mostrarMensaje(
            "Seleccione al menos un grado.",
            "warning"
        );

        return;
    }

    $.ajax({

        url: "/Curso/Crear",

        type: "POST",

        data: {

            Codigo: $("#crearCodigo").val(),

            Nombre: $("#crearNombre").val(),

            Descripcion:
                $("#crearDescripcion").val(),

            IdArea:
                $("#crearArea").val(),

            Estado: true,

            IdGrados: grados

        },

        success: function (response) {

            if (response.esCorrecto) {

                bootstrap.Modal
                    .getInstance(
                        document.getElementById(
                            "modalCrearCurso"
                        )
                    )
                    .hide();

                $("#formCrearCurso")[0].reset();

                $("#crearGrados").val([]);

                cargarCursos();

                mostrarMensaje(
                    response.mensaje,
                    "success"
                );

            } else {

                mostrarMensaje(
                    response.mensaje,
                    "danger"
                );
            }
        },

        error: function (xhr) {

            let mensaje =
                xhr.responseJSON?.mensaje ||
                "Error al registrar el curso.";

            mostrarMensaje(
                mensaje,
                "danger"
            );
        }
    });
}


function obtenerCurso(id) {

    $.get(
        "/Curso/Obtener",
        { id: id },
        function (response) {

            if (response.esCorrecto &&
                response.dato) {

                $("#editIdCurso")
                    .val(response.dato.idCurso);

                $("#editCodigoCurso")
                    .val(response.dato.codigo);

                $("#editNombreCurso")
                    .val(response.dato.nombre);

                $("#editDescripcionCurso")
                    .val(response.dato.descripcion);

                $("#editAreaCurso")
                    .val(response.dato.idArea);

                $("#editGradosCurso")
                    .val(response.dato.idGrados);

                new bootstrap.Modal(
                    document.getElementById(
                        "modalEditarCurso"
                    )
                ).show();

            } else {

                mostrarMensaje(
                    response.mensaje,
                    "danger"
                );
            }
        }
    );
}


function actualizarCurso() {

    let grados =
        $("#editGradosCurso").val() || [];

    if (grados.length === 0) {

        mostrarMensaje(
            "Seleccione al menos un grado.",
            "warning"
        );

        return;
    }

    $.ajax({

        url: "/Curso/Editar",

        type: "PUT",

        data: {

            IdCurso:
                $("#editIdCurso").val(),

            Codigo:
                $("#editCodigoCurso").val(),

            Nombre:
                $("#editNombreCurso").val(),

            Descripcion:
                $("#editDescripcionCurso").val(),

            IdArea:
                $("#editAreaCurso").val(),

            IdGrados:
                grados

        },

        success: function (response) {

            if (response.esCorrecto) {

                bootstrap.Modal
                    .getInstance(
                        document.getElementById(
                            "modalEditarCurso"
                        )
                    )
                    .hide();

                cargarCursos();

                mostrarMensaje(
                    response.mensaje,
                    "success"
                );

            } else {

                mostrarMensaje(
                    response.mensaje,
                    "danger"
                );
            }
        },

        error: function (xhr) {

            let mensaje =
                xhr.responseJSON?.mensaje ||
                "Error al actualizar el curso.";

            mostrarMensaje(
                mensaje,
                "danger"
            );
        }
    });
}


async function cambiarEstadoCurso(
    id,
    estado) {

    const ok = await confirmar(

        estado
            ? "¿Activar este curso?"
            : "¿Desactivar este curso?",

        estado
            ? "Activar"
            : "Desactivar",

        "warning"
    );

    if (!ok) {
        return;
    }

    $.post(

        "/Curso/CambiarEstado",

        {
            id: id,
            estado: estado
        },

        function (response) {

            if (response.esCorrecto) {

                cargarCursos();

                mostrarMensaje(
                    response.mensaje,
                    "success"
                );

            } else {

                mostrarMensaje(
                    response.mensaje,
                    "danger"
                );
            }
        }
    ).fail(function () {

        mostrarMensaje(
            "No se pudo cambiar el estado del curso.",
            "danger"
        );
    });
}


async function eliminarCurso(id) {

    const ok = await confirmar(

        "¿Está seguro de eliminar este curso? Esta acción no podrá realizarse si existen dependencias académicas.",

        "Eliminar",

        "danger"
    );

    if (!ok) {
        return;
    }

    $.ajax({

        url: "/Curso/Eliminar?id=" + id,

        type: "DELETE",

        success: function (response) {

            if (response.esCorrecto) {

                cargarCursos();

                mostrarMensaje(
                    response.mensaje,
                    "success"
                );

            } else {

                mostrarMensaje(
                    response.mensaje,
                    "danger"
                );
            }
        },

        error: function (xhr) {

            let mensaje =
                xhr.responseJSON?.mensaje ||
                "No se pudo eliminar el curso.";

            mostrarMensaje(
                mensaje,
                "danger"
            );
        }
    });
}