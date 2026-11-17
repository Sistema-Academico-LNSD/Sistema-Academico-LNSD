using ClosedXML.Excel;
using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Inventario
{
    public static class InventarioReporteExcel
    {
        private const string ColorEncabezado = "#0D6EFD";

        public static byte[] Existencias(ReporteExistenciasDTO reporte)
        {
            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add("Existencias");

            Titulo(hoja, "Sistema Académico LNSD - Reporte de existencias de inventario",
                $"Generado el {reporte.GeneradoEl:dd/MM/yyyy HH:mm}",
                $"Tipo: {reporte.Categoria}   |   Artículos: {reporte.TotalArticulos}   |   " +
                $"Bajo el mínimo: {reporte.BajoMinimo}   |   Vencidos: {reporte.Vencidos}");

            string[] encabezados =
                { "Tipo", "Artículo", "Descripción", "Disponible", "Unidad", "Mínimo", "Vencimiento", "Estado", "Alerta" };

            const int filaEncabezado = 5;
            Encabezados(hoja, filaEncabezado, encabezados);

            int fila = filaEncabezado + 1;

            foreach (var a in reporte.Articulos)
            {
                hoja.Cell(fila, 1).Value = TextoSeguro(a.Categoria);
                hoja.Cell(fila, 2).Value = TextoSeguro(a.Nombre);
                hoja.Cell(fila, 3).Value = TextoSeguro(a.Descripcion);
                hoja.Cell(fila, 4).Value = a.Cantidad;
                hoja.Cell(fila, 5).Value = TextoSeguro(a.UnidadMedida);
                hoja.Cell(fila, 6).Value = a.CantidadMinima;

                if (a.FechaVencimiento.HasValue)
                {
                    hoja.Cell(fila, 7).Value = a.FechaVencimiento.Value.ToDateTime(TimeOnly.MinValue);
                    hoja.Cell(fila, 7).Style.DateFormat.Format = "dd/MM/yyyy";
                }

                hoja.Cell(fila, 8).Value = a.Activo ? "Activo" : "Inactivo";
                hoja.Cell(fila, 9).Value = string.Join(", ",
                    new[] { a.BajoMinimo ? "Bajo el mínimo" : null, a.Vencido ? "Vencido" : null }
                        .Where(x => x != null));

                fila++;
            }

            return Cerrar(libro, hoja, filaEncabezado, fila - 1, encabezados.Length);
        }

        public static byte[] Movimientos(ReporteMovimientosDTO reporte)
        {
            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add("Movimientos");

            Titulo(hoja, "Sistema Académico LNSD - Reporte de movimientos de inventario",
                $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}   |   Período: {reporte.Desde:dd/MM/yyyy} al {reporte.Hasta:dd/MM/yyyy}",
                $"Tipo: {reporte.Categoria}   |   {reporte.Tipo}   |   Entradas: {reporte.TotalEntradas}   |   Salidas: {reporte.TotalSalidas}");

            string[] encabezados =
            {
                "Fecha", "Movimiento", "Artículo", "Tipo de inventario", "Cantidad", "Unidad",
                "Existencia resultante", "Responsable", "Cédula", "Motivo", "Registró"
            };

            const int filaEncabezado = 5;
            Encabezados(hoja, filaEncabezado, encabezados);

            int fila = filaEncabezado + 1;

            foreach (var m in reporte.Movimientos)
            {
                hoja.Cell(fila, 1).Value = m.Fecha;
                hoja.Cell(fila, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                hoja.Cell(fila, 2).Value = m.Tipo;
                hoja.Cell(fila, 3).Value = TextoSeguro(m.Articulo);
                hoja.Cell(fila, 4).Value = TextoSeguro(m.Categoria);
                hoja.Cell(fila, 5).Value = m.Cantidad;
                hoja.Cell(fila, 6).Value = TextoSeguro(m.UnidadMedida);
                hoja.Cell(fila, 7).Value = m.ExistenciaResultante;
                hoja.Cell(fila, 8).Value = TextoSeguro(m.NombreResponsable);
                hoja.Cell(fila, 9).Value = TextoSeguro(m.CedulaResponsable);
                hoja.Cell(fila, 10).Value = TextoSeguro(m.Motivo);
                hoja.Cell(fila, 11).Value = TextoSeguro(m.UsuarioRegistra);

                fila++;
            }

            return Cerrar(libro, hoja, filaEncabezado, fila - 1, encabezados.Length);
        }

        // ---- Auxiliares ----

        private static void Titulo(IXLWorksheet hoja, string titulo, string linea2, string linea3)
        {
            hoja.Cell(1, 1).Value = titulo;
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontSize = 14;
            hoja.Cell(2, 1).Value = linea2;
            hoja.Cell(3, 1).Value = linea3;
        }

        private static void Encabezados(IXLWorksheet hoja, int fila, string[] encabezados)
        {
            for (int i = 0; i < encabezados.Length; i++)
                hoja.Cell(fila, i + 1).Value = encabezados[i];

            var rango = hoja.Range(fila, 1, fila, encabezados.Length);
            rango.Style.Font.Bold = true;
            rango.Style.Font.FontColor = XLColor.White;
            rango.Style.Fill.BackgroundColor = XLColor.FromHtml(ColorEncabezado);
        }

        private static byte[] Cerrar(XLWorkbook libro, IXLWorksheet hoja, int filaEncabezado, int ultimaFila, int columnas)
        {
            if (ultimaFila >= filaEncabezado + 1)
                hoja.Range(filaEncabezado, 1, ultimaFila, columnas).SetAutoFilter();

            hoja.SheetView.FreezeRows(filaEncabezado);

            // Solo se ajusta el ancho de la tabla (no el del título, que es muy largo)
            hoja.Columns(1, columnas).AdjustToContents(filaEncabezado, Math.Max(ultimaFila, filaEncabezado));

            using var memoria = new MemoryStream();
            libro.SaveAs(memoria);
            return memoria.ToArray();
        }

        // Evita que Excel interprete como fórmula un texto escrito por un usuario (=, +, -, @)
        private static string TextoSeguro(string? texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return "=+-@".Contains(texto[0]) ? " " + texto : texto;
        }
    }
}