const puedeEditarDocentes = $("#hdnPuedeEditar").val() === "true";
const puedeVerCursosDocente = $("#hdnPuedeVerCursos").val() === "true";

$(document).ready(function ()
{
    cargarAreas();
    cargarCursosFiltro();
    buscarDocentes();

    $("#btnBuscar").on("click", buscarDocentes);
    $("#btnLimpiar").on("click", limpiarFiltros);
    $("#btnNuevoDocente").on("click", abrirModalNuevo);
    $("#btnGuardarDocente").on("click", guardarDocente);
    $("#btnVincularCuenta").on("click", vincularCuenta);
    $("#btnCrearCuenta").on("click", crearCuenta);
    $("#btnDesvincularCuenta").on("click", desvincularCuenta);

    $("#txtBuscar").on("keydown", function (e)
    {
        if (e.key === "Enter")
        {
            buscarDocentes();
        }
    });
});


function buscarDocentes()
{
    let filtros = {
        texto: $("#txtBuscar").val(),
        idArea: $("#ddlFiltroArea").val(),
        idCurso: $("#ddlFiltroCurso").val(),
        estado: $("#ddlFiltroEstado").val()
    };

    $.ajax({
        url: '/Docente/Buscar',
        type: 'GET',
        data: filtros,

        success: function (response)
        {
            if (!response.esCorrecto)
            {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            renderizarDocentes(response.dato ?? []);
        },

        error: function ()
        {
            mostrarMensaje('Error al cargar los docentes.', 'danger');
        }
    });
}


function renderizarDocentes(docentes)
{
    let columnas = 8;
    let filas = '';

    $.each(docentes, function (index, d)
    {
        let badgeCuenta = d.tieneCuenta
            ? '<span class="badge bg-info text-dark"><i class="bi bi-person-check"></i> Con cuenta</span>'
            : '<span class="badge bg-light text-muted border">Sin cuenta</span>';

        let badgeEstado = d.estado
            ? '<span class="badge bg-success">Activo</span>'
            : '<span class="badge bg-danger">Inactivo</span>';

        // Expediente completo (MDOF-01-12): todos los que ven esta pantalla pueden abrirlo.
        let botonExpediente = `
            <a class="btn btn-secondary btn-sm me-1" title="Ver expediente"
               href="/Docente/Expediente/${d.idDocente}">
                <i class="bi bi-folder2-open"></i>
            </a>`;

        let acciones = '';

        // Botón "Cursos" (MDOF-01-05): lo ven quienes consultan asignaciones,
        // aunque no puedan editar el expediente (ej. el Director).
        let botonCursos = puedeVerCursosDocente
            ? `<button class="btn btn-primary btn-sm me-1" title="Cursos asignados"
                       onclick="abrirModalCursos(${d.idDocente})">
                   <i class="bi bi-journal-bookmark"></i>
               </button>`
            : '';

        if (puedeEditarDocentes)
        {
            let botonEstado = d.estado
                ? `<button class="btn btn-danger btn-sm" title="Inactivar"
                           onclick="cambiarEstadoDocente(${d.idDocente}, false)">
                       <i class="bi bi-x-circle"></i>
                   </button>`
                : `<button class="btn btn-success btn-sm" title="Activar"
                           onclick="cambiarEstadoDocente(${d.idDocente}, true)">
                       <i class="bi bi-check-circle"></i>
                   </button>`;

            acciones = `
                <td class="text-nowrap">
                    ${botonExpediente}
                    <button class="btn btn-warning btn-sm me-1" title="Editar"
                            onclick="abrirModalEditar(${d.idDocente})">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    <button class="btn btn-info btn-sm me-1" title="Cuenta de usuario"
                            onclick="abrirModalCuenta(${d.idDocente})">
                        <i class="bi bi-person-gear"></i>
                    </button>
                    ${botonCursos}
                    ${botonEstado}
                </td>`;
        }
        else
        {
            acciones = `<td class="text-nowrap">${botonExpediente}${botonCursos}</td>`;
        }

        filas += `
            <tr>
                <td>${escapeHtml(d.identificacion)}</td>
                <td>${escapeHtml(d.apellidos)}, ${escapeHtml(d.nombre)}</td>
                <td>${escapeHtml(d.correo)}</td>
                <td>${escapeHtml(d.especialidad ?? '—')}</td>
                <td>${escapeHtml(d.nombreArea ?? '—')}</td>
                <td>${badgeCuenta}</td>
                <td>${badgeEstado}</td>
                ${acciones}
            </tr>`;
    });

    if (filas === '')
    {
        filas = `<tr><td colspan="${columnas}" class="text-center text-muted">No se encontraron docentes</td></tr>`;
    }

    $("#tblDocentes tbody").html(filas);
    $("#lblTotal").text(docentes.length === 1 ? "1 docente" : `${docentes.length} docentes`);
}

function limpiarFiltros()
{
    $("#txtBuscar").val('');
    $("#ddlFiltroArea").val('');
    $("#ddlFiltroCurso").val('');
    $("#ddlFiltroEstado").val('');

    buscarDocentes();
}

function cargarCursosFiltro()
{
    $.ajax({
        url: '/Docente/GetCursosFiltro',
        type: 'GET',

        success: function (response)
        {
            if (!response.esCorrecto || response.dato == null)
            {
                return;
            }

            let opciones = '';

            $.each(response.dato, function (index, c)
            {
                opciones += `<option value="${c.idCurso}">${escapeHtml(c.codigo)} — ${escapeHtml(c.nombre)}</option>`;
            });

            $("#ddlFiltroCurso").append(opciones);
        },

        error: function ()
        {
            mostrarMensaje('Error al cargar los cursos.', 'danger');
        }
    });
}

function cargarAreas()
{
    $.ajax({
        url: '/Docente/GetAreas',
        type: 'GET',

        success: function (response)
        {
            if (!response.esCorrecto || response.dato == null)
            {
                return;
            }

            let opciones = '';

            $.each(response.dato, function (index, a)
            {
                opciones += `
                    <option value="${a.idArea}">
                        ${escapeHtml(a.nombre)}
                    </option>`;
            });

            $("#ddlFiltroArea").append(opciones);
            $("#ddlArea").append(opciones);
        },

        error: function ()
        {
            mostrarMensaje(
                'Error al cargar las áreas académicas.',
                'danger'
            );
        }
    });
}


function abrirModalNuevo()
{
    limpiarFormulario();

    $("#lblTituloModalDocente").text("Nuevo docente");
    $("#lblBotonGuardar").text("Registrar");

    bootstrap.Modal
        .getOrCreateInstance(
            document.getElementById("modalDocente")
        )
        .show();
}


function abrirModalEditar(idDocente)
{
    $.ajax({
        url: '/Docente/GetDocente',
        type: 'GET',
        data: { id: idDocente },

        success: function (response)
        {
            if (!response.esCorrecto || response.dato == null)
            {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let d = response.dato;

            limpiarFormulario();

            $("#hdnIdDocente").val(d.idDocente);
            $("#txtNombre").val(d.nombre);
            $("#txtApellidos").val(d.apellidos);
            $("#txtIdentificacion").val(d.identificacion);
            $("#txtCorreo").val(d.correo);
            $("#txtTelefono").val(d.telefono);
            $("#txtDireccion").val(d.direccion);
            $("#txtEspecialidad").val(d.especialidad);
            $("#ddlArea").val(d.idArea ?? '');
            $("#txtExperiencia").val(d.aniosExperiencia ?? '');
            $("#txtTitulos").val(d.titulos);

            $("#lblTituloModalDocente").text("Editar docente");
            $("#lblBotonGuardar").text("Guardar cambios");

            bootstrap.Modal
                .getOrCreateInstance(
                    document.getElementById("modalDocente")
                )
                .show();
        },

        error: function ()
        {
            mostrarMensaje(
                'Error al obtener el docente.',
                'danger'
            );
        }
    });
}


function guardarDocente()
{
    let idDocente = parseInt(
        $("#hdnIdDocente").val(),
        10
    ) || 0;

    let esNuevo = idDocente === 0;

    let docente = {
        idDocente: idDocente,
        nombre: $("#txtNombre").val(),
        apellidos: $("#txtApellidos").val(),
        identificacion: $("#txtIdentificacion").val(),
        correo: $("#txtCorreo").val(),
        telefono: $("#txtTelefono").val(),
        direccion: $("#txtDireccion").val(),
        especialidad: $("#txtEspecialidad").val(),
        idArea: $("#ddlArea").val(),
        aniosExperiencia: $("#txtExperiencia").val(),
        titulos: $("#txtTitulos").val()
    };

    let $btn = $("#btnGuardarDocente");

    $btn.prop("disabled", true);

    $.ajax({
        url: esNuevo
            ? '/Docente/Crear'
            : '/Docente/Actualizar',

        type: 'POST',
        data: docente,

        success: function (response)
        {
            if (response.esCorrecto)
            {
                bootstrap.Modal
                    .getInstance(
                        document.getElementById("modalDocente")
                    )
                    .hide();

                buscarDocentes();

                mostrarMensaje(
                    response.mensaje,
                    'success'
                );
            }
            else
            {
                mostrarMensaje(
                    response.mensaje,
                    'danger'
                );
            }
        },

        error: function ()
        {
            mostrarMensaje(
                esNuevo
                    ? 'Error al registrar el docente.'
                    : 'Error al actualizar el docente.',
                'danger'
            );
        },

        complete: function ()
        {
            $btn.prop("disabled", false);
        }
    });
}


function limpiarFormulario()
{
    $("#hdnIdDocente").val(0);

    $("#modalDocente")
        .find(
            "input[type=text], input[type=email], input[type=tel], input[type=number], textarea"
        )
        .val('');

    $("#ddlArea").val('');
}


async function cambiarEstadoDocente(idDocente, nuevoEstado)
{
    let mensaje = nuevoEstado
        ? "¿Desea activar este docente? Volverá a estar disponible para asignaciones académicas."
        : "¿Desea inactivar este docente? Conservará su expediente e historial, pero no podrá recibir nuevas asignaciones.";

    if (!(await confirmar(
        mensaje,
        nuevoEstado ? 'Activar' : 'Inactivar',
        nuevoEstado ? 'success' : 'danger'
    )))
    {
        return;
    }

    $.ajax({
        url: '/Docente/CambiarEstado',
        type: 'POST',
        data: {
            id: idDocente,
            estado: nuevoEstado
        },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                buscarDocentes();

                mostrarMensaje(
                    response.mensaje,
                    'success'
                );
            }
            else
            {
                mostrarMensaje(
                    response.mensaje,
                    'danger'
                );
            }
        },

        error: function ()
        {
            mostrarMensaje(
                'Error al cambiar el estado del docente.',
                'danger'
            );
        }
    });
}


