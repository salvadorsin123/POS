using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Detalle_Venta
{
    public class tdetalle_venta
    {
        public int nDetalleVentaID { get; set; }
        public int nVentaID { get; set; }
        public int nProductoID { get; set; }
        public int nCantidad { get; set; }
        public decimal nPrecio { get; set; }
        public decimal nSubtotal { get; set; }
    }

    public class tventa
    {
        public int nVentaID { get; set; }
        public DateTime dFecha { get; set; }
        // Agrega otras propiedades necesarias
    }

    public class tproducto
    {
        public int nProductoID { get; set; }
        public string sNombre { get; set; }
        public decimal nPrecio { get; set; }
        // Agrega otras propiedades necesarias
    }
}
