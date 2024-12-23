using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.Data;
using ToucansApi.Core.DTOs;
using ToucansApi.Core.Extensions;
using ToucansApi.Core.Models;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ToucansDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ToucansDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<UserResponseDto>> GetAllAsync(UserFilterDto filter)
    {
        try
        {
            var query = _context.Users.AsNoTracking();

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
            _logger.LogError(ex, "Error retrieving users");
            throw;
        }
    }

    public async Task<UserResponseDto?> GetByIdAsync(Guid id)
    {
        return await _context.Users
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

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

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
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TodoListResponseDto>> GetUserListsAsync(Guid userId)
    {
        return await _context.TodoLists
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
        return await _context.TodoListShares
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