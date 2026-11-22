using System.ComponentModel.DataAnnotations;

namespace ProyectoLNSD_WApp.BLL.DTO
{
    // =====================  Constantes  =====================

    /// Tipos de campo que puede tener una plantilla. El navegador dibuja cada tipo; el servidor valida cada tipo.
    public static class TiposCampoPauta
    {
        public const string Texto = "texto";
        public const string TextoLargo = "textoLargo";
        public const string Fecha = "fecha";
        public const string Hora = "hora";
        public const string Numero = "numero";
        public const string Correo = "correo";
        public const string Telefono = "telefono";
        public const string SiNo = "siNo";
        public const string Casilla = "casilla";
        public const string Seleccion = "seleccion";     // una opción
        public const string Opciones = "opciones";       // varias casillas
        public const string Tabla = "tabla";             // filas que se agregan
        public const string TextoFijo = "textoFijo";     // texto del formulario que no se llena
        public const string Firma = "firma";             // línea de firma (queda en blanco para firmar a mano)
        public const string Sello = "sello";             // espacio para el sello institucional
    }

    public static class EstadosFormularioPauta
    {
        public const string Borrador = "Borrador";               // creado, aún sin datos guardados
        public const string EnElaboracion = "EnElaboracion";     // con datos guardados
        public const string Finalizado = "Finalizado";           // bloqueado
    }

    public static class EstadosExpedientePauta
    {
        public const string Abierto = "Abierto";
        public const string Cerrado = "Cerrado";
    }

    public static class RolesEstudiantePauta
    {
        public const string Recibe = "Recibe";
        public const string Ejerce = "Ejerce";
        public const string Observador = "Observador";
        public const string Identificada = "Identificada";

        public static readonly string[] Todos = { Recibe, Ejerce, Observador, Identificada };
    }

    public static class AccionesBitacoraPauta
    {
        public const string Ver = "Ver";
        public const string Crear = "Crear";
        public const string Editar = "Editar";
        public const string Finalizar = "Finalizar";
        public const string Reabrir = "Reabrir";
        public const string Eliminar = "Eliminar";
        public const string Descargar = "Descargar";
        public const string Imprimir = "Imprimir";
        public const string Cerrar = "Cerrar";
    }

    // =====================  Plantillas  =====================

    public class OpcionPautaDTO
    {
        public string Valor { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
    }

    public class CampoPlantillaDTO
    {
        public string Clave { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public string Tipo { get; set; } = TiposCampoPauta.Texto;
        public bool Requerido { get; set; }
        public int? MaxLongitud { get; set; }
        public string? Ayuda { get; set; }

        /// Condición para mostrar el campo: "clave=valor", "clave!=valor" o "clave=" (vacío). Varias se unen con ";" (Y).
        public string? VisibleSi { get; set; }

        /// De dónde se precarga el valor al crear el formulario (ver PrecargaPauta).
        public string? Origen { get; set; }

        public List<OpcionPautaDTO>? Opciones { get; set; }

        // Solo para Tabla
        public List<CampoPlantillaDTO>? Columnas { get; set; }
        public int? MinFilas { get; set; }
        public int? MaxFilas { get; set; }
        public List<string>? FilasIniciales { get; set; }   // valores de la primera columna que se crean ya escritos
    }

    public class SeccionPlantillaDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? VisibleSi { get; set; }

        /// Cómo se dibuja en el documento: "normal", "cuadro" (caja con borde) o "fila" (los campos como una sola fila de tabla).
        public string Presentacion { get; set; } = "normal";

        public List<CampoPlantillaDTO> Campos { get; set; } = new();
    }

    public class PlantillaPautaDTO
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Repetible { get; set; }
        public bool Confidencial { get; set; } = true;
        public List<SeccionPlantillaDTO> Secciones { get; set; } = new();
    }

