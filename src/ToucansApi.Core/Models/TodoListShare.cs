using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToucansApi.Core.Models

{
    public class TodoListShare
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TodoListId { get; set; }

        [ForeignKey("TodoListId")]
        public virtual TodoList? TodoList { get; set; }
        
        [Required]
        public Guid SharedWithUserId { get; set; }

        [ForeignKey("SharedWithUserId")]
        public virtual User? SharedWithUser { get; set; }
        
        [Required]
        public SharePermission Permission { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}