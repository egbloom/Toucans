using ToucansApi.Core.DTOs;

namespace ToucansApi.Functions.Interfaces.Repositories;

public interface IUserRepository
{
    Task<PaginatedResponse<UserResponseDto>> GetAllAsync(UserFilterDto filter);
    Task<UserResponseDto?> GetByIdAsync(Guid id);
    Task<UserResponseDto> CreateAsync(UserCreateDto dto);
    Task<bool> UpdateAsync(Guid id, UserUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<TodoListResponseDto>> GetUserListsAsync(Guid userId);
    Task<IEnumerable<TodoListResponseDto>> GetSharedListsAsync(Guid userId);
}