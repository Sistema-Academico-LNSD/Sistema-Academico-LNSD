namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class AsistenciaFiltroDTO
    {
        public int? IdPeriodo { get; set; }

        public int? IdGrado { get; set; }

        public int? IdSeccion { get; set; }

        public int? IdGrupo { get; set; }

        public int? IdCurso { get; set; }

        public string? Texto { get; set; }

        public DateOnly? Desde { get; set; }

        public DateOnly? Hasta { get; set; }

        public string? Estado { get; set; }

        public decimal? Umbral { get; set; }

        public bool SoloBajoUmbral { get; set; }
    }
}
