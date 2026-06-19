namespace POS.Models;

public class Empleado
{
    public int IdEmpleado { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int IdTipoEmpleado { get; set; } = 2;
    public string? NombreTipo { get; set; }
    public decimal Salario { get; set; }
    public int NumVentas { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? UltimoAcceso { get; set; }

    public string NombreCompleto =>
        $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

    public bool EsAdministrador => IdTipoEmpleado == 1;

    public override string ToString() => NombreCompleto;
}
