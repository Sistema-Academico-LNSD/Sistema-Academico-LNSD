const SUFIJO_ACTIVO = ' (activo)';
const DIAS_POR_DEFECTO = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes'];

let opcionesFormulario = null;   // período activo, secciones activas y días (GetOpciones)
let horariosBase = [];           // horarios del período elegido, sin otros filtros: de aquí salen las listas de los filtros
let horariosListados = [];       // lo que muestra la tabla

$(document).ready(function () {
    cargarOpciones();
    cargarPeriodosFiltro();

    // Registrar
    $("#hrSeccion").on("change", function () { alCambiarSeccion('hr'); });
    $("#hrCurso").on("change", function () { alCambiarCurso('hr'); });
    $("#btnLimpiarHorario").on("click", limpiarFormulario);

    $("#formHorario").on("submit", function (e) {
        e.preventDefault();
        guardarHorario();
    });

    // Buscar
    $("#fltPeriodo").on("change", function () { cargarBaseDelPeriodo(false); });
    $("#fltCurso, #fltDocente, #fltSeccion, #fltGrupo, #fltEstado").on("change", buscarHorarios);
    $("#btnBuscarHorarios").on("click", buscarHorarios);
    $("#btnLimpiarBusqueda").on("click", limpiarBusqueda);

    $("#fltAula").on("keydown", function (e) {
        if (e.key === 'Enter') {
            buscarHorarios();
        }
    });

    // Acciones de cada fila
    $("#tblHorarios").on("click", ".btn-detalle-horario", function () {
        abrirDetalle(parseInt($(this).data("id")));
    });

    $("#tblHorarios").on("click", ".btn-editar-horario", function () {
        abrirEditar(parseInt($(this).data("id")));
    });

    $("#tblHorarios").on("click", ".btn-estado-horario", function () {
        cambiarEstado(parseInt($(this).data("id")), $(this).attr("data-activar") === 'true');
    });

    // Editar
    $("#edSeccion").on("change", function () { alCambiarSeccion('ed'); });
    $("#edCurso").on("change", function () { alCambiarCurso('ed'); });

    $("#formEditarHorario").on("submit", function (e) {
        e.preventDefault();
        guardarEdicion();
    });
});

// =====================================================
// Utilidades
// =====================================================

function llenarSelect(selector, lista, textoPrimera, formatoTexto) {
    let opciones = `<option value="">${escapeHtml(textoPrimera)}</option>`;

    $.each(lista, function (index, item) {
        opciones += `<option value="${item.id}">${escapeHtml(formatoTexto(item))}</option>`;
    });

    $(selector).html(opciones);
}

function reiniciarSelect(selector, texto) {
    $(selector).html(`<option value="">${escapeHtml(texto)}</option>`);
}

// Deja elegido un valor aunque ya no esté entre las opciones (por ejemplo, una sección inactiva)
function asegurarOpcion(selector, valor, texto) {
    let $select = $(selector);

    if ($select.find(`option[value="${valor}"]`).length === 0) {
        $select.append(`<option value="${valor}">${escapeHtml(texto)}</option>`);
    }

    $select.val(valor);
}

function badgeEstado(activo) {
    return activo
        ? '<span class="badge bg-success">Activo</span>'
        : '<span class="badge bg-secondary">Inactivo</span>';
}

function ocultarModal(id) {
    bootstrap.Modal.getOrCreateInstance(document.getElementById(id)).hide();
}

function mostrarModal(id) {
    bootstrap.Modal.getOrCreateInstance(document.getElementById(id)).show();
}

// =====================================================
// Listas encadenadas (sirven al formulario de registro "hr" y al de edición "ed")
// =====================================================

function cargarGrupos(p, idSeccion) {
    return $.ajax({
        url: '/Horario/GetGrupos',
        type: 'GET',
        data: { idSeccion: idSeccion },

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            llenarSelect(`#${p}Grupo`, response.dato, "Seleccione un grupo", g => g.nombre);

            if (response.dato.length === 0) {
                mostrarMensaje(response.mensaje, 'warning');
            }
            else if (response.dato.length === 1) {
                $(`#${p}Grupo`).val(response.dato[0].id);
            }
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los grupos.", 'danger');
        }
    });
}

function cargarCursos(p, idSeccion) {
    return $.ajax({
        url: '/Horario/GetCursos',
        type: 'GET',
        data: { idSeccion: idSeccion },

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            llenarSelect(`#${p}Curso`, response.dato, "Seleccione un curso", c => c.nombre);

            if (response.dato.length === 0) {
                mostrarMensaje(response.mensaje, 'warning');
            }
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los cursos.", 'danger');
        }
    });
}

