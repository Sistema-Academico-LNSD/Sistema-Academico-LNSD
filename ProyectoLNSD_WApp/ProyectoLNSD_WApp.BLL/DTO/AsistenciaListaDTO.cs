namespace ProyectoLNSD_WApp.BLL.DTO
{
    /// Lista de una clase en una fecha, lista para pasar asistencia (MCAF-01-01 y 01-02)
    public class AsistenciaListaDTO
    {
        public HorarioDTO Clase { get; set; } = new();

        public DateOnly Fecha { get; set; }

        /// true si la lista de esa fecha ya se había guardado (se está editando)
        public bool YaRegistrada { get; set; }

        /// false cuando el período está cerrado o el usuario no tiene permiso de editar lo ya guardado
        public bool PuedeGuardar { get; set; }

        /// Motivo por el que no se puede guardar (se muestra en la pantalla)
        public string? Aviso { get; set; }

        public List<AsistenciaFilaDTO> Estudiantes { get; set; } = new();
    }

    /// Un estudiante de la lista, con lo que ya tenga registrado
    public class AsistenciaFilaDTO
    {
        public int IdMatricula { get; set; }

        public string NombreEstudiante { get; set; } = string.Empty;

        public string Carnet { get; set; } = string.Empty;

        /// null = todavía sin registrar
        public string? Estado { get; set; }

        public string? Observacion { get; set; }
    }
}
