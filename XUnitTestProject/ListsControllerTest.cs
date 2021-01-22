using ToDoApi.Controllers;
using ToDoApi.Models;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace XUnitTestProject
{
  public class ListsControllerTest
  {
    DbContextOptions<ToDoContext> _options;

    public ListsControllerTest()
    {
      _options = new DbContextOptionsBuilder<ToDoContext>()
        .UseInMemoryDatabase(databaseName: "ToDoLists")
        .Options;
    }

    [Fact]
    public async void GetAll_gets_all_lists()
    {
      using (var context = new ToDoContext(_options))
      {
        context.ToDoLists.Add(new ToDoList
        {
          Title = "12345",
          Items = new List<ToDoItem>()
          {
            new ToDoItem()
            {
              Title = "345",
              Description = "any",
              Position = 1
            }
          }
        });

        context.SaveChanges();
      }
      using (var context = new ToDoContext(_options))
      {
        var sut = new ListsController(context);
        var actionResult = await sut.GetAll();
        Assert.NotEmpty(actionResult.Value);
        var list = actionResult.Value.First();

        Assert.Equal(1, list.Id);
        Assert.Equal("12345", list.Title);

        Assert.Collection(list.Items,
          item =>
          {
            Assert.Equal(1, item.Id);
            Assert.Equal("345", item.Title);
            Assert.Equal("any", item.Description);
            Assert.Equal(1, item.Position);
          }
        );
      }
    }
  }
}
