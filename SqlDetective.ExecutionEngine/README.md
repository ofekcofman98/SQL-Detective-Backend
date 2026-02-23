# SQL Detective - Execution Engine Microservice

## Overview

The Execution Engine is an isolated microservice responsible for executing SQL queries against the PostgreSQL database. It supports both HTTP endpoints (for testing/debugging) and **RabbitMQ consumer** for production asynchronous processing.

## Architecture

```
SqlDetective.ExecutionEngine/
├── SqlDetective.ExecutionEngine.Api/          # Web API layer (port 5001)
│   ├── Controllers/
│   │   └── QueryExecutionController.cs        # HTTP endpoints (optional)
│   ├── Program.cs                             # Service configuration
│   └── appsettings.json                       # Configuration
├── SqlDetective.ExecutionEngine.Domain/       # Domain contracts
│   ├── Models/
│   │   ├── ExecuteQueryRequest.cs            # Request DTO
│   │   └── QueryExecutionResponse.cs         # Response DTO
│   └── Services/
│       └── IQueryExecutionService.cs          # Service interface
└── SqlDetective.ExecutionEngine.Data/         # Data access + Messaging
    ├── PostgresQueryExecutionService.cs       # PostgreSQL implementation
    ├── Consumers/
    │   └── ExecutionRequestConsumer.cs        # ✨ RabbitMQ Consumer (NEW)
    └── MessageQueue/
        ├── ExecutionRequest.cs                # ✨ Queue message models
        ├── ExecutionResult.cs
        └── RabbitMqConfiguration.cs
```

## Key Features

### ✨ RabbitMQ Consumer (Step 3 Complete!)
- **Asynchronous Processing**: Listens to `sql-execution-requests` queue
- **Auto-Execute**: Processes SQL queries automatically when received
- **Result Publishing**: Publishes results to `sql-execution-results` queue
- **Error Handling**: Nacks failed messages for retry or dead-letter queue
- **Graceful Shutdown**: Properly closes connections on service stop

### Security
- **Read-Only Transactions**: All queries run in `READ ONLY` transaction mode
- **Isolated Database User**: Should use a read-only PostgreSQL role
- **No Session Validation**: Session validation happens in the Relay Service (removes coupling)

## Message Flow

```
[Relay Service]
    ↓ Publishes ExecutionRequest
    ↓ { correlationId, sessionKey, sql, timestamp }
    
[RabbitMQ: sql-execution-requests]
    ↓
    
[Execution Engine Consumer] ← YOU ARE HERE (Step 3)
    ↓ 1. Receives message
    ↓ 2. Deserializes ExecutionRequest
    ↓ 3. Calls PostgresQueryExecutionService.ExecuteAsync()
    ↓ 4. Creates ExecutionResult
    ↓ 5. Publishes to sql-execution-results queue
    ↓ 6. Acknowledges message (ACK)
    
[RabbitMQ: sql-execution-results]
    ↓
    
[Relay Service Consumer] (Step 4 - Next)
    ↓ Routes result back to PC client
```

## API Endpoints (Optional - For Testing)

### Execute Query (HTTP - Optional)
**Endpoint**: `POST /api/execution/execute`
**Port**: 5001

**Request**:
```json
{
  "sessionKey": "abc123",
  "sql": "SELECT * FROM persons LIMIT 5"
}
```

**Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "firstName": "John",
      "lastName": "Doe"
    }
  ],
  "errorMessage": null
}
```

### Health Check
**Endpoint**: `GET /api/execution/health`

**Response**:
```json
{
  "status": "healthy",
  "service": "execution-engine"
}
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "SqlDetectiveDatabase": "Host=localhost;Port=5432;Database=sqldetective;Username=readonly_user;Password=your_password"
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
        "Url": "http://localhost:5001"
      }
    }
  }
}
```

## Running the Service

### Prerequisites

1. **RabbitMQ** running:
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

2. **PostgreSQL** with read-only user (see below)

### Start Service

```bash
cd SqlDetective.ExecutionEngine/SqlDetective.ExecutionEngine.Api
dotnet run
```

**Console Output**:
```
[ExecutionEngine] Starting RabbitMQ consumer...
[ExecutionEngine] RabbitMQ connection established to localhost:5672
[ExecutionEngine] Consumer is ready and listening to queue: sql-execution-requests
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5001
```

## Testing the Consumer

### Test Flow

1. **Start RabbitMQ**:
   ```bash
   docker start rabbitmq
   ```

2. **Start Relay Service** (to publish messages):
   ```bash
   cd SqlDetective.RelayService/SqlDetective.RelayService.Api
   dotnet run  # Port 5002
   ```

3. **Start Execution Engine** (this service with consumer):
   ```bash
   cd SqlDetective.ExecutionEngine/SqlDetective.ExecutionEngine.Api
   dotnet run  # Port 5001
   ```

4. **Submit Query via Relay Service**:
   ```bash
   curl -X POST "http://localhost:5002/api/relay/query?key=test-123" \
     -H "Content-Type: application/json" \
     -d '{"queryString": "SELECT * FROM persons LIMIT 3"}'
   ```

5. **Watch Execution Engine Console**:
   ```
   [ExecutionEngine] Processing execution request. CorrelationId: 550e8400-e29b-41d4-a716-446655440000, SessionKey: test-123
   [ExecutionEngine] [PostgresQueryExecutionService] Executing query for session: test-123
   [ExecutionEngine] Successfully processed request. CorrelationId: 550e8400-e29b-41d4-a716-446655440000, Success: True
   [ExecutionEngine] Published result to queue sql-execution-results. CorrelationId: 550e8400-e29b-41d4-a716-446655440000
   ```

6. **Verify Result in RabbitMQ**:
   - Open http://localhost:15672 (guest/guest)
   - Go to **Queues** → `sql-execution-results`
   - Click **Get messages** to see the result

## Message Formats

### ExecutionRequest (Incoming from Relay Service)
```json
{
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "sessionKey": "test-session-123",
  "sql": "SELECT * FROM persons WHERE age > 30",
  "timestamp": "2026-02-22T16:30:00Z"
}
```

### ExecutionResult (Outgoing to Relay Service)
```json
{
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "sessionKey": "test-session-123",
  "success": true,
  "data": [
    {
      "id": "uuid-1",
      "firstName": "John",
      "lastName": "Doe",
      "age": 35
    }
  ],
  "errorMessage": null,
  "timestamp": "2026-02-22T16:30:00.456Z"
}
```

## Consumer Behavior

### Message Acknowledgment
- **ACK (Acknowledge)**: Message processed successfully → removed from queue
- **NACK (Negative Acknowledge)**: 
  - `requeue: false` - JSON deserialization errors → send to dead-letter queue
  - `requeue: true` - Execution errors → retry message

### Prefetch Configuration
- **QoS Prefetch Count**: 1 (processes one message at a time)
- Ensures fair distribution across multiple instances

### Error Handling

**JSON Deserialization Error**:
```csharp
// Message: Invalid JSON → NACK (don't requeue)
await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
```

**SQL Execution Error**:
```csharp
// Message: SQL failed → NACK (requeue for retry)
await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
```

## Creating Read-Only Database User

```sql
-- Create read-only role
CREATE ROLE readonly_user WITH LOGIN PASSWORD 'secure_password';

