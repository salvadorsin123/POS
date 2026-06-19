
	CREATE TABLE TCliente (
		nClienteID INT PRIMARY KEY IDENTITY(1,1),
		cNombre VARCHAR(255),
		cCorreo varchar(255),
		cTelefono VARCHAR(255)
	);

	CREATE TABLE TProductos (
		nProductoID INT PRIMARY KEY IDENTITY(1,1),
		cModelo VARCHAR(255),
		nPrecio MONEY NOT NULL,
		nDescuento INT DEFAULT 0,
		nCantidad INT NOT NULL,
		cTipo VARCHAR(100) NOT NULL
	);

	CREATE TABLE TProveedores (
		nProveedorID INT PRIMARY KEY IDENTITY (1,1),
		cNombreP VARCHAR(100) NOT NULL,
		cContacto VARCHAR(100),       
		cTelefono VARCHAR(20),        
		cEmail VARCHAR(100),         
		dRegistro DATE DEFAULT GETDATE()  
	);

	CREATE TABLE TProveedores_Productos(
		nProveedorProductoID INT PRIMARY KEY IDENTITY(1,1),
		nProveedorID INT NOT NULL,
		cProducto VARCHAR(100) NOT NULL,
		nPrecioUnitario MONEY,
		FOREIGN KEY (nProveedorID) REFERENCES TProveedores(nProveedorID) 
			ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT UQ_ProveedorProducto UNIQUE (nProveedorID, cProducto)
	);

	CREATE TABLE TProveedores_Servicio (
		nServicioID INT PRIMARY KEY IDENTITY(1,1),
		nProveedorID INT NOT NULL,
		nDeudaTotal MONEY NOT NULL,
		nAnticipo MONEY DEFAULT 0,
		dPedido DATE NOT NULL,
		cEstado VARCHAR(50) DEFAULT 'Pendiente'
			CHECK (cEstado IN ('Pendiente', 'Completado', 'Cancelado')),
		cObservaciones VARCHAR(500),
		FOREIGN KEY(nProveedorID) REFERENCES TProveedores(nProveedorID) ON DELETE CASCADE ON UPDATE CASCADE
	);

	CREATE TABLE TServicio_DetalleProductos (
		nServicioDetalleID INT PRIMARY KEY IDENTITY(1,1),
		nServicioID INT NOT NULL,
		nProveedorProductoID INT NOT NULL,
		nCantidad INT NOT NULL,
		FOREIGN KEY (nServicioID) REFERENCES TProveedores_Servicio(nServicioID) ON DELETE NO ACTION ON UPDATE NO ACTION,
		FOREIGN KEY (nProveedorProductoID) REFERENCES TProveedores_Productos(nProveedorProductoID) ON DELETE NO ACTION ON UPDATE CASCADE
	);

	CREATE TABLE TCategorias (
		nCategoriaID INT PRIMARY KEY IDENTITY(1,1),
		cNombre varchar(255) NOT NULL
	);

	CREATE TABLE TApartados (
		nApartadoID INT PRIMARY KEY IDENTITY(1,1),
		nClienteID INT NOT NULL,
		tFechaApartado DATETIME DEFAULT GETDATE(), -- Fecha en que se realizó el apartado
		nAnticipo MONEY NOT NULL,
		nTotalApartado MONEY NOT NULL, -- Total del apartado
		nSaldoPendiente MONEY NOT NULL,
		dFechaLimite DATE NOT NULL,
		cEstado VARCHAR(50) DEFAULT 'Pendiente', -- Estado del apartado
		FOREIGN KEY (nClienteID) REFERENCES TCliente(nClienteID) ON DELETE CASCADE ON UPDATE CASCADE
	);

	
	CREATE TABLE TDetalleApartado (
		nDetalleID INT PRIMARY KEY IDENTITY(1,1),
		nApartadoID INT NOT NULL,
		nProductoID INT NOT NULL,
		nCantidad INT NOT NULL,
		nPrecio MONEY NOT NULL,
		FOREIGN KEY (nApartadoID) REFERENCES TApartados(nApartadoID) ON DELETE CASCADE ON UPDATE CASCADE,
		FOREIGN KEY (nProductoID) REFERENCES TProductos(nProductoID) ON DELETE CASCADE ON UPDATE CASCADE
	);

