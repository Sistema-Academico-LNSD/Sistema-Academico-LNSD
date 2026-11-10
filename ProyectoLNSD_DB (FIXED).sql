/* ============================================================
   PROYECTO LNSD - SCRIPT DE BASE DE DATOS
   ============================================================ */

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

CREATE INDEX IX_RolPermiso_Rol
    ON Rol_Permiso(id_rol);
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


/* ============================================================
   2. ÁREAS ACADÉMICAS
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
   3. GRADOS, SECCIONES, GRUPOS Y PERÍODOS
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
    CONSTRAINT CK_Seccion_Capacidad
        CHECK (capacidad_maxima > 0),
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
    CONSTRAINT UQ_Grupo_Nombre_Nivel_Anio UNIQUE (nombre, nivel, anio)
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
    CONSTRAINT CK_Periodo_Fechas
        CHECK (fecha_fin >= fecha_inicio)
);
GO

/* Solo un período lectivo activo a la vez. */
CREATE UNIQUE INDEX UQ_Periodo_Activo
    ON Periodo_Lectivo(activo)
    WHERE activo = 1;
GO


/* ============================================================
   4. DOCENTES, ESTUDIANTES Y ENCARGADOS
   ============================================================ */

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
    CONSTRAINT CK_Docente_Experiencia
        CHECK (anios_experiencia IS NULL OR
               (anios_experiencia >= 0 AND anios_experiencia <= 60)),
    CONSTRAINT FK_Docente_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Docente_Area
        FOREIGN KEY (id_area) REFERENCES Area_Academica(id_area)
);
GO

CREATE UNIQUE INDEX UX_Docente_Usuario
    ON Docente(id_usuario)
    WHERE id_usuario IS NOT NULL;
GO

CREATE TABLE Estudiante
(
    id_estudiante INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    identificacion NVARCHAR(30) NOT NULL,
    carnet NVARCHAR(20) NOT NULL,
    fecha_ingreso DATE NOT NULL,
    fecha_nacimiento DATE NULL,
    telefono NVARCHAR(30) NULL,
    direccion NVARCHAR(300) NULL,
    correo_emergencia NVARCHAR(150) NULL,
    id_grado INT NULL,
    estado_academico NVARCHAR(30) NOT NULL
        CONSTRAINT DF_Estudiante_EstadoAcademico DEFAULT N'Regular',
    alergias NVARCHAR(500) NULL,
    observaciones_medicas NVARCHAR(1000) NULL,
    adecuaciones_educativas NVARCHAR(1000) NULL,
    estado BIT NOT NULL CONSTRAINT DF_Estudiante_Estado DEFAULT 1,

    CONSTRAINT PK_Estudiante PRIMARY KEY (id_estudiante),
    CONSTRAINT UQ_Estudiante_Usuario UNIQUE (id_usuario),
    CONSTRAINT UQ_Estudiante_Identificacion UNIQUE (identificacion),
    CONSTRAINT UQ_Estudiante_Carnet UNIQUE (carnet),
    CONSTRAINT CK_Estudiante_EstadoAcademico
        CHECK (estado_academico IN (N'Regular', N'Repitente', N'Retirado', N'Egresado')),
    CONSTRAINT FK_Estudiante_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Estudiante_Grado
        FOREIGN KEY (id_grado) REFERENCES Grado(id_grado)
);
GO

CREATE INDEX IX_Estudiante_Grado
    ON Estudiante(id_grado);
GO

CREATE TABLE Encargado
(
    id_encargado INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    telefono NVARCHAR(30) NULL,

    CONSTRAINT PK_Encargado PRIMARY KEY (id_encargado),
    CONSTRAINT UQ_Encargado_Usuario UNIQUE (id_usuario),
    CONSTRAINT FK_Encargado_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE TABLE Encargado_Estudiante
(
    id_encargado INT NOT NULL,
    id_estudiante INT NOT NULL,
    parentesco NVARCHAR(50) NOT NULL,
    es_principal BIT NOT NULL CONSTRAINT DF_EncEst_EsPrincipal DEFAULT 0,

    CONSTRAINT PK_Encargado_Estudiante
        PRIMARY KEY (id_encargado, id_estudiante),
    CONSTRAINT FK_Encargado_Estudiante_Encargado
        FOREIGN KEY (id_encargado) REFERENCES Encargado(id_encargado),
    CONSTRAINT FK_Encargado_Estudiante_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante)
);
GO

CREATE UNIQUE INDEX UX_Encargado_Estudiante_Principal
    ON Encargado_Estudiante(id_estudiante)
    WHERE es_principal = 1;
GO

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


/* ============================================================
   5. CURSOS
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
    CONSTRAINT FK_DocenteCurso_Docente
        FOREIGN KEY (id_docente) REFERENCES Docente(id_docente),
    CONSTRAINT FK_DocenteCurso_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT FK_DocenteCurso_Periodo
        FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo)
);
GO

CREATE INDEX IX_DocenteCurso_Curso
    ON Docente_Curso(id_curso);

CREATE INDEX IX_DocenteCurso_Periodo
    ON Docente_Curso(id_periodo);
GO


/* ============================================================
   6. MATRÍCULA
   ============================================================ */

CREATE TABLE Matricula
(
    id_matricula INT IDENTITY(1,1) NOT NULL,
    id_estudiante INT NOT NULL,
    id_periodo INT NOT NULL,
    id_grupo INT NOT NULL,
    id_seccion INT NOT NULL,
    fecha_matricula DATE NOT NULL
        CONSTRAINT DF_Matricula_Fecha DEFAULT (CONVERT(DATE, GETDATE())),
    estado NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Matricula_Estado DEFAULT N'Activa',
    observaciones NVARCHAR(500) NULL,

    CONSTRAINT PK_Matricula PRIMARY KEY (id_matricula),
    CONSTRAINT CK_Matricula_Estado
        CHECK (estado IN (N'Activa', N'Pendiente', N'Anulada', N'Inactiva')),
    CONSTRAINT FK_Matricula_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Matricula_Periodo
        FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo),
    CONSTRAINT FK_Matricula_Grupo
        FOREIGN KEY (id_grupo) REFERENCES Grupo(id_grupo),
    CONSTRAINT FK_Matricula_Seccion
        FOREIGN KEY (id_seccion) REFERENCES Seccion(id_seccion)
);
GO

