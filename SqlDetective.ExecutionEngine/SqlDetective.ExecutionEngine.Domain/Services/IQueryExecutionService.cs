using SqlDetective.ExecutionEngine.Domain.Models;

namespace SqlDetective.ExecutionEngine.Domain.Services
{
    public interface IQueryExecutionService
    {
        Task<QueryExecutionResponse> ExecuteAsync(
            string sessionKey, 
            string sql, 
            CancellationToken ct = default);
    }
}
