using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Pauta
{
    /// Los 12 formularios de las Pautas de actuación del MEP, definidos como datos.
    /// Para corregir un campo o agregar uno basta con editar este archivo: la pantalla, la validación
    /// y el documento imprimible se generan a partir de estas definiciones.
    public static class PlantillasPauta
    {
        public static readonly IReadOnlyList<PlantillaPautaDTO> Todas = new List<PlantillaPautaDTO>
        {
            F1(), F2(), F3(), F4(), F5(), F6(), F7(), F8(), F9(), F10(), F11(), F12()
        };

        public static PlantillaPautaDTO? Obtener(int numero) => Todas.FirstOrDefault(p => p.Numero == numero);

        // =====================  Ayudantes para armar campos  =====================

        private static CampoPlantillaDTO Texto(string clave, string etiqueta, bool req = false, string? origen = null, string? visibleSi = null, int? max = null, string? ayuda = null) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Texto, Requerido = req, Origen = origen, VisibleSi = visibleSi, MaxLongitud = max, Ayuda = ayuda };

        private static CampoPlantillaDTO Largo(string clave, string etiqueta, bool req = false, string? visibleSi = null, string? ayuda = null, int? max = null) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.TextoLargo, Requerido = req, VisibleSi = visibleSi, Ayuda = ayuda, MaxLongitud = max };

        private static CampoPlantillaDTO Fecha(string clave, string etiqueta, bool req = false, string? origen = null, string? visibleSi = null) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Fecha, Requerido = req, Origen = origen, VisibleSi = visibleSi };

        private static CampoPlantillaDTO Hora(string clave, string etiqueta, bool req = false) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Hora, Requerido = req };

        private static CampoPlantillaDTO Num(string clave, string etiqueta, bool req = false, string? origen = null) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Numero, Requerido = req, Origen = origen };

        private static CampoPlantillaDTO Correo(string clave, string etiqueta, bool req = false) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Correo, Requerido = req };

        private static CampoPlantillaDTO Tel(string clave, string etiqueta, bool req = false) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Telefono, Requerido = req };

        private static CampoPlantillaDTO SiNo(string clave, string etiqueta, bool req = false, string? visibleSi = null) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.SiNo, Requerido = req, VisibleSi = visibleSi };

        private static CampoPlantillaDTO Casilla(string clave, string etiqueta) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Casilla };

        private static CampoPlantillaDTO Seleccion(string clave, string etiqueta, bool req, params (string valor, string texto)[] opciones) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Seleccion, Requerido = req, Opciones = Ops(opciones) };

        private static CampoPlantillaDTO Varias(string clave, string etiqueta, bool req, string? visibleSi, params (string valor, string texto)[] opciones) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Opciones, Requerido = req, VisibleSi = visibleSi, Opciones = Ops(opciones) };

        private static CampoPlantillaDTO Tabla(string clave, string etiqueta, int minFilas, int maxFilas, string? origen, string? visibleSi, params CampoPlantillaDTO[] columnas) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Tabla, MinFilas = minFilas > 0 ? minFilas : null, MaxFilas = maxFilas, Origen = origen, VisibleSi = visibleSi, Columnas = columnas.ToList(), Requerido = minFilas > 0 };

        private static CampoPlantillaDTO Fijo(string etiqueta) =>
            new() { Clave = "", Etiqueta = etiqueta, Tipo = TiposCampoPauta.TextoFijo };

        private static CampoPlantillaDTO Firma(string clave, string etiqueta) =>
            new() { Clave = clave, Etiqueta = etiqueta, Tipo = TiposCampoPauta.Firma };

        private static CampoPlantillaDTO Sello() =>
            new() { Clave = "sello", Etiqueta = "Sello institucional", Tipo = TiposCampoPauta.Sello };

        private static List<OpcionPautaDTO> Ops(IEnumerable<(string valor, string texto)> o) =>
            o.Select(x => new OpcionPautaDTO { Valor = x.valor, Texto = x.texto }).ToList();

        private static SeccionPlantillaDTO Sec(string titulo, params CampoPlantillaDTO[] campos) =>
            new() { Titulo = titulo, Campos = campos.ToList() };

        private static SeccionPlantillaDTO SecCond(string titulo, string visibleSi, params CampoPlantillaDTO[] campos) =>
            new() { Titulo = titulo, VisibleSi = visibleSi, Campos = campos.ToList() };

        private static SeccionPlantillaDTO Fila(string titulo, params CampoPlantillaDTO[] campos) =>
            new() { Titulo = titulo, Presentacion = "fila", Campos = campos.ToList() };

        private static PlantillaPautaDTO Plantilla(int numero, string nombre, string descripcion, params SeccionPlantillaDTO[] secciones) =>
            new() { Numero = numero, Nombre = nombre, Descripcion = descripcion, Repetible = PautasCatalogo.EsRepetible(numero), Secciones = secciones.ToList() };

        // Datos del centro educativo que se repiten en varios formularios (se precargan desde Configuración).
        private static SeccionPlantillaDTO Centro(bool conCodigo) => conCodigo
            ? Sec("Datos del centro educativo",
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Texto("centro", "Nombre del Centro Educativo", true, "centro_nombre"),
                Texto("codigo_presupuestario", "Código presupuestario", false, "centro_codigo"),
                Texto("circuito", "Circuito escolar", false, "centro_circuito"),
                Texto("dre", "DRE", false, "centro_dre"))
            : Sec("Datos del centro educativo",
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Texto("centro", "Nombre del Centro Educativo", true, "centro_nombre"),
                Texto("circuito", "Circuito escolar", false, "centro_circuito"),
                Texto("dre", "DRE", false, "centro_dre"));

        // Datos de una persona estudiante en una sola fila (se precargan al elegir la persona estudiante del expediente).
        private static SeccionPlantillaDTO DatosEstudiante(string titulo) =>
            Fila(titulo,
                Texto("est_nombre", "Nombre completo", true, "est_nombre"),
                Texto("est_cedula", "Cédula", false, "est_cedula"),
                Fecha("est_nacimiento", "Fecha de nacimiento", false, "est_nacimiento"),
                Num("est_edad", "Edad", false, "est_edad"),
                Texto("est_nivel", "Nivel que cursa", false, "est_nivel"));

        private static CampoPlantillaDTO Direccion(string clave = "direccion") =>
            Largo(clave, "Dirección de residencia (provincia, cantón, distrito, poblado, otras señas; ser muy específico)", false, null, null, 500);

        // =====================  Formulario N°1  =====================
        private static PlantillaPautaDTO F1() => Plantilla(1,
            "Información inicial y comunicación escrita a la dirección ante presunta manifestación de violencia",
            "Primer paso de las pautas de violencia entre estudiantes, bullying y violencia por una tercera persona o persona funcionaria.",
            Centro(true),
            Sec("Origen del relato",
                Varias("origen", "De dónde proviene el relato, cómo se enteró de la situación", true, null,
                    ("deteccion_propia", "Detección propia"), ("familiares", "Familiares"),
                    ("victima", "Presunta persona víctima"), ("direccion", "Dirección"),
                    ("observadores", "Estudiantes observadores"), ("otros", "Otros")),
                Texto("origen_otros", "Otros (indique)", true, null, "origen=otros")),
            Sec("Recepción inicial de la situación",
                Largo("relato", "Transcriba el relato tal cual es narrado (no interprete)", true),
                Largo("cuando_donde_quien", "¿Cuándo, dónde y quién?"),
                Largo("desde_cuando", "¿Desde cuándo se producen estas situaciones? Posibles fechas de los eventos")),
            Sec("Datos de las personas estudiantes involucradas",
                Tabla("estudiantes", "Personas estudiantes involucradas", 1, 30, "expediente_estudiantes", null,
                    Texto("nombre", "Nombre completo", true), Texto("seccion", "Sección"),
                    Casilla("rol_recibe", "Quien recibe la violencia"), Casilla("rol_ejerce", "Quien ejerce la violencia"),
                    Casilla("rol_observador", "Observadores-testigos"))),
            Sec("Sospecha de situación",
                Varias("pauta_sospechada", "Existe la sospecha de que en el centro educativo se presenta una situación de", true, null,
                    ("1", "Violencia física y psicológica entre personas estudiantes"),
                    ("2", "Violencia sexual (acoso, hostigamiento sexual y otras) entre personas estudiantes"),
                    ("3", "Bullying (acoso escolar)"),
                    ("4", "Violencia ejercida por una tercera persona fuera del sistema educativo"),
                    ("5", "Violencia ejercida por una persona funcionaria del sistema educativo"))),
            Sec("Persona responsable y dirección",
                Texto("responsable", "Nombre de la persona a cargo de ejecutar esta Pauta", true, "usuario_nombre"),
                Firma("firma_funcionario", "Firma funcionario que activa la Pauta"),
                Texto("director", "Nombre de la persona directora", false, "centro_director"),
                Fecha("fecha_director", "Fecha"),
                Fijo("Se le recuerda que la información aquí descrita debe ser tratada con confidencialidad y discreción.")));

        // =====================  Formulario N°2  =====================
        private static PlantillaPautaDTO F2() => Plantilla(2,
            "Información inicial y comunicación escrita ante presuntas situaciones de vulnerabilidad y riesgo",
            "Primer paso de las pautas de riesgo suicida, drogas, armas y embarazo, postparto y lactancia.",
            Centro(true),
            Sec("Origen del relato",
                Varias("origen", "De dónde proviene el relato, cómo se enteró de la situación", true, null,
                    ("funcionaria", "Persona funcionaria del centro educativo"), ("direccion", "Dirección"),
                    ("observador", "Persona estudiante observador"), ("familiares", "Familiares"), ("otros", "Otros")),
                Texto("origen_otros", "Otros (indique)", true, null, "origen=otros")),
            Sec("Recepción inicial de la situación",
                Largo("relato", "Transcriba el relato tal cual es narrado (no interprete)", true),
                Largo("cuando_donde", "¿Cuándo y dónde sucedió?"),
                Largo("desde_cuando", "¿Desde cuándo se producen estas situaciones? Posibles fechas de los eventos")),
            Sec("Datos de las personas estudiantes involucradas",
                Tabla("estudiantes", "Personas estudiantes involucradas", 1, 30, "expediente_estudiantes", null,
                    Texto("nombre", "Nombre completo", true), Texto("seccion", "Sección"),
                    Casilla("rol_identificada", "Persona estudiante identificada"), Casilla("rol_observador", "Observadores-testigos"))),
            Sec("Sospecha de situación",
                Varias("pauta_sospechada", "Existe la sospecha de que en el centro educativo se presenta una situación de", true, null,
                    ("6", "Riesgo (autolesión) y comportamiento suicida: ideación suicida, intento suicida y suicidio consumado"),
                    ("7", "Hallazgo, tenencia, consumo y tráfico de drogas, sustancias psicoactivas y otros dispositivos"),
                    ("8", "Hallazgo, tenencia y uso de armas por parte de personas estudiantes"),
                    ("9", "Embarazo, postparto y lactancia en estudiantes"))),
            Sec("Persona responsable y dirección",
                Texto("responsable", "Nombre de la persona a cargo de ejecutar esta Pauta", true, "usuario_nombre"),
                Firma("firma_funcionario", "Firma funcionario que activa la Pauta"),
                Texto("director", "Nombre de la persona directora", false, "centro_director"),
                Fecha("fecha_director", "Fecha"),
                Fijo("Se le recuerda que la información aquí descrita debe ser tratada con confidencialidad y discreción.")));

        // =====================  Formulario N°3  =====================
        private static PlantillaPautaDTO F3() => Plantilla(3,
            "Acta de traslado de la persona estudiante hacia otra instancia para su debida atención",
            "En caso de que la persona estudiante requiera ser trasladada al área de salud más cercana.",
            Sec("Datos de la persona estudiante",
                Texto("est_nombre", "Nombre", true, "est_nombre"),
                Texto("est_seccion", "Sección", false, "est_seccion"),
                Fijo("RECUERDE: En caso de intereses contrapuestos se debe contactar al PANI."),
                SiNo("contacto_familia", "Se contactó a la madre, el padre, la persona encargada o familiar", true),
                Largo("motivo", "Explique motivo de traslado", true)),
            Sec("Traslado",
                Fijo("Después de desplegar las actuaciones correspondientes en la ejecución de las Pautas y en aras de velar por el interés superior de la persona estudiante, en presencia de los testigos quienes firman esta acta, se tomó la decisión de trasladar a la persona estudiante con el siguiente destino:"),
                Texto("destino", "Destino", true),
                Texto("transporte", "Medio de transporte", true),
                Texto("placa", "Placa")),
            Sec("Personas acompañantes",
                Texto("acomp1_nombre", "Nombre acompañante 1 (conductor)", true),
                Texto("acomp1_identificacion", "# identificación"),
                Tel("acomp1_telefono", "Teléfono"),
                Texto("acomp1_cargo", "Cargo"),
                Texto("acomp2_nombre", "Nombre acompañante 2"),
                Texto("acomp2_identificacion", "# identificación"),
                Tel("acomp2_telefono", "Teléfono"),
                Texto("acomp2_cargo", "Cargo")),
            Sec("Firmas",
                Texto("director", "Nombre de la persona directora", false, "centro_director"),
                Firma("firma_director", "Firma de la persona directora"),
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Firma("testigo1", "Testigo 1 (nombre y firma)"),
                Firma("testigo2", "Testigo 2 (nombre y firma)")));

        // =====================  Formulario N°4  =====================
        private static PlantillaPautaDTO F4() => Plantilla(4,
            "Acta de recolección y resguardo de armas y drogas en el contexto educativo",
            "Cuando se halle o decomise un arma, droga o sustancia psicoactiva dentro del contexto educativo.",
            Sec("Persona responsable del centro educativo (quien activa la Pauta)",
                Fijo("En caso de hallarse o decomisarse algún arma, droga o sustancia psicoactiva dentro del contexto educativo, se debe custodiar hasta que se determine qué se hará con el artículo; remítase a las indicaciones en el apartado de medidas de las Pautas respectivas."),
                Texto("resp_nombre", "Nombre", true, "usuario_nombre"),
                Texto("resp_identificacion", "# Identificación"),
                Texto("resp_cargo", "Cargo", false, "usuario_cargo"),
                Varias("tipo_hallazgo", "Marque lo que corresponde", true, null,
                    ("deteccion", "1. Detección de armas y drogas"), ("tenencia", "2. Tenencia de armas y drogas"))),
            SecCond("1. Detección de armas y drogas", "tipo_hallazgo=deteccion",
                Largo("det_descripcion", "Descripción del arma / droga", true),
                Texto("det_lugar", "Lugar donde se encontró arma / droga", true),
                Texto("det_testigo", "Nombre de la persona testigo"),
                Texto("det_testigo_cargo", "Cargo")),
            SecCond("2. Tenencia de armas y drogas", "tipo_hallazgo=tenencia",
                Texto("ten_estudiante", "Nombre de la persona estudiante", true, "est_nombre"),
                Texto("ten_seccion", "Sección", false, "est_seccion"),
                Largo("ten_descripcion", "Descripción del arma / droga", true),
                Texto("ten_tipo", "Tipo de arma o droga"),
                Texto("ten_testigo", "Nombre de la persona testigo"),
                Texto("ten_testigo_cargo", "Cargo")),
            Sec("Cierre del acta",
                Firma("firma_activa", "Firma de la persona encargada de la activación de la Pauta"),
                Texto("director_recibe", "Nombre de la persona directora quien recibe lo decomisado", false, "centro_director"),
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Fijo("Se recomienda tomar fotografías del artículo que está recolectando, ya que estas podrán respaldar lo indicado en la presente acta."),
                Num("cantidad_fotos", "Cantidad de fotos que adjunta")),
            Sec("Devolución del arma o droga legal decomisada al encargado legal de la persona estudiante",
                Texto("dev_nombre", "Nombre del encargado legal"),
                Texto("dev_identificacion", "# Identificación"),
                Firma("dev_firma", "Firma"),
                Fecha("dev_fecha", "Fecha")));

        // =====================  Formulario N°5  =====================
        private static PlantillaPautaDTO F5() => Plantilla(5,
            "Comunicación a la persona encargada legal de la persona estudiante",
            "Citación a la persona encargada legal a una reunión en el centro educativo.",
            Sec("Datos de la comunicación",
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Texto("centro", "Nombre del Centro Educativo", true, "centro_nombre"),
                Texto("encargado", "Persona encargada legal", true),
                Texto("estudiante", "Nombre de la persona estudiante", true, "est_nombre"),
                Largo("hechos", "Se ha visto implicado/a en los supuestos hechos", true)),
            Sec("Reunión",
                Texto("reunion_con", "Reunión con", true),
                Fecha("reunion_fecha", "Día de la reunión", true),
                Hora("reunion_hora", "Hora", true),
                Tel("contacto_telefono", "Teléfono para reprogramar la reunión"),
                Correo("contacto_correo", "Correo electrónico para reprogramar la reunión")),
            Sec("Emisión",
                Fecha("emision_fecha", "Emitido el día", false, "fecha_hoy"),
                Hora("emision_hora", "a las"),
                Firma("firma", "Nombre y firma de la persona encargada de activar las Pautas"),
                Fijo("LEER CON ATENCIÓN: Es deber de las madres, padres o personas encargadas de una persona menor de edad de cooperar y vincularse con el centro educativo, así como presentarse cuando sean citados por este (Reglamento General de Establecimientos de Educación Media, art. 73 y 74.5, Reglamento de Evaluación de los Aprendizajes art. 20 incisos g y h, Código de Familia art. 143). De manera que la falta de atención a TRES comunicaciones de forma inmediata el centro educativo dará aviso al PANI para que este verifique si la omisión constituye algún indicio de negligencia o descuido que esté afectando a la persona menor de edad (Código de Niñez y Adolescencia art. 130) o bien se solicitará la representación de la persona menor de edad ante el proceso a seguir en el centro educativo.")));

        // =====================  Formulario N°6  =====================
        private static PlantillaPautaDTO F6() => Plantilla(6,
            "Compilación de información de las personas estudiantes involucradas",
            "Entrevista a la persona violentada, a las observadoras y a la que presuntamente ejerció la violencia, en compañía de su persona encargada legal.",
            Sec("Información general para brindar a las familias",
                Fijo("De qué trata la activación de la presente Pauta, explique cuáles son los próximos pasos y enfatice el interés del centro educativo por garantizar la protección de las personas estudiantes a su cargo y trabajar de manera conjunta para un abordaje integral."),
                Fijo("Las presuntas situaciones de cualquier manifestación de violencia y bullying no se median, ni concilian."),
                Fijo("La responsabilidad parental confiere los derechos e impone los deberes de educar, guardar, vigilar y, en forma moderada, corregir al hijo (art. 143 del Código de Familia)."),
                Fijo("No se pueden brindar los datos de las personas estudiantes ni de sus familias involucradas con el fin de evitar conflictos. Ni las otras familias conocerán sus datos."),
                Fijo("El uso del castigo corporal o trato humillante es prohibido para personas menores de edad y puede ser considerado un delito; enfatice sobre la importancia de no utilizar la violencia en el hogar (Ley N° 8654)."),
                Fijo("Medidas con las que cuenta el centro educativo: implementar medidas para detener la situación dentro del centro educativo; coordinación interna para apoyos educativos; elaboración de referencias, denuncias e informes interinstitucionales; comunicar las medidas que se adoptan (solo se brinda información al padre, madre o encargado responsable de la persona estudiante a su cargo); comprometer a las familias en una participación activa para detener cualquier manifestación de violencia."),
                Fijo("Responsabilidad del uso correcto de los dispositivos electrónicos: el acceso y uso de tecnología y dispositivos (teléfonos celulares, tabletas y laptops) que utilicen las personas estudiantes son responsabilidad de los encargados legales, porque ellos los suministraron y dan el acceso a internet, por lo que deberán velar por su contenido.")),
            Sec("A. Datos de la persona estudiante",
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Texto("est_nombre", "Nombre completo", true, "est_nombre"),
                Texto("grado_seccion", "Grado y sección", false, "est_nivel"),
                Fecha("est_nacimiento", "Fecha de nacimiento", false, "est_nacimiento"),
                Num("est_edad", "Edad", false, "est_edad"),
                Texto("encargado_presente", "Nombre de la persona encargada legal presente", true),
                Seleccion("entrevistado", "Se realiza la entrevista a", true,
                    ("violentada", "Persona estudiante violentada"),
                    ("observadora", "Persona estudiante observadora"),
                    ("ejercio", "Persona estudiante que supuestamente ejerció la violencia"))),
            Sec("B. Compilación de la información",
                Fijo("Recuerde que el relato tiene que ser narrado por la persona estudiante involucrada en compañía de la persona encargada legal."),
                Largo("p1_relato", "1. Relato de la situación: transcriba de forma clara el relato tal cual es narrado (no interprete)", true),
                Largo("p2_cuando_donde", "2. ¿Cuándo y dónde ha sucedido?"),
                Largo("p3_quienes", "3. ¿Quiénes son las personas que lo hacen?"),
                Largo("p4_quien_conoce", "4. ¿Quién conoce la situación? ¿A quién le has contado estas situaciones que estás viviendo?"),
                Largo("p5_desde_cuando", "5. ¿Desde cuándo se producen estas situaciones?")),
            Sec("Firmas",
                Firma("firma_ejecutor", "Nombre y firma de la persona a cargo de ejecutar esta Pauta"),
                Firma("firma_encargado", "Firma de la persona encargada legal"),
                Firma("firma_estudiante", "Firma de la persona estudiante (mayor de 12 años)")));

        // =====================  Formulario N°7  =====================
        private static PlantillaPautaDTO F7() => Plantilla(7,
            "Referencia a otras instituciones de atención",
            "Referencia a CCSS, PANI, RISA, IAFA u otra institución.",
            Sec("Datos de la referencia",
                Fijo("Se debe resguardar la confidencialidad de los datos de la persona funcionaria denunciante, de conformidad con el artículo 49 del Código de la Niñez y la Adolescencia."),
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Texto("centro", "Centro educativo", true, "centro_nombre"),
                Varias("instituciones", "Se solicita la intervención de las siguientes instituciones", true, null,
                    ("ccss", "C.C.S.S"), ("pani", "PANI"), ("risa", "RISA (menores de 12 años)"), ("iafa", "IAFA"), ("otra", "OTRA")),
                Texto("otra_nombre", "Indique el nombre de la otra institución", true, null, "instituciones=otra"),
                Varias("pani_motivo", "Motivo de referencia al PANI", true, "instituciones=pani",
                    ("negligencia", "Presunta negligencia parental"),
                    ("omision", "Omisión de responsabilidad parental"),
                    ("vulnerabilidad", "Vulnerabilidad o riesgo de la persona menor de edad"))),
            DatosEstudiante("Datos de la persona estudiante"),
            Sec("Residencia y personas encargadas",
                Direccion(),
                Tabla("encargados", "Nombre de las personas encargadas legales", 1, 5, null, null,
                    Texto("nombre", "Nombre completo", true), Texto("cedula", "Cédula"), Tel("telefono", "Teléfono"))),
            Sec("Motivo y firma",
                Largo("motivo", "Motivo de referencia", true),
                Firma("firma", "Nombre y firma de la persona responsable de activar la Pauta o persona directora"),
                Sello()));

        // =====================  Formulario N°8  =====================
        private static PlantillaPautaDTO F8() => Plantilla(8,
            "Denuncia a Fiscalía, Penal Juvenil u Organismo de Investigación Judicial (OIJ)",
            "Se llena un formulario por cada estudiante que presuntamente ejerció la violencia.",
            Sec("Datos del centro educativo",
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Texto("centro", "Nombre del Centro Educativo", true, "centro_nombre"),
                Texto("circuito", "Circuito escolar", false, "centro_circuito"),
                Texto("dre", "DRE", false, "centro_dre"),
                Fijo("Observación: se llena un formulario por estudiante que presuntamente ejerció la violencia.")),
            DatosEstudiante("Datos de la persona estudiante (quien presuntamente ejerció la violencia)"),
            Sec("Contacto y encargados de quien presuntamente ejerció la violencia",
                Correo("est_correo", "Correo electrónico"),
                Direccion("est_direccion"),
                Tabla("encargados", "Nombre de las personas encargadas legales", 1, 5, null, null,
                    Texto("nombre", "Nombre completo", true), Texto("cedula", "Cédula"), Tel("telefono", "Teléfono"), Correo("correo", "Correo electrónico"))),
            Sec("Motivo de la denuncia",
                Largo("motivo", "Motivo de la denuncia", true, null, null, 6000)),
            Sec("Datos de la persona estudiante (quien presuntamente recibe la violencia)",
                Texto("vic_nombre", "Nombre completo", true),
                Texto("vic_cedula", "Cédula"),
                Fecha("vic_nacimiento", "Fecha de nacimiento"),
                Num("vic_edad", "Edad"),
                Texto("vic_nivel", "Nivel que cursa"),
                Tabla("vic_encargados", "Nombre de las personas encargadas legales", 0, 5, null, null,
                    Texto("nombre", "Nombre completo", true), Texto("cedula", "Cédula")),
                Correo("vic_correo", "Correo electrónico"),
                Tel("vic_telefono", "Teléfono"),
                Direccion("vic_direccion")),
            Sec("Datos de personas observadoras o testigos",
                Tabla("testigos", "Personas observadoras o testigos", 0, 20, null, null,
                    Texto("nombre", "Nombre completo", true), Texto("cedula", "Cédula"), Num("edad", "Edad"),
                    Texto("nivel_ocupacion", "Nivel / ocupación"), Texto("contacto", "Teléfono / correo electrónico"),
                    Texto("direccion", "Dirección exacta de residencia")),
                Sello(),
                Fijo("En caso de que esta no sea la instancia de atención, respetuosamente se solicita redireccionarla, conforme lo establece el Código Procesal Penal.")));

        // =====================  Formulario N°9  =====================
        private static PlantillaPautaDTO F9() => Plantilla(9,
            "Referencia interinstitucional con riesgo suicida o necesidades de atención en salud mental",
            "Contiene información sensible de salud mental: solo la ve la persona responsable del expediente, la dirección y el administrador.",
            Sec("Datos de la referencia",
                Fecha("fecha", "Fecha", true, "fecha_hoy")),
            DatosEstudiante("Datos de la persona estudiante que requiere la atención"),
            Sec("Residencia, familia y apoyo",
                Direccion(),
                Texto("encargado_nombre", "En caso de ser persona menor de edad, nombre de la madre, padre o persona encargada legal"),
                Tel("encargado_telefono", "Número de teléfono o celular"),
                Texto("grupo_familiar", "Grupo familiar con el que vive la persona estudiante"),
                Texto("apoyo_contacto", "Nombre del contacto de persona de apoyo, número de teléfono y vínculo o parentesco")),
            Sec("Datos del centro educativo que refiere",
                Texto("centro", "Nombre del centro educativo", true, "centro_nombre"),
                Texto("centro_contacto", "Número de teléfono y correo electrónico institucional", false, "centro_contacto"),
                Texto("emisor", "Nombre y cargo de la persona que emite la referencia", true, "usuario_nombre_cargo")),
            Sec("Motivo de referencia",
                Largo("motivo", "Motivo de referencia (breve descripción del caso, enfatizar en los factores de riesgo de la persona estudiante)", true)),
            Sec("Situación de vulnerabilidad y riesgo",
                Casilla("autolesiones", "Autolesiones"),
                Largo("autolesiones_desc", "Descripción de la situación (autolesiones)", false, "autolesiones=true"),
                Casilla("ideacion", "Ideación suicida"),
                Largo("ideacion_desc", "Descripción de la situación (ideación suicida)", false, "ideacion=true"),
                Casilla("intento", "Intento suicida"),
                Largo("intento_desc", "Descripción de la situación (intento suicida)", false, "intento=true"),
                Sello(),
                Fijo("Recuerde copiar a COLOSAM y CORESAM, según lo establecido en el Reglamento de Salud Mental.")));

        // =====================  Formulario N°10  =====================
        private static PlantillaPautaDTO F10() => Plantilla(10,
            "Referencia a dependencias internas del MEP",
            "Solicitud de apoyo a la DRE, asesorías, Vida Estudiantil, UPRE, PANEA u otras instancias del Nivel Central.",
            Sec("Datos de la solicitud",
                Fecha("fecha", "Fecha", true, "fecha_hoy"),
                Texto("centro", "Centro educativo", true, "centro_nombre"),
                Texto("director_solicita", "Nombre de la persona directora que solicita el apoyo", true, "centro_director"),
                Varias("dependencias", "Se solicita la intervención de las siguientes dependencias", true, null,
                    ("dre", "Dirección Regional de Educación"),
                    ("asesorias", "Departamento de Asesorías Pedagógicas, Regionales"),
                    ("curricular", "Dirección de Desarrollo Curricular, Nivel Central"),
                    ("vida_estudiantil", "Dirección de Vida Estudiantil"),
                    ("upre", "Unidad para la Permanencia y Reincorporación Educativa (UPRE)"),
                    ("panea", "Programa de Alimentación y Nutrición del Escolar y del Adolescente (PANEA)"),
                    ("otra", "Otra instancia del Nivel Central")),
                Texto("otra_nombre", "Otra instancia (indique)", true, null, "dependencias=otra")),
            DatosEstudiante("Datos de la persona estudiante referida"),
            Sec("Solicitud y firma",
                Largo("solicitud", "Describa la solicitud", true),
                Firma("firma_director", "Firma persona directora"),
                Sello(),
                Fijo("Recuerde adjuntar el comprobante de recepción de documentos al archivar el formulario en el expediente de activación de la Pauta.")));

        // =====================  Formulario N°11  =====================
        private const string HayManifestacion = "sin_identificacion=";

        private static PlantillaPautaDTO F11() => Plantilla(11,
            "Informe de actuación",
            "Resumen de lo actuado: manifestación, personas involucradas, medidas, plan de atención y fechas de seguimiento.",
            Sec("Datos del informe",
                Texto("centro", "Centro educativo", true, "centro_nombre"),
                Texto("elaborado_por", "Informe elaborado por", true, "usuario_nombre"),
                Fecha("fecha_elaboracion", "Fecha de elaboración del informe", true, "fecha_hoy"),
                Fecha("fecha_activacion", "Fecha en que se activó la Pauta", true, "expediente_fecha"),
                Varias("sin_identificacion", "Si no se identificó nada, marque y proceda al cierre", false, null,
                    ("sin_violencia", "No se identificó alguna manifestación de violencia o bullying (proceda al cierre)"),
                    ("sin_vulnerabilidad", "No se identificó alguna situación de vulnerabilidad y riesgo (proceda al cierre)"))),
            SecCond("Pauta activada y su manifestación", HayManifestacion,
                Varias("pauta_manifestacion", "Pauta activada", true, null,
                    ("entre_estudiantes", "Violencia o bullying (física, psicológica, sexual) entre personas estudiantes"),
                    ("funcionaria", "Violencia ejercida por una persona funcionaria del sistema educativo"),
                    ("tercera_persona", "Violencia ejercida por una tercera persona fuera del sistema educativo"),
                    ("suicida", "Riesgo (autolesión) y comportamiento suicida"),
                    ("drogas", "Hallazgo, tenencia, consumo y tráfico de drogas, sustancias psicoactivas y otros dispositivos"),
                    ("armas", "Hallazgo, tenencia y uso de armas"),
                    ("embarazo", "Embarazo, postparto y lactancia")),
                Varias("violencia_tipos", "Violencia", false, "pauta_manifestacion=entre_estudiantes",
                    ("fisica", "Física"), ("psicologica", "Psicológica"), ("sexual", "Sexual")),
                Varias("bullying_tipos", "Bullying / acoso escolar", false, "pauta_manifestacion=entre_estudiantes",
                    ("fisica", "Física"), ("psicologica", "Psicológica"), ("cyberbullying", "Cyberbullying"),
                    ("lgtb", "Contra la población LGTB"), ("racial", "Discriminación racial y xenofobia")),
                Varias("funcionaria_tipos", "Tipo de violencia (persona funcionaria)", false, "pauta_manifestacion=funcionaria",
                    ("fisica", "Física"), ("psicologica", "Psicológica"), ("sexual", "Sexual"), ("trato_corruptor", "Trato corruptor")),
                Varias("tercera_tipos", "Tipo de violencia (tercera persona)", false, "pauta_manifestacion=tercera_persona",
                    ("fisica", "Física"), ("psicologica", "Psicológica"), ("sexual", "Sexual"), ("trato_corruptor", "Trato corruptor"))),
            SecCond("Datos de las personas estudiantes involucradas", HayManifestacion,
                Tabla("estudiantes", "Personas estudiantes involucradas", 1, 30, "expediente_estudiantes", null,
                    Texto("nombre", "Nombre completo", true), Texto("seccion", "Sección"),
                    Casilla("rol_recibe", "Quien recibe la violencia"), Casilla("rol_ejerce", "Quien ejerce la violencia"),
                    Casilla("presunta_armas_drogas", "Presunta situación de armas o drogas"))),
            SecCond("Datos de las personas funcionarias involucradas", "sin_identificacion=;pauta_manifestacion=funcionaria",
                Tabla("funcionarias", "Personas funcionarias involucradas", 1, 10, null, null,
                    Texto("nombre", "Nombre completo", true), Texto("cedula", "Cédula"), Texto("puesto", "Puesto"),
                    Casilla("psicologica", "Psicológica"), Casilla("fisica", "Física"),
                    Casilla("sexual", "Sexual / relaciones impropias"), Casilla("trato_corruptor", "Trato corruptor"))),
            SecCond("Breve relato de lo acontecido", HayManifestacion,
                Largo("relato", "Quién, dónde, cuándo, fecha, lugar y situación", true, null, null, 6000)),
            SecCond("Medidas de actuación: 1. Articulación inter e intrainstitucional", HayManifestacion,
                SiNo("aviso_911", "¿Dio aviso por medio del 9-1-1?", true),
                Texto("reporte_911", "Número de reporte", false, null, "aviso_911=Si"),
                SiNo("denuncia_fiscalia", "¿Interpuso denuncia ante la Fiscalía / OIJ?", true),
                SiNo("denuncia_red_social", "¿Realizó denuncia ante alguna cuenta activa de red social?", true),
                SiNo("aviso_pani", "¿Dio aviso al PANI para que interviniera?", true),
                SiNo("referencia_ccss", "¿Realizó referencia a la CCSS?", true),
                SiNo("denuncia_disciplinarios", "¿Realizó denuncia ante el departamento de asuntos disciplinarios?", true),
                SiNo("coordinaciones", "¿Se realizaron coordinaciones con otras instituciones u otros recursos?", true),
                Texto("coord_institucion", "Nombre de la institución o recurso", true, null, "coordinaciones=Si"),
                Texto("coord_contacto", "Persona contacto", false, null, "coordinaciones=Si"),
                Fecha("coord_fecha", "Fecha de coordinación", false, null, "coordinaciones=Si")),
            SecCond("Medidas de actuación: 2. Medidas de protección a realizar", HayManifestacion,
                Tabla("medidas_proteccion", "Medidas de protección", 1, 12, null, null,
                    Largo("medida", "Medida de protección", true))),
            SecCond("Medidas de actuación: 3. Acciones psico socioeducativas (plan de atención)", HayManifestacion,
                Largo("acciones_psico", "Acciones psico socioeducativas (plan de atención)", true, null, null, 6000)),
            SecCond("Medidas de actuación: 4. ¿Requiere la activación de otra Pauta?", HayManifestacion,
                SiNo("requiere_otra_pauta", "¿Requiere la activación de otra Pauta?", true),
                Texto("otra_pauta_cual", "Especifique cuál", true, null, "requiere_otra_pauta=Si"),
                Texto("otra_pauta_porque", "¿Por qué?", false, null, "requiere_otra_pauta=No")),
            Sec("Seguimiento y firmas",
                Fecha("seguimiento_1", "Fecha del primer informe de seguimiento"),
                Fecha("seguimiento_2", "Fecha del segundo informe de seguimiento"),
                Firma("firma_elaboro", "Nombre y firma de quien elaboró el informe"),
                Firma("firma_director", "Nombre y firma de la persona directora"),
                Fecha("fecha_recibido", "Fecha de recibido del informe en Dirección"),
                Sello()));

        // =====================  Formulario N°12  =====================
        private static PlantillaPautaDTO F12() => Plantilla(12,
            "Informe de seguimiento",
            "Seguimiento mensual (hasta tres meses) a las medidas de actuación y cierre del expediente.",
            Sec("Datos del informe",
                Fecha("fecha_informe", "Fecha de elaboración del informe de seguimiento", true, "fecha_hoy"),
                Texto("centro", "Nombre del centro educativo", true, "centro_nombre"),
                Texto("elaborado_por", "Informe elaborado por", true, "usuario_nombre")),
            Sec("A. Articulación inter e intrainstitucional",
                Tabla("articulacion", "Seguimiento a las medidas de actuación", 0, 8, null, null,
                    Texto("instancia", "Instancia"), Texto("mes1", "Primer mes"), Texto("mes2", "Segundo mes"), Texto("mes3", "Tercer mes"))
                    .ConFilas("Fiscalía", "PANI", "CCSS", "ASUNTOS DISCIPLINARIOS-MEP", "Otro:", "Otro:")),
            Sec("B. Seguimiento a las medidas de protección registradas en el informe de actuación",
                Tabla("medidas_seguimiento", "Medidas de protección", 0, 12, "f11_medidas", null,
                    Texto("medida", "Medida de protección"), Texto("mes1", "Primer mes / fecha"), Texto("mes2", "Segundo mes / fecha"), Texto("mes3", "Tercer mes / fecha"))),
            Sec("C. Acciones psico socioeducativas (plan de atención)",
                Tabla("acciones_seguimiento", "Acciones establecidas", 0, 12, "f11_acciones", null,
                    Texto("accion", "Acción establecida"), Texto("mes1", "Primer mes / fecha"), Texto("mes2", "Segundo mes / fecha"), Texto("mes3", "Tercer mes / fecha"))),
            Sec("Cierre del expediente",
                Fijo("Antes del cierre del expediente de la Pauta recuerde realizar la devolución de las acciones realizadas con las personas encargadas legales de las personas estudiantes involucradas (personas afectadas), registrar la devolución en un acta y firmada por las partes."),
                Fecha("fecha_cierre", "Fecha de cierre de la Pauta"),
                Texto("encargado_nombre", "Nombre del encargado de la activación de la Pauta", false, "usuario_nombre"),
                Firma("firma_encargado", "Firma del encargado de la activación de la Pauta"),
                Texto("visto_bueno", "Visto bueno de la persona directora para el cierre del expediente (nombre)", false, "centro_director"),
                Firma("firma_director", "Firma de la persona directora"),
                Sello()));

        private static CampoPlantillaDTO ConFilas(this CampoPlantillaDTO campo, params string[] filas)
        {
            campo.FilasIniciales = filas.ToList();
            return campo;
        }
    }
}
