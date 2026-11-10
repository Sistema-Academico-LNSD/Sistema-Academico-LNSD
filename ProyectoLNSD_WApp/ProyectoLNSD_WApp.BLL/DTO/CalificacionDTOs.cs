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

    public class PlanillaEstudianteDTO
    {
        public int IdMatricula { get; set; }
        public string Carnet { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<NotaCeldaDTO> Notas { get; set; } = new();
        public List<RubroResumenDTO> Resumen { get; set; } = new();

        public decimal Acumulado { get; set; }
        public decimal Evaluado { get; set; }

        /// Promedio del semestre sobre lo ya calificado (0-100); null si aún no hay notas.
        public decimal? Promedio { get; set; }
    }

    public class PlanillaDTO
    {
        public int IdPeriodo { get; set; }
        public int IdCurso { get; set; }
        public int IdSeccion { get; set; }
        public int IdGrupo { get; set; }

        // Con el período cerrado la planilla es solo de consulta
        public bool PeriodoActivo { get; set; }

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
}
