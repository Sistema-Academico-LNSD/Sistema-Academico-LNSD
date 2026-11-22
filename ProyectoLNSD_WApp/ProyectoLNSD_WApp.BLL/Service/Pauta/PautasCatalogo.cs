using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Pauta
{
    /// Las 9 pautas de actuación del MEP y los formularios que usa cada una.
    /// "Requeridos" = sin ellos no se puede cerrar el expediente. "Sugeridos" = los que indica la pauta según el caso.
    public static class PautasCatalogo
    {
        private static readonly string[] RolesViolencia =
            { RolesEstudiantePauta.Recibe, RolesEstudiantePauta.Ejerce, RolesEstudiantePauta.Observador };

        private static readonly string[] RolesRiesgo =
            { RolesEstudiantePauta.Identificada, RolesEstudiantePauta.Observador };

        public static readonly IReadOnlyList<PautaCatalogoDTO> Pautas = new List<PautaCatalogoDTO>
        {
            Crear(1, "Violencia física y psicológica entre personas estudiantes",
                "Manifestaciones de violencia física y psicológica entre personas estudiantes.", 1, new[] { 5, 6, 7, 8, 10, 12 }),
            Crear(2, "Violencia sexual (acoso, hostigamiento sexual y otras) entre personas estudiantes",
                "Manifestaciones de violencia sexual entre personas estudiantes.", 1, new[] { 5, 6, 7, 8, 10, 12 }),
            Crear(3, "Bullying (acoso escolar)",
                "Manifestaciones de bullying o acoso escolar.", 1, new[] { 6, 7, 8, 10, 12 }),
            Crear(4, "Violencia ejercida por una tercera persona fuera del sistema educativo",
                "Violencia psicológica, física, sexual y trato corruptor ejercida por una tercera persona en perjuicio de una persona estudiante.", 1, new[] { 5, 7, 8, 12 }),
            Crear(5, "Violencia ejercida por una persona funcionaria del sistema educativo",
                "Violencia psicológica, física, sexual y trato corruptor ejercida por una persona funcionaria en perjuicio de una persona estudiante.", 1, new[] { 5, 7, 8, 12 }),
            Crear(6, "Riesgo (autolesión) y comportamiento suicida",
                "Ideación suicida, intento suicida y suicidio consumado de personas estudiantes en el contexto educativo.", 2, new[] { 7, 9, 10, 12 }),
            Crear(7, "Hallazgo, tenencia, consumo y tráfico de drogas y sustancias",
                "Drogas, sustancias psicoactivas y otros dispositivos (vapeadores, parches, cigarros electrónicos, entre otros).", 2, new[] { 4, 5, 7, 8, 10, 12 }),
            Crear(8, "Hallazgo, tenencia y uso de armas",
                "Hallazgo, tenencia y uso de armas por parte de personas estudiantes.", 2, new[] { 3, 4, 7, 8, 12 }),
            Crear(9, "Embarazo, postparto y lactancia",
                "Situaciones de embarazo, postparto y lactancia en estudiantes.", 2, new[] { 5, 7, 12 }),
        };

        private static PautaCatalogoDTO Crear(int numero, string nombre, string descripcion, int inicial, int[] sugeridos)
        {
            return new PautaCatalogoDTO
            {
                Numero = numero,
                Nombre = nombre,
                Descripcion = descripcion,
                FormularioInicial = inicial,
                Requeridos = new List<int> { inicial, 11 },
                Sugeridos = sugeridos.ToList(),
                RolesEstudiante = (inicial == 1 ? RolesViolencia : RolesRiesgo).ToList()
            };
        }

        public static PautaCatalogoDTO? Obtener(int numero) => Pautas.FirstOrDefault(p => p.Numero == numero);

        /// Formularios que se pueden crear más de una vez en el mismo expediente.
        public static bool EsRepetible(int numeroFormulario) =>
            numeroFormulario is 3 or 4 or 5 or 6 or 7 or 8 or 9 or 10 or 12;
    }
}
