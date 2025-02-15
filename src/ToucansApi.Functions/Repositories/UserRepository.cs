using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.Data;
using ToucansApi.Core.DTOs;
using ToucansApi.Core.Extensions;
using ToucansApi.Core.Models;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Repositories;

public class UserRepository(ToucansDbContext context, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<PaginatedResponse<UserResponseDto>> GetAllAsync(UserFilterDto filter)
    {
        try
        {
            var query = context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(u =>
                    EF.Functions.Like(u.Email, $"%{filter.SearchTerm}%") ||
                    EF.Functions.Like(u.FirstName, $"%{filter.SearchTerm}%") ||
                    EF.Functions.Like(u.LastName, $"%{filter.SearchTerm}%"));

            if (filter.StartDate.HasValue) query = query.Where(u => u.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue) query = query.Where(u => u.CreatedAt <= filter.EndDate.Value);

            return await query
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToPaginatedResponseAsync(filter.PageNumber, filter.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            throw;
        }
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
    {
        var user = new User
        {
            Email = dto.Email ?? string.Empty,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<bool> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return false;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TodoListResponseDto>> GetUserListsAsync(Guid userId)
    {
        return await context.TodoLists
            .AsNoTracking()
            .Where(l => l.OwnerId == userId)
            .Select(l => new TodoListResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                CreatedAt = l.CreatedAt,
                LastModifiedAt = l.LastModifiedAt,
                ItemCount = l.Items.Count,
                CompletedItemCount = l.Items.Count(i => i.Status == TodoStatus.Completed)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoListResponseDto>> GetSharedListsAsync(Guid userId)
    {
        return await context.TodoListShares
            .AsNoTracking()
            .Where(s => s.SharedWithUserId == userId)
            .Select(s => new TodoListResponseDto
            {
                Id = s.TodoList.Id,
                Name = s.TodoList.Name,
                Description = s.TodoList.Description,
                CreatedAt = s.TodoList.CreatedAt,
                LastModifiedAt = s.TodoList.LastModifiedAt,
                ItemCount = s.TodoList.Items.Count,
                CompletedItemCount = s.TodoList.Items.Count(i => i.Status == TodoStatus.Completed),
                Permission = s.Permission
            })
            .ToListAsync();
    }
}