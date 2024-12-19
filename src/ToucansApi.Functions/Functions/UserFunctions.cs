using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ToucansApi.Functions.Interfaces.Repositories;
using ToucansApi.Core.DTOs;
using ToucansApi.Functions.Extensions;

namespace ToucansApi.Functions.Functions
{
    public class UserFunctions
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserFunctions> _logger;

        public UserFunctions(
            IUserRepository userRepository,
            ILogger<UserFunctions> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [Function("GetUsers")]
        public async Task<HttpResponseData> GetUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequestData req)
        {
            try
            {
                var filterQuery = await req.ReadFromJsonAsync<UserFilterDto>() ?? new UserFilterDto();
                var users = await _userRepository.GetAllAsync(filterQuery);
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(users.ToApiResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error retrieving users"));
                return response;
            }
        }

        [Function("GetUser")]
        public async Task<HttpResponseData> GetUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}")] HttpRequestData req,
            Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("User not found"));
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(user.ToApiResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error retrieving user"));
                return response;
            }
        }

[Function("CreateUser")]
public async Task<HttpResponseData> CreateUser(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] HttpRequestData req)
{
    try
    {
        var createDto = await req.ReadFromJsonAsync<UserCreateDto>();
        if (createDto == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Invalid request body"));
            return badRequestResponse;
        }

        var user = await _userRepository.CreateAsync(createDto);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(user.ToApiResponse());
        response.Headers.Add("Location", $"/api/users/{user.Id}");
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating user");
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);
        await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error creating user"));
        return response;
    }
}

[Function("UpdateUser")]
public async Task<HttpResponseData> UpdateUser(
    [HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id}")] HttpRequestData req,
    Guid id)
{
    try
    {
        var updateDto = await req.ReadFromJsonAsync<UserUpdateDto>();
        if (updateDto == null)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("Invalid request body"));
            return badRequestResponse;
        }

        var updated = await _userRepository.UpdateAsync(id, updateDto);
        if (!updated)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("User not found"));
            return notFoundResponse;
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating user {Id}", id);
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);
        await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error updating user"));
        return response;
    }
}

        [Function("DeleteUser")]
        public async Task<HttpResponseData> DeleteUser(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/{id}")] HttpRequestData req,
            Guid id)
        {
            try
            {
                var deleted = await _userRepository.DeleteAsync(id);
                if (!deleted)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("User not found"));
                    return notFoundResponse;
                }

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error deleting user"));
                return response;
            }
        }

        [Function("GetUserTodoLists")]
        public async Task<HttpResponseData> GetUserTodoLists(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}/lists")] HttpRequestData req,
            Guid id)
        {
            try
            {
                var lists = await _userRepository.GetUserListsAsync(id);
                if (lists == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("User not found"));
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(lists.ToApiResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lists for user {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error retrieving user lists"));
                return response;
            }
        }

        [Function("GetUserSharedLists")]
        public async Task<HttpResponseData> GetUserSharedLists(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}/shared")] HttpRequestData req,
            Guid id)
        {
            try
            {
                var lists = await _userRepository.GetSharedListsAsync(id);
                if (lists == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(ApiResponse<string>.Fail("User not found"));
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(lists.ToApiResponse());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shared lists for user {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ApiResponse<string>.Fail("Error retrieving shared lists"));
                return response;
            }
        }
    }
}