CREATE TABLE TTipoEmpleado (
    nTipoID INT PRIMARY KEY IDENTITY(1,1),
    cNombre VARCHAR(50) NOT NULL,
    cDescripcion VARCHAR(255)
);

	CREATE TABLE TEmpleados (
	nEmpleadoID INT PRIMARY KEY IDENTITY(1,1),
	cNombreUsuario VARCHAR(255),
    cContrasena varchar(255),
	nSalario MONEY NOT NULL,
	nTipoID INT NOT NULL,
	nVentas INT NOT NULL,
	dUltimoLogin DATETIME NULL,
	FOREIGN KEY (nTipoID) REFERENCES TTipoEmpleado(nTipoID)
);

CREATE TABLE TSesionesEmpleado (
    nSesionID INT PRIMARY KEY IDENTITY(1,1),
    nEmpleadoID INT NOT NULL FOREIGN KEY REFERENCES TEmpleados(nEmpleadoID),
    dInicioSesion DATETIME NOT NULL DEFAULT GETDATE(),
    dFinSesion DATETIME NULL,
);

	CREATE TABLE TVenta (
		nVentaID INT PRIMARY KEY IDENTITY(1,1),
		nTotal DECIMAL(10,2) NOT NULL,
		dFecha DATE NOT NULL,
		nClienteID INT,
		nEmpleadoID INT,
		FOREIGN KEY (nClienteID) REFERENCES TCliente(nClienteID) ON DELETE CASCADE ON UPDATE CASCADE,
		FOREIGN KEY (nEmpleadoID) REFERENCES TEmpleados(nEmpleadoID) ON DELETE CASCADE ON UPDATE CASCADE
	);

	CREATE TABLE TReporte (
		nReporteID INT PRIMARY KEY IDENTITY(1,1),
		cTipoReporte VARCHAR(50) NOT NULL, -- 'VentasMes', 'ComprasProveedores', 'ProductosMovimiento', etc.
		dFechaGeneracion DATETIME DEFAULT GETDATE(),
		cPeriodo VARCHAR(20) NOT NULL, -- 'Semana', 'Mes', 'Año', 'Rango'
		dFechaInicio DATE NOT NULL,
		dFechaFin DATE NULL, -- Para rangos personalizados
		cNombreArchivo VARCHAR(255) NOT NULL, -- Nombre del archivo PDF generado
		nEmpleadoID INT NOT NULL, -- Quién generó el reporte
		FOREIGN KEY (nEmpleadoID) REFERENCES TEmpleados(nEmpleadoID)
	);

	CREATE TABLE TDetalleVenta (
		nDetalleVentaID INT PRIMARY KEY IDENTITY(1,1),
		nVentaID INT NOT NULL,
		nProductoID INT NOT NULL,
		nCantidad INT NOT NULL,
		nPrecio DECIMAL(10,2) NOT NULL,
		nSubtotal DECIMAL(10,2) NOT NULL,
		FOREIGN KEY (nVentaID) REFERENCES TVenta(nVentaID) ON DELETE CASCADE ON UPDATE CASCADE,
		FOREIGN KEY (nProductoID) REFERENCES TProductos(nProductoID)  ON DELETE CASCADE ON UPDATE CASCADE
	);

-- Insertar tipos de empleado
INSERT INTO TTipoEmpleado (cNombre, cDescripcion)
VALUES 
    ('Administrador', 'Acceso completo al sistema, puede gestionar otros usuarios'),
    ('Empleado', 'Acceso limitado, solo funciones básicas de ventas');

	-- Insertar un administrador (contraseña: Admin123)
-- Luego ejecuta el INSERT
INSERT INTO TEmpleados (cNombreUsuario, cContrasena, nSalario, nTipoID, nVentas, dUltimoLogin)
VALUES 
('admin', '5d4565b02217e1761d3b981c1f1c6878', 5000.00, 1, 0, GETDATE());

-- Insertar un empleado regular (contraseña: Empleado456)
INSERT INTO TEmpleados (cNombreUsuario, cContrasena, nSalario, nTipoID, nVentas, dUltimoLogin)
VALUES 
('jperez', '5493020a1d0393b2a106ea27bf9bbe3a', 2500.00, 2, 15, GETDATE());
-- Ejemplo de registro de sesión (opcional)

