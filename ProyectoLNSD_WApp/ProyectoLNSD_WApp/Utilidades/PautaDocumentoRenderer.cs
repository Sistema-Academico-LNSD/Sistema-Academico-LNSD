using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ProyectoLNSD_WApp.BLL.DTO;
using ProyectoLNSD_WApp.BLL.Service.Pauta;

namespace ProyectoLNSD_WApp.Utilidades
{
    /// Convierte un formulario del MEP, con sus datos, en el documento imprimible.
    /// Se dibuja desde la misma plantilla que la pantalla: títulos, cuadros, tablas, casillas, firmas y sello.
    /// Todo texto que viene de la base de datos se codifica (HtmlEncode) antes de entrar al documento.
    public static class PautaDocumentoRenderer
    {
        public static string Renderizar(DocumentoPautaDTO documento)
        {
            var sb = new StringBuilder();

            foreach (var formulario in documento.Formularios)
                Formulario(sb, documento, formulario);

            return sb.ToString();
        }

        // =====================  Formulario completo  =====================

        private static void Formulario(StringBuilder sb, DocumentoPautaDTO doc, FormularioDocumentoDTO f)
        {
            var datos = Parsear(f.Datos);
            var inst = doc.Institucion;
            bool borrador = f.Estado != EstadosFormularioPauta.Finalizado;

            sb.Append("<article class=\"formulario").Append(borrador ? " borrador" : "").Append("\">");

            if (borrador)
            {
                sb.Append("<div class=\"marca-borrador\" aria-hidden=\"true\">BORRADOR</div>");
                sb.Append("<div class=\"aviso-borrador\">BORRADOR: este formulario todavía no está finalizado.</div>");
            }

            // Encabezado con la identidad institucional configurada
            sb.Append("<header class=\"encabezado\"><div class=\"logo\">");
            if (!string.IsNullOrEmpty(inst.RutaLogo))
                sb.Append("<img src=\"").Append(E(inst.RutaLogo)).Append("\" alt=\"Logotipo\" />");
            sb.Append("</div><div class=\"institucion\"><strong>").Append(E(inst.Nombre)).Append("</strong>");

            string contacto = string.Join(" · ", new[] { inst.Direccion, Prefijo("Tel. ", inst.Telefono), inst.Correo }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            if (contacto.Length > 0) sb.Append("<br/>").Append(E(contacto));

            string codigos = string.Join(" · ", new[]
            {
                Prefijo("Código presupuestario: ", inst.CodigoPresupuestario),
                Prefijo("Circuito: ", inst.CircuitoEscolar),
                Prefijo("DRE: ", inst.Dre)
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (codigos.Length > 0) sb.Append("<br/>").Append(E(codigos));

            sb.Append("</div><div class=\"numero\">Formulario<br/><span>N°").Append(f.NumeroFormulario).Append("</span></div></header>");

            sb.Append("<h1 class=\"titulo\">").Append(E(f.Plantilla.Nombre)).Append("</h1>");
            sb.Append("<div class=\"meta\">Expediente ").Append(E(doc.CodigoExpediente))
              .Append(" · Pauta ").Append(doc.NumeroPauta).Append(". ").Append(E(doc.NombrePauta));
            if (f.Secuencia > 1) sb.Append(" · Formulario #").Append(f.Secuencia);
            sb.Append("</div>");

            foreach (var seccion in f.Plantilla.Secciones)
                Seccion(sb, seccion, datos);

            sb.Append("<footer class=\"pie\">Documento confidencial: la información debe ser tratada con confidencialidad y discreción. ")
              .Append("Generado por ").Append(E(doc.GeneradoPor)).Append(" el ")
              .Append(doc.FechaGeneracion.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)).Append(".</footer>");

            sb.Append("</article>");
        }

        // =====================  Secciones y campos  =====================

        private static void Seccion(StringBuilder sb, SeccionPlantillaDTO s, JsonObject datos)
        {
            if (!FormularioPautaHelper.EsVisible(s.VisibleSi, datos))
                return;

            sb.Append("<section class=\"seccion\"><h2>").Append(E(s.Titulo)).Append("</h2>");

            if (!string.IsNullOrWhiteSpace(s.Descripcion))
                sb.Append("<p class=\"fijo\">").Append(E(s.Descripcion)).Append("</p>");

            if (s.Presentacion == "fila")
                FilaUnica(sb, s, datos);
            else
                Campos(sb, s, datos);

            sb.Append("</section>");
        }

        private static void FilaUnica(StringBuilder sb, SeccionPlantillaDTO s, JsonObject datos)
        {
            var visibles = s.Campos.Where(c => FormularioPautaHelper.EsVisible(c.VisibleSi, datos)).ToList();

            sb.Append("<table class=\"tabla\"><thead><tr>");
            foreach (var c in visibles) sb.Append("<th>").Append(E(c.Etiqueta)).Append("</th>");
            sb.Append("</tr></thead><tbody><tr>");

            foreach (var c in visibles)
            {
                datos.TryGetPropertyValue(c.Clave, out var valor);
                sb.Append("<td>").Append(Formatear(c, valor)).Append("</td>");
            }

            sb.Append("</tr></tbody></table>");
        }

        private static void Campos(StringBuilder sb, SeccionPlantillaDTO s, JsonObject datos)
        {
            bool tablaAbierta = false;

            void CerrarTabla()
            {
                if (!tablaAbierta) return;
                sb.Append("</table>");
                tablaAbierta = false;
            }

            foreach (var c in s.Campos)
            {
                if (!FormularioPautaHelper.EsVisible(c.VisibleSi, datos))
                    continue;

                JsonNode? valor = null;
                if (!string.IsNullOrEmpty(c.Clave))
                    datos.TryGetPropertyValue(c.Clave, out valor);

                switch (c.Tipo)
                {
                    case TiposCampoPauta.TextoFijo:
                        CerrarTabla();
                        sb.Append("<p class=\"fijo\">").Append(E(c.Etiqueta)).Append("</p>");
                        break;

                    case TiposCampoPauta.Firma:
                        CerrarTabla();
                        sb.Append("<div class=\"firma\"><div class=\"linea\"></div><div class=\"etq\">").Append(E(c.Etiqueta)).Append("</div></div>");
                        break;

                    case TiposCampoPauta.Sello:
                        CerrarTabla();
                        sb.Append("<div class=\"sello\">Sello institucional</div>");
                        break;

                    case TiposCampoPauta.Tabla:
                        CerrarTabla();
                        Tabla(sb, c, valor);
                        break;

                    case TiposCampoPauta.TextoLargo:
                    case TiposCampoPauta.Opciones:
                        CerrarTabla();
                        sb.Append("<div class=\"bloque\"><div class=\"etq\">").Append(E(c.Etiqueta)).Append("</div>")
                          .Append("<div class=\"valor largo\">").Append(Formatear(c, valor)).Append("</div></div>");
                        break;

                    default:
                        if (!tablaAbierta)
                        {
                            sb.Append("<table class=\"campos\">");
                            tablaAbierta = true;
                        }
                        sb.Append("<tr><th>").Append(E(c.Etiqueta)).Append("</th><td>").Append(Formatear(c, valor)).Append("</td></tr>");
                        break;
                }
            }

            CerrarTabla();
        }

        private static void Tabla(StringBuilder sb, CampoPlantillaDTO c, JsonNode? valor)
        {
            var columnas = c.Columnas ?? new List<CampoPlantillaDTO>();

            sb.Append("<div class=\"bloque\"><div class=\"etq\">").Append(E(c.Etiqueta)).Append("</div>");
            sb.Append("<table class=\"tabla\"><thead><tr>");
            foreach (var col in columnas) sb.Append("<th>").Append(E(col.Etiqueta)).Append("</th>");
            sb.Append("</tr></thead><tbody>");

            int pintadas = 0;

            if (valor is JsonArray filas)
            {
                foreach (var fila in filas)
                {
                    if (fila is not JsonObject objeto) continue;

                    pintadas++;
                    sb.Append("<tr>");
                    foreach (var col in columnas)
                    {
                        objeto.TryGetPropertyValue(col.Clave, out var celda);
                        sb.Append("<td").Append(col.Tipo == TiposCampoPauta.Casilla ? " class=\"centro\"" : "").Append(">")
                          .Append(Formatear(col, celda)).Append("</td>");
                    }
                    sb.Append("</tr>");
                }
            }

            // Filas en blanco para completar a mano.
            for (int i = pintadas; i < 2; i++)
            {
                sb.Append("<tr>");
                foreach (var col in columnas)
                    sb.Append("<td").Append(col.Tipo == TiposCampoPauta.Casilla ? " class=\"centro\"" : "").Append(">")
                      .Append(Formatear(col, null)).Append("</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></div>");
        }

        // =====================  Valores  =====================

        private static string Formatear(CampoPlantillaDTO c, JsonNode? valor)
        {
            switch (c.Tipo)
            {
                case TiposCampoPauta.Fecha:
                    {
                        string? s = Texto(valor);
                        return DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
                            ? fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : E(s);
                    }

                case TiposCampoPauta.SiNo:
                    {
                        string? s = Texto(valor);
                        return Caja(s == "Si") + " Sí &nbsp;&nbsp; " + Caja(s == "No") + " No";
                    }

                case TiposCampoPauta.Casilla:
                    return Caja(Marcado(valor));

                case TiposCampoPauta.Seleccion:
                    {
                        string? elegido = Texto(valor);
                        return string.Join("<br/>", (c.Opciones ?? new List<OpcionPautaDTO>())
                            .Select(o => Caja(o.Valor == elegido) + " " + E(o.Texto)));
                    }

                case TiposCampoPauta.Opciones:
                    {
                        var elegidas = Lista(valor);
                        return string.Join("<br/>", (c.Opciones ?? new List<OpcionPautaDTO>())
                            .Select(o => Caja(elegidas.Contains(o.Valor)) + " " + E(o.Texto)));
                    }

                default:
                    return E(Texto(valor));
            }
        }

        private static string Caja(bool marcada) =>
            "<span class=\"caja\">" + (marcada ? "X" : "&nbsp;") + "</span>";

        private static string? Texto(JsonNode? nodo)
        {
            if (nodo is JsonValue v)
                return v.TryGetValue<string>(out var s) ? s : v.ToJsonString().Trim('"');

            return null;
        }

        private static bool Marcado(JsonNode? nodo) =>
            nodo is JsonValue v && v.TryGetValue<bool>(out var b) && b;

        private static List<string> Lista(JsonNode? nodo) =>
            nodo is JsonArray a
                ? a.Select(x => Texto(x) ?? string.Empty).Where(s => s.Length > 0).ToList()
                : new List<string>();

        private static JsonObject Parsear(string json)
        {
            try { return JsonNode.Parse(json) as JsonObject ?? new JsonObject(); }
            catch (JsonException) { return new JsonObject(); }
        }

        private static string? Prefijo(string prefijo, string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : prefijo + valor;

        private static string E(string? texto) => WebUtility.HtmlEncode(texto ?? string.Empty);
    }
}
