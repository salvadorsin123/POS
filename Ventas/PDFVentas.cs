using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Windows;
using System.Collections.ObjectModel;


namespace POS.Ventas
{
    public static class PDFVentas
    {
        public static string GenerarTicketVenta(int ventaID, DateTime fecha, ClienteItem cliente, ObservableCollection<ItemCarrito> carrito, double total)
        {
            try
            {
                string nombreArchivo = $"Ticket_Venta_{ventaID}_{fecha:yyyyMMdd_HHmmss}.pdf";
                string rutaCarpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");

                if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);
                string rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);

                using (FileStream fs = new FileStream(rutaArchivo, FileMode.Create))
                {
                    // DEFINICIÓN DE TAMAÑO 58mm (164 pts) x Alto variable (ej. 500 pts)
                    // El alto se puede ajustar según la cantidad de productos
                    float anchoTicket = 164f;
                    float altoEstimado = 200f + (carrito.Count * 30f);
                    Rectangle tamañoTicket = new Rectangle(anchoTicket, altoEstimado);

                    // Márgenes mínimos (5 puntos a los lados)
                    Document doc = new Document(tamañoTicket, 5, 5, 10, 10);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    // Fuentes más pequeñas para ticket térmico
                    Font fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                    Font fontTexto = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
                    Font fontNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.BLACK);

                    // Logo (Ajustado a 60px de ancho)
                    string rutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imagenes", "Inicio_IMG", "BiciCT.png");
                    if (File.Exists(rutaLogo))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(rutaLogo);
                        logo.ScaleToFit(60f, 60f);
                        logo.Alignment = Element.ALIGN_CENTER;
                        doc.Add(logo);
                    }

                    // Encabezado
                    doc.Add(new Paragraph("TICKET DE VENTA", fontTitulo) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("------------------------------------", fontTexto));
                    doc.Add(new Paragraph($"Venta: {ventaID}", fontNegrita));
                    doc.Add(new Paragraph($"Fecha: {fecha:dd/MM/yy HH:mm}", fontTexto));
                    doc.Add(new Paragraph($"Cliente: {cliente.Nombre}", fontTexto));
                    doc.Add(new Paragraph("------------------------------------", fontTexto));

                    // Tabla de productos (Simplificada a 3 columnas para 58mm)
                    PdfPTable tabla = new PdfPTable(3);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 0.8f, 2f, 1.2f }); // Cant | Producto | Subtotal

                    // Encabezados de tabla
                    tabla.AddCell(new PdfPCell(new Phrase("Cant", fontNegrita)) { Border = Rectangle.BOTTOM_BORDER });
                    tabla.AddCell(new PdfPCell(new Phrase("Prod", fontNegrita)) { Border = Rectangle.BOTTOM_BORDER });
                    tabla.AddCell(new PdfPCell(new Phrase("Subtotal", fontNegrita)) { Border = Rectangle.BOTTOM_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                    foreach (var item in carrito)
                    {
                        tabla.AddCell(new PdfPCell(new Phrase(item.Cantidad.ToString(), fontTexto)) { Border = Rectangle.NO_BORDER });
                        tabla.AddCell(new PdfPCell(new Phrase(item.Nombre, fontTexto)) { Border = Rectangle.NO_BORDER });
                        tabla.AddCell(new PdfPCell(new Phrase($"${item.Subtotal:0.00}", fontTexto)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                    }

                    doc.Add(tabla);
                    doc.Add(new Paragraph("------------------------------------", fontTexto));

                    // Total grande
                    Paragraph pTotal = new Paragraph($"TOTAL: ${total:0.00}", fontTitulo);
                    pTotal.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(pTotal);

                    doc.Add(new Paragraph("\n¡Gracias por su compra!", fontTexto) { Alignment = Element.ALIGN_CENTER });

                    doc.Close();
                }
                return rutaArchivo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return null;
            }
        }
    }
}