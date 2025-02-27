using Marten;
using ToucansApi.Core.Models;

namespace ToucansApi.Functions.Events;

public class TodoListEventHandlers
{
    public static async Task Handle(TodoListCreated created, IDocumentSession session)
    {
        await session.SaveChangesAsync();
    }

    public static async Task Handle(TodoItemAdded itemAdded, IDocumentSession session)
    {
        await session.SaveChangesAsync();
    }
}