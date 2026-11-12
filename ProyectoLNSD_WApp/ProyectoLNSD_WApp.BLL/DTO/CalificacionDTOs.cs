using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class PeriodoCalificacionDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    /// Un curso con su sección y grupo que el docente tiene asignado (MCNF-01-02).
    public class ClaseDocenteDTO
    {
        public int IdPeriodo { get; set; }
        public int IdCurso { get; set; }
        public int IdSeccion { get; set; }
        public int IdGrupo { get; set; }
        public string Curso { get; set; } = string.Empty;
        public string Grado { get; set; } = string.Empty;
        public string Seccion { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    /// Práctica, examen o tarea de un rubro.
    public class ActividadDTO
    {
        public int IdActividad { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Puntos { get; set; }

        /// Lo que pesa en la nota final del semestre; se calcula solo (porcentaje del rubro / cantidad de actividades).
        public decimal Porcentaje { get; set; }

        public DateTime? Fecha { get; set; }
    }

    /// Rubro (cotidiano, pruebas, tareas) con las actividades que el docente creó en él.
    public class RubroPlanillaDTO
    {
        public int IdRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public decimal? PuntosMinimo { get; set; }
        public decimal? PuntosMaximo { get; set; }

        /// Cantidad fija de actividades por período (pruebas: 2); null = la define el docente.
        public int? CantidadFija { get; set; }

        /// Lo que aporta un período (semestre) del porcentaje anual del rubro.
        public decimal PorcentajePeriodo { get; set; }

        /// Rubro donde el docente reparte el porcentaje anual (pruebas), con lo que queda por repartir en el año
        /// y cuántas actividades faltan por crear en todo el año.
        public bool PesoManual { get; set; }
        public decimal PesoRestante { get; set; }
        public int ActividadesFaltantesAnio { get; set; }

        public List<ActividadDTO> Actividades { get; set; } = new();
    }

    /// Nota ya registrada de un estudiante en una actividad (solo se envían las que existen).
    public class NotaCeldaDTO
    {
        public int IdActividad { get; set; }
        public int IdNota { get; set; }
        public decimal Puntos { get; set; }
        public string? Observacion { get; set; }
    }

    /// Lo que lleva un estudiante en un rubro: Acumulado = puntos de la nota final ganados;
    /// Evaluado = puntos de la nota final que ya se han calificado.
    public class RubroResumenDTO
    {
        public int IdRubro { get; set; }
        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }
    }

    /// Asistencia del estudiante en el período: Porcentaje = clases sin ausencia / clases registradas.
    /// Acumulado y Evaluado van en puntos de la nota anual (la asistencia vale 5%, 2.5% por semestre).
    public class AsistenciaCalificacionDTO
    {
        public int Registros { get; set; }
        public int Ausencias { get; set; }
        public decimal? Porcentaje { get; set; }
        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }
    }

    public class ConfiguracionCalificacionDTO
    {
        public decimal NotaMinima { get; set; }
        public bool ConsideraAsistencia { get; set; }
        public decimal PorcentajeAsistencia { get; set; }
    }

    /// Lo que lleva el estudiante en un curso durante UN período (semestre).
    public class ResultadoPeriodoDTO
    {
        public int IdPeriodo { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public List<RubroResumenDTO> Rubros { get; set; } = new();
        public AsistenciaCalificacionDTO Asistencia { get; set; } = new();
        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }
        public decimal? Promedio { get; set; }
    }

    /// Resultado del estudiante en un curso durante el año (suma de sus semestres).
    /// Estado: Sin notas, Aprobando, Reprobando (año en curso), Aprobado o Reprobado (año completo y cerrado).
    public class ResultadoCursoDTO
    {
        public int IdCurso { get; set; }
        public string Curso { get; set; } = string.Empty;
        public decimal NotaMinima { get; set; }
        public List<ResultadoPeriodoDTO> Periodos { get; set; } = new();
        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }
        public decimal? NotaAnual { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool Definitivo { get; set; }
    }

    /// Promoción del año: aprueba el año el estudiante que aprueba todos sus cursos.
    /// Estado: Promovido, No promovido, En curso o Sin cursos.
    public class PromocionDTO
    {
        public int IdEstudiante { get; set; }
        public int Anio { get; set; }
        public decimal NotaMinima { get; set; }
        public List<ResultadoCursoDTO> Cursos { get; set; } = new();
        public int Aprobados { get; set; }
        public int Reprobados { get; set; }
        public int EnCurso { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class PlanillaEstudianteDTO
    {
        public int IdMatricula { get; set; }
        public int IdEstudiante { get; set; }
        public string Carnet { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<NotaCeldaDTO> Notas { get; set; } = new();
        public List<RubroResumenDTO> Resumen { get; set; } = new();

        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }

        /// Promedio del semestre sobre lo ya calificado, con la asistencia (0-100); null si aún no hay notas.
        public decimal? Promedio { get; set; }

        public AsistenciaCalificacionDTO Asistencia { get; set; } = new();

        /// Nota del año (suma de los semestres) y resultado del curso según la nota mínima.
        public decimal? NotaAnual { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public bool ResultadoDefinitivo { get; set; }
    }

    public class PlanillaDTO
    {
        public int IdPeriodo { get; set; }
        public int IdCurso { get; set; }
        public int IdSeccion { get; set; }
        public int IdGrupo { get; set; }

        // Con el período cerrado la planilla es solo de consulta
        public bool PeriodoActivo { get; set; }

        public decimal NotaMinima { get; set; }
        public bool ConsideraAsistencia { get; set; }
        public decimal PorcentajeAsistencia { get; set; }

        public List<RubroPlanillaDTO> Rubros { get; set; } = new();
        public List<PlanillaEstudianteDTO> Estudiantes { get; set; } = new();
    }

    public class ActividadCrearDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el período lectivo")]
        public int IdPeriodo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el curso")]
        public int IdCurso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la sección")]
        public int IdSeccion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el grupo")]
        public int IdGrupo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el rubro")]
        public int IdRubro { get; set; }

        [Required(ErrorMessage = "Debe indicar el nombre de la actividad")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Range(0.01d, 100d, ErrorMessage = "El valor de la actividad debe estar entre 0.01 y 100 puntos")]
        public decimal Puntos { get; set; }

        /// Solo en rubros de peso manual (pruebas): lo que pesa en la nota anual.
        /// En la última actividad del año el servidor usa lo que queda del rubro.
        [Range(0.01d, 100d, ErrorMessage = "El porcentaje de la actividad debe estar entre 0.01 y 100")]
        public decimal? Porcentaje { get; set; }

        public DateTime? Fecha { get; set; }
    }

    public class NotaGuardarDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la matrícula del estudiante")]
        public int IdMatricula { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la actividad")]
        public int IdActividad { get; set; }

        [Range(0d, 100d, ErrorMessage = "La nota debe estar entre 0 y el valor de la actividad")]
        public decimal PuntosObtenidos { get; set; }

        [StringLength(500, ErrorMessage = "La observación no puede superar 500 caracteres")]
        public string? Observacion { get; set; }
    }

    /// Resultado de cada nota enviada en un guardado en lote.
    public class NotaLoteResultadoDTO
    {
        public int IdMatricula { get; set; }
        public int IdActividad { get; set; }
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public NotaCeldaDTO? Nota { get; set; }
    }

    /// Cambiar el valor de una nota y/o agregar una observación (MCNF-01-06 y 01-07).
    public class NotaEditarDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la nota")]
        public int IdNota { get; set; }

        [Range(0d, 100d, ErrorMessage = "La nota debe estar entre 0 y el valor de la actividad")]
        public decimal PuntosObtenidos { get; set; }

        /// Justifica el cambio; es obligatoria cuando cambia el valor de la nota.
        [StringLength(500, ErrorMessage = "La observación no puede superar 500 caracteres")]
        public string? Observacion { get; set; }
    }

    /// Un registro del historial de notas de un estudiante en un curso (MCNF-01-08).
    public class HistorialNotaDTO
    {
        public DateTime Fecha { get; set; }
        public string Rubro { get; set; } = string.Empty;
        public string Actividad { get; set; } = string.Empty;
        public decimal PuntosActividad { get; set; }

        /// Registro, Edicion o Correccion.
        public string Tipo { get; set; } = string.Empty;

        public decimal? ValorAnterior { get; set; }
        public decimal ValorNuevo { get; set; }
        public string? Observacion { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    /// Una nota de un curso con el estado de su autorización de corrección (MCNF-01-13).
    public class CorreccionNotaDTO
    {
        public int IdNota { get; set; }
        public string Carnet { get; set; } = string.Empty;
        public string Estudiante { get; set; } = string.Empty;
        public string Rubro { get; set; } = string.Empty;
        public string Actividad { get; set; } = string.Empty;
        public decimal Puntos { get; set; }
        public decimal PuntosActividad { get; set; }

        /// "Sin autorización", "Autorizada" (pendiente de usar) o "Usada".
        public string Estado { get; set; } = string.Empty;

        public string? Motivo { get; set; }
        public bool Autorizable { get; set; }
    }

    public class CorreccionAutorizarDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la nota")]
        public int IdNota { get; set; }

        [Required(ErrorMessage = "Debe indicar el motivo de la corrección")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar 500 caracteres")]
        public string Motivo { get; set; } = string.Empty;
    }

    // ---------- Consulta de calificaciones (MCNF-01-09, 01-10, 01-11 y 01-15) ----------

    /// Una actividad con la nota del estudiante. PuntosObtenidos es null mientras no se haya calificado.
    /// Porcentaje = puntos obtenidos / valor de la actividad x 100; Peso = lo que vale en la nota anual;
    /// Aporte = lo que esa nota suma a la nota anual.
    public class DetalleActividadDTO
    {
        public string Actividad { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public decimal Puntos { get; set; }
        public decimal? PuntosObtenidos { get; set; }
        public decimal? Porcentaje { get; set; }
        public decimal Peso { get; set; }
        public decimal? Aporte { get; set; }

        /// Solo se envía a quien puede verla (docente, administrador y director).
        public string? Observacion { get; set; }
    }

    public class DetalleRubroDTO
    {
        public int IdRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;

        /// Porcentaje del rubro en la nota anual.
        public decimal Porcentaje { get; set; }

        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }
        public List<DetalleActividadDTO> Actividades { get; set; } = new();
    }

    public class DetallePeriodoDTO
    {
        public int IdPeriodo { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public List<DetalleRubroDTO> Rubros { get; set; } = new();
        public AsistenciaCalificacionDTO Asistencia { get; set; } = new();
        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }
        public decimal? Promedio { get; set; }
    }

    public class DetalleCursoDTO
    {
        public int IdCurso { get; set; }
        public string Curso { get; set; } = string.Empty;
        public decimal NotaMinima { get; set; }
        public List<DetallePeriodoDTO> Periodos { get; set; } = new();
        public decimal? NotaAnual { get; set; }

        /// Sin notas, Aprobando, Reprobando, Aprobado o Reprobado.
        public string Estado { get; set; } = string.Empty;

        public bool Definitivo { get; set; }
    }

    public class DetalleEstudianteDTO
    {
        public int IdEstudiante { get; set; }
        public string Carnet { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Grado { get; set; }
        public int Anio { get; set; }
        public decimal NotaMinima { get; set; }
        public bool IncluyeObservaciones { get; set; }
        public List<DetalleCursoDTO> Cursos { get; set; } = new();

        /// Promoción del año (vacía cuando solo se consulta un curso).
        public int Aprobados { get; set; }
        public int Reprobados { get; set; }
        public int EnCurso { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    /// Un estudiante en un curso, para la consulta del administrador y director.
    public class ConsultaFilaDTO
    {
        public int IdEstudiante { get; set; }
        public string Carnet { get; set; } = string.Empty;
        public string Estudiante { get; set; } = string.Empty;
        public int IdCurso { get; set; }
        public string Curso { get; set; } = string.Empty;
        public string Grado { get; set; } = string.Empty;
        public string Seccion { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public decimal? Promedio { get; set; }
        public decimal? NotaAnual { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class EstudianteResumenDTO
    {
        public int IdEstudiante { get; set; }
        public string Carnet { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Grado { get; set; }
    }
}