function cargarDocentes(p, idCurso) {
    return $.ajax({
        url: '/Horario/GetDocentes',
        type: 'GET',
        data: { idCurso: idCurso },

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                return;
            }

            llenarSelect(`#${p}Docente`, response.dato, "Seleccione un docente", d => d.nombre);

            if (response.dato.length === 0) {
                mostrarMensaje(response.mensaje, 'warning');
            }
            else if (response.dato.length === 1) {
                $(`#${p}Docente`).val(response.dato[0].id);
            }
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los docentes.", 'danger');
        }
    });
}

function alCambiarSeccion(p) {
    let idSeccion = $(`#${p}Seccion`).val();

    reiniciarSelect(`#${p}Grupo`, "Seleccione primero una sección");
    reiniciarSelect(`#${p}Curso`, "Seleccione primero una sección");
    reiniciarSelect(`#${p}Docente`, "Seleccione primero un curso");

    if (!idSeccion) {
        return;
    }

    cargarGrupos(p, idSeccion);
    cargarCursos(p, idSeccion);
}

function alCambiarCurso(p) {
    let idCurso = $(`#${p}Curso`).val();

    reiniciarSelect(`#${p}Docente`, "Seleccione primero un curso");

    if (!idCurso) {
        return;
    }

    cargarDocentes(p, idCurso);
}

// Validaciones rápidas en el navegador; el servidor las repite todas
function validarFormulario(p) {
    if (!$(`#${p}Seccion`).val()) return "Seleccione una sección.";
    if (!$(`#${p}Grupo`).val()) return "Seleccione un grupo.";
    if (!$(`#${p}Curso`).val()) return "Seleccione un curso.";
    if (!$(`#${p}Docente`).val()) return "Seleccione un docente.";
    if (!$(`#${p}Dia`).val()) return "Seleccione el día.";

    let inicio = $(`#${p}Inicio`).val();
    let fin = $(`#${p}Fin`).val();

    if (!inicio || !fin) return "Indique la hora de inicio y la de finalización.";

    // Comparación de textos "HH:mm": funciona porque tienen el mismo formato
    if (fin <= inicio) return "La hora de finalización debe ser posterior a la hora de inicio.";

    if (!$(`#${p}Aula`).val().trim()) return "Indique el aula.";

    return null;
}

// Envía el formulario y muestra el resultado; quien llama decide qué hacer después
function enviarHorario(url, formulario, boton, textoError, alResponder) {
    $(boton).prop('disabled', true);

    $.ajax({
        url: url,
        type: 'POST',
        data: $(formulario).serialize(),

        success: function (response) {
            if (response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                // 1305 a 1307 = conflictos de docente, sección y grupo, o aula
                let esConflicto = response.codigo >= 1305 && response.codigo <= 1307;

                mostrarMensaje(response.mensaje, esConflicto ? 'warning' : 'danger');
            }

            alResponder(response);
        },

        error: function () {
            mostrarMensaje(textoError, 'danger');
        },

        complete: function () {
            $(boton).prop('disabled', false);
        }
    });
}

// =====================================================
// Opciones del formulario de registro
// =====================================================

function cargarOpciones() {
    $.ajax({
        url: '/Horario/GetOpciones',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                // Sin período activo no se puede registrar nada
                $("#avisoHorario").html(`
                    <div class="alert alert-warning mb-0">
                        <i class="bi bi-exclamation-triangle"></i>
                        ${escapeHtml(response.mensaje)}
                    </div>`);

                $("#fsHorario").prop('disabled', true);
                $("#hrPeriodo").text('Sin período activo');
                return;
            }

            opcionesFormulario = response.dato;

            $("#hrPeriodo").text('Período activo: ' + opcionesFormulario.nombrePeriodo);

            let secciones = '<option value="">Seleccione una sección</option>';

            $.each(opcionesFormulario.secciones, function (index, s) {
                secciones += `<option value="${s.idSeccion}">${escapeHtml(s.nombreGrado)} - ${escapeHtml(s.nombre)}</option>`;
            });

            $("#hrSeccion").html(secciones);
            $("#hrDia").html(opcionesDias());
        },

        error: function () {
            mostrarMensaje("No fue posible cargar las opciones.", 'danger');
        }
    });
}

