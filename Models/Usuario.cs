namespace SGR_API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public String NombreUsuario { get; set; }
        public String PasswordHash { get; set; } 
        public String? Email { get; set; }
        public String? Rol { get; set; }
        public bool Estado { get; set; }

    }

    
}