/*-----
-- Insert para TCliente
INSERT INTO TCliente (cNombre, cCorreo, cTelefono) VALUES
('Publico General', '',''),
('Ana Pérez', 'ana.perez@email.com', '461-123-4567'),
('Juan López', 'juan.lopez89@otrocorreo.net', '442-987-6543'),
('María García', 'mgarcia@miempresa.org', '55-1122-3344'),
('Carlos Rodríguez', 'carlos.r@servidor.com', '477-567-8901'),
('Sofía Martínez', 'sofi.m@digitalmail.info', '415-234-5678');

-- Insert para TProductos
INSERT INTO TProductos (cModelo, nPrecio, nDescuento, nCantidad, cTipo) VALUES
('Bicicleta de Montaña Giant Trance X', 35000, 8, 15, 'Bicicleta'),
('Bicicleta de Ruta Specialized Tarmac', 42000, 5, 10, 'Bicicleta'),
('Bicicleta Híbrida Trek FX 3', 15000, 10, 20, 'Bicicleta'),
('Bicicleta Urbana Brompton M6L', 28000, 0, 12, 'Bicicleta'),
('Bicicleta Eléctrica Cannondale Topstone Neo SL', 55000, 3, 8, 'Bicicleta');

-- 1. First, insert suppliers
INSERT INTO TProveedores (cNombreP, cContacto, cTelefono, cEmail) 
VALUES 
('Distribuidora de Bicicletas SA', 'Juan Pérez', '555-100-2000', 'ventas@bicidistribuidora.com'),
('Bicicletas Premium México', 'María García', '555-300-4000', 'contacto@bicipremium.mx'),
('Importadora Ciclista', 'Roberto López', '555-500-6000', 'info@importadoraciclista.com'),
('BiciPartes Express', 'Ana Martínez', '555-700-8000', 'ventas@bicipartes.com');

-- 2. Insert products offered by these suppliers
INSERT INTO TProveedores_Productos (nProveedorID, cProducto, nPrecioUnitario)
VALUES
-- Products from Distribuidora de Bicicletas SA (Supplier 1)
(1, 'Bicicleta de Montaña Giant Trance X', 32000),
(1, 'Bicicleta de Ruta Specialized Tarmac', 40000),
-- Products from Bicicletas Premium México (Supplier 2)
(2, 'Bicicleta Híbrida Trek FX 3', 14000),
(2, 'Bicicleta Urbana Brompton M6L', 27000),
-- Products from Importadora Ciclista (Supplier 3)
(3, 'Bicicleta Eléctrica Cannondale Topstone Neo SL', 53000),
(3, 'Bicicleta de Montaña Giant Trance X', 33000),  -- Same product from different supplier
-- Products from BiciPartes Express (Supplier 4)
(4, 'Rueda de carbono 700C', 4500),
(4, 'Frenos hidráulicos Shimano', 3200);

-- 3. Create service orders (purchase orders)
INSERT INTO TProveedores_Servicio (nProveedorID, nDeudaTotal, nAnticipo, dPedido, cEstado, cObservaciones)
VALUES
-- Order from Distribuidora de Bicicletas SA
(1, 72000, 20000, '2023-05-10', 'Completado', 'Pedido urgente para temporada alta'),
-- Order from Bicicletas Premium México
(2, 41000, 10000, '2023-05-15', 'Pendiente', 'Esperando confirmación de stock'),
-- Order from Importadora Ciclista
(3, 106000, 30000, '2023-05-18', 'Pendiente', 'Pedido especial para cliente corporativo'),
-- Order from BiciPartes Express
(4, 15400, 5000, '2023-05-20', 'Cancelado', 'Cliente canceló proyecto de reparación');

-- 4. Add line items to each service order
INSERT INTO TServicio_DetalleProductos (nServicioID, nProveedorProductoID, nCantidad)
VALUES
-- Items for Service ID 1 (Distribuidora de Bicicletas SA)
(1, 1, 1),  -- 1 x Giant Trance X
(1, 2, 1),  -- 1 x Specialized Tarmac
-- Items for Service ID 2 (Bicicletas Premium México)
(2, 3, 2),  -- 2 x Trek FX 3
(2, 4, 1),  -- 1 x Brompton M6L
-- Items for Service ID 3 (Importadora Ciclista)
(3, 5, 2),  -- 2 x Cannondale Topstone Neo SL
-- Items for Service ID 4 (BiciPartes Express - cancelled order)
(4, 7, 2),  -- 2 x Rueda de carbono
(4, 8, 2);  -- 2 x Frenos hidráulicos
-- Insert para TCategorias
INSERT INTO TCategorias (cNombre) VALUES
('Bicicleta Montaña'),
('Bicileta BMX'),
('Bicicleta Dama'),
('Bicicleta Hibrida'),
('Bicicleta Carrera');

-- Insertando un nuevo apartado para el Cliente con ID 1
INSERT INTO TApartados (nClienteID, tFechaApartado, nAnticipo, nTotalApartado, nSaldoPendiente, dFechaLimite, cEstado)
VALUES (2, GETDATE(), 50.00, 112000.00, 111950.00, '2025-05-16', 'Pendiente');

-- Insertando los detalles del apartado (asumiendo que el apartado recién insertado tiene nApartadoID = 1 y existen productos con ID 1 y 2)
INSERT INTO TDetalleApartado (nApartadoID, nProductoID, nCantidad, nPrecio) VALUES
(1, 1, 2, 35000.00), 
(1, 2, 1, 42000.00); 

SELECT a.nApartadoID, a.nClienteID, a.tFechaApartado, a.nAnticipo, a.nTotalApartado, a.nSaldoPendiente, a.dFechaLimite, a.cEstado, c.cNombre FROM TApartados a join TCliente c on a.nClienteID = c.nClienteID;

SELECT SUM(p.nPrecio * d.nCantidad) AS TotalCalculado FROM TProductos p
JOIN TDetalleApartado d ON p.nProductoID = d.nProductoID
WHERE d.nApartadoID = 1;

-- Para la tabla TVenta, primero necesitamos algunos empleados. Asumo que ya tienes la tabla TEmpleados llena. Si no, por favor, házmelo saber para agregar inserts.
-- Asumiendo que tienes empleados con nEmpleadoID 1, 2 y 3.
INSERT INTO TVenta (nTotal, dFecha, nClienteID, nEmpleadoID) VALUES
(22500, '2025-04-20', 1, 1), -- Laptop con descuento
(17100, '2025-04-21', 2, 2), -- Smartphone con descuento
(8000, '2025-04-21', 3, 1), -- Silla sin descuento
(4675, '2025-04-22', 4, 2), -- Mesa con descuento
(7000, '2025-04-23', 5, 2); -- Audífonos sin descuento



-- Insert para TDetalleVenta
INSERT INTO TDetalleVenta (nVentaID, nProductoID, nCantidad, nPrecio, nSubtotal) VALUES
(1, 1, 1, 25000.00, 22500.00), -- 10% de descuento aplicado en la venta
(2, 2, 1, 18000.00, 17100.00), -- 5% de descuento aplicado en la venta
(3, 3, 1, 8000.00, 8000.00),
(4, 4, 1, 5500.00, 4675.00),  -- 15% de descuento aplicado en la venta
(5, 5, 1, 7000.00, 7000.00);

*/---

	--select * from TProductos;
	--select * from TVenta;
	--select * from TDetalleVenta;
	--select * from TEmpleados;
	--select * from TCliente;
	--SELECT * FROM TProveedores;
	--SELECT * FROM TProveedores_Productos;