CREATE UNIQUE INDEX UQ_Matricula_Estudiante_Periodo
    ON Matricula(id_estudiante, id_periodo)
    WHERE estado <> N'Anulada';

CREATE INDEX IX_Matricula_Periodo
    ON Matricula(id_periodo);

CREATE INDEX IX_Matricula_Seccion
    ON Matricula(id_seccion);

CREATE INDEX IX_Matricula_Estado_Periodo
    ON Matricula(estado, id_periodo);
GO

CREATE TABLE Matricula_Curso
(
    id_matricula INT NOT NULL,
    id_curso INT NOT NULL,

    CONSTRAINT PK_Matricula_Curso PRIMARY KEY (id_matricula, id_curso),
    CONSTRAINT FK_MatriculaCurso_Matricula
        FOREIGN KEY (id_matricula) REFERENCES Matricula(id_matricula),
    CONSTRAINT FK_MatriculaCurso_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso)
);
GO

CREATE TABLE Matricula_Observacion
(
    id_observacion INT IDENTITY(1,1) NOT NULL,
    id_matricula INT NOT NULL,
    observacion NVARCHAR(1000) NOT NULL,
    fecha_registro DATE NOT NULL
        CONSTRAINT DF_Matricula_Observacion_Fecha DEFAULT (CONVERT(DATE, GETDATE())),

    CONSTRAINT PK_Matricula_Observacion PRIMARY KEY (id_observacion),
    CONSTRAINT CK_Matricula_Observacion_NoVacia
        CHECK (LEN(LTRIM(RTRIM(observacion))) > 0),
    CONSTRAINT FK_Matricula_Observacion_Matricula
        FOREIGN KEY (id_matricula) REFERENCES Matricula(id_matricula)
);
GO

CREATE INDEX IX_Matricula_Observacion_Matricula_Fecha
    ON Matricula_Observacion(id_matricula, fecha_registro DESC);
GO

CREATE TABLE Matricula_Historial
(
    id_historial INT IDENTITY(1,1) NOT NULL,
    id_matricula INT NOT NULL,
    id_usuario INT NULL,
    tipo_cambio NVARCHAR(30) NOT NULL,
    estado_anterior NVARCHAR(20) NULL,
    estado_nuevo NVARCHAR(20) NULL,
    grupo_anterior INT NULL,
    grupo_nuevo INT NULL,
    seccion_anterior INT NULL,
    seccion_nueva INT NULL,
    detalle NVARCHAR(1000) NULL,
    fecha_cambio DATETIME2(0) NOT NULL
        CONSTRAINT DF_Matricula_Historial_Fecha DEFAULT (SYSDATETIME()),

    CONSTRAINT PK_Matricula_Historial PRIMARY KEY (id_historial),
    CONSTRAINT CK_Matricula_Historial_Tipo
        CHECK (tipo_cambio IN (N'Registro', N'Estado', N'Traslado', N'Edicion')),
    CONSTRAINT FK_Matricula_Historial_Matricula
        FOREIGN KEY (id_matricula) REFERENCES Matricula(id_matricula),
    CONSTRAINT FK_Matricula_Historial_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Matricula_Historial_GrupoAnterior
        FOREIGN KEY (grupo_anterior) REFERENCES Grupo(id_grupo),
    CONSTRAINT FK_Matricula_Historial_GrupoNuevo
        FOREIGN KEY (grupo_nuevo) REFERENCES Grupo(id_grupo),
    CONSTRAINT FK_Matricula_Historial_SeccionAnterior
        FOREIGN KEY (seccion_anterior) REFERENCES Seccion(id_seccion),
    CONSTRAINT FK_Matricula_Historial_SeccionNueva
        FOREIGN KEY (seccion_nueva) REFERENCES Seccion(id_seccion)
);
GO

CREATE INDEX IX_Matricula_Historial_Matricula_Fecha
    ON Matricula_Historial(id_matricula, fecha_cambio DESC);
GO


/* ============================================================
   7. HORARIOS, ASISTENCIA Y NOTAS
   ============================================================ */

CREATE TABLE Horario
(
    id_horario INT IDENTITY(1,1) NOT NULL,
    id_docente INT NOT NULL,
    id_curso INT NOT NULL,
    dia NVARCHAR(15) NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,

    CONSTRAINT PK_Horario PRIMARY KEY (id_horario),
    CONSTRAINT UQ_Horario_Docente_Dia_Hora
        UNIQUE (id_docente, dia, hora_inicio, hora_fin),
    CONSTRAINT FK_Horario_Docente
        FOREIGN KEY (id_docente) REFERENCES Docente(id_docente),
    CONSTRAINT FK_Horario_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso)
);
GO

CREATE INDEX IX_Horario_Docente
    ON Horario(id_docente);

CREATE INDEX IX_Horario_Curso
    ON Horario(id_curso);
GO

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

-- Rubros de evaluación: catálogo FIJO según el MEP.
-- Trabajo cotidiano 50% + Pruebas 35% + Tarea 10% = 95%.
-- Los porcentajes son de la NOTA ANUAL (suma de los dos semestres): cada semestre aporta la mitad de cada rubro.
-- "cantidad_actividades": cuántas actividades por período (semestre) tiene el rubro cuando es fija
--   (pruebas: 2 por semestre, 4 al año según el MEP); NULL = el docente decide cuántas crea.
-- "peso_manual": 1 = el docente reparte el porcentaje anual del rubro entre sus actividades como quiera (pruebas);
--   0 = el peso de cada actividad se calcula en partes iguales dentro del semestre.
-- La asistencia (5%) se calcula aparte (ver Configuracion_Calificacion).