// ============================================================
// Cuenta de usuario (MDOF-01-02)
// ============================================================

function abrirModalCuenta(idDocente)
{
    $.ajax({
        url: '/Docente/GetCuenta',
        type: 'GET',
        data: { id: idDocente },

        success: function (response)
        {
            if (!response.esCorrecto || response.dato == null)
            {
                mostrarMensaje(
                    response.mensaje,
                    'danger'
                );

                return;
            }

            mostrarCuenta(response.dato);

            bootstrap.Modal
                .getOrCreateInstance(
                    document.getElementById("modalCuentaDocente")
                )
                .show();
        },

        error: function ()
        {
            mostrarMensaje(
                'Error al obtener la cuenta del docente.',
                'danger'
            );
        }
    });
}


function mostrarCuenta(cuenta)
{
    $("#hdnIdDocenteCuenta").val(
        cuenta.idDocente
    );

    $("#lblNombreDocenteCuenta").text(
        cuenta.nombreDocente
    );

    // Se limpian los campos de contraseña
    // cada vez que se abre el modal.
    $("#txtPasswordCuenta, #txtConfirmarCuenta").val('');

    if (cuenta.tieneCuenta)
    {
        $("#lblCorreoCuenta").text(
            cuenta.correoCuenta
        );

        $("#lblEstadoCuenta").text(
            cuenta.cuentaActiva
                ? "(activa)."
                : "(inactiva: no puede iniciar sesión)."
        );

        $("#panelConCuenta").removeClass("d-none");
        $("#panelSinCuenta").addClass("d-none");
        $("#btnDesvincularCuenta").removeClass("d-none");
    }
    else
    {
        $("#txtCorreoCuenta").val(
            cuenta.correoDocente
        );

        $("#panelConCuenta").addClass("d-none");
        $("#panelSinCuenta").removeClass("d-none");
        $("#btnDesvincularCuenta").addClass("d-none");

        cargarUsuariosDisponibles();
    }
}


