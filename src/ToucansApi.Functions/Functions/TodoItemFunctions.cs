using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ToucansApi.Core.DTOs;
using ToucansApi.Functions.Extensions;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Functions;

public class TodoItemFunctions(
    ITodoListRepository listRepository,
    ITodoItemRepository itemRepository,
    ILogger<TodoItemFunctions> logger)
{
    private readonly ITodoItemRepository _itemRepository =
        itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));

    private readonly ITodoListRepository _listRepository =
        listRepository ?? throw new ArgumentNullException(nameof(listRepository));

    private readonly ILogger<TodoItemFunctions> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(nameof(GetItems))]
    public async Task<HttpResponseData> GetItems(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists/{listId:guid}/items")]
        HttpRequestData req,
        Guid listId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(GetItems),
            ["ListId"] = listId
        });

        _logger.LogInformation("Retrieving items for list {ListId}", listId);

        try
        {
            var filterDto = await req.ReadFromJsonAsync<TodoItemFilterDto>() ?? new TodoItemFilterDto();
            filterDto.ListId = listId;

            var items = await _itemRepository.GetAllAsync(filterDto);

            _logger.LogInformation(
                "Retrieved {ItemCount} items for list {ListId}",
                items.TotalItems,
                listId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve items for list {ListId}. Error: {ErrorMessage}",
                listId, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error retrieving items");
        }
    }

    [Function(nameof(CreateItem))]
    public async Task<HttpResponseData> CreateItem(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "lists/{listId:guid}/items")]
        HttpRequestData req,
        Guid listId)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(CreateItem),
            ["ListId"] = listId
        });

        _logger.LogInformation("Creating new item in list {ListId}", listId);

        try
        {
            var createDto = await req.ReadFromJsonAsync<TodoItemCreateDto>();
            if (createDto is null)
            {
                _logger.LogWarning("Invalid request body for list {ListId}", listId);
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request body");
            }

            if (string.IsNullOrWhiteSpace(createDto.Title))
            {
                _logger.LogWarning("Item title is required for list {ListId}", listId);
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Item title is required");
            }

            var item = await _itemRepository.CreateAsync(listId, createDto);

            _logger.LogInformation("Created item {ItemId} in list {ListId}", item.Id, listId);

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Location", $"/api/lists/{listId}/items/{item.Id}");
            await response.WriteAsJsonAsync(item.ToApiResponse());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create item in list {ListId}. Error: {ErrorMessage}",
                listId, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error creating item");
        }
    }

    [Function(nameof(UpdateItem))]
    public async Task<HttpResponseData> UpdateItem(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "lists/{listId:guid}/items/{id:guid}")]
        HttpRequestData req,
        Guid listId,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(UpdateItem),
            ["ListId"] = listId,
            ["ItemId"] = id
        });

        _logger.LogInformation("Updating item {ItemId} in list {ListId}", id, listId);

        try
        {
            var updateDto = await req.ReadFromJsonAsync<TodoItemUpdateDto>();
            if (updateDto is null)
            {
                _logger.LogWarning("Invalid update request for item {ItemId} in list {ListId}",
                    id, listId);
                return await req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request body");
            }

            var updated = await _itemRepository.UpdateAsync(listId, id, updateDto);
            if (!updated)
            {
                _logger.LogWarning("Item {ItemId} not found in list {ListId}", id, listId);
                return await req.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found");
            }

            _logger.LogInformation("Successfully updated item {ItemId} in list {ListId}", id, listId);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to update item {ItemId} in list {ListId}. Error: {ErrorMessage}",
                id, listId, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error updating item");
        }
    }

    [Function(nameof(DeleteItem))]
    public async Task<HttpResponseData> DeleteItem(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "lists/{listId:guid}/items/{id:guid}")]
        HttpRequestData req,
        Guid listId,
        Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = nameof(DeleteItem),
            ["ListId"] = listId,
            ["ItemId"] = id
        });

        _logger.LogInformation("Deleting item {ItemId} from list {ListId}", id, listId);

        try
        {
            var deleted = await _itemRepository.DeleteAsync(listId, id);
            if (!deleted)
            {
                _logger.LogWarning("Item {ItemId} not found in list {ListId} for deletion",
                    id, listId);
                return await req.CreateErrorResponse(HttpStatusCode.NotFound, "Item not found");
            }

            _logger.LogInformation("Successfully deleted item {ItemId} from list {ListId}",
                id, listId);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete item {ItemId} from list {ListId}. Error: {ErrorMessage}",
                id, listId, ex.Message);
            return await req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error deleting item");
        }
    }
}