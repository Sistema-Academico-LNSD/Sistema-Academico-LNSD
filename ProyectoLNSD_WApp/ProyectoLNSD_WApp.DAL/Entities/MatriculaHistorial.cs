namespace ProyectoLNSD_WApp.DAL.Entities
{
    public class MatriculaHistorial
    {
        public int IdHistorial { get; set; }
        public int IdMatricula { get; set; }
        public int? IdUsuario { get; set; }
        public string TipoCambio { get; set; } = string.Empty;
        public string? EstadoAnterior { get; set; }
        public string? EstadoNuevo { get; set; }
        public int? GrupoAnterior { get; set; }
        public int? GrupoNuevo { get; set; }
        public int? SeccionAnterior { get; set; }
        public int? SeccionNueva { get; set; }
        public string? Detalle { get; set; }
        public DateTime FechaCambio { get; set; }

        public virtual Matricula? Matricula { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public virtual Grupo? GrupoAnteriorNavigation { get; set; }
        public virtual Grupo? GrupoNuevoNavigation { get; set; }
        public virtual Seccion? SeccionAnteriorNavigation { get; set; }
        public virtual Seccion? SeccionNuevaNavigation { get; set; }
    }
}
