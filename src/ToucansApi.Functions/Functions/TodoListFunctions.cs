using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.DTOs;
using ToucansApi.Functions.Extensions;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Functions;

public class TodoListFunctions(
    ITodoListRepository listRepository,
    ITodoItemRepository itemRepository,
    ILogger<TodoListFunctions> logger)
{
    private readonly ITodoItemRepository _itemRepository =
        itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));

    private readonly ITodoListRepository _listRepository =
        listRepository ?? throw new ArgumentNullException(nameof(listRepository));

    private readonly ILogger<TodoListFunctions> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(nameof(GetLists))]
    public async Task<HttpResponseData> GetLists(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists")]
        HttpRequestData req)
    {
        using var scope = _logger.BeginScope("Operation {Operation}", nameof(GetLists));
        _logger.LogInformation("Starting get lists operation");

        try
        {
            var lists = await _listRepository.GetAllAsync();

            _logger.LogInformation("Successfully retrieved {Count} lists", lists.Count());

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(lists.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve todo lists. Error: {ErrorMessage}", ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving lists");
        }
    }

    [Function(nameof(GetList))]
    public async Task<HttpResponseData> GetList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists/{id:guid}")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(GetList),
            ["ListId"] = id
        });

        _logger.LogInformation("Retrieving list with ID: {ListId}", id);

        try
        {
            var list = await _listRepository.GetByIdAsync(id);

            _logger.LogInformation("Successfully retrieved list {ListId}", id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(list.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve list {ListId}. Error: {ErrorMessage}", id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving list");
        }
    }

    [Function(nameof(CreateList))]
    public async Task<HttpResponseData> CreateList(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "lists")]
        HttpRequestData req)
    {
        using var scope = _logger.BeginScope("Operation {Operation}", nameof(CreateList));
        _logger.LogInformation("Starting create list operation");

        try
        {
            var createDto = await req.ReadFromJsonAsync<TodoListCreateDto>();
            if (createDto is null)
            {
                _logger.LogWarning("Invalid request body received");
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate DTO properties
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                _logger.LogWarning("List name is required");
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "List name is required");
            }

            var list = await _listRepository.CreateAsync(createDto);
            _logger.LogInformation("Successfully created list with ID: {ListId}", list.Id);

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Location", $"/api/lists/{list.Id}");
            await response.WriteAsJsonAsync(list.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create todo list. Error: {ErrorMessage}", ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating list");
        }
    }

    [Function(nameof(UpdateList))]
    public async Task<HttpResponseData> UpdateList(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "lists/{id:guid}")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(UpdateList),
            ["ListId"] = id
        });

        _logger.LogInformation("Starting update operation for list {ListId}", id);

        try
        {
            var updateDto = await req.ReadFromJsonAsync<TodoListUpdateDto>();
            if (updateDto is null)
            {
                _logger.LogWarning("Invalid update request body for list {ListId}", id);
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate DTO properties
            if (string.IsNullOrWhiteSpace(updateDto.Name))
            {
                _logger.LogWarning("List title is required for update");
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "List title is required");
            }

            var updated = await _listRepository.UpdateAsync(id, updateDto);
            if (!updated)
            {
                _logger.LogWarning("List {ListId} not found for update", id);
                return await req.CreateErrorResponse(HttpStatusCode.NotFound, "List not found");
            }

            _logger.LogInformation("Successfully updated list {ListId}", id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update list {ListId}. Error: {ErrorMessage}", id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error updating list");
        }
    }

    [Function(nameof(DeleteList))]
    public async Task<HttpResponseData> DeleteList(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "lists/{id:guid}")]
        HttpRequestData req,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(DeleteList),
            ["ListId"] = id
        });

        _logger.LogInformation("Starting delete operation for list {ListId}", id);

        try
        {
            var deleted = await _listRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("List {ListId} not found for deletion", id);
                return await req.CreateErrorResponse(HttpStatusCode.NotFound, "List not found");
            }

            _logger.LogInformation("Successfully deleted list {ListId}", id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete list {ListId}. Error: {ErrorMessage}", id, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error deleting list");
        }
    }
}

// Extension methods for consistent response handling
public static class HttpRequestDataExtensions
{
    public static async Task<HttpResponseData> CreateErrorResponse(
        this HttpRequestData req,
        HttpStatusCode statusCode,
        string message)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(ApiResponse<string>.Fail(message));
        return response;
    }
}