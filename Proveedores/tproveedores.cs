using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace POS.Proveedores
{
    public class TProveedores
    {
        public int nProveedorID { get; set; }
        public string cNombreP { get; set; }
        public string cContacto { get; set; }
        public string cTelefono { get; set; }
        public string cEmail { get; set; }
        public DateTime dRegistro { get; set; }
    }
    public class TProveedores_Productos
    {
        public int nProveedorProductoID { get; set; }
        public int nProveedorID { get; set; }
        public string cProducto { get; set; }
        public double nPrecioUnitario { get; set; }
    }

    public class ProveedorServicio
    {
        public int nServicioID { get; set; }
        public int nProveedorID { get; set; }
        public double nDeudaTotal { get; set; }
        public double nAnticipo { get; set; }
        public DateTime dPedido { get; set; }
        public string cEstado { get; set; }
        public string cObservaciones { get; set; }
        public string ProveedorNombre { get; set; } // Para mostrar en UI
        public List<ServicioDetalleProducto> Detalles { get; set; } = new List<ServicioDetalleProducto>();
    }

    public class TProveedores_Servicio
    {
        public int nServicioID { get; set; }
        public int nProveedorID { get; set; }
        public double nDeudaTotal { get; set; }
        public double nAnticipo { get; set; }
        public DateTime dPedido { get; set; }
        public string cEstado { get; set; }
        public string cObservaciones { get; set; }
        public string cNombreProveedor { get; set; } // Para mostrar en la UI
        public double SaldoPendiente => nDeudaTotal - nAnticipo;
        public ObservableCollection<TServicio_DetalleProductos> Detalles { get; set; } = new ObservableCollection<TServicio_DetalleProductos>();
    }
    //public class TServicio_DetalleProductos
    //{
    //    public int nServicioDetalleID { get; set; }
    //    public int nServicioID { get; set; }
    //    public int nProveedorProductoID { get; set; }
    //    public int nCantidad { get; set; }

    //    // Campos adicionales para la UI
    //    public string cProducto { get; set; }
    //    public double nPrecioUnitario { get; set; }
    //    public double nSubtotal => nPrecioUnitario * nCantidad;
    //}
    public class TServicio_DetalleProductos : INotifyPropertyChanged
    {
        public int nServicioDetalleID { get; set; }
        public int nServicioID { get; set; }
        public int nProveedorProductoID { get; set; }

        // Campos adicionales para la UI
        public string cProducto { get; set; }

        private int _nCantidad;
        private double _nPrecioUnitario;

        public int nCantidad
        {
            get => _nCantidad;
            set { _nCantidad = value; OnPropertyChanged(); OnPropertyChanged(nameof(nSubtotal)); }
        }

        public double nPrecioUnitario
        {
            get => _nPrecioUnitario;
            set { _nPrecioUnitario = value; OnPropertyChanged(); OnPropertyChanged(nameof(nSubtotal)); }
        }

        public double nSubtotal => nPrecioUnitario * nCantidad;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ServicioDetalleProducto
    {
        public int nServicioDetalleID { get; set; }
        public int nServicioID { get; set; }
        public int nProveedorProductoID { get; set; }
        public int nCantidad { get; set; }
        public string ProductoNombre { get; set; } // Para mostrar en UI
        public double PrecioUnitario { get; set; } // Para mostrar en UI
    }

    public class Producto
    {
        public int nProductoID { get; set; }
        public string cModelo { get; set; }
        public double nPrecio { get; set; }
        public int nDescuento { get; set; }
        public int nCantidad { get; set; }
        public string cTipo { get; set; }
    }

    public class Categoria
    {
        public int nCategoriaID { get; set; }
        public string cNombre { get; set; }
    }
}
