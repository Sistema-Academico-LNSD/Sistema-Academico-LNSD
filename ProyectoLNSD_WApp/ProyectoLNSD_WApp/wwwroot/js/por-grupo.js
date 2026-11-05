let horariosPeriodo = [];

$(document).ready(function () {
    cargarPeriodosConsulta("#pgPeriodo", cargarProgramacion);

    $("#pgPeriodo").on("change", cargarProgramacion);

    $("#pgGrado").on("change", function () {
        llenarSecciones();
        consultarPorGrupo();
    });

    $("#pgSeccion, #pgGrupo").on("change", consultarPorGrupo);

    $("#btnLimpiarPg").on("click", function () {
        $("#pgGrado").val('');
        llenarSecciones();
        $("#pgGrupo").val('');
        consultarPorGrupo();
    });
});

function avisoVacio(texto) {
    return `
        <div class="card">
            <div class="card-body text-center text-muted py-4">
                <i class="bi bi-calendar-x fs-2"></i>
                <p class="mb-0 mt-2">${escapeHtml(texto)}</p>
            </div>
        </div>`;
}

// Valores distintos de una lista, ordenados por texto
function valoresUnicos(lista, obtenerId, obtenerTexto) {
    let vistos = {};

    $.each(lista, function (index, h) {
        vistos[obtenerId(h)] = obtenerTexto(h);
    });

    return Object.keys(vistos)
        .map(id => ({ id: id, nombre: vistos[id] }))
        .sort((a, b) => a.nombre.localeCompare(b.nombre, 'es'));
}

function llenarLista(selector, lista, textoPrimera) {
    let opciones = `<option value="">${escapeHtml(textoPrimera)}</option>`;

    $.each(lista, function (index, x) {
        opciones += `<option value="${x.id}">${escapeHtml(x.nombre)}</option>`;
    });

    $(selector).html(opciones);
}

// ---------- Datos del período ----------

// Se traen los horarios vigentes del período y de ahí salen las listas y las cuadrículas
function cargarProgramacion() {
    $.ajax({
        url: '/Horario/GetHorarios',
        type: 'GET',
        data: { idPeriodo: $("#pgPeriodo").val(), estado: true },

        success: function (response) {
            if (!response.esCorrecto) {
                mostrarMensaje(response.mensaje, 'warning');
            }

            horariosPeriodo = response.esCorrecto ? (response.dato || []) : [];

            llenarLista("#pgGrado",
                valoresUnicos(horariosPeriodo, h => h.idGrado, h => h.nombreGrado),
                "Todos los grados");

            llenarSecciones();

            llenarLista("#pgGrupo",
                valoresUnicos(horariosPeriodo, h => h.idGrupo, h => h.nombreGrupo),
                "Todos los grupos");

            consultarPorGrupo();
        },

        error: function () {
            mostrarMensaje("No fue posible consultar los horarios.", 'danger');
        }
    });
}

// Con un grado elegido, solo se ofrecen sus secciones
function llenarSecciones() {
    let idGrado = $("#pgGrado").val();

    let base = idGrado
        ? horariosPeriodo.filter(h => String(h.idGrado) === idGrado)
        : horariosPeriodo;

    llenarLista("#pgSeccion",
        valoresUnicos(base, h => h.idSeccion, h => `${h.nombreGrado} - ${h.nombreSeccion}`),
        "Todas las secciones");
}

// ---------- Consulta (HU 01-08) ----------

function consultarPorGrupo() {
    let idGrado = $("#pgGrado").val();
    let idSeccion = $("#pgSeccion").val();
    let idGrupo = $("#pgGrupo").val();

    // Sin ningún criterio, mostrar todas las cuadrículas haría una página enorme
    if (!idGrado && !idSeccion && !idGrupo) {
        $("#pgContador").text('');
        $("#pgResultado").html(avisoVacio("Seleccione un grado, una sección o un grupo para ver la distribución semanal de clases"));
        return;
    }

    // Los criterios se combinan: solo quedan las clases que cumplen todos
    let clases = horariosPeriodo.filter(h =>
        (!idGrado || String(h.idGrado) === idGrado) &&
        (!idSeccion || String(h.idSeccion) === idSeccion) &&
        (!idGrupo || String(h.idGrupo) === idGrupo));

    $("#pgContador").text(textoContador(clases.length));

    // HU 01-08, esc. 2
    if (clases.length === 0) {
        $("#pgResultado").html(avisoVacio("No hay programación disponible para los criterios seleccionados"));
        return;
    }

    // Una cuadrícula por cada sección y grupo (HU 01-08, esc. 3)
    let conjuntos = {};

    $.each(clases, function (index, h) {
        let clave = `${h.idSeccion}|${h.idGrupo}`;

        if (!conjuntos[clave]) {
            conjuntos[clave] = {
                titulo: `${h.nombreGrado} - ${h.nombreSeccion}`,
                grupo: h.nombreGrupo,
                clases: []
            };
        }

        conjuntos[clave].clases.push(h);
    });

    let ordenados = Object.values(conjuntos)
        .sort((a, b) => (a.titulo + a.grupo).localeCompare(b.titulo + b.grupo, 'es'));

    let html = '';

    $.each(ordenados, function (i, c) {
        html += `
            <div class="card mb-4">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h2>
                        <i class="bi bi-diagram-3"></i> ${escapeHtml(c.titulo)}
                        <small class="text-muted fs-6">${escapeHtml(etiquetaGrupo(c.grupo))}</small>
                    </h2>
                    <span class="badge bg-secondary">${textoContador(c.clases.length)}</span>
                </div>
                <div class="card-body">
                    <div id="pgCuadricula${i}"></div>
                </div>
            </div>`;
    });

    $("#pgResultado").html(html);

    $.each(ordenados, function (i, c) {
        pintarCuadricula(`#pgCuadricula${i}`, c.clases, '', formatoClaseEstudiante);
    });
}