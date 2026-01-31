using MongoDB.Driver;
using MusterPoint.Models.Authentication;

namespace MusterPoint.Services;

/// <summary>
/// Repository for managing user accounts in MongoDB
/// </summary>
public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int id);
    Task<bool> UserExistsAsync(string username);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task InitializeIndexesAsync();
}

/// <summary>
/// MongoDB implementation of user repository
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly ILogger<UserRepository> _logger;
    private bool _indexesInitialized = false;

    public UserRepository(IMongoClient mongoClient, ILogger<UserRepository> logger)
    {
        _logger = logger;
        var database = mongoClient.GetDatabase("musterpoint");
        _usersCollection = database.GetCollection<User>("users");
        _logger.LogInformation("UserRepository initialized for database: musterpoint, collection: users");
    }

    /// <summary>
 /// Initialize indexes asynchronously
    /// </summary>
    public async Task InitializeIndexesAsync()
    {
        if (_indexesInitialized)
            return;

     try
        {
       var indexModel = new CreateIndexModel<User>(
      Builders<User>.IndexKeys.Ascending(u => u.Username),
          new CreateIndexOptions { Unique = true }
            );
       await _usersCollection.Indexes.CreateOneAsync(indexModel);
            _indexesInitialized = true;
            _logger.LogInformation("Username index created successfully");
        }
        catch (Exception ex)
     {
_logger.LogWarning($"Could not create username index: {ex.Message}");
   }
    }

 /// <summary>
    /// Get user by username
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
  var filter = Builders<User>.Filter.Eq(u => u.Username, username);
         var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();
        if (user != null)
         _logger.LogInformation($"User '{username}' found in database");
            else
              _logger.LogInformation($"User '{username}' not found in database");
      return user;
        }
        catch (Exception ex)
        {
 _logger.LogError($"Error getting user by username '{username}': {ex.Message}");
return null;
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        try
   {
       var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            return await _usersCollection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
    _logger.LogError($"Error getting user by ID {id}: {ex.Message}");
   return null;
        }
    }

  /// <summary>
    /// Check if user exists by username
    /// </summary>
    public async Task<bool> UserExistsAsync(string username)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
   var count = await _usersCollection.CountDocumentsAsync(filter);
  var exists = count > 0;
            _logger.LogInformation($"User existence check for '{username}': {exists}");
            return exists;
    }
        catch (Exception ex)
  {
    _logger.LogError($"Error checking user existence for '{username}': {ex.Message}");
      return false;
        }
 }

    /// <summary>
    /// Create a new user
    /// </summary>
    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
        _logger.LogInformation($"Starting user creation for username: {user.Username}, email: {user.Email}");
   
     user.Id = (int)DateTime.UtcNow.Ticks % int.MaxValue;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
   user.IsActive = true;

            _logger.LogInformation($"Inserting user into MongoDB. UserId: {user.Id}, Username: {user.Username}");
       await _usersCollection.InsertOneAsync(user);
      _logger.LogInformation($"User '{user.Username}' created successfully with ID {user.Id}");
 return user;
   }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating user '{user.Username}': {ex.GetType().Name} - {ex.Message}");
      throw;
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    public async Task UpdateUserAsync(User user)
    {
      try
        {
 user.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
    var options = new ReplaceOptions { IsUpsert = false };

            var result = await _usersCollection.ReplaceOneAsync(filter, user, options);
            _logger.LogInformation($"User '{user.Username}' updated successfully. Matched: {result.MatchedCount}, Modified: {result.ModifiedCount}");
        }
        catch (Exception ex)
        {
     _logger.LogError($"Error updating user '{user.Username}': {ex.Message}");
     throw;
        }
    }
}
