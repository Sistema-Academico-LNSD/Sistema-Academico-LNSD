/* DESCARTABLE: no va en el archivo final.
   Borra por completo la base Proyecto_LNSD_DB para volver a correr el script en limpio.
   Cierra las conexiones abiertas antes de borrarla. */

USE master;
GO

IF DB_ID(N'Proyecto_LNSD_DB') IS NOT NULL
BEGIN
    ALTER DATABASE Proyecto_LNSD_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Proyecto_LNSD_DB;
END
GO