CREATE TABLE Rubro_Evaluacion
(
    id_rubro INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    porcentaje DECIMAL(5,2) NOT NULL,
    orden INT NOT NULL,
    puntos_minimo DECIMAL(5,2) NULL,
    puntos_maximo DECIMAL(5,2) NULL,
    cantidad_actividades INT NULL,
    peso_manual BIT NOT NULL CONSTRAINT DF_Rubro_PesoManual DEFAULT 0,
    estado BIT NOT NULL CONSTRAINT DF_Rubro_Estado DEFAULT 1,

    CONSTRAINT PK_Rubro_Evaluacion PRIMARY KEY (id_rubro),
    CONSTRAINT UQ_Rubro_Nombre UNIQUE (nombre),
    CONSTRAINT CK_Rubro_Porcentaje CHECK (porcentaje > 0 AND porcentaje <= 100),
    CONSTRAINT CK_Rubro_Puntos CHECK (puntos_minimo IS NULL OR puntos_maximo IS NULL OR puntos_minimo <= puntos_maximo),
    CONSTRAINT CK_Rubro_Cantidad CHECK (cantidad_actividades IS NULL OR cantidad_actividades > 0)
);
GO

INSERT INTO Rubro_Evaluacion (nombre, porcentaje, orden, puntos_minimo, puntos_maximo, cantidad_actividades, peso_manual)
VALUES (N'Trabajo cotidiano',  50, 1, NULL, NULL, NULL, 0),
       (N'Pruebas / Exámenes', 35, 2, 15,   30,   2,    1),
       (N'Tareas',             10, 3, NULL, NULL, NULL, 0);
GO

-- Actividades de evaluación: cada práctica, examen o tarea que el docente crea en su curso, sección y grupo.
-- "puntos" es lo que vale la actividad (la nota del estudiante va de 0 a ese valor).
-- "porcentaje": lo que pesa la actividad en la nota ANUAL; solo se guarda en los rubros de peso manual (pruebas).
-- En los demás es NULL y el peso se calcula: (porcentaje del rubro / 2 semestres) / actividades del semestre.
-- El docente decide cuántas actividades tiene cada rubro, salvo en los que tienen cantidad fija (ver Rubro_Evaluacion).

CREATE TABLE Actividad_Evaluacion
(
    id_actividad INT IDENTITY(1,1) NOT NULL,
    id_rubro INT NOT NULL,
    id_periodo INT NOT NULL,
    id_curso INT NOT NULL,
    id_seccion INT NOT NULL,
    id_grupo INT NOT NULL,
    id_docente INT NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    puntos DECIMAL(5,2) NOT NULL,
    porcentaje DECIMAL(5,2) NULL,
    fecha DATE NULL,
    estado BIT NOT NULL CONSTRAINT DF_Actividad_Estado DEFAULT 1,
    fecha_creacion DATETIME2(0) NOT NULL CONSTRAINT DF_Actividad_FechaCreacion DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Actividad_Evaluacion PRIMARY KEY (id_actividad),
    CONSTRAINT CK_Actividad_Puntos CHECK (puntos > 0 AND puntos <= 100),
    CONSTRAINT CK_Actividad_Porcentaje CHECK (porcentaje IS NULL OR (porcentaje > 0 AND porcentaje <= 100)),
    CONSTRAINT FK_Actividad_Rubro FOREIGN KEY (id_rubro) REFERENCES Rubro_Evaluacion(id_rubro),
    CONSTRAINT FK_Actividad_Periodo FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo),
    CONSTRAINT FK_Actividad_Curso FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT FK_Actividad_Seccion FOREIGN KEY (id_seccion) REFERENCES Seccion(id_seccion),
    CONSTRAINT FK_Actividad_Grupo FOREIGN KEY (id_grupo) REFERENCES Grupo(id_grupo),
    CONSTRAINT FK_Actividad_Docente FOREIGN KEY (id_docente) REFERENCES Docente(id_docente)
);
GO

-- No se repite el nombre de una actividad activa dentro del mismo rubro, curso, sección, grupo y período
CREATE UNIQUE INDEX UQ_Actividad_Nombre
    ON Actividad_Evaluacion(id_periodo, id_curso, id_seccion, id_grupo, id_rubro, nombre)
    WHERE estado = 1;

CREATE INDEX IX_Actividad_Clase
    ON Actividad_Evaluacion(id_periodo, id_curso, id_seccion, id_grupo);
GO