function cargarUsuariosDisponibles()
{
    $.ajax({
        url: '/Docente/GetUsuariosDisponibles',
        type: 'GET',

        success: function (response)
        {
            let usuarios =
                (response.esCorrecto && response.dato)
                    ? response.dato
                    : [];

            let opciones = '';

            $.each(usuarios, function (index, u)
            {
                opciones += `
                    <option value="${u.idUsuario}">
                        ${escapeHtml(u.nombre)}
                        ${escapeHtml(u.apellido)}
                        — ${escapeHtml(u.correo)}
                    </option>`;
            });

            $("#ddlUsuariosDisponibles")
                .html(opciones)
                .prop(
                    "disabled",
                    usuarios.length === 0
                );

            $("#btnVincularCuenta")
                .prop(
                    "disabled",
                    usuarios.length === 0
                );

            $("#lblSinDisponibles")
                .toggleClass(
                    "d-none",
                    usuarios.length > 0
                );
        },

        error: function ()
        {
            mostrarMensaje(
                'Error al cargar las cuentas disponibles.',
                'danger'
            );
        }
    });
}


function vincularCuenta()
{
    let datos = {
        idDocente: $("#hdnIdDocenteCuenta").val(),
        idUsuario: $("#ddlUsuariosDisponibles").val()
    };

    if (!datos.idUsuario)
    {
        mostrarMensaje(
            'Seleccione una cuenta.',
            'warning'
        );

        return;
    }

    enviarOperacionCuenta(
        '/Docente/VincularCuenta',
        datos,
        $("#btnVincularCuenta")
    );
}


