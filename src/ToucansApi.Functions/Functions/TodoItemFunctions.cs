using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ToucansApi.Core.DTOs;
using ToucansApi.Functions.Extensions;
using ToucansApi.Functions.Interfaces.Repositories;

namespace ToucansApi.Functions.Functions
{
    public class TodoItemFunctions
    {
        private readonly ITodoListRepository _listRepository;
        private readonly ITodoItemRepository _itemRepository;
        private readonly ILogger<TodoItemFunctions> _logger;

        public TodoItemFunctions(
            ITodoListRepository listRepository,
            ITodoItemRepository itemRepository,
            ILogger<TodoItemFunctions> logger)
        {
            _listRepository = listRepository;
            _itemRepository = itemRepository;
            _logger = logger;
        }

        [Function("GetItems")]
        public async Task<HttpResponseData> GetItems(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists/{listId}/items")]
            HttpRequestData req,
            Guid listId)
        {
            try
            {
                var filterDto = await req.ReadFromJsonAsync<TodoItemFilterDto>() ?? new TodoItemFilterDto();
                filterDto.ListId = listId;

                var items = await _itemRepository.GetAllAsync(filterDto);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(items.ToApiResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting todo items for list {ListId}", listId);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error retrieving items"));
                return response;
            }
        }

        [Function("CreateItem")]
        public async Task<HttpResponseData> CreateItem(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "lists/{listId}/items")]
            HttpRequestData req,
            Guid listId)
        {
            try
            {
                var createDto = await req.ReadFromJsonAsync<TodoItemCreateDto>();
   if (createDto == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Invalid request body"));
            return badRequestResponse;
        }
                var item = await _itemRepository.CreateAsync(listId, createDto);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(item.ToApiResponse());
                response.Headers.Add("Location", $"/api/lists/{listId}/items/{item.Id}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating todo item in list {ListId}", listId);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error creating item"));
                return response;
            }
        }

        [Function("UpdateItem")]
        public async Task<HttpResponseData> UpdateItem(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "lists/{listId}/items/{id}")]
            HttpRequestData req,
            Guid listId,
            Guid id)
        {
            try
            {
                var updateDto = await req.ReadFromJsonAsync<TodoItemUpdateDto>();
        if (updateDto == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Invalid request body"));
            return badRequestResponse;
        }


                var updated = await _itemRepository.UpdateAsync(listId, id, updateDto);
                if (!updated)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Item not found"));
                    return notFoundResponse;
                }

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating todo item {Id} in list {ListId}", id, listId);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error updating item"));
                return response;
            }
        }

        [Function("DeleteItem")]
        public async Task<HttpResponseData> DeleteItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "lists/{listId}/items/{id}")]
            HttpRequestData req,
            Guid listId,
            Guid id)
        {
            try
            {
                var deleted = await _itemRepository.DeleteAsync(listId, id);
                if (!deleted)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Item not found"));
                    return notFoundResponse;
                }

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting todo item {Id} from list {ListId}", id, listId);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error deleting item"));
                return response;
            }
        }
    }
}