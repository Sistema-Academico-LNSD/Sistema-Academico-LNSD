namespace ProyectoLNSD_WApp.DAL.Entities
{
    /// Requisito de elegibilidad para beca socioeconómica (MBEF-01-01).
    /// No se borra: se desactiva, para conservar lo que cumplió cada beca.
    public class BecaRequisito
    {
        public int IdRequisito { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        /// true = se exige para activar una beca
        public bool Obligatorio { get; set; } = true;

        /// null = lo marca el administrador; uno de VerificacionRequisito = lo comprueba el sistema
        public string? Verificacion { get; set; }

        public bool Activo { get; set; } = true;
    }

    /// Requisitos que el sistema comprueba solo (CK_BecaRequisito_Verificacion)
    public static class VerificacionRequisito
    {
        /// El estudiante tiene matrícula "Activa" en el período de la beca
        public const string MatriculaActiva = "MatriculaActiva";

        public static readonly string[] Todas = { MatriculaActiva };
    }

    /// Requisito que el administrador marcó como cumplido en una beca (MBEF-01-03)
    public class BecaRequisitoCumplido
    {
        public int IdBeca { get; set; }

        public int IdRequisito { get; set; }

        public virtual Beca? Beca { get; set; }

        public virtual BecaRequisito? Requisito { get; set; }
    }
}