    public class PlantillaResumenDTO
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Repetible { get; set; }
        public List<int> Pautas { get; set; } = new();
    }

    public class PautaCatalogoDTO
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int FormularioInicial { get; set; }
        public List<int> Requeridos { get; set; } = new();
        public List<int> Sugeridos { get; set; } = new();
        public List<string> RolesEstudiante { get; set; } = new();
    }

    // =====================  Expedientes y formularios  =====================

    public class EstudianteInvolucradoDTO
    {
        public int? IdEstudiante { get; set; }

        [Required(ErrorMessage = "El nombre de la persona estudiante es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "La sección no puede superar los 50 caracteres")]
        public string? Seccion { get; set; }

        [Required(ErrorMessage = "El rol de la persona estudiante es requerido")]
        public string Rol { get; set; } = string.Empty;

        public int IdPautaEstudiante { get; set; }
    }

    public class ExpedienteCrearDTO
    {
        [Range(1, 9, ErrorMessage = "La pauta seleccionada no es válida")]
        public int NumeroPauta { get; set; }

        public int? IdExpedienteOrigen { get; set; }

        public List<EstudianteInvolucradoDTO> Estudiantes { get; set; } = new();
    }

    public class FiltroExpedientesDTO
    {
        public string? Estado { get; set; }
        public int? NumeroPauta { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? TextoEstudiante { get; set; }
        public int? IdEstudiante { get; set; }
    }

    public class ExpedienteResumenDTO
    {
        public int IdExpediente { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int NumeroPauta { get; set; }
        public string NombrePauta { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateOnly FechaActivacion { get; set; }
        public string Responsable { get; set; } = string.Empty;
        public List<string> Estudiantes { get; set; } = new();
        public int TotalFormularios { get; set; }
        public int FormulariosFinalizados { get; set; }
    }

    public class FormularioResumenDTO
    {
        public int IdFormulario { get; set; }
        public int NumeroFormulario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Secuencia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool Requerido { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    public class FormularioFaltanteDTO
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Requerido { get; set; }
    }

    public class ExpedienteDetalleDTO : ExpedienteResumenDTO
    {
        public int? IdExpedienteOrigen { get; set; }
        public List<EstudianteInvolucradoDTO> EstudiantesInvolucrados { get; set; } = new();
        public List<FormularioResumenDTO> Formularios { get; set; } = new();

        /// Formularios de la pauta que todavía no se han creado (requeridos y sugeridos).
        public List<FormularioFaltanteDTO> FormulariosFaltantes { get; set; } = new();

        public DateTime? FechaCierre { get; set; }
        public string? NombreCierre { get; set; }
        public string? VistoBuenoDirector { get; set; }
        public bool DevolucionRealizada { get; set; }
        public string? ObservacionCierre { get; set; }

        public bool PuedeEditar { get; set; }

        /// Motivos por los que todavía no se puede cerrar. Vacío = se puede cerrar.
        public List<string> PendientesCierre { get; set; } = new();
    }

    public class AgregarFormularioDTO
    {
        public int IdExpediente { get; set; }

        [Range(1, 12, ErrorMessage = "El formulario seleccionado no es válido")]
        public int NumeroFormulario { get; set; }

        /// Para los formularios de una sola persona estudiante (F3, F6, F7, F8, F9, F10): precarga sus datos.
        public int? IdPautaEstudiante { get; set; }

        /// Obligatoria cuando el formulario no es de los que indica la pauta.
        [StringLength(300, ErrorMessage = "La justificación no puede superar los 300 caracteres")]
        public string? Justificacion { get; set; }
    }

    public class FormularioDetalleDTO
    {
        public int IdFormulario { get; set; }
        public int IdExpediente { get; set; }
        public string CodigoExpediente { get; set; } = string.Empty;
        public int NumeroFormulario { get; set; }
        public int Secuencia { get; set; }
        public string Estado { get; set; } = string.Empty;
        public PlantillaPautaDTO Plantilla { get; set; } = new();

        /// Valores guardados, en JSON: { "clave": valor, ... }
        public string Datos { get; set; } = "{}";

        public bool PuedeEditar { get; set; }
        public bool PuedeReabrir { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    public class FormularioGuardarDTO
    {
        public int IdFormulario { get; set; }

        /// JSON con los valores. Las claves que no existan en la plantilla se descartan.
        public string Datos { get; set; } = "{}";

        /// false = guardar borrador; true = validar todo y finalizar.
        public bool Finalizar { get; set; }
    }

    public class CierreExpedienteDTO
    {
        public int IdExpediente { get; set; }
        public DateTime? FechaCierre { get; set; }

        [StringLength(150, ErrorMessage = "El nombre del encargado no puede superar los 150 caracteres")]
        public string? NombreCierre { get; set; }

        [StringLength(150, ErrorMessage = "El visto bueno no puede superar los 150 caracteres")]
        public string? VistoBuenoDirector { get; set; }

        public bool DevolucionRealizada { get; set; }

        [StringLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres")]
        public string? Observacion { get; set; }
    }

    public class EstudiantePautaBusquedaDTO
    {
        public int IdEstudiante { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string? Nivel { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public int? Edad { get; set; }
    }

    public class BitacoraPautaDTO
    {
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public string CodigoExpediente { get; set; } = string.Empty;
        public int? IdFormulario { get; set; }
    }
}
