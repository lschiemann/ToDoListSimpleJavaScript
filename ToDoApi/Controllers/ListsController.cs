using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Models;

namespace ToDoApi.Controllers
{
  [Route("api/lists")]
  [ApiController]
  public class ListsController : ControllerBase
  {
    private readonly ToDoContext _context;

    public ListsController(ToDoContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ToDoList>>> GetAll()
    {
      return await _context.ToDoLists.Include(tl => tl.Items).ToListAsync();
    }

    [HttpGet("{listId}")]
    public async Task<ActionResult<ToDoList>> Get([FromRoute] int listId)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);

      if (toDoList == null)
      {
        return NotFound();
      }

      return toDoList;
    }

    [HttpPut("{listId}")]
    public async Task<IActionResult> Update([FromRoute] int listId, [FromBody] ToDoList toDoList)
    {
      if (listId != toDoList.Id)
      {
        return BadRequest();
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      _context.Entry(toDoList).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!_context.ToDoLists.Any(e => e.Id == listId))
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

    [HttpPost]
    public async Task<ActionResult<ToDoList>> Create([FromBody] string title)
    {
      var toDoList = new ToDoList()
      {
        Title = title
      };
      _context.ToDoLists.Add(toDoList);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(Get), new { listId = toDoList.Id }, toDoList);
    }

    [HttpDelete("{listId}")]
    public async Task<IActionResult> Delete([FromRoute] int listId)
    {
      var toDoList = await _context.ToDoLists.FindAsync(listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      _context.ToDoLists.Remove(toDoList);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