-- Notas: una por matrícula y rubro. 
-- La matrícula ya trae estudiante, período, grupo y sección.
-- La nota de cada rubro es el resultado que el docente asigna (0-100).
CREATE TABLE Nota
(
    id_nota INT IDENTITY(1,1) NOT NULL,
    id_matricula INT NOT NULL,
    id_actividad INT NOT NULL,
    puntos_obtenidos DECIMAL(5,2) NOT NULL,
    observacion NVARCHAR(500) NULL,
    id_usuario_registro INT NOT NULL,
    fecha_registro DATETIME2(0) NOT NULL CONSTRAINT DF_Nota_Fecha DEFAULT SYSDATETIME(),
    fecha_modificacion DATETIME2(0) NULL,

    CONSTRAINT PK_Nota PRIMARY KEY (id_nota),
    CONSTRAINT UQ_Nota_Matricula_Actividad UNIQUE (id_matricula, id_actividad),
    CONSTRAINT CK_Nota_Puntos CHECK (puntos_obtenidos >= 0 AND puntos_obtenidos <= 100),
    CONSTRAINT FK_Nota_Matricula FOREIGN KEY (id_matricula) REFERENCES Matricula(id_matricula),
    CONSTRAINT FK_Nota_Actividad FOREIGN KEY (id_actividad) REFERENCES Actividad_Evaluacion(id_actividad),
    CONSTRAINT FK_Nota_UsuarioRegistro FOREIGN KEY (id_usuario_registro) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_Nota_Actividad ON Nota(id_actividad);
GO

-- Historial de cambios de cada nota. 
CREATE TABLE Nota_Historial
(
    id_historial INT IDENTITY(1,1) NOT NULL,
    id_nota INT NOT NULL,
    id_usuario INT NOT NULL,
    tipo_cambio NVARCHAR(20) NOT NULL,
    valor_anterior DECIMAL(5,2) NULL,
    valor_nuevo DECIMAL(5,2) NOT NULL,
    observacion NVARCHAR(500) NULL,
    fecha_cambio DATETIME2(0) NOT NULL CONSTRAINT DF_NotaHistorial_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Nota_Historial PRIMARY KEY (id_historial),
    CONSTRAINT CK_NotaHistorial_Tipo CHECK (tipo_cambio IN (N'Registro', N'Edicion', N'Correccion')),
    CONSTRAINT FK_NotaHistorial_Nota FOREIGN KEY (id_nota) REFERENCES Nota(id_nota),
    CONSTRAINT FK_NotaHistorial_Usuario FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_NotaHistorial_Nota_Fecha ON Nota_Historial(id_nota, fecha_cambio DESC);
GO

-- Autorización de correcciones con el período cerrado. 
-- Una autorización sirve para UN cambio (usada = 1 después).
CREATE TABLE Correccion_Autorizada
(
    id_autorizacion INT IDENTITY(1,1) NOT NULL,
    id_nota INT NOT NULL,
    id_usuario_autoriza INT NOT NULL,
    motivo NVARCHAR(500) NOT NULL,
    usada BIT NOT NULL CONSTRAINT DF_CorrAut_Usada DEFAULT 0,
    fecha_autorizacion DATETIME2(0) NOT NULL CONSTRAINT DF_CorrAut_Fecha DEFAULT SYSDATETIME(),
    fecha_uso DATETIME2(0) NULL,

    CONSTRAINT PK_Correccion_Autorizada PRIMARY KEY (id_autorizacion),
    CONSTRAINT CK_CorrAut_Motivo CHECK (LEN(LTRIM(RTRIM(motivo))) > 0),
    CONSTRAINT FK_CorrAut_Nota FOREIGN KEY (id_nota) REFERENCES Nota(id_nota),
    CONSTRAINT FK_CorrAut_Usuario FOREIGN KEY (id_usuario_autoriza) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_CorrAut_Nota_Usada ON Correccion_Autorizada(id_nota, usada);
GO

-- Configuración (una sola fila). 
-- nota_minima: mínimo para aprobar.
-- Asistencia = 5% de la nota final (MEP): rubros 95% + asistencia 5% = 100%.
CREATE TABLE Configuracion_Calificacion
(
    id_config INT NOT NULL,
    nota_minima DECIMAL(5,2) NOT NULL CONSTRAINT DF_ConfCal_Minima DEFAULT 70,
    considerar_asistencia BIT NOT NULL CONSTRAINT DF_ConfCal_UsaAsist DEFAULT 1,
    porcentaje_asistencia DECIMAL(5,2) NOT NULL CONSTRAINT DF_ConfCal_PctAsist DEFAULT 5,

    CONSTRAINT PK_Configuracion_Calificacion PRIMARY KEY (id_config),
    CONSTRAINT CK_ConfCal_UnaFila CHECK (id_config = 1),
    CONSTRAINT CK_ConfCal_Minima CHECK (nota_minima >= 0 AND nota_minima <= 100),
    CONSTRAINT CK_ConfCal_PctAsist CHECK (porcentaje_asistencia >= 0 AND porcentaje_asistencia < 100)
);
GO

INSERT INTO Configuracion_Calificacion (id_config) VALUES (1);
GO


/* ============================================================
   8. BECAS Y RECURSOS ACADÉMICOS
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
    id_docente INT NOT NULL,
    id_curso INT NOT NULL,
    id_grado INT NOT NULL,
    titulo NVARCHAR(200) NOT NULL,
    descripcion NVARCHAR(500) NULL,
    tipo_recurso NVARCHAR(30) NOT NULL,
    modalidad NVARCHAR(10) NOT NULL,
    nombre_archivo_original NVARCHAR(260) NULL,
    ruta_archivo NVARCHAR(300) NULL,
    extension NVARCHAR(10) NULL,
    tamano_bytes BIGINT NULL,
    hash_archivo CHAR(64) NULL,
    url_enlace NVARCHAR(500) NULL,
    fecha_publicacion DATETIME NOT NULL
        CONSTRAINT DF_Recurso_FechaPublicacion DEFAULT GETUTCDATE(),
    fecha_modificacion DATETIME NULL,

    CONSTRAINT PK_Recursos_Academicos PRIMARY KEY (id_recurso),
    CONSTRAINT CK_Recurso_Modalidad
        CHECK (modalidad IN (N'Archivo', N'Enlace')),
    CONSTRAINT CK_Recurso_Contenido
        CHECK ((modalidad = N'Archivo' AND ruta_archivo IS NOT NULL
                                       AND hash_archivo IS NOT NULL
                                       AND url_enlace IS NULL)
            OR (modalidad = N'Enlace'  AND url_enlace IS NOT NULL
                                       AND ruta_archivo IS NULL
                                       AND hash_archivo IS NULL)),
    CONSTRAINT FK_Recurso_Docente
        FOREIGN KEY (id_docente) REFERENCES Docente(id_docente),
    CONSTRAINT FK_Recurso_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT FK_Recurso_Grado
        FOREIGN KEY (id_grado) REFERENCES Grado(id_grado)
);
GO

CREATE UNIQUE INDEX UX_Recurso_Curso_Hash
    ON Recursos_Academicos(id_curso, hash_archivo)
    WHERE hash_archivo IS NOT NULL;

CREATE UNIQUE INDEX UX_Recurso_Curso_Url
    ON Recursos_Academicos(id_curso, url_enlace)
    WHERE url_enlace IS NOT NULL;
GO


/* ============================================================
   9. COMUNICACIÓN Y REPORTES
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
   10. INVENTARIO
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
    CONSTRAINT CK_Inventario_Cantidad
        CHECK (cantidad >= 0),
    CONSTRAINT FK_Inventario_Categoria
        FOREIGN KEY (id_categoria) REFERENCES Categoria_Inv(id_categoria),
    CONSTRAINT FK_Inventario_Personal_Admin
        FOREIGN KEY (id_perAdm) REFERENCES Personal_Admin(id_perAdm)
);
GO


/* ============================================================
   11. RECUPERACIÓN DE CONTRASEÑA
   ============================================================ */

CREATE TABLE Usuario_Token_Reset
(
    id_token INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    token_hash VARBINARY(64) NOT NULL,
    fecha_creacion DATETIME NOT NULL
        CONSTRAINT DF_TokenReset_FechaCreacion DEFAULT GETUTCDATE(),
    fecha_expiracion DATETIME NOT NULL,
    usado BIT NOT NULL CONSTRAINT DF_TokenReset_Usado DEFAULT 0,

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
   12. AUDITORÍA
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

CREATE INDEX IX_LogAcceso_Usuario
    ON Log_Acceso(id_usuario);

CREATE INDEX IX_LogAcceso_Fecha
    ON Log_Acceso(fecha);
GO


/* ============================================================
   13. CONFIGURACIÓN DEL SITIO
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
   14. BOLETERÍA
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

/* Un solo tiquete disponible por usuario. */
CREATE UNIQUE INDEX UX_Tiquete_Usuario_Disponible
    ON Tiquete(id_usuario)
    WHERE estado = N'Disponible';
GO


/* ============================================================
   XX. DATOS INICIALES DE PRUEBA
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
    (N'Docentes', N'Expedientes del personal docente: registro, edición, estado y consulta.'),
    (N'Estudiantes', N'Expedientes estudiantiles: registro, edición, estado, carné y consulta.'),
    (N'Recursos', N'Publicación, consulta y supervisión de recursos académicos.'),
    (N'AsignacionCursos', N'Asignación de cursos a docentes por período lectivo, consulta e historial.'),
    (N'Matriculas', N'Registro, consulta, modificación, traslados, historial y reportes de matrícula.'),
    (N'Matricula', N'Registro, consulta, edición, traslados e informes de matrícula.');
GO

/* Permisos por rol y módulo: ver, crear, editar, eliminar. */
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
    p.ver,
    p.crear,
    p.editar,
    p.eliminar
FROM
(
    VALUES
        -- Administrador
        (N'Administrador',           N'Usuarios',         1, 1, 1, 1),
        (N'Administrador',           N'Roles',            1, 1, 1, 1),
        (N'Administrador',           N'Permisos',         1, 1, 1, 1),
        (N'Administrador',           N'Auditoria',        1, 1, 1, 1),
        (N'Administrador',           N'Configuracion',    1, 1, 1, 1),
        (N'Administrador',           N'Boleteria',        1, 1, 1, 1),
        (N'Administrador',           N'Grados',           1, 1, 1, 0),
        (N'Administrador',           N'Cursos',           1, 1, 1, 1),
        (N'Administrador',           N'Docentes',         1, 1, 1, 1),
        (N'Administrador',           N'Estudiantes',      1, 1, 1, 1),
        (N'Administrador',           N'Recursos',         1, 0, 0, 0),
        (N'Administrador',           N'AsignacionCursos', 1, 1, 1, 1),
        (N'Administrador',           N'Matriculas',       1, 1, 1, 1),
        (N'Administrador',           N'Matricula',        1, 1, 1, 0),

        -- Director
        (N'Director',                N'Docentes',         1, 0, 0, 0),
        (N'Director',                N'Estudiantes',      1, 0, 0, 0),
        (N'Director',                N'Recursos',         1, 0, 0, 0),
        (N'Director',                N'AsignacionCursos', 1, 1, 0, 1),
        (N'Director',                N'Matricula',        1, 1, 1, 0),

        -- Docente
        (N'Docente',                 N'Recursos',         1, 1, 1, 1),

        -- Estudiante
        (N'Estudiante',              N'Recursos',         1, 0, 0, 0),

        -- Personal administrativo
        (N'Personal administrativo', N'Matriculas',       1, 1, 1, 1),
        (N'Personal administrativo', N'Matricula',        1, 1, 1, 0)
) AS p (rol, modulo, ver, crear, editar, eliminar)
JOIN Rol r ON r.nombre = p.rol
JOIN Modulo m ON m.nombre = p.modulo;
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
   16. DATOS DE PRUEBA

   Usuarios de prueba:
     admin@lnsd.local              /  Admin123!         (rol Administrador)
     estudiante.prueba@lnsd.local  /  Estudiante123*    (rol Estudiante)
     estudiante2.prueba@lnsd.local /  Estudiante123*
     estudiante3.prueba@lnsd.local /  Estudiante123*
     encargado.prueba@lnsd.local   /  Encargado123*     (rol Encargado) de estudiante.prueba
     FMendez@ejemplo.com           /  aaaaaaaa
     docente.prueba@lnsd.local     /  Docente123*       (rol Docente)
     director.prueba@lnsd.local    /  Director123*      (rol Director)
   ============================================================ */

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
),
(
    (SELECT id_rol FROM Rol WHERE nombre = N'Estudiante'),
    N'Estudiante',
    N'Prueba',
    N'estudiante.prueba@lnsd.local',
    0x0100000001000186A0000000108760C60D5E72121B52832E5B76E8C2D38B7594203BFD27E0345D59EB4C449C76A4166C59C2F2C683396299ABE595520A,
    1
),
(
    (SELECT id_rol FROM Rol WHERE nombre = N'Encargado'),
    N'Encargado',
    N'Prueba',
    N'encargado.prueba@lnsd.local',
    0x0100000001000186A000000010979CBF60A620784CF213010E9F5B14A1FEE56743FBC1CFEFFF35E2CCEA915B377F1FC3CBF9DABB4591A82D8BC0E60B2F,
    1
);
GO

INSERT INTO Estudiante
(
    id_usuario,
    identificacion,
    carnet,
    fecha_ingreso,
    fecha_nacimiento,
    telefono,
    direccion,
    correo_emergencia,
    id_grado,
    estado_academico,
    alergias,
    observaciones_medicas,
    adecuaciones_educativas,
    estado
)
VALUES
(
    (SELECT id_usuario FROM Usuario WHERE correo = N'estudiante.prueba@lnsd.local'),
    N'1-1111-1111',
    N'PRUEBA-0001',
    '2026-02-01',
    '2012-05-10',
    N'8888-0001',
    N'San José, Costa Rica',
    N'emergencia.prueba@lnsd.local',
    (SELECT TOP 1 id_grado FROM Grado ORDER BY id_grado),
    N'Regular',
    N'Alergia al maní',
    N'Usa lentes',
    N'Tiempo adicional en exámenes',
    1
);
GO

INSERT INTO Encargado
(
    id_usuario,
    telefono
)
VALUES
(
    (SELECT id_usuario FROM Usuario WHERE correo = N'encargado.prueba@lnsd.local'),
    N'8888-0002'
);
GO

INSERT INTO Encargado_Estudiante
(
    id_encargado,
    id_estudiante,
    parentesco,
    es_principal
)
VALUES
(
    (SELECT en.id_encargado
     FROM Encargado en
     JOIN Usuario u ON u.id_usuario = en.id_usuario
     WHERE u.correo = N'encargado.prueba@lnsd.local'),
    (SELECT es.id_estudiante
     FROM Estudiante es
     JOIN Usuario u ON u.id_usuario = es.id_usuario
     WHERE u.correo = N'estudiante.prueba@lnsd.local'),
    N'Madre',
    1
);
GO

USE Proyecto_LNSD_DB;
GO
 
SET NOCOUNT ON;
SET XACT_ABORT ON;
 
BEGIN TRAN;
 
DECLARE @hashDocente  VARBINARY(256) = 0x0100000001000186A000000010959499D207F9274D7793E020A8A296BB801A347D0C0C62579F8985D6D0A37B1C850A7899216AED4D605AD9756F15D832;
DECLARE @hashDirector VARBINARY(256) = 0x0100000001000186A0000000106AA91A711AE3510EB75F380235D6693AEEF1B09ADBE1A2985683D4A5D8CCA310F58F62D052604301ED990A6AFAD365CD;
DECLARE @hashEstud    VARBINARY(256) = 0x0100000001000186A0000000108760C60D5E72121B52832E5B76E8C2D38B7594203BFD27E0345D59EB4C449C76A4166C59C2F2C683396299ABE595520A;
 
/* ---- Período lectivo: se usa el activo; si no hay, se crea uno ---- */
DECLARE @idPeriodo INT = (SELECT TOP 1 id_periodo FROM Periodo_Lectivo WHERE activo = 1);
 
IF @idPeriodo IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Periodo_Lectivo WHERE nombre = N'Período 2026 prueba')
        INSERT INTO Periodo_Lectivo (nombre, fecha_inicio, fecha_fin, activo)
        VALUES (N'Período 2026 prueba', '2026-02-01', '2026-12-15', 0);
 
    UPDATE Periodo_Lectivo SET activo = 1 WHERE nombre = N'Período 2026 prueba';
    SET @idPeriodo = (SELECT id_periodo FROM Periodo_Lectivo WHERE nombre = N'Período 2026 prueba');
END
 
/* ---- Usuarios ---- */
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = N'docente.prueba@lnsd.local')
    INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
    VALUES ((SELECT id_rol FROM Rol WHERE nombre = N'Docente'), N'Docente', N'Prueba',
            N'docente.prueba@lnsd.local', @hashDocente, 1);
 
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = N'director.prueba@lnsd.local')
    INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
    VALUES ((SELECT id_rol FROM Rol WHERE nombre = N'Director'), N'Director', N'Prueba',
            N'director.prueba@lnsd.local', @hashDirector, 1);
 
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = N'estudiante2.prueba@lnsd.local')
    INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
    VALUES ((SELECT id_rol FROM Rol WHERE nombre = N'Estudiante'), N'María', N'Rojas Prueba',
            N'estudiante2.prueba@lnsd.local', @hashEstud, 1);
 
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE correo = N'estudiante3.prueba@lnsd.local')
    INSERT INTO Usuario (id_rol, nombre, apellido, correo, password_hash, estado)
    VALUES ((SELECT id_rol FROM Rol WHERE nombre = N'Estudiante'), N'Luis', N'Vargas Prueba',
            N'estudiante3.prueba@lnsd.local', @hashEstud, 1);
 
