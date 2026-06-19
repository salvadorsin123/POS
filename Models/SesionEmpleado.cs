namespace POS.Models;

public class SesionEmpleado
{
    public int IdSesion { get; set; }
    public int IdEmpleado { get; set; }
    public string? NombreEmpleado { get; set; }
    public DateTime FechaEntrada { get; set; } = DateTime.Now;
    public DateTime? FechaSalida { get; set; }

    public TimeSpan? Duracion => FechaSalida.HasValue
        ? FechaSalida.Value - FechaEntrada
        : null;
}
