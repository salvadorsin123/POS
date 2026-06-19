using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using POS.ConexionBD;


namespace POS.Empleados
{
    public class templeados
    {
        public int nEmpleadoID { get; set; }
        public string cNombreUsuario { get; set; }
        public string cContrasena { get; set; }
        public decimal nSalario { get; set; }
        public string cTipo { get; set; }
        public int nVentas { get; set; }
        public DateTime? dUltimoLogin { get; set; } // Nullable DateTime para el último login


        public templeados Clonar()
        {
            return new templeados
            {
                nEmpleadoID = this.nEmpleadoID,
                cNombreUsuario = this.cNombreUsuario,
                cContrasena = this.cContrasena,
                nSalario = this.nSalario,
                nVentas = this.nVentas,
                cTipo = this.cTipo,
                // dUltimoLogin se deja como null si no se ha establecid
                // o un valor específico
                dUltimoLogin = this.dUltimoLogin

            };
        }
    }
}
