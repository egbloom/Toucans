using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ToucansApi.Core.Models

{
    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
    
    public enum SharePermission
    {
        ReadOnly = 0,
        ReadWrite = 1,
        Admin = 2
    }

    public enum TodoStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Blocked = 2,
        Completed = 3,
        Cancelled = 4
    }
}