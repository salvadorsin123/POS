using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
    /// Lógica de interacción para Proveedores.xaml
    /// </summary>
    public partial class Proveedores : Window
    {
        private ProveedoresBLL proveedoresBLL = new ProveedoresBLL();

        public Proveedores()
        {
            InitializeComponent();
            CargarProveedores();
            CargarPedidos();
            dgPedidos.ItemsSource = proveedoresBLL.ObtenerServicios();
            dgDetallesPedido.ItemsSource = new List<TServicio_DetalleProductos>();
        }

        private void CargarProveedores()
        {
            dgProveedores.ItemsSource = proveedoresBLL.ObtenerProveedores();
        }

        private void CargarPedidos()
        {
            var pedidos = proveedoresBLL.ObtenerServicios();
            // Aquí puedes agregar el nombre del proveedor a cada pedido para mostrarlo
            dgPedidos.ItemsSource = pedidos;
        }

        private void DgProveedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProveedores.SelectedItem is TProveedores proveedor)
            {
                dgProductosProveedor.ItemsSource = proveedoresBLL.ObtenerProductosPorProveedor(proveedor.nProveedorID);
            }
        }

        private void BtnNuevoProveedor_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ProveedorFormWindow();
            if (ventana.ShowDialog() == true)
            {
                proveedoresBLL.InsertarProveedor(ventana.Proveedor);
                CargarProveedores();
            }
        }

        private void BtnEditarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (dgProveedores.SelectedItem is TProveedores proveedor)
            {
                var ventana = new ProveedorFormWindow(proveedor);
                if (ventana.ShowDialog() == true)
                {
                    proveedoresBLL.ActualizarProveedor(ventana.Proveedor);
                    CargarProveedores();
                }
            }
        }

        private void BtnEliminarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (dgProveedores.SelectedItem is TProveedores proveedor)
            {
                if (MessageBox.Show("¿Está seguro de eliminar este proveedor?", "Confirmar",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    proveedoresBLL.EliminarProveedor(proveedor.nProveedorID);
                    CargarProveedores();
                }
            }
        }

        private void BtnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            if (dgProveedores.SelectedItem is TProveedores proveedor)
            {
                var ventana = new ProductoProveedorFormWindow(proveedor.nProveedorID);
                if (ventana.ShowDialog() == true)
                {
                    proveedoresBLL.AgregarProductoProveedor(ventana.Producto);
                    dgProductosProveedor.ItemsSource = proveedoresBLL.ObtenerProductosPorProveedor(proveedor.nProveedorID);
                }
            }
        }

        private void BtnNuevoPedido_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new NuevoPedidoWindow();
            if (ventana.ShowDialog() == true)
            {
                proveedoresBLL.CrearPedidoProveedor(ventana.Servicio, ventana.Detalles);
                CargarPedidos();
            }
        }

        private void BtnMarcarRecibido_Click(object sender, RoutedEventArgs e)
        {
            if (dgPedidos.SelectedItem is TProveedores_Servicio pedido && pedido.cEstado == "Pendiente")
            {
                if (MessageBox.Show("¿Marcar este pedido como recibido?", "Confirmar",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    proveedoresBLL.MarcarPedidoRecibido(pedido.nServicioID);
                    CargarPedidos();
                }
            }
        }

        private void BtnCancelarPedido_Click(object sender, RoutedEventArgs e)
        {
            if (dgPedidos.SelectedItem is TProveedores_Servicio pedido)
            {
                var confirmacion = MessageBox.Show(
                    $"¿Cancelar el pedido #{pedido.nServicioID}? Esta acción no se puede deshacer.",
                    "Confirmar cancelación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmacion == MessageBoxResult.Yes)
                {
                    bool resultado = proveedoresBLL.CancelarPedido(pedido.nServicioID);

                    if (resultado)
                    {
                        MessageBox.Show($"Pedido #{pedido.nServicioID} cancelado correctamente",
                              "Operación exitosa",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                        // Refrescar la lista de pedidos
                        CargarPedidos();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo cancelar el pedido. Verifica que esté en estado Pendiente.",
                                      "Error");
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione un pedido para cancelar", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // Método para registrar abonos
        private void BtnRegistrarAbono_Click(object sender, RoutedEventArgs e)
        {
            if (dgPedidos.SelectedItem is TProveedores_Servicio pedido)
            {
                // Convierte el texto directamente a double para evitar problemas
                if (double.TryParse(txtMontoAbono.Text, out double monto))
                {
                    // Validar monto (todos los valores como double)
                    double saldoPendiente = pedido.nDeudaTotal - pedido.nAnticipo;
                    if (monto <= 0 || monto > saldoPendiente)
                    {
                        MessageBox.Show("Monto inválido. El máximo permitido es " + saldoPendiente.ToString("C0"));
                        return;
                    }

                    // Actualizar en BD 
                    new ServiciosDAL().RegistrarAbono(pedido.nServicioID, (double)monto);

                    // Actualizar localmente (ambos son double)
                    pedido.nAnticipo += monto;

                    // Refrescar UI manualmente
                    dgPedidos.Items.Refresh();
                    txtMontoAbono.Clear();
                    txtSaldoPendiente.Text = (pedido.nDeudaTotal - pedido.nAnticipo).ToString("C00");

                    MessageBox.Show("Abono registrado correctamente");
                }
                else
                {
                    MessageBox.Show("Ingrese un monto válido");
                }
            }
            else
            {
                MessageBox.Show("Seleccione un pedido primero");
            }
        }

        // Actualiza el método SelectionChanged
        private void DgPedidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPedidos.SelectedItem is TProveedores_Servicio pedido)
            {
                try
                {
                    // Cargar detalles
                    dgDetallesPedido.ItemsSource = proveedoresBLL.ObtenerDetallesServicio(pedido.nServicioID);

                    // Calcular y mostrar saldo
                    double saldo = pedido.nDeudaTotal - pedido.nAnticipo;
                    txtSaldoPendiente.Text = saldo.ToString("C");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar detalles: {ex.Message}");
                }
            }
            else
            {
                LimpiarControles();
            }
        }

        private void LimpiarControles()
        {
            dgDetallesPedido.ItemsSource = null;
            txtSaldoPendiente.Text = "$0.00";
        }

    }
}