using System;
using System.Threading.Tasks;
using ToucansApi.Core.DTOs;
using ToucansApi.Core.Models;

namespace ToucansApi.Functions.Interfaces.Repositories

{
    public interface ITodoItemRepository
    {
        Task<PaginatedResponse<TodoItemResponseDto>> GetAllAsync(TodoItemFilterDto filter);
        Task<TodoItemResponseDto> GetByIdAsync(Guid listId, Guid id);
        Task<TodoItemResponseDto> CreateAsync(Guid listId, TodoItemCreateDto dto);
        Task<bool> UpdateAsync(Guid listId, Guid id, TodoItemUpdateDto dto);
        Task<bool> DeleteAsync(Guid listId, Guid id);
    }
}