--	SELECT * FROM TProveedores_Servicio;
--	SELECT * FROM TServicio_DetalleProductos;
--	SELECT * FROM TEmpleados;
--	SELECT * FROM TSesionesEmpleado;


--	select * from TProductos;
---	select * from TCategorias;
-- SELECT 
 --                                               p.nProveedorID,
 --                                               p.cNombreP as Proveedor,
  --                                              SUM(ps.nDeudaTotal - ps.nAnticipo) as TotalComprado,
 --                                               SUM(ps.nDeudaTotal) as DeudaFinal,
--                                                COUNT(DISTINCT ps.nServicioID) as CantidadCompras
 --                                           FROM TProveedores_Servicio ps
--                                            JOIN TProveedores p ON ps.nProveedorID = p.nProveedorID
--                                            WHERE MONTH(ps.dPedido) = 05 
--                                              AND YEAR(ps.dPedido) = 2023
--                                              AND ps.cEstado != 'Cancelado'  -- Excluir pedidos cancelados
  --                                          GROUP BY p.nProveedorID, p.cNombreP
    --                                        ORDER BY TotalComprado DESC;
--
	--										                    SELECT 
      --                  pp.cProducto as NombreProducto,
       --                 SUM(sdp.nCantidad) as Cantidad,
         --               pp.nPrecioUnitario,
           --             SUM(sdp.nCantidad * pp.nPrecioUnitario) as Subtotal
             --       FROM TServicio_DetalleProductos sdp
               --     JOIN TProveedores_Productos pp ON sdp.nProveedorProductoID = pp.nProveedorProductoID
--                    JOIN TProveedores_Servicio ps ON sdp.nServicioID = ps.nServicioID
 --                   WHERE ps.nProveedorID = 2 
--                      AND MONTH(ps.dPedido) = 05 
--                      AND YEAR(ps.dPedido) = 2023
--                     AND ps.cEstado != 'Cancelado'  -- Excluir pedidos cancelados
--                    GROUP BY pp.cProducto, pp.nPrecioUnitario
 --                   ORDER BY Subtotal DESC;

--	use bicis;