/* ---- Grado, sección, grupo y curso ---- */
IF NOT EXISTS (SELECT 1 FROM Grado WHERE codigo = N'PRB-7')
    INSERT INTO Grado (codigo, nombre, nivel, estado) VALUES (N'PRB-7', N'Sétimo prueba', N'Secundaria', 1);
 
DECLARE @idGrado INT = (SELECT id_grado FROM Grado WHERE codigo = N'PRB-7');
 
IF NOT EXISTS (SELECT 1 FROM Seccion WHERE id_grado = @idGrado AND nombre = N'A')
    INSERT INTO Seccion (id_grado, nombre, capacidad_maxima, estado) VALUES (@idGrado, N'A', 30, 1);
 
DECLARE @idSeccion INT = (SELECT id_seccion FROM Seccion WHERE id_grado = @idGrado AND nombre = N'A');
 
IF NOT EXISTS (SELECT 1 FROM Grupo WHERE nombre = N'A' AND nivel = N'Secundaria' AND anio = 2026)
    INSERT INTO Grupo (nombre, nivel, anio) VALUES (N'A', N'Secundaria', 2026);
 
DECLARE @idGrupo INT = (SELECT id_grupo FROM Grupo WHERE nombre = N'A' AND nivel = N'Secundaria' AND anio = 2026);
 
