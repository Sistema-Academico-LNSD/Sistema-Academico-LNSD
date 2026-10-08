/* ============================================================
   PROYECTO LNSD - SCRIPT INICIAL DE BASE DE DATOS
   Crea la BD, tablas, restricciones, índices y datos iniciales.
   ============================================================ */

USE master;
GO

IF DB_ID(N'Proyecto_LNSD_DB') IS NOT NULL
BEGIN
    ALTER DATABASE Proyecto_LNSD_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Proyecto_LNSD_DB;
END
GO

CREATE DATABASE Proyecto_LNSD_DB;
GO

USE Proyecto_LNSD_DB;
GO

/* ============================================================
   1. SEGURIDAD Y USUARIOS
   ============================================================ */

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
    CONSTRAINT FK_Usuario_Rol
        FOREIGN KEY (id_rol) REFERENCES Rol(id_rol)
);
GO

CREATE INDEX IX_Usuario_Rol
    ON Usuario(id_rol);
GO

CREATE TABLE Personal_Admin
(
    id_perAdm INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    cargo NVARCHAR(100) NOT NULL,
    departamento NVARCHAR(100) NOT NULL,

    CONSTRAINT PK_Personal_Admin PRIMARY KEY (id_perAdm),
    CONSTRAINT UQ_Personal_Admin_Usuario UNIQUE (id_usuario),
    CONSTRAINT FK_Personal_Admin_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

-- ESTUDIANTE ESTÁ ALTER *****NUEVA*****

CREATE TABLE Docente
(
    id_docente INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NULL,
    nombre NVARCHAR(100) NOT NULL,
    apellidos NVARCHAR(150) NOT NULL,
    identificacion NVARCHAR(30) NOT NULL,
    correo NVARCHAR(150) NOT NULL,
    telefono NVARCHAR(30) NULL,
    direccion NVARCHAR(300) NULL,
    estado BIT NOT NULL CONSTRAINT DF_Docente_Estado DEFAULT 1,
    especialidad NVARCHAR(150) NULL,
    titulos NVARCHAR(1000) NULL,
    anios_experiencia INT NULL,
    id_area INT NULL,
    fecha_registro DATETIME NOT NULL
        CONSTRAINT DF_Docente_FechaRegistro DEFAULT GETUTCDATE(),

    CONSTRAINT PK_Docente PRIMARY KEY (id_docente),
    CONSTRAINT UQ_Docente_Identificacion UNIQUE (identificacion),
    CONSTRAINT UQ_Docente_Correo UNIQUE (correo),
    CONSTRAINT FK_Docente_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT CK_Docente_Experiencia
        CHECK (anios_experiencia IS NULL OR
               (anios_experiencia >= 0 AND anios_experiencia <= 60))
);
GO

CREATE UNIQUE INDEX UX_Docente_Usuario
    ON Docente(id_usuario)
    WHERE id_usuario IS NOT NULL;
GO


-- ESTUDIANTE ESTÁ ALTER *****NUEVA*****

CREATE TABLE Estudiante
(
    id_estudiante INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    identificacion NVARCHAR(30) NOT NULL,
    carnet NVARCHAR(20) NOT NULL,
    fecha_ingreso DATE NOT NULL,
    telefono NVARCHAR(30) NULL,
    direccion NVARCHAR(300) NULL,
    estado BIT NOT NULL CONSTRAINT DF_Estudiante_Estado DEFAULT 1,

    CONSTRAINT PK_Estudiante PRIMARY KEY (id_estudiante),
    CONSTRAINT UQ_Estudiante_Usuario UNIQUE (id_usuario),
    CONSTRAINT UQ_Estudiante_Identificacion UNIQUE (identificacion),
    CONSTRAINT UQ_Estudiante_Carnet UNIQUE (carnet),

    CONSTRAINT FK_Estudiante_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE Encargado
(
    id_encargado INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,

    CONSTRAINT PK_Encargado PRIMARY KEY (id_encargado),
    CONSTRAINT UQ_Encargado_Usuario UNIQUE (id_usuario),
    CONSTRAINT FK_Encargado_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO
/* ---------- Estudiante (agregados) ---------- */
IF COL_LENGTH('Estudiante','fecha_nacimiento') IS NULL
    ALTER TABLE Estudiante ADD fecha_nacimiento DATE NULL;           
GO
IF COL_LENGTH('Estudiante','correo_emergencia') IS NULL
    ALTER TABLE Estudiante ADD correo_emergencia NVARCHAR(150) NULL; 
GO
IF COL_LENGTH('Estudiante','id_grado') IS NULL
    ALTER TABLE Estudiante ADD id_grado INT NULL;                    
GO
IF COL_LENGTH('Estudiante','estado_academico') IS NULL
    ALTER TABLE Estudiante ADD estado_academico NVARCHAR(30) NOT NULL
        CONSTRAINT DF_Estudiante_EstadoAcademico DEFAULT N'Regular'; 
GO
IF COL_LENGTH('Estudiante','alergias') IS NULL
    ALTER TABLE Estudiante ADD alergias NVARCHAR(500) NULL;          
GO
IF COL_LENGTH('Estudiante','observaciones_medicas') IS NULL
    ALTER TABLE Estudiante ADD observaciones_medicas NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('Estudiante','adecuaciones_educativas') IS NULL
    ALTER TABLE Estudiante ADD adecuaciones_educativas NVARCHAR(1000) NULL;
GO
 
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Estudiante_Grado')
    ALTER TABLE Estudiante ADD CONSTRAINT FK_Estudiante_Grado
        FOREIGN KEY (id_grado) REFERENCES Grado(id_grado);
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Estudiante_EstadoAcademico')
    ALTER TABLE Estudiante ADD CONSTRAINT CK_Estudiante_EstadoAcademico
        CHECK (estado_academico IN (N'Regular', N'Repitente', N'Retirado', N'Egresado'));
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Estudiante_Grado')
    CREATE INDEX IX_Estudiante_Grado ON Estudiante(id_grado);
GO
 
/* ---------- Encargado (agregados) ---------- */
IF COL_LENGTH('Encargado','telefono') IS NULL
    ALTER TABLE Encargado ADD telefono NVARCHAR(30) NULL;            
GO
 
/* ---------- Encargado_Estudiante ---------- */
IF COL_LENGTH('Encargado_Estudiante','es_principal') IS NULL
    ALTER TABLE Encargado_Estudiante ADD es_principal BIT NOT NULL
        CONSTRAINT DF_EncEst_EsPrincipal DEFAULT 0;                  
GO
-- Solo un encargado principal por estudiante
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Encargado_Estudiante_Principal')
    CREATE UNIQUE INDEX UX_Encargado_Estudiante_Principal
        ON Encargado_Estudiante(id_estudiante) WHERE es_principal = 1;
GO

USE Proyecto_LNSD_DB;
GO
 
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Estudiantes')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Estudiantes', N'Expedientes estudiantiles: registro, edición, estado, carné y consulta.');
GO
 
-- Administrador: todos los permisos sobre Estudiantes
IF NOT EXISTS (SELECT 1 FROM Rol_Permiso rp
               JOIN Rol r ON r.id_rol = rp.id_rol
               JOIN Modulo m ON m.id_modulo = rp.id_modulo
               WHERE r.nombre = N'Administrador' AND m.nombre = N'Estudiantes')
    INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
    SELECT r.id_rol, m.id_modulo, 1, 1, 1, 1
    FROM Rol r CROSS JOIN Modulo m
    WHERE r.nombre = N'Administrador' AND m.nombre = N'Estudiantes';
GO
 
-- Director: solo consulta
IF NOT EXISTS (SELECT 1 FROM Rol_Permiso rp
               JOIN Rol r ON r.id_rol = rp.id_rol
               JOIN Modulo m ON m.id_modulo = rp.id_modulo
               WHERE r.nombre = N'Director' AND m.nombre = N'Estudiantes')
    INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
    SELECT r.id_rol, m.id_modulo, 1, 0, 0, 0
    FROM Rol r CROSS JOIN Modulo m
    WHERE r.nombre = N'Director' AND m.nombre = N'Estudiantes';
GO

/* ============================================================
   2. ÁREAS ACADÉMICAS *****NUEVA*****
   ============================================================ */

CREATE TABLE Area_Academica
(
    id_area INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    estado BIT NOT NULL CONSTRAINT DF_Area_Academica_Estado DEFAULT 1,

    CONSTRAINT PK_Area_Academica PRIMARY KEY (id_area),
    CONSTRAINT UQ_Area_Academica_Nombre UNIQUE (nombre)
);
GO

/* ============================================================
   3. GRADOS Y SECCIONES
   ============================================================ */

CREATE TABLE Grado
(
    id_grado INT IDENTITY(1,1) NOT NULL,
    codigo NVARCHAR(20) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    nivel NVARCHAR(50) NOT NULL,
    estado BIT NOT NULL CONSTRAINT DF_Grado_Estado DEFAULT 1,

    CONSTRAINT PK_Grado PRIMARY KEY (id_grado),
    CONSTRAINT UQ_Grado_Codigo UNIQUE (codigo),
    CONSTRAINT UQ_Grado_Nombre UNIQUE (nombre)
);
GO

CREATE TABLE Seccion
(
    id_seccion INT IDENTITY(1,1) NOT NULL,
    id_grado INT NOT NULL,
    nombre NVARCHAR(50) NOT NULL,
    capacidad_maxima INT NOT NULL,
    estado BIT NOT NULL CONSTRAINT DF_Seccion_Estado DEFAULT 1,

    CONSTRAINT PK_Seccion PRIMARY KEY (id_seccion),
    CONSTRAINT UQ_Seccion_Grado_Nombre UNIQUE (id_grado, nombre),
    CONSTRAINT CK_Seccion_Capacidad CHECK (capacidad_maxima > 0),
    CONSTRAINT FK_Seccion_Grado
        FOREIGN KEY (id_grado) REFERENCES Grado(id_grado)
);
GO

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

/* ============================================================
   4. CURSOS
   ============================================================ */

CREATE TABLE Curso
(
    id_curso INT IDENTITY(1,1) NOT NULL,
    codigo NVARCHAR(30) NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(500) NOT NULL,
    id_area INT NOT NULL,
    estado BIT NOT NULL CONSTRAINT DF_Curso_Estado DEFAULT 1,

    CONSTRAINT PK_Curso PRIMARY KEY (id_curso),
    CONSTRAINT UQ_Curso_Codigo UNIQUE (codigo),
    CONSTRAINT UQ_Curso_Nombre_Area UNIQUE (nombre, id_area),
    CONSTRAINT FK_Curso_Area
        FOREIGN KEY (id_area) REFERENCES Area_Academica(id_area)
);
GO

-- INTERMADIA CURSO_GRADO *****NUEVA*****

CREATE TABLE Curso_Grado
(
    id_curso INT NOT NULL,
    id_grado INT NOT NULL,

    CONSTRAINT PK_Curso_Grado PRIMARY KEY (id_curso, id_grado),
    CONSTRAINT FK_Curso_Grado_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT FK_Curso_Grado_Grado
        FOREIGN KEY (id_grado) REFERENCES Grado(id_grado)
);
GO

/* ============================================================
   5. MATRÍCULA Y HORARIOS
   ============================================================ */

CREATE TABLE Matricula
(
    id_matricula INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    id_grupo INT NOT NULL,

    CONSTRAINT PK_Matricula PRIMARY KEY (id_matricula),
    CONSTRAINT UQ_Matricula_Estudiante_Grupo
        UNIQUE (id_estudiante, id_grupo),
    CONSTRAINT FK_Matricula_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Matricula_Grupo
        FOREIGN KEY (id_grupo) REFERENCES Grupo(id_grupo)
);
GO

CREATE TABLE Horario
(
    id_horario INT IDENTITY(1,1) NOT NULL,
    id_docente INT NOT NULL,
    id_curso INT NOT NULL,
    dia NVARCHAR(15) NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,

    CONSTRAINT PK_Horario PRIMARY KEY (id_horario),
    CONSTRAINT FK_Horario_Docente
        FOREIGN KEY (id_docente) REFERENCES Docente(id_docente),
    CONSTRAINT FK_Horario_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT UQ_Horario_Docente_Dia_Hora
        UNIQUE (id_docente, dia, hora_inicio, hora_fin)
);
GO

CREATE INDEX IX_Horario_Docente ON Horario(id_docente);
CREATE INDEX IX_Horario_Curso ON Horario(id_curso);
GO

/* ============================================================
   6. ASISTENCIA Y NOTAS
   ============================================================ */

CREATE TABLE Asistencia
(
    id_asistencia INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    id_horario INT NOT NULL,
    fecha DATE NOT NULL,
    estado NVARCHAR(20) NOT NULL,
    observacion NVARCHAR(250) NULL,

    CONSTRAINT PK_Asistencia PRIMARY KEY (id_asistencia),
    CONSTRAINT UQ_Asistencia
        UNIQUE (id_estudiante, id_horario, fecha),
    CONSTRAINT FK_Asistencia_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Asistencia_Horario
        FOREIGN KEY (id_horario) REFERENCES Horario(id_horario)
);
GO

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
    CONSTRAINT CK_Nota_Valor
        CHECK (valor_nota >= 0 AND valor_nota <= 100),
    CONSTRAINT FK_Nota_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Nota_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso)
);
GO

