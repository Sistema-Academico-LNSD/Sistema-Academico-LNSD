using Microsoft.EntityFrameworkCore;
using ProyectoLNSD_WApp.DAL.Entities;

namespace ProyectoLNSD_WApp.DAL.Data
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        //Tables
        public virtual DbSet<Rol> Roles { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<PersonalAdmin> PersonalAdmins { get; set; }
        public virtual DbSet<Docente> Docentes { get; set; }
        public virtual DbSet<Estudiante> Estudiantes { get; set; }
        public virtual DbSet<Encargado> Encargados { get; set; }
        public virtual DbSet<EstudianteDocente> EstudiantesDocentes { get; set; }
        public virtual DbSet<EncargadoEstudiante> EncargadosEstudiantes { get; set; }
        public virtual DbSet<UsuarioTokenReset> UsuariosTokensReset { get; set; }
        public virtual DbSet<Modulo> Modulos { get; set; }
        public virtual DbSet<RolPermiso> RolesPermisos { get; set; }
        public virtual DbSet<LogAcceso> LogsAcceso { get; set; }



        //Tables override mode builder
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol");

                entity.HasKey(e => e.IdRol)
                      .HasName("PK_Rol");

                entity.Property(e => e.IdRol)
                      .HasColumnName("id_rol");

                entity.Property(e => e.Nombre)
                      .HasColumnName("nombre")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.Descripcion)
                      .HasColumnName("descripcion")
                      .HasMaxLength(250);

                entity.HasIndex(e => e.Nombre)
                      .IsUnique()
                      .HasDatabaseName("UQ_Rol_Nombre");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");

                entity.HasKey(e => e.IdUsuario)
                      .HasName("PK_Usuario");

                entity.Property(e => e.IdUsuario)
                      .HasColumnName("id_usuario");

                entity.Property(e => e.IdRol)
                      .HasColumnName("id_rol");

                entity.Property(e => e.Nombre)
                      .HasColumnName("nombre")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Apellido)
                      .HasColumnName("apellido")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Correo)
                      .HasColumnName("correo")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.PasswordHash)
                      .HasColumnName("password_hash")
                      .HasColumnType("varbinary(256)")
                      .IsRequired();

                entity.Property(e => e.Estado)
                      .HasColumnName("estado")
                      .HasDefaultValue(true);

                entity.HasIndex(e => e.Correo)
                      .IsUnique()
                      .HasDatabaseName("UQ_Usuario_Correo");

                entity.HasIndex(e => e.IdRol)
                      .HasDatabaseName("IX_Usuario_Rol");

                entity.HasOne(d => d.Rol)
                      .WithMany(p => p.Usuarios)
                      .HasForeignKey(d => d.IdRol)
                      .HasConstraintName("FK_Usuario_Rol");
            });

            modelBuilder.Entity<PersonalAdmin>(entity =>
            {
                entity.ToTable("Personal_Admin");

                entity.HasKey(e => e.IdPerAdm);

                entity.Property(e => e.IdPerAdm)
                      .HasColumnName("id_perAdm");

                entity.Property(e => e.IdUsuario)
                      .HasColumnName("id_usuario");

                entity.Property(e => e.Cargo)
                      .HasColumnName("cargo")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Departamento)
                      .HasColumnName("departamento")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.HasIndex(e => e.IdUsuario)
                      .IsUnique()
                      .HasDatabaseName("UQ_Personal_Admin_Usuario");

                entity.HasOne(d => d.Usuario)
                      .WithOne(p => p.PersonalAdmin)
                      .HasForeignKey<PersonalAdmin>(d => d.IdUsuario)
                      .HasConstraintName("FK_Personal_Admin_Usuario");
            });

            modelBuilder.Entity<Docente>(entity =>
            {
                entity.ToTable("Docente");

                entity.HasKey(e => e.IdDocente);

                entity.Property(e => e.IdDocente)
                      .HasColumnName("id_docente");

                entity.Property(e => e.IdUsuario)
                      .HasColumnName("id_usuario");

                entity.Property(e => e.Especialidad)
                      .HasColumnName("especialidad")
                      .HasMaxLength(150);

                entity.HasOne(d => d.Usuario)
                      .WithOne(p => p.Docente)
                      .HasForeignKey<Docente>(d => d.IdUsuario)
                      .HasConstraintName("FK_Docente_Usuario");

                entity.HasIndex(e => e.IdUsuario)
                      .IsUnique()
                      .HasDatabaseName("UQ_Docente_Usuario");

            });

            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.ToTable("Estudiante");

                entity.HasKey(e => e.IdEstudiante);

                entity.Property(e => e.IdEstudiante)
                      .HasColumnName("id_estudiante");

                entity.Property(e => e.IdUsuario)
                      .HasColumnName("id_usuario");

                entity.Property(e => e.Identificacion)
                      .HasColumnName("identificacion")
                      .HasMaxLength(30)
                      .IsRequired();

                entity.HasOne(d => d.Usuario)
                      .WithOne(p => p.Estudiante)
                      .HasForeignKey<Estudiante>(d => d.IdUsuario)
                      .HasConstraintName("FK_Estudiante_Usuario");

                entity.HasIndex(e => e.IdUsuario)
                       .IsUnique()
                       .HasDatabaseName("UQ_Estudiante_Usuario");

                entity.HasIndex(e => e.Identificacion)
                      .IsUnique()
                      .HasDatabaseName("UQ_Estudiante_Identificacion");


            });

            modelBuilder.Entity<Encargado>(entity =>
            {
                entity.ToTable("Encargado");

                entity.HasKey(e => e.IdEncargado);

                entity.Property(e => e.IdEncargado)
                      .HasColumnName("id_encargado");

                entity.Property(e => e.IdUsuario)
                      .HasColumnName("id_usuario");

                entity.HasOne(d => d.Usuario)
                      .WithOne(p => p.Encargado)
                      .HasForeignKey<Encargado>(d => d.IdUsuario)
                      .HasConstraintName("FK_Encargado_Usuario");

                entity.HasIndex(e => e.IdUsuario)
                      .IsUnique()
                      .HasDatabaseName("UQ_Encargado_Usuario");
            });

            modelBuilder.Entity<EstudianteDocente>(entity =>
            {
                entity.ToTable("Estudiante_Docente");

                entity.HasKey(e => new
                {
                    e.IdEstudiante,
                    e.IdDocente
                })
                .HasName("PK_Estudiante_Docente");

                entity.Property(e => e.IdEstudiante)
                      .HasColumnName("id_estudiante");

                entity.Property(e => e.IdDocente)
                      .HasColumnName("id_docente");

                entity.HasOne(d => d.Estudiante)
                      .WithMany(p => p.EstudiantesDocentes)
                      .HasForeignKey(d => d.IdEstudiante)
                      .HasConstraintName("FK_Estudiante_Docente_Estudiante");

                entity.HasOne(d => d.Docente)
                      .WithMany(p => p.EstudiantesDocentes)
                      .HasForeignKey(d => d.IdDocente)
                      .HasConstraintName("FK_Estudiante_Docente_Docente");
            });

            modelBuilder.Entity<EncargadoEstudiante>(entity =>
            {
                entity.ToTable("Encargado_Estudiante");

                entity.HasKey(e => new
                {
                    e.IdEncargado,
                    e.IdEstudiante
                })
                .HasName("PK_Encargado_Estudiante");

                entity.Property(e => e.IdEncargado)
                      .HasColumnName("id_encargado");

                entity.Property(e => e.IdEstudiante)
                      .HasColumnName("id_estudiante");

                entity.Property(e => e.Parentesco)
                      .HasColumnName("parentesco")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasOne(d => d.Encargado)
                      .WithMany(p => p.EncargadosEstudiantes)
                      .HasForeignKey(d => d.IdEncargado);

                entity.HasOne(d => d.Estudiante)
                      .WithMany(p => p.EncargadosEstudiantes)
                      .HasForeignKey(d => d.IdEstudiante);
            });

            modelBuilder.Entity<UsuarioTokenReset>(entity =>
            {
                entity.ToTable("Usuario_Token_Reset");

                entity.HasKey(e => e.IdToken)
                      .HasName("PK_Usuario_Token_Reset");

                entity.Property(e => e.IdToken)
                      .HasColumnName("id_token");

                entity.Property(e => e.IdUsuario)
                      .HasColumnName("id_usuario");

                entity.Property(e => e.TokenHash)
                      .HasColumnName("token_hash")
                      .HasColumnType("varbinary(64)")
                      .IsRequired();

                entity.Property(e => e.FechaCreacion)
                      .HasColumnName("fecha_creacion")
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.FechaExpiracion)
                      .HasColumnName("fecha_expiracion")
                      .IsRequired();

                entity.Property(e => e.Usado)
                      .HasColumnName("usado")
                      .HasDefaultValue(false);

                entity.HasIndex(e => e.TokenHash)
                      .HasDatabaseName("IX_TokenReset_TokenHash");

                entity.HasIndex(e => e.IdUsuario)
                      .HasDatabaseName("IX_TokenReset_Usuario");

                entity.HasOne(d => d.Usuario)
                      .WithMany()
                      .HasForeignKey(d => d.IdUsuario)
                      .HasConstraintName("FK_TokenReset_Usuario");
            });

            modelBuilder.Entity<Modulo>(entity =>
            {
                entity.ToTable("Modulo");

                entity.HasKey(e => e.IdModulo)
                      .HasName("PK_Modulo");

                entity.Property(e => e.IdModulo)
                      .HasColumnName("id_modulo");

                entity.Property(e => e.Nombre)
                      .HasColumnName("nombre")
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Descripcion)
                      .HasColumnName("descripcion")
                      .HasMaxLength(250);

                entity.HasIndex(e => e.Nombre)
                      .IsUnique()
                      .HasDatabaseName("UQ_Modulo_Nombre");
            });

            modelBuilder.Entity<RolPermiso>(entity =>
            {
                entity.ToTable("Rol_Permiso");

                entity.HasKey(e => e.IdRolPermiso)
                      .HasName("PK_Rol_Permiso");

                entity.Property(e => e.IdRolPermiso)
                      .HasColumnName("id_rol_permiso");

                entity.Property(e => e.IdRol)
                      .HasColumnName("id_rol");

                entity.Property(e => e.IdModulo)
                      .HasColumnName("id_modulo");

                entity.Property(e => e.PuedeVer)
                      .HasColumnName("puede_ver")
                      .HasDefaultValue(false);

                entity.Property(e => e.PuedeCrear)
                      .HasColumnName("puede_crear")
                      .HasDefaultValue(false);

                entity.Property(e => e.PuedeEditar)
                      .HasColumnName("puede_editar")
                      .HasDefaultValue(false);

                entity.Property(e => e.PuedeEliminar)
                      .HasColumnName("puede_eliminar")
                      .HasDefaultValue(false);

                entity.HasIndex(e => new { e.IdRol, e.IdModulo })
                      .IsUnique()
                      .HasDatabaseName("UQ_Rol_Permiso");

                entity.HasIndex(e => e.IdRol)
                      .HasDatabaseName("IX_RolPermiso_Rol");

                entity.HasOne(d => d.Rol)
                      .WithMany()
                      .HasForeignKey(d => d.IdRol)
                      .HasConstraintName("FK_RolPermiso_Rol");

                entity.HasOne(d => d.Modulo)
                      .WithMany(m => m.RolesPermisos)
                      .HasForeignKey(d => d.IdModulo)
                      .HasConstraintName("FK_RolPermiso_Modulo");
            });

            modelBuilder.Entity<LogAcceso>(entity =>
            {
                entity.ToTable("Log_Acceso");

                entity.HasKey(e => e.IdLog)
                      .HasName("PK_Log_Acceso");

                entity.Property(e => e.IdLog)
                      .HasColumnName("id_log");

                entity.Property(e => e.IdUsuario)
                      .HasColumnName("id_usuario");

                entity.Property(e => e.Correo)
                      .HasColumnName("correo")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.TipoEvento)
                      .HasColumnName("tipo_evento")
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(e => e.Exitoso)
                      .HasColumnName("exitoso");

                entity.Property(e => e.Mensaje)
                      .HasColumnName("mensaje")
                      .HasMaxLength(250);

                entity.Property(e => e.Fecha)
                      .HasColumnName("fecha")
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.IdUsuario)
                      .HasDatabaseName("IX_LogAcceso_Usuario");

                entity.HasIndex(e => e.Fecha)
                      .HasDatabaseName("IX_LogAcceso_Fecha");

                entity.HasOne(d => d.Usuario)
                      .WithMany()
                      .HasForeignKey(d => d.IdUsuario)
                      .HasConstraintName("FK_LogAcceso_Usuario");
            });


            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}