/*
    Columna pedida al final del laboratorio.
    Después de ejecutarla hay que regenerar el modelo con --force.
*/
USE HotelDB;
GO

IF COL_LENGTH(N'Reserva', N'Observaciones') IS NULL
    ALTER TABLE Reserva ADD Observaciones NVARCHAR(200) NULL;
GO
