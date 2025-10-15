using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;

namespace Common.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly string _connectionString;
        private readonly string _serviceName;
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseLoggerProvider(
            string connectionString,
            string serviceName,
            IHttpContextAccessor httpContextAccessor,
            Func<string, LogLevel, bool> filter)
        {
            _connectionString = connectionString;
            _serviceName = serviceName;
            _httpContextAccessor = httpContextAccessor;
            _filter = filter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(categoryName, _connectionString, _serviceName, _httpContextAccessor, _filter);
        }

        public void Dispose()
        {
        }
    }

    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _connectionString;
        private readonly string _serviceName;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Func<string, LogLevel, bool> _filter;

        public DatabaseLogger(
            string categoryName,
            string connectionString,
            string serviceName,
            IHttpContextAccessor httpContextAccessor,
            Func<string, LogLevel, bool> filter)
        {
            _categoryName = categoryName;
            _connectionString = connectionString;
            _serviceName = serviceName;
            _httpContextAccessor = httpContextAccessor;
            _filter = filter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_categoryName, logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var exceptionMessage = exception?.ToString();

            SaveToDatabase(logLevel, _categoryName, message, exceptionMessage);
        }

        private void SaveToDatabase(LogLevel logLevel, string category, string message, string exception)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                return;
            }

            try
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var machineName = Environment.MachineName;

                // Get Correlation ID from HttpContext
                string correlationId = null;
                try
                {
                    var httpContext = _httpContextAccessor?.HttpContext;
                    if (httpContext != null && httpContext.Items.ContainsKey("X-Correlation-ID"))
                    {
                        correlationId = httpContext.Items["X-Correlation-ID"]?.ToString();
                    }
                }
                catch
                {
                    // If we can't get the correlation ID, just continue without it
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO [Logging].[Logs]
                        (Timestamp, LogLevel, Category, Message, Exception, Environment, MachineName, ServiceName, CorrelationId)
                        VALUES
                        (@Timestamp, @LogLevel, @Category, @Message, @Exception, @Environment, @MachineName, @ServiceName, @CorrelationId)";

                    command.Parameters.AddWithValue("@Timestamp", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@LogLevel", logLevel.ToString());
                    command.Parameters.AddWithValue("@Category", category ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Message", message ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Exception", exception ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Environment", environment ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MachineName", machineName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ServiceName", _serviceName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CorrelationId", correlationId ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                // Silently fail to prevent logging errors from crashing the application
            }
        }
    }

    public static class DatabaseLoggerExtensions
    {
        public static ILoggerFactory AddDatabase(
            this ILoggerFactory factory,
            string connectionString,
            string serviceName,
            IHttpContextAccessor httpContextAccessor = null,
            Func<string, LogLevel, bool> filter = null)
        {
            factory.AddProvider(new DatabaseLoggerProvider(connectionString, serviceName, httpContextAccessor, filter));
            return factory;
        }
    }
}
