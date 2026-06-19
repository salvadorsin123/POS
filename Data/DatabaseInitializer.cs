using Microsoft.Data.Sqlite;
using POS.Services;

namespace POS.Data;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        try
        {
            CreateTables();
            SeedData();
            LoggerService.Info("Base de datos inicializada correctamente.");
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al inicializar la base de datos.", ex);
            throw;
        }
    }

    private static void CreateTables()
    {
        using var conn = DatabaseService.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
PRAGMA journal_mode=WAL;
PRAGMA foreign_keys=ON;

CREATE TABLE IF NOT EXISTS TCategorias (
    IdCategoria   INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre        TEXT    NOT NULL,
    Descripcion   TEXT,
    FechaCreacion TEXT    DEFAULT (datetime('now','localtime'))
);

CREATE TABLE IF NOT EXISTS TProductos (
    IdProducto        INTEGER PRIMARY KEY AUTOINCREMENT,
    Modelo            TEXT    NOT NULL,
    Precio            REAL    NOT NULL DEFAULT 0,
    Descuento         REAL    NOT NULL DEFAULT 0,
    Cantidad          INTEGER NOT NULL DEFAULT 0,
    IdCategoria       INTEGER,
    CodigoBarras      TEXT,
    Descripcion       TEXT,
    Activo            INTEGER NOT NULL DEFAULT 1,
    FechaCreacion     TEXT    DEFAULT (datetime('now','localtime')),
    FechaActualizacion TEXT,
    FOREIGN KEY (IdCategoria) REFERENCES TCategorias(IdCategoria)
);

CREATE TABLE IF NOT EXISTS TCliente (
    IdCliente      INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre         TEXT NOT NULL,
    ApellidoPaterno TEXT,
    ApellidoMaterno TEXT,
    Correo         TEXT,
    Telefono       TEXT,
    Direccion      TEXT,
    FechaRegistro  TEXT DEFAULT (datetime('now','localtime')),
    Activo         INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS TProveedores (
    IdProveedor  INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre       TEXT NOT NULL,
    Contacto     TEXT,
    Telefono     TEXT,
    Correo       TEXT,
    Direccion    TEXT,
    RFC          TEXT,
    Activo       INTEGER NOT NULL DEFAULT 1,
    FechaRegistro TEXT DEFAULT (datetime('now','localtime'))
);

CREATE TABLE IF NOT EXISTS TProveedores_Productos (
    IdProveedorProducto INTEGER PRIMARY KEY AUTOINCREMENT,
    IdProveedor         INTEGER NOT NULL,
    IdProducto          INTEGER NOT NULL,
    PrecioCompra        REAL    NOT NULL DEFAULT 0,
    FechaAsignacion     TEXT    DEFAULT (datetime('now','localtime')),
    FOREIGN KEY (IdProveedor) REFERENCES TProveedores(IdProveedor),
    FOREIGN KEY (IdProducto)  REFERENCES TProductos(IdProducto)
);

CREATE TABLE IF NOT EXISTS TProveedores_Servicio (
    IdServicio   INTEGER PRIMARY KEY AUTOINCREMENT,
    IdProveedor  INTEGER NOT NULL,
    FechaPedido  TEXT    DEFAULT (datetime('now','localtime')),
    Total        REAL    NOT NULL DEFAULT 0,
    Estado       TEXT    NOT NULL DEFAULT 'Pendiente',
    Notas        TEXT,
    FOREIGN KEY (IdProveedor) REFERENCES TProveedores(IdProveedor)
);

CREATE TABLE IF NOT EXISTS TServicio_DetalleProductos (
    IdDetalle      INTEGER PRIMARY KEY AUTOINCREMENT,
    IdServicio     INTEGER NOT NULL,
    IdProducto     INTEGER NOT NULL,
    Cantidad       INTEGER NOT NULL DEFAULT 0,
    PrecioUnitario REAL    NOT NULL DEFAULT 0,
    FOREIGN KEY (IdServicio)   REFERENCES TProveedores_Servicio(IdServicio),
    FOREIGN KEY (IdProducto)   REFERENCES TProductos(IdProducto)
);

CREATE TABLE IF NOT EXISTS TTipoEmpleado (
    IdTipoEmpleado INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre         TEXT NOT NULL,
    Descripcion    TEXT
);

CREATE TABLE IF NOT EXISTS TEmpleados (
    IdEmpleado      INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre          TEXT NOT NULL,
    ApellidoPaterno TEXT,
    ApellidoMaterno TEXT,
    Telefono        TEXT,
    Correo          TEXT,
    Username        TEXT NOT NULL UNIQUE,
    PasswordHash    TEXT NOT NULL,
    IdTipoEmpleado  INTEGER NOT NULL DEFAULT 2,
    Salario         REAL    NOT NULL DEFAULT 0,
    NumVentas       INTEGER NOT NULL DEFAULT 0,
    Activo          INTEGER NOT NULL DEFAULT 1,
    FechaRegistro   TEXT    DEFAULT (datetime('now','localtime')),
    UltimoAcceso    TEXT,
    FOREIGN KEY (IdTipoEmpleado) REFERENCES TTipoEmpleado(IdTipoEmpleado)
);

CREATE TABLE IF NOT EXISTS TUsuarios (
    IdUsuario    INTEGER PRIMARY KEY AUTOINCREMENT,
    IdEmpleado   INTEGER NOT NULL,
    Username     TEXT    NOT NULL UNIQUE,
    PasswordHash TEXT    NOT NULL,
    Rol          TEXT    NOT NULL DEFAULT 'Cajero',
    Activo       INTEGER NOT NULL DEFAULT 1,
    FechaCreacion TEXT   DEFAULT (datetime('now','localtime')),
    FOREIGN KEY (IdEmpleado) REFERENCES TEmpleados(IdEmpleado)
);

CREATE TABLE IF NOT EXISTS TSesionesEmpleado (
    IdSesion     INTEGER PRIMARY KEY AUTOINCREMENT,
    IdEmpleado   INTEGER NOT NULL,
    FechaEntrada TEXT    DEFAULT (datetime('now','localtime')),
    FechaSalida  TEXT,
    FOREIGN KEY (IdEmpleado) REFERENCES TEmpleados(IdEmpleado)
);

CREATE TABLE IF NOT EXISTS TVenta (
    IdVenta    INTEGER PRIMARY KEY AUTOINCREMENT,
    IdEmpleado INTEGER NOT NULL,
    IdCliente  INTEGER,
    FechaVenta TEXT    DEFAULT (datetime('now','localtime')),
    Subtotal   REAL    NOT NULL DEFAULT 0,
    Descuento  REAL    NOT NULL DEFAULT 0,
    IVA        REAL    NOT NULL DEFAULT 0,
    Total      REAL    NOT NULL DEFAULT 0,
    Estado     TEXT    NOT NULL DEFAULT 'Completada',
    NumTicket  TEXT,
    FOREIGN KEY (IdEmpleado) REFERENCES TEmpleados(IdEmpleado),
    FOREIGN KEY (IdCliente)  REFERENCES TCliente(IdCliente)
);

CREATE TABLE IF NOT EXISTS TDetalleVenta (
    IdDetalleVenta INTEGER PRIMARY KEY AUTOINCREMENT,
    IdVenta        INTEGER NOT NULL,
    IdProducto     INTEGER NOT NULL,
    Cantidad       INTEGER NOT NULL DEFAULT 1,
    PrecioUnitario REAL    NOT NULL DEFAULT 0,
    Descuento      REAL    NOT NULL DEFAULT 0,
    Subtotal       REAL    NOT NULL DEFAULT 0,
    FOREIGN KEY (IdVenta)      REFERENCES TVenta(IdVenta),
    FOREIGN KEY (IdProducto)   REFERENCES TProductos(IdProducto)
);

CREATE TABLE IF NOT EXISTS TApartados (
    IdApartado     INTEGER PRIMARY KEY AUTOINCREMENT,
    IdCliente      INTEGER NOT NULL,
    IdEmpleado     INTEGER NOT NULL,
    FechaCreacion  TEXT    DEFAULT (datetime('now','localtime')),
    FechaLimite    TEXT,
    Total          REAL    NOT NULL DEFAULT 0,
    Anticipo       REAL    NOT NULL DEFAULT 0,
    SaldoPendiente REAL    NOT NULL DEFAULT 0,
    Estado         TEXT    NOT NULL DEFAULT 'Activo',
    Notas          TEXT,
    FOREIGN KEY (IdCliente)  REFERENCES TCliente(IdCliente),
    FOREIGN KEY (IdEmpleado) REFERENCES TEmpleados(IdEmpleado)
);

CREATE TABLE IF NOT EXISTS TDetalleApartado (
    IdDetalleApartado INTEGER PRIMARY KEY AUTOINCREMENT,
    IdApartado        INTEGER NOT NULL,
    IdProducto        INTEGER NOT NULL,
    Cantidad          INTEGER NOT NULL DEFAULT 1,
    PrecioUnitario    REAL    NOT NULL DEFAULT 0,
    Subtotal          REAL    NOT NULL DEFAULT 0,
    FOREIGN KEY (IdApartado)  REFERENCES TApartados(IdApartado),
    FOREIGN KEY (IdProducto)  REFERENCES TProductos(IdProducto)
);

CREATE TABLE IF NOT EXISTS TReporte (
    IdReporte        INTEGER PRIMARY KEY AUTOINCREMENT,
    Tipo             TEXT NOT NULL,
    FechaGeneracion  TEXT DEFAULT (datetime('now','localtime')),
    FechaInicio      TEXT,
    FechaFin         TEXT,
    RutaArchivo      TEXT,
    IdEmpleado       INTEGER,
    FOREIGN KEY (IdEmpleado) REFERENCES TEmpleados(IdEmpleado)
);

CREATE TABLE IF NOT EXISTS TDevoluciones (
    IdDevolucion    INTEGER PRIMARY KEY AUTOINCREMENT,
    IdVenta         INTEGER NOT NULL,
    IdEmpleado      INTEGER NOT NULL,
    FechaDevolucion TEXT    DEFAULT (datetime('now','localtime')),
    Motivo          TEXT,
    Total           REAL    NOT NULL DEFAULT 0,
    FOREIGN KEY (IdVenta)      REFERENCES TVenta(IdVenta),
    FOREIGN KEY (IdEmpleado)   REFERENCES TEmpleados(IdEmpleado)
);

CREATE TABLE IF NOT EXISTS TDetalleDevolucion (
    IdDetalleDevolucion INTEGER PRIMARY KEY AUTOINCREMENT,
    IdDevolucion        INTEGER NOT NULL,
    IdProducto          INTEGER NOT NULL,
    Cantidad            INTEGER NOT NULL DEFAULT 1,
    PrecioUnitario      REAL    NOT NULL DEFAULT 0,
    FOREIGN KEY (IdDevolucion) REFERENCES TDevoluciones(IdDevolucion),
    FOREIGN KEY (IdProducto)   REFERENCES TProductos(IdProducto)
);

CREATE TABLE IF NOT EXISTS TMovimientosCaja (
    IdMovimiento    INTEGER PRIMARY KEY AUTOINCREMENT,
    Tipo            TEXT NOT NULL,
    Monto           REAL NOT NULL DEFAULT 0,
    Descripcion     TEXT,
    FechaMovimiento TEXT DEFAULT (datetime('now','localtime')),
    IdEmpleado      INTEGER,
    FOREIGN KEY (IdEmpleado) REFERENCES TEmpleados(IdEmpleado)
);
";
        cmd.ExecuteNonQuery();
    }

    private static void SeedData()
    {
        using var conn = DatabaseService.GetConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            SeedTiposEmpleado(conn, tx);
            SeedAdminEmpleado(conn, tx);
            SeedCategorias(conn, tx);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static void SeedTiposEmpleado(SqliteConnection conn, SqliteTransaction tx)
    {
        const string sql = @"
INSERT OR IGNORE INTO TTipoEmpleado (IdTipoEmpleado, Nombre, Descripcion)
VALUES (1,'Administrador','Acceso completo al sistema'),
       (2,'Cajero','Acceso a ventas y clientes');";
        using var cmd = new SqliteCommand(sql, conn, tx);
        cmd.ExecuteNonQuery();
    }

    private static void SeedAdminEmpleado(SqliteConnection conn, SqliteTransaction tx)
    {
        const string check = "SELECT COUNT(*) FROM TEmpleados WHERE Username='admin'";
        using var checkCmd = new SqliteCommand(check, conn, tx);
        var count = (long)(checkCmd.ExecuteScalar() ?? 0L);
        if (count > 0) return;

        string hash = BCrypt.Net.BCrypt.HashPassword("admin123");

        const string insert = @"
INSERT INTO TEmpleados (Nombre,ApellidoPaterno,Username,PasswordHash,IdTipoEmpleado,Salario)
VALUES ('Administrador','Sistema','admin',@hash,1,0);";
        using var cmd = new SqliteCommand(insert, conn, tx);
        cmd.Parameters.AddWithValue("@hash", hash);
        cmd.ExecuteNonQuery();

        using var lastIdCmd = new SqliteCommand("SELECT last_insert_rowid()", conn, tx);
        long id = (long)(lastIdCmd.ExecuteScalar() ?? 0L);
        const string userInsert = @"
INSERT OR IGNORE INTO TUsuarios (IdEmpleado,Username,PasswordHash,Rol)
VALUES (@id,'admin',@hash,'Administrador');";
        using var uCmd = new SqliteCommand(userInsert, conn, tx);
        uCmd.Parameters.AddWithValue("@id", id);
        uCmd.Parameters.AddWithValue("@hash", hash);
        uCmd.ExecuteNonQuery();
    }

    private static void SeedCategorias(SqliteConnection conn, SqliteTransaction tx)
    {
        const string sql = @"
INSERT OR IGNORE INTO TCategorias (IdCategoria,Nombre,Descripcion)
VALUES (1,'General','Categoría general');";
        using var cmd = new SqliteCommand(sql, conn, tx);
        cmd.ExecuteNonQuery();
    }
}
