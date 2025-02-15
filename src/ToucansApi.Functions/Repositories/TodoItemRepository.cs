using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.Data;
using ToucansApi.Core.DTOs;
using ToucansApi.Core.Exceptions;
using ToucansApi.Core.Extensions;
using ToucansApi.Core.Models;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Repositories;

public class TodoItemRepository(ToucansDbContext context, ILogger<TodoItemRepository> logger)
    : ITodoItemRepository
{
    private readonly ILogger<TodoItemRepository> _logger = logger;

    public async Task<PaginatedResponse<TodoItemResponseDto>> GetAllAsync(TodoItemFilterDto filter)
    {
        var query = BuildFilteredQuery(filter);
        var orderedQuery = ApplySorting(query, filter);
        var projectedQuery = ProjectToDto(orderedQuery);

        return await projectedQuery
            .ToPaginatedResponseAsync(filter.PageNumber, filter.PageSize);
    }

    public async Task<TodoItemResponseDto> CreateAsync(Guid listId, TodoItemCreateDto dto)
    {
        await ValidateListExists(listId);

        var item = new TodoItem
        {
            ListId = listId,
            Title = dto.Title ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            DueDate = dto.DueDate,
            Priority = dto.Priority,
            Status = TodoStatus.NotStarted,
            AssignedToId = dto.AssignedToId
        };

        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        return await GetByIdAsync(listId, item.Id);
    }

    public async Task<TodoItemResponseDto> GetByIdAsync(Guid listId, Guid id)
    {
        return await context.TodoItems
            .AsNoTracking()
            .Where(i => i.ListId == listId && i.Id == id)
            .Select(i => new TodoItemResponseDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                CreatedAt = i.CreatedAt,
                DueDate = i.DueDate,
                CompletedAt = i.CompletedAt,
                Priority = i.Priority,
                Status = i.Status,
                AssignedTo = i.AssignedTo != null && i.AssignedTo.Id != Guid.Empty
                    ? new UserResponseDto
                    {
                        Id = i.AssignedToId ?? Guid.Empty,
                        Email = i.AssignedTo.Email,
                        FullName = i.AssignedTo != null
                            ? $"{i.AssignedTo.FirstName} {i.AssignedTo.LastName}"
                            : string.Empty
                    }
                    : new UserResponseDto
                    {
                        Id = Guid.Empty,
                        Email = string.Empty,
                        FullName = string.Empty
                    }
            }).FirstAsync();
    }

    public async Task<bool> UpdateAsync(Guid listId, Guid id, TodoItemUpdateDto dto)
    {
        var item = await GetItemOrThrow(listId, id);

        item.Title = dto.Title ?? string.Empty;
        item.Description = dto.Description ?? string.Empty;
        item.DueDate = dto.DueDate;
        item.Priority = dto.Priority;
        item.AssignedToId = dto.AssignedToId;

        if (item.Status != dto.Status)
        {
            item.Status = dto.Status;
            item.CompletedAt = dto.Status == TodoStatus.Completed ? DateTime.UtcNow : null;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid listId, Guid id)
    {
        var item = await GetItemOrThrow(listId, id);

        context.TodoItems.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }

    private IQueryable<TodoItem> BuildFilteredQuery(TodoItemFilterDto filter)
    {
        var query = context.TodoItems
            .AsNoTracking()
            .Where(i => i.ListId == filter.ListId);

        if (filter.Priority.HasValue)
            query = query.Where(i => i.Priority == filter.Priority);

        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status);

        if (filter.DueDateFrom.HasValue)
            query = query.Where(i => i.DueDate >= filter.DueDateFrom);

        if (filter.DueDateTo.HasValue)
            query = query.Where(i => i.DueDate <= filter.DueDateTo);

        if (filter.AssignedToId.HasValue)
            query = query.Where(i => i.AssignedToId == filter.AssignedToId);

        return query;
    }

    private IQueryable<TodoItem> ApplySorting(IQueryable<TodoItem> query, TodoItemFilterDto filter)
    {
        return filter.SortBy?.ToLower() switch
        {
            "duedate" => filter.IsDescending
                ? query.OrderByDescending(i => i.DueDate)
                : query.OrderBy(i => i.DueDate),
            "priority" => filter.IsDescending
                ? query.OrderByDescending(i => i.Priority)
                : query.OrderBy(i => i.Priority),
            "status" => filter.IsDescending
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => query.OrderBy(i => i.CreatedAt)
        };
    }

    private IQueryable<TodoItemResponseDto> ProjectToDto(IQueryable<TodoItem> query)
    {
        return query.Select(i => new TodoItemResponseDto
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            CreatedAt = i.CreatedAt,
            DueDate = i.DueDate,
            CompletedAt = i.CompletedAt,
            Priority = i.Priority,
            Status = i.Status,
            AssignedTo = i.AssignedTo == null
                ? null
                : new UserResponseDto
                {
                    Id = i.AssignedTo.Id,
                    Email = i.AssignedTo.Email,
                    FullName = $"{i.AssignedTo.FirstName} {i.AssignedTo.LastName}"
                }
        });
    }

    public async Task<bool> UpdateStatusAsync(Guid listId, Guid id, TodoStatus status)
    {
        var item = await GetItemOrThrow(listId, id);

        item.Status = status;
        item.CompletedAt = status == TodoStatus.Completed ? DateTime.UtcNow : null;

        await context.SaveChangesAsync();
        return true;
    }

    private async Task<TodoItem> GetItemOrThrow(Guid listId, Guid id)
    {
        var item = await context.TodoItems
            .FirstOrDefaultAsync(i => i.ListId == listId && i.Id == id);

        if (item == null)
            throw new NotFoundException($"TodoItem with ID {id} not found in list {listId}");

        return item;
    }

    private async Task ValidateListExists(Guid listId)
    {
        if (!await context.TodoLists.AnyAsync(l => l.Id == listId))
            throw new NotFoundException($"TodoList with ID {listId} not found");
    }
}