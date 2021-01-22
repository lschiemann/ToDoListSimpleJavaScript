using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApi.Models
{
  public class ToDoItem
  {
    [Key]
    public int Id { get; set; }

    [ForeignKey("ToDoListId")]
    public int ToDoListId { get; set; }

    [Required]

    public string Title { get; set; }

    public string Description { get; set; }

    public int Position { get; set; }

    public bool IsCompleted { get; set; }
  }
}