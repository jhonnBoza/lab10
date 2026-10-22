/*
    HotelDB
    Semana 10 - EF Core Database First
    Ejecutar en (localdb)\MSSQLLocalDB

    El enunciado indica ejecutar el script que entrega el docente.
    Este script crea el mismo esquema descrito en la guía:
    TipoHabitacion, Habitacion, Huesped y Reserva.
*/
USE master;
GO

IF DB_ID(N'HotelDB') IS NOT NULL
BEGIN
    ALTER DATABASE HotelDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HotelDB;
END
GO

CREATE DATABASE HotelDB;
GO

USE HotelDB;
GO

CREATE TABLE TipoHabitacion
(
    TipoHabitacionId  INT IDENTITY(1, 1) NOT NULL,
    Nombre            NVARCHAR(80)       NOT NULL,
    PrecioNoche       DECIMAL(10, 2)     NOT NULL,
    CONSTRAINT PK_TipoHabitacion PRIMARY KEY (TipoHabitacionId),
    CONSTRAINT CK_TipoHabitacion_PrecioNoche CHECK (PrecioNoche > 0)
);
GO

CREATE TABLE Habitacion
(
    HabitacionId      INT IDENTITY(1, 1) NOT NULL,
    Numero            INT                NOT NULL,
    TipoHabitacionId  INT                NOT NULL,
    Activo            BIT                NOT NULL CONSTRAINT DF_Habitacion_Activo DEFAULT (1),
    CONSTRAINT PK_Habitacion PRIMARY KEY (HabitacionId),
    CONSTRAINT UQ_Habitacion_Numero UNIQUE (Numero),
    CONSTRAINT FK_Habitacion_TipoHabitacion FOREIGN KEY (TipoHabitacionId) REFERENCES TipoHabitacion (TipoHabitacionId)
);
GO

CREATE TABLE Huesped
(
    HuespedId   INT IDENTITY(1, 1) NOT NULL,
    Documento   NVARCHAR(15)       NOT NULL,
    Nombres     NVARCHAR(80)       NOT NULL,
    Apellidos   NVARCHAR(80)       NOT NULL,
    Correo      NVARCHAR(120)      NULL,
    Activo      BIT                NOT NULL CONSTRAINT DF_Huesped_Activo DEFAULT (1),
    CONSTRAINT PK_Huesped PRIMARY KEY (HuespedId),
    CONSTRAINT UQ_Huesped_Documento UNIQUE (Documento)
);
GO

CREATE TABLE Reserva
(
    ReservaId     INT IDENTITY(1, 1) NOT NULL,
    HuespedId     INT                NOT NULL,
    HabitacionId  INT                NOT NULL,
    FechaIngreso  DATE               NOT NULL,
    FechaSalida   DATE               NOT NULL,
    Estado        NVARCHAR(20)       NOT NULL,
    CONSTRAINT PK_Reserva PRIMARY KEY (ReservaId),
    CONSTRAINT FK_Reserva_Huesped FOREIGN KEY (HuespedId) REFERENCES Huesped (HuespedId),
    CONSTRAINT FK_Reserva_Habitacion FOREIGN KEY (HabitacionId) REFERENCES Habitacion (HabitacionId),
    CONSTRAINT CK_Reserva_Fechas CHECK (FechaSalida > FechaIngreso),
    CONSTRAINT CK_Reserva_Estado CHECK (Estado IN (N'Pendiente', N'Confirmada', N'Cancelada', N'Finalizada'))
);
GO

INSERT INTO TipoHabitacion (Nombre, PrecioNoche) VALUES
(N'Individual', 120.00),
(N'Doble', 180.00),
(N'Suite', 320.00);
GO

INSERT INTO Habitacion (Numero, TipoHabitacionId, Activo) VALUES
(101, 1, 1),
(102, 1, 1),
(201, 2, 1),
(202, 2, 1),
(301, 3, 1),
(302, 3, 1),
(303, 3, 0);
GO

INSERT INTO Huesped (Documento, Nombres, Apellidos, Correo, Activo) VALUES
(N'45678901', N'Ana', N'Torres', N'ana.torres@correo.com', 1),
(N'12345678', N'Luis', N'Pérez', N'luis.perez@correo.com', 1),
(N'87654321', N'María', N'Quispe', N'maria.quispe@correo.com', 1),
(N'11223344', N'José', N'Ramírez', N'jose.ramirez@correo.com', 1),
(N'99887766', N'Carmen', N'Díaz', N'carmen.diaz@correo.com', 0);
GO

DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

/* 101 Confirmada: ocupa desde mañana por tres noches. */
INSERT INTO Reserva (HuespedId, HabitacionId, FechaIngreso, FechaSalida, Estado)
VALUES (1, 1, DATEADD(DAY, 1, @hoy), DATEADD(DAY, 4, @hoy), N'Confirmada');

/* 201 Pendiente: se puede cancelar desde el listado. */
INSERT INTO Reserva (HuespedId, HabitacionId, FechaIngreso, FechaSalida, Estado)
VALUES (2, 3, DATEADD(DAY, 10, @hoy), DATEADD(DAY, 13, @hoy), N'Pendiente');

/* 102 Cancelada: la habitación debe seguir libre en esas fechas. */
INSERT INTO Reserva (HuespedId, HabitacionId, FechaIngreso, FechaSalida, Estado)
VALUES (3, 2, DATEADD(DAY, 1, @hoy), DATEADD(DAY, 4, @hoy), N'Cancelada');

/* 301 Finalizada: no se puede cancelar y no bloquea fechas futuras. */
INSERT INTO Reserva (HuespedId, HabitacionId, FechaIngreso, FechaSalida, Estado)
VALUES (4, 5, DATEADD(DAY, -10, @hoy), DATEADD(DAY, -7, @hoy), N'Finalizada');
GO
