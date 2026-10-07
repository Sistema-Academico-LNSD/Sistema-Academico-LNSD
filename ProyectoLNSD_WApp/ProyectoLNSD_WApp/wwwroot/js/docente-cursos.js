const puedeAsignarCursos = $("#hdnPuedeAsignarCursos").val() === "true";
const puedeQuitarCursos = $("#hdnPuedeQuitarCursos").val() === "true";

$(document).ready(function ()
{
    $("#btnAsignarCurso").on("click", asignarCurso);

    $("#tblCursosActuales .col-quitar").toggleClass("d-none", !puedeQuitarCursos);
});

function abrirModalCursos(idDocente)
{
    $("#hdnIdDocenteCursos").val(idDocente);

    cargarCursosDocente(function ()
    {
        bootstrap.Modal.getOrCreateInstance(document.getElementById("modalCursosDocente")).show();
    });
}

function cargarCursosDocente(alTerminar)
{
    $.ajax({
        url: '/AsignacionCurso/GetCursosDocente',
        type: 'GET',
        data: { idDocente: $("#hdnIdDocenteCursos").val() },

        success: function (response)
        {
            if (!response.esCorrecto || response.dato == null)
            {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            renderizarCursosDocente(response.dato);

            if (alTerminar)
            {
                alTerminar();
            }
        },

        error: function ()
        {
            mostrarMensaje('Error al cargar los cursos del docente.', 'danger');
        }
    });
}

function renderizarCursosDocente(datos)
{
    $("#lblNombreDocenteCursos").text(datos.nombreDocente);
    $("#lblDocenteInactivo").toggleClass("d-none", datos.docenteActivo);
    $("#lblPeriodoActivo").text(datos.nombrePeriodoActivo ?? "sin período activo");

    let motivoBloqueo = null;

    if (!datos.nombrePeriodoActivo)
    {
        motivoBloqueo = "No hay un período lectivo activo. Actívelo en Configuración para poder asignar cursos.";
    }
    else if (!datos.docenteActivo)
    {
        motivoBloqueo = "El docente está inactivo: no puede recibir nuevas asignaciones.";
    }

    let mostrarPanel = puedeAsignarCursos && motivoBloqueo === null;

    $("#panelAsignar").toggleClass("d-none", !mostrarPanel);
    $("#alertaAsignacion")
        .toggleClass("d-none", !(puedeAsignarCursos && motivoBloqueo !== null))
        .text(motivoBloqueo ?? "");

    if (mostrarPanel)
    {
        cargarCursosDisponibles();
    }

    let filas = '';

    $.each(datos.actuales, function (index, a)
    {
        let botonQuitar = puedeQuitarCursos
            ? `<td class="text-end">
                   <button class="btn btn-outline-danger btn-sm" title="Quitar asignación"
                           onclick="quitarCurso(${a.idDocenteCurso})">
                       <i class="bi bi-x-lg"></i>
                   </button>
               </td>`
            : '';

        filas += `
            <tr>
                <td>${escapeHtml(a.codigoCurso)}</td>
                <td>${escapeHtml(a.nombreCurso)}</td>
                <td>${escapeHtml(a.nombreArea ?? '—')}</td>
                <td>${formatearFechaCorta(a.fechaAsignacion)}</td>
                ${botonQuitar}
            </tr>`;
    });

    if (filas === '')
    {
        let columnas = puedeQuitarCursos ? 5 : 4;
        filas = `<tr><td colspan="${columnas}" class="text-center text-muted">Sin cursos asignados en el período actual</td></tr>`;
    }

    $("#tblCursosActuales tbody").html(filas);

    renderizarHistorial(datos.historial);
}
function renderizarHistorial(historial)
{
    if (historial.length === 0)
    {
        $("#panelHistorial").html('<p class="text-muted small mb-0">Sin cursos en períodos anteriores.</p>');
        return;
    }

    let html = '';
    let periodoActual = null;

    $.each(historial, function (index, a)
    {
        if (a.idPeriodo !== periodoActual)
        {
            if (periodoActual !== null)
            {
                html += '</ul></div>';
            }

            periodoActual = a.idPeriodo;

            html += `
                <div class="mb-3">
                    <div class="fw-semibold small text-secondary mb-1">
                        <i class="bi bi-calendar3"></i> ${escapeHtml(a.nombrePeriodo)}
                    </div>
                    <ul class="list-group list-group-flush">`;
        }

        html += `
            <li class="list-group-item px-0 py-1 small">
                <span class="text-muted">${escapeHtml(a.codigoCurso)}</span> — ${escapeHtml(a.nombreCurso)}
                ${a.nombreArea ? `<span class="text-muted">(${escapeHtml(a.nombreArea)})</span>` : ''}
            </li>`;
    });

    html += '</ul></div>';

    $("#panelHistorial").html(html);
}

function cargarCursosDisponibles()
{
    $.ajax({
        url: '/AsignacionCurso/GetCursosDisponibles',
        type: 'GET',
        data: { idDocente: $("#hdnIdDocenteCursos").val() },

        success: function (response)
        {
            let cursos = (response.esCorrecto && response.dato) ? response.dato : [];
            let opciones = '';

            $.each(cursos, function (index, c)
            {
                let area = c.nombreArea ? ` (${escapeHtml(c.nombreArea)})` : '';
                opciones += `<option value="${c.idCurso}">${escapeHtml(c.codigo)} — ${escapeHtml(c.nombre)}${area}</option>`;
            });

            if (cursos.length === 0)
            {
                opciones = '<option value="">No hay más cursos activos para asignar</option>';
            }

            $("#ddlCursosDisponibles").html(opciones).prop("disabled", cursos.length === 0);
            $("#btnAsignarCurso").prop("disabled", cursos.length === 0);
        },

        error: function ()
        {
            mostrarMensaje('Error al cargar los cursos disponibles.', 'danger');
        }
    });
}

function asignarCurso()
{
    let idCurso = $("#ddlCursosDisponibles").val();

    if (!idCurso)
    {
        mostrarMensaje('Seleccione un curso.', 'warning');
        return;
    }

    let $btn = $("#btnAsignarCurso");
    $btn.prop("disabled", true);

    $.ajax({
        url: '/AsignacionCurso/Asignar',
        type: 'POST',
        data: { idDocente: $("#hdnIdDocenteCursos").val(), idCurso: idCurso },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                mostrarMensaje(response.mensaje, 'success');
                cargarCursosDocente();
                refrescarListadoDocentes();
            }
            else
            {
                mostrarMensaje(response.mensaje, 'danger');
                $btn.prop("disabled", false);
            }
        },

        error: function ()
        {
            mostrarMensaje('Error al asignar el curso.', 'danger');
            $btn.prop("disabled", false);
        }
    });
}

async function quitarCurso(idDocenteCurso)
{
    if (!(await confirmar("¿Desea quitar este curso de las asignaciones del período actual?", "Quitar")))
    {
        return;
    }

    $.ajax({
        url: '/AsignacionCurso/Quitar',
        type: 'POST',
        data: { idDocenteCurso: idDocenteCurso },

        success: function (response)
        {
            if (response.esCorrecto)
            {
                mostrarMensaje(response.mensaje, 'success');
                cargarCursosDocente();
                refrescarListadoDocentes();
            }
            else
            {
                mostrarMensaje(response.mensaje, 'danger');
            }
        },

        error: function ()
        {
            mostrarMensaje('Error al quitar la asignación.', 'danger');
        }
    });
}

function refrescarListadoDocentes()
{
    if ($("#ddlFiltroCurso").val() && typeof buscarDocentes === "function")
    {
        buscarDocentes();
    }
}

function formatearFechaCorta(fechaIso)
{
    return new Date(fechaIso).toLocaleDateString('es-CR', { dateStyle: 'medium' });
}