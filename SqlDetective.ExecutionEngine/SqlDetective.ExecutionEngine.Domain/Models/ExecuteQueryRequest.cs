namespace SqlDetective.ExecutionEngine.Domain.Models
{
    public class ExecuteQueryRequest
    {
        public string SessionKey { get; set; } = string.Empty;
        public string Sql { get; set; } = string.Empty;
    }
}
