using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ToucansApi.Core.Models;

namespace ToucansApi.Core.DTOs
{
    public class ShareListDto
    {
        [Required] public Guid?UserId { get; set; }

        [Required] public SharePermission? Permission { get; set; }
    }

    public class UpdateSharePermissionDto
    {
        [Required] public SharePermission? Permission { get; set; }
    }

    public class ShareResponseDto
    {
        public Guid Id { get; set; }
        public UserResponseDto? SharedWithUser { get; set; }
        public SharePermission Permission { get; set; }
        public DateTime SharedAt { get; set; }
    }
}
