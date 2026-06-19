using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.Modelos.Productos;
using POS.Productos;

namespace POS.Proveedores
{
    public class ProveedoresBLL
    {
        private ProveedoresDAL proveedoresDAL = new ProveedoresDAL();
        private ProveedoresProductosDAL productosDAL = new ProveedoresProductosDAL();
        private ServiciosDAL serviciosDAL = new ServiciosDAL();
        private ProductosDAL3 productosDAL3 = new ProductosDAL3();

        public List<TProveedores> ObtenerProveedores() => proveedoresDAL.ObtenerProveedores();
        public bool InsertarProveedor(TProveedores proveedor) => proveedoresDAL.InsertarProveedor(proveedor);
        public bool ActualizarProveedor(TProveedores proveedor) => proveedoresDAL.ActualizarProveedor(proveedor);
        public bool EliminarProveedor(int id) => proveedoresDAL.EliminarProveedor(id);

        public List<TProveedores_Productos> ObtenerProductosPorProveedor(int proveedorId) =>
            productosDAL.ObtenerProductosPorProveedor(proveedorId);

        public bool AgregarProductoProveedor(TProveedores_Productos producto)
        {
            // Insertar en productos del proveedor
            bool success = productosDAL.InsertarProductoProveedor(producto);

            if (success)
            {
                // Insertar en tabla de productos sin stock
                var nuevoProducto = new tproductos
                {
                    cModelo = producto.cProducto,
                    nPrecio = (double)producto.nPrecioUnitario,
                    nDescuento = 0,
                    nCantidad = 0,
                    cTipo = "Sin categoría" // Puedes ajustar esto
                };

                productosDAL3.InsertarProducto(nuevoProducto);
            }

            return success;
        }

        public List<TProveedores_Servicio> ObtenerServicios()
        {
            return serviciosDAL.ObtenerServicios();
        }
        public List<TServicio_DetalleProductos> ObtenerDetallesServicio(int servicioId)
        {
            return serviciosDAL.ObtenerDetallesServicio(servicioId);
        }
        public int CrearPedidoProveedor(TProveedores_Servicio servicio, List<TServicio_DetalleProductos> detalles)
        {
            // Calcular deuda total
            servicio.nDeudaTotal = detalles.Sum(d =>
                productosDAL.ObtenerProducto(d.nProveedorProductoID).nPrecioUnitario * d.nCantidad);

            // Insertar servicio
            int servicioId = serviciosDAL.InsertarServicio(servicio);

            // Insertar detalles
            foreach (var detalle in detalles)
            {
                detalle.nServicioID = servicioId;
                serviciosDAL.InsertarDetalleServicio(detalle);
            }

            return servicioId;
        }

        public bool MarcarPedidoRecibido(int servicioId)
        {
            // Obtener detalles del servicio - CORRECCIÓN: Usar la instancia serviciosDAL
            var detalles = serviciosDAL.ObtenerDetallesServicio(servicioId);

            // Actualizar stock de productos
            foreach (var detalle in detalles)
            {
                var productoProveedor = productosDAL.ObtenerProducto(detalle.nProveedorProductoID);
                var producto = productosDAL3.ObtenerProductos()
                    .FirstOrDefault(p => p.cModelo == productoProveedor.cProducto);

                if (producto != null)
                {
                    producto.nCantidad += detalle.nCantidad;
                    productosDAL3.ActualizarProducto(producto);
                }
            }

            // Actualizar estado del servicio
            return serviciosDAL.ActualizarEstadoServicio(servicioId, "Completado");
        }
        public bool CancelarPedido(int servicioId)
        {
            try
            {
                // 1. Verificar que el pedido existe y está en estado cancelable
                var pedido = serviciosDAL.ObtenerServicioPorId(servicioId);
                if (pedido == null || !(pedido.cEstado == "Pendiente"))
                {
                    return false;
                }

                // 2. Opcional: Revertir anticipos si es necesario (depende de tu lógica de negocio)
                //RevertirAnticipoSiAplica(pedido);

                // 3. Actualizar estado del servicio a "Cancelado"
                return serviciosDAL.ActualizarEstadoServicio(servicioId, "Cancelado");
            }
            catch (Exception ex)
            {
                // Log del error (debes implementar tu sistema de logging)
                Console.WriteLine($"Error al cancelar pedido {servicioId}: {ex.Message}");
                return false;
            }
        }

        private void RevertirAnticipoSiAplica(TProveedores_Servicio pedido)
        {
            // Solo para pedidos con anticipo mayor a cero
            if (pedido.nAnticipo > 0)
            {
                // Aquí iría la lógica para revertir el anticipo
                // Por ejemplo, registrar una devolución en el sistema de pagos
                // Esto depende completamente de tu implementación específica
                Console.WriteLine($"Se debe revertir anticipo de {pedido.nAnticipo} para pedido {pedido.nServicioID}");
            }
        }
    }
}