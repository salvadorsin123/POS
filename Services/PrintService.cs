using System.Drawing;
using System.Drawing.Printing;
using POS.Models;

namespace POS.Services;

public class PrintService
{
    private const string NombreNegocio = "Mi Negocio POS";
    private Venta? _ventaActual;

    public void ImprimirTicket(Venta venta)
    {
        _ventaActual = venta;
        try
        {
            using var pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 315, 1000);
            pd.PrintPage += OnPrintPage;
            pd.Print();
            LoggerService.Info($"Ticket impreso: {venta.NumTicket}");
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al imprimir ticket.", ex);
            throw;
        }
    }

    private void OnPrintPage(object sender, PrintPageEventArgs e)
    {
        if (_ventaActual == null || e.Graphics == null) return;

        var g = e.Graphics;
        float x = 5f;
        float y = 5f;
        float lineH = 14f;
        float pageW = 295f;

        using var fontTitle  = new Font("Courier New", 10, FontStyle.Bold);
        using var fontBold   = new Font("Courier New", 8,  FontStyle.Bold);
        using var fontNormal = new Font("Courier New", 7,  FontStyle.Regular);
        using var fontSmall  = new Font("Courier New", 6,  FontStyle.Regular);
        var brush = Brushes.Black;

        string Center(string text, Font f)
        {
            float w = g.MeasureString(text, f).Width;
            return text;
        }

        // Encabezado
        var titleSize = g.MeasureString(NombreNegocio, fontTitle);
        g.DrawString(NombreNegocio, fontTitle, brush, (pageW - titleSize.Width) / 2, y); y += lineH + 2;

        string fecha = _ventaActual.FechaVenta.ToString("dd/MM/yyyy HH:mm");
        var fechaSize = g.MeasureString(fecha, fontNormal);
        g.DrawString(fecha, fontNormal, brush, (pageW - fechaSize.Width) / 2, y); y += lineH;

        string ticket = $"Ticket: {_ventaActual.NumTicket}";
        var ticketSize = g.MeasureString(ticket, fontNormal);
        g.DrawString(ticket, fontNormal, brush, (pageW - ticketSize.Width) / 2, y); y += lineH;

        g.DrawLine(Pens.Black, x, y, pageW + x, y); y += 4;

        // Encabezado columnas
        g.DrawString("Producto", fontBold, brush, x, y);
        g.DrawString("Cant", fontBold, brush, 180, y);
        g.DrawString("Subtotal", fontBold, brush, 225, y);
        y += lineH;
        g.DrawLine(Pens.Black, x, y, pageW + x, y); y += 4;

        // Items
        foreach (var d in _ventaActual.Detalles)
        {
            string nombre = d.NombreProducto ?? "—";
            if (nombre.Length > 22) nombre = nombre[..22];
            g.DrawString(nombre, fontNormal, brush, x, y);
            g.DrawString(d.Cantidad.ToString(), fontNormal, brush, 185, y);
            g.DrawString(d.Subtotal.ToString("C2"), fontNormal, brush, 228, y);
            y += lineH;
        }

        g.DrawLine(Pens.Black, x, y, pageW + x, y); y += 4;

        // Totales
        void PrintLine(string label, string value, Font f)
        {
            g.DrawString(label, f, brush, x, y);
            var vs = g.MeasureString(value, f);
            g.DrawString(value, f, brush, pageW - vs.Width + x, y);
            y += lineH;
        }

        PrintLine("Subtotal:", _ventaActual.Subtotal.ToString("C2"), fontNormal);
        if (_ventaActual.Descuento > 0)
            PrintLine("Descuento:", $"-{_ventaActual.Descuento:C2}", fontNormal);
        if (_ventaActual.IVA > 0)
            PrintLine("IVA (16%):", _ventaActual.IVA.ToString("C2"), fontNormal);

        g.DrawLine(Pens.Black, x, y, pageW + x, y); y += 4;
        PrintLine("TOTAL:", _ventaActual.Total.ToString("C2"), fontBold);

        y += 6;
        string gracias = "¡Gracias por su compra!";
        var gSize = g.MeasureString(gracias, fontNormal);
        g.DrawString(gracias, fontNormal, brush, (pageW - gSize.Width) / 2, y);

        e.HasMorePages = false;
    }
}
