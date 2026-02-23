namespace SqlDetective.ExecutionEngine.Domain.Models
{
    public class QueryExecutionResponse
    {
        public bool Success { get; set; }
        public List<Dictionary<string, object>>? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
