using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Apartados
{
    //Aquí es donde le damos valores a los apartados
    public class tapartados
    {
        public int nApartadoID { get; set; }
        public int nClienteID { get; set; }
        public string cNombreCliente { get; set; } // Nombre del cliente
        public DateTime tFechaApartado { get; set; }
        public double nAnticipo { get; set; }
        public double nTotalApartado { get; set; }
        public double nSaldoPendiente { get; set; }
        public DateTime dFechaLimite { get; set; }
        public string cEstado { get; set; }

        // Agregar la propiedad Detalles para los productos asociados al apartado
        public List<TDetalleApartado> Detalles { get; set; } = new List<TDetalleApartado>();
    }

    public class TDetalleApartado
    {
        public int nDetalleID { get; set; }
        public int nApartadoID { get; set; }
        public int nProductoID { get; set; }
        public int nCantidad { get; set; }
        public double nPrecio { get; set; }
    }


}