-- Grant permissions
GRANT CONNECT ON DATABASE sqldetective TO readonly_user;
GRANT USAGE ON SCHEMA public TO readonly_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_user;

-- Ensure future tables are also readable
ALTER DEFAULT PRIVILEGES IN SCHEMA public 
GRANT SELECT ON TABLES TO readonly_user;
```

## Dependencies

- **.NET 10.0** (or .NET 8.0)
- **Npgsql 10.0.1** - PostgreSQL driver
- **RabbitMQ.Client 7.2.0** - ✨ Message queue client (NEW)
- **Microsoft.Extensions.Hosting.Abstractions 10.0.3** - ✨ Background services (NEW)
- **Microsoft.Extensions.Configuration.Abstractions 10.0.3**

## Troubleshooting

### Consumer Not Starting

**Error**: `Failed to initialize RabbitMQ connection`

**Solutions**:
1. Check RabbitMQ is running: `docker ps | grep rabbitmq`
2. Verify connection settings in `appsettings.json`
3. Test connection: `telnet localhost 5672`

### Messages Not Being Consumed

**Issue**: Messages sit in queue but aren't processed

**Solutions**:
1. Check consumer is running: Look for log `Consumer is ready and listening to queue`
2. Check queue name matches: `sql-execution-requests`
3. Verify no dead-letter messages: Check RabbitMQ Management UI

### SQL Execution Fails

**Error**: `[ExecutionEngine] Error processing message`

**Solutions**:
1. Check database connection string
2. Verify readonly_user has SELECT permissions
3. Check SQL syntax in the message
4. Review logs for specific Npgsql errors

### Port Already in Use

**Error**: `Failed to bind to address http://localhost:5001`

**Solutions**:
1. Kill existing process: `taskkill /F /IM SqlDetective.ExecutionEngine.Api.exe` (Windows)
2. Change port in `appsettings.json`

## Performance Tuning

### Scaling Horizontally

Run multiple instances to process messages in parallel:

```bash
# Terminal 1
dotnet run --urls http://localhost:5001

# Terminal 2
dotnet run --urls http://localhost:5011

# Terminal 3
dotnet run --urls http://localhost:5021
```

RabbitMQ will distribute messages across all consumers (round-robin).

### Increasing Throughput

Modify `BasicQosAsync` in `ExecutionRequestConsumer.cs`:

```csharp
// Process up to 10 messages concurrently per instance
await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false);
```

## Monitoring

### RabbitMQ Management UI

Access: http://localhost:15672 (guest/guest)

**Key Metrics**:
- **Queue Depth**: `sql-execution-requests` should be near 0 (processed quickly)
- **Message Rate**: Incoming vs. processing rate
- **Consumer Count**: Number of active consumers
- **Unacked Messages**: Should be low (messages being processed)

### Application Logs

```bash
# Watch logs in real-time
dotnet run | grep "\[ExecutionEngine\]"
```

**Key Log Messages**:
- `Consumer is ready and listening` - Consumer started successfully
- `Processing execution request` - Message received
- `Successfully processed request` - Query executed
- `Published result to queue` - Result sent back

## Security Considerations

1. **Read-Only Transactions**: `SET TRANSACTION READ ONLY` prevents writes
2. **Database Isolation**: Use separate read-only user with limited permissions
3. **Network Isolation**: Execution Engine should not be publicly accessible
4. **Message Validation**: Validate SQL before execution (future enhancement)
5. **Rate Limiting**: RabbitMQ QoS limits concurrent processing

## Next Steps

### Step 4: Add Result Consumer to Relay Service

The Relay Service needs to:
1. Listen to `sql-execution-results` queue
2. Route results back to PC clients via SignalR or polling
3. Handle result caching/expiration

### Future Enhancements

1. **SQL Validation**: Add SQL parser to reject dangerous queries
2. **Query Timeout**: Implement timeout for long-running queries
3. **Metrics**: Add Prometheus/Grafana metrics
4. **Dead-Letter Queue Handling**: Process failed messages
5. **Distributed Tracing**: Add correlation ID tracing across services

## License

Part of SQL Detective - Educational SQL game backend

---

**Status**: ✅ Step 3 Complete - RabbitMQ Consumer Integrated!
