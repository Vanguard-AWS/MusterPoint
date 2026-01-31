using MongoDB.Bson;
using MongoDB.Driver;
using MusterPoint.Models;

namespace MusterPoint.Services;

/// <summary>
/// Service to handle MongoDB schema migrations
/// </summary>
public class MigrationService
{
    private readonly IMongoClient _mongoClient;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(IMongoClient mongoClient, ILogger<MigrationService> logger)
    {
     _mongoClient = mongoClient;
        _logger = logger;
    }

 /// <summary>
    /// Migrate ListEntry documents from Count field to ModelCount field
    /// </summary>
    public async Task MigrateCountToModelCountAsync()
    {
  try
        {
            var database = _mongoClient.GetDatabase("musterpoint");
            var collection = database.GetCollection<BsonDocument>("lists");

         // Find all documents where entries have Count field
    var filter = Builders<BsonDocument>.Filter.Exists("Entries.Count");
   var documents = await collection.Find(filter).ToListAsync();

      _logger.LogInformation($"Found {documents.Count} documents to migrate");

  foreach (var doc in documents)
            {
var entries = doc["Entries"].AsBsonArray;
           bool modified = false;

           for (int i = 0; i < entries.Count; i++)
       {
   var entry = entries[i].AsBsonDocument;
     
            // If Count exists and ModelCount doesn't, migrate it
        if (entry.Contains("Count") && !entry.Contains("ModelCount"))
{
               var countValue = entry["Count"].AsInt32;
            entry["ModelCount"] = countValue;
     entry.Remove("Count");
        modified = true;
            _logger.LogInformation($"Migrated entry {i}: Count={countValue} -> ModelCount={countValue}");
  }
          }

           if (modified)
             {
            var updateFilter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
       await collection.ReplaceOneAsync(updateFilter, doc);
                    _logger.LogInformation($"Updated document {doc["_id"]}");
     }
      }

    _logger.LogInformation("Migration completed successfully");
        }
        catch (Exception ex)
      {
      _logger.LogError($"Error during migration: {ex.Message}");
            throw;
        }
    }
}
