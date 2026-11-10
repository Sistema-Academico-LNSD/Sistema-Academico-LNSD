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

    public class PlanillaEstudianteDTO
    {
        public int IdMatricula { get; set; }
        public string Carnet { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<NotaCeldaDTO> Notas { get; set; } = new();
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
}
