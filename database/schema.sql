IF DB_ID('SistemaInventario') IS NULL
    CREATE DATABASE SistemaInventario;
GO
USE SistemaInventario;
GO

CREATE TABLE Categorias (
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    Nombre             NVARCHAR(100) NOT NULL,
    Descripcion        NVARCHAR(500) NOT NULL,
    Activo             BIT           NOT NULL DEFAULT 1,
    FechaCreacion      DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    FechaModificacion  DATETIME2     NULL,
    CONSTRAINT UQ_Categorias_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Categorias_Nombre CHECK (LEN(Nombre) >= 3)
);

CREATE TABLE CategoriaFotos (
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    CategoriaId        INT           NOT NULL,
    RutaArchivo        NVARCHAR(260) NOT NULL,
    Activo             BIT           NOT NULL DEFAULT 1,
    FechaCreacion      DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    FechaModificacion  DATETIME2     NULL,
    CONSTRAINT FK_CategoriaFotos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);

CREATE TABLE Suplidores (
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    NombreEmpresa      NVARCHAR(150) NOT NULL,
    NombreContacto     NVARCHAR(100) NOT NULL,
    Telefono           NVARCHAR(12)  NOT NULL,
    Correo             NVARCHAR(150) NOT NULL,
    SitioWeb           NVARCHAR(200) NULL,
    Activo             BIT           NOT NULL DEFAULT 1,
    FechaCreacion      DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    FechaModificacion  DATETIME2     NULL,
    CONSTRAINT UQ_Suplidores_NombreEmpresa UNIQUE (NombreEmpresa)
);

CREATE TABLE Productos (
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    Nombre             NVARCHAR(150) NOT NULL,
    PrecioUnitario     DECIMAL(10,2) NOT NULL,
    CategoriaId        INT           NOT NULL,
    SuplidorId         INT           NOT NULL,
    Activo             BIT           NOT NULL DEFAULT 1,
    FechaCreacion      DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    FechaModificacion  DATETIME2     NULL,
    CONSTRAINT CK_Productos_Precio CHECK (PrecioUnitario > 0),
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id),
    CONSTRAINT FK_Productos_Suplidores FOREIGN KEY (SuplidorId)  REFERENCES Suplidores(Id)
);

CREATE INDEX IX_Productos_CategoriaId ON Productos(CategoriaId);
CREATE INDEX IX_Productos_SuplidorId  ON Productos(SuplidorId);
GO