/* ============================================================
   7. BECAS Y RECURSOS ACADÉMICOS
   ============================================================ */

CREATE TABLE Beca
(
    id_beca INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    porcentaje DECIMAL(5,2) NULL,
    fecha_inicio DATE NULL,
    fecha_fin DATE NULL,
    descripcion NVARCHAR(250) NULL,

    CONSTRAINT PK_Beca PRIMARY KEY (id_beca),
    CONSTRAINT FK_Beca_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante)
);
GO

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
    CONSTRAINT FK_Recurso_Horario
        FOREIGN KEY (id_horario) REFERENCES Horario(id_horario)
);
GO

/* ============================================================
   8. RELACIONES ESTUDIANTE / DOCENTE / ENCARGADO
   ============================================================ */

CREATE TABLE Estudiante_Docente
(
    id_estudiante INT NOT NULL,
    id_docente INT NOT NULL,

    CONSTRAINT PK_Estudiante_Docente
        PRIMARY KEY (id_estudiante, id_docente),
    CONSTRAINT FK_Estudiante_Docente_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Estudiante_Docente_Docente
        FOREIGN KEY (id_docente) REFERENCES Docente(id_docente)
);
GO

CREATE TABLE Encargado_Estudiante
(
    id_encargado INT NOT NULL,
    id_estudiante INT NOT NULL,
    parentesco NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_Encargado_Estudiante
        PRIMARY KEY (id_encargado, id_estudiante),
    CONSTRAINT FK_Encargado_Estudiante_Encargado
        FOREIGN KEY (id_encargado) REFERENCES Encargado(id_encargado),
    CONSTRAINT FK_Encargado_Estudiante_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante)
);
GO