function opcionesDias() {
    let dias = opcionesFormulario ? opcionesFormulario.dias : DIAS_POR_DEFECTO;
    let opciones = '<option value="">Seleccione el día</option>';

    $.each(dias, function (index, d) {
        opciones += `<option value="${escapeHtml(d)}">${escapeHtml(d)}</option>`;
    });

    return opciones;
}

function limpiarFormulario() {
    $("#formHorario")[0].reset();

    reiniciarSelect("#hrGrupo", "Seleccione primero una sección");
    reiniciarSelect("#hrCurso", "Seleccione primero una sección");
    reiniciarSelect("#hrDocente", "Seleccione primero un curso");
}

// =====================================================
// Registrar (MHOF-01-01 a 01-04)
// =====================================================

function guardarHorario() {
    let error = validarFormulario('hr');

    if (error) {
        mostrarMensaje(error, 'warning');
        return;
    }

    enviarHorario('/Horario/Registrar', '#formHorario', '#btnGuardarHorario',
        "No se pudo registrar el horario. Intente nuevamente.",
        function (response) {
            if (!response.esCorrecto) {
                return;
            }

            // Se conservan sección, grupo y día para registrar el siguiente bloque más rápido
            $("#hrCurso").val('');
            reiniciarSelect("#hrDocente", "Seleccione primero un curso");
            $("#hrInicio").val('');
            $("#hrFin").val('');
            $("#hrAula").val('');

            refrescar();
        });
}

// =====================================================
// Búsqueda y listado (MHOF-01-11)
// =====================================================

function cargarPeriodosFiltro() {
    $.ajax({
        url: '/Horario/GetPeriodos',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto || !response.dato || response.dato.length === 0) {
                $("#tblHorarios tbody").html(`
                    <tr><td colspan="9" class="text-center text-muted">No existen períodos lectivos registrados</td></tr>`);
                return;
            }

            llenarSelect("#fltPeriodo", response.dato, "Seleccione un período", p => p.nombre);

            // Se muestra por defecto el período activo; si no hay, el primero de la lista
            let activo = response.dato.find(p => p.nombre.endsWith(SUFIJO_ACTIVO)) ?? response.dato[0];

            $("#fltPeriodo").val(activo.id);

            cargarBaseDelPeriodo(false);
        },

        error: function () {
            mostrarMensaje("No fue posible cargar los períodos.", 'danger');
        }
    });
}

function distintos(lista, obtenerId, obtenerTexto) {
    let vistos = {};

    $.each(lista, function (index, h) {
        vistos[obtenerId(h)] = obtenerTexto(h);
    });

    return Object.keys(vistos)
        .map(id => ({ id: id, nombre: vistos[id] }))
        .sort((a, b) => a.nombre.localeCompare(b.nombre, 'es'));
}

// Las listas de los filtros salen de los horarios que existen en el período elegido
function llenarFiltros(conservar) {
    let anterior = {
        curso: $("#fltCurso").val(),
        docente: $("#fltDocente").val(),
        seccion: $("#fltSeccion").val(),
        grupo: $("#fltGrupo").val()
    };

    llenarSelect("#fltCurso",
        distintos(horariosBase, h => h.idCurso, h => `${h.codigoCurso} - ${h.nombreCurso}`),
        "Todos los cursos", x => x.nombre);

    llenarSelect("#fltDocente",
        distintos(horariosBase, h => h.idDocente, h => h.nombreDocente),
        "Todos los docentes", x => x.nombre);

    llenarSelect("#fltSeccion",
        distintos(horariosBase, h => h.idSeccion, h => `${h.nombreGrado} - ${h.nombreSeccion}`),
        "Todas las secciones", x => x.nombre);

    llenarSelect("#fltGrupo",
        distintos(horariosBase, h => h.idGrupo, h => h.nombreGrupo),
        "Todos los grupos", x => x.nombre);

    if (conservar) {
        $("#fltCurso").val(anterior.curso);
        $("#fltDocente").val(anterior.docente);
        $("#fltSeccion").val(anterior.seccion);
        $("#fltGrupo").val(anterior.grupo);
    }
}

// Trae todos los horarios del período para armar los filtros y luego aplica la búsqueda
function cargarBaseDelPeriodo(conservarFiltros) {
    let idPeriodo = $("#fltPeriodo").val();

    if (!idPeriodo) {
        return;
    }

    $.ajax({
        url: '/Horario/GetHorarios',
        type: 'GET',
        data: { idPeriodo: idPeriodo },

        success: function (response) {
            horariosBase = response.esCorrecto ? (response.dato || []) : [];

            if (!conservarFiltros) {
                $("#fltAula").val('');
                $("#fltEstado").val('');
            }

            llenarFiltros(conservarFiltros);
            buscarHorarios();
        },

        error: function () {
            mostrarMensaje("No fue posible consultar los horarios.", 'danger');
        }
    });
}

