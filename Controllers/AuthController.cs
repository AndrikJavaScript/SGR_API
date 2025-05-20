namespace SGR_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using SGR_API.Data;
    using SGR_API.Models;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SGRContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(SGRContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Login
        [HttpPost("login")]
        public IActionResult Login([FromBody] Usuario loginUser)
        {
            if (loginUser == null || string.IsNullOrEmpty(loginUser.NombreUsuario) || string.IsNullOrEmpty(loginUser.PasswordHash))
            {
                return BadRequest(new { message = "Debe proporcionar nombre de usuario y contraseña." });
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == loginUser.NombreUsuario);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Verificar la contraseña hasheada
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginUser.PasswordHash, usuario.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Contraseña incorrecta." });
            }

            // Crear el token JWT
            var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),  // 👈 Este claim ya tiene el Id, pero mejor lo agregamos explícitamente
    new Claim("userId", usuario.Id.ToString()),  // ✅ Agregar el usuarioId al token
    new Claim("userName", usuario.NombreUsuario),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim(ClaimTypes.Role, usuario.Rol)
};


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        // Registro
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Usuario newUser)
        {
            if (newUser == null)
            {
                return BadRequest(new { message = "Datos de usuario no proporcionados." });
            }
            if (string.IsNullOrEmpty(newUser.NombreUsuario) || string.IsNullOrEmpty(newUser.PasswordHash) || string.IsNullOrEmpty(newUser.Email))
            {
                return BadRequest(new { message = "Nombre de usuario, contraseña y email son obligatorios." });
            }
            if (!IsValidEmail(newUser.Email))
            {
                return BadRequest(new { message = "El formato del email es inválido." });
            }

            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == newUser.NombreUsuario || u.Email == newUser.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "El nombre de usuario o el email ya están en uso." });
            }

            // Hashear la contraseña
            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash);

            // Asignar valores predeterminados
            newUser.Estado = true;
            newUser.Rol = "User";

            _context.Usuarios.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado exitosamente." });
        }

        // Validación del formato del email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Endpoint protegido
        [Authorize]
        [HttpGet("datos-protegidos")]
        public IActionResult GetDatosProtegidos()
        {
            var currentUser = HttpContext.User;
            var userName = currentUser.Claims.FirstOrDefault(c => c.Type == "userName")?.Value;

            return Ok(new
            {
                message = "Datos accesibles solo con un token válido.",
                user = userName
            });
        }

        // Obtener perfil del usuario autenticado
        [Authorize]
        [HttpGet("perfil")]
        public IActionResult GetUserProfile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }

            int userId = int.Parse(userIdClaim.Value);

            return Ok(new { userId, message = "Perfil del usuario obtenido correctamente." });
        }



        // Verificar existencia de un usuario
        [HttpGet("verificar-usuario/{nombreUsuario}")]
        public IActionResult VerificarUsuario(string nombreUsuario)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            return Ok(new { message = "Usuario encontrado." });
        }

        // Restablecimiento de contraseña
        [HttpPost("restablecer-contrasena")]
        public async Task<IActionResult> RestablecerContrasena([FromBody] RestablecerContrasenaRequest request)
        {
            if (string.IsNullOrEmpty(request.NombreUsuario) || string.IsNullOrEmpty(request.NuevaContraseña))
            {
                return BadRequest(new { message = "Todos los campos son requeridos." });
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Cifrar la nueva contraseña
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaContraseña);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña restablecida exitosamente." });
        }
    }

    // Clase para el restablecimiento de contraseña
    public class RestablecerContrasenaRequest
    {
        public string NombreUsuario { get; set; }
        public string NuevaContraseña { get; set; }
    }
}
