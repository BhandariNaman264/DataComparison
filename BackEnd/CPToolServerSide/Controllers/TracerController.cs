using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CPToolServerSide.Models;

namespace CPToolServerSide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TracerController : ControllerBase
    {
        private readonly CPTDBContext _context;

        public TracerController(CPTDBContext context)
        {
            _context = context;
        }

        // GET: api/Tracer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnalyzePSR>>> GetAnalyze()
        {
            return await _context.AnalyzePSR.ToListAsync();
        }

        // GET: api/Tracer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnalyzePSR>> GetAnalyze(int id)
        {
            var analyze = await _context.AnalyzePSR.FindAsync(id);

            if (analyze == null)
            {
                return NotFound();
            }

            return analyze;
        }

        // PUT: api/Tracer/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnalyze(int id, AnalyzePSR analyze)
        {
            if (id != analyze.ID)
            {
                return BadRequest();
            }

            _context.Entry(analyze).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnalyzeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tracer
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AnalyzePSR>> PostAnalyze(AnalyzePSR analyze)
        {
            _context.AnalyzePSR.Add(analyze);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnalyze", new { id = analyze.ID }, analyze);
        }

        // DELETE: api/Tracer/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AnalyzePSR>> DeleteAnalyze(int id)
        {
            var analyze = await _context.AnalyzePSR.FindAsync(id);
            if (analyze == null)
            {
                return NotFound();
            }

            _context.AnalyzePSR.Remove(analyze);
            await _context.SaveChangesAsync();

            return analyze;
        }

        private bool AnalyzeExists(int id)
        {
            return _context.AnalyzePSR.Any(e => e.ID == id);
        }
    }
}
