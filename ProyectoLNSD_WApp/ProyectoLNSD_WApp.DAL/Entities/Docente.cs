namespace ProyectoLNSD_WApp.DAL.Entities
{
    public partial class Docente
    {
        public int IdDocente { get; set; }
        public int? IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public bool Estado { get; set; } = true;
        public string? Titulos { get; set; }
        public string? Especialidad { get; set; }
        public int? AniosExperiencia { get; set; }
        public int? IdArea { get; set; }
        public DateTime FechaRegistro { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public virtual AreaAcademica? Area { get; set; }
        //public virtual ICollection<Horario> Horarios { get; set; }
        //    = new List<Horario>();
        public virtual ICollection<EstudianteDocente> EstudiantesDocentes { get; set; }
            = new List<EstudianteDocente>();
    }
}