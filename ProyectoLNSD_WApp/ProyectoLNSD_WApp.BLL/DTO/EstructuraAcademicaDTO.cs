using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class EstructuraAcademicaDTO
    {
        public int IdGrado { get; set; }
        public int? IdSeccion { get; set; }
        public string CodigoGrado { get; set; } = string.Empty;
        public string NombreGrado { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public string? NombreSeccion { get; set; }
        public int? CapacidadMaxima { get; set; }
        public int? CuposDisponibles { get; set; }
        public bool Estado { get; set; }
        public bool EstadoGrado { get; set; }
    }
}
