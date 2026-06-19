using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.Ventas;

namespace POS.Ventas
{
    public class Producto
    {
        public int ID { get; set; }  // Agrega esta propiedad
        public string Nombre { get; set; }
        public double Precio { get; set; }
        public int Descuento { get; set; }
        public double PrecioConDescuento { get; set; }
        public int Stock { get; set; }
        public string cTipo { get; set; }
    }


    public class ItemCarrito
    {
        public string Nombre { get; set; }
        public double Precio { get; set; }
        public int Descuento { get; set; } 
        public double PrecioConDescuento { get; set; }
        public int Cantidad { get; set; }
        public double Subtotal { get; set; }
        public int ProductoID { get; set; } // Agrega esta propiedad
    }
    public class ClienteItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }

        public override string ToString()
        {
            return Nombre; // Esto es lo que se mostrará en el ComboBox
        }
    }

}
