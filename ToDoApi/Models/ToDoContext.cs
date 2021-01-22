using Microsoft.EntityFrameworkCore;

namespace ToDoApi.Models
{
  public class ToDoContext : DbContext
  {
    public ToDoContext(DbContextOptions<ToDoContext> options)
          : base(options)
    {
    }

    public DbSet<ToDoList> ToDoLists { get; set; }
  }
}