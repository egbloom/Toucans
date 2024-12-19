using System;
using System.Collections.Generic;
using ToucansApi.Core.DTOs;
using ToucansApi.Core.Models;

namespace ToucansApi.Functions.Interfaces.Repositories
{
    public interface ITodoListRepository
    {
        Task<IEnumerable<TodoListResponseDto>> GetAllAsync();
        Task<TodoListResponseDto> GetByIdAsync(Guid id);
        Task<TodoListResponseDto> CreateAsync(TodoListCreateDto dto);
        Task<bool> UpdateAsync(Guid id, TodoListUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ShareListAsync(Guid listId, ShareListDto dto);
    }
}