// Después de registrar, editar o cambiar el estado: se actualiza la base y se mantienen los filtros
function refrescar() {
    cargarBaseDelPeriodo(true);
}

function buscarHorarios() {
    let idPeriodo = $("#fltPeriodo").val();

    if (!idPeriodo) {
        return;
    }

    $.ajax({
        url: '/Horario/GetHorarios',
        type: 'GET',
        data: {
            idPeriodo: idPeriodo,
            idCurso: $("#fltCurso").val(),
            idDocente: $("#fltDocente").val(),
            idSeccion: $("#fltSeccion").val(),
            idGrupo: $("#fltGrupo").val(),
            aula: $("#fltAula").val().trim(),
            estado: $("#fltEstado").val()
        },

        success: function (response) {
            pintarTabla(response);
        },

        error: function () {
            mostrarMensaje("No fue posible consultar los horarios.", 'danger');
        }
    });
}

function limpiarBusqueda() {
    $("#fltCurso").val('');
    $("#fltDocente").val('');
    $("#fltSeccion").val('');
    $("#fltGrupo").val('');
    $("#fltAula").val('');
    $("#fltEstado").val('');

    buscarHorarios();
}

function pintarTabla(response) {
    if (!response.esCorrecto) {
        horariosListados = [];
        $("#contadorHorarios").text('');
        $("#tblHorarios tbody").html(`
            <tr><td colspan="9" class="text-center text-muted">${escapeHtml(response.mensaje)}</td></tr>`);
        return;
    }

    horariosListados = response.dato || [];

    let total = horariosListados.length;
    $("#contadorHorarios").text(total + (total === 1 ? ' horario' : ' horarios'));

    if (total === 0) {
        $("#tblHorarios tbody").html(`
            <tr><td colspan="9" class="text-center text-muted">${escapeHtml(response.mensaje)}</td></tr>`);
        return;
    }

    let filas = '';

    $.each(horariosListados, function (index, h) {
        let botonEstado = h.estado
            ? `<button type="button" class="btn btn-sm btn-outline-secondary btn-estado-horario"
                       data-id="${h.idHorario}" data-activar="false" title="Inactivar">
                   <i class="bi bi-toggle-on"></i>
               </button>`
            : `<button type="button" class="btn btn-sm btn-outline-success btn-estado-horario"
                       data-id="${h.idHorario}" data-activar="true" title="Activar">
                   <i class="bi bi-toggle-off"></i>
               </button>`;

        filas += `
            <tr>
                <td>${escapeHtml(h.dia)}</td>
                <td class="text-nowrap">${escapeHtml(h.horaInicio)} - ${escapeHtml(h.horaFin)}</td>
                <td>${escapeHtml(h.nombreGrado)} - ${escapeHtml(h.nombreSeccion)}</td>
                <td>${escapeHtml(h.nombreGrupo)}</td>
                <td>${escapeHtml(h.codigoCurso)} - ${escapeHtml(h.nombreCurso)}</td>
                <td>${escapeHtml(h.nombreDocente)}</td>
                <td>${escapeHtml(h.aula)}</td>
                <td>${badgeEstado(h.estado)}</td>
                <td class="text-nowrap">
                    <button type="button" class="btn btn-sm btn-outline-secondary btn-detalle-horario"
                            data-id="${h.idHorario}" title="Ver detalle">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-primary btn-editar-horario"
                            data-id="${h.idHorario}" title="Editar">
                        <i class="bi bi-pencil-square"></i>
                    </button>
                    ${botonEstado}
                </td>
            </tr>`;
    });

    $("#tblHorarios tbody").html(filas);
}

// =====================================================
// Detalle (MHOF-01-12)
// =====================================================

