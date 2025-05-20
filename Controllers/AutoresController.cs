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
    [Route("api/autores")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly SGRContext _context;

        public AutoresController(SGRContext context)
        {
            _context = context;
        }

        // Crear un nuevo registro de autores
        [HttpPost]
        public async Task<IActionResult> CrearAutor([FromBody] Autores nuevoAutor)
        {
            if (nuevoAutor == null)
            {
                return BadRequest(new { mensaje = "Datos inválidos." });
            }

            try
            {
                // Procesamos cada uno de los campos de autor usando el nuevo formato.
                nuevoAutor.Autor1 = FormatearAutor(nuevoAutor.Autor1);
                nuevoAutor.Autor2 = FormatearAutor(nuevoAutor.Autor2);
                nuevoAutor.Autor3 = FormatearAutor(nuevoAutor.Autor3);
                nuevoAutor.Autor4 = FormatearAutor(nuevoAutor.Autor4);
                nuevoAutor.Autor5 = FormatearAutor(nuevoAutor.Autor5);
                nuevoAutor.Autor6 = FormatearAutor(nuevoAutor.Autor6);
                nuevoAutor.Autor7 = FormatearAutor(nuevoAutor.Autor7);
                nuevoAutor.Autor8 = FormatearAutor(nuevoAutor.Autor8);
                nuevoAutor.Autor9 = FormatearAutor(nuevoAutor.Autor9);
                nuevoAutor.Autor10 = FormatearAutor(nuevoAutor.Autor10);
                nuevoAutor.Autor11 = FormatearAutor(nuevoAutor.Autor11);
                nuevoAutor.Autor12 = FormatearAutor(nuevoAutor.Autor12);
                nuevoAutor.Autor13 = FormatearAutor(nuevoAutor.Autor13);
                nuevoAutor.Autor14 = FormatearAutor(nuevoAutor.Autor14);
                nuevoAutor.Autor15 = FormatearAutor(nuevoAutor.Autor15);
                nuevoAutor.Autor16 = FormatearAutor(nuevoAutor.Autor16);
                nuevoAutor.Autor17 = FormatearAutor(nuevoAutor.Autor17);
                nuevoAutor.Autor18 = FormatearAutor(nuevoAutor.Autor18);
                nuevoAutor.Autor19 = FormatearAutor(nuevoAutor.Autor19);
                nuevoAutor.Autor20 = FormatearAutor(nuevoAutor.Autor20);

                _context.Autores.Add(nuevoAutor);
                await _context.SaveChangesAsync();

                return Ok(new { id = nuevoAutor.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al guardar los autores.", error = ex.Message });
            }
        }

        // Obtener un registro de autores por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Autores>> GetAutoresById(int id)
        {
            var autor = await _context.Autores.FindAsync(id);
            if (autor == null)
            {
                return NotFound(new { mensaje = $"Autor con id {id} no encontrado." });
            }
            return Ok(autor);
        }

        // Actualizar el registro de autores
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAutores(int id, [FromBody] Autores autoresActualizado)
        {
            if (autoresActualizado == null || id != autoresActualizado.Id)
            {
                return BadRequest(new { mensaje = "Datos inválidos o id no coincide." });
            }

            // Opcional: volver a formatear los campos, en caso de que se hayan modificado sin formateo.
            autoresActualizado.Autor1 = FormatearAutor(autoresActualizado.Autor1);
            autoresActualizado.Autor2 = FormatearAutor(autoresActualizado.Autor2);
            autoresActualizado.Autor3 = FormatearAutor(autoresActualizado.Autor3);
            autoresActualizado.Autor4 = FormatearAutor(autoresActualizado.Autor4);
            autoresActualizado.Autor5 = FormatearAutor(autoresActualizado.Autor5);
            autoresActualizado.Autor6 = FormatearAutor(autoresActualizado.Autor6);
            autoresActualizado.Autor7 = FormatearAutor(autoresActualizado.Autor7);
            autoresActualizado.Autor8 = FormatearAutor(autoresActualizado.Autor8);
            autoresActualizado.Autor9 = FormatearAutor(autoresActualizado.Autor9);
            autoresActualizado.Autor10 = FormatearAutor(autoresActualizado.Autor10);
            autoresActualizado.Autor11 = FormatearAutor(autoresActualizado.Autor11);
            autoresActualizado.Autor12 = FormatearAutor(autoresActualizado.Autor12);
            autoresActualizado.Autor13 = FormatearAutor(autoresActualizado.Autor13);
            autoresActualizado.Autor14 = FormatearAutor(autoresActualizado.Autor14);
            autoresActualizado.Autor15 = FormatearAutor(autoresActualizado.Autor15);
            autoresActualizado.Autor16 = FormatearAutor(autoresActualizado.Autor16);
            autoresActualizado.Autor17 = FormatearAutor(autoresActualizado.Autor17);
            autoresActualizado.Autor18 = FormatearAutor(autoresActualizado.Autor18);
            autoresActualizado.Autor19 = FormatearAutor(autoresActualizado.Autor19);
            autoresActualizado.Autor20 = FormatearAutor(autoresActualizado.Autor20);

            _context.Entry(autoresActualizado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(autoresActualizado);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Autores.AnyAsync(a => a.Id == id))
                {
                    return NotFound(new { mensaje = $"Autor con id {id} no existe." });
                }
                else
                {
                    throw;
                }
            }
        }

        // Obtener todos los registros de autores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Autores>>> GetAllAutores()
        {
            var autores = await _context.Autores.ToListAsync();
            return Ok(autores);
        }

        // Método auxiliar para formatear el nombre completo de un autor.
        private string FormatearAutor(string autor)
        {
            if (string.IsNullOrWhiteSpace(autor))
                return null; // Evita almacenar valores vacíos

            // Si ya contiene "|" se asume que ya está formateado correctamente.
            if (autor.Contains("|"))
                return autor;

            // Dividir el autor en palabras
            var partes = autor.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var connectors = new string[] { "de", "del", "da", "dos", "das" };

            if (partes.Length >= 4)
            {
                // Verificar si la tercera palabra desde el final es un conector
                string posibleConector = partes[partes.Length - 3].ToLower();
                if (connectors.Contains(posibleConector))
                {
                    // Caso: Apellido compuesto
                    string apellidoPaterno = partes[partes.Length - 3] + " " + partes[partes.Length - 2];
                    string apellidoMaterno = partes[partes.Length - 1];
                    string nombres = string.Join(" ", partes.Take(partes.Length - 3));
                    return $"{apellidoPaterno}|{nombres}|{apellidoMaterno}";
                }
                else
                {
                    // Caso estándar: las últimas dos palabras son los apellidos.
                    string apellidoPaterno = partes[partes.Length - 2];
                    string apellidoMaterno = partes[partes.Length - 1];
                    string nombres = string.Join(" ", partes.Take(partes.Length - 2));
                    return $"{apellidoPaterno}|{nombres}|{apellidoMaterno}";
                }
            }
            else if (partes.Length == 3)
            {
                // Caso con 3 partes: asumimos que la primera es el nombre y las dos siguientes son apellidos.
                string nombres = partes[0];
                string apellidoPaterno = partes[1];
                string apellidoMaterno = partes[2];
                return $"{apellidoPaterno}|{nombres}|{apellidoMaterno}";
            }
            else if (partes.Length == 2)
            {
                return $"{partes[0].Trim()}|{partes[1].Trim()}";
            }
            else
            {
                // Si sólo hay una palabra se devuelve tal cual.
                return autor;
            }
        }
    }
}