/* ============================================================
   9. COMUNICACIÓN
   ============================================================ */

CREATE TABLE Notificacion
(
    id_notificacion INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,

    CONSTRAINT PK_Notificacion PRIMARY KEY (id_notificacion),
    CONSTRAINT FK_Notificacion_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE Anuncio
(
    id_anuncio INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,

    CONSTRAINT PK_Anuncio PRIMARY KEY (id_anuncio),
    CONSTRAINT FK_Anuncio_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

/* ============================================================
   10. REPORTES
   ============================================================ */

CREATE TABLE Reporte_Anonimo
(
    id_reporte INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NULL,
    reporte NVARCHAR(MAX) NOT NULL,

    CONSTRAINT PK_Reporte_Anonimo PRIMARY KEY (id_reporte),
    CONSTRAINT FK_Reporte_Anonimo_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

/* ============================================================
   11. INVENTARIO
   ============================================================ */

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
    CONSTRAINT FK_Inventario_Categoria
        FOREIGN KEY (id_categoria) REFERENCES Categoria_Inv(id_categoria),
    CONSTRAINT FK_Inventario_Personal_Admin
        FOREIGN KEY (id_perAdm) REFERENCES Personal_Admin(id_perAdm)
);
GO

/* ============================================================
   12. RECUPERACIÓN DE CONTRASEÑA
   ============================================================ */

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
    CONSTRAINT FK_TokenReset_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_TokenReset_TokenHash
    ON Usuario_Token_Reset(token_hash);

CREATE INDEX IX_TokenReset_Usuario
    ON Usuario_Token_Reset(id_usuario);
GO

/* ============================================================
   13. ROLES, MÓDULOS Y PERMISOS
   ============================================================ */

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
    CONSTRAINT FK_RolPermiso_Rol
        FOREIGN KEY (id_rol) REFERENCES Rol(id_rol),
    CONSTRAINT FK_RolPermiso_Modulo
        FOREIGN KEY (id_modulo) REFERENCES Modulo(id_modulo)
);
GO

CREATE INDEX IX_RolPermiso_Rol ON Rol_Permiso(id_rol);
GO

/* ============================================================
   14. AUDITORÍA
   ============================================================ */

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
    CONSTRAINT CK_LogAcceso_TipoEvento
        CHECK (tipo_evento IN (N'Login', N'Logout')),
    CONSTRAINT FK_LogAcceso_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_LogAcceso_Usuario ON Log_Acceso(id_usuario);
CREATE INDEX IX_LogAcceso_Fecha ON Log_Acceso(fecha);
GO

/* ============================================================
   15. CONFIGURACIÓN DEL SITIO
   ============================================================ */

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
GO

-- TABLA INTERMEDIA DOCENTE_CURSO *****NUEVA*****

CREATE TABLE Docente_Curso
(
    id_docente_curso INT IDENTITY(1,1) NOT NULL,
    id_docente INT NOT NULL,
    id_curso INT NOT NULL,
    id_periodo INT NOT NULL,
    fecha_asignacion DATETIME NOT NULL
        CONSTRAINT DF_DocenteCurso_Fecha DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Docente_Curso PRIMARY KEY (id_docente_curso),
    CONSTRAINT UQ_Docente_Curso_Periodo UNIQUE (id_docente, id_curso, id_periodo),
    CONSTRAINT FK_DocenteCurso_Docente FOREIGN KEY (id_docente) REFERENCES Docente(id_docente),
    CONSTRAINT FK_DocenteCurso_Curso FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT FK_DocenteCurso_Periodo FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo)
);


