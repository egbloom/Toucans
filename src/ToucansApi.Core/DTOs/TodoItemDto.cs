using System.ComponentModel.DataAnnotations;
using ToucansApi.Core.Models;

namespace ToucansApi.Core.DTOs

{
    public class TodoItemCreateDto
    {
        [Required] [StringLength(200)] public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public Priority Priority { get; set; }

        public Guid? AssignedToId { get; set; }
    }

    public class TodoItemFilterDto : BaseFilterDto
    {
        public Priority? Priority { get; set; }
        public TodoStatus? Status { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public Guid ListId { get; set; }
        public Guid? AssignedToId { get; set; }
        public bool? IsOverdue { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }

    public class TodoItemUpdateDto
    {
        [Required] [StringLength(200)] public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public Priority Priority { get; set; }

        public TodoStatus Status { get; set; }

        public Guid? AssignedToId { get; set; }
    }

    public class TodoItemResponseDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Priority Priority { get; set; }
        public TodoStatus Status { get; set; }
        
        public Guid? AssignedToId { get; set; }
        public UserResponseDto? AssignedTo { get; set; }
    }
}