IF NOT EXISTS (SELECT 1 FROM Curso WHERE codigo = N'MAT-PRB')
    INSERT INTO Curso (codigo, nombre, descripcion, id_area, estado)
    VALUES (N'MAT-PRB', N'Matemática prueba', N'Curso de prueba para el módulo de calificaciones',
            (SELECT id_area FROM Area_Academica WHERE nombre = N'Matemáticas'), 1);
 
DECLARE @idCurso INT = (SELECT id_curso FROM Curso WHERE codigo = N'MAT-PRB');
 
IF NOT EXISTS (SELECT 1 FROM Curso_Grado WHERE id_curso = @idCurso AND id_grado = @idGrado)
    INSERT INTO Curso_Grado (id_curso, id_grado) VALUES (@idCurso, @idGrado);
 
/* ---- Docente, asignación del curso y horario ---- */
IF NOT EXISTS (SELECT 1 FROM Docente WHERE correo = N'docente.prueba@lnsd.local')
    INSERT INTO Docente (id_usuario, nombre, apellidos, identificacion, correo, telefono, estado, especialidad, id_area)
    VALUES ((SELECT id_usuario FROM Usuario WHERE correo = N'docente.prueba@lnsd.local'),
            N'Docente', N'Prueba', N'2-2222-2222', N'docente.prueba@lnsd.local', N'8888-0003', 1,
            N'Matemática', (SELECT id_area FROM Area_Academica WHERE nombre = N'Matemáticas'));
 
