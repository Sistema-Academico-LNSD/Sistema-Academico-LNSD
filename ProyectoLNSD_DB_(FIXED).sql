/* ============================================================
   PROYECTO LNSD - SCRIPT DE BASE DE DATOS (versión unificada)
   Motor: SQL Server. Se ejecuta COMPLETO sobre un servidor limpio: crea la base desde cero.

   PARTE 1. ESTRUCTURA (una sola definición por tabla, en orden de dependencia)
      1. Seguridad y usuarios            10. Becas
      2. Áreas académicas                11. Recursos académicos
      3. Grados, secciones y períodos    12. Comunicación y reportes
      4. Docentes, estudiantes, encargados 13. Inventario
      5. Cursos                          14. Recuperación de contraseña
      6. Matrícula                       15. Auditoría
      7. Horarios                        16. Configuración del sitio
      8. Asistencia                      17. Boletería
      9. Calificaciones                  18. Pautas de actuación (MEP)

   PARTE 2. DATOS
      19. Roles, áreas, módulos y permisos base
      20. Módulos y permisos de los módulos posteriores
      21. Datos de prueba (usuarios, estudiantes y curso de prueba)

   Para agregar un módulo nuevo: sus tablas van en la PARTE 1 (antes de la sección 19)
   y su módulo y permisos en la sección 20.
   ============================================================ */

CREATE DATABASE Proyecto_LNSD_DB;
GO

USE Proyecto_LNSD_DB;
GO

/* ############################################################
   PARTE 1. ESTRUCTURA
   ############################################################ */

/* ============================================================
   1. SEGURIDAD Y USUARIOS
   Roles, usuarios, módulos del sistema y permisos por rol.
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
   Catálogo de áreas (Matemáticas, Ciencias, etc.).
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
   Estructura académica y períodos lectivos.
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
   Expedientes de personas y su relación (encargado - estudiante, estudiante - docente).
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
   Oferta de cursos, cursos por grado y asignación a docentes por período.
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
   Matrícula por período, cursos matriculados, observaciones e historial.
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
   7. HORARIOS
   Horario de clases por período, docente, curso, sección y grupo (MHOF-01).
   ============================================================ */

-- Un bloque de clase: docente + curso + sección + grupo, un día y una hora, dentro de un período lectivo.
-- Los índices únicos filtrados (estado = 1) impiden el mismo bloque duplicado;
-- los traslapes parciales de hora los valida el servicio.
CREATE TABLE Horario
(
    id_horario INT IDENTITY(1,1) NOT NULL,
    id_periodo INT NOT NULL,
    id_docente INT NOT NULL,
    id_curso INT NOT NULL,
    id_seccion INT NOT NULL,
    id_grupo INT NOT NULL,
    dia NVARCHAR(15) NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    aula NVARCHAR(50) NOT NULL,
    estado BIT NOT NULL
        CONSTRAINT DF_Horario_Estado DEFAULT 1,

    CONSTRAINT PK_Horario PRIMARY KEY (id_horario),
    CONSTRAINT CK_Horario_Horas CHECK (hora_fin > hora_inicio),
    CONSTRAINT FK_Horario_Periodo
        FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo),
    CONSTRAINT FK_Horario_Docente
        FOREIGN KEY (id_docente) REFERENCES Docente(id_docente),
    CONSTRAINT FK_Horario_Curso
        FOREIGN KEY (id_curso) REFERENCES Curso(id_curso),
    CONSTRAINT FK_Horario_Seccion
        FOREIGN KEY (id_seccion) REFERENCES Seccion(id_seccion),
    CONSTRAINT FK_Horario_Grupo
        FOREIGN KEY (id_grupo) REFERENCES Grupo(id_grupo)
);
GO

CREATE INDEX IX_Horario_Docente
    ON Horario(id_docente);

CREATE INDEX IX_Horario_Curso
    ON Horario(id_curso);

CREATE INDEX IX_Horario_Periodo_Seccion
    ON Horario(id_periodo, id_seccion, id_grupo);
GO

CREATE UNIQUE INDEX UX_Horario_Docente_Bloque
    ON Horario(id_periodo, id_docente, dia, hora_inicio)
    WHERE estado = 1;

CREATE UNIQUE INDEX UX_Horario_Seccion_Bloque
    ON Horario(id_periodo, id_seccion, id_grupo, dia, hora_inicio)
    WHERE estado = 1;
GO

/* ============================================================
   8. ASISTENCIA
   Asistencia por clase y fecha, y cierre por período (MCAF-01).
   ============================================================ */

/* ------------------------------------------------------------
   8.1 Asistencia = una CLASE del horario en una FECHA, por estudiante.
      - id_horario  -> período, curso, docente, sección (grado) y grupo
      - id_matricula -> el estudiante en ese período
   ------------------------------------------------------------ */


-- "estado": solo 'Ausente' resta a la nota de asistencia (5% de la nota anual); cualquier otro estado
-- (Presente, Tardia, Justificada) cuenta como asistencia. El porcentaje se calcula con los registros del período.
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
   8.2 Cierre del registro de asistencia por período (MCAF-01-13).
      Que exista la fila = el período está cerrado.
      Tabla aparte para no tocar Periodo_Lectivo (Configuración).
   ------------------------------------------------------------ */
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

