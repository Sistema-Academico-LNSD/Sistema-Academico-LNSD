using System.Globalization;
using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Pauta
{
    /// Trabaja los datos de un formulario contra su plantilla:
    /// limpia lo que llega del navegador, valida lo obligatorio y arma los valores iniciales.
    public static class FormularioPautaHelper
    {
        private const int MaxCaracteresJson = 262144;
        private static readonly Regex PatronTelefono = new(@"^[0-9+\-\s()]{4,30}$", RegexOptions.Compiled);

        private static readonly JsonSerializerOptions OpcionesJson = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static bool TieneDatos(CampoPlantillaDTO c) =>
            c.Tipo != TiposCampoPauta.TextoFijo && c.Tipo != TiposCampoPauta.Firma && c.Tipo != TiposCampoPauta.Sello;

        // =====================  Limpieza  =====================

        /// Deja solo las claves de la plantilla, con el tipo correcto. Si algo viene mal formado, lo informa en la lista de errores.
        public static (string Json, List<string> Errores) Sanitizar(PlantillaPautaDTO plantilla, string? datosJson)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(datosJson))
                return ("{}", errores);

            if (datosJson.Length > MaxCaracteresJson)
            {
                errores.Add("Los datos superan el tamaño permitido.");
                return ("{}", errores);
            }

            JsonObject? entrada;
            try { entrada = JsonNode.Parse(datosJson) as JsonObject; }
            catch (JsonException) { entrada = null; }

            if (entrada == null)
            {
                errores.Add("Los datos no tienen un formato válido.");
                return ("{}", errores);
            }

            var salida = new JsonObject();

            foreach (var seccion in plantilla.Secciones)
            {
                foreach (var campo in seccion.Campos)
                {
                    if (!TieneDatos(campo) || !entrada.TryGetPropertyValue(campo.Clave, out var valor))
                        continue;

                    var limpio = SanitizarValor(campo, valor, string.Empty, errores);
                    if (limpio != null)
                        salida[campo.Clave] = limpio;
                }
            }

            return (salida.ToJsonString(OpcionesJson), errores);
        }

        private static JsonNode? SanitizarValor(CampoPlantillaDTO c, JsonNode? v, string prefijo, List<string> errores)
        {
            if (v == null)
                return null;

            string etiqueta = prefijo + c.Etiqueta;

            switch (c.Tipo)
            {
                case TiposCampoPauta.Texto:
                case TiposCampoPauta.TextoLargo:
                case TiposCampoPauta.Correo:
                case TiposCampoPauta.Telefono:
                    {
                        string? s = Cadena(v);
                        if (string.IsNullOrEmpty(s)) return null;

                        int max = c.MaxLongitud ?? (c.Tipo == TiposCampoPauta.TextoLargo ? 4000 : 300);
                        if (s.Length > max)
                        {
                            errores.Add($"{etiqueta}: no puede superar los {max} caracteres.");
                            return null;
                        }
                        if (c.Tipo == TiposCampoPauta.Correo && !EsCorreo(s))
                        {
                            errores.Add($"{etiqueta}: el correo no tiene un formato válido.");
                            return null;
                        }
                        if (c.Tipo == TiposCampoPauta.Telefono && !PatronTelefono.IsMatch(s))
                        {
                            errores.Add($"{etiqueta}: el teléfono no tiene un formato válido.");
                            return null;
                        }
                        return JsonValue.Create(s);
                    }

                case TiposCampoPauta.Fecha:
                    {
                        string? s = Cadena(v);
                        if (string.IsNullOrEmpty(s)) return null;

                        if (!DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
                            || fecha.Year < 1900 || fecha.Year > 2100)
                        {
                            errores.Add($"{etiqueta}: la fecha no es válida.");
                            return null;
                        }
                        return JsonValue.Create(fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    }

                case TiposCampoPauta.Hora:
                    {
                        string? s = Cadena(v);
                        if (string.IsNullOrEmpty(s)) return null;

                        if (!TimeOnly.TryParseExact(s, new[] { "HH:mm", "HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
                        {
                            errores.Add($"{etiqueta}: la hora no es válida.");
                            return null;
                        }
                        return JsonValue.Create(hora.ToString("HH:mm", CultureInfo.InvariantCulture));
                    }

                case TiposCampoPauta.Numero:
                    {
                        string? s = Cadena(v);
                        if (string.IsNullOrEmpty(s)) return null;

                        if (!decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) || n < 0 || n > 1000000)
                        {
                            errores.Add($"{etiqueta}: el número no es válido.");
                            return null;
                        }
                        return JsonValue.Create(n);
                    }

                case TiposCampoPauta.SiNo:
                    {
                        string? s = Cadena(v);
                        if (string.IsNullOrEmpty(s)) return null;

                        if (s.Equals("si", StringComparison.OrdinalIgnoreCase) || s.Equals("sí", StringComparison.OrdinalIgnoreCase))
                            return JsonValue.Create("Si");
                        if (s.Equals("no", StringComparison.OrdinalIgnoreCase))
                            return JsonValue.Create("No");

                        errores.Add($"{etiqueta}: debe ser Sí o No.");
                        return null;
                    }

                case TiposCampoPauta.Casilla:
                    return JsonValue.Create(EsVerdadero(v));

                case TiposCampoPauta.Seleccion:
                    {
                        string? s = Cadena(v);
                        if (string.IsNullOrEmpty(s)) return null;

                        if (c.Opciones == null || !c.Opciones.Any(o => o.Valor == s))
                        {
                            errores.Add($"{etiqueta}: la opción seleccionada no es válida.");
                            return null;
                        }
                        return JsonValue.Create(s);
                    }

                case TiposCampoPauta.Opciones:
                    {
                        if (v is not JsonArray arreglo) return null;

                        var elegidas = new List<string>();
                        foreach (var item in arreglo)
                        {
                            string? s = item == null ? null : Cadena(item);
                            if (string.IsNullOrEmpty(s) || elegidas.Contains(s)) continue;

                            if (c.Opciones == null || !c.Opciones.Any(o => o.Valor == s))
                            {
                                errores.Add($"{etiqueta}: hay una opción que no es válida.");
                                return null;
                            }
                            elegidas.Add(s);
                        }

                        if (elegidas.Count == 0) return null;

                        var resultado = new JsonArray();
                        foreach (var o in c.Opciones!.Where(o => elegidas.Contains(o.Valor)))
                            resultado.Add(JsonValue.Create(o.Valor));
                        return resultado;
                    }

                case TiposCampoPauta.Tabla:
                    return SanitizarTabla(c, v, etiqueta, errores);

                default:
                    return null;
            }
        }

        private static JsonNode? SanitizarTabla(CampoPlantillaDTO c, JsonNode v, string etiqueta, List<string> errores)
        {
            if (v is not JsonArray filas || c.Columnas == null)
                return null;

            int maxFilas = c.MaxFilas ?? 50;
            if (filas.Count > maxFilas)
            {
                errores.Add($"{etiqueta}: no puede tener más de {maxFilas} filas.");
                return null;
            }

            var resultado = new JsonArray();
            int numero = 0;

            foreach (var fila in filas)
            {
                numero++;
                if (fila is not JsonObject entrada) continue;

                var celdas = new JsonObject();
                bool conContenido = false;

                foreach (var col in c.Columnas.Where(TieneDatos))
                {
                    if (!entrada.TryGetPropertyValue(col.Clave, out var valor)) continue;

                    var limpio = SanitizarValor(col, valor, $"{etiqueta}, fila {numero}: ", errores);
                    if (limpio == null) continue;

                    if (col.Tipo == TiposCampoPauta.Casilla && !EsVerdadero(limpio)) continue;

                    celdas[col.Clave] = limpio;
                    conContenido = true;
                }

                if (conContenido)
                    resultado.Add(celdas);
            }

            return resultado.Count > 0 ? resultado : null;
        }

        // =====================  Validación para finalizar  =====================

        /// Lista lo que falta para poder finalizar el formulario. Lista vacía = completo.
        public static List<string> Validar(PlantillaPautaDTO plantilla, string datosJson)
        {
            var errores = new List<string>();

            JsonObject datos;
            try { datos = JsonNode.Parse(datosJson) as JsonObject ?? new JsonObject(); }
            catch (JsonException) { datos = new JsonObject(); }

            foreach (var seccion in plantilla.Secciones)
            {
                if (!EsVisible(seccion.VisibleSi, datos)) continue;

                foreach (var campo in seccion.Campos)
                {
                    if (!TieneDatos(campo) || !EsVisible(campo.VisibleSi, datos)) continue;

                    datos.TryGetPropertyValue(campo.Clave, out var valor);

                    if (campo.Tipo == TiposCampoPauta.Tabla)
                    {
                        var filas = valor as JsonArray;
                        int cantidad = filas?.Count ?? 0;

                        if (campo.MinFilas.HasValue && cantidad < campo.MinFilas.Value)
                            errores.Add($"{seccion.Titulo} › {campo.Etiqueta}: agregue al menos {campo.MinFilas} fila(s).");

                        if (filas != null && campo.Columnas != null)
                        {
                            int n = 0;
                            foreach (var fila in filas)
                            {
                                n++;
                                var objeto = fila as JsonObject;
                                foreach (var col in campo.Columnas.Where(x => x.Requerido))
                                {
                                    JsonNode? celda = null;
                                    objeto?.TryGetPropertyValue(col.Clave, out celda);
                                    if (EstaVacio(celda, col.Tipo))
                                        errores.Add($"{seccion.Titulo} › {campo.Etiqueta}, fila {n}: {col.Etiqueta} es requerido.");
                                }
                            }
                        }
                    }
                    else if (campo.Requerido && EstaVacio(valor, campo.Tipo))
                    {
                        errores.Add($"{seccion.Titulo} › {campo.Etiqueta}: es requerido.");
                    }
                }
            }

            return errores;
        }

        // =====================  Valores iniciales  =====================

        /// Arma los datos con que nace un formulario: datos del centro, de la persona usuaria y de las personas estudiantes.
        public static string CrearDatosIniciales(
            PlantillaPautaDTO plantilla,
            IDictionary<string, string> precarga,
            IReadOnlyList<EstudianteInvolucradoDTO> estudiantes,
            JsonObject? informeActuacion)
        {
            var datos = new JsonObject();

            foreach (var seccion in plantilla.Secciones)
            {
                foreach (var campo in seccion.Campos)
                {
                    if (!TieneDatos(campo)) continue;

                    if (campo.Tipo == TiposCampoPauta.Tabla)
                    {
                        var filas = FilasIniciales(campo, estudiantes, informeActuacion);
                        if (filas.Count > 0)
                            datos[campo.Clave] = filas;
                    }
                    else if (campo.Origen != null
                             && precarga.TryGetValue(campo.Origen, out var valor)
                             && !string.IsNullOrWhiteSpace(valor))
                    {
                        if (campo.Tipo == TiposCampoPauta.Numero)
                        {
                            if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var n))
                                datos[campo.Clave] = JsonValue.Create(n);
                        }
                        else
                        {
                            datos[campo.Clave] = JsonValue.Create(valor);
                        }
                    }
                }
            }

            return datos.ToJsonString(OpcionesJson);
        }

        private static JsonArray FilasIniciales(CampoPlantillaDTO campo, IReadOnlyList<EstudianteInvolucradoDTO> estudiantes, JsonObject? f11)
        {
            var filas = new JsonArray();

            if (campo.Origen == "expediente_estudiantes")
            {
                foreach (var e in estudiantes)
                {
                    var fila = new JsonObject { ["nombre"] = e.NombreCompleto };
                    if (!string.IsNullOrWhiteSpace(e.Seccion)) fila["seccion"] = e.Seccion;

                    foreach (var rol in RolesEstudiantePauta.Todos)
                    {
                        string clave = "rol_" + rol.ToLowerInvariant();
                        if (e.Rol == rol && campo.Columnas != null && campo.Columnas.Any(c => c.Clave == clave))
                            fila[clave] = true;
                    }
                    filas.Add(fila);
                }
            }
            else if (campo.Origen == "f11_medidas" && f11 != null && f11["medidas_proteccion"] is JsonArray medidas)
            {
                foreach (var m in medidas)
                {
                    string? texto = m is JsonObject mo && mo["medida"] != null ? Cadena(mo["medida"]!) : null;
                    if (!string.IsNullOrWhiteSpace(texto))
                        filas.Add(new JsonObject { ["medida"] = texto });
                }
            }
            else if (campo.Origen == "f11_acciones" && f11 != null && f11["acciones_psico"] != null)
            {
                string? texto = Cadena(f11["acciones_psico"]!);
                if (!string.IsNullOrWhiteSpace(texto))
                    filas.Add(new JsonObject { ["accion"] = texto });
            }
            else if (campo.FilasIniciales != null && campo.Columnas is { Count: > 0 })
            {
                foreach (var f in campo.FilasIniciales)
                    filas.Add(new JsonObject { [campo.Columnas[0].Clave] = f });
            }

            return filas;
        }

        // =====================  Condiciones de visibilidad  =====================

        /// "clave=valor", "clave!=valor" o "clave=" (vacío). Varias condiciones se separan con ";" y deben cumplirse todas.
        public static bool EsVisible(string? condicion, JsonObject datos)
        {
            if (string.IsNullOrWhiteSpace(condicion))
                return true;

            foreach (var parte in condicion.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                bool negada = parte.Contains("!=");
                var pedazos = parte.Split(negada ? "!=" : "=", 2);
                string clave = pedazos[0].Trim();
                string esperado = pedazos.Length > 1 ? pedazos[1].Trim() : string.Empty;

                datos.TryGetPropertyValue(clave, out var nodo);
                var valores = Valores(nodo);

                bool cumple = esperado.Length == 0 ? valores.Count == 0 : valores.Contains(esperado);
                if (negada) cumple = !cumple;

                if (!cumple) return false;
            }

            return true;
        }

        private static List<string> Valores(JsonNode? nodo)
        {
            var lista = new List<string>();

            if (nodo is JsonArray arreglo)
            {
                foreach (var item in arreglo)
                {
                    string? s = item == null ? null : Cadena(item);
                    if (!string.IsNullOrEmpty(s)) lista.Add(s);
                }
            }
            else if (nodo is JsonValue valor)
            {
                if (valor.TryGetValue<bool>(out var b))
                {
                    if (b) lista.Add("true");
                }
                else
                {
                    string? s = Cadena(valor);
                    if (!string.IsNullOrEmpty(s)) lista.Add(s);
                }
            }

            return lista;
        }

        // =====================  Auxiliares  =====================

        private static bool EstaVacio(JsonNode? valor, string tipo)
        {
            if (valor == null) return true;
            if (tipo == TiposCampoPauta.Casilla) return !EsVerdadero(valor);
            if (valor is JsonArray a) return a.Count == 0;

            string? s = Cadena(valor);
            return string.IsNullOrWhiteSpace(s);
        }

        private static string? Cadena(JsonNode v)
        {
            if (v is JsonValue jv)
            {
                if (jv.TryGetValue<string>(out var s)) return s?.Trim();
                return jv.ToJsonString().Trim('"').Trim();
            }
            return null;
        }

        private static bool EsVerdadero(JsonNode v)
        {
            if (v is JsonValue jv)
            {
                if (jv.TryGetValue<bool>(out var b)) return b;
                string? s = Cadena(jv);
                return s != null && (s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1");
            }
            return false;
        }

        private static bool EsCorreo(string texto)
        {
            try
            {
                var direccion = new MailAddress(texto);
                return direccion.Address == texto;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
