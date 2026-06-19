using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace POS.Proveedores
{
    /// <summary>
    /// Lógica de interacción para NuevoPedidoWindow.xaml
    /// </summary>
    public partial class NuevoPedidoWindow : Window

    {
        private ProveedoresBLL proveedoresBLL = new ProveedoresBLL();
        public TProveedores_Servicio Servicio { get; private set; }
        public List<TServicio_DetalleProductos> Detalles { get; private set; }

        private double _anticipo;
        public double Anticipo
        {
            get => _anticipo;
            set => _anticipo = value; 
        }

        private double _anticipoTotal;
        public double AnticipoTotal
        {
            get => _anticipoTotal;
            set
            {
                _anticipoTotal = value;
                ActualizarResumenPagos(); // Método manual para actualizar UI
            }
        }

        private double _totalPedido;
        public double TotalPedido
        {
            get => _totalPedido;
            set
            {
                _totalPedido = value;
                ActualizarResumenPagos(); // Método manual para actualizar UI
            }
        }

        public double SaldoPendiente => TotalPedido - AnticipoTotal;

        public NuevoPedidoWindow()
        {
            InitializeComponent();
            Detalles = new List<TServicio_DetalleProductos>();
            dgProductosPedido.ItemsSource = Detalles;

            Servicio = new TProveedores_Servicio
            {
                dPedido = DateTime.Now,
                cEstado = "Pendiente",
                nAnticipo = 0
            };

            CargarProveedores();
        }


        // Elimina ActualizarSaldoPendiente() y refactoriza ActualizarResumenPagos:
        private void ActualizarResumenPagos()
        {
            // Actualizar controles manualmente
            txtTotalPedido.Text = TotalPedido.ToString("C");
            txtAnticipoTotal.Text = AnticipoTotal.ToString("C");

            // Manejar saldo pendiente con la lógica de color
            txtSaldoPendiente.Text = SaldoPendiente.ToString("C");
            txtSaldoPendiente.Foreground = SaldoPendiente <= 0 ? Brushes.Green : Brushes.Black;

            // Opcional: Cambiar estilo si es completamente pagado
            if (SaldoPendiente <= 0)
            {
                txtSaldoPendiente.FontWeight = FontWeights.Bold;
            }
            else
            {
                txtSaldoPendiente.FontWeight = FontWeights.Normal;
            }
        }


        // Botón Abonar
        private void BtnAbonar_Click(object sender, RoutedEventArgs e)
        {
            // Validar que el anticipo sea un número válido
            if (!double.TryParse(txtAnticipo.Text, out double anticipo))
            {
                MessageBox.Show("Por favor ingrese un valor numérico válido");
                return;
            }

            // Validar que el anticipo sea positivo
            if (anticipo <= 0)
            {
                MessageBox.Show("El anticipo debe ser mayor que cero");
                return;
            }

            // Validar que no exceda el total
            if (AnticipoTotal + anticipo > TotalPedido)
            {
                MessageBox.Show("El anticipo no puede ser mayor al total del pedido");
                return;
            }

            AnticipoTotal += anticipo;
            Servicio.nAnticipo = AnticipoTotal;
            txtAnticipo.Text = "0";

            // Solo necesitas llamar a uno de los métodos ahora
            ActualizarResumenPagos();
            MessageBox.Show($"Abono de {anticipo:C} aplicado correctamente");
        }

        // Modificar BtnAceptar_Click para calcular el total
        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (Servicio.nProveedorID == 0)
            {
                MessageBox.Show("Seleccione un proveedor");
                return;
            }

            if (Detalles.Count == 0)
            {
                MessageBox.Show("Agregue al menos un producto al pedido");
                return;
            }

            // Calcular el total del pedido
            TotalPedido = Detalles.Sum(d => d.nSubtotal);
            Servicio.nDeudaTotal = TotalPedido;
            Servicio.cObservaciones = txtObservaciones.Text;

            DialogResult = true;
            Close();
        }
        private void CargarProveedores()
        {
            cbProveedores.ItemsSource = proveedoresBLL.ObtenerProveedores();
        }

        private void CbProveedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Detalles.Count > 0)
            {
                MessageBox.Show("No puede cambiar de proveedor una vez agregados productos al pedido.\n" +
                              "Elimine los productos actuales primero o cancele y cree un nuevo pedido.",
                              "Cambio de proveedor no permitido",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);

                // Revertir la selección al proveedor original
                cbProveedores.SelectedItem = proveedoresBLL.ObtenerProveedores()
                    .FirstOrDefault(p => p.nProveedorID == Servicio.nProveedorID);
                return;
            }

            if (cbProveedores.SelectedItem is TProveedores proveedor)
            {
                Servicio.nProveedorID = proveedor.nProveedorID;
                dgProductosDisponibles.ItemsSource = proveedoresBLL.ObtenerProductosPorProveedor(proveedor.nProveedorID);
            }
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductosDisponibles.SelectedItem is TProveedores_Productos producto)
            {
                if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad válida");
                    return;
                }

                var detalleExistente = Detalles.FirstOrDefault(d => d.nProveedorProductoID == producto.nProveedorProductoID);
                if (detalleExistente != null)
                {
                    detalleExistente.nCantidad += cantidad;
                }
                else
                {
                    Detalles.Add(new TServicio_DetalleProductos
                    {
                        nProveedorProductoID = producto.nProveedorProductoID,
                        nCantidad = cantidad,
                        cProducto = producto.cProducto,
                        nPrecioUnitario = producto.nPrecioUnitario
                    });
                }

                cbProveedores.IsEnabled = false;

                dgProductosPedido.ItemsSource = null;
                dgProductosPedido.ItemsSource = Detalles;
                txtCantidad.Clear();
                // Calcula el nuevo total
                CalcularTotalPedido();

            }
        }
        private void CalcularTotalPedido()
        {
            TotalPedido = Detalles.Sum(d => d.nSubtotal);
        }


        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        // Botón Limpiar Todo
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            // Confirmar antes de limpiar
            if (Detalles.Count > 0)
            {
                var result = MessageBox.Show("¿Está seguro que desea limpiar todo el pedido?",
                                           "Confirmar",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            Detalles.Clear();
            dgProductosPedido.ItemsSource = null;
            cbProveedores.IsEnabled = true;
            txtCantidad.Clear();

            // Si quieres mantener el proveedor seleccionado pero permitir cambiar:
            // dgProductosDisponibles.ItemsSource = proveedoresBLL.ObtenerProductosPorProveedor(Servicio.nProveedorID);
        }

        // Botón Eliminar Producto
        private void BtnEliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (dgProductosPedido.SelectedItem is TServicio_DetalleProductos detalle)
            {
                Detalles.Remove(detalle);

                // Refrescar la vista
                dgProductosPedido.ItemsSource = null;
                dgProductosPedido.ItemsSource = Detalles;

                // Si no quedan productos, habilitar cambio de proveedor
                if (Detalles.Count == 0)
                {
                    cbProveedores.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para eliminar", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }


}
