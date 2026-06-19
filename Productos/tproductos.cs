using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Modelos.Productos
{
    public class tproductos
    {
        public int nProductoID { get; set; }
        public string cModelo { get; set; }
        public double nPrecio { get; set; }
        public int nDescuento { get; set; }
        public int nCantidad { get; set; }
        public string cTipo { get; set; }
        public override string ToString()
        {
            return cTipo; // Esto es lo que se mostrará en el ComboBox
        }
    }
    public class CategoriaItem
    {
        public int Id { get; set; }
        public string cNombre { get; set; }

        public override string ToString()
        {
            return cNombre; // Esto es lo que se mostrará en el ComboBox
        }
    }
}
