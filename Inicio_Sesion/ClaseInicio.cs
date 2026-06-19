using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Inicio_Sesion
{
    public class UsuarioItem
    {
        public int Id { get; set; }
        public string usuarioNombre { get; set; }
        public static string Tipo { get; set; }

        public override string ToString()
        {
            return usuarioNombre; // Esto es lo que se mostrará en el ComboBox
        }
    }
    public static class Sesion
    {
        public static string NombreUsuario { get; set; }
        public static string Tipo { get; set; }
        public static int EmpleadoID { get; set; }
        public static void Limpiar()
        {
            EmpleadoID = 0;
            NombreUsuario = string.Empty;
            Tipo = string.Empty;
        }
    }


}
