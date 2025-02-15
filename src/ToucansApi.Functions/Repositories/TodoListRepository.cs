using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.Data;
using ToucansApi.Core.DTOs;
using ToucansApi.Core.Models;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Repositories;

public class TodoListRepository(ToucansDbContext context, ILogger<TodoListRepository> logger)
    : ITodoListRepository
{
    public async Task<IEnumerable<TodoListResponseDto>> GetAllAsync()
    {
        try
        {
            var lists = await context.TodoLists
                .AsNoTracking()
                .Include(l => l.Owner)
                .Include(l => l.Items)
                .Select(list => new TodoListResponseDto
                {
                    Id = list.Id,
                    Name = list.Name,
                    Description = list.Description,
                    CreatedAt = list.CreatedAt,
                    LastModifiedAt = list.LastModifiedAt,
                    ItemCount = list.Items != null ? list.Items.Count : 0,
                    CompletedItemCount = list.Items != null
                        ? list.Items.Count(i => i.Status == TodoStatus.Completed)
                        : 0,
                    Owner = list.Owner == null
                        ? null
                        : new UserResponseDto
                        {
                            Id = list.Owner.Id,
                            Email = list.Owner.Email,
                            FullName = $"{list.Owner.FirstName} {list.Owner.LastName}"
                        },
                    Items = list.Items != null
                        ? list.Items.Select(i => new TodoItemResponseDto
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Priority = i.Priority,
                            Status = i.Status,
                            DueDate = i.DueDate
                        })
                        : Enumerable.Empty<TodoItemResponseDto>()
                })
                .ToListAsync();

            return lists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all todo lists");
            throw;
        }
    }

    public async Task<TodoListResponseDto> GetByIdAsync(Guid id)
    {
        try
        {
            var list = await context.TodoLists
                .AsNoTracking()
                .Include(l => l.Owner)
                .Include(l => l.Items)
                .Where(l => l.Id == id)
                .Select(list => new TodoListResponseDto
                {
                    Id = list.Id,
                    Name = list.Name,
                    Description = list.Description,
                    CreatedAt = list.CreatedAt,
                    LastModifiedAt = list.LastModifiedAt,
                    ItemCount = list.Items != null ? list.Items.Count : 0,
                    CompletedItemCount =
                        list.Items != null ? list.Items.Count(i => i.Status == TodoStatus.Completed) : 0,
                    Owner = list.Owner != null
                        ? new UserResponseDto
                        {
                            Id = list.Owner.Id,
                            Email = list.Owner.Email,
                            FullName = $"{list.Owner.FirstName} {list.Owner.LastName}"
                        }
                        : null,
                    Items = list.Items != null
                        ? list.Items.Select(i => new TodoItemResponseDto
                        {
                            Id = i.Id,
                            Title = i.Title,
                            Priority = i.Priority,
                            Status = i.Status,
                            DueDate = i.DueDate
                        }).ToList()
                        : new List<TodoItemResponseDto>()
                })
                .ToListAsync();

            return list.First();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving todo list with id {Id}", id);
            throw;
        }
    }

    public async Task<TodoListResponseDto> CreateAsync(TodoListCreateDto dto)
    {
        try
        {
            var list = new TodoList
            {
                Name = dto.Name ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                OwnerId = dto.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            context.TodoLists.Add(list);
            await context.SaveChangesAsync();

            return await GetByIdAsync(list.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating todo list");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, TodoListUpdateDto dto)
    {
        try
        {
            var list = await context.TodoLists.FindAsync(id);
            if (list == null) return false;

            list.Name = dto.Name;
            list.Description = dto.Description;
            list.LastModifiedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating todo list {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var list = await context.TodoLists.FindAsync(id);
            if (list == null) return false;

            context.TodoLists.Remove(list);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting todo list {Id}", id);
            throw;
        }
    }

    public async Task<bool> ShareListAsync(Guid listId, ShareListDto dto)
    {
        if (!dto.UserId.HasValue || !dto.Permission.HasValue)
        {
            logger.LogError("Invalid ShareListDto: UserId or Permission is null");
            return false;
        }

        try
        {
            var list = await context.TodoLists.FindAsync(listId);
            if (list == null) return false;

            var user = await context.Users.FindAsync(dto.UserId.Value);
            if (user == null) return false;

            var existingShare = await context.TodoListShares
                .FirstOrDefaultAsync(s => s.TodoListId == listId && s.SharedWithUserId == dto.UserId.Value);

            if (existingShare != null)
            {
                context.TodoListShares.Remove(existingShare);
                var updatedShare = new TodoListShare
                {
                    TodoListId = listId,
                    SharedWithUserId = dto.UserId.Value,
                    Permission = dto.Permission.Value,
                    CreatedAt = existingShare.CreatedAt,
                    TodoList = list,
                    SharedWithUser = user
                };
                context.TodoListShares.Add(updatedShare);
            }
            else
            {
                var share = new TodoListShare
                {
                    TodoListId = listId,
                    SharedWithUserId = dto.UserId.Value,
                    Permission = dto.Permission.Value,
                    CreatedAt = DateTime.UtcNow,
                    TodoList = list,
                    SharedWithUser = user
                };
                context.TodoListShares.Add(share);
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sharing todo list {ListId} with user {UserId}", listId, dto.UserId);
            throw;
        }
    }

    public async Task<bool> RemoveShareAsync(Guid listId, Guid userId)
    {
        try
        {
            var share = await context.TodoListShares
                .FirstOrDefaultAsync(s => s.TodoListId == listId && s.SharedWithUserId == userId);

            if (share == null) return false;

            context.TodoListShares.Remove(share);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing share for list {ListId} from user {UserId}", listId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<ShareResponseDto>> GetSharesAsync(Guid listId)
    {
        try
        {
            return await context.TodoListShares
                .AsNoTracking()
                .Where(s => s.TodoListId == listId)
                .Select(s => new ShareResponseDto
                {
                    Id = s.Id,
                    SharedWithUser = new UserResponseDto
                    {
                        Id = s.SharedWithUser.Id,
                        Email = s.SharedWithUser.Email,
                        FullName = $"{s.SharedWithUser.FirstName} {s.SharedWithUser.LastName}"
                    },
                    Permission = s.Permission,
                    SharedAt = s.CreatedAt
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving shares for list {ListId}", listId);
            throw;
        }
    }
}