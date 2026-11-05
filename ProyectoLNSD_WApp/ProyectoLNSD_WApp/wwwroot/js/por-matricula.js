$(document).ready(cargarHorarioMatricula);

function avisoPagina(texto) {
    $("#pmContenido").html(`
        <div class="card">
            <div class="card-body text-center text-muted py-4">
                <i class="bi bi-calendar-x fs-2"></i>
                <p class="mb-0 mt-2">${escapeHtml(texto)}</p>
            </div>
        </div>`);
}

function cargarHorarioMatricula() {
    $.ajax({
        url: '/Horario/GetHorarioPorMatricula',
        type: 'GET',

        success: function (response) {
            if (!response.esCorrecto) {
                avisoPagina(response.mensaje);
                return;
            }

            let matriculas = response.dato || [];

            // HU 01-09, esc. 2: sin matrícula vigente
            if (matriculas.length === 0) {
                avisoPagina(response.mensaje);
                return;
            }

            // Una tarjeta por matrícula: el estudiante ve una, el encargado una por cada estudiante
            let html = '';

            $.each(matriculas, function (i, m) {
                html += `
                    <div class="card mb-4">
                        <div class="card-header d-flex justify-content-between align-items-center">
                            <h2>
                                <i class="bi bi-person"></i> ${escapeHtml(m.nombreEstudiante)}
                                <small class="text-muted fs-6">
                                    ${escapeHtml(m.nombreGrado)} - ${escapeHtml(m.nombreSeccion)}
                                    · ${escapeHtml(etiquetaGrupo(m.nombreGrupo))}
                                    · ${escapeHtml(m.nombrePeriodo)}
                                </small>
                            </h2>
                            <span class="badge bg-secondary">${textoContador(m.horarios.length)}</span>
                        </div>
                        <div class="card-body">
                            <div id="pmCuadricula${i}"></div>
                        </div>
                    </div>`;
            });

            $("#pmContenido").html(html);

            $.each(matriculas, function (i, m) {
                // HU 01-09, esc. 3: matrícula vigente, pero todavía sin horarios publicados
                pintarCuadricula(`#pmCuadricula${i}`, m.horarios,
                    "El horario todavía no está disponible", formatoClaseEstudiante);
            });
        },

        error: function () {
            mostrarMensaje("No fue posible consultar el horario.", 'danger');
        }
    });
}