const puedeEditarEstudiantes = $("#hdnPuedeEditar").val() === "true";

$(document).ready(function ()
{
    cargarGrados();
    buscarEstudiantes();

    $("#btnBuscar").on("click", buscarEstudiantes);
    $("#btnLimpiar").on("click", limpiarFiltros);
    $("#btnNuevoEstudiante").on("click", abrirModalNuevo);
    $("#btnGuardarEstudiante").on("click", guardarEstudiante);

    $("#txtBuscar").on("keydown", function (e)
    {
        if (e.key === "Enter")
        {
            buscarEstudiantes();
        }
    });
});


function buscarEstudiantes()
{
    $.ajax({
        url: '/Estudiante/Buscar',
        type: 'GET',
        data: {
            texto: $("#txtBuscar").val(),
            idGrado: $("#ddlFiltroGrado").val(),
            estado: $("#ddlFiltroEstado").val()
        },

        success: function (response)
        {
            if (!response.esCorrecto)
            {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            renderizarEstudiantes(response.dato ?? []);
        },

        error: function ()
        {
            mostrarMensaje('Error al cargar los estudiantes.', 'danger');
        }
    });
}


function renderizarEstudiantes(estudiantes)
{
    let columnas = puedeEditarEstudiantes ? 8 : 7;
    let filas = '';

    $.each(estudiantes, function (index, e)
    {
        let badgeEstado = e.estado
            ? '<span class="badge bg-success">Activo</span>'
            : '<span class="badge bg-danger">Inactivo</span>';

        let acciones = '';

        if (puedeEditarEstudiantes)
        {
            let botonEstado = e.estado
                ? `<button class="btn btn-danger btn-sm" title="Inactivar"
                           onclick="cambiarEstadoEstudiante(${e.idEstudiante}, false)">
                       <i class="bi bi-x-circle"></i>
                   </button>`
                : `<button class="btn btn-success btn-sm" title="Activar"
                           onclick="cambiarEstadoEstudiante(${e.idEstudiante}, true)">
                       <i class="bi bi-check-circle"></i>
                   </button>`;

            acciones = `
                <td class="text-nowrap">
                    <button class="btn btn-warning btn-sm me-1" title="Editar"
                            onclick="abrirModalEditar(${e.idEstudiante})">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    ${botonEstado}
                </td>`;
        }

        filas += `
            <tr>
                <td class="text-nowrap">${escapeHtml(e.carnet)}</td>
                <td>${escapeHtml(e.identificacion)}</td>
                <td>${escapeHtml(e.apellidos)}, ${escapeHtml(e.nombre)}</td>
                <td>${escapeHtml(e.correo)}</td>
                <td>${escapeHtml(e.nombreGrado ?? '—')}</td>
                <td>${escapeHtml(e.estadoAcademico)}</td>
                <td>${badgeEstado}</td>
                ${acciones}
            </tr>`;
    });

    if (filas === '')
    {
        filas = `<tr>
                    <td colspan="${columnas}" class="text-center text-muted">
                        No se encontraron estudiantes
                    </td>
                </tr>`;
    }

    $("#tblEstudiantes tbody").html(filas);

    $("#lblTotal").text(
        estudiantes.length === 1
            ? "1 estudiante"
            : `${estudiantes.length} estudiantes`
    );
}


function limpiarFiltros()
{
    $("#txtBuscar").val('');
    $("#ddlFiltroGrado").val('');
    $("#ddlFiltroEstado").val('');

    buscarEstudiantes();
}


function cargarGrados()
{
    $.ajax({
        url: '/Estudiante/GetGrados',
        type: 'GET',

        success: function (response)
        {
            if (!response.esCorrecto || response.dato == null)
            {
                return;
            }

            let opciones = '';

            $.each(response.dato, function (index, g)
            {
                opciones += `<option value="${g.idGrado}">${escapeHtml(g.nombre)}</option>`;
            });

            $("#ddlFiltroGrado").append(opciones);
            $("#ddlGrado").append(opciones);
        },

        error: function ()
        {
            mostrarMensaje('Error al cargar los grados.', 'danger');
        }
    });
}


function abrirModalNuevo()
{
    limpiarFormularioEstudiante();

    $("#lblTituloModalEstudiante").text("Nuevo estudiante");
    $("#lblBotonGuardar").text("Registrar");
    $("#seccionCuenta").show();

    bootstrap.Modal
        .getOrCreateInstance(document.getElementById("modalEstudiante"))
        .show();
}


function abrirModalEditar(idEstudiante)
{
    $.ajax({
        url: '/Estudiante/GetEstudiante',
        type: 'GET',
        data: { id: idEstudiante },

        success: function (response)
        {
            if (!response.esCorrecto || response.dato == null)
            {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            let e = response.dato;

            limpiarFormularioEstudiante();

            $("#hdnIdEstudiante").val(e.idEstudiante);
            $("#txtNombre").val(e.nombre);
            $("#txtApellidos").val(e.apellidos);
            $("#txtIdentificacion").val(e.identificacion);
            $("#txtFechaNacimiento").val(e.fechaNacimiento);
            $("#txtCorreo").val(e.correo);
            $("#txtCorreoEmergencia").val(e.correoEmergencia);
            $("#txtTelefono").val(e.telefono);
            $("#txtDireccion").val(e.direccion);
            $("#ddlGrado").val(e.idGrado ?? '');
            $("#ddlEstadoAcademico").val(e.estadoAcademico);
            $("#txtFechaIngreso").val(e.fechaIngreso);
            $("#txtAlergias").val(e.alergias);
            $("#txtObservacionesMedicas").val(e.observacionesMedicas);
            $("#txtAdecuaciones").val(e.adecuacionesEducativas);

            // La contraseña no se edita aquí
            $("#seccionCuenta").hide();

            $("#lblTituloModalEstudiante").text(`Editar estudiante (${e.carnet})`);
            $("#lblBotonGuardar").text("Guardar cambios");

            bootstrap.Modal
                .getOrCreateInstance(document.getElementById("modalEstudiante"))
                .show();
        },

        error: function ()
        {
            mostrarMensaje('Error al obtener el estudiante.', 'danger');
        }
    });
}


function limpiarFormularioEstudiante()
{
    $("#hdnIdEstudiante").val(0);

    $("#modalEstudiante")
        .find("input[type=text], input[type=email], input[type=tel], input[type=date], input[type=password], textarea")
        .val('');

    $("#ddlGrado").val('');
    $("#ddlEstadoAcademico").val('Regular');

    // Por defecto, la fecha de ingreso es hoy (fecha local)
    let hoy = new Date();
    let local = new Date(hoy.getTime() - hoy.getTimezoneOffset() * 60000);
    $("#txtFechaIngreso").val(local.toISOString().slice(0, 10));
}


function guardarEstudiante()
{
    let idEstudiante = parseInt($("#hdnIdEstudiante").val(), 10) || 0;
    let esNuevo = idEstudiante === 0;

    let estudiante = {
        idEstudiante: idEstudiante,
        nombre: $("#txtNombre").val(),
        apellidos: $("#txtApellidos").val(),
        identificacion: $("#txtIdentificacion").val(),
        fechaNacimiento: $("#txtFechaNacimiento").val(),
        correo: $("#txtCorreo").val(),
        correoEmergencia: $("#txtCorreoEmergencia").val(),
        telefono: $("#txtTelefono").val(),
        direccion: $("#txtDireccion").val(),
        idGrado: $("#ddlGrado").val(),
        estadoAcademico: $("#ddlEstadoAcademico").val(),
        fechaIngreso: $("#txtFechaIngreso").val(),
        alergias: $("#txtAlergias").val(),
        observacionesMedicas: $("#txtObservacionesMedicas").val(),
        adecuacionesEducativas: $("#txtAdecuaciones").val(),
        password: $("#txtPassword").val(),
        confirmarPassword: $("#txtConfirmarPassword").val()
    };

    let $btn = $("#btnGuardarEstudiante");

    $btn.prop("disabled", true);

    $.ajax({
        url: esNuevo ? '/Estudiante/Crear' : '/Estudiante/Actualizar',
        type: 'POST',
        data: estudiante,

        success: function (response)
        {
            if (response.esCorrecto)
            {
                bootstrap.Modal
                    .getInstance(document.getElementById("modalEstudiante"))
                    .hide();

                buscarEstudiantes();

                mostrarMensaje(response.mensaje, 'success');
            }
            else
            {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function ()
        {
            mostrarMensaje(
                esNuevo
                    ? 'Error al registrar el estudiante.'
                    : 'Error al actualizar el estudiante.',
                'danger');
        },

        complete: function ()
        {
            $btn.prop("disabled", false);
        }
    });
}



async function cambiarEstadoEstudiante(idEstudiante, nuevoEstado)
{
    let mensaje = nuevoEstado
        ? "¿Desea activar este estudiante? Volverá a estar disponible para matrícula."
        : "¿Desea inactivar este estudiante? Conservará su expediente e historial, pero no estará disponible para nuevas matrículas.";

    if (!(await confirmar(
        mensaje,
        nuevoEstado ? 'Activar' : 'Inactivar',
        nuevoEstado ? 'success' : 'danger'
    )))
    {
        return;
    }

    $.ajax({
        url: '/Estudiante/CambiarEstado',
        type: 'POST',
        data: { id: idEstudiante, estado: nuevoEstado },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                buscarEstudiantes();
                mostrarMensaje(response.mensaje, 'success');
            }
            else
            {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function ()
        {
            mostrarMensaje('Error al cambiar el estado del estudiante.', 'danger');
        }
    });
}
