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