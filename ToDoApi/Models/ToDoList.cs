using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ToDoApi.Models
{
  public class ToDoList
  {
    [Key]
    public int Id { get; set; }

    [Required]

    public string Title { get; set; }

    public List<ToDoItem> Items { get; set; }
  }
}
