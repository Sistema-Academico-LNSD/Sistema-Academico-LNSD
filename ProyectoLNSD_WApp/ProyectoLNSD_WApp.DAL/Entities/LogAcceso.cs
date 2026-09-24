using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoLNSD_WApp.DAL.Entities
{
    [Table("Log_Acceso")]
    public class LogAcceso
    {
        [Key]
        [Column("id_log")]
        public int IdLog { get; set; }

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }

        [Required]
        [StringLength(150)]
        [Column("correo")]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("tipo_evento")]
        public string TipoEvento { get; set; } = string.Empty;

        [Required]
        [Column("exitoso")]
        public bool Exitoso { get; set; }

        [StringLength(250)]
        [Column("mensaje")]
        public string? Mensaje { get; set; }

        [Required]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        // Navegación
        [ForeignKey(nameof(IdUsuario))]
        public virtual Usuario? Usuario { get; set; }
    }
}