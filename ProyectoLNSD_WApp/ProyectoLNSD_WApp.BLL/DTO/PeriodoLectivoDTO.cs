using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    public class PeriodoLectivoDTO
    {
        public int IdPeriodo { get; set; }

        [Required(ErrorMessage = "El nombre del período es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateOnly? FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es requerida")]
        public DateOnly? FechaFin { get; set; }

        public bool Activo { get; set; }
    }
}
