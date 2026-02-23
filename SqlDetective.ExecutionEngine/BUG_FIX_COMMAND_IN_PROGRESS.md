# Execution Engine Bug Fix - Command Already in Progress

## Issue

When testing the Execution Engine with a simple query:
```json
{
  "sessionKey": "test-session",
  "sql": "SELECT 1 as is_working"
}
```

Error received:
```
A command is already in progress: SELECT 1 as is_working
```

## Root Cause

The issue was caused by improper disposal of `NpgsqlCommand` and `NpgsqlDataReader` objects. When using `await using` declarations at the statement level, the objects remain active until the end of the containing scope, which caused conflicts when:

1. The `SET TRANSACTION READ ONLY` command was executed but not disposed before the next command
2. The `DataReader` remained open when trying to commit the transaction

## Solution

Changed from statement-level `await using` to scoped `await using` blocks to ensure immediate disposal:

### Before (Broken):
```csharp
await using var setReadOnlyCmd = new NpgsqlCommand(
    "SET TRANSACTION READ ONLY", 
    conn, 
    transaction);
await setReadOnlyCmd.ExecuteNonQueryAsync(ct);

await using var cmd = new NpgsqlCommand(sql, conn, transaction);
await using var reader = await cmd.ExecuteReaderAsync(ct);

while (await reader.ReadAsync(ct))
{
    // Read rows...
}

await transaction.CommitAsync(ct); // ERROR: Reader still open!
```

### After (Fixed):
```csharp
// SET TRANSACTION READ ONLY with immediate disposal
await using (var setReadOnlyCmd = new NpgsqlCommand(
    "SET TRANSACTION READ ONLY", 
    conn, 
    transaction))
{
    await setReadOnlyCmd.ExecuteNonQueryAsync(ct);
} // Disposed here

await using var cmd = new NpgsqlCommand(sql, conn, transaction);

// Reader with scoped disposal
await using (var reader = await cmd.ExecuteReaderAsync(ct))
{
    while (await reader.ReadAsync(ct))
    {
        // Read rows...
    }
} // Reader disposed here

await transaction.CommitAsync(ct); // Works!
```

## Verification

### Test 1: Simple Query ✅
**Request:**
```json
{
  "sessionKey": "test-session",
  "sql": "SELECT 1 as is_working"
}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "is_working": 1
    }
  ],
  "errorMessage": null
}
```

### Test 2: Multi-Row Query ✅
**Request:**
```json
{
  "sessionKey": "test-session",
  "sql": "SELECT 'John' as first_name, 'Doe' as last_name, 42 as age UNION SELECT 'Jane', 'Smith', 35"
}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "first_name": "Jane",
      "last_name": "Smith",
      "age": 35
    },
    {
      "first_name": "John",
      "last_name": "Doe",
      "age": 42
    }
  ],
  "errorMessage": null
}
```

### Test 3: Read-Only Protection ✅
**Request:**
```json
{
  "sessionKey": "test-session",
  "sql": "CREATE TABLE hacker(id INT)"
}
```

**Response:**
```json
{
  "success": false,
  "data": null,
  "errorMessage": "25006: cannot execute CREATE TABLE in a read-only transaction"
}
```

## Key Takeaway

In Npgsql (and ADO.NET in general), only **one command can be active per connection at a time**. When using `await using` at the statement level:

```csharp
await using var cmd1 = ...;  // Active until end of scope
await using var cmd2 = ...;  // ERROR: cmd1 still active!
```

Use scoped blocks for immediate disposal:

```csharp
await using (var cmd1 = ...) 
{
    // Use cmd1
} // cmd1 disposed

await using (var cmd2 = ...)
{
    // Use cmd2
} // cmd2 disposed
```

## Status

✅ **Fixed** - Execution Engine now properly handles SQL execution with read-only transaction protection.

---

**Date:** February 22, 2026
**Fixed By:** Cursor AI Agent
