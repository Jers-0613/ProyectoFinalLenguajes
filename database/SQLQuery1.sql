--Creacion base de datos
CREATE DATABASE ProyectoLenguajes;
GO

-- Crear tablas roles y preferencias de notificacion
USE ProyectoLenguajes;
GO

CREATE TABLE Roles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE,
    descripcion VARCHAR(255),
    activo BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE PreferenciasNotificacion (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(30) NOT NULL UNIQUE
);
GO

--Crear tabla usuarios 
USE ProyectoLenguajes;
GO

CREATE TABLE Usuarios (
    id INT IDENTITY(1,1) PRIMARY KEY,
    correo VARCHAR(255) NOT NULL UNIQUE,
    telefono VARCHAR(30) NOT NULL,
    fechaNacimiento DATE NOT NULL,
    nickname VARCHAR(50) NOT NULL UNIQUE,
    passwordHash VARCHAR(255) NOT NULL,
    fotoOriginal VARCHAR(500),
    fotoModificada VARCHAR(500),
    preferenciaNotificacionId INT NOT NULL,
    rolId INT NOT NULL,
    activo BIT NOT NULL DEFAULT 1,
    fechaRegistro DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Usuarios_Preferencia
        FOREIGN KEY (preferenciaNotificacionId)
        REFERENCES PreferenciasNotificacion(id),

    CONSTRAINT FK_Usuarios_Rol
        FOREIGN KEY (rolId)
        REFERENCES Roles(id)
);
GO

--Crear tabla de bitacora paraauditorias 
USE ProyectoLenguajes;
GO

CREATE TABLE BitacoraLogin (
    id INT IDENTITY(1,1) PRIMARY KEY,
    usuarioId INT NOT NULL,
    fechaHora DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ip VARCHAR(45) NOT NULL,

    CONSTRAINT FK_BitacoraLogin_Usuario
        FOREIGN KEY (usuarioId)
        REFERENCES Usuarios(id)
);
GO

-- Instertar roles y preferencias de noti
USE ProyectoLenguajes;
GO

INSERT INTO Roles (nombre, descripcion)
VALUES
('ADMIN', 'Administrador del sistema'),
('SUPERVISOR', 'Supervisor del sistema'),
('ANALISTA', 'Analista de archivos');
GO

INSERT INTO PreferenciasNotificacion (nombre)
VALUES
('Correo electrónico'),
('WhatsApp'),
('Ambos');
GO

SELECT * FROM Roles;
GO

SELECT * FROM PreferenciasNotificacion;
GO

--Validaciones de correcta creacion 
USE ProyectoLenguajes;
GO

SELECT 
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
    'Roles',
    'PreferenciasNotificacion',
    'Usuarios',
    'BitacoraLogin'
)
ORDER BY TABLE_NAME, ORDINAL_POSITION;

--Verificar FK
USE ProyectoLenguajes;
GO

SELECT
    fk.name AS NombreClaveForanea,
    OBJECT_NAME(fk.parent_object_id) AS TablaOrigen,
    COL_NAME(fkc.parent_obj ect_id, fkc.parent_column_id) AS ColumnaOrigen,
    OBJECT_NAME(fk.referenced_object_id) AS TablaDestino,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ColumnaDestino
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc
    ON fk.object_id = fkc.constraint_object_id
ORDER BY TablaOrigen;
S
S