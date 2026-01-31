# MongoDB Setup for MusterPoint

## Overview
The application now uses MongoDB for storing user lists instead of browser localStorage. This approach:
- ? Eliminates the need for `IJSRuntime` JavaScript interop
- ? Provides server-side persistence
- ? Allows users to access their lists from any device
- ? Supports user-specific list isolation via JWT authentication

## Architecture

### Server-Side
- **ListStorageService** (`MusterPoint/Services/ListStorageService.cs`)
  - Manages MongoDB CRUD operations
  - Filters lists by authenticated user ID
  - Handles database errors gracefully

- **ListsController** (`MusterPoint/Controllers/ListsController.cs`)
  - REST API endpoints: GET, POST, DELETE
  - Requires JWT authentication
  - Automatically associates lists with the authenticated user

### Client-Side
- **ListStorageService** (`MusterPoint.Client/Services/ListStorageService.cs`)
  - Makes HTTP requests to the `/api/lists` endpoints
  - No longer uses IJSRuntime
  - Works in both Server and WebAssembly render modes

### Data Models
- **Server Model** (`MusterPoint/Models/UserList.cs`)
  - Includes `UserId` field for ownership
  - MongoDB BSON attributes for serialization
  - Managed by MongoDB.Driver

- **Client Model** (`MusterPoint.Client/Models/UserList.cs`)
  - Shared data transfer object
  - Used for UI binding

## Installation & Setup

### 1. Install MongoDB

#### Windows
- Download from https://www.mongodb.com/try/download/community
- Run installer and follow prompts
- MongoDB runs as a Windows Service on `localhost:27017`

#### Mac
```bash
brew tap mongodb/brew
brew install mongodb-community
brew services start mongodb-community
```

#### Linux (Ubuntu)
```bash
sudo apt-get install -y mongodb
sudo systemctl start mongodb
```

#### Docker (Recommended for Development)
```bash
docker run -d -p 27017:27017 --name mongodb mongo:latest
```

### 2. Verify MongoDB is Running
```bash
# Test connection (you should see a successful connection message)
mongosh
> db.version()
```

### 3. Update Connection String (if needed)
Edit `MusterPoint/appsettings.json`:
```json
{
  "ConnectionStrings": {
 "MongoDB": "mongodb://localhost:27017"
  }
}
```

For MongoDB Atlas (cloud):
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority"
  }
}
```

### 4. Run the Application
```bash
dotnet run
```

## API Endpoints

All endpoints require JWT authentication (except `/api/auth/*`).

### Lists Management

**Get all lists for current user**
```
GET /api/lists
Authorization: Bearer <JWT_TOKEN>
Response: List<UserList>
```

**Get a specific list**
```
GET /api/lists/{listId}
Authorization: Bearer <JWT_TOKEN>
Response: UserList
```

**Create or update a list**
```
POST /api/lists
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "id": "...",
  "name": "My Army",
  "faction": "Space Marines",
  "entries": [...]
}

Response: UserList
```

**Delete a list**
```
DELETE /api/lists/{listId}
Authorization: Bearer <JWT_TOKEN>
Response: { message: "List deleted successfully" }
```

## Database Structure

### MongoDB Collections

**musterpoint.lists**
```json
{
  "_id": "ObjectId",
  "id": "string (UUID)",
  "userId": "string (from JWT claim)",
  "name": "string",
  "faction": "string",
  "entries": [
    {
      "id": "string",
      "unitId": "string",
      "unitName": "string",
   "count": "int",
  "selectedOptions": ["string"]
    }
  ],
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

## Security Notes

1. **User Isolation**: All list operations filter by `userId` from JWT claims
2. **Authentication Required**: All list endpoints require valid JWT token
3. **No Direct MongoDB Access**: Application uses official MongoDB driver with parameterized queries
4. **Connection String**: Keep MongoDB connection strings secure (use secrets manager in production)

## Troubleshooting

### "Unable to connect to MongoDB"
- Verify MongoDB service is running: `mongosh`
- Check connection string in `appsettings.json`
- For Docker: `docker ps` to verify container is running

### "Unauthorized" on list endpoints
- Ensure you're logged in first
- Check that JWT token is being sent in Authorization header
- Verify JWT token hasn't expired

### Lists not appearing
- Ensure you're authenticated with a user account
- Check browser console and server logs for errors
- Verify MongoDB has the `musterpoint` database with `lists` collection

## Next Steps

1. Implement user registration/login pages
2. Add list export/import functionality  
3. Create a MongoDB backup strategy
4. Set up MongoDB Atlas for production deployment