CREATE INDEX IX_DocenteCurso_Curso ON Docente_Curso(id_curso);
CREATE INDEX IX_DocenteCurso_Periodo ON Docente_Curso(id_periodo);


CREATE UNIQUE INDEX UQ_Periodo_Activo
    ON Periodo_Lectivo(activo)
    WHERE activo = 1;
GO

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
    CONSTRAINT CK_Contenido_Tipo
        CHECK (tipo IN (N'Mision', N'Vision', N'Historia', N'Banner', N'Bloque')),
    CONSTRAINT CK_Contenido_Estado
        CHECK (estado IN (N'Borrador', N'Publicado')),
    CONSTRAINT FK_Contenido_Usuario
        FOREIGN KEY (id_usuario_modifica) REFERENCES Usuario(id_usuario)
);
GO

CREATE UNIQUE INDEX UQ_Contenido_Tipo_Unico
    ON Contenido_Sitio(tipo)
    WHERE tipo IN (N'Mision', N'Vision', N'Historia');
GO

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
    CONSTRAINT FK_Acceso_Modulo
        FOREIGN KEY (id_modulo) REFERENCES Modulo(id_modulo)
);
GO

CREATE TABLE Acceso_Rapido_Rol
(
    id_acceso INT NOT NULL,
    id_rol INT NOT NULL,

    CONSTRAINT PK_Acceso_Rapido_Rol
        PRIMARY KEY (id_acceso, id_rol),
    CONSTRAINT FK_AccesoRol_Acceso
        FOREIGN KEY (id_acceso) REFERENCES Acceso_Rapido(id_acceso)
        ON DELETE CASCADE,
    CONSTRAINT FK_AccesoRol_Rol
        FOREIGN KEY (id_rol) REFERENCES Rol(id_rol)
);
GO

