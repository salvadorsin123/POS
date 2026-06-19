-- 1. CLIENTES
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TCliente' AND xtype='U')
BEGIN
	CREATE TABLE TCliente (
		nClienteID INT PRIMARY KEY IDENTITY(1,1),
		cNombre VARCHAR(255),
		cCorreo varchar(255),
		cTelefono VARCHAR(255)
	);
END
GO

-- 2. PRODUCTOS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TProductos' AND xtype='U')
BEGIN
	CREATE TABLE TProductos (
		nProductoID INT PRIMARY KEY IDENTITY(1,1),
		cModelo VARCHAR(255),
		nPrecio MONEY NOT NULL,
		nDescuento INT DEFAULT 0,
		nCantidad INT NOT NULL,
		cTipo VARCHAR(100) NOT NULL
	);
END
GO

-- 3. PROVEEDORES
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TProveedores' AND xtype='U')
BEGIN
	CREATE TABLE TProveedores (
		nProveedorID INT PRIMARY KEY IDENTITY (1,1),
		cNombreP VARCHAR(100) NOT NULL,
		cContacto VARCHAR(100),       
		cTelefono VARCHAR(20),        
		cEmail VARCHAR(100),          
		dRegistro DATE DEFAULT GETDATE()  
	);
END
GO

-- 4. PROVEEDORES PRODUCTOS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TProveedores_Productos' AND xtype='U')
BEGIN
	CREATE TABLE TProveedores_Productos(
		nProveedorProductoID INT PRIMARY KEY IDENTITY(1,1),
		nProveedorID INT NOT NULL,
		cProducto VARCHAR(100) NOT NULL,
		nPrecioUnitario MONEY,
		FOREIGN KEY (nProveedorID) REFERENCES TProveedores(nProveedorID) 
			ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT UQ_ProveedorProducto UNIQUE (nProveedorID, cProducto)
	);
END
GO

-- 5. PROVEEDORES SERVICIO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TProveedores_Servicio' AND xtype='U')
BEGIN
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
END
GO

-- 6. DETALLE PRODUCTOS SERVICIO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TServicio_DetalleProductos' AND xtype='U')
BEGIN
	CREATE TABLE TServicio_DetalleProductos (
		nServicioDetalleID INT PRIMARY KEY IDENTITY(1,1),
		nServicioID INT NOT NULL,
		nProveedorProductoID INT NOT NULL,
		nCantidad INT NOT NULL,
		FOREIGN KEY (nServicioID) REFERENCES TProveedores_Servicio(nServicioID) ON DELETE NO ACTION ON UPDATE NO ACTION,
		FOREIGN KEY (nProveedorProductoID) REFERENCES TProveedores_Productos(nProveedorProductoID) ON DELETE NO ACTION ON UPDATE CASCADE
	);
END
GO

-- 7. CATEGORIAS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TCategorias' AND xtype='U')
BEGIN
	CREATE TABLE TCategorias (
		nCategoriaID INT PRIMARY KEY IDENTITY(1,1),
		cNombre varchar(255) NOT NULL
	);
END
GO

-- 8. APARTADOS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TApartados' AND xtype='U')
BEGIN
	CREATE TABLE TApartados (
		nApartadoID INT PRIMARY KEY IDENTITY(1,1),
		nClienteID INT NOT NULL,
		tFechaApartado DATETIME DEFAULT GETDATE(),
		nAnticipo MONEY NOT NULL,
		nTotalApartado MONEY NOT NULL,
		nSaldoPendiente MONEY NOT NULL,
		dFechaLimite DATE NOT NULL,
		cEstado VARCHAR(50) DEFAULT 'Pendiente',
		FOREIGN KEY (nClienteID) REFERENCES TCliente(nClienteID) ON DELETE CASCADE ON UPDATE CASCADE
	);
END
GO

-- 9. DETALLE APARTADO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TDetalleApartado' AND xtype='U')
BEGIN
	CREATE TABLE TDetalleApartado (
		nDetalleID INT PRIMARY KEY IDENTITY(1,1),
		nApartadoID INT NOT NULL,
		nProductoID INT NOT NULL,
		nCantidad INT NOT NULL,
		nPrecio MONEY NOT NULL,
		FOREIGN KEY (nApartadoID) REFERENCES TApartados(nApartadoID) ON DELETE CASCADE ON UPDATE CASCADE,
		FOREIGN KEY (nProductoID) REFERENCES TProductos(nProductoID) ON DELETE CASCADE ON UPDATE CASCADE
	);
