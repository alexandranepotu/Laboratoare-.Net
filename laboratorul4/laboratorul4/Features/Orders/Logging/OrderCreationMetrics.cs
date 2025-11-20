namespace laboratorul4.Features.Orders.Logging
{
    public class OrderCreationMetrics
    {
        public string OperationId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public TimeSpan ValidationDuration { get; set; }
        public TimeSpan DatabaseSaveDuration { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public bool Success { get; set; }
        public string? ErrorReason { get; set; }

        // Constructor pentru cazul de succes 
        public OrderCreationMetrics(
            string operationId,
            string title,
            string isbn,
            object category,
            TimeSpan validationDuration,
            TimeSpan databaseSaveDuration,
            TimeSpan totalDuration,
            bool success)
        {
            OperationId = operationId;
            Title = title ?? string.Empty;
            ISBN = isbn ?? string.Empty;
            Category = category?.ToString() ?? string.Empty;
            ValidationDuration = validationDuration;
            DatabaseSaveDuration = databaseSaveDuration;
            TotalDuration = totalDuration;
            Success = success;
        }

        // Constructor pentru cazul de eroare
        public OrderCreationMetrics(
            string operationId,
            string title,
            string isbn,
            object category,
            TimeSpan validationDuration,
            TimeSpan databaseSaveDuration,
            TimeSpan totalDuration,
            bool success,
            string errorReason)
            : this(operationId, title, isbn, category, validationDuration, databaseSaveDuration, totalDuration, success)
        {
            ErrorReason = errorReason;
        }

        // Constructor gol pentru serializare/deserializare
        public OrderCreationMetrics() { }
    }
}
