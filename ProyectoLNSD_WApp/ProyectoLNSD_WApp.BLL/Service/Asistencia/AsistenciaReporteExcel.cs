using ClosedXML.Excel;
using ProyectoLNSD_WApp.BLL.DTO;

namespace ProyectoLNSD_WApp.BLL.Service.Asistencia
{
    /// Reporte de asistencia en Excel (MCAF-01-12). Separado del servicio: solo arma el archivo.
    public static class AsistenciaReporteExcel
    {
        public static byte[] Generar(List<AsistenciaResumenDTO> lista, string nombrePeriodo, AsistenciaFiltroDTO filtro)
        {
            decimal umbral = filtro.Umbral is > 0 and <= 100 ? filtro.Umbral.Value : AsistenciaReglas.UmbralSugerido;

            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add("Asistencia");

            hoja.Cell(1, 1).Value = "Sistema Académico LNSD - Reporte de asistencia";
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontSize = 14;

            hoja.Cell(2, 1).Value = $"Período: {nombrePeriodo}   |   Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
            hoja.Cell(3, 1).Value =
                $"Umbral de alerta: {umbral}%   |   Filas: {lista.Count}   |   Bajo el umbral: {lista.Count(r => r.BajoUmbral)}";

            string[] encabezados =
            {
                "Estudiante", "Carné", "Grado", "Sección", "Grupo", "Curso",
                "Presentes", "Ausentes", "Tardías", "Justificadas", "Total", "% asistencia"
            };

            const int filaEncabezado = 5;

            for (int i = 0; i < encabezados.Length; i++)
                hoja.Cell(filaEncabezado, i + 1).Value = encabezados[i];

            var rangoEncabezado = hoja.Range(filaEncabezado, 1, filaEncabezado, encabezados.Length);
            rangoEncabezado.Style.Font.Bold = true;
            rangoEncabezado.Style.Font.FontColor = XLColor.White;
            rangoEncabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#021C53");

            int fila = filaEncabezado + 1;

            foreach (var r in lista)
            {
                hoja.Cell(fila, 1).Value = TextoSeguro(r.NombreEstudiante);
                hoja.Cell(fila, 2).Value = TextoSeguro(r.Carnet);
                hoja.Cell(fila, 3).Value = TextoSeguro(r.NombreGrado);
                hoja.Cell(fila, 4).Value = TextoSeguro(r.NombreSeccion);
                hoja.Cell(fila, 5).Value = TextoSeguro(r.NombreGrupo);
                hoja.Cell(fila, 6).Value = TextoSeguro(r.NombreCurso);
                hoja.Cell(fila, 7).Value = r.Presentes;
                hoja.Cell(fila, 8).Value = r.Ausentes;
                hoja.Cell(fila, 9).Value = r.Tardias;
                hoja.Cell(fila, 10).Value = r.Justificadas;
                hoja.Cell(fila, 11).Value = r.Total;
                hoja.Cell(fila, 12).Value = r.Porcentaje / 100m;
                hoja.Cell(fila, 12).Style.NumberFormat.Format = "0.0%";

                if (r.BajoUmbral)
                    hoja.Range(fila, 1, fila, encabezados.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#FBEBEB");

                fila++;
            }

            hoja.Range(filaEncabezado, 1, fila - 1, encabezados.Length).SetAutoFilter();
            hoja.SheetView.FreezeRows(filaEncabezado);
            hoja.Columns(1, encabezados.Length).AdjustToContents(filaEncabezado, fila - 1);

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
