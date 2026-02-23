namespace SqlDetective.ExecutionEngine.Data.MessageQueue;

public class RabbitMqConfiguration
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    
    public string ExecutionRequestQueue { get; set; } = "sql-execution-requests";
    public string ExecutionResultQueue { get; set; } = "sql-execution-results";
}
