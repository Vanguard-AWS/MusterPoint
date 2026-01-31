using MongoDB.Driver;
using MusterPoint.Models;

namespace MusterPoint.Services;

/// <summary>
/// Server-side list storage service using MongoDB
/// </summary>
public class ListStorageService
{
    private readonly IMongoCollection<UserList> _listsCollection;
    private readonly ILogger<ListStorageService> _logger;

    public ListStorageService(IMongoClient mongoClient, ILogger<ListStorageService> logger)
    {
        _logger = logger;
        var database = mongoClient.GetDatabase("musterpoint");
  _listsCollection = database.GetCollection<UserList>("lists");
    }

 public async Task<List<UserList>> GetAllListsAsync(string userId)
    {
        try
 {
      var filter = Builders<UserList>.Filter.Eq(l => l.UserId, userId);
  var lists = await _listsCollection.Find(filter).ToListAsync();
  return lists;
      }
        catch (Exception ex)
        {
     _logger.LogError($"Error reading lists from database: {ex.Message}");
   return new();
        }
    }

    public async Task<UserList?> GetListByIdAsync(string listId, string userId)
    {
        try
      {
         var filter = Builders<UserList>.Filter.And(
   Builders<UserList>.Filter.Eq(l => l.Id, listId),
       Builders<UserList>.Filter.Eq(l => l.UserId, userId)
          );
     return await _listsCollection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
   {
       _logger.LogError($"Error reading list from database: {ex.Message}");
            return null;
    }
    }

    public async Task SaveListAsync(UserList list)
    {
 try
      {
         list.UpdatedAt = DateTime.UtcNow;
            
            var filter = Builders<UserList>.Filter.Eq(l => l.Id, list.Id);
            var options = new ReplaceOptions { IsUpsert = true };
        
     await _listsCollection.ReplaceOneAsync(filter, list, options);
     _logger.LogInformation($"List '{list.Name}' saved successfully");
        }
        catch (Exception ex)
        {
        _logger.LogError($"Error saving list to database: {ex.Message}");
       throw;
      }
    }

    public async Task DeleteListAsync(string listId, string userId)
    {
        try
        {
      var filter = Builders<UserList>.Filter.And(
   Builders<UserList>.Filter.Eq(l => l.Id, listId),
          Builders<UserList>.Filter.Eq(l => l.UserId, userId)
            );
   
            await _listsCollection.DeleteOneAsync(filter);
        _logger.LogInformation($"List '{listId}' deleted successfully");
        }
        catch (Exception ex)
        {
     _logger.LogError($"Error deleting list from database: {ex.Message}");
            throw;
        }
    }
}
