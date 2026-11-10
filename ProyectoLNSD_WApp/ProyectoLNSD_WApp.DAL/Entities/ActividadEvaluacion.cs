namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Práctica, examen o tarea que el docente crea en su curso, sección y grupo.
    public class ActividadEvaluacion
    {
        public int IdActividad { get; set; }
        public int IdRubro { get; set; }
        public int IdPeriodo { get; set; }
        public int IdCurso { get; set; }
        public int IdSeccion { get; set; }
        public int IdGrupo { get; set; }
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;

        /// Lo que vale la actividad: la nota del estudiante va de 0 a este valor.
        public decimal Puntos { get; set; }

        /// Lo que pesa en la nota anual; solo se guarda en rubros de peso manual (pruebas), en los demás es null.
        public decimal? Porcentaje { get; set; }

        public DateTime? Fecha { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        public virtual RubroEvaluacion? Rubro { get; set; }
        public virtual Curso? Curso { get; set; }
    }
}