function abrirDetalle(idHorario) {
    $.ajax({
        url: '/Horario/GetHorario',
        type: 'GET',
        data: { idHorario: idHorario },

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                refrescar();
                return;
            }

            let h = response.dato;

            $("#dtPeriodo").text(h.nombrePeriodo);
            $("#dtGrado").text(h.nombreGrado);
            $("#dtSeccion").text(h.nombreSeccion);
            $("#dtGrupo").text(h.nombreGrupo);
            $("#dtCurso").text(`${h.codigoCurso} - ${h.nombreCurso}`);
            $("#dtDocente").text(h.nombreDocente);
            $("#dtDia").text(h.dia);
            $("#dtHorario").text(`${h.horaInicio} - ${h.horaFin}`);
            $("#dtAula").text(h.aula);
            $("#dtEstado").html(badgeEstado(h.estado));

            mostrarModal('modalDetalleHorario');
        },

        error: function () {
            mostrarMensaje("No fue posible consultar el horario.", 'danger');
        }
    });
}

// =====================================================
// Editar (MHOF-01-05)
// =====================================================

function abrirEditar(idHorario) {
    $.ajax({
        url: '/Horario/GetHorario',
        type: 'GET',
        data: { idHorario: idHorario },

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'danger');
                refrescar();
                return;
            }

            prepararEdicion(response.dato);
        },

        error: function () {
            mostrarMensaje("No fue posible cargar el horario.", 'danger');
        }
    });
}

function prepararEdicion(h) {
    $("#edIdHorario").val(h.idHorario);
    $("#edPeriodo").text(h.nombrePeriodo);

    // Sección: las activas, más la del horario aunque hoy esté inactiva
    let secciones = '<option value="">Seleccione una sección</option>';

    if (opcionesFormulario) {
        $.each(opcionesFormulario.secciones, function (index, s) {
            secciones += `<option value="${s.idSeccion}">${escapeHtml(s.nombreGrado)} - ${escapeHtml(s.nombre)}</option>`;
        });
    }

    $("#edSeccion").html(secciones);
    asegurarOpcion("#edSeccion", h.idSeccion, `${h.nombreGrado} - ${h.nombreSeccion}`);

    $("#edDia").html(opcionesDias()).val(h.dia);
    $("#edInicio").val(h.horaInicio);
    $("#edFin").val(h.horaFin);
    $("#edAula").val(h.aula);

    reiniciarSelect("#edGrupo", "Cargando...");
    reiniciarSelect("#edCurso", "Cargando...");
    reiniciarSelect("#edDocente", "Cargando...");

    mostrarModal('modalEditarHorario');

    // Las listas se cargan una tras otra y al final se deja elegido lo que tenía el horario
    cargarGrupos('ed', h.idSeccion).always(function () {
        asegurarOpcion("#edGrupo", h.idGrupo, h.nombreGrupo);

        cargarCursos('ed', h.idSeccion).always(function () {
            asegurarOpcion("#edCurso", h.idCurso, `${h.codigoCurso} - ${h.nombreCurso}`);

            cargarDocentes('ed', h.idCurso).always(function () {
                asegurarOpcion("#edDocente", h.idDocente, h.nombreDocente);
            });
        });
    });
}

function guardarEdicion() {
    let error = validarFormulario('ed');

    if (error) {
        mostrarMensaje(error, 'warning');
        return;
    }

    enviarHorario('/Horario/Actualizar', '#formEditarHorario', '#btnGuardarEdicion',
        "No se pudo actualizar el horario. Intente nuevamente.",
        function (response) {
            // 1308 = el horario ya no existe: se cierra el modal y se refresca la lista
            if (response.esCorrecto || response.codigo === 1308) {
                ocultarModal('modalEditarHorario');
                refrescar();
            }
        });
}

// =====================================================
// Activar o inactivar (MHOF-01-06)
// =====================================================

function cambiarEstado(idHorario, activar) {
    let h = horariosListados.find(x => x.idHorario === idHorario);

    if (!h) {
        return;
    }

    let accion = activar ? 'activar' : 'inactivar';

    // Si cancela, no se llama al servidor y el horario queda como estaba
    if (!confirm(`¿Desea ${accion} el horario de ${h.nombreCurso} (${h.dia} ${h.horaInicio} - ${h.horaFin})?`)) {
        return;
    }

    $.ajax({
        url: '/Horario/CambiarEstado',
        type: 'POST',
        data: { idHorario: idHorario, activar: activar },

        success: function (response) {
            if (response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'success');
            }
            else {
                let esConflicto = response.codigo >= 1305 && response.codigo <= 1307;

                mostrarMensaje(response.mensaje, esConflicto ? 'warning' : 'danger');
            }

            // En todos los casos se refresca, así se ve el estado real
            refrescar();
        },

        error: function () {
            mostrarMensaje("No se pudo cambiar el estado del horario. Intente nuevamente.", 'danger');
        }
    });
}