/* ============================================================
   16. BOLETERÍA
   ============================================================ */

CREATE TABLE Tiquete
(
    id_tiquete INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    codigo NVARCHAR(30) NOT NULL,
    estado NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Tiquete_Estado DEFAULT N'Disponible',
    fecha_generacion DATETIME NOT NULL
        CONSTRAINT DF_Tiquete_FechaGeneracion DEFAULT GETUTCDATE(),
    fecha_utilizacion DATETIME NULL,
    id_usuario_validador INT NULL,

    CONSTRAINT PK_Tiquete PRIMARY KEY (id_tiquete),
    CONSTRAINT UQ_Tiquete_Codigo UNIQUE (codigo),
    CONSTRAINT CK_Tiquete_Estado
        CHECK (estado IN (N'Disponible', N'Utilizado')),
    CONSTRAINT FK_Tiquete_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Tiquete_Validador
        FOREIGN KEY (id_usuario_validador) REFERENCES Usuario(id_usuario)
);
GO

CREATE UNIQUE INDEX UX_Tiquete_Usuario_Disponible
    ON Tiquete(id_usuario)
    WHERE estado = N'Disponible';
GO

/* ============================================================
   17. DATOS INICIALES DE PRUEBA (TESTING DUMMY DATA)
   ============================================================ */

INSERT INTO Rol (nombre, descripcion)
VALUES
    (N'Administrador', N'Acceso total al sistema.'),
    (N'Director', N'Dirección de la institución.'),
    (N'Docente', N'Personal docente.'),
    (N'Estudiante', N'Estudiantes de la institución.'),
    (N'Encargado', N'Padre, madre o encargado legal.'),
    (N'Personal administrativo', N'Personal administrativo.');
GO

INSERT INTO Area_Academica (nombre, estado)
VALUES
    (N'Matemáticas', 1),
    (N'Ciencias', 1),
    (N'Idiomas', 1),
    (N'Estudios Sociales', 1),
    (N'Arte', 1);
GO

INSERT INTO Usuario
(
    id_rol,
    nombre,
    apellido,
    correo,
    password_hash,
    estado
)
VALUES
(
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
    (N'Roles', N'Administración de roles del sistema.'),
    (N'Permisos', N'Administración de permisos por rol.'),
    (N'Auditoria', N'Historial de accesos al sistema.'),
    (N'Configuracion', N'Datos institucionales, períodos lectivos, contenido del sitio y accesos rápidos.'),
    (N'Boleteria', N'Generación, validación y control de tiquetes del comedor.'),
    (N'Grados', N'Gestión de grados académicos, grupos y secciones.'),
    (N'Cursos', N'Gestión de cursos y oferta académica de la institución.'),
    (N'Docentes', N'Expedientes del personal docente: registro, edición, estado y consulta.');
GO

INSERT INTO Rol_Permiso
(
    id_rol,
    id_modulo,
    puede_ver,
    puede_crear,
    puede_editar,
    puede_eliminar
)
SELECT
    r.id_rol,
    m.id_modulo,
    1,
    CASE WHEN m.nombre IN (N'Grados', N'Docentes') THEN 1 ELSE 1 END,
    CASE WHEN m.nombre IN (N'Grados', N'Docentes') THEN 1 ELSE 1 END,
    CASE WHEN m.nombre = N'Grados' THEN 0 ELSE 1 END
FROM Rol r
CROSS JOIN Modulo m
WHERE r.nombre = N'Administrador';
GO

INSERT INTO Rol_Permiso
(
    id_rol,
    id_modulo,
    puede_ver,
    puede_crear,
    puede_editar,
    puede_eliminar
)
SELECT
    r.id_rol,
    m.id_modulo,
    1,
    0,
    0,
    0
FROM Rol r
CROSS JOIN Modulo m
WHERE r.nombre = N'Director'
  AND m.nombre = N'Docentes';
GO

INSERT INTO Acceso_Rapido
(
    nombre,
    descripcion,
    icono,
    enlace,
    orden,
    activo
)
VALUES
    (N'Usuarios', N'Edición y activación de cuentas del sistema.', N'bi-people', N'/Usuario/Index', 1, 1),
    (N'Roles', N'Roles y descripción dentro del sistema.', N'bi-person-badge', N'/Rol/Index', 2, 1),
    (N'Permisos', N'Permisos para cada rol.', N'bi-shield-lock', N'/Permiso/Index', 3, 1),
    (N'Historial de Accesos', N'Auditoría de sesión.', N'bi-journal-text', N'/Auditoria/Index', 4, 1),
    (N'Configuración', N'Datos institucionales, períodos y contenido del sitio.', N'bi-gear', N'/Configuracion/Index', 5, 1);
GO

INSERT INTO Acceso_Rapido_Rol (id_acceso, id_rol)
SELECT
    a.id_acceso,
    r.id_rol
FROM Acceso_Rapido a
CROSS JOIN Rol r
WHERE r.nombre = N'Administrador';
GO

/* ============================================================
   MÓDULO ESTUDIANTES - DATOS DE PRUEBA
   Ejecutar DESPUÉS de 01_ALTER_Modulo_Estudiantes.sql (no duplica).
 
   Usuarios de prueba:
     estudiante.prueba@lnsd.local  /  Estudiante123*   (rol Estudiante)
     encargado.prueba@lnsd.local   /  Encargado123*    (rol Encargado)
   ============================================================ */
USE Proyecto_LNSD_DB;
GO
 
/* ---------- Usuario estudiante ---------- */
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = N'estudiante.prueba@lnsd.local')
    INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
    VALUES ((SELECT id_rol FROM Rol WHERE nombre = N'Estudiante'),
            N'Estudiante', N'Prueba', N'estudiante.prueba@lnsd.local',
            0x0100000001000186A0000000108760C60D5E72121B52832E5B76E8C2D38B7594203BFD27E0345D59EB4C449C76A4166C59C2F2C683396299ABE595520A, 1);
