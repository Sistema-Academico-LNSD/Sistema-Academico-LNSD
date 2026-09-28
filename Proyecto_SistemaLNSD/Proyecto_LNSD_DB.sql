 /* ---- Creación DB y Control ---- */

USE master;
GO

IF DB_ID(N'Proyecto_LNSD_DB') IS NOT NULL
BEGIN
    ALTER DATABASE Proyecto_LNSD_DB
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;

    DROP DATABASE Proyecto_LNSD_DB;
END
GO

CREATE DATABASE Proyecto_LNSD_DB;
GO

USE Proyecto_LNSD_DB;
GO


/* ---- Seguridad y Usuarios ---- */

CREATE TABLE Rol
(
    id_rol INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(50) NOT NULL,
    descripcion NVARCHAR(250) NULL,
    CONSTRAINT PK_Rol PRIMARY KEY (id_rol),
    CONSTRAINT UQ_Rol_Nombre UNIQUE (nombre)
);
GO

CREATE TABLE Usuario
(
    id_usuario INT IDENTITY(1,1) NOT NULL,
    id_rol INT NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    apellido NVARCHAR(100) NOT NULL,
    correo NVARCHAR(150) NOT NULL,
    password_hash VARBINARY(256) NOT NULL,
    estado BIT NOT NULL CONSTRAINT DF_Usuario_Estado DEFAULT 1,
    CONSTRAINT PK_Usuario PRIMARY KEY (id_usuario),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (correo),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (id_rol) REFERENCES Rol(id_rol)
);
GO

/* ---- Personal Administrativo ---- */

CREATE TABLE Personal_Admin
(
    id_perAdm INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    cargo NVARCHAR(100) NOT NULL,
    departamento NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_Personal_Admin PRIMARY KEY (id_perAdm),
    CONSTRAINT UQ_Personal_Admin_Usuario UNIQUE (id_usuario),
    CONSTRAINT FK_Personal_Admin_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO


/* ---- Docentes ---- */

CREATE TABLE Docente
(
    id_docente INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    especialidad NVARCHAR(150) NULL,
    CONSTRAINT PK_Docente PRIMARY KEY (id_docente),
    CONSTRAINT UQ_Docente_Usuario UNIQUE (id_usuario),
    CONSTRAINT FK_Docente_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

/* ---- Estudiantes ---- */

CREATE TABLE Estudiante 
(
    id_estudiante INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    identificacion NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_Estudiante PRIMARY KEY (id_estudiante),
    CONSTRAINT UQ_Estudiante_Usuario UNIQUE (id_usuario),
    CONSTRAINT UQ_Estudiante_Identificacion UNIQUE (identificacion),
    CONSTRAINT FK_Estudiante_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

-- TABLA ESTUDIANTE se complementa con TABLA INTERMEDIA "Estudiante_Docente" para correcta relación N:M --


/* ---- Encargados ---- */
-- TABLA ENCARGADO se complementa con TABLA INTERMEDIA "Encargado_Estudiante" para correcta relación N:M --

CREATE TABLE Encargado
(
    id_encargado INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    CONSTRAINT PK_Encargado PRIMARY KEY(id_encargado),
    CONSTRAINT UQ_Encargado_Usuario UNIQUE(id_usuario),
    CONSTRAINT FK_Encargado_Usuario FOREIGN KEY(id_usuario) REFERENCES Usuario(id_usuario)
);
GO

/* ---- Becas ---- */
-- Se agregan nuevas columnas con características necesarias para implementar en APP WEB --

CREATE TABLE Beca
(
    id_beca INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    porcentaje DECIMAL(5,2),
	fecha_inicio DATE,
	fecha_fin DATE,
	descripcion NVARCHAR(250),
    CONSTRAINT PK_Beca PRIMARY KEY (id_beca),
    CONSTRAINT FK_Beca_Estudiante FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante)
);
GO


/* ---- Cursos ---- */

CREATE TABLE Curso
(
    id_curso INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    CONSTRAINT PK_Curso PRIMARY KEY (id_curso)
);
GO

/* ---- Grupos ---- */
-- Se agregan nuevas columnas con características necesarias para implementar en APP WEB --

CREATE TABLE Grupo
(
    id_grupo INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(50) NOT NULL,
    nivel NVARCHAR(50) NOT NULL,
    anio INT NOT NULL,
    CONSTRAINT PK_Grupo PRIMARY KEY (id_grupo),
    CONSTRAINT UQ_Grupo_Nombre_Anio UNIQUE (nombre, anio)
);
GO


/* ---- Matrículas ---- */

CREATE TABLE Matricula
(
    id_matricula INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    id_grupo INT NOT NULL,
    CONSTRAINT PK_Matricula PRIMARY KEY (id_matricula),
    CONSTRAINT UQ_Matricula_Estudiante_Grupo UNIQUE (id_estudiante, id_grupo),
    CONSTRAINT FK_Matricula_Estudiante FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Matricula_Grupo FOREIGN KEY (id_grupo) REFERENCES Grupo(id_grupo)
);
GO

/* ---- Horarios ---- */

CREATE TABLE Horario
(
    id_horario INT IDENTITY(1,1) NOT NULL,
    id_docente INT NOT NULL,
    id_curso INT NOT NULL,
    dia NVARCHAR(15) NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    CONSTRAINT PK_Horario PRIMARY KEY (id_horario),
    CONSTRAINT FK_Horario_Docente FOREIGN KEY (id_docente) REFERENCES Docente(id_docente),
    CONSTRAINT FK_Horario_Curso FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT UQ_Horario_Docente_Dia_Hora UNIQUE(id_docente, dia, hora_inicio, hora_fin)
);
GO

/* ---- Asistencia ---- */

CREATE TABLE Asistencia
(
    id_asistencia INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    id_horario INT NOT NULL,
    fecha DATE NOT NULL,
    estado NVARCHAR(20) NOT NULL,
    observacion NVARCHAR(250) NULL,
    CONSTRAINT PK_Asistencia PRIMARY KEY (id_asistencia),
    CONSTRAINT UQ_Asistencia UNIQUE(id_estudiante,id_horario,fecha),
    CONSTRAINT FK_Asistencia_Estudiante FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Asistencia_Horario FOREIGN KEY (id_horario) REFERENCES Horario(id_horario)
);
GO

/* ---- Notas ---- */

CREATE TABLE Nota
(
    id_nota INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    id_curso INT NOT NULL,
	tipo_evaluacion NVARCHAR(50) NOT NULL,
    valor_nota DECIMAL(5,2) NOT NULL,
    porcentaje DECIMAL(5,2) NULL,
    periodo NVARCHAR(50) NULL,
    fecha_registro DATE NOT NULL,
    CONSTRAINT PK_Nota PRIMARY KEY (id_nota),
    CONSTRAINT CK_Nota_Valor CHECK (valor_nota >= 0 AND valor_nota <= 100),
    CONSTRAINT FK_Nota_Estudiante FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Nota_Curso FOREIGN KEY (id_curso) REFERENCES Curso(id_curso)
);
GO

/* ---- Recursos Académicos ---- */

CREATE TABLE Recursos_Academicos
(
    id_recurso INT IDENTITY(1,1) NOT NULL,
    id_horario INT NOT NULL,
    titulo NVARCHAR(200) NOT NULL,
    descripcion NVARCHAR(500) NULL,
    url_recurso NVARCHAR(500) NULL,
    fecha_publicacion DATETIME NOT NULL
        CONSTRAINT DF_Recurso_Fecha DEFAULT GETDATE(),
    CONSTRAINT PK_Recursos_Academicos PRIMARY KEY (id_recurso),
    CONSTRAINT FK_Recurso_Horario FOREIGN KEY (id_horario) REFERENCES Horario(id_horario)
);
GO

/* ---- Comunicación ---- */


CREATE TABLE Notificacion
(
    id_notificacion INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    CONSTRAINT PK_Notificacion PRIMARY KEY (id_notificacion),
    CONSTRAINT FK_Notificacion_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE Anuncio
(
    id_anuncio INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    CONSTRAINT PK_Anuncio PRIMARY KEY (id_anuncio),
    CONSTRAINT FK_Anuncio_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

/* ---- Reportes ---- */


CREATE TABLE Reporte_Anonimo
(
    id_reporte INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NULL,
    reporte NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_Reporte_Anonimo PRIMARY KEY (id_reporte),
    CONSTRAINT FK_Reporte_Anonimo_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO


/* ---- Manejo de Inventario/Equipo ---- */

CREATE TABLE Categoria_Inv
(
    id_categoria INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_Categoria_Inv PRIMARY KEY (id_categoria),
    CONSTRAINT UQ_Categoria_Inv_Nombre UNIQUE (nombre)
);
GO

CREATE TABLE Inventario
(
    id_inventario INT IDENTITY(1,1) NOT NULL,
    id_categoria INT NOT NULL,
    id_perAdm INT NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(300) NULL,
    cantidad INT NOT NULL,
    estado NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Inventario PRIMARY KEY (id_inventario),
    CONSTRAINT CK_Inventario_Cantidad CHECK (cantidad >= 0),
    CONSTRAINT FK_Inventario_Categoria FOREIGN KEY (id_categoria) REFERENCES Categoria_Inv(id_categoria),
    CONSTRAINT FK_Inventario_Personal_Admin FOREIGN KEY (id_perAdm) REFERENCES Personal_Admin(id_perAdm)
);
GO


/* ---- TABLAS INTERMEDIAS ---- */

CREATE TABLE Estudiante_Docente
(
    id_estudiante INT NOT NULL,
    id_docente INT NOT NULL,
    CONSTRAINT PK_Estudiante_Docente PRIMARY KEY (id_estudiante, id_docente),
    CONSTRAINT FK_Estudiante_Docente_Estudiante FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Estudiante_Docente_Docente FOREIGN KEY (id_docente) REFERENCES Docente(id_docente)
);
GO


CREATE TABLE Encargado_Estudiante
(
    id_encargado INT NOT NULL,
    id_estudiante INT NOT NULL,
    parentesco NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Encargado_Estudiante PRIMARY KEY(id_encargado,id_estudiante),
    CONSTRAINT FK_Encargado_Estudiante_Encargado FOREIGN KEY(id_encargado) REFERENCES Encargado(id_encargado),
    CONSTRAINT FK_Encargado_Estudiante_Estudiante FOREIGN KEY(id_estudiante) REFERENCES Estudiante(id_estudiante)
);
GO


/*Passwords management*/

CREATE TABLE Usuario_Token_Reset
(
    id_token INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    token_hash VARBINARY(64) NOT NULL,
    fecha_creacion DATETIME NOT NULL
        CONSTRAINT DF_TokenReset_FechaCreacion DEFAULT GETUTCDATE(),
    fecha_expiracion DATETIME NOT NULL,
    usado BIT NOT NULL
        CONSTRAINT DF_TokenReset_Usado DEFAULT 0,
    CONSTRAINT PK_Usuario_Token_Reset PRIMARY KEY (id_token),
    CONSTRAINT FK_TokenReset_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO



/* ---- Permisos por rol (MUSF-01) ---- */
-- Modulo: catálogo de pantallas/funcionalidades del sistema.

CREATE TABLE Modulo
(
    id_modulo INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(250) NULL,
    CONSTRAINT PK_Modulo PRIMARY KEY (id_modulo),
    CONSTRAINT UQ_Modulo_Nombre UNIQUE (nombre)
);
GO

CREATE TABLE Rol_Permiso
(
    id_rol_permiso INT IDENTITY(1,1) NOT NULL,
    id_rol INT NOT NULL,
    id_modulo INT NOT NULL,
    puede_ver BIT NOT NULL CONSTRAINT DF_RolPermiso_Ver DEFAULT 0,
    puede_crear BIT NOT NULL CONSTRAINT DF_RolPermiso_Crear DEFAULT 0,
    puede_editar BIT NOT NULL CONSTRAINT DF_RolPermiso_Editar DEFAULT 0,
    puede_eliminar BIT NOT NULL CONSTRAINT DF_RolPermiso_Eliminar DEFAULT 0,
    CONSTRAINT PK_Rol_Permiso PRIMARY KEY (id_rol_permiso),
    CONSTRAINT UQ_Rol_Permiso UNIQUE (id_rol, id_modulo),
    CONSTRAINT FK_RolPermiso_Rol FOREIGN KEY (id_rol) REFERENCES Rol(id_rol),
    CONSTRAINT FK_RolPermiso_Modulo FOREIGN KEY (id_modulo) REFERENCES Modulo(id_modulo)
);
GO

/* LOGs de Acceso AUDITORÍA*/

CREATE TABLE Log_Acceso
(
    id_log INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NULL,
    correo NVARCHAR(150) NOT NULL,
    tipo_evento NVARCHAR(20) NOT NULL,
    exitoso BIT NOT NULL,
    mensaje NVARCHAR(250) NULL,
    fecha DATETIME NOT NULL
        CONSTRAINT DF_LogAcceso_Fecha DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Log_Acceso PRIMARY KEY (id_log),
    CONSTRAINT CK_LogAcceso_TipoEvento CHECK (tipo_evento IN (N'Login', N'Logout')),
    CONSTRAINT FK_LogAcceso_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_LogAcceso_Usuario
ON Log_Acceso(id_usuario);
GO

CREATE INDEX IX_LogAcceso_Fecha
ON Log_Acceso(fecha);
GO



/* ---- INDICES ---- */

CREATE INDEX IX_Usuario_Rol
ON Usuario(id_rol);
GO

CREATE INDEX IX_Horario_Docente
ON Horario(id_docente);
GO

CREATE INDEX IX_Horario_Curso
ON Horario(id_curso);
GO

CREATE INDEX IX_TokenReset_TokenHash
ON Usuario_Token_Reset(token_hash);
GO

CREATE INDEX IX_TokenReset_Usuario
ON Usuario_Token_Reset(id_usuario);
GO

CREATE INDEX IX_RolPermiso_Rol
ON Rol_Permiso(id_rol);
GO

CREATE INDEX IX_LogAcceso_Usuario
ON Log_Acceso(id_usuario);
GO

CREATE INDEX IX_LogAcceso_Fecha
ON Log_Acceso(fecha);
GO




/* TEST Inserts */

INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
VALUES (
    (SELECT id_rol FROM Rol WHERE nombre = N'Administrador'),
    N'Admin',
    N'Sistema',
    N'admin@lnsd.local',
    0x0100000001000186A000000010AABE60C9F6BC5529223DBB4951C347B70856A178469303277C4D854B9F0B2E79183FA7B4B3BD2A4C1CE30BE8D34122C5,
    1
);
GO

-- User: admin@lnsd.local 
-- Pass: Admin123!


-- TEST
-- User: FMendez@ejemplo.com 
-- Pass: aaaaaaaa


INSERT INTO Modulo (nombre, descripcion)
VALUES
    (N'Usuarios', N'Alta, edición y activación/desactivación de usuarios.'),
    (N'Roles', N'Administración de roles del sistema.');
GO

INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, 1, 1, 1, 1
FROM Rol r
CROSS JOIN Modulo m
WHERE r.nombre = N'Administrador';
GO

USE Proyecto_LNSD_DB;
GO
 
/* ---- HU 01: datos de la institución (una sola fila) ---- */
IF OBJECT_ID(N'Institucion', N'U') IS NULL
CREATE TABLE Institucion
(
    id_institucion INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    direccion NVARCHAR(250) NULL,
    telefono NVARCHAR(50) NOT NULL,
    telefono_secundario NVARCHAR(50) NULL,
    correo NVARCHAR(150) NOT NULL,
    ruta_logo NVARCHAR(250) NULL,
    fecha_actualizacion DATETIME NOT NULL
        CONSTRAINT DF_Institucion_Fecha DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Institucion PRIMARY KEY (id_institucion)
);
GO
 
/* ---- HU 02: períodos lectivos ---- */
IF OBJECT_ID(N'Periodo_Lectivo', N'U') IS NULL
BEGIN
    CREATE TABLE Periodo_Lectivo
    (
        id_periodo INT IDENTITY(1,1) NOT NULL,
        nombre NVARCHAR(100) NOT NULL,
        fecha_inicio DATE NOT NULL,
        fecha_fin DATE NOT NULL,
        activo BIT NOT NULL CONSTRAINT DF_Periodo_Activo DEFAULT 0,
        CONSTRAINT PK_Periodo_Lectivo PRIMARY KEY (id_periodo),
        CONSTRAINT UQ_Periodo_Nombre UNIQUE (nombre),
        CONSTRAINT CK_Periodo_Fechas CHECK (fecha_fin >= fecha_inicio)
    );
 
    -- Solo puede existir un período activo
    CREATE UNIQUE INDEX UQ_Periodo_Activo
        ON Periodo_Lectivo (activo) WHERE activo = 1;
END
GO
 
/* ---- HU 03, 04, 05, 07, 08: contenido editable de la landing ---- */
IF OBJECT_ID(N'Contenido_Sitio', N'U') IS NULL
BEGIN
    CREATE TABLE Contenido_Sitio
    (
        id_contenido INT IDENTITY(1,1) NOT NULL,
        tipo NVARCHAR(30) NOT NULL,
        titulo NVARCHAR(150) NOT NULL,
        descripcion NVARCHAR(MAX) NULL,
        ruta_imagen NVARCHAR(250) NULL,
        orden INT NOT NULL CONSTRAINT DF_Contenido_Orden DEFAULT 0,
        estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Contenido_Estado DEFAULT N'Borrador',
        fecha_publicacion DATETIME NULL,
        fecha_actualizacion DATETIME NOT NULL
            CONSTRAINT DF_Contenido_Fecha DEFAULT GETUTCDATE(),
        id_usuario_modifica INT NULL,
        CONSTRAINT PK_Contenido_Sitio PRIMARY KEY (id_contenido),
        CONSTRAINT CK_Contenido_Tipo CHECK (tipo IN (N'Mision', N'Vision', N'Historia', N'Banner', N'Bloque')),
        CONSTRAINT CK_Contenido_Estado CHECK (estado IN (N'Borrador', N'Publicado')),
        CONSTRAINT FK_Contenido_Usuario FOREIGN KEY (id_usuario_modifica) REFERENCES Usuario(id_usuario)
    );
 
    -- Misión, Visión e Historia: una sola fila de cada una
    CREATE UNIQUE INDEX UQ_Contenido_Tipo_Unico
        ON Contenido_Sitio (tipo) WHERE tipo IN (N'Mision', N'Vision', N'Historia');
END
GO
 
/* ---- HU 06: accesos rápidos ---- */
IF OBJECT_ID(N'Acceso_Rapido', N'U') IS NULL
CREATE TABLE Acceso_Rapido
(
    id_acceso INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(250) NULL,
    icono NVARCHAR(50) NULL,
    enlace NVARCHAR(250) NOT NULL,
    orden INT NOT NULL CONSTRAINT DF_Acceso_Orden DEFAULT 0,
    activo BIT NOT NULL CONSTRAINT DF_Acceso_Activo DEFAULT 1,
    id_modulo INT NULL,
    CONSTRAINT PK_Acceso_Rapido PRIMARY KEY (id_acceso),
    CONSTRAINT FK_Acceso_Modulo FOREIGN KEY (id_modulo) REFERENCES Modulo(id_modulo)
);
GO
 
/* ---- Módulo de permisos "Configuracion" ---- */
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Configuracion')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Configuracion', N'Datos institucionales, períodos lectivos, contenido del sitio y accesos rápidos.');
GO
 
INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, 1, 1, 1, 1
FROM Rol r
CROSS JOIN Modulo m
WHERE r.nombre = N'Administrador'
  AND m.nombre = N'Configuracion'
  AND NOT EXISTS (SELECT 1 FROM Rol_Permiso rp WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO


USE Proyecto_LNSD_DB;
GO

-- 1) Roles (los que menciona el Excel de HU; solo Administrador es obligatorio para entrar)
INSERT INTO Rol (nombre, descripcion)
SELECT v.nombre, v.descripcion
FROM (VALUES
    (N'Administrador', N'Acceso total al sistema.'),
    (N'Director', N'Dirección de la institución.'),
    (N'Docente', N'Personal docente.'),
    (N'Estudiante', N'Estudiantes de la institución.'),
    (N'Encargado', N'Padre, madre o encargado legal.'),
    (N'Personal administrativo', N'Personal administrativo.')
) AS v(nombre, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM Rol r WHERE r.nombre = v.nombre);
GO

-- 2) Usuario admin de prueba (admin@lnsd.local / Admin123!, mismo hash del script)
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = N'admin@lnsd.local')
INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
VALUES (
    (SELECT id_rol FROM Rol WHERE nombre = N'Administrador'),
    N'Admin', N'Sistema', N'admin@lnsd.local',
    0x0100000001000186A000000010AABE60C9F6BC5529223DBB4951C347B70856A178469303277C4D854B9F0B2E79183FA7B4B3BD2A4C1CE30BE8D34122C5,
    1
);
GO

-- 3) Permisos del Administrador sobre los módulos que ya existan
INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, 1, 1, 1, 1
FROM Rol r
CROSS JOIN Modulo m
WHERE r.nombre = N'Administrador'
  AND NOT EXISTS (SELECT 1 FROM Rol_Permiso rp
                  WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO

SELECT r.nombre AS rol, m.nombre AS modulo
FROM Rol_Permiso rp
JOIN Rol r ON r.id_rol = rp.id_rol
JOIN Modulo m ON m.id_modulo = rp.id_modulo;