/* ============================================================
   9. CALIFICACIONES
   Rubros, actividades, notas, historial, correcciones y configuración (MCNF-01).
   ============================================================ */

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
   10. BECAS
   Requisitos, becas por período y requisitos cumplidos (MBEF-01).
   ============================================================ */

/* ------------------------------------------------------------
   10.1 Requisitos de elegibilidad (MBEF-01-01).
      - obligatorio = 1: se exige para activar la beca.
      - verificacion: NULL = lo marca el administrador;
        'MatriculaActiva' = lo comprueba el sistema con la matrícula
        del estudiante en el período de la beca.
      - No se borran: se desactivan, para no perder el historial
        de lo que cumplió cada beca.
   ------------------------------------------------------------ */
CREATE TABLE Beca_Requisito
(
    id_requisito INT IDENTITY(1,1) NOT NULL,
    nombre       NVARCHAR(150) NOT NULL,
    descripcion  NVARCHAR(500) NULL,
    obligatorio  BIT NOT NULL CONSTRAINT DF_BecaRequisito_Obligatorio DEFAULT 1,
    verificacion NVARCHAR(30) NULL,
    activo       BIT NOT NULL CONSTRAINT DF_BecaRequisito_Activo DEFAULT 1,

    CONSTRAINT PK_Beca_Requisito PRIMARY KEY (id_requisito),
    CONSTRAINT UQ_BecaRequisito_Nombre UNIQUE (nombre),
    CONSTRAINT CK_BecaRequisito_Verificacion
        CHECK (verificacion IS NULL OR verificacion IN (N'MatriculaActiva'))
);
GO

INSERT INTO Beca_Requisito (nombre, descripcion, obligatorio, verificacion)
VALUES
    (N'Condición de vulnerabilidad según SINIRUBE',
     N'El hogar del estudiante está clasificado en pobreza extrema, pobreza o vulnerabilidad en el SINIRUBE.',
     1, NULL),
    (N'Matrícula activa',
     N'El estudiante tiene matrícula activa en el período lectivo de la beca. La verifica el sistema.',
     1, N'MatriculaActiva'),
    (N'Aprobación del grado anterior',
     N'El estudiante aprobó el grado cursado en el período lectivo anterior.',
     1, NULL);
GO

/* ------------------------------------------------------------
   10.2 Beca de un estudiante en un período lectivo (MBEF-01-02 a 01-06).
      - Una sola beca por estudiante y período.
      - Vigencia (MBEF-01-06): la aplicación exige que inicio y fin
        estén dentro del período lectivo (máximo un año lectivo).
      - Estados: Pendiente (registrada, sin activar), Activa, Inactiva.
   ------------------------------------------------------------ */
CREATE TABLE Beca
(
    id_beca              INT IDENTITY(1,1) NOT NULL,
    id_estudiante        INT NOT NULL,
    id_periodo           INT NOT NULL,
    monto                DECIMAL(12,2) NOT NULL,
    fecha_inicio         DATE NOT NULL,
    fecha_fin            DATE NOT NULL,
    estado               NVARCHAR(20) NOT NULL
        CONSTRAINT DF_Beca_Estado DEFAULT N'Pendiente',
    observacion          NVARCHAR(500) NULL,
    motivo_desactivacion NVARCHAR(500) NULL,
    fecha_activacion     DATETIME2(0) NULL,
    fecha_desactivacion  DATETIME2(0) NULL,
    id_usuario_registra  INT NOT NULL,
    fecha_registro       DATETIME2(0) NOT NULL
        CONSTRAINT DF_Beca_FechaRegistro DEFAULT SYSDATETIME(),
    id_usuario_modifica  INT NULL,
    fecha_modificacion   DATETIME2(0) NULL,

    CONSTRAINT PK_Beca PRIMARY KEY (id_beca),
    CONSTRAINT UQ_Beca_Estudiante_Periodo UNIQUE (id_estudiante, id_periodo),
    CONSTRAINT CK_Beca_Monto CHECK (monto > 0),
    CONSTRAINT CK_Beca_Fechas CHECK (fecha_fin >= fecha_inicio),
    CONSTRAINT CK_Beca_Estado
        CHECK (estado IN (N'Pendiente', N'Activa', N'Inactiva')),

    CONSTRAINT FK_Beca_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante),
    CONSTRAINT FK_Beca_Periodo
        FOREIGN KEY (id_periodo) REFERENCES Periodo_Lectivo(id_periodo),
    CONSTRAINT FK_Beca_UsuarioRegistra
        FOREIGN KEY (id_usuario_registra) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_Beca_UsuarioModifica
        FOREIGN KEY (id_usuario_modifica) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_Beca_Periodo_Estado
    ON Beca(id_periodo, estado);
GO

/* ------------------------------------------------------------
   10.3 Requisitos que el administrador marcó como cumplidos
      en cada beca (MBEF-01-03). Los automáticos no se guardan:
      se calculan en el momento.
   ------------------------------------------------------------ */
CREATE TABLE Beca_Requisito_Cumplido
(
    id_beca      INT NOT NULL,
    id_requisito INT NOT NULL,

    CONSTRAINT PK_Beca_Requisito_Cumplido PRIMARY KEY (id_beca, id_requisito),
    CONSTRAINT FK_BecaCumplido_Beca
        FOREIGN KEY (id_beca) REFERENCES Beca(id_beca),
    CONSTRAINT FK_BecaCumplido_Requisito
        FOREIGN KEY (id_requisito) REFERENCES Beca_Requisito(id_requisito)
);
GO

/* ============================================================
   11. RECURSOS ACADÉMICOS
   Material de estudio publicado por curso.
   ============================================================ */

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
   12. COMUNICACIÓN Y REPORTES
   Notificaciones, anuncios, reportes anónimos y módulo informativo.
   ============================================================ */

CREATE TABLE dbo.Notificacion_Tipo
(
    id_tipo INT IDENTITY(1,1) NOT NULL,
    codigo NVARCHAR(60) NOT NULL,
    nombre NVARCHAR(120) NOT NULL,
    modulo NVARCHAR(60) NOT NULL,
    descripcion NVARCHAR(300) NOT NULL,
    activo BIT NOT NULL
        CONSTRAINT DF_NotificacionTipo_Activo DEFAULT (1),
    fecha_creacion DATETIME2 NOT NULL
        CONSTRAINT DF_NotificacionTipo_Fecha DEFAULT (GETDATE()),

    CONSTRAINT PK_Notificacion_Tipo PRIMARY KEY (id_tipo),
    CONSTRAINT UQ_NotificacionTipo_Codigo UNIQUE (codigo)
);
GO

CREATE TABLE dbo.Notificacion_Tipo_Rol
(
    id_tipo INT NOT NULL,
    rol NVARCHAR(60) NOT NULL,

    CONSTRAINT PK_Notificacion_Tipo_Rol
        PRIMARY KEY (id_tipo, rol),

    CONSTRAINT FK_NotificacionTipoRol_Tipo
        FOREIGN KEY (id_tipo)
        REFERENCES dbo.Notificacion_Tipo(id_tipo)
        ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Notificacion
(
    id_notificacion BIGINT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    id_tipo INT NOT NULL,
    titulo NVARCHAR(180) NOT NULL,
    mensaje NVARCHAR(2000) NOT NULL,
    referencia NVARCHAR(250) NULL,
    clave_evento NVARCHAR(180) NOT NULL,
    fecha_creacion DATETIME2 NOT NULL
        CONSTRAINT DF_Notificacion_Fecha DEFAULT (GETDATE()),
    leida BIT NOT NULL
        CONSTRAINT DF_Notificacion_Leida DEFAULT (0),
    fecha_lectura DATETIME2 NULL,

    CONSTRAINT PK_Notificacion PRIMARY KEY (id_notificacion),

    CONSTRAINT FK_Notificacion_Usuario
        FOREIGN KEY (id_usuario)
        REFERENCES dbo.Usuario(id_usuario)
        ON DELETE CASCADE,

    CONSTRAINT FK_Notificacion_Tipo
        FOREIGN KEY (id_tipo)
        REFERENCES dbo.Notificacion_Tipo(id_tipo),

    CONSTRAINT UQ_Notificacion_Usuario_Evento
        UNIQUE (id_usuario, clave_evento)
);
GO

CREATE INDEX IX_Notificacion_Usuario_Leida_Fecha
    ON dbo.Notificacion(id_usuario, leida, fecha_creacion DESC);
GO

/* Tipos de notificaciones disponibles */

INSERT INTO dbo.Notificacion_Tipo
    (codigo, nombre, modulo, descripcion)
VALUES
(N'CALIFICACION_REGISTRADA', N'Calificación registrada',
 N'Calificaciones', N'Se registra una nueva calificación.'),

(N'CALIFICACION_ACTUALIZADA', N'Calificación actualizada',
 N'Calificaciones', N'Se modifica una calificación existente.'),

(N'ASISTENCIA_EXCEPCIONAL', N'Asistencia excepcional',
 N'Asistencia', N'Se registra ausencia, tardía o justificación.'),

(N'HORARIO_ASIGNADO', N'Nuevo horario asignado',
 N'Horarios', N'Se asigna una nueva clase.'),

(N'HORARIO_MODIFICADO', N'Horario modificado',
 N'Horarios', N'Se modifica una clase existente.'),

(N'HORARIO_RETIRADO', N'Horario retirado',
 N'Horarios', N'Se retira o inactiva una clase.'),

(N'ANUNCIO_PUBLICADO', N'Anuncio publicado',
 N'Informativo', N'Se publica un anuncio institucional.'),

(N'ANUNCIO_ACTUALIZADO', N'Anuncio actualizado',
 N'Informativo', N'Se actualiza un anuncio publicado.'),

(N'ACTIVIDAD_PUBLICADA', N'Actividad publicada',
 N'Informativo', N'Se publica una actividad de calendario.'),

(N'ACTIVIDAD_ACTUALIZADA', N'Actividad actualizada',
 N'Informativo', N'Se modifica una actividad publicada.'),

(N'ACTIVIDAD_CANCELADA', N'Actividad cancelada',
 N'Informativo', N'Se cancela una actividad publicada.'),

(N'EVENTO_PUBLICADO', N'Evento publicado',
 N'Informativo', N'Se publica un evento institucional.'),

(N'EVENTO_ACTUALIZADO', N'Evento actualizado',
 N'Informativo', N'Se actualiza un evento institucional.'),

(N'EVENTO_CANCELADO', N'Evento cancelado',
 N'Informativo', N'Se cancela un evento institucional.'),

(N'BECA_APROBADA', N'Beca aprobada',
 N'Becas', N'Se activa o aprueba una beca.'),

(N'BECA_ACTUALIZADA', N'Beca actualizada',
 N'Becas', N'Se actualiza el monto o vigencia de una beca.'),

(N'BECA_CANCELADA', N'Beca cancelada',
 N'Becas', N'Se desactiva o cancela una beca.');
GO

/* Roles destinatarios predeterminados */

INSERT INTO dbo.Notificacion_Tipo_Rol (id_tipo, rol)
SELECT t.id_tipo, r.rol
FROM
(
    VALUES
    (N'CALIFICACION_REGISTRADA', N'Estudiante'),
    (N'CALIFICACION_REGISTRADA', N'Encargado'),
    (N'CALIFICACION_ACTUALIZADA', N'Estudiante'),
    (N'CALIFICACION_ACTUALIZADA', N'Encargado'),

    (N'ASISTENCIA_EXCEPCIONAL', N'Estudiante'),
    (N'ASISTENCIA_EXCEPCIONAL', N'Encargado'),

    (N'HORARIO_ASIGNADO', N'Estudiante'),
    (N'HORARIO_ASIGNADO', N'Docente'),
    (N'HORARIO_MODIFICADO', N'Estudiante'),
    (N'HORARIO_MODIFICADO', N'Docente'),
    (N'HORARIO_RETIRADO', N'Estudiante'),
    (N'HORARIO_RETIRADO', N'Docente'),

    (N'ANUNCIO_PUBLICADO', N'Estudiante'),
    (N'ANUNCIO_PUBLICADO', N'Encargado'),
    (N'ANUNCIO_PUBLICADO', N'Docente'),
    (N'ANUNCIO_ACTUALIZADO', N'Docente'),

    (N'ACTIVIDAD_PUBLICADA', N'Estudiante'),
    (N'ACTIVIDAD_PUBLICADA', N'Encargado'),
    (N'ACTIVIDAD_PUBLICADA', N'Docente'),
    (N'ACTIVIDAD_ACTUALIZADA', N'Estudiante'),
    (N'ACTIVIDAD_ACTUALIZADA', N'Encargado'),
    (N'ACTIVIDAD_ACTUALIZADA', N'Docente'),
    (N'ACTIVIDAD_CANCELADA', N'Estudiante'),
    (N'ACTIVIDAD_CANCELADA', N'Encargado'),
    (N'ACTIVIDAD_CANCELADA', N'Docente'),

    (N'EVENTO_PUBLICADO', N'Estudiante'),
    (N'EVENTO_PUBLICADO', N'Encargado'),
    (N'EVENTO_PUBLICADO', N'Docente'),
    (N'EVENTO_ACTUALIZADO', N'Estudiante'),
    (N'EVENTO_ACTUALIZADO', N'Encargado'),
    (N'EVENTO_ACTUALIZADO', N'Docente'),
    (N'EVENTO_CANCELADO', N'Estudiante'),
    (N'EVENTO_CANCELADO', N'Encargado'),
    (N'EVENTO_CANCELADO', N'Docente'),

    (N'BECA_APROBADA', N'Estudiante'),
    (N'BECA_APROBADA', N'Encargado'),
    (N'BECA_ACTUALIZADA', N'Estudiante'),
    (N'BECA_ACTUALIZADA', N'Encargado'),
    (N'BECA_CANCELADA', N'Estudiante'),
    (N'BECA_CANCELADA', N'Encargado')
) AS r(codigo, rol)
INNER JOIN dbo.Notificacion_Tipo t
    ON t.codigo = r.codigo;
GO

/* Pautas de actuación (plantillas del MEP): información confidencial, solo personal con acceso al módulo.
   Los destinatarios reales los decide el sistema (responsable, Director y Administrador); estudiantes y encargados nunca. */

INSERT INTO dbo.Notificacion_Tipo
    (codigo, nombre, modulo, descripcion)
VALUES
(N'PAUTA_ABIERTA', N'Expediente de pauta abierto',
 N'Pautas', N'Se activa una pauta de actuación y se abre su expediente.'),

(N'PAUTA_CERRADA', N'Expediente de pauta cerrado',
 N'Pautas', N'Se cierra un expediente de pauta de actuación.'),

(N'PAUTA_FORMULARIO_FINALIZADO', N'Formulario de pauta finalizado',
 N'Pautas', N'Se finaliza un formulario del MEP dentro de un expediente.'),

(N'PAUTA_FORMULARIO_REABIERTO', N'Formulario de pauta reabierto',
 N'Pautas', N'Se reabre para edición un formulario finalizado.');
GO

INSERT INTO dbo.Notificacion_Tipo_Rol (id_tipo, rol)
SELECT t.id_tipo, r.rol
FROM dbo.Notificacion_Tipo t
CROSS JOIN (VALUES (N'Administrador'), (N'Director'), (N'Docente'), (N'Personal administrativo')) AS r(rol)
WHERE t.modulo = N'Pautas';
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

IF OBJECT_ID(N'dbo.Informativo_Registro', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Informativo_Registro
    (
        id_informativo INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Informativo_Registro PRIMARY KEY,
        tipo NVARCHAR(20) NOT NULL,
        titulo NVARCHAR(180) NOT NULL,
        descripcion NVARCHAR(4000) NOT NULL,
        fecha_publicacion DATETIME2 NOT NULL,
        fecha_vigencia DATETIME2 NULL,
        fecha_inicio DATETIME2 NOT NULL,
        fecha_fin DATETIME2 NULL,
        hora_inicio TIME NULL,
        hora_fin TIME NULL,
        ubicacion NVARCHAR(250) NULL,
        categoria NVARCHAR(60) NULL,
        estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Informativo_Estado DEFAULT N'Borrador',
        eliminado BIT NOT NULL CONSTRAINT DF_Informativo_Eliminado DEFAULT 0,
        id_usuario_crea INT NULL,
        fecha_creacion DATETIME2 NOT NULL CONSTRAINT DF_Informativo_Fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Informativo_Tipo CHECK (tipo IN (N'Anuncio',N'Actividad',N'Evento')),
        CONSTRAINT CK_Informativo_Estado CHECK (estado IN (N'Borrador',N'Programado',N'Publicado')),
        CONSTRAINT FK_Informativo_Usuario FOREIGN KEY (id_usuario_crea) REFERENCES dbo.Usuario(id_usuario)
    );
    CREATE INDEX IX_Informativo_Tipo_Fecha_Estado ON dbo.Informativo_Registro(tipo, fecha_inicio, estado) INCLUDE (fecha_publicacion, fecha_vigencia, eliminado);
END;
GO

/* ============================================================
   13. INVENTARIO
   Bodega institucional: categorías, artículos y movimientos (MIVF-01).
   ============================================================ */

-- Tipos de artículos de la bodega. Se activan y desactivan, no se borran (MIVF-01-02, 01-03).
CREATE TABLE Categoria_Inv
(
    id_categoria INT IDENTITY(1,1) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    activo BIT NOT NULL
        CONSTRAINT DF_CategoriaInv_Activo DEFAULT 1,

    CONSTRAINT PK_Categoria_Inv PRIMARY KEY (id_categoria),
    CONSTRAINT UQ_Categoria_Inv_Nombre UNIQUE (nombre)
);
GO

INSERT INTO Categoria_Inv (nombre)
VALUES
    (N'Papelería'),
    (N'Suministros de limpieza y materiales de oficina'),
    (N'Equipo tecnológico');
GO

-- Artículos de la bodega (MIVF-01-04, 01-14). Quién registra cada cambio queda en Inventario_Movimiento.
-- Un mismo nombre no se repite dentro de la misma categoría.
CREATE TABLE Inventario
(
    id_inventario INT IDENTITY(1,1) NOT NULL,
    id_categoria INT NOT NULL,
    nombre NVARCHAR(150) NOT NULL,
    descripcion NVARCHAR(300) NULL,
    unidad_medida NVARCHAR(30) NOT NULL,
    cantidad INT NOT NULL,
    cantidad_minima INT NOT NULL
        CONSTRAINT DF_Inventario_CantidadMinima DEFAULT 0,
    fecha_registro DATETIME2(0) NOT NULL
        CONSTRAINT DF_Inventario_FechaRegistro DEFAULT SYSDATETIME(),
    fecha_vencimiento DATE NULL,
    activo BIT NOT NULL
        CONSTRAINT DF_Inventario_Activo DEFAULT 1,

    CONSTRAINT PK_Inventario PRIMARY KEY (id_inventario),
    CONSTRAINT CK_Inventario_Cantidad CHECK (cantidad >= 0),
    CONSTRAINT CK_Inventario_Minima CHECK (cantidad_minima >= 0),
    CONSTRAINT FK_Inventario_Categoria
        FOREIGN KEY (id_categoria) REFERENCES Categoria_Inv(id_categoria)
);
GO

CREATE UNIQUE INDEX UX_Inventario_Categoria_Nombre
    ON Inventario(id_categoria, nombre);

CREATE INDEX IX_Inventario_Activo
    ON Inventario(activo, id_categoria);
GO

IF OBJECT_ID(N'dbo.Inventario_Movimiento', N'U') IS NULL
BEGIN
    CREATE TABLE Inventario_Movimiento
    (
        id_movimiento       INT IDENTITY(1,1) NOT NULL,
        id_inventario       INT NOT NULL,
        tipo                NVARCHAR(10) NOT NULL,
        cantidad            INT NOT NULL,
        existencia_resultante INT NOT NULL,
        fecha               DATETIME2(0) NOT NULL
            CONSTRAINT DF_InvMov_Fecha DEFAULT SYSDATETIME(),
        motivo              NVARCHAR(300) NULL,
        nombre_responsable  NVARCHAR(150) NULL,
        cedula_responsable  NVARCHAR(30) NULL,
        id_usuario_registra INT NOT NULL,

        CONSTRAINT PK_Inventario_Movimiento PRIMARY KEY (id_movimiento),
        CONSTRAINT CK_InvMov_Tipo CHECK (tipo IN (N'Entrada', N'Salida')),
        CONSTRAINT CK_InvMov_Cantidad CHECK (cantidad > 0),
        CONSTRAINT CK_InvMov_Existencia CHECK (existencia_resultante >= 0),
        -- La salida exige responsable y motivo (MIVF-01-10 y 01-11)
        CONSTRAINT CK_InvMov_Salida CHECK (
            tipo = N'Entrada'
            OR (motivo IS NOT NULL AND nombre_responsable IS NOT NULL AND cedula_responsable IS NOT NULL)),

        CONSTRAINT FK_InvMov_Inventario
            FOREIGN KEY (id_inventario) REFERENCES Inventario(id_inventario),
        CONSTRAINT FK_InvMov_Usuario
            FOREIGN KEY (id_usuario_registra) REFERENCES Usuario(id_usuario)
    );

    CREATE INDEX IX_InvMov_Inventario_Fecha ON Inventario_Movimiento(id_inventario, fecha);
    CREATE INDEX IX_InvMov_Fecha ON Inventario_Movimiento(fecha, tipo);
END
GO

/* ============================================================
   14. RECUPERACIÓN DE CONTRASEÑA
   Tokens de restablecimiento de contraseña.
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
   15. AUDITORÍA
   Historial de accesos al sistema.
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
   16. CONFIGURACIÓN DEL SITIO
   Datos institucionales, contenido del sitio y accesos rápidos.
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
    codigo_presupuestario NVARCHAR(30) NULL,
    circuito_escolar NVARCHAR(100) NULL,
    dre NVARCHAR(150) NULL,
    nombre_director NVARCHAR(150) NULL,
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
   17. BOLETERÍA
   Tiquetes del comedor.
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
   18. PAUTAS DE ACTUACIÓN DEL MEP
   Expedientes, formularios, estudiantes involucrados y bitácora de acceso (MPPAF-01). Los campos de cada formulario viven en el código; "datos" guarda los valores en JSON.
   ============================================================ */

CREATE TABLE Pauta_Expediente
(
    id_expediente INT IDENTITY(1,1) NOT NULL,
    codigo NVARCHAR(30) NOT NULL,
    numero_pauta TINYINT NOT NULL,
    id_usuario_responsable INT NOT NULL,
    estado NVARCHAR(15) NOT NULL
        CONSTRAINT DF_PautaExp_Estado DEFAULT N'Abierto',
    fecha_activacion DATE NOT NULL
        CONSTRAINT DF_PautaExp_FechaAct DEFAULT CONVERT(date, SYSDATETIME()),
    id_expediente_origen INT NULL,
    fecha_cierre DATETIME2(0) NULL,
    nombre_cierre NVARCHAR(150) NULL,
    visto_bueno_director NVARCHAR(150) NULL,
    devolucion_realizada BIT NOT NULL
        CONSTRAINT DF_PautaExp_Devol DEFAULT 0,
    observacion_cierre NVARCHAR(500) NULL,
    fecha_creacion DATETIME2(0) NOT NULL
        CONSTRAINT DF_PautaExp_FCrea DEFAULT SYSDATETIME(),
    fecha_actualizacion DATETIME2(0) NOT NULL
        CONSTRAINT DF_PautaExp_FAct DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Pauta_Expediente PRIMARY KEY (id_expediente),
    CONSTRAINT UQ_Pauta_Expediente_Codigo UNIQUE (codigo),
    CONSTRAINT CK_PautaExp_Numero CHECK (numero_pauta BETWEEN 1 AND 9),
    CONSTRAINT CK_PautaExp_Estado CHECK (estado IN (N'Abierto', N'Cerrado')),
    CONSTRAINT FK_PautaExp_Responsable
        FOREIGN KEY (id_usuario_responsable) REFERENCES Usuario(id_usuario),
    CONSTRAINT FK_PautaExp_Origen
        FOREIGN KEY (id_expediente_origen) REFERENCES Pauta_Expediente(id_expediente)
);
GO

CREATE INDEX IX_PautaExp_Responsable_Estado ON Pauta_Expediente(id_usuario_responsable, estado);
CREATE INDEX IX_PautaExp_Pauta_Fecha ON Pauta_Expediente(numero_pauta, fecha_activacion);
GO

CREATE TABLE Pauta_Expediente_Estudiante
(
    id_pauta_estudiante INT IDENTITY(1,1) NOT NULL,
    id_expediente INT NOT NULL,
    id_estudiante INT NULL,
    nombre_completo NVARCHAR(200) NOT NULL,
    seccion NVARCHAR(50) NULL,
    rol NVARCHAR(20) NOT NULL,

    CONSTRAINT PK_Pauta_Expediente_Estudiante PRIMARY KEY (id_pauta_estudiante),
    CONSTRAINT CK_PautaEst_Rol CHECK (rol IN (N'Recibe', N'Ejerce', N'Observador', N'Identificada')),
    CONSTRAINT FK_PautaEst_Expediente
        FOREIGN KEY (id_expediente) REFERENCES Pauta_Expediente(id_expediente),
    CONSTRAINT FK_PautaEst_Estudiante
        FOREIGN KEY (id_estudiante) REFERENCES Estudiante(id_estudiante)
);
GO

CREATE INDEX IX_PautaEst_Expediente ON Pauta_Expediente_Estudiante(id_expediente);
CREATE INDEX IX_PautaEst_Estudiante ON Pauta_Expediente_Estudiante(id_estudiante);
GO

-- Los campos de cada formulario del MEP viven en el código; "datos" guarda los valores en JSON.
CREATE TABLE Pauta_Formulario
(
    id_formulario INT IDENTITY(1,1) NOT NULL,
    id_expediente INT NOT NULL,
    numero_formulario TINYINT NOT NULL,
    secuencia SMALLINT NOT NULL
        CONSTRAINT DF_PautaForm_Sec DEFAULT 1,
    estado NVARCHAR(15) NOT NULL
        CONSTRAINT DF_PautaForm_Estado DEFAULT N'Borrador',
    datos NVARCHAR(MAX) NOT NULL
        CONSTRAINT DF_PautaForm_Datos DEFAULT N'{}',
    id_usuario_actualiza INT NOT NULL,
    fecha_creacion DATETIME2(0) NOT NULL
        CONSTRAINT DF_PautaForm_FCrea DEFAULT SYSDATETIME(),
    fecha_actualizacion DATETIME2(0) NOT NULL
        CONSTRAINT DF_PautaForm_FAct DEFAULT SYSDATETIME(),
    fecha_finalizacion DATETIME2(0) NULL,

    CONSTRAINT PK_Pauta_Formulario PRIMARY KEY (id_formulario),
    CONSTRAINT UQ_Pauta_Formulario UNIQUE (id_expediente, numero_formulario, secuencia),
    CONSTRAINT CK_PautaForm_Numero CHECK (numero_formulario BETWEEN 1 AND 12),
    CONSTRAINT CK_PautaForm_Estado CHECK (estado IN (N'Borrador', N'EnElaboracion', N'Finalizado')),
    CONSTRAINT CK_PautaForm_Json CHECK (ISJSON(datos) = 1),
    CONSTRAINT FK_PautaForm_Expediente
        FOREIGN KEY (id_expediente) REFERENCES Pauta_Expediente(id_expediente),
    CONSTRAINT FK_PautaForm_Usuario
        FOREIGN KEY (id_usuario_actualiza) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_PautaForm_Expediente ON Pauta_Formulario(id_expediente, numero_formulario);
GO

CREATE TABLE Pauta_Bitacora
(
    id_bitacora INT IDENTITY(1,1) NOT NULL,
    id_expediente INT NOT NULL,
    id_formulario INT NULL,
    id_usuario INT NOT NULL,
    accion NVARCHAR(30) NOT NULL,
    detalle NVARCHAR(300) NULL,
    fecha DATETIME2(0) NOT NULL
        CONSTRAINT DF_PautaBit_Fecha DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Pauta_Bitacora PRIMARY KEY (id_bitacora),
    CONSTRAINT FK_PautaBit_Expediente
        FOREIGN KEY (id_expediente) REFERENCES Pauta_Expediente(id_expediente),
    CONSTRAINT FK_PautaBit_Formulario
        FOREIGN KEY (id_formulario) REFERENCES Pauta_Formulario(id_formulario),
    CONSTRAINT FK_PautaBit_Usuario
        FOREIGN KEY (id_usuario) REFERENCES Usuario(id_usuario)
);
GO

CREATE INDEX IX_PautaBit_Expediente_Fecha ON Pauta_Bitacora(id_expediente, fecha);
GO

/* ############################################################
   PARTE 2. DATOS
   ############################################################ */

/* ============================================================
   19. ROLES, ÁREAS, MÓDULOS Y PERMISOS BASE
   Datos mínimos para que el sistema funcione.
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
   20. MÓDULOS POSTERIORES Y SUS PERMISOS
   Horarios, Calificaciones, Asistencia, Becas, Informativo, Inventario y Pautas. Estudiante y Encargado sin fila = sin acceso al módulo.
   ============================================================ */

-- Horarios
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

-- Calificaciones
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

-- Asistencia
/* ------------------------------------------------------------
   Módulo y permisos.
      - Docente: Ver (sus reportes), Crear (pasar lista), Editar (corregir, MCAF-01-05)
      - Administrador y Director: Ver (consulta y reportes) y Editar (cerrar período)
      - Estudiante y Encargado entran por su rol a "Mi asistencia" y solo
        ven lo propio; no necesitan permiso sobre el módulo.
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Asistencia')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Asistencia', N'Registro, consulta, reportes y cierre de la asistencia por clase.');
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

-- Becas
/* ------------------------------------------------------------
   Módulo y permisos.
      - Administrador: Ver, Crear (registrar), Editar (actualizar,
        activar, desactivar y administrar requisitos).
      - Director: Ver (lista, búsqueda e historial).
      - Estudiante y Encargado entran por su rol a "Mi beca" y solo
        ven lo propio; no necesitan permiso sobre el módulo.
   ------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Becas')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Becas', N'Becas socioeconómicas: requisitos, registro, activación, consulta e historial.');
GO

INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, p.ver, p.crear, p.editar, p.eliminar
FROM
(
    VALUES
        (N'Administrador', 1, 1, 1, 0),
        (N'Director',      1, 0, 0, 0)
) AS p (rol, ver, crear, editar, eliminar)
JOIN Rol r ON r.nombre = p.rol
JOIN Modulo m ON m.nombre = N'Becas';
GO

-- Informativo
/* Registra el módulo y sus permisos. Los nombres de roles deben existir previamente. */
IF NOT EXISTS (SELECT 1 FROM dbo.Modulo WHERE nombre = N'Informativo')
    INSERT INTO dbo.Modulo(nombre, descripcion) VALUES (N'Informativo', N'Anuncios institucionales, calendario académico y eventos');
GO

DECLARE @ModuloId INT = (SELECT id_modulo FROM dbo.Modulo WHERE nombre = N'Informativo');
IF @ModuloId IS NOT NULL
BEGIN
    INSERT INTO dbo.Rol_Permiso(id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
    SELECT r.id_rol, @ModuloId,
           1,
           CASE WHEN r.nombre IN (N'Administrador',N'Director') THEN 1 ELSE 0 END,
           CASE WHEN r.nombre IN (N'Administrador',N'Director') THEN 1 ELSE 0 END,
           CASE WHEN r.nombre IN (N'Administrador',N'Director') THEN 1 ELSE 0 END
    FROM dbo.Rol r
    WHERE r.nombre IN (N'Administrador',N'Director',N'Docente',N'Estudiante',N'Encargado')
      AND NOT EXISTS (SELECT 1 FROM dbo.Rol_Permiso rp WHERE rp.id_rol = r.id_rol AND rp.id_modulo = @ModuloId);
END;
GO

-- Inventario
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Inventario')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Inventario', N'Tipos, artículos, entradas, salidas, existencias y reportes de la bodega institucional.');
GO

INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, p.ver, p.crear, p.editar, p.eliminar
FROM (VALUES
        (N'Administrador',           1, 1, 1, 0),
        (N'Personal administrativo', 1, 1, 1, 0),
        (N'Director',                1, 0, 0, 0)
     ) AS p(rol, ver, crear, editar, eliminar)
JOIN Rol r ON r.nombre = p.rol
JOIN Modulo m ON m.nombre = N'Inventario'
WHERE NOT EXISTS (SELECT 1 FROM Rol_Permiso rp
                  WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO

-- Pautas de actuación
IF NOT EXISTS (SELECT 1 FROM Modulo WHERE nombre = N'Pautas')
    INSERT INTO Modulo (nombre, descripcion)
    VALUES (N'Pautas', N'Pautas de actuación del MEP: expedientes, formularios, documentos imprimibles y cierre.');
GO

-- Estudiante y Encargado no reciben permisos: la información es confidencial.
INSERT INTO Rol_Permiso (id_rol, id_modulo, puede_ver, puede_crear, puede_editar, puede_eliminar)
SELECT r.id_rol, m.id_modulo, p.ver, p.crear, p.editar, p.eliminar
FROM (VALUES
        (N'Administrador',           1, 1, 1, 1),
        (N'Director',                1, 1, 1, 0),
        (N'Docente',                 1, 1, 1, 0),
        (N'Personal administrativo', 1, 1, 1, 0)
     ) AS p(rol, ver, crear, editar, eliminar)
JOIN Rol r ON r.nombre = p.rol
JOIN Modulo m ON m.nombre = N'Pautas'
WHERE NOT EXISTS (SELECT 1 FROM Rol_Permiso rp
                  WHERE rp.id_rol = r.id_rol AND rp.id_modulo = m.id_modulo);
GO

/* ============================================================
   21. DATOS DE PRUEBA
   Usuarios, estudiantes y un curso de prueba. Ejecutar al final; NO usar en producción.
   ============================================================ */
/* ============================================================
   Usuarios de prueba:
     admin@lnsd.local              /  Admin123!         (rol Administrador)
     estudiante.prueba@lnsd.local  /  Estudiante123*    (rol Estudiante)
     estudiante2.prueba@lnsd.local /  Estudiante123*
     estudiante3.prueba@lnsd.local /  Estudiante123*
     encargado.prueba@lnsd.local   /  Encargado123*     (rol Encargado) de estudiante.prueba
     FMendez@lnsd.com              /  aaaaaaaa
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

/* ============================================================
   DATOS DE PRUEBA DE CALIFICACIONES (MCNF-01)
   Van al final porque usan Horario, Curso, Matrícula, Docente y demás
   tablas ya creadas.
   ============================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;
 
-- Si algo falla, se deshace todo: nunca queda una transacción abierta
-- bloqueando Grado, Curso, Horario, etc. (la aplicación se quedaría esperando).
BEGIN TRY
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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

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

-- Red de seguridad: si la ejecución se interrumpió a medias, cerrar la transacción pendiente
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
GO
