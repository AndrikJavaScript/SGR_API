using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGR_API.Data;
using SGR_API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGR_API.Controllers
{
    [Route("api/contenidos")]
    [ApiController]
    public class ContenidoController : ControllerBase
    {
        private readonly SGRContext _context;

        public ContenidoController(SGRContext context)
        {
            _context = context;
        }

        // GET: api/contenidos - (Opcional) Devuelve todos los contenidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contenido>>> GetContenidos()
        {
            return await _context.Contenidos.ToListAsync();
        }

        // GET: api/contenidos/referencia/{referenciaId} - Devuelve los contenidos asociados a una referencia
        [HttpGet("referencia/{referenciaId}")]
        public async Task<ActionResult<IEnumerable<Contenido>>> GetContenidosByReferencia(int referenciaId)
        {
            var contenidos = await _context.Contenidos
                .Where(c => c.ReferenciaId == referenciaId)
                .ToListAsync();

            if (contenidos == null || contenidos.Count == 0)
                return NotFound();

            return contenidos;
        }

        // POST: api/contenidos - Crea un nuevo contenido (párrafo)
        [HttpPost]
        public async Task<ActionResult<Contenido>> CreateContenido([FromBody] Contenido contenido)
        {
            // Aquí se puede agregar validación adicional (por ejemplo, máximo 10 párrafos, etc.)
            _context.Contenidos.Add(contenido);
            await _context.SaveChangesAsync();

            // Retornamos CreatedAtAction para que el cliente sepa cuál es el objeto creado
            return CreatedAtAction(nameof(GetContenidosByReferencia), new { referenciaId = contenido.ReferenciaId }, contenido);
        }

        // PUT: api/contenidos/{id} - Actualiza un contenido existente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContenido(int id, [FromBody] Contenido contenido)
        {
            if (id != contenido.Id)
                return BadRequest();

            _context.Entry(contenido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ContenidoExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/contenidos/{id} - Elimina un contenido
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContenido(int id)
        {
            var contenido = await _context.Contenidos.FindAsync(id);
            if (contenido == null)
                return NotFound();

            _context.Contenidos.Remove(contenido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/contenidos/porReferencia - Actualiza en masa los contenidos asociados a una referencia
        [HttpPut("porReferencia")]
        public async Task<IActionResult> ActualizarContenidosPorReferencia([FromBody] List<Contenido> contenidos)
        {
            if (contenidos == null || contenidos.Count == 0)
                return BadRequest(new { mensaje = "No se proporcionaron contenidos para actualizar." });

            // Itera sobre cada contenido recibido
            foreach (var contenido in contenidos)
            {
                // Si el contenido tiene un ID válido (> 0), se intenta actualizarlo
                if (contenido.Id > 0)
                {
                    _context.Entry(contenido).State = EntityState.Modified;
                }
                else
                {
                    // Si no tiene ID, se asume que es un contenido nuevo, por lo que se agrega a la base de datos
                    _context.Contenidos.Add(contenido);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Esto ocurrirá si, pese al chequeo, algún contenido cuyo ID era > 0 no se encuentra
                return NotFound(new { mensaje = "Uno o más contenidos no se encontraron." });
            }

            return Ok(new { mensaje = "Contenidos actualizados correctamente." });
        }
        // Método privado que se utiliza para verificar la existencia de un contenido
        private async Task<bool> ContenidoExists(int id)
        {
            return await _context.Contenidos.AnyAsync(e => e.Id == id);
        }
    }
}