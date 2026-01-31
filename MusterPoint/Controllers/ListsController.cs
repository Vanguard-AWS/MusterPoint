using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusterPoint.Models;
using MusterPoint.Services;
using System.Security.Claims;

namespace MusterPoint.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ListsController : ControllerBase
{
    private readonly ListStorageService _listStorageService;
    private readonly ILogger<ListsController> _logger;

    public ListsController(ListStorageService listStorageService, ILogger<ListsController> logger)
    {
    _listStorageService = listStorageService;
        _logger = logger;
    }

    private string GetUserId()
    {
   var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
   _logger.LogError($"User ID claim not found. User claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}:{c.Value}"))}");
       throw new UnauthorizedAccessException("User ID not found in token");
 }
        
  return userIdClaim.Value;
    }

    /// <summary>
    /// Get all lists for the current user
    /// GET /api/lists
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserList>>> GetAllLists()
    {
        try
        {
     var userId = GetUserId();
         var lists = await _listStorageService.GetAllListsAsync(userId);
  return Ok(lists);
        }
        catch (Exception ex)
  {
   _logger.LogError($"Error getting lists: {ex.Message}");
            return StatusCode(500, new { message = "Error retrieving lists" });
        }
    }

    /// <summary>
    /// Get a specific list by ID
    /// GET /api/lists/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserList>> GetListById(string id)
    {
        try
        {
            var userId = GetUserId();
   var list = await _listStorageService.GetListByIdAsync(id, userId);
            if (list == null)
     return NotFound(new { message = "List not found" });
            return Ok(list);
        }
        catch (Exception ex)
     {
 _logger.LogError($"Error getting list: {ex.Message}");
       return StatusCode(500, new { message = "Error retrieving list" });
        }
    }

    /// <summary>
    /// Create or update a list
    /// POST /api/lists
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserList>> SaveList([FromBody] UserList list)
    {
        try
        {
    var userId = GetUserId();
       list.UserId = userId;
 await _listStorageService.SaveListAsync(list);
         return Ok(list);
    }
        catch (Exception ex)
   {
    _logger.LogError($"Error saving list: {ex.Message}");
        return StatusCode(500, new { message = "Error saving list" });
        }
    }

    /// <summary>
    /// Delete a list
    /// DELETE /api/lists/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteList(string id)
    {
        try
        {
          var userId = GetUserId();
   await _listStorageService.DeleteListAsync(id, userId);
            return Ok(new { message = "List deleted successfully" });
    }
        catch (Exception ex)
        {
  _logger.LogError($"Error deleting list: {ex.Message}");
     return StatusCode(500, new { message = "Error deleting list" });
    }
    }
}
