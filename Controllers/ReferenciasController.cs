using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGR_API.Data;
using SGR_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGR_API.Controllers
{
    [Route("api/referencias")]
    [ApiController]
    public class ReferenciasController : ControllerBase
    {
        private readonly SGRContext _context;

        public ReferenciasController(SGRContext context)
        {
            _context = context;
        }

        // GET: api/referencias - Obtiene todas las referencias unificadas (con contenidos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Referencia>>> GetReferencias()
        {
            var referencias = await _context.Referencias
                .Include(r => r.Contenidos)
                .ToListAsync();
            return Ok(referencias);
        }

        // GET: api/referencias/usuario/{usuarioId} - Obtiene las referencias de un usuario con datos formateados
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> ObtenerReferenciasPorUsuario(int usuarioId)
        {
            var referencias = await _context.Referencias
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();

            // Obtención y concatenación de autores. Se asume que en Autores están los campos Autor1...Autor20
            var autoresIds = referencias.Select(r => r.AutoresId).Distinct().ToList();
            var autoresDict = await _context.Autores
                .Where(a => autoresIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            string ObtenerAutoresConcatenados(int autoresId)
            {
                if (!autoresDict.ContainsKey(autoresId))
                    return string.Empty;

                var autor = autoresDict[autoresId];
                var listaCampos = new List<string>
                {
                    autor.Autor1, autor.Autor2, autor.Autor3, autor.Autor4, autor.Autor5,
                    autor.Autor6, autor.Autor7, autor.Autor8, autor.Autor9, autor.Autor10,
                    autor.Autor11, autor.Autor12, autor.Autor13, autor.Autor14, autor.Autor15,
                    autor.Autor16, autor.Autor17, autor.Autor18, autor.Autor19, autor.Autor20
                };

                var autoresIndividuales = listaCampos
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Replace(",", "").Trim())
                    .ToList();
                return string.Join(", ", autoresIndividuales);
            }

            var referenciasProyectadas = referencias.Select(r => new
            {
                r.Id,
                autoresId = r.AutoresId,
                Autores = ObtenerAutoresConcatenados(r.AutoresId),
                r.Titulo,
                r.Anio,
                // En formato APA no se muestra el Lugar
                Lugar = r.Formato.Equals("APA", StringComparison.OrdinalIgnoreCase) ? null : r.Lugar,
                r.Fuente,
                r.Formato
            }).OrderByDescending(r => r.Anio);
            return Ok(referenciasProyectadas);
        }

        // GET: api/referencias/{id} - Obtiene una referencia específica (incluyendo sus contenidos)
        [HttpGet("{id}")]
        public async Task<ActionResult<Referencia>> GetReferencia(int id)
        {
            var referencia = await _context.Referencias
                .Include(r => r.Contenidos)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (referencia == null)
            {
                return NotFound(new { mensaje = "Referencia no encontrada." });
            }
            return Ok(referencia);
        }


        // POST: api/referencias - Crea una nueva referencia (sin contenidos enviados en esta llamada)
        [HttpPost]
        public async Task<IActionResult> CrearReferencia([FromBody] Referencia referencia)
        {
            if (referencia == null || string.IsNullOrWhiteSpace(referencia.Titulo) || referencia.AutoresId <= 0)
            {
                return BadRequest(new { mensaje = "Debe incluir un título y un ID de autores válido." });
            }

            // Verificar existencia del usuario
            var usuario = await _context.Usuarios.FindAsync(referencia.UsuarioId);
            if (usuario == null)
                return NotFound(new { mensaje = "El usuario no existe." });

            // Verificar existencia de los autores
            var autores = await _context.Autores.FindAsync(referencia.AutoresId);
            if (autores == null)
                return NotFound(new { mensaje = "Los autores no existen." });

            try
            {
                // Para formato APA, se ignora el campo Lugar
                if (referencia.Formato.Equals("APA", StringComparison.OrdinalIgnoreCase))
                {
                    referencia.Lugar = null;
                }

                // Asegurarse de inicializar la colección si viene null
                if (referencia.Contenidos == null)
                {
                    referencia.Contenidos = new List<Contenido>();
                }

                _context.Referencias.Add(referencia);
                await _context.SaveChangesAsync();

                // Mensaje en la terminal indicando que se guardó la referencia
                Console.WriteLine($"Referencia guardada con éxito. ID: {referencia.Id}");
                return Ok(new { mensaje = "Referencia guardada con éxito.", id = referencia.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al guardar la referencia.", error = ex.Message });
            }
        }

        // PUT: api/referencias/{id} - Actualiza una referencia existente (no modifica UsuarioId ni AutoresId ni contenidos)
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarReferencia(int id, [FromBody] Referencia referencia)
        {
            if (referencia == null)
                return BadRequest(new { mensaje = "Datos inválidos." });

            try
            {
                var referenciaExistente = await _context.Referencias.FindAsync(id);
                if (referenciaExistente == null)
                    return NotFound(new { mensaje = "Referencia no encontrada." });

                // Actualizar solo los campos básicos, dejando intactas las llaves foráneas
                referenciaExistente.Titulo = referencia.Titulo;
                referenciaExistente.Anio = referencia.Anio;
                referenciaExistente.Fuente = referencia.Fuente;
                referenciaExistente.Formato = referencia.Formato;
                // Solo se actualiza Lugar en caso de formato Chicago, de lo contrario se limpia
                referenciaExistente.Lugar = referencia.Formato.Equals("Chicago", StringComparison.OrdinalIgnoreCase)
                                             ? referencia.Lugar
                                             : null;

                _context.Referencias.Update(referenciaExistente);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Referencia actualizada. ID: {referenciaExistente.Id}");
                return Ok(new { mensaje = "Referencia actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al actualizar la referencia.", error = ex.Message });
            }
        }


        // DELETE: api/referencias/{id} - Elimina una referencia (y opcionalmente el registro de autores)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReferencia(int id)
        {
            var referencia = await _context.Referencias.FindAsync(id);
            if (referencia == null)
                return NotFound(new { mensaje = "Referencia no encontrada." });

            var registroAutores = await _context.Autores.FindAsync(referencia.AutoresId);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Referencias.Remove(referencia);
                    await _context.SaveChangesAsync();

                    // (Opcional) Eliminar el registro de autores asociado
                    if (registroAutores != null)
                    {
                        _context.Autores.Remove(registroAutores);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    Console.WriteLine($"Referencia eliminada. ID: {id}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { mensaje = "Error al eliminar la referencia.", error = ex.Message });
                }
            }

            return Ok(new { mensaje = "Referencia eliminada correctamente." });
        }

        // PUT: api/referencias/cambiar-formato/{id} - Cambia el formato de la referencia (APA <--> Chicago)
        [HttpPut("cambiar-formato/{id}")]
        public async Task<IActionResult> CambiarFormatoReferencia(int id, [FromBody] Referencia referencia)
        {
            if (referencia == null || string.IsNullOrWhiteSpace(referencia.Formato))
                return BadRequest(new { mensaje = "Formato inválido." });

            try
            {
                var refExistente = await _context.Referencias.FindAsync(id);
                if (refExistente == null)
                    return NotFound(new { mensaje = "Referencia no encontrada." });

                if (referencia.Formato.Equals("APA", StringComparison.OrdinalIgnoreCase))
                {
                    refExistente.Formato = "APA";
                    refExistente.Lugar = null;
                }
                else if (referencia.Formato.Equals("Chicago", StringComparison.OrdinalIgnoreCase))
                {
                    refExistente.Formato = "Chicago";
                    refExistente.Lugar = referencia.Lugar;
                }
                else
                {
                    return BadRequest(new { mensaje = "Formato no soportado." });
                }

                _context.Referencias.Update(refExistente);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Formato de referencia (ID: {id}) cambiado a {refExistente.Formato}");
                return Ok(new { mensaje = "Formato cambiado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al cambiar el formato.", error = ex.Message });
            }
        }
    }
}
