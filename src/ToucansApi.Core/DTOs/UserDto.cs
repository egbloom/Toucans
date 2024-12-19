using System.ComponentModel.DataAnnotations;

namespace ToucansApi.Core.DTOs;

public class UserCreateDto
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string? FirstName { get; set; }

    [Required] [StringLength(50)] public string? LastName { get; set; }
}

public class UserFilterDto : BaseFilterDto
{
    public string? Email { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UserUpdateDto
{
    [StringLength(50)] public string? FirstName { get; set; }

    [StringLength(50)] public string? LastName { get; set; }
}

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}