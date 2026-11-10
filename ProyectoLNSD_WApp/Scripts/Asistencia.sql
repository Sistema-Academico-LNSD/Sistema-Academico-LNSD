/* ============================================================
   PROYECTO LNSD - MÓDULO DE CONTROL DE ASISTENCIA (MCAF-01)
   Ejecutar sobre Proyecto_LNSD_DB DESPUÉS del script base y de
   los cambios de Horarios (Horario con período, sección, grupo).

   Reemplaza la propuesta anterior (Asistencia_cambios_BD.sql):
   como Horario ya indica docente + curso + sección + grupo,
   no hace falta modificar Docente_Curso.
   ============================================================ */

USE Proyecto_LNSD_DB;
GO

/* ------------------------------------------------------------
   1. Asistencia = una CLASE del horario en una FECHA, por estudiante.
      - id_horario  -> período, curso, docente, sección (grado) y grupo
      - id_matricula -> el estudiante en ese período
      La tabla Asistencia del script base no la usa ningún código,
      así que se reemplaza sin migrar datos.
   ------------------------------------------------------------ */
DROP TABLE IF EXISTS Asistencia;
GO

CREATE TABLE Asistencia
(
    id_asistencia       INT IDENTITY(1,1) NOT NULL,
    id_horario          INT NOT NULL,
    id_matricula        INT NOT NULL,
    fecha               DATE NOT NULL,
    estado              NVARCHAR(20) NOT NULL,
    observacion         NVARCHAR(250) NULL,
    id_usuario_registra INT NOT NULL,
    fecha_registro      DATETIME2(0) NOT NULL
        CONSTRAINT DF_Asistencia_FechaRegistro DEFAULT SYSDATETIME(),
    id_usuario_modifica INT NULL,
    fecha_modificacion  DATETIME2(0) NULL,

    CONSTRAINT PK_Asistencia PRIMARY KEY (id_asistencia),

    -- Un registro por estudiante, clase y fecha
    CONSTRAINT UQ_Asistencia_Horario_Matricula_Fecha
        UNIQUE (id_horario, id_matricula, fecha),

    -- MCAF-01-03 (sin tilde, igual que los demás estados del sistema)
    CONSTRAINT CK_Asistencia_Estado
        CHECK (estado IN (N'Presente', N'Ausente', N'Tardia', N'Justificada')),

    CONSTRAINT FK_Asistencia_Horario
        FOREIGN KEY (id_horario) REFERENCES Horario(id_horario),
    CONSTRAINT FK_Asistencia_Matricula
        FOREIGN KEY (id_matricula) REFERENCES Matricula(id_matricula),
    CONSTRAINT FK_Asistencia_UsuarioRegistra
        FOREIGN KEY (id_usuario_registra) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Asistencia_UsuarioModifica
        FOREIGN KEY (id_usuario_modifica) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_Asistencia_Matricula
    ON Asistencia(id_matricula);

CREATE INDEX IX_Asistencia_Fecha
    ON Asistencia(fecha);
GO

/* ------------------------------------------------------------
   2. Cierre del registro de asistencia por período (MCAF-01-13).
      Que exista la fila = el período está cerrado.
      Tabla aparte para no tocar Periodo_Lectivo (Configuración).
   ------------------------------------------------------------ */
DROP TABLE IF EXISTS Asistencia_Cierre;
GO

CREATE TABLE Asistencia_Cierre
(
    id_periodo   INT NOT NULL,
    fecha_cierre DATETIME2(0) NOT NULL
        CONSTRAINT DF_AsistenciaCierre_Fecha DEFAULT SYSDATETIME(),
    id_usuario   INT NOT NULL,

    CONSTRAINT PK_Asistencia_Cierre PRIMARY KEY (id_periodo),
    CONSTRAINT FK_AsistenciaCierre_Periodo
        FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo),
    CONSTRAINT FK_AsistenciaCierre_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

/* ------------------------------------------------------------
   3. Módulo y permisos.
      - Docente: Ver (sus reportes), Crear (pasar lista), Editar (corregir, MCAF-01-05)
      - Administrador y Director: Ver (consulta y reportes) y Editar (cerrar período)
      - Estudiante y Encargado entran por su rol a "Mi asistencia" y solo
        ven lo propio; no necesitan permiso sobre el módulo.
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Asistencia')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Asistencia', N'Registro, consulta, reportes y cierre de la asistencia por clase.');
GO

DELETE rp
FROM Rol_Permiso rp
JOIN Modulo m ON m.id_modulo = rp.id_modulo
WHERE m.nombre = N'Asistencia';
GO

INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, p.ver, p.crear, p.editar, p.eliminar
FROM
(
    VALUES
        (N'Administrador', 1, 0, 1, 0),
        (N'Director',      1, 0, 1, 0),
        (N'Docente',       1, 1, 1, 0)
) AS p (rol, ver, crear, editar, eliminar)
JOIN Rol r ON r.nombre = p.rol
JOIN Modulo m ON m.nombre = N'Asistencia';
GO
