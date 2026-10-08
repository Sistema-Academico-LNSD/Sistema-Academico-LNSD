namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class ExpedienteDocenteDTO
    {
        public DocenteDTO Docente { get; set; } = new();
        public string? CorreoCuenta { get; set; }
        public CursosDocenteDTO Cursos { get; set; } = new();
    }
}