using POS.Apartados;
using POS.Clientes;
using POS.ConexionBD;
using POS.Inicio_Sesion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace POS.Ventas
{
    public partial class Ventas : UserControl
    {
        public ObservableCollection<Producto> ProductosDisponibles { get; set; }
        public ObservableCollection<ItemCarrito> Carrito { get; set; }

        public Ventas()
        {
            InitializeComponent();

            VentaDAL productoDAL = new VentaDAL();

            List<Producto> listaProductos = productoDAL.ObtenerProductos();

            // Asignar productos disponibles desde la lista cargada
            ProductosDisponibles = new ObservableCollection<Producto>(listaProductos);
            Lista_Productos.ItemsSource = ProductosDisponibles;

            // Inicializar carrito
            Carrito = new ObservableCollection<ItemCarrito>();
            Lista_Carrito.ItemsSource = Carrito;

            // Eventos
            Txt_BuscarProducto.TextChanged += Txt_BuscarProducto_TextChanged;
            Btn_AgregarAlCarrito.Click += Btn_AgregarAlCarrito_Click;
            Btn_FinalizarVenta.Click += Btn_FinalizarVenta_Click;

            ActualizarTotal();
        }


        private void Carga_Clientes(object sender, RoutedEventArgs e)
        {
            try
            {
                Conexion cnx = new Conexion();

                using (SqlConnection connection = cnx.ObtenerConexion())
                {
                    string query = "SELECT cNombre, nClienteID, cCorreo  FROM TCliente";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Cbo_clientes.Items.Add(new ClienteItem
                        {
                            Id = Convert.ToInt32(reader["nClienteID"]),
                            Nombre = reader["cNombre"].ToString(),
                            Correo = reader["cCorreo"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Btn_AgregarNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            // Abrir ventana de gestión de clientes
            EditarCliente ventana = new EditarCliente(); // Cambia el nombre si es diferente
            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
            {
                ClientesDAL accion = new ClientesDAL();
                accion.InsertarCliente(ventana.Cliente);

                Cbo_clientes.Items.Clear();
                Carga_Clientes(null, null); // Vuelve a cargar desde la BD
            }
        }


        private void Txt_BuscarProducto_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = Txt_BuscarProducto.Text.ToLower();
            Lista_Productos.ItemsSource = ProductosDisponibles
                .Where(p => p.Nombre.ToLower().Contains(filtro));
        }

        private void AgregarProductoAlCarrito(Producto seleccionado)
        {
            if (seleccionado.Stock > 0)
            {
                var existente = Carrito.FirstOrDefault(i => i.Nombre == seleccionado.Nombre);
                if (existente != null)
                {
                    existente.Cantidad++;
                    existente.Subtotal = existente.Cantidad * existente.PrecioConDescuento;
                }
                else
                {
                    Carrito.Add(new ItemCarrito
                    {
                        ProductoID = seleccionado.ID,
                        Nombre = seleccionado.Nombre,
                        Precio = seleccionado.PrecioConDescuento,
                        PrecioConDescuento = seleccionado.PrecioConDescuento, // <- esta es clave
                        Cantidad = 1,
                        Subtotal = seleccionado.PrecioConDescuento
                    });


                }

                seleccionado.Stock--;
                Lista_Productos.Items.Refresh();
                Lista_Carrito.Items.Refresh();
                ActualizarTotal();

            }
            else
            {
                MessageBox.Show("Este producto ya no tiene stock.");
            }
        }

        // Mantén tu evento Click del botón AgregarAlCarrito, pero ahora puede llamar a la misma lógica:
        private void Btn_AgregarAlCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (Lista_Productos.SelectedItem is Producto seleccionado)
            {
                AgregarProductoAlCarrito(seleccionado);
            }
        }

        private void Lista_Productos_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Lista_Productos.SelectedItem is Producto seleccionado)
            {
                // Llama a la lógica para agregar el producto al carrito
                AgregarProductoAlCarrito(seleccionado);
            }
        }

        private void Lista_Carrito_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Lista_Carrito.SelectedItem is ItemCarrito itemAEliminar)
            {
                // Llama a la lógica para eliminar el item del carrito
                EliminarItemDelCarrito(itemAEliminar);
            }
        }

        private void EliminarItemDelCarrito(ItemCarrito itemAEliminar)
        {
            if (itemAEliminar != null)
            {
                // Encuentra el producto correspondiente en la lista de productos disponibles y actualiza el stock
                var productoParaActualizar = ProductosDisponibles.FirstOrDefault(p => p.Nombre == itemAEliminar.Nombre);
                if (productoParaActualizar != null)
                {
                    productoParaActualizar.Stock += itemAEliminar.Cantidad;
                }

                // Elimina el item del carrito
                Carrito.Remove(itemAEliminar);

                // Refresca las vistas y actualiza el total
                Lista_Productos.Items.Refresh();
                Lista_Carrito.Items.Refresh();
                ActualizarTotal();
            }
        }

        private void Btn_HacerApartado_Click(object sender, RoutedEventArgs e)
        {
            if (Cbo_clientes.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un cliente.");
                return;
            }

            if (Lista_Carrito.Items.Count == 0)
            {
                MessageBox.Show("Agrega productos al carrito.");
                return;
            }

            double total = Convert.ToDouble(
                Txt_nTotal.Content.ToString().Replace("$", "").Replace(",", ""));

            var ventana = new CrearTApartado(total);
            if (ventana.ShowDialog() == true)
            {
                var abono = ventana.Abono;
                var saldo = total - abono;
                var cliente = (ClienteItem)Cbo_clientes.SelectedItem;

                // Construir lista de detalles (List<ItemCarrito>)
                var detalles = Carrito.Select(c => new ItemCarrito
                {
                    ProductoID = c.ProductoID,
                    Cantidad = c.Cantidad,
                    Precio = c.Precio
                }).ToList();

                // Mandar directamente los parámetros como espera tu método DAL
                ApartadosDAL.InsertarApartadoConDetalles(
                    cliente.Id,
                    DateTime.Now.AddDays(30),  // Fecha límite
                    abono,
                    total,
                    saldo,
                    saldo == 0 ? "Pagado" : "Pendiente",
                    detalles
                );

                MessageBox.Show("Apartado guardado exitosamente.");
                Carrito.Clear();
                Lista_Carrito.Items.Refresh();
                ActualizarTotal();
            }
        }



        private async void Btn_FinalizarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (Carrito.Count == 0)
            {
                MessageBox.Show("Agrega productos al carrito antes de finalizar la venta.");
                return;
            }

            ClienteItem clienteSeleccionado = Cbo_clientes.SelectedItem as ClienteItem;
            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un cliente válido.");
                return;
            }

            double total = Carrito.Sum(i => i.Subtotal);
            Txt_nTotal.Content = total.ToString("0.00");

            try
            {
                Correo objLogic = new Correo();
                var ventaDAL = new VentaDAL();
                DateTime fecha = DateTime.Now;
                int clienteID = clienteSeleccionado.Id;
                string clienteCorreo = clienteSeleccionado.Correo;
                int empleadoID = Sesion.EmpleadoID;

                // Insertar venta principal
                int ventaID = ventaDAL.InsertarVenta((int)total, fecha, clienteID, empleadoID);

                // Insertar cada detalle de la venta
                foreach (var item in Carrito)
                {
                    ventaDAL.InsertarDetalleVenta(ventaID, item);
                }

                MessageBox.Show($"Venta #{ventaID} realizada el {fecha:dd/MM/yyyy}.\n" +
                                $"Total: ${total:0.00}\nGracias por tu compra!");

                // Generar PDF usando la nueva clase
                string rutaArchivo = PDFVentas.GenerarTicketVenta(ventaID, fecha, clienteSeleccionado, Carrito, total);

                if (!string.IsNullOrEmpty(rutaArchivo) || !string.IsNullOrWhiteSpace(clienteCorreo))
                {
                    // Espera para simular procesamiento
                    await Task.Delay(1000);

                    // Enviar correo con el PDF adjunto
                    string asunto = $"Confirmación de venta #{ventaID}";
                    string body = $@"
                    <h1 style='color: #0066cc;'>Gracias por su compra, {clienteSeleccionado.Nombre}</h1>
                    <p>Detalles de su pedido:</p>
                    <ul>
                        <li><strong>Número de venta:</strong> {ventaID}</li>
                        <li><strong>Fecha:</strong> {fecha:dd/MM/yyyy}</li>
                        <li><strong>Total:</strong> {total:C}</li>
                    </ul>
                    <p>Adjunto encontrará su ticket de venta en formato PDF.</p>
                    <p style='color: #666;'><em>Si tiene alguna pregunta, contáctenos respondiendo este correo.</em></p>";

                    var resultado = objLogic.enviarCorreo(clienteCorreo, asunto, body, rutaArchivo);
                    if (!string.IsNullOrEmpty(resultado)) // Solo muestra mensaje si hubo envío real
                    {
                        
                    MessageBox.Show(resultado, "Estado del envío", MessageBoxButton.OK,
                        resultado.Contains("éxito") ? MessageBoxImage.Information : MessageBoxImage.Information);
                    }


                }

                // Abrir el PDF después de guardarlo
                try
                {
                    Process.Start(new ProcessStartInfo(rutaArchivo) { UseShellExecute= true});
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo abrir el PDF automáticamente: {ex.Message}", "Error al abrir PDF", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Limpiar interfaz
                Carrito.Clear();
                Lista_Carrito.Items.Refresh();
                ActualizarTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al finalizar la venta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ActualizarTotal()
        {
            double total = Carrito.Sum(i => i.Subtotal);
            Txt_nTotal.Content = total.ToString("C");

        }

        private void Txt_BuscarProducto_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }


}