END
GO

-- 10. TIPOS EMPLEADO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TTipoEmpleado' AND xtype='U')
BEGIN
	CREATE TABLE TTipoEmpleado (
		nTipoID INT PRIMARY KEY IDENTITY(1,1),
		cNombre VARCHAR(50) NOT NULL,
		cDescripcion VARCHAR(255)
	);
END
GO

-- 11. EMPLEADOS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TEmpleados' AND xtype='U')
BEGIN
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
END
GO

-- 12. SESIONES EMPLEADO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TSesionesEmpleado' AND xtype='U')
BEGIN
	CREATE TABLE TSesionesEmpleado (
		nSesionID INT PRIMARY KEY IDENTITY(1,1),
		nEmpleadoID INT NOT NULL FOREIGN KEY REFERENCES TEmpleados(nEmpleadoID),
		dInicioSesion DATETIME NOT NULL DEFAULT GETDATE(),
		dFinSesion DATETIME NULL,
	);
END
GO

-- 13. VENTAS
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TVenta' AND xtype='U')
BEGIN
	CREATE TABLE TVenta (
		nVentaID INT PRIMARY KEY IDENTITY(1,1),
		nTotal DECIMAL(10,2) NOT NULL,
		dFecha DATE NOT NULL,
		nClienteID INT,
		nEmpleadoID INT,
		FOREIGN KEY (nClienteID) REFERENCES TCliente(nClienteID) ON DELETE CASCADE ON UPDATE CASCADE,
		FOREIGN KEY (nEmpleadoID) REFERENCES TEmpleados(nEmpleadoID) ON DELETE CASCADE ON UPDATE CASCADE
	);
END
GO

-- 14. REPORTES
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TReporte' AND xtype='U')
BEGIN
	CREATE TABLE TReporte (
		nReporteID INT PRIMARY KEY IDENTITY(1,1),
		cTipoReporte VARCHAR(50) NOT NULL,
		dFechaGeneracion DATETIME DEFAULT GETDATE(),
		cPeriodo VARCHAR(20) NOT NULL,
		dFechaInicio DATE NOT NULL,
		dFechaFin DATE NULL,
		cNombreArchivo VARCHAR(255) NOT NULL,
		nEmpleadoID INT NOT NULL,
		FOREIGN KEY (nEmpleadoID) REFERENCES TEmpleados(nEmpleadoID)
	);
END
GO

-- 15. DETALLE VENTA
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TDetalleVenta' AND xtype='U')
BEGIN
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
END
GO

-- =============================================
-- INSERCIÓN DE DATOS INICIALES
-- =============================================

-- Insertar tipos de empleado (Solo si la tabla está vacía)
IF NOT EXISTS (SELECT TOP 1 * FROM TTipoEmpleado)
BEGIN
	INSERT INTO TTipoEmpleado (cNombre, cDescripcion)
	VALUES 
		('Administrador', 'Acceso completo al sistema, puede gestionar otros usuarios'),
		('Empleado', 'Acceso limitado, solo funciones básicas de ventas');
END
GO

-- Insertar empleados iniciales (Solo si no existen por nombre de usuario)
IF NOT EXISTS (SELECT * FROM TEmpleados WHERE cNombreUsuario = 'admin')
BEGIN
	INSERT INTO TEmpleados (cNombreUsuario, cContrasena, nSalario, nTipoID, nVentas, dUltimoLogin)
	VALUES ('admin', '5d4565b02217e1761d3b981c1f1c6878', 5000.00, 1, 0, GETDATE());
END
GO

IF NOT EXISTS (SELECT * FROM TEmpleados WHERE cNombreUsuario = 'jperez')
BEGIN
	INSERT INTO TEmpleados (cNombreUsuario, cContrasena, nSalario, nTipoID, nVentas, dUltimoLogin)
	VALUES ('jperez', '5493020a1d0393b2a106ea27bf9bbe3a', 2500.00, 2, 15, GETDATE());
END
GO