DECLARE @idDocente INT = (SELECT id_docente FROM Docente WHERE correo = N'docente.prueba@lnsd.local');
 
IF NOT EXISTS (SELECT 1 FROM Docente_Curso WHERE id_docente = @idDocente AND id_curso = @idCurso AND id_periodo = @idPeriodo)
    INSERT INTO Docente_Curso (id_docente, id_curso, id_periodo) VALUES (@idDocente, @idCurso, @idPeriodo);
 
IF NOT EXISTS (SELECT 1 FROM Horario WHERE id_docente = @idDocente AND id_periodo = @idPeriodo AND dia = N'Lunes' AND hora_inicio = '07:00')
    INSERT INTO Horario (id_docente, id_curso, dia, hora_inicio, hora_fin, id_periodo, id_seccion, id_grupo, aula, estado)
    VALUES (@idDocente, @idCurso, N'Lunes', '07:00', '08:20', @idPeriodo, @idSeccion, @idGrupo, N'Aula 1', 1);
 
IF NOT EXISTS (SELECT 1 FROM Horario WHERE id_docente = @idDocente AND id_periodo = @idPeriodo AND dia = N'Miércoles' AND hora_inicio = '07:00')
    INSERT INTO Horario (id_docente, id_curso, dia, hora_inicio, hora_fin, id_periodo, id_seccion, id_grupo, aula, estado)
    VALUES (@idDocente, @idCurso, N'Miércoles', '07:00', '08:20', @idPeriodo, @idSeccion, @idGrupo, N'Aula 1', 1);
 
/* ---- Estudiantes ---- */
IF NOT EXISTS (SELECT 1 FROM Estudiante WHERE carnet = N'PRUEBA-0002')
    INSERT INTO Estudiante (id_usuario, identificacion, carnet, fecha_ingreso, fecha_nacimiento, id_grado, estado_academico, estado)
    VALUES ((SELECT id_usuario FROM Usuario WHERE correo = N'estudiante2.prueba@lnsd.local'),
            N'1-2222-2222', N'PRUEBA-0002', '2026-02-01', '2012-03-15', @idGrado, N'Regular', 1);
 
IF NOT EXISTS (SELECT 1 FROM Estudiante WHERE carnet = N'PRUEBA-0003')
    INSERT INTO Estudiante (id_usuario, identificacion, carnet, fecha_ingreso, fecha_nacimiento, id_grado, estado_academico, estado)
    VALUES ((SELECT id_usuario FROM Usuario WHERE correo = N'estudiante3.prueba@lnsd.local'),
            N'1-3333-3333', N'PRUEBA-0003', '2026-02-01', '2012-08-20', @idGrado, N'Regular', 1);
 
UPDATE Estudiante SET id_grado = @idGrado WHERE carnet = N'PRUEBA-0001' AND id_grado IS NULL;
 
/* ---- Matrículas activas en el curso ---- */
INSERT INTO Matricula (id_estudiante, id_periodo, id_grupo, id_seccion, estado)
SELECT e.id_estudiante, @idPeriodo, @idGrupo, @idSeccion, N'Activa'
FROM Estudiante e
WHERE e.carnet IN (N'PRUEBA-0001', N'PRUEBA-0002', N'PRUEBA-0003')
  AND NOT EXISTS (SELECT 1 FROM Matricula m
                  WHERE m.id_estudiante = e.id_estudiante
                    AND m.id_periodo = @idPeriodo
                    AND m.estado <> N'Anulada');
 
INSERT INTO Matricula_Curso (id_matricula, id_curso)
SELECT m.id_matricula, @idCurso
FROM Matricula m
JOIN Estudiante e ON e.id_estudiante = m.id_estudiante
WHERE e.carnet IN (N'PRUEBA-0001', N'PRUEBA-0002', N'PRUEBA-0003')
  AND m.id_periodo = @idPeriodo
  AND m.estado = N'Activa'
  AND NOT EXISTS (SELECT 1 FROM Matricula_Curso mc
                  WHERE mc.id_matricula = m.id_matricula AND mc.id_curso = @idCurso);
 
COMMIT;

/* ---- Verificación: debe devolver 3 estudiantes en el curso de prueba ---- */
SELECT e.carnet, u.apellido, u.nombre, m.estado AS estado_matricula, s.nombre AS seccion, g.nombre AS grupo
FROM Matricula m
JOIN Matricula_Curso mc ON mc.id_matricula = m.id_matricula
JOIN Curso c ON c.id_curso = mc.id_curso AND c.codigo = N'MAT-PRB'
JOIN Estudiante e ON e.id_estudiante = m.id_estudiante
JOIN Usuario u ON u.id_usuario = e.id_usuario
JOIN Seccion s ON s.id_seccion = m.id_seccion
JOIN Grupo g ON g.id_grupo = m.id_grupo
ORDER BY u.apellido;
GO

/* ---- Horarios (MHOF-01) ---- */

-- 1) Si Horario ya tiene datos con la estructura antigua, hay que revisarlos antes
IF COL_LENGTH(N'dbo.Horario', N'id_periodo') IS NULL
   AND EXISTS (SELECT 1 FROM Horario)
    THROW 50002, N'Horario tiene datos con la estructura antigua. Revisar antes de modificarla.', 1;
GO

-- 2) Columnas nuevas (con ALTER, para no romper la FK de Asistencia)
IF COL_LENGTH(N'dbo.Horario', N'id_periodo') IS NULL
    ALTER TABLE Horario ADD id_periodo INT NULL;
GO
IF COL_LENGTH(N'dbo.Horario', N'id_seccion') IS NULL
    ALTER TABLE Horario ADD id_seccion INT NULL;
GO
IF COL_LENGTH(N'dbo.Horario', N'id_grupo') IS NULL
    ALTER TABLE Horario ADD id_grupo INT NULL;
GO
IF COL_LENGTH(N'dbo.Horario', N'aula') IS NULL
    ALTER TABLE Horario ADD aula NVARCHAR(50) NULL;
GO
IF COL_LENGTH(N'dbo.Horario', N'estado') IS NULL
    ALTER TABLE Horario ADD estado BIT NOT NULL
        CONSTRAINT DF_Horario_Estado DEFAULT 1;
GO

-- 3) Con la tabla vacía, las columnas nuevas pasan a ser obligatorias
IF NOT EXISTS (SELECT 1 FROM Horario)
BEGIN
    ALTER TABLE Horario ALTER COLUMN id_periodo INT NOT NULL;
    ALTER TABLE Horario ALTER COLUMN id_seccion INT NOT NULL;
    ALTER TABLE Horario ALTER COLUMN id_grupo INT NOT NULL;
    ALTER TABLE Horario ALTER COLUMN aula NVARCHAR(50) NOT NULL;
END
GO

-- 4) Relaciones y regla de horas
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Horario_Periodo')
    ALTER TABLE Horario ADD CONSTRAINT FK_Horario_Periodo
        FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Horario_Seccion')
    ALTER TABLE Horario ADD CONSTRAINT FK_Horario_Seccion
        FOREIGN KEY (id_seccion) REFERENCES Seccion(id_seccion);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Horario_Grupo')
    ALTER TABLE Horario ADD CONSTRAINT FK_Horario_Grupo
        FOREIGN KEY (id_grupo) REFERENCES Grupo(id_grupo);
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Horario_Horas')
    ALTER TABLE Horario ADD CONSTRAINT CK_Horario_Horas
        CHECK (hora_fin > hora_inicio);
GO

-- 5) La restricción única anterior no distinguía período ni sección
IF EXISTS (SELECT 1 FROM sys.key_constraints
           WHERE name = N'UQ_Horario_Docente_Dia_Hora'
             AND parent_object_id = OBJECT_ID(N'dbo.Horario'))
    ALTER TABLE Horario DROP CONSTRAINT UQ_Horario_Docente_Dia_Hora;
GO

-- 6) Protección contra el mismo bloque duplicado (los traslapes parciales los valida el servicio)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Horario_Docente_Bloque')
    CREATE UNIQUE INDEX UX_Horario_Docente_Bloque
        ON Horario(id_periodo, id_docente, dia, hora_inicio)
        WHERE estado = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Horario_Seccion_Bloque')
    CREATE UNIQUE INDEX UX_Horario_Seccion_Bloque
        ON Horario(id_periodo, id_seccion, id_grupo, dia, hora_inicio)
        WHERE estado = 1;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Horario_Periodo_Seccion')
    CREATE INDEX IX_Horario_Periodo_Seccion
        ON Horario(id_periodo, id_seccion, id_grupo);
GO

-- 7) Módulo y permisos
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Horarios')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Horarios', N'Registro, consulta y administración de horarios de clases por período lectivo.');
GO

INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, p.ver, p.crear, p.editar, p.eliminar
FROM (VALUES
        (N'Administrador', 1, 1, 1, 0),
        (N'Director',      1, 1, 1, 0),
        (N'Docente',       1, 0, 0, 0),
        (N'Estudiante',    1, 0, 0, 0),
        (N'Encargado',     1, 0, 0, 0)
     ) AS p(rol, ver, crear, editar, eliminar)
JOIN Rol r ON r.nombre = p.rol
JOIN Modulo m ON m.nombre = N'Horarios'
WHERE NOT EXISTS (SELECT 1 FROM Rol_Permiso rp
                  WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO

/* ---- Calificaciones (MCNF-01) ---- */

-- Módulo y permisos (sembrados aquí para no depender
-- de la pantalla de permisos).
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Calificaciones')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Calificaciones', N'Registro, consulta, corrección y seguimiento de calificaciones por curso y período lectivo.');
GO

INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, p.ver, p.crear, p.editar, p.eliminar
FROM (VALUES
        (N'Administrador', 1, 1, 1, 1),
        (N'Director',      1, 0, 1, 0),
        (N'Docente',       1, 1, 1, 0),
        (N'Estudiante',    1, 0, 0, 0),
        (N'Encargado',     1, 0, 0, 0)
     ) AS p(rol, ver, crear, editar, eliminar)
JOIN Rol r ON r.nombre = p.rol
JOIN Modulo m ON m.nombre = N'Calificaciones'
WHERE NOT EXISTS (SELECT 1 FROM Rol_Permiso rp
                  WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO