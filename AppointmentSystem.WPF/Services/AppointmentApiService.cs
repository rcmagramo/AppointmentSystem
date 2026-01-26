using AppointmentSystem.WPF.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppointmentSystem.WPF.Services;

public class AppointmentApiService : IAppointmentApiService
{
    private readonly HttpClient _httpClient;

    public AppointmentApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Build query string
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            }

            var queryString = string.Join("&", queryParams);
            var url = $"/api/v1/appointments?{queryString}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PagedResult<AppointmentDto>>(json);
                return result ?? new PagedResult<AppointmentDto>();
            }

            await HandleErrorResponse(response);
            return new PagedResult<AppointmentDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"Network error: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("Request was cancelled");
        }
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/appointments/{id}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AppointmentDto>(json);
            }

            await HandleErrorResponse(response);
            return null;
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            throw new ApiException($"Error fetching appointment: {ex.Message}", ex);
        }
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "/api/v1/appointments",
                content,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AppointmentDto>(responseJson);
                return result ?? throw new ApiException("Invalid response from server");
            }

            await HandleErrorResponse(response);
            throw new ApiException("Failed to create appointment");
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            throw new ApiException($"Error creating appointment: {ex.Message}", ex);
        }
    }

    public async Task<AppointmentDto> UpdateAppointmentAsync(
        int id,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"/api/v1/appointments/{id}",
                content,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AppointmentDto>(responseJson);
                return result ?? throw new ApiException("Invalid response from server");
            }

            await HandleErrorResponse(response);
            throw new ApiException("Failed to update appointment");
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            throw new ApiException($"Error updating appointment: {ex.Message}", ex);
        }
    }

    public async Task DeleteAppointmentAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"/api/v1/appointments/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
            }
        }
        catch (Exception ex) when (ex is not ApiException)
        {
            throw new ApiException($"Error deleting appointment: {ex.Message}", ex);
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(content);

            if (problemDetails?.Errors != null && problemDetails.Errors.Any())
            {
                // Validation errors
                var errors = string.Join(Environment.NewLine,
                    problemDetails.Errors.SelectMany(kvp =>
                        kvp.Value.Select(error => $"• {kvp.Key}: {error}")));

                throw new ApiValidationException($"Validation failed:{Environment.NewLine}{errors}")
                {
                    ValidationErrors = problemDetails.Errors
                };
            }

            // Other errors
            var message = problemDetails?.Detail ?? problemDetails?.Title ?? "An error occurred";
            throw new ApiException(message);
        }
        catch (JsonException)
        {
            // Not a ProblemDetails response
            throw new ApiException($"Server error: {response.StatusCode}");
        }
    }
}

/// <summary>
/// Exception thrown by API service
/// </summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ApiValidationException : ApiException
{
    public Dictionary<string, string[]>? ValidationErrors { get; init; }

    public ApiValidationException(string message) : base(message) { }
}