GO
 
IF NOT EXISTS (SELECT 1 FROM Estudiante e JOIN Usuario u ON u.id_usuario = e.id_usuario
               WHERE u.correo = N'estudiante.prueba@lnsd.local')
    INSERT INTO Estudiante
        (id_usuario, identificacion, carnet, fecha_ingreso, fecha_nacimiento,
         telefono, direccion, correo_emergencia, id_grado, estado_academico,
         alergias, observaciones_medicas, adecuaciones_educativas, estado)
    VALUES
        ((SELECT id_usuario FROM Usuario WHERE correo = N'estudiante.prueba@lnsd.local'),
         N'1-1111-1111', N'PRUEBA-0001', '2026-02-01', '2012-05-10',
         N'8888-0001', N'San José, Costa Rica', N'emergencia.prueba@lnsd.local',
         (SELECT TOP 1 id_grado FROM Grado ORDER BY id_grado),
         N'Regular',
         N'Alergia al maní', N'Usa lentes', N'Tiempo adicional en exámenes', 1);
GO
 
/* ---------- Usuario encargado ---------- */
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = N'encargado.prueba@lnsd.local')
    INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
    VALUES ((SELECT id_rol FROM Rol WHERE nombre = N'Encargado'),
            N'Encargado', N'Prueba', N'encargado.prueba@lnsd.local',
            0x0100000001000186A000000010979CBF60A620784CF213010E9F5B14A1FEE56743FBC1CFEFFF35E2CCEA915B377F1FC3CBF9DABB4591A82D8BC0E60B2F, 1);
GO
 
IF NOT EXISTS (SELECT 1 FROM Encargado e JOIN Usuario u ON u.id_usuario = e.id_usuario
               WHERE u.correo = N'encargado.prueba@lnsd.local')
    INSERT INTO Encargado (id_usuario, telefono)
    VALUES ((SELECT id_usuario FROM Usuario WHERE correo = N'encargado.prueba@lnsd.local'),
            N'8888-0002');
GO
 
/* ---------- Vínculo encargado principal ---------- */
IF NOT EXISTS (
    SELECT 1
    FROM Encargado_Estudiante ee
    JOIN Encargado en ON en.id_encargado = ee.id_encargado
    JOIN Usuario ue ON ue.id_usuario = en.id_usuario
    WHERE ue.correo = N'encargado.prueba@lnsd.local')
    INSERT INTO Encargado_Estudiante (id_encargado, id_estudiante, parentesco, es_principal)
    VALUES (
        (SELECT en.id_encargado FROM Encargado en JOIN Usuario u ON u.id_usuario = en.id_usuario
          WHERE u.correo = N'encargado.prueba@lnsd.local'),
        (SELECT es.id_estudiante FROM Estudiante es JOIN Usuario u ON u.id_usuario = es.id_usuario
          WHERE u.correo = N'estudiante.prueba@lnsd.local'),
        N'Madre', 1);

/* 2. Módulo de permisos para testing de docentes* *****NUEVO*****/


IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'AsignacionCursos')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'AsignacionCursos', N'Asignación de cursos a docentes por período lectivo, consulta e historial.');
GO

-- Administrador: todo
INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, 1, 1, 1, 1
FROM Rol r
CROSS JOIN Modulo m
WHERE r.nombre = N'Administrador'
  AND m.nombre = N'AsignacionCursos'
  AND NOT EXISTS (SELECT 1 FROM Rol_Permiso rp WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO

-- Director: consultar, asignar y quitar (MDOF-01-04 / 01-05 dicen "administrador o director")
INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, 1, 1, 0, 1
FROM Rol r
CROSS JOIN Modulo m
WHERE r.nombre = N'Director'
  AND m.nombre = N'AsignacionCursos'
  AND NOT EXISTS (SELECT 1 FROM Rol_Permiso rp WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO