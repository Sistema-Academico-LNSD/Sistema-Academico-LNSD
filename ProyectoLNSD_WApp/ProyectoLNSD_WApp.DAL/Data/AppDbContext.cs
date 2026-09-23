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


            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}