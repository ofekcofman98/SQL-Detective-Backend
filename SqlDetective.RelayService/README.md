# SQL Detective - Relay Service Microservice

## Overview

The Relay Service is a microservice responsible for coordinating communication between mobile clients and the SQL Execution Engine using a message queue architecture. It replaces the original database-polling mechanism with a scalable, event-driven approach using RabbitMQ.

## Architecture

```
SqlDetective.RelayService/
├── SqlDetective.RelayService.Api/          # Web API layer (port 5002)
│   ├── Controllers/
│   │   └── RelayController.cs              # REST endpoints
│   ├── Program.cs                          # Service configuration
│   ├── appsettings.json                    # Configuration
│   └── appsettings.Development.json
├── SqlDetective.RelayService.Domain/       # Domain contracts
│   ├── Models/
│   │   ├── QueryRelayRequest.cs            # Mobile → Relay DTO
│   │   ├── ExecutionRequest.cs             # Relay → Queue message
│   │   └── ExecutionResult.cs              # Queue → Relay result
│   └── Services/
│       ├── IMessageQueueService.cs         # Queue abstraction
│       └── ISessionValidationService.cs    # Session validation
└── SqlDetective.RelayService.Infrastructure/ # Infrastructure layer
    ├── MessageQueue/
    │   ├── RabbitMqService.cs              # RabbitMQ implementation
    │   └── RabbitMqConfiguration.cs        # Config model
    └── Validation/
        └── SessionValidationService.cs     # Session validation via HTTP
```

## Message Flow

```
[Mobile App]
    ↓ POST /api/relay/query?key=abc123
    ↓ Body: { "queryString": "SELECT * FROM persons LIMIT 5" }

[Relay Service API]
    ↓ 1. Validate session key (calls monolith API)
    ↓ 2. Generate correlation ID
    ↓ 3. Publish to RabbitMQ: "sql-execution-requests"
    ↓    Message: {
    ↓      "correlationId": "uuid",
    ↓      "sessionKey": "abc123",
    ↓      "sql": "SELECT * FROM persons LIMIT 5",
    ↓      "timestamp": "2026-02-22T15:30:00Z"
    ↓    }
    ↓ 4. Return 202 Accepted

[Execution Engine Consumer] (Step 3 - Not Yet Implemented)
    ↓ Consumes from "sql-execution-requests" queue
    ↓ Executes SQL query
    ↓ Publishes result to "sql-execution-results" queue

[Relay Service Consumer] (Step 4 - Not Yet Implemented)
    ↓ Consumes from "sql-execution-results" queue
    ↓ Routes to PC client via SignalR or polling
```

## API Endpoints

### Submit Query for Execution

**Endpoint**: `POST /api/relay/query`

**Query Parameters**:
- `key` (required): Session key

**Request Body**:
```json
{
  "queryString": "SELECT * FROM persons WHERE age > 30"
}
```

**Response** (202 Accepted):
```json
{
  "message": "Query accepted for processing",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "sessionKey": "abc123"
}
```

**Error Responses**:
- `400 Bad Request`: Missing session key or query string
- `404 Not Found`: Session key not found or inactive

### Health Check

**Endpoint**: `GET /api/relay/health`

**Response**:
```json
{
  "service": "relay-service",
  "status": "healthy",
  "messageQueue": "connected",
  "timestamp": "2026-02-22T15:45:00Z"
}
```

## Configuration

### appsettings.json

```json
{
  "MonolithApi": {
    "BaseUrl": "http://localhost:5000"
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExecutionRequestQueue": "sql-execution-requests",
    "ExecutionResultQueue": "sql-execution-results"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5002"
      }
    }
  }
}
```

## Prerequisites

### RabbitMQ Setup

Install and run RabbitMQ using Docker:

```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

Access RabbitMQ Management UI:
- URL: http://localhost:15672
- Username: `guest`
- Password: `guest`

### Monolith API

The Relay Service requires the original monolith API to be running for session validation:

```bash
cd SqlDetective.Api
dotnet run
# Runs on http://localhost:5000
```

Ensure the monolith has a session endpoint:
- `GET /api/sessions/{key}` - Returns 200 if session exists, 404 otherwise

## Running the Service

### Development

```bash
cd SqlDetective.RelayService/SqlDetective.RelayService.Api
dotnet run
```

The service will start on `http://localhost:5002`

### Production

```bash
dotnet publish -c Release
cd bin/Release/net8.0/publish
dotnet SqlDetective.RelayService.Api.dll
```

## Testing

### Prerequisites
1. Start RabbitMQ: `docker start rabbitmq`
2. Start Monolith API: `cd SqlDetective.Api && dotnet run`
3. Start Relay Service: `cd SqlDetective.RelayService/SqlDetective.RelayService.Api && dotnet run`

### Test Health Endpoint

```bash
curl http://localhost:5002/api/relay/health
```

### Test Query Submission

```bash
curl -X POST "http://localhost:5002/api/relay/query?key=test-session-123" \
  -H "Content-Type: application/json" \
  -d '{
    "queryString": "SELECT * FROM persons LIMIT 3"
  }'
```

### Verify Message in RabbitMQ

1. Open http://localhost:15672
2. Go to **Queues** tab
3. Click on `sql-execution-requests` queue
4. Click **Get messages** to see the published message

## Key Differences from Original Implementation

| Aspect | Old (QueryRelayController) | New (Relay Service) |
|--------|----------------------------|---------------------|
| **Storage** | Saves queries to `RelayQuery` table in Postgres | Publishes to RabbitMQ queue |
| **Retrieval** | PC polls `GET /api/relay?key={key}` every 1-5s | Consumer listens to queue (push-based) |
| **Latency** | 1-5 seconds (polling interval) | <50ms (event-driven) |
| **Database Load** | High (constant SELECT queries) | Zero (no relay queries in DB) |
| **Scalability** | Poor (N clients = N polling queries/sec) | Excellent (queue distributes load) |
| **State Management** | `IsConsumed` flag in database | Message acknowledgment in queue |
| **Backpressure** | None (DB fills up) | Built-in (queue depth monitoring) |

## Dependencies

- **.NET 8.0**
- **RabbitMQ.Client 7.2.0** - Message queue client
- **Newtonsoft.Json 13.0.4** - JSON serialization
- **Microsoft.Extensions.Http 10.0.3** - HttpClient factory
- **Microsoft.Extensions.Hosting.Abstractions 10.0.3**
- **Microsoft.Extensions.Logging.Abstractions 10.0.3**
- **Microsoft.Extensions.Configuration.Abstractions 10.0.3**

## Next Steps

### Step 3: Add Queue Consumer to Execution Engine

Modify the Execution Engine to:
1. Listen to `sql-execution-requests` queue
2. Execute SQL queries
3. Publish results to `sql-execution-results` queue

### Step 4: Add Result Consumer to Relay Service

Add a background service to:
1. Listen to `sql-execution-results` queue
2. Route results back to PC clients via SignalR or polling

### Step 5: Implement SignalR Hub

Replace polling with real-time push notifications:
- PC connects to SignalR hub with session key
- Results are pushed instantly when available

## Troubleshooting

### RabbitMQ Connection Failed

**Error**: `Failed to initialize RabbitMQ connection`

**Solutions**:
1. Check RabbitMQ is running: `docker ps | grep rabbitmq`
2. Verify connection settings in `appsettings.json`
3. Check network access: `telnet localhost 5672`

### Session Validation Failed

**Error**: `Session 'xyz' not found or inactive`

**Solutions**:
1. Ensure monolith API is running on port 5000
2. Verify session exists: `curl http://localhost:5000/api/sessions/{key}`
3. Check `MonolithApi.BaseUrl` configuration

### Port Already in Use

**Error**: `Failed to bind to address http://0.0.0.0:5002`

**Solutions**:
1. Change port in `appsettings.json`: `"Url": "http://0.0.0.0:5003"`
2. Kill process using port: `netstat -ano | findstr :5002` (Windows) or `lsof -i :5002` (Linux/Mac)

## Security Considerations

1. **Session Validation**: Always validate session before publishing to queue
2. **SQL Injection**: Validation should happen at the Execution Engine (read-only mode)
3. **Rate Limiting**: Add rate limiting middleware to prevent queue flooding
4. **Authentication**: Add API keys or JWT for inter-service communication
5. **Network Isolation**: Relay Service should not be publicly accessible in production

## Environment Variables

For containerization:

```bash
MonolithApi__BaseUrl="http://monolith:5000"
RabbitMq__HostName="rabbitmq"
RabbitMq__UserName="relay_user"
RabbitMq__Password="secure_password"
Kestrel__Endpoints__Http__Url="http://0.0.0.0:5002"
```

## Logging

The service logs:
- Query submissions with session keys
- RabbitMQ connection status
- Session validation results
- Message publication events
- Health check requests

Configure logging level in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "SqlDetective.RelayService": "Debug"
    }
  }
}
```

## License

Part of SQL Detective - Educational SQL game backend
