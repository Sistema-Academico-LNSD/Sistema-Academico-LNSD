namespace ProyectoLNSD_WApp.DAL.Repositories.Beca
{
    public static class EstadoVisibleBeca
    {
        public const string Activa = "Activa";
        public const string Vencida = "Vencida";
        public const string Pendiente = "Pendiente";
        public const string Inactiva = "Inactiva";

        public static readonly string[] Todos = { Activa, Vencida, Pendiente, Inactiva };
    }

    public class BecaFiltro
    {
        public int? IdPeriodo { get; set; }

        public int? IdEstudiante { get; set; }

        public int? IdGrado { get; set; }

        public string? Texto { get; set; }

        public string? Estado { get; set; }

        public DateOnly Hoy { get; set; }

        public int? IdUsuarioEstudiante { get; set; }

        public int? IdUsuarioEncargado { get; set; }
    }

    public class BecaListado
    {
        public int IdBeca { get; set; }

        public int IdEstudiante { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Identificacion { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        public bool EstudianteActivo { get; set; }

        public int IdPeriodo { get; set; }

        public string NombrePeriodo { get; set; } = string.Empty;

        public string? NombreGrado { get; set; }

        public string? NombreSeccion { get; set; }

        public string? EstadoMatricula { get; set; }

        public decimal Monto { get; set; }

        public DateOnly FechaInicio { get; set; }

        public DateOnly FechaFin { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string? MotivoDesactivacion { get; set; }

        public List<int> IdsRequisitosCumplidos { get; set; } = new();
    }
}
