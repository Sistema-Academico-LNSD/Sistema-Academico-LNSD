namespace ProyectoLNSD_WApp.DAL.Entities;
public class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

    public bool Estado { get; set; }

    public virtual Rol? Rol { get; set; }

    public virtual PersonalAdmin? PersonalAdmin { get; set; }

    public virtual Docente? Docente { get; set; }

    public virtual Estudiante? Estudiante { get; set; }

    public virtual Encargado? Encargado { get; set; }
}