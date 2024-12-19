using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ToucansApi.Core.DTOs;
using ToucansApi.Functions.Interfaces.Repositories;
using ToucansApi.Functions.Extensions;

namespace ToucansApi.Functions.Functions
{
    public class TodoListFunctions
    {
        private readonly ITodoListRepository _listRepository;
        private readonly ITodoItemRepository _itemRepository;
        private readonly ILogger<TodoListFunctions> _logger;

        public TodoListFunctions(
            ITodoListRepository listRepository,
            ITodoItemRepository itemRepository,
            ILogger<TodoListFunctions> logger)
        {
            _listRepository = listRepository;
            _itemRepository = itemRepository;
            _logger = logger;
        }

        [Function("GetLists")]
        public async Task<HttpResponseData> GetLists(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists")] HttpRequestData req)
        {
            try
            {
                var lists = await _listRepository.GetAllAsync();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(lists.ToApiResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting todo lists");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error retrieving lists"));
                return response;
            }
        }

        [Function("GetList")]
        public async Task<HttpResponseData> GetList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lists/{id}")] HttpRequestData req,
            Guid id)
        {
            try
            {
                var list = await _listRepository.GetByIdAsync(id);
                if (list == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("List not found"));
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(list.ToApiResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting todo list {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error retrieving list"));
                return response;
            }
        }

        [Function("CreateList")]
        public async Task<HttpResponseData> CreateList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "lists")] HttpRequestData req)
        {
            try
            {
                var createDto = await req.ReadFromJsonAsync<TodoListCreateDto>();
        if (createDto == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Invalid request body"));
            return badRequestResponse;
        }

                var list = await _listRepository.CreateAsync(createDto);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(list.ToApiResponse());
                response.Headers.Add("Location", $"/api/lists/{list.Id}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating todo list");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error creating list"));
                return response;
            }
        }

        [Function("UpdateList")]
        public async Task<HttpResponseData> UpdateList(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "lists/{id}")] HttpRequestData req,
            Guid id)
        {
            try
            {
                var updateDto = await req.ReadFromJsonAsync<TodoListUpdateDto>();
        if (updateDto == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Invalid request body"));
            return badRequestResponse;
        }

                var updated = await _listRepository.UpdateAsync(id, updateDto);
                if (!updated)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("List not found"));
                    return notFoundResponse;
                }

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating todo list {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error updating list"));
                return response;
            }
        }

        [Function("DeleteList")]
        public async Task<HttpResponseData> DeleteList(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "lists/{id}")] HttpRequestData req,
            Guid id)
        {
            try
            {
                var deleted = await _listRepository.DeleteAsync(id);
                if (!deleted)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("List not found"));
                    return notFoundResponse;
                }

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting todo list {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error deleting list"));
                return response;
            }
        }
    }
}