function crearCuenta()
{
    let datos = {
        idDocente: $("#hdnIdDocenteCuenta").val(),
        correo: $("#txtCorreoCuenta").val(),
        password: $("#txtPasswordCuenta").val(),
        confirmarPassword: $("#txtConfirmarCuenta").val()
    };

    enviarOperacionCuenta(
        '/Docente/CrearCuenta',
        datos,
        $("#btnCrearCuenta")
    );
}


async function desvincularCuenta()
{
    if (!(await confirmar(
        "¿Desea desvincular la cuenta de este docente? La cuenta no se elimina, pero dejará de estar asociada al expediente.",
        "Desvincular"
    )))
    {
        return;
    }

    enviarOperacionCuenta(
        '/Docente/DesvincularCuenta',
        {
            idDocente: $("#hdnIdDocenteCuenta").val()
        },
        $("#btnDesvincularCuenta")
    );
}


/**
 * Las tres operaciones de cuenta comparten la misma respuesta:
 * si sale bien, se cierra el modal y se refresca la tabla
 * para actualizar la columna "Cuenta".
 */
function enviarOperacionCuenta(url, datos, $boton)
{
    $boton.prop("disabled", true);

    $.ajax({
        url: url,
        type: 'POST',
        data: datos,

        success: function (response)
        {
            if (response.esCorrecto)
            {
                bootstrap.Modal
                    .getInstance(
                        document.getElementById("modalCuentaDocente")
                    )
                    .hide();

                buscarDocentes();

                mostrarMensaje(
                    response.mensaje,
                    'success'
                );
            }
            else
            {
                mostrarMensaje(
                    response.mensaje,
                    'danger'
                );
            }
        },

        error: function ()
        {
            mostrarMensaje(
                'Error al procesar la cuenta del docente.',
                'danger'
            );
        },

        complete: function ()
        {
            $boton.prop("disabled", false);
        }
    });
}
