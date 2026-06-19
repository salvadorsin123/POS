using System.IO;
using POS.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace POS.Services;

public class PdfService
{
    private const string NombreNegocio = "Mi Negocio POS";
    private const string Slogan = "¡Gracias por su compra!";

    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string GenerarTicket(Venta venta)
    {
        string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
        Directory.CreateDirectory(folder);
        string fileName = $"Ticket_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string filePath = Path.Combine(folder, fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A6.Portrait());
                page.Margin(10, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    // Encabezado
                    col.Item().AlignCenter().Text(NombreNegocio)
                        .Bold().FontSize(14);
                    col.Item().AlignCenter().Text(venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"))
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().AlignCenter().Text($"Ticket: {venta.NumTicket}")
                        .FontSize(8);

                    col.Item().PaddingVertical(4)
                        .LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                    // Productos
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Producto").Bold();
                            header.Cell().AlignCenter().Text("Cant").Bold();
                            header.Cell().AlignRight().Text("Subtotal").Bold();
                        });

                        foreach (var d in venta.Detalles)
                        {
                            table.Cell().Text(d.NombreProducto ?? "—");
                            table.Cell().AlignCenter().Text(d.Cantidad.ToString());
                            table.Cell().AlignRight().Text(d.Subtotal.ToString("C2"));
                        }
                    });

                    col.Item().PaddingVertical(4)
                        .LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                    // Totales
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Subtotal:");
                        row.ConstantItem(80).AlignRight()
                            .Text(venta.Subtotal.ToString("C2"));
                    });
                    if (venta.Descuento > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Descuento:");
                            row.ConstantItem(80).AlignRight()
                                .Text($"-{venta.Descuento:C2}").FontColor(Colors.Red.Medium);
                        });
                    }
                    if (venta.IVA > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("IVA (16%):");
                            row.ConstantItem(80).AlignRight()
                                .Text(venta.IVA.ToString("C2"));
                        });
                    }

                    col.Item().PaddingVertical(2)
                        .LineHorizontal(1).LineColor(Colors.Black);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL:").Bold().FontSize(12);
                        row.ConstantItem(80).AlignRight()
                            .Text(venta.Total.ToString("C2")).Bold().FontSize(12);
                    });

                    col.Item().PaddingTop(8).AlignCenter()
                        .Text(Slogan).Italic().FontColor(Colors.Grey.Medium);

                    if (!string.IsNullOrEmpty(venta.NombreCliente))
                        col.Item().AlignCenter()
                            .Text($"Cliente: {venta.NombreCliente}").FontSize(8);
                });
            });
        }).GeneratePdf(filePath);

        LoggerService.Info($"PDF generado: {filePath}");
        return filePath;
    }

    public string GenerarReporteVentas(List<ResumenVenta> resumen, DateTime desde, DateTime hasta)
    {
        string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");
        Directory.CreateDirectory(folder);
        string fileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string filePath = Path.Combine(folder, fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Text(NombreNegocio).Bold().FontSize(18);
                    col.Item().Text($"Reporte de Ventas: {desde:dd/MM/yyyy} — {hasta:dd/MM/yyyy}")
                        .FontSize(11).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Blue.Darken3).Padding(4).Text("Fecha").FontColor(Colors.White).Bold();
                        h.Cell().Background(Colors.Blue.Darken3).Padding(4).Text("Transacciones").FontColor(Colors.White).Bold();
                        h.Cell().Background(Colors.Blue.Darken3).Padding(4).Text("Ingresos").FontColor(Colors.White).Bold();
                        h.Cell().Background(Colors.Blue.Darken3).Padding(4).Text("Descuentos").FontColor(Colors.White).Bold();
                    });

                    bool alt = false;
                    foreach (var row in resumen)
                    {
                        string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        alt = !alt;
                        table.Cell().Background(bg).Padding(3).Text(row.Fecha.ToString("dd/MM/yyyy"));
                        table.Cell().Background(bg).Padding(3).AlignCenter()
                            .Text(row.TotalTransacciones.ToString());
                        table.Cell().Background(bg).Padding(3).AlignRight()
                            .Text(row.TotalIngresos.ToString("C2"));
                        table.Cell().Background(bg).Padding(3).AlignRight()
                            .Text(row.TotalDescuentos.ToString("C2"));
                    }
                });

                page.Footer().AlignRight()
                    .Text(t =>
                    {
                        t.Span("Generado: ").FontSize(8);
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                    });
            });
        }).GeneratePdf(filePath);

        return filePath;
    }
}
