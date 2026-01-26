namespace AppointmentSystem.WPF.Models;

/// <summary>
/// Matches the DTO from the refactored API
/// </summary>
public class AppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Pagination wrapper from API
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// Request to create appointment
/// </summary>
public class CreateAppointmentRequest
{
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request to update appointment
/// </summary>
public class UpdateAppointmentRequest
{
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// ProblemDetails error response from API
/// </summary>
public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int? Status { get; set; }
    public string? Detail { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}