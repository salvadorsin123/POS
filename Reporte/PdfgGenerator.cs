using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace POS.Reporte
{

    public class PdfGenerator
    {
        public static void GenerarReporteVentasMes(ReporteVentasMes reporte, string filePath)
        {
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            // Encabezado
            Paragraph header = new Paragraph($"REPORTE DE VENTAS - MES {reporte.Mes}/{reporte.Año}",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18));
            header.Alignment = Element.ALIGN_CENTER;
            document.Add(header);

            // Información resumen
            document.Add(new Paragraph($" "));
            document.Add(new Paragraph($"Total Ventas del Mes: {reporte.TotalVentas.ToString("C")}",
                FontFactory.GetFont(FontFactory.HELVETICA, 12)));
            document.Add(new Paragraph($"Cantidad de Ventas: {reporte.CantidadVentas}",
                FontFactory.GetFont(FontFactory.HELVETICA, 12)));
            document.Add(new Paragraph(" "));

            // Tabla de detalles
            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;
            document.Add(new Paragraph($" "));
            // Encabezados de tabla
            table.AddCell(new PdfPCell(new Phrase("Fecha",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Total Día",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Cant. Ventas",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));

            // Datos
            foreach (var detalle in reporte.Detalles)
            {
                table.AddCell(detalle.Fecha.ToShortDateString());
                table.AddCell(detalle.TotalDia.ToString("C"));
                table.AddCell(detalle.CantidadVentasDia.ToString());
            }

            document.Add(table);
            document.Close();
        }
        public static void GenerarReporteComprasProveedores(ReporteComprasProveedores reporte, string filePath)
        {
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            // Encabezado
            Paragraph header = new Paragraph($"REPORTE DE COMPRAS A PROVEEDORES - MES {reporte.Mes}/{reporte.Año}",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18));
            header.Alignment = Element.ALIGN_CENTER;
            document.Add(header);

            // Información resumen
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph($"Adeudo del Mes: {reporte.TotalCompras.ToString("C")}",
                FontFactory.GetFont(FontFactory.HELVETICA, 12)));

            // Tabla de proveedores
            foreach (var proveedor in reporte.Detalles)
            {
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph($"Proveedor: {proveedor.Proveedor}",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));
                document.Add(new Paragraph($"Total Comprado: {proveedor.DeudaFinal.ToString("C")} - Compras: {proveedor.CantidadCompras}",
                    FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                document.Add(new Paragraph(" "));

                // Tabla de productos
                if (proveedor.Productos.Count > 0)
                {
                    PdfPTable table = new PdfPTable(4);
                    table.WidthPercentage = 100;

                    // Encabezados de tabla
                    table.AddCell(new PdfPCell(new Phrase("Producto", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
                    table.AddCell(new PdfPCell(new Phrase("Cantidad", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
                    table.AddCell(new PdfPCell(new Phrase("Precio Unit.", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
                    table.AddCell(new PdfPCell(new Phrase("Subtotal", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));

                    // Datos
                    foreach (var producto in proveedor.Productos)
                    {
                        table.AddCell(producto.NombreProducto);
                        table.AddCell(producto.Cantidad.ToString());
                        table.AddCell(producto.PrecioUnitario.ToString("C"));
                        table.AddCell(producto.Subtotal.ToString("C"));
                    }

                    document.Add(table);
                }
            }

            document.Close();
        }

        public static void GenerarReporteMovimientoProductos(ReporteMovimientoProductos reporte, string filePath)
        {
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            // Encabezado
            Paragraph header = new Paragraph($"REPORTE DE MOVIMIENTO DE PRODUCTOS\n" +
                                           $"Del {reporte.FechaInicio.ToShortDateString()} al {reporte.FechaFin.ToShortDateString()}",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16));
            document.Add(new Paragraph(" "));
            header.Alignment = Element.ALIGN_CENTER;
            document.Add(header);

            // Tabla de productos
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;

            // Encabezados de tabla
            table.AddCell(new PdfPCell(new Phrase("Producto", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Modelo", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Recibidos", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Vendidos", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));
            table.AddCell(new PdfPCell(new Phrase("Stock Actual", FontFactory.GetFont(FontFactory.HELVETICA_BOLD))));

            // Datos
            foreach (var producto in reporte.Productos)
            {
                table.AddCell(producto.Producto);
                table.AddCell(producto.Modelo);
                table.AddCell(producto.CantidadRecibida.ToString());
                table.AddCell(producto.CantidadVendida.ToString());
                table.AddCell(producto.StockActual.ToString());
            }

            document.Add(table);

            // Totales
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph($"Total productos recibidos: {reporte.Productos.Sum(p => p.CantidadRecibida)}",
                FontFactory.GetFont(FontFactory.HELVETICA, 12)));
            document.Add(new Paragraph($"Total productos vendidos: {reporte.Productos.Sum(p => p.CantidadVendida)}",
                FontFactory.GetFont(FontFactory.HELVETICA, 12)));

            document.Close();
        }
    }
}
