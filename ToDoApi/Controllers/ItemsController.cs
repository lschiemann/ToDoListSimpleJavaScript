using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Models;

namespace ToDoApi.Controllers
{
  [Route("api/lists/{listId}/items/")]
  [ApiController]
  public class ItemsController : ControllerBase
  {
    private readonly ToDoContext _context;

    public ItemsController(ToDoContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ToDoItem>>> GetAll([FromRoute] int listId)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);

      if (toDoList == null)
      {
        return NotFound();
      }

      return toDoList.Items.OrderBy(ti => ti.Position).ToArray();
    }

    [HttpGet("{itemId}")]
    public async Task<ActionResult<ToDoItem>> Get([FromRoute] int listId, [FromRoute] int itemId)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);

      if (toDoList == null)
      {
        return NotFound();
      }

      var toDoItem = toDoList.Items.SingleOrDefault(ti => ti.Id == itemId);

      if (toDoItem == null)
      {
        return NotFound();
      }

      return toDoItem;
    }

    [HttpPut("{itemId}")]
    public async Task<IActionResult> Change([FromRoute] int listId, [FromRoute] int itemId, [FromBody] ToDoItem toDoItem)
    {
      if (itemId != toDoItem.Id)
      {
        return BadRequest();
      }

      //_context.Entry(toDoItem).State = EntityState.Modified;

      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      var toDoItemToChange = toDoList.Items.SingleOrDefault(ti => ti.Id == itemId);
      if (toDoItemToChange == null)
      {
        return NotFound();
      }

      toDoItemToChange.Title = toDoItem.Title;
      toDoItemToChange.Description = toDoItem.Description;
      toDoItemToChange.IsCompleted = toDoItem.IsCompleted;

      await _context.SaveChangesAsync();

      return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<ToDoItem>> Create([FromRoute] int listId, [FromBody] string title)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      var lastPosition = toDoList.Items.Select(ti => ti.Position).DefaultIfEmpty(-1).Max();
      var toDoItem = new ToDoItem()
      {
        ToDoListId = toDoList.Id,
        Title = title,
        Position = lastPosition + 1
      };
      toDoList.Items.Add(toDoItem);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(Get), new { itemId = toDoItem.Id, listId = listId }, toDoItem);
    }

    [HttpDelete("{itemId}")]
    public async Task<IActionResult> Delete([FromRoute] int listId, [FromRoute] int itemId)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      var toDoItem = toDoList.Items.SingleOrDefault(ti => ti.Id == itemId);
      if (toDoItem == null)
      {
        return NotFound();
      }

      foreach (var item in toDoList.Items.Where(ti => ti.Position > toDoItem.Position))
      {
        item.Position -= 1;
      }

      toDoList.Items.Remove(toDoItem);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    [HttpDelete("deleteAll")]
    public async Task<IActionResult> DeleteAll([FromRoute] int listId)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      toDoList.Items.Clear();
      await _context.SaveChangesAsync();

      return NoContent();
    }

    [HttpPost("{itemId}/moveUp")]
    public async Task<ActionResult<ToDoItem>> MoveUp([FromRoute] int listId, [FromRoute] int itemId)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      var toDoItem = toDoList.Items.SingleOrDefault(ti => ti.Id == itemId);
      if (toDoItem == null)
      {
        return NotFound();
      }

      var predecessor = toDoList.Items.Where(ti => ti.Position < toDoItem.Position).OrderBy(ti => ti.Position).LastOrDefault();
      if (predecessor == null)
      {
        return NoContent();
      }

      predecessor.Position += 1;
      toDoItem.Position -= 1;

      await _context.SaveChangesAsync();

      return toDoItem;
    }

    [HttpPost("{itemId}/moveDown")]
    public async Task<ActionResult<ToDoItem>> MoveDown([FromRoute] int listId, [FromRoute] int itemId)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      var toDoItem = toDoList.Items.SingleOrDefault(ti => ti.Id == itemId);
      if (toDoItem == null)
      {
        return NotFound();
      }

      var successor = toDoList.Items.Where(ti => ti.Position > toDoItem.Position).OrderBy(ti => ti.Position).FirstOrDefault();
      if (successor == null)
      {
        return NoContent();
      }

      successor.Position -= 1;
      toDoItem.Position += 1;

      await _context.SaveChangesAsync();

      return toDoItem;
    }

    [HttpPost("{itemId}/moveToPosition")]
    public async Task<ActionResult<ToDoItem>> MoveToPosition([FromRoute] int listId, [FromRoute] int itemId, [FromBody] int position)
    {
      var toDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (toDoList == null)
      {
        return NotFound();
      }

      var toDoItem = toDoList.Items.SingleOrDefault(ti => ti.Id == itemId);
      if (toDoItem == null)
      {
        return NotFound();
      }

      if (position < 0 && position >= toDoList.Items.Count)
      {
        return BadRequest();
      }

      if (position > toDoItem.Position)
      {
        foreach (var item in toDoList.Items.Where(ti => ti.Position > toDoItem.Position && ti.Position <= position))
        {
          item.Position -= 1;
        }
      }
      else if (position < toDoItem.Position)
      {
        foreach (var item in toDoList.Items.Where(ti => ti.Position < toDoItem.Position && ti.Position >= position))
        {
          item.Position += 1;
        }
      }

      toDoItem.Position = position;

      await _context.SaveChangesAsync();

      return toDoItem;
    }

    [HttpPost("{itemId}/moveToAnotherList")]
    public async Task<ActionResult<ToDoItem>> MoveToAnotherList([FromRoute] int listId, [FromRoute] int itemId, [FromBody] int destinationListId)
    {
      var sourceToDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == listId);
      if (sourceToDoList == null)
      {
        return NotFound();
      }

      var destinationToDoList = await _context.ToDoLists.Include(tl => tl.Items).SingleOrDefaultAsync(tl => tl.Id == destinationListId);
      if (destinationToDoList == null)
      {
        return NotFound();
      }

      var toDoItem = sourceToDoList.Items.SingleOrDefault(ti => ti.Id == itemId);
      if (toDoItem == null)
      {
        return NotFound();
      }

      var lastPosition = sourceToDoList.Items.Select(ti => ti.Position).DefaultIfEmpty(-1).Max();
      toDoItem.ToDoListId = destinationListId;
      toDoItem.Position = lastPosition;

      await _context.SaveChangesAsync();

      return toDoItem;
    }
  }
}
