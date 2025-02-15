using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.DTOs;
using ToucansApi.Functions.Extensions;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Functions;

public class UserFunctions(
    IUserRepository userRepository,
    ILogger<UserFunctions> logger)
{
    private readonly ILogger<UserFunctions> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IUserRepository _userRepository =
        userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    [Function(nameof(GetUsers))]
    public async Task<HttpResponseData> GetUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")]
        HttpRequestData req)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(GetUsers),
            ["Timestamp"] = DateTime.UtcNow
        });

        _logger.LogInformation("Starting user search operation");

        try
        {
            var filterQuery = await req.ReadFromJsonAsync<UserFilterDto>() ?? new UserFilterDto();
            var users = await _userRepository.GetAllAsync(filterQuery);
            _logger.LogInformation("Retrieved {Count} users matching filter criteria",
                users.TotalItems);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(users.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users. Error: {ErrorMessage}", ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving users");
        }
    }

    [Function(nameof(GetUser))]
    public async Task<HttpResponseData> GetUser(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:guid}")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(GetUser),
            ["UserId"] = id
        });

        _logger.LogInformation("Retrieving user {UserId}", id);

        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found", id);
                return await req.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");
            }

            _logger.LogInformation("Successfully retrieved user {UserId}", id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(user.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user {UserId}. Error: {ErrorMessage}",
                id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving user");
        }
    }

    [Function(nameof(CreateUser))]
    public async Task<HttpResponseData> CreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")]
        HttpRequestData req)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(CreateUser),
            ["Timestamp"] = DateTime.UtcNow
        });

        _logger.LogInformation("Creating new user");

        try
        {
            var createDto = await req.ReadFromJsonAsync<UserCreateDto>();
            if (createDto is null)
            {
                _logger.LogWarning("Invalid user creation request body");
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request body");
            }

            if (!IsValidUserCreateDto(createDto))
            {
                _logger.LogWarning("Invalid user data provided: {ValidationErrors}",
                    GetValidationErrors(createDto));
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid user data");
            }

            var user = await _userRepository.CreateAsync(createDto);

            _logger.LogInformation("Successfully created user {UserId}", user.Id);

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Location", $"/api/users/{user.Id}");
            await response.WriteAsJsonAsync(user.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user. Error: {ErrorMessage}", ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating user");
        }
    }

    [Function(nameof(UpdateUser))]
    public async Task<HttpResponseData> UpdateUser(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id:guid}")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(UpdateUser),
            ["UserId"] = id
        });

        _logger.LogInformation("Updating user {UserId}", id);

        try
        {
            var updateDto = await req.ReadFromJsonAsync<UserUpdateDto>();
            if (updateDto is null)
            {
                _logger.LogWarning("Invalid update request for user {UserId}", id);
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request body");
            }

            if (!IsValidUserUpdateDto(updateDto))
            {
                _logger.LogWarning("Invalid update data for user {UserId}: {ValidationErrors}",
                    id, GetValidationErrors(updateDto));
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid user data");
            }

            var updated = await _userRepository.UpdateAsync(id, updateDto);
            if (!updated)
            {
                _logger.LogWarning("User {UserId} not found for update", id);
                return await req.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");
            }

            _logger.LogInformation("Successfully updated user {UserId}", id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}. Error: {ErrorMessage}",
                id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error updating user");
        }
    }

    [Function(nameof(DeleteUser))]
    public async Task<HttpResponseData> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/{id:guid}")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(DeleteUser),
            ["UserId"] = id
        });

        _logger.LogInformation("Deleting user {UserId}", id);

        try
        {
            var deleted = await _userRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("User {UserId} not found for deletion", id);
                return await req.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");
            }

            _logger.LogInformation("Successfully deleted user {UserId}", id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user {UserId}. Error: {ErrorMessage}",
                id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error deleting user");
        }
    }

    [Function(nameof(GetUserTodoLists))]
    public async Task<HttpResponseData> GetUserTodoLists(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:guid}/lists")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(GetUserTodoLists),
            ["UserId"] = id
        });

        _logger.LogInformation("Retrieving todo lists for user {UserId}", id);

        try
        {
            var lists = await _userRepository.GetSharedListsAsync(id);

            _logger.LogInformation("Retrieved shared lists for user {UserId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(lists.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve lists for user {UserId}. Error: {ErrorMessage}",
                id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving user lists");
        }
    }

    [Function(nameof(GetUserSharedLists))]
    public async Task<HttpResponseData> GetUserSharedLists(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id:guid}/shared")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(GetUserSharedLists),
            ["UserId"] = id
        });

        _logger.LogInformation("Retrieving shared lists for user {UserId}", id);

        try
        {
            var lists = await _userRepository.GetSharedListsAsync(id);

            _logger.LogInformation("Retrieved shared lists for user {UserId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(lists.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve shared lists for user {UserId}. Error: {ErrorMessage}",
                id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving shared lists");
        }
    }

    private static bool IsValidUserCreateDto(UserCreateDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.Email) &&
               !string.IsNullOrWhiteSpace(dto.FirstName) &&
               !string.IsNullOrWhiteSpace(dto.LastName);
    }

    private static bool IsValidUserUpdateDto(UserUpdateDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.FirstName) &&
               !string.IsNullOrWhiteSpace(dto.LastName);
    }

    private static string GetValidationErrors<T>(T dto)
    {
        var errors = new List<string>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(dto);
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                errors.Add($"{prop.Name} is required");
        }

        return string.Join(